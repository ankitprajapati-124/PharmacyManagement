# Pharmacy Management System

A web-based Pharmacy Management System developed using **ASP.NET Core MVC, C#, SQL Server, and ADO.NET**.

The system is designed to manage pharmacy operations including medicines, categories, suppliers, purchases, sales, stock, users, reports, and audit logs.

The application supports local development as well as production deployment on **MonsterASP with Microsoft SQL Server**.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Architecture](#project-architecture)
- [Project Structure](#project-structure)
- [Modules](#modules)
- [Authentication and Authorization](#authentication-and-authorization)
- [Medicine Management](#medicine-management)
- [Category Management](#category-management)
- [Supplier Management](#supplier-management)
- [Purchase Management](#purchase-management)
- [Sales Management](#sales-management)
- [Automatic Invoice Generation](#automatic-invoice-generation)
- [Stock Management](#stock-management)
- [Reports](#reports)
- [Audit Logs](#audit-logs)
- [Database](#database)
- [Database Tables](#database-tables)
- [Database Relationships](#database-relationships)
- [Connection String](#connection-string)
- [Local Development Setup](#local-development-setup)
- [Running the Application](#running-the-application)
- [Accessing the Application from a Phone](#accessing-the-application-from-a-phone)
- [Database Backup](#database-backup)
- [Database Restore](#database-restore)
- [Production Deployment](#production-deployment)
- [MonsterASP Deployment](#monsterasp-deployment)
- [Production Workflow](#production-workflow)
- [Testing Checklist](#testing-checklist)
- [Security Notes](#security-notes)
- [Troubleshooting](#troubleshooting)
- [Future Improvements](#future-improvements)
- [License](#license)

---

# Project Overview

The Pharmacy Management System provides a centralized system for managing daily pharmacy operations.

The main workflow is:

```text
Login
  │
  ├── Dashboard
  │
  ├── Medicines
  │     ├── Add Medicine
  │     ├── Edit Medicine
  │     ├── Delete Medicine
  │     └── Stock Management
  │
  ├── Categories
  │
  ├── Suppliers
  │
  ├── Purchases
  │     ├── Create Purchase
  │     ├── Purchase History
  │     └── Purchase Details
  │
  ├── Sales
  │     ├── Create Sale
  │     ├── Sales History
  │     └── Sale Details
  │
  ├── Reports
  │
  ├── Users
  │
  └── Audit Logs
