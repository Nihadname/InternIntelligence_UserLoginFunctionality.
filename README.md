# Authentication Service Documentation

## Overview
This project is an authentication service built with **ASP.NET Core 9**, using **CQRS, Mediator, Identity, JWT**, and **Entity Framework Core**. The service provides functionality for user authentication, token validation, email verification, and user profile management.

## Technologies Used
- **ASP.NET Core 9**
- **Entity Framework Core** (SQL-based database)
- **CQRS (Command Query Responsibility Segregation)**
- **MediatR** (for implementing CQRS pattern)
- **Identity Framework** (User management)
- **JWT Authentication** (Token-based authentication)
- **AutoMapper** (Object-to-object mapping)
- **Dependency Injection**
- **ASP.NET Core Middleware**

## Features
- User Registration with validation
- User Login with JWT token generation
- Token validation
- Refresh Token mechanism
- Email verification system
- Profile Image update

## Installation

### Prerequisites
- .NET SDK 9
- SQL Server
- Visual Studio / VS Code
- Postman (for testing APIs)

### Setup
1. Clone the repository:
   ```sh
   git clone https://github.com/your-repository.git
   ```
2. Navigate to the project directory:
   ```sh
   cd UserAuthFunctionality
   ```
3. Configure **appsettings.json**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;"
     },
     "JwtSettings": {
       "SecretKey": "your_secret_key",
       "Issuer": "your_issuer",
       "Audience": "your_audience"
     }
   }
   ```
4. Apply migrations:
   ```sh
   dotnet ef database update
   ```
5. Run the project:
   ```sh
   dotnet run
   ```

## API Endpoints

### 1. **Register User**
   **POST** `/api/auth/register`
   ```json
   {
     "userName": "testUser",
     "email": "test@example.com",
     "fullName": "Test User",
     "phoneNumber": "1234567890",
     "birthDate": "2000-01-01",
     "password": "Test@123"
   }
   ```

### 2. **Login**
   **POST** `/api/auth/login`
   ```json
   {
     "userNameOrGmail": "test@example.com",
     "password": "Test@123"
   }
   ```

### 3. **Validate Token**
   **GET** `/api/auth/validate`
   **Headers:**
   ```
   Authorization: Bearer <jwt-token>
   ```

### 4. **Refresh Token**
   **POST** `/api/auth/refresh-token`

### 5. **Send Verification Code**
   **POST** `/api/auth/send-verification-code`
   ```json
   {
     "email": "test@example.com"
   }
   ```

### 6. **Verify Code**
   **POST** `/api/auth/verify-code`
   ```json
   {
     "email": "test@example.com",
     "code": "123456"
   }
   ```

### 7. **Update Profile Image**
   **PUT** `/api/auth/update-image`
   ```json
   {
     "image": "base64EncodedImageString"
   }
   ```

## Architecture
### **CQRS & Mediator Pattern**
- The project uses the CQRS pattern with **MediatR** to separate read and write operations.
- Queries handle data retrieval, and Commands handle mutations (Create, Update, Delete).

### **Identity & Authentication**
- Uses **ASP.NET Core Identity** for user management.
- JWT is used for securing endpoints.
- **Refresh Token Mechanism** implemented for session management.

### **Entity Framework Core (SQL-based Database)**
- Uses **ApplicationDbContext** for database operations.
- Implements repository pattern.

## Security Measures
- **Password Hashing** with ASP.NET Core Identity.
- **JWT Authentication** with secure **refresh token**.
- **Email Verification** before allowing login.
- **Role-based Authorization** using Identity Roles.

## Contribution
If you want to contribute:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request.

## License
This project is licensed under the MIT License.

## Contact
For any questions, please contact [nihadcoding@gmail.com]

