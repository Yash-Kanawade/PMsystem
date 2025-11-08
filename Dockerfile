# Multi-platform Dockerfile for PMSystem
# Works on Windows (Docker Desktop), Linux, and macOS
# Supports: linux/amd64, linux/arm64

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PMSystem.csproj", "./"]
RUN dotnet restore "PMSystem.csproj" -a $TARGETARCH

# Copy everything else (including Migrations folder)
COPY . .

# Build the project
RUN dotnet build "PMSystem.csproj" -c Release -o /app/build -a $TARGETARCH --no-restore

# Publish the application
FROM build AS publish
ARG TARGETARCH
RUN dotnet publish "PMSystem.csproj" -c Release -o /app/publish -a $TARGETARCH --no-restore /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create a non-root user for better security
RUN addgroup --system --gid 1000 appuser \
    && adduser --system --uid 1000 --ingroup appuser --shell /bin/sh appuser

# Expose port 5001
EXPOSE 5001

# Copy published files
COPY --from=publish /app/publish .

# Change ownership of the app directory
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

ENTRYPOINT ["dotnet", "PMSystem.dll"]