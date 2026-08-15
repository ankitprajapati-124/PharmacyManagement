# 💊 Pharmacy Management System

A complete web-based **Pharmacy Management System** built with **ASP.NET Core MVC, C#, ADO.NET, and Microsoft SQL Server**.

The system manages medicines, categories, suppliers, purchases, sales, stock, users, reports, and audit logs through a responsive web interface.

---

## 📋 Table of Contents

- [Project Overview](#-project-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Modules](#-modules)
- [Authentication and User Management](#-authentication-and-user-management)
- [Medicine Management](#-medicine-management)
- [Category Management](#-category-management)
- [Supplier Management](#-supplier-management)
- [Purchase Management](#-purchase-management)
- [Sales Management](#-sales-management)
- [Automatic Invoice Generation](#-automatic-invoice-generation)
- [Stock Management](#-stock-management)
- [Reports](#-reports)
- [Audit Logs](#-audit-logs)
- [Database](#-database)
- [UI and Responsive Design](#-ui-and-responsive-design)
- [Local Development Setup](#-local-development-setup)
- [Database Backup and Restore](#-database-backup-and-restore)
- [Production Deployment](#-production-deployment)
- [Testing Checklist](#-testing-checklist)
- [Security](#-security)
- [Future Improvements](#-future-improvements)
- [Project Status](#-project-status)
- [Author](#-author)

---

## 📌 Project Overview

The Pharmacy Management System provides a centralized application for managing daily pharmacy operations.

### Main workflow

```text
Login
  │
  ├── Dashboard
  ├── Medicines
  ├── Categories
  ├── Suppliers
  ├── Purchases
  │     ├── Create Purchase
  │     ├── Purchase History
  │     └── Purchase Details
  ├── Sales
  │     ├── Create Sale
  │     ├── Sales History
  │     └── Sale Details
  ├── Reports
  ├── Users
  └── Audit Logs
```

---

# 🚀 Features

## 📊 Dashboard

- Total medicines
- Total stock
- Today's sales count and amount
- Total purchases and purchase amount
- Low-stock alerts
- Expiring-soon medicines
- Expired medicines
- Recent sales
- Recent purchases

## 💊 Medicine Management

- Add, edit, and delete/deactivate medicines
- Medicine name and manufacturer
- Batch number
- Expiry date
- Purchase and selling prices
- Stock quantity
- Category assignment
- Supplier assignment
- Active/inactive status

## 🗂️ Category Management

- Add categories
- Edit categories
- Delete categories
- Organize medicines by category

## 🏢 Supplier Management

- Add suppliers
- Edit suppliers
- Delete/deactivate suppliers
- Supplier phone, email, and address
- Supplier selection during purchases

## 📦 Purchase Management

- Create purchases
- Multiple medicines per purchase
- Automatic invoice generation
- Purchase total calculation
- Automatic stock increase
- Purchase history
- Purchase details
- Delete purchase with stock reversal

## 🛒 Sales Management

- Create sales
- Multiple medicines per sale
- Customer name and mobile
- Discount support
- Automatic invoice generation
- Sale total calculation
- Stock validation
- Automatic stock deduction
- Sales history
- Sale details
- Delete sale with stock reversal

## 👥 User Management

Supported roles:

```text
Admin
Pharmacist
Staff
```

Features:

- Create users
- Username validation
- Password hashing
- Role management
- Activate/deactivate users

## 📈 Reports

- Sales reports
- Purchase reports
- Stock reports
- Date filtering
- Transaction totals
- Discount totals
- Stock valuation
- Low-stock and out-of-stock counts
- Expiry reporting

## 📝 Audit Logs

Audit records can track important activities involving:

- Users
- Medicines
- Categories
- Suppliers
- Purchases
- Sales

---

# 🔢 Automatic Invoice Generation

Invoice numbers are generated automatically and are read-only in the create forms.

### Sales

```text
SAL-YYYYMMDD-001
SAL-YYYYMMDD-002
SAL-YYYYMMDD-003
```

Example:

```text
SAL-20260815-001
```

### Purchases

```text
PUR-YYYYMMDD-001
PUR-YYYYMMDD-002
PUR-YYYYMMDD-003
```

Example:

```text
PUR-20260815-001
```

---

# 📦 Stock Management

### Purchase

```text
New Stock = Existing Stock + Purchased Quantity
```

Example:

```text
Existing Stock = 50
Purchased      = 20
New Stock      = 70
```

### Sale

```text
New Stock = Existing Stock - Sold Quantity
```

Example:

```text
Existing Stock = 70
Sold           = 5
New Stock      = 65
```

The sale process validates available stock before completing the transaction.

### Stock Reversal

Deleting a sale restores the sold quantity.

Deleting a purchase reverses the purchased quantity.

This keeps inventory synchronized with transaction history.

---

# 🔐 Authentication and User Management

User records contain:

```text
UserId
Username
PasswordHash
FullName
Role
IsActive
CreatedAt
```

Passwords are hashed using:

```csharp
Microsoft.AspNetCore.Identity.PasswordHasher<User>
```

Plain-text passwords are not stored.

---

# 🏗️ Architecture

The project follows a layered architecture:

```text
┌────────────────────────────┐
│        Razor Views         │
│       HTML / CSS / JS      │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│        Controllers         │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│          Services          │
│       Business Logic       │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│        Repositories        │
│           ADO.NET          │
└──────────────┬─────────────┘
               │
               ▼
┌────────────────────────────┐
│        SQL Server          │
└────────────────────────────┘
```

### Controllers
Handle HTTP requests, routing, validation, and interaction with services.

### Services
Contain application and business logic.

### Repositories
Handle SQL queries, connections, transactions, and database mapping using ADO.NET.

### Models
Represent database entities and view models.

### Views
Razor `.cshtml` files provide the web interface.

---

# 📁 Project Structure

```text
PharmacyManagement/
│
├── Controllers/
├── Models/
├── Repositories/
├── Services/
├── Views/
│   ├── Account/
│   ├── Dashboard/
│   ├── Medicine/
│   ├── Category/
│   ├── Supplier/
│   ├── Purchase/
│   ├── Sale/
│   ├── Report/
│   ├── User/
│   ├── AuditLog/
│   └── Shared/
│
├── Database/
│   └── 01_CreateDatabase.sql
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── Properties/
├── Program.cs
├── appsettings.json
├── README.md
└── .gitignore
```

---

# 🛠️ Technology Stack

| Technology | Purpose |
|---|---|
| C# | Backend programming |
| ASP.NET Core MVC | Web framework |
| .NET 10 | Runtime |
| Razor | Server-side UI |
| ADO.NET | Database access |
| Microsoft.Data.SqlClient | SQL Server connectivity |
| Microsoft SQL Server | Database |
| HTML5 | UI structure |
| CSS3 | Styling |
| JavaScript | Client-side functionality |
| Bootstrap | Responsive components |
| Visual Studio | Development |

---

# 🗄️ Database

The application uses Microsoft SQL Server.

### Main tables

```text
Users
Categories
Medicines
Suppliers
Purchases
PurchaseItems
Sales
SaleItems
AuditLogs
```

### Main relationships

```text
Categories
    │
    └── Medicines
          │
          ├── PurchaseItems ── Purchases
          │
          └── SaleItems ───── Sales
```

Users are associated with purchases, sales, and audit logs.

---

# 🎨 UI and Responsive Design

The application includes:

- Responsive sidebar
- Pharmacy branding/logo
- Orange active navigation
- Lighter orange hover state
- Dashboard cards
- Responsive forms
- Responsive tables
- Mobile-friendly layout
- Horizontal scrolling for wide tables where required
- Responsive login page
- Consistent edit/delete page styling

Active sidebar example:

```css
.pm-sidebar-link.pm-active {
    background: linear-gradient(135deg, #f58220, #e86f0c);
    color: #fff;
}
```

Hover example:

```css
.pm-sidebar-link:hover {
    background: #ff9a45;
    color: #fff;
}
```

---

# 💻 Local Development Setup

## Requirements

Install:

- Visual Studio
- .NET 10 SDK
- SQL Server / SQL Server Express
- SQL Server Management Studio

## Clone the repository

```bash
git clone <YOUR-GITHUB-REPOSITORY-URL>
cd PharmacyManagement
```

Restore dependencies:

```bash
dotnet restore
```

---

# 🗃️ Database Setup

The repository contains:

```text
Database/01_CreateDatabase.sql
```

Run the script in SQL Server Management Studio.

Example local connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\SQLEXPRESS;Database=PharmacyManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> Use your own local configuration. Never commit production credentials to GitHub.

---

# ▶️ Run the Application

Clean:

```bash
dotnet clean
```

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run
```

---

# 📱 Access from a Phone

The application can be tested from a phone on the same Wi-Fi network.

Start the application:

```powershell
dotnet run --urls "http://0.0.0.0:5000"
```

Find the PC IP:

```powershell
ipconfig
```

Example:

```text
IPv4 Address: 192.168.1.4
```

Open on the phone:

```text
http://192.168.1.4:5000
```

Replace the IP with the actual PC IP.

If Windows Firewall blocks the connection, run PowerShell as Administrator:

```powershell
New-NetFirewallRule `
    -DisplayName "Pharmacy Management 5000" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 5000 `
    -Action Allow
```

---

# 💾 Database Backup and Restore

## Backup

Example SQL Server backup:

```sql
BACKUP DATABASE [PharmacyManagementDB]
TO DISK = 'E:\PharmacyManagement\PharmacyManagement.bak'
WITH INIT,
     FORMAT,
     NAME = 'PharmacyManagement Full Backup';
```

Recommended filename:

```text
PharmacyManagementDB_YYYY-MM-DD.bak
```

Keep backups outside the Git repository.

## Restore

Using SQL Server Management Studio:

```text
Databases
  ↓
Restore Database
  ↓
Device
  ↓
Select .bak
  ↓
Restore
```

Verify tables:

```sql
SELECT name
FROM sys.tables
ORDER BY name;
```

Expected tables:

```text
AuditLogs
Categories
Medicines
PurchaseItems
Purchases
SaleItems
Sales
Suppliers
Users
```

---

# 🌐 Production Deployment

The application has been deployed using:

```text
ASP.NET Core MVC
        │
        ▼
    MonsterASP
        │
        ▼
 Microsoft SQL Server
```

Deployment is performed using **Web Deploy**.

## MonsterASP deployment flow

```text
Create Website
      ↓
Create MSSQL Database
      ↓
Create local .bak
      ↓
Upload and restore database
      ↓
Get production connection details
      ↓
Download .publishsettings
      ↓
Import profile into Visual Studio
      ↓
Configure production database
      ↓
Publish
      ↓
Production testing
```

The hosted application must use the production SQL Server connection instead of:

```text
.\SQLEXPRESS
```

Never commit production credentials or `.publishsettings` files.

---

# 🔄 Recommended Development Workflow

```text
Local Development
       ↓
Make Changes
       ↓
dotnet build
       ↓
Local Testing
       ↓
Database Backup
       ↓
Publish
       ↓
Production Testing
```

Before deployment:

```bash
dotnet clean
dotnet build
```

---

# ✅ Testing Checklist

### Authentication

- [x] Login
- [x] Invalid login handling
- [x] Logout
- [x] User activation/deactivation

### Users

- [x] Create user
- [x] Username validation
- [x] Role selection
- [x] Activate/deactivate user

### Medicines

- [x] Add medicine
- [x] Edit medicine
- [x] Delete/deactivate medicine
- [x] Category assignment
- [x] Supplier assignment
- [x] Stock
- [x] Expiry information

### Categories

- [x] Add category
- [x] Edit category
- [x] Delete category

### Suppliers

- [x] Add supplier
- [x] Edit supplier
- [x] Delete supplier

### Purchases

- [x] Create purchase
- [x] Multiple items
- [x] Automatic invoice generation
- [x] Total calculation
- [x] Stock increase
- [x] Purchase history
- [x] Purchase details
- [x] Purchase deletion
- [x] Stock reversal

### Sales

- [x] Create sale
- [x] Multiple items
- [x] Automatic invoice generation
- [x] Discount
- [x] Total calculation
- [x] Stock validation
- [x] Stock decrease
- [x] Sales history
- [x] Sale details
- [x] Sale deletion
- [x] Stock reversal

### Reports

- [x] Sales report
- [x] Purchase report
- [x] Stock report
- [x] Date filters
- [x] Totals

### UI

- [x] Desktop UI
- [x] Mobile UI
- [x] Responsive sidebar
- [x] Responsive tables
- [x] Login UI
- [x] Category UI
- [x] Supplier UI
- [x] Sale UI
- [x] Purchase UI
- [x] Edit pages
- [x] Delete pages
- [x] Pharmacy logo
- [x] Orange sidebar theme

### Production

- [x] Website
- [x] Database
- [x] Database connection
- [x] Login
- [x] Sales
- [x] Purchases
- [x] Stock
- [x] Reports
- [x] Audit logs

---

# 🔐 Security

Never commit the following to GitHub:

```text
*.publishsettings
*.bak
Production passwords
Database passwords
API keys
Private keys
Secret connection strings
```

Recommended `.gitignore` entries:

```gitignore
.vs/
*.user
*.suo
bin/
obj/
publish/
*.publishsettings
*.bak
.env
```

Use secure environment variables or hosting configuration for production secrets.

---

# 🐛 Troubleshooting

## Build errors

```bash
dotnet clean
dotnet restore
dotnet build
```

## Local database errors

Check:

- SQL Server is running
- Server name is correct
- Database exists
- Connection string is correct

Typical local server:

```text
.\SQLEXPRESS
```

## Phone cannot connect

Check:

- PC and phone are on the same Wi-Fi
- Application uses `0.0.0.0`
- Correct PC IPv4 address is used
- Windows Firewall allows port 5000

## Production database errors

Check:

- Database was restored
- Required tables exist
- Production connection string is correct
- Database credentials are correct
- Production application is not using `.\SQLEXPRESS`

---

# 🔮 Future Improvements

Possible future features:

- Barcode scanning
- Barcode generation
- Prescription management
- Customer management
- Purchase returns
- Sales returns
- GST/tax management
- Profit and loss reports
- PDF invoice generation
- Thermal printer support
- Excel/PDF report export
- Expiry notifications
- Low-stock notifications
- Supplier payment tracking
- Customer credit management
- Multi-branch support
- Advanced permissions
- REST API
- Mobile application
- Automated cloud backups

---

# 📊 Project Status

## Production Ready

The current version has been:

- Developed
- Built successfully
- Tested locally
- Tested on mobile
- Database tested
- Database backed up
- Database restored on production
- Published to MonsterASP
- Production tested

---

# 👨‍💻 Author

**Ankit Prajapati**

### Built With

```text
C#
ASP.NET Core MVC
.NET 10
ADO.NET
Microsoft SQL Server
Razor
HTML5
CSS3
JavaScript
Bootstrap
```

---

# ⭐ Contributing

Contributions and suggestions are welcome.

For major changes:

1. Create a branch.
2. Make your changes.
3. Test the application.
4. Test database operations.
5. Create a database backup when required.
6. Submit a pull request.

---

# 📄 License

This project is currently intended for educational, development, and pharmacy-management purposes.

Add an appropriate open-source license if the repository will be publicly distributed.
