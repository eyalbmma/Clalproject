# שלב ראשון - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# העתק קבצי הפרויקט והנוספים
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# שלב שני - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ServiceAppointment.dll"]
