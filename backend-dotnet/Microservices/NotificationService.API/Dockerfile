# שלב 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# העתק קבצי הפרויקט והנכסים
COPY . ./

# Restore חבילות NuGet
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o out

# שלב 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# העתק את התוצאה מהשלב הקודם
COPY --from=build /app/out .

# הפעל את האפליקציה
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
