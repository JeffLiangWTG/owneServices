FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

COPY NuGet.Config .
COPY *.csproj .
RUN export http_proxy=http://proxy.wtg.zone:8080 && \
    dotnet restore

COPY . .
RUN dotnet publish -o out /p:PublishWithAspNetCoreTargetManifest="false"

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
ENV ASPNETCORE_URLS http://+:80
ENV ASPNETCORE_ENVIRONMENT Development
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Hawking.CSI.Apps.XXXXXX.Send.dll"]
