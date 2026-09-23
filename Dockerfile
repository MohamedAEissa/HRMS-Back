# 1. Base Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Build Image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HR- Management System/HR- Management System.csproj", "HR- Management System/"]
COPY ["HR-Infrastructure/HR-Infrastructure.csproj", "HR-Infrastructure/"]
COPY ["HR-Application/HR-Application.csproj", "HR-Application/"]
COPY ["HR-Domain/HR-Domain.csproj", "HR-Domain/"]

RUN dotnet restore "HR- Management System/HR- Management System.csproj"

COPY . .
WORKDIR "/src/HR- Management System"
RUN dotnet build "HR- Management System.csproj" -c Release -o /app/build

# 3. Publish Image
FROM build AS publish
RUN dotnet publish "HR- Management System.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HR- Management System.dll"]