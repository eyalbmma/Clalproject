# ��� 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# ���� ���� ������� �������
COPY . ./

# Restore ������ NuGet
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o out

# ��� 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# ���� �� ������ ����� �����
COPY --from=build /app/out .

# ���� �� ���������
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
