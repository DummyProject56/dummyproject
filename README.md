# dummyproject
# Employee Management System

A full-stack Employee Management System built with **Angular 16** and **ASP.NET Core Web API**, using **SQL Server** for database management.

The application provides employee management, searching, filtering, pagination, and Excel/CSV import functionality through a responsive web interface.

## Tech Stack

### Frontend
- Angular 16
- TypeScript
- HTML5
- SCSS
- Angular Reactive Forms
- Angular Router
- RxJS
- HttpClient

### Backend
- ASP.NET Core Web API
- C#
- Entity Framework Core
- RESTful APIs
- Swagger / OpenAPI

### Database
- Microsoft SQL Server
- Entity Framework Core Migrations

## Project Structure

The project is organized into separate frontend and backend applications.

```text
Employee-Management-System/
│
├── Web/                     # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   └── employees/
│   │   │   ├── services/
│   │   │   └── models/
│   │   ├── assets/
│   │   └── environments/
│   ├── angular.json
│   └── package.json
│
├── Backend/                 # ASP.NET Core solution
│   └── ...
│
└── README.md
```

*Folder names may vary depending on your local project structure.*

## Application Features

### Employee Management
- Create employee records
- View employee information
- Update employee details
- Delete employee records

### Search and Filtering
- Search employees by name or email
- Filter employees by department
- Filter by active or inactive status
- Filter using date ranges

### Pagination
- Server-side or client-side pagination based on API implementation
- Navigate through employee records

### Excel and CSV Import
- Upload `.xlsx` and `.csv` files
- Validate required employee fields
- Check duplicate email addresses
- Import valid employee records into SQL Server

### Employee Data Fields

| Field | Description |
|---|---|
| Name | Employee full name |
| Email | Unique email address |
| Phone | Contact number |
| Department | Employee department |
| Designation | Job title |
| JoiningDate | Employee joining date |
| Status | Active / Inactive |

## Getting Started

### Prerequisites

Install the following software:

- Node.js and npm
- Angular CLI compatible with Angular 16
- .NET SDK compatible with the backend project
- Microsoft SQL Server
- Visual Studio or VS Code
- Git

## Frontend Setup — Angular

**1. Navigate to the Angular folder**

```bash
cd Web
```

**2. Install dependencies**

```bash
npm install
```

**3. Start the Angular application**

```bash
ng serve
```

**4. Open the application**

```text
http://localhost:4200
```

## Backend Setup — ASP.NET Core

**1. Open the backend solution**

Open the `.sln` file in Visual Studio, or navigate to the backend project directory using the terminal.

**2. Restore dependencies**

```bash
dotnet restore
```

**3. Configure the database**

Update the connection string in `appsettings.json` according to your SQL Server instance.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EmployeeManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**4. Apply database migrations**

If your project uses EF Core migrations:

```bash
dotnet ef database update
```

**5. Run the backend**

```bash
dotnet run
```

**6. Open Swagger**

Use the HTTPS URL shown in your terminal and append `/swagger`.

Example:

```text
https://localhost:7186/swagger
```

## Angular API Configuration

Configure your Angular application to communicate with the ASP.NET Core backend.

Example `environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7186/api'
};
```

Use the configured API URL in Angular services.

Example:

```typescript
getEmployees() {
  return this.http.get(
    `${environment.apiUrl}/employees`
  );
}
```

Ensure the backend allows requests from the Angular development origin through its CORS configuration.

## Employee Import API

Example endpoint:

```http
POST /api/employees/import
Content-Type: multipart/form-data
```

The request must include a file field named `file`.

Supported file formats:

- `.xlsx`
- `.csv`

### Example Excel Format

| Name | Email | Phone | Department | Designation | JoiningDate | Status |
|---|---|---|---|---|---|---|
| Rahul Sharma | rahul@example.com | 9876543210 | IT | Developer | 2025-01-15 | Active |
| Priya Patel | priya@example.com | 9876543211 | HR | HR Executive | 2025-02-10 | Active |

Use the `yyyy-MM-dd` date format for importing employee records.

## GitHub Repository Usage

### Download Project

1. Open the GitHub repository.
2. Click **Code**.
3. Select **Download ZIP**.
4. Extract the project.
5. Follow the frontend and backend setup instructions.

### Clone Repository

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPOSITORY.git
```

### Repository Permissions

Users can view, download, and clone a public repository.

Only users with appropriate GitHub permissions can push changes directly to the original repository.

Downloading or cloning a project does not grant Write access.

## Security Guidelines

- Do not commit database passwords.
- Do not expose API keys or JWT secrets.
- Use environment-specific configuration.
- Validate incoming API requests.
- Use appropriate CORS policies.
- Store sensitive configuration using environment variables or a secrets manager.

## Future Enhancements

- Authentication and authorization
- Role-based access control
- Employee document management
- Employee attendance management
- Dashboard and analytics
- Excel export
- Advanced reporting

## License

No license is currently specified. All rights reserved unless the repository owner grants permission.

---

**Built with Angular, ASP.NET Core, and SQL Server.**
