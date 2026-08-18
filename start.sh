#!/bin/bash
cd ECommerce
dotnet publish -c Release -o ../publish
cd ../publish
exec dotnet ECommerce.dll
