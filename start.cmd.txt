@echo off
cd ECommerce
dotnet publish -c Release -o ../publish
cd ../publish
dotnet ECommerce.dll