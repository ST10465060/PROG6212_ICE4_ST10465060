# Contract Claim Data Manager

A .NET 8 console application that stores lecturer claim records in a SQLite
database using Entity Framework Core.

**Student:** Eduan Pretorius (ST10465060)
**Module:** PROG6212 Programming 2B

## How to run
1. Open `ClaimDataManager.sln` in Visual Studio 2022.
2. Restore the NuGet packages.
3. Press F5. The database and the two sample records are created on the first run.

## Data-access concepts

### What is an ORM?
An ORM, or object relational mapper, is a library that sits between the
application and the database. It translates C# objects into database rows and
database rows back into C# objects, so the developer works with normal classes
instead of writing SQL by hand. Entity Framework Core is the ORM used here.

### Entity
An entity is a plain C# class that represents one table in the database, and
each object of that class represents one row. In this project `Claim` is the
entity and each `Claim` object is one lecturer claim.

### DbContext
The `DbContext` is the session between the application and the database. It
keeps track of the objects that have been loaded, records the changes made to
them and writes those changes out when `SaveChanges()` is called. It also holds
the connection configuration. `ClaimContext` is the DbContext in this project.

### DbSet
A `DbSet<T>` is a property on the DbContext that represents one table and lets
you query and change the rows in it. `DbSet<Claim> Claims` maps to the Claims
table.

### Provider
A provider is the package that teaches EF Core how to talk to one particular
database engine, because each engine has its own SQL dialect and connection
type. This project uses the `Microsoft.EntityFrameworkCore.Sqlite` provider.

### Code First vs Database First
With Code First the C# classes are written first and the database schema is
generated from them, which is the approach used here through
`Database.EnsureCreated()`. With Database First an existing database is the
starting point and the entity classes and DbContext are reverse engineered
from that schema instead.

## Features
- Add, view, update and delete claim records (EF Core)
- Validation: hours between 1 and 160, hourly rate above zero
- Export all claims to `Reports/claim_summary.txt` using `StreamWriter`
- Direct ADO.NET `SELECT COUNT(*)` using `SqliteConnection` and `SqliteCommand`
