# AI-Powered Insurance Underwriting Platform

An AI-assisted insurance underwriting platform built with **ASP.NET Core Web API**, **React**, and **PostgreSQL**. The application streamlines the insurance submission process by combining traditional underwriting workflows with AI-generated risk analysis using the **Groq API**.

## Features

- User authentication using JWT
- Secure password hashing with BCrypt
- Create and manage insurance submissions
- AI-assisted risk analysis using Groq LLM
- Automatic underwriting recommendation generation
- Audit trail for submission changes
- Role-based access for users and administrators
- RESTful APIs for frontend integration
- Global exception handling middleware
- PostgreSQL database integration

---

## Tech Stack

### Backend
- ASP.NET Core Web API (.NET)
- C#
- PostgreSQL
- JWT Authentication
- BCrypt Password Hashing
- Groq API
- REST APIs

### Frontend
- React
- TypeScript
- Vite
- CSS

---

## Project Structure

```
AI-Powered Insurance Underwriting Platform
│
├── Verity.Insurance.Api
│   ├── Controllers
│   ├── Services
│   ├── Infrastructure
│   ├── Contracts
│   ├── Middleware
│   └── Common
│
├── Verity.Insurance.Portal
│   ├── src
│   ├── public
│   └── components
│
└── Verity.Insurance.sln
```

---

## Key Functionalities

### Authentication
- User registration
- Login using JWT authentication
- Password hashing using BCrypt

### Insurance Submission
- Create insurance applications
- Retrieve submission details
- Update underwriting information

### AI Risk Analysis
- Sends submission details to the Groq API
- Generates AI-assisted risk assessments
- Returns underwriting recommendations

### Audit Logging
- Tracks important submission activities
- Maintains an audit trail for changes

---

## API Modules

- Authentication
- Users
- Insurance Submissions
- AI Risk Analysis

---

## Getting Started

### Prerequisites

- .NET SDK
- Node.js
- PostgreSQL

### Backend

```bash
cd Verity.Insurance.Api
dotnet restore
dotnet run
```

### Frontend

```bash
cd Verity.Insurance.Portal
npm install
npm run dev
```

---

## Environment Configuration

Create an `appsettings.json` file and configure:

- PostgreSQL connection string
- JWT Secret
- Groq API Key

> Sensitive configuration files are excluded from version control using `.gitignore`.

---

## Future Improvements

- Docker deployment
- Refresh token authentication
- File upload support for underwriting documents
- Dashboard with analytics
- Email notifications
- Unit and integration testing

---

## Learning Outcomes

This project strengthened my understanding of:

- ASP.NET Core Web API development
- REST API design
- JWT authentication and authorization
- PostgreSQL integration
- React frontend development
- AI service integration using Groq
- Layered application architecture
- Exception handling and middleware
- Secure password management

---

## Disclaimer

This project was developed as an independent learning project to simulate an insurance underwriting workflow. It is intended for educational and portfolio purposes.
