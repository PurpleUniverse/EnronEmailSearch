FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to restore dependencies
COPY ["EnronEmailSearch.sln", "./"]
COPY ["EnronEmailSearch/Core/Core.csproj", "EnronEmailSearch/Core/"]
COPY ["EnronEmailSearch/Infrastructure/Infrastructure.csproj", "EnronEmailSearch/Infrastructure/"]
# Change this line to match the actual location of your Web project
COPY ["EnronEmailSearch/EnronEmailSearch.csproj", "EnronEmailSearch/"]

# Restore dependencies before copying the rest of the source code
RUN dotnet restore "EnronEmailSearch/EnronEmailSearch.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/EnronEmailSearch"
RUN dotnet build "EnronEmailSearch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnronEmailSearch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnronEmailSearch.dll"]