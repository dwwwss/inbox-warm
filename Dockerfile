# Use the .NET ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["InboxWarmUp/InboxWarmUp.csproj", "InboxWarmUp/"]
RUN dotnet restore "InboxWarmUp/InboxWarmUp.csproj"

# Copy the rest of the source code
COPY . .

# Set the working directory for building the project
WORKDIR "/src/InboxWarmUp"
RUN dotnet build "InboxWarmUp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Create the publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InboxWarmUp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the base image for the final stage
FROM base AS final
WORKDIR /app

# Copy the published output from the previous stage
COPY --from=publish /app/publish .

# Specify the entry point for the application
ENTRYPOINT ["dotnet", "InboxWarmUp.dll"]
