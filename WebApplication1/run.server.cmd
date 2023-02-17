@echo off

set ASPNETCORE_ENVIRONMENT=development
set ASPNETCORE_URLS=https://localhost:7000;http://localhost:5259

dotnet run
