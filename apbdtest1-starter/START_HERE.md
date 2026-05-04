# APBD Exam — Step by step, no guessing

---

## BEFORE YOU TOUCH ANY CODE

Read the PDF and write down on paper:
- [ ] The URL of the GET endpoint  (e.g. `api/customers/{id}/rentals`)
- [ ] The URL of the POST endpoint (e.g. `api/customers/{id}/rentals`)
- [ ] All table names from the DB diagram
- [ ] All column names from the DB diagram
- [ ] The example GET response JSON (you will paste parts of it into DTOs)
- [ ] The example POST request JSON (you will paste parts of it into DTOs)

---

## STEP 1 — Rename the project

In Rider: Right-click the solution → Find & Replace in Files
Search: `MyApp`
Replace: your project name, e.g. `APBD_TASK_8`
Scope: Whole project

Then rename the files themselves:
- `MyApp.csproj` → right-click → Rename → `APBD_TASK_8.csproj`
- `MyApp.http`   → right-click → Rename → `APBD_TASK_8.http`

---

## STEP 2 — Run the SQL script in SSMS

The exam gives you a `.sql` file. You do not put it in your C# project.

1. Open SSMS
2. Connect to your server — usually `(localdb)\mssqllocaldb`
3. File → Open → File → select the `.sql` file from the exam
4. Press **F5** to run it (creates the database and tables)
5. Look at the top of the SQL file for the database name, it will be in a line like:
   ```sql
   CREATE DATABASE YourDbName
   ```
   Write that name down — you need it in the next step.

---

## STEP 3 — Connection string

Open `appsettings.json`

Find this line:
```
"Default": "Server=(localdb)\\mssqllocaldb;Database=TODO_DB_NAME;...
```
Replace `TODO_DB_NAME` with the database name you found in the SQL file.

---

## STEP 4 — DTOs   →   open `DTOs/SampleDtos.cs`

This file needs two groups of changes.

---

### 3A — Response DTOs (what the GET returns)

Look at the example GET response JSON in the PDF. It looks like:
```json
{
  "firstName": "Alice",
  "lastName": "Johnson",
  "rentals": [
    {
      "id": 1001,
      "rentalDate": "2025-04-25T10:00:00",
      "returnDate": null,
      "status": "Returned",
      "movies": [
        {
          "title": "Inception",
          "priceAtRental": 3.99
        }
      ]
    }
  ]
}
```

The top-level object → becomes `SampleDetailsResponse` (rename the class)
Each key in it → becomes one property in that class

Rules for converting JSON keys to C# properties:
```
"firstName"    → public string FirstName { get; set; } = null!;
"lastName"     → public string LastName  { get; set; } = null!;
"id"           → public int Id { get; set; }
"rentalDate"   → public DateTime RentalDate { get; set; }
"returnDate": null  → public DateTime? ReturnDate { get; set; }   ← add ? because it can be null
"status"       → public string Status { get; set; } = null!;
"rentals": [   → public List<RentalResponse> Rentals { get; set; } = new();
"movies": [    → public List<MovieResponse> Movies { get; set; } = new();
3.99           → public decimal PriceAtRental { get; set; }
```

Each nested `[...]` array → becomes a new class below in the same file.

**Example — for the exam JSON above you would write:**
```csharp
public class CustomerRentalsResponse        // was: SampleDetailsResponse
{
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public List<RentalResponse> Rentals { get; set; } = new();
}

public class RentalResponse                 // was: ChildResponse
{
    public int Id { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = null!;
    public List<MovieResponse> Movies { get; set; } = new();
}

public class MovieResponse                  // was: GrandItemResponse
{
    public string Title { get; set; } = null!;
    public decimal PriceAtRental { get; set; }
}
```

---

### 3B — Request DTOs (what the POST body contains)

Look at the example POST request JSON in the PDF. It looks like:
```json
{
  "id": 1010,
  "rentalDate": "2025-05-05",
  "movies": [
    { "title": "Inception",    "rentalPrice": 2.2 },
    { "title": "Interstellar", "rentalPrice": 3.2 }
  ]
}
```

Same rules as above, but also add `[Required]` above every property,
and `[Range(0, double.MaxValue)]` above every decimal/price property.

