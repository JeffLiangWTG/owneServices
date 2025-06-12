FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Hawking.eHub.Api/Hawking.eHub.Api.csproj Hawking.eHub.Api/
RUN dotnet restore Hawking.eHub.Api/Hawking.eHub.Api.csproj
COPY . .
WORKDIR /src/Hawking.eHub.Api
RUN dotnet build Hawking.eHub.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Hawking.eHub.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Hawking.eHub.Api.dll"]
