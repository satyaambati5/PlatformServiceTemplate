FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Directory.Build.props", "src/"]
COPY ["src/Directory.Packages.props", "src/"]
COPY ["src/PlatformServiceTemplate.Domain/PlatformServiceTemplate.Domain.csproj", "src/PlatformServiceTemplate.Domain/"]
COPY ["src/PlatformServiceTemplate.Application/PlatformServiceTemplate.Application.csproj", "src/PlatformServiceTemplate.Application/"]
COPY ["src/PlatformServiceTemplate.Infrastructure/PlatformServiceTemplate.Infrastructure.csproj", "src/PlatformServiceTemplate.Infrastructure/"]
COPY ["src/PlatformServiceTemplate.Api/PlatformServiceTemplate.Api.csproj", "src/PlatformServiceTemplate.Api/"]
RUN dotnet restore "src/PlatformServiceTemplate.Api/PlatformServiceTemplate.Api.csproj"

COPY . .
RUN dotnet publish "src/PlatformServiceTemplate.Api/PlatformServiceTemplate.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PlatformServiceTemplate.Api.dll"]
