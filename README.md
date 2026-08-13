# Pharmacy Management System

## Stack

- C#
- ASP.NET Core MVC
- .NET 10
- ADO.NET
- SQL Server
- Microsoft.Data.SqlClient
- Bootstrap

.NET 10 is the current LTS release. The project uses Microsoft.Data.SqlClient 7.0.2.

## First module

This starter project currently implements:

- Dashboard
- Medicine CRUD
- Search
- Model validation
- Dependency Injection
- Service layer
- Repository layer
- ADO.NET
- SQL Server
- Parameterized SQL
- Async database access
- Soft delete

## 1. Create the SQL Server database

Open SQL Server Management Studio.

Run:

Database/01_CreateDatabase.sql

This creates:

PharmacyDB
  └── Medicines

## 2. Check the connection string

Open:

appsettings.json

Default:

Server=.\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;TrustServerCertificate=True;

If your SQL Server instance is different, change the Server value.

Examples:

Server=localhost;Database=PharmacyDB;Trusted_Connection=True;TrustServerCertificate=True;

Server=.\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;TrustServerCertificate=True;

## 3. Open the project

Visual Studio:
- Open PharmacyManagement.csproj
- Make sure the ASP.NET and web development workload is installed.

Or terminal:

dotnet restore
dotnet run

## 4. Open the site

Use the localhost URL shown by dotnet run.

Start with:

/Dashboard/Index

or:

/Medicine/Index

## Architecture

Browser
  ↓
Controller
  ↓
Service
  ↓
Repository
  ↓
ADO.NET
  ↓
SQL Server

## Next modules

1. Categories
2. Suppliers
3. Customers
4. Purchases
5. Sales
6. Stock transactions
7. Login/authentication
8. Roles/authorization
9. Dashboard statistics
10. Reports
11. Invoice printing
12. Audit logging
13. Deployment
