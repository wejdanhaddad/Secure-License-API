# Secure-License-API
A .NET Blazor Server application for managing software licenses. Supports license generation, activation, and status checks via a JWT-secured Web API. Features role-based access control, multi-tenancy, and XAF/XPO persistence for enterprise-grade security and scalability.
# Secure License Management API

## Overview

This project implements a **secure ASP.NET Core Blazor Server application** for **software license management**, built using **DevExpress eXpressApp Framework (XAF)**, **XPO**, and **Blazor Server**.

The system allows clients to request licenses for software products, while administrators can review, approve, activate, and deactivate licenses. All operations are secured using **JWT authentication** and **role-based authorization**.

The repository includes both the **Blazor Server UI** for management and the **Web API endpoints** for programmatic license operations.

---

## Technologies Used

- **C# / .NET 7+**
- **Blazor Server**
- **DevExpress XAF**
- **XPO (eXpress Persistent Objects)**
- **JWT (JSON Web Tokens)**
- **Role-Based Authorization**
- **Multi-Tenancy**
- **Visual Studio**
- **Postman** for API testing

---

## Architecture

The application follows a layered architecture:

- **Presentation Layer**: Blazor Server UI for clients and administrators
- **Business Logic Layer**: Handles license validation, generation, activation, and status checks
- **Data Access Layer**: XPO ORM for secure, transactional data persistence
- **Security Layer**: JWT authentication and role-based authorization with XAF
- **Multi-Tenancy**: Tenant-isolated data access

---

## Features

- **Software License Management**
  - Generate licenses programmatically
  - Check license status (active, inactive, expired)
  - Activate or deactivate licenses
- **Admin Dashboard**
  - View and manage license requests
  - Role-based access control for administrative operations
- **Security**
  - JWT authentication for API endpoints
  - Role-based authorization (Admin / Client)
  - XAF-permission enforcement at object and property level
- **Multi-Tenancy**
  - Tenant isolation using `ITenantProvider`
- **API Testing**
  - Easily test endpoints with **Postman**

---

## Project Structure

APIlicense.Blazor.Server/
│
├── Models/ # XPO persistent objects (xaf_license)
├── Services/ # Business logic & JWT provider
├── Security/ # Authentication & authorization setup
├── Controllers/ # Web API endpoints (LicenseController)
├── UI/ # Blazor Server components
├── Data/ # Database configuration
├── BlazorApplication.cs # Main application class
├── appsettings.json # Configuration (database, logging, JWT)
└── Startup.cs # Service & middleware configuration

---

## License Entity (`xaf_license`)

Represents a software license:

| Property         | Type       | Description                                |
|-----------------|------------|--------------------------------------------|
| LicenseKey       | string     | Unique license key                          |
| Type             | string     | License type (Trial, Standard, Premium)    |
| Client           | string     | Client or organization                      |
| Product          | string     | Product associated with the license        |
| Category         | string     | Logical category of license                 |
| ExpirationDate   | DateTime   | License expiration date                     |
| IsActive         | bool       | Indicates if the license is active         |

All operations respect **XAF security rules** and **tenant isolation**.

---

## Web API Endpoints

| Method | Route                          | Description |
|--------|--------------------------------|------------|
| POST   | `/api/licenses/generate`        | Generate a new license |
| GET    | `/api/licenses/check/{key}`     | Check the status of a license |
| POST   | `/api/licenses/activate/{key}`  | Activate a license |
| POST   | `/api/licenses/deactivate/{key}`| Deactivate a license |

### License Generation Logic

- Combines `Client`, `Product`, `Type`, and timestamp to create a **unique 20-character key**
- Base64-encoded and sanitized for safe usage

---

## API Testing (Postman)
![postman test](https://github.com/user-attachments/assets/2ea2f7cf-ad3a-4336-9dba-143920b31c30)

All API endpoints can be tested using **Postman**:

- **Generate License:** POST `/api/licenses/generate`  
  Body (JSON):
  ```json
  {
      "Client": "CompanyABC",
      "Product": "ProductX",
      "Type": "Trial",
      "Category": "Software",
      "ExpirationDate": "2026-12-31T23:59:59Z"
  }
Check License: GET /api/licenses/check/{key}

Activate License: POST /api/licenses/activate/{key}

Deactivate License: POST /api/licenses/deactivate/{key}

Authentication Header: 
Authorization: Bearer <JWT_TOKEN>
Security :
Authentication :

JWT authentication for API endpoints

Cookie authentication for Blazor Server UI

Authorization :

Role-based authorization using XAF roles (Admin, Client)

Object-level permission enforcement with XPO

Endpoints protected via [Authorize] attributes

Multi-Tenancy :

Tenant-specific data access using ITenantProvider

Each tenant sees only their licenses

Setup and Run:
Prerequisites:

Visual Studio 2022+

.NET 7+ SDK

SQL Server or compatible database

DevExpress components installed

Steps:

Clone the repository

Open the solution in Visual Studio

Restore NuGet packages

Configure appsettings.json (Connection string & JWT key)

Run the application

Blazor Server UI for management

Web API for license endpoints

References:

XAF Documentation: https://docs.devexpress.com/eXpressAppFramework/112569

Blazor Server UI: https://docs.devexpress.com/eXpressAppFramework/401675/overview/supported-ui-platforms#aspnet-core-blazor-ui

XAF Web API: https://docs.devexpress.com/eXpressAppFramework/403394/backend-web-api-service

Model Editor & XAFML: https://docs.devexpress.com/eXpressAppFramework/112582

Community Extensions: https://www.devexpress.com/products/net/application_framework/#extensions

License: 

This project is licensed under the MIT License.
