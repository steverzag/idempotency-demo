# IdempotencyDemo

This is a simple .NET project demonstrating how to implement idempotency in Web APIs using two main approaches:

   - A basic in-memory strategy, ideal for learning or lightweight use cases
   - A cached approach, simulating more robust, scalable scenarios

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server Database
- Container runtime as [Docker Desktop](https://www.docker.com/products/docker-desktop/) or [Podman](https://podman.io/)

## Installation
1. Clone this repository:
   ```sh
   git clone https://github.com/steverzag/idempotency-demo.git
   cd <repository-folder>
   ```
2. Restore dependencies:
   ```sh
   dotnet restore
   ```
3. Build the application:
   ```sh
   dotnet build
   ```
4. Apply Database Migrations:
  ```sh
  dotnet ef database update --project ./IdempotencyDemo.API/IdempotencyDemo.API.csproj
  ```
  Make sure you have the EF Core tools installed globally:
  ```sh
  dotnet tool install --global dotnet-ef
  ```
  and you configure your connection string at [appsettings](IdempotencyDemo.API/appsettings.json) file


## Running the Application
To run the application locally:
   ```sh
   dotnet run --project ./IdempotencyDemo.API/IdempotencyDemo.API.csproj
   ```

Or to run the application using [Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
   ```
   dotnet run --project ./IdempotencyDemo.AppHost/IdempotencyDemo.AppHost.csproj
   ```

Being this last the default one to use if want to take the cached implementation for handling idempotency.

By default, the application will be available at `http://localhost:5000` (or `https://localhost:5001` for HTTPS).

## Usage
The application exposes the endpoints that allows to create or modify users. It emplements two ways to manage idempotency on the Create User endpoint(POST /users). You can use one or the other by toggle them on the [User Endpoints](IdempotencyDemo.API/UserEndpoints.cs) file.

![Toogle idempotency filters on create user endpoint](https://github.com/steverzag/docs-assets/blob/main/images/idempotency-demo-toggle-filters.png)

> [!TIP]
> Refer to the [./IdempotencyDemo.API/IdempotencyDemo.http](IdempotencyDemo.API/IdempotencyDemo.http) file for example requests and usage.
    