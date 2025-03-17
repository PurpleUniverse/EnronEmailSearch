FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnronEmailSearch/Indexer/Indexer.csproj", "EnronEmailSearch/Indexer/"]
COPY ["EnronEmailSearch/Core/Core.csproj", "EnronEmailSearch/Core/"]
COPY ["EnronEmailSearch/Infrastructure/Infrastructure.csproj", "EnronEmailSearch/Infrastructure/"]
RUN dotnet restore "EnronEmailSearch/Web/Web.csproj"
COPY . .
WORKDIR "/src/EnronEmailSearch/Web"
RUN dotnet build "Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]