FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PMSystem.csproj", "./"]
RUN dotnet restore "PMSystem.csproj"

# Copy everything else (including Migrations folder)
COPY . .

# Build the project
RUN dotnet build "PMSystem.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PMSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Expose port 5001
EXPOSE 5001

# Copy published files
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PMSystem.dll"]