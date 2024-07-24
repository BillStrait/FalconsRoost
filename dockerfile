#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
# syntax=docker/dockerfile:1

# Base stage for building
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
ARG TARGETPLATFORM
RUN if [ "$DOCKER_TAG" = "LinuxAMD64" ]; then \
        dotnet publish --runtime linux-x64 --self-contained -c Release -o out; \
    else \
        dotnet publish --runtime linux-arm64 --self-contained -c Release -o out; \
    fi

# Runtime image
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "FalconsRoost.dll"]
CMD ["trace","dt=YourDiscordToken", "oa=YourOpenAIKey"]
