# Stage 1: Build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY src/CoursedogImporter/CoursedogImporter.csproj ./src/CoursedogImporter/
RUN dotnet restore ./src/CoursedogImporter/CoursedogImporter.csproj

COPY src/ ./src/
RUN dotnet publish ./src/CoursedogImporter/CoursedogImporter.csproj --configuration Release --output /app/publish

# Stage 2: Runtime

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CoursedogImporter.dll"]
