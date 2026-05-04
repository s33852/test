using MyApp.DTOs;        // ← replace MyApp
using MyApp.Exceptions;  // ← replace MyApp
using Microsoft.Data.SqlClient;

namespace MyApp.Repositories; // ← replace MyApp

public class SampleRepository : ISampleRepository // ← rename both, e.g. CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public SampleRepository(IConfiguration configuration) // ← rename SampleRepository
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing 'Default' connection string.");
    }

    public async Task<SampleResponse?> GetDetailsAsync(int id) // ← rename method + return type to match interface
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        string field1; // ← rename to match your parent columns, e.g. firstName
        string field2; // ← rename to match your parent columns, e.g. lastName

        await using (var cmd = new SqlCommand(
            "SELECT col1, col2 FROM ParentTable WHERE parent_id = @id;", // ← replace col1, col2, ParentTable, parent_id with your DB columns
            connection))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null; // ← leave this line as-is

            field1 = reader.GetString(0); // ← 0 = first column in SELECT above
            field2 = reader.GetString(1); // ← 1 = second column in SELECT above
        }

        var childrenById = new Dictionary<int, ChildResponse>(); // ← replace ChildResponse with your child DTO class name

        await using (var cmd = new SqlCommand(@"
            SELECT  c.child_id,       -- index 0
                    c.child_date,     -- index 1
                    c.nullable_date,  -- index 2  (remove this line if no nullable column)
                    s.status_name,    -- index 3
                    g.grandchild_name,-- index 4
                    g.amount          -- index 5
            FROM    ChildTable      c                                    -- ← replace ChildTable
            JOIN    StatusTable     s  ON s.status_id  = c.status_id    -- ← replace StatusTable + column names
            LEFT JOIN GrandTable    g  ON g.child_id   = c.child_id     -- ← replace GrandTable + column names
            WHERE   c.parent_id = @id                                    -- ← replace parent_id
            ORDER BY c.child_id;",                                       // ← replace child_id
            connection))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var childId = reader.GetInt32(0); // ← 0 = child_id index in SELECT above

                if (!childrenById.TryGetValue(childId, out var child))
                {
                    child = new ChildResponse // ← replace ChildResponse with your child DTO class name
                    {
                        Id          = childId,
                        ChildDate   = reader.GetDateTime(1),                                    // ← rename ChildDate, index must match SELECT
                        NullableCol = reader.IsDBNull(2) ? null : reader.GetDateTime(2),        // ← rename NullableCol, remove if no nullable column
                        Status      = reader.GetString(3),                                      // ← index must match SELECT
                        GrandItems  = new List<GrandItemResponse>()                             // ← replace GrandItemResponse with your grandchild DTO class name
                    };
                    childrenById.Add(childId, child);
                }

                if (!reader.IsDBNull(4)) // ← 4 = grandchild_name index in SELECT above
                {
                    child.GrandItems.Add(new GrandItemResponse // ← replace GrandItemResponse
                    {
                        Name   = reader.GetString(4),   // ← rename Name, index must match SELECT
                        Amount = reader.GetDecimal(5)   // ← rename Amount, index must match SELECT
                    });
                }
            }
        }

        return new SampleResponse // ← replace SampleResponse with your top-level response DTO class name
        {
            Field1   = field1,                              // ← rename Field1 to match your DTO property, e.g. FirstName = firstName
            Field2   = field2,                              // ← rename Field2 to match your DTO property, e.g. LastName = lastName
            Children = childrenById.Values.ToList()         // ← rename Children to match your DTO list property, e.g. Rentals
        };
    }

    public async Task CreateItemAsync(int id, CreateSampleRequest request) // ← rename method + request type to match interface
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            await using (var cmd = new SqlCommand(
                "SELECT 1 FROM ParentTable WHERE parent_id = @id;", // ← replace ParentTable + parent_id with your table/column
                connection, transaction))
            {
                cmd.Parameters.AddWithValue("@id", id);
                if (await cmd.ExecuteScalarAsync() is null)
                    throw new NotFoundException($"Parent with id {id} not found."); // ← replace "Parent" with your resource name
            }

            var resolvedIds = new List<int>();
            foreach (var item in request.Items) // ← replace Items with your request list property name
            {
                await using var cmd = new SqlCommand(
                    "SELECT related_id FROM RelatedTable WHERE name = @name;", // ← replace related_id, RelatedTable, name with your column/table names
                    connection, transaction);
                cmd.Parameters.AddWithValue("@name", item.Name); // ← replace Name with your item property, e.g. item.Title

                var relatedId = await cmd.ExecuteScalarAsync();
                if (relatedId is null)
                    throw new NotFoundException($"Related item '{item.Name}' not found."); // ← replace item.Name with your property

                resolvedIds.Add((int)relatedId);
            }

            await using (var cmd = new SqlCommand(@"
                INSERT INTO ChildTable (child_date, parent_id, status_id)
                VALUES (@date, @parentId, 1);", // ← replace ChildTable + columns with your table/column names
                connection, transaction))
            {
                cmd.Parameters.AddWithValue("@date",     request.Date);     // ← replace Date with your request property name
                cmd.Parameters.AddWithValue("@parentId", id);
                await cmd.ExecuteNonQueryAsync();
            }

            for (var i = 0; i < request.Items.Count; i++) // ← replace Items with your request list property name
            {
                await using var cmd = new SqlCommand(@"
                    INSERT INTO GrandTable (child_id, related_id, price)
                    VALUES (@childId, @relatedId, @price);", // ← replace GrandTable + columns with your table/column names
                    connection, transaction);
                cmd.Parameters.AddWithValue("@childId",   request.Id);            // ← replace Id with your request ID property
                cmd.Parameters.AddWithValue("@relatedId", resolvedIds[i]);
                cmd.Parameters.AddWithValue("@price",     request.Items[i].Price); // ← replace Items + Price with your property names
                await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();   // ← leave as-is
        }
        catch
        {
            await transaction.RollbackAsync(); // ← leave as-is
            throw;                             // ← leave as-is
        }
    }
}
