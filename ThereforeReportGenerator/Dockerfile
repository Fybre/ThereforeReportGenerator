#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y locales
RUN locale-gen en_AU.UTF-8 en_US.UTF-8
USER app
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ThereforeReportGenerator/ThereforeReportGenerator.csproj", "ThereforeReportGenerator/"]
RUN dotnet restore "./ThereforeReportGenerator/ThereforeReportGenerator.csproj"
COPY . .
WORKDIR "/src/ThereforeReportGenerator"
RUN dotnet build "./ThereforeReportGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ThereforeReportGenerator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThereforeReportGenerator.dll"]

USER root
RUN chown -R app ./Data
RUN chown -R app ./Imports
USER app