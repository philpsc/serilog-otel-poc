FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["grpc-web-api/grpc-web-api.csproj", "grpc-web-api/"]
RUN dotnet restore "grpc-web-api/grpc-web-api.csproj"
COPY . .
WORKDIR "/src/grpc-web-api"
RUN dotnet build "grpc-web-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "grpc-web-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "grpc-web-api.dll"]
