FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnronEmailSearch.Indexer/EnronEmailSearch.Indexer.csproj", "EnronEmailSearch.Indexer/"]
COPY ["EnronEmailSearch.Core/EnronEmailSearch.Core.csproj", "EnronEmailSearch.Core/"]
COPY ["EnronEmailSearch.Infrastructure/EnronEmailSearch.Infrastructure.csproj", "EnronEmailSearch.Infrastructure/"]
RUN dotnet restore "EnronEmailSearch.Indexer/EnronEmailSearch.Indexer.csproj"
COPY . .
WORKDIR "/src/EnronEmailSearch.Indexer"
RUN dotnet build "EnronEmailSearch.Indexer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnronEmailSearch.Indexer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnronEmailSearch.Indexer.dll"]