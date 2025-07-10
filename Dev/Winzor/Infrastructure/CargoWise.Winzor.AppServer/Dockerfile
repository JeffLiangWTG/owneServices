#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat
# FROM mcr.microsoft.com/dotnet/framework/aspnet

#FROM microsoft/dotnet-framework:4.8-runtime-windowsservercore-1903
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-1909 AS base
#FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app
EXPOSE 80
EXPOSE 443

#FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS framework

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["AppServer.csproj", ""]
RUN dotnet restore "./AppServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AppServer.csproj" -c Debug -o /app/build

#FROM build AS publish
#RUN dotnet publish "BlazorApp.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "AppServer.dll"]
#ENTRYPOINT["BlazorApp.exe" ]