**Example:**
```csharp
public class CreateRentalRequest            // was: CreateSampleRequest
{
    [Required] public int Id { get; set; }
    [Required] public DateTime RentalDate { get; set; }
    [Required][MinLength(1)] public List<CreateMovieRequest> Movies { get; set; } = new();
}

public class CreateMovieRequest             // was: CreateSampleItemRequest
{
    [Required] public string Title { get; set; } = null!;
    [Required][Range(0, double.MaxValue)] public decimal RentalPrice { get; set; }
}
```

---

## STEP 5 — Interfaces

You need to update two files. Both must have the exact same method signatures.

### 4A → open `Services/ISampleService.cs`

Replace the two method signatures with your actual ones.
Use the names of your real DTO classes (from Step 3).

```csharp
// GET endpoint → return type is nullable (?) so null means "not found"
Task<CustomerRentalsResponse?> GetCustomerRentalsAsync(int customerId);

// POST endpoint → just Task, no return value
Task CreateRentalAsync(int customerId, CreateRentalRequest request);
```

### 4B → open `Repositories/ISampleRepository.cs`

Copy the exact same two lines here. The file is identical to the service interface.

---

## STEP 6 — Repository   →   open `Repositories/SampleRepository.cs`

This is where all the SQL goes. The file has two methods. Fill them in.

---

### 5A — GetDetailsAsync (the GET method)

**Part 1: the first SQL block** — fetches the parent row.

Replace the SQL string and the column reads:
```csharp
await using (var cmd = new SqlCommand(
    "SELECT first_name, last_name FROM Customer WHERE customer_id = @id;",
//   ↑ copy exact column names from DB diagram                     ↑ match param name below
    connection))
{
    cmd.Parameters.AddWithValue("@id", id);
    await using var reader = await cmd.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return null;           // ← leave this line as-is, it causes the 404

    field1 = reader.GetString(0);   // 0 = first column in SELECT = first_name
    field2 = reader.GetString(1);   // 1 = second column          = last_name
}
```

**Part 2: the second SQL block** — fetches children + grandchildren.

Write the SELECT with all the JOINs you need.
Column order in SELECT = index you use in reader.GetXxx(index).

```csharp
// Example for the exam:
@"SELECT  r.rental_id,       -- index 0
          r.rental_date,     -- index 1
          r.return_date,     -- index 2  (nullable → use IsDBNull)
          s.name,            -- index 3
          m.title,           -- index 4
          ri.price_at_rental -- index 5
  FROM    Rental r
  JOIN    Status s      ON s.status_id  = r.status_id
  LEFT JOIN Rental_Item ri ON ri.rental_id = r.rental_id
  LEFT JOIN Movie m     ON m.movie_id   = ri.movie_id
  WHERE   r.customer_id = @id
  ORDER BY r.rental_id;"
```

Then fix the Dictionary and the objects you create inside the while loop.
Match property names to your DTO class names from Step 3.

For nullable columns use:
```csharp
ReturnDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
//                           ↑ same index as in SELECT
```

**Part 3: the return statement at the bottom** — use your real DTO class name:
```csharp
return new CustomerRentalsResponse
{
    FirstName = firstName,
    LastName  = lastName,
    Rentals   = rentalsById.Values.ToList()
};
```

---

### 5B — CreateItemAsync (the POST method)

The method has 4 blocks inside the `try`. Replace the SQL in each one.

**Block 1 — check the customer exists:**
```csharp
"SELECT 1 FROM Customer WHERE customer_id = @id;"
// if result is null → throws NotFoundException → controller returns 404
// leave the if/throw line as-is, just fix the SQL and table name
```

**Block 2 — foreach loop, look up FK ids:**
```csharp
"SELECT movie_id FROM Movie WHERE title = @name;"
// for each item in the request, find its ID in the DB
// if not found → throws NotFoundException
// leave the loop structure as-is, just fix the SQL
```

**Block 3 — insert the main row:**
```csharp
@"INSERT INTO Rental (rental_id, rental_date, return_date, customer_id, status_id)
  VALUES (@rentalId, @rentalDate, NULL, @customerId, @statusId);"
// fix column names to match your DB diagram
// fix parameter names to match AddWithValue calls below
```

**Block 4 — loop, insert child rows:**
```csharp
@"INSERT INTO Rental_Item (rental_id, movie_id, price_at_rental)
  VALUES (@rentalId, @movieId, @price);"
// fix column names to match your DB diagram
```

Do not touch the `try/catch`, `CommitAsync`, or `RollbackAsync` lines.

---

## STEP 7 — Service   →   open `Services/SampleService.cs`

Only two things to change:
1. Rename `Sample` → your resource name in the class name and constructor
2. If you added methods to the interface, add matching one-liners here:
```csharp
public Task<CustomerRentalsResponse?> GetCustomerRentalsAsync(int id)
    => _repository.GetCustomerRentalsAsync(id);
```

No SQL here. No logic. Just forwarding.

---

## STEP 8 — Controller   →   open `Controllers/SampleController.cs`

Three things to change:

**1. The `[Route]` attribute** — use the base path from the exam URL:
```
exam URL: api/customers/{id}/rentals
base path → [Route("api/customers")]
```

**2. The `[HttpGet]` and `[HttpPost]` route segments:**
```
exam URL: api/customers/{id}/rentals
segment  → [HttpGet("{id:int}/rentals")]
           [HttpPost("{id:int}/rentals")]
```

**3. The method calls** — use your real service method names:
```csharp
var result = await _sampleService.GetCustomerRentalsAsync(id);
await _sampleService.CreateRentalAsync(id, request);
```

Do not change the `if (result is null) return NotFound(...)` pattern.
Do not change the `CreatedAtAction` line, just make sure `nameof(...)` points to your GET method.

---

## STEP 9 — Program.cs

Find the TODO comment block. Add your two lines:
```csharp
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService,    CustomerService>();
```

Replace `Customer` with whatever you named your classes.
One pair of lines per resource. That's it for this file.

---

## STEP 10 — Run the project

First, check your port. Open `Properties/launchSettings.json` and find this line:
```
"applicationUrl": "http://localhost:5000"
```
That number (e.g. `5000`) is your port. Use it everywhere below and in the `.http` file.
The starter has `5000` but your real project will have a random number like `5041` — always use whatever is in that file.

**Option A — in Rider:**
- Click the green ▶ button at the top right
- A browser opens automatically at the Swagger page
- If it doesn't open, go to `http://localhost:YOUR_PORT/swagger` manually

**Option B — in terminal:**
```
dotnet run
```
Then open `http://localhost:YOUR_PORT/swagger` in your browser.

Swagger is just for manual clicking around. For the actual test cases use the `.http` file in the next step.

---

## STEP 11 — Test with the .http file

Open `MyApp.http`. At the top change the port to match `launchSettings.json`:
```
@MyApp_HostAddress = http://localhost:YOUR_PORT
```
Then update the 4 requests with your real URLs.

Then click the green ▶ play button next to each one and check:

| Request | Expected response |
|---|---|
| GET with a real id | 200 + full JSON matching the exam example |
| GET with id 9999 | 404 |
| POST with valid data | 201 |
| POST with a name that doesn't exist in DB | 404 |

If any of these is wrong, fix it before submitting.

---

## Final checklist before pushing to GitHub

- [ ] Code compiles with no errors (Build → Build Solution)
- [ ] All 4 test requests from Step 11 return the right status codes
- [ ] No SQL in Controller or Service files (only in Repository)
- [ ] `bin/` and `obj/` folders are NOT visible in GitHub (check the repo in browser)
- [ ] `.gitignore` file is present in the repo root
- [ ] The repo is public (or shared with the lecturer)

---

## Penalty table

| What went wrong | Points lost |
|---|---|
| SQL in Controller or Service | -15 |
| No AddScoped lines in Program.cs | -12 |
| Wrong HTTP status codes (e.g. 200 instead of 201/404) | -5 |
| Unclear or wrong variable names | -4 |
| bin/obj in repo OR missing .gitignore | -50% of total |
| Code doesn't compile | -100% (0 pts) |
| Not on GitHub | -100% (0 pts) |
