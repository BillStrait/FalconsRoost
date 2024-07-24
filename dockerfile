#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish --runtime linux-arm64 --self-contained -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0.100-alpine3.18-arm64v8
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "FalconsRoost.dll"]
CMD ["trace","dt=YourDiscordToken", "oa=YourOpenAIKey"]