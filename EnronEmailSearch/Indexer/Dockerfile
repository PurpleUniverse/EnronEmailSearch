FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to restore dependencies
COPY ["EnronEmailSearch.sln", "./"]
COPY ["EnronEmailSearch/Core/Core.csproj", "EnronEmailSearch/Core/"]
COPY ["EnronEmailSearch/Infrastructure/Infrastructure.csproj", "EnronEmailSearch/Infrastructure/"]
COPY ["EnronEmailSearch/Indexer/Indexer.csproj", "EnronEmailSearch/Indexer/"]

# Restore dependencies before copying the rest of the source code
RUN dotnet restore "EnronEmailSearch/Indexer/Indexer.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/EnronEmailSearch/Indexer"
RUN dotnet build "Indexer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Indexer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Indexer.dll"]