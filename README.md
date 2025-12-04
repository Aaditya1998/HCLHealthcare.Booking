# HCLHealthcare.Booking

## Overview
HCLHealthcare.Booking is a comprehensive healthcare appointment booking system built with **.NET 8 (ASP.NET Core MVC)**. It facilitates seamless interaction between patients, doctors, and administrators, allowing for efficient appointment scheduling, doctor management, and center administration.

## Features

### 🏥 Patient Features
*   **Book Appointments**: Browse doctors by specialization and center, and book available time slots.
*   **Symptom Checker**: Basic symptom checker to help patients identify potential issues (powered by `SymptomCheckerController`).
*   **View Doctors**: Detailed profiles for doctors including experience, qualifications, and patient stories.

### 👨‍⚕️ Doctor Features
*   **Doctor Dashboard**: Dedicated dashboard for doctors to view their schedule.
*   **Slot Management**: Manage availability and time slots.
*   **View Appointments**: Track upcoming patient appointments.

### 🛡️ Admin Features
*   **Dashboard**: Overview of system statistics.
*   **Manage Entities**: CRUD operations for:
    *   Doctors
    *   Centers
    *   Specializations
    *   Users (Roles and Permissions)

## Technology Stack
*   **Framework**: .NET 8 (ASP.NET Core MVC)
*   **Database**: SQL Server (configured for LocalDB)
*   **ORM**: Entity Framework Core
*   **Authentication**: ASP.NET Core Identity
*   **Frontend**: Razor Views, Bootstrap, JavaScript

## Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   SQL Server (LocalDB) or compatible SQL Server instance.

## Getting Started

1.  **Clone the Repository**
    ```bash
    git clone <repository-url>
    cd HCLHealthcare.Booking
    ```

2.  **Configuration**
    The application uses `appsettings.json` for configuration. By default, it points to a LocalDB instance:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HCLHealthcareBooking;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```

3.  **Run the Application**
    The application includes a `DbInitializer` that automatically creates the database and seeds initial data if it doesn't exist.
    ```bash
    dotnet run
    ```

4.  **Access the Application**
    Open your browser and navigate to:
    *   `http://localhost:5285` (HTTP)
    *   `https://localhost:7072` (HTTPS)

## Default Credentials (Seeded Data)
The system comes pre-loaded with the following accounts for testing:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@hcl.com` | `Pass@123` |
| **Doctor** | `doctor@hcl.com` | `Pass@123` |
| **Doctor** | `doctor2@hcl.com` | `Pass@123` |

## Project Structure
*   **Controllers**: Handles incoming HTTP requests and application logic (e.g., `AppointmentsController`, `AdminController`).
*   **Models**: Represents the data structure (e.g., `Doctor`, `Appointment`, `TimeSlot`).
*   **Views**: Razor pages for the UI.
*   **Data**: Database context and seeding logic (`ApplicationDbContext`, `DbInitializer`).
*   **Services**: Business logic services (e.g., `SlotService`).

## Troubleshooting
*   **Database Errors**: If you encounter database errors, ensure you have LocalDB installed or update the connection string in `appsettings.json` to point to a valid SQL Server instance.
*   **Port Conflicts**: If ports 5285 or 7072 are in use, modify `Properties/launchSettings.json`.
