@echo off

mkdir Bin

dotnet tool restore
dotnet tool run paket restore
