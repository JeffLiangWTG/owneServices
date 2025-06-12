FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
ENV HTTPS_PROXY=http://sydco-spxy-1.wtg.zone:8080
WORKDIR /src
COPY OCM_POC.sln .
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "API.dll"]
