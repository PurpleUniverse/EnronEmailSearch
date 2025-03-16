FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnronEmailSearch.Web/EnronEmailSearch.Web.csproj", "EnronEmailSearch.Web/"]
COPY ["EnronEmailSearch.Core/EnronEmailSearch.Core.csproj", "EnronEmailSearch.Core/"]
COPY ["EnronEmailSearch.Infrastructure/EnronEmailSearch.Infrastructure.csproj", "EnronEmailSearch.Infrastructure/"]
RUN dotnet restore "EnronEmailSearch.Web/EnronEmailSearch.Web.csproj"
COPY . .
WORKDIR "/src/EnronEmailSearch.Web"
RUN dotnet build "EnronEmailSearch.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnronEmailSearch.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnronEmailSearch.Web.dll"]