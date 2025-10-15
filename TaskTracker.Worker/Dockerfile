# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TaskTracker.Domain/ TaskTracker.Domain/
COPY TaskTracker.Infrastructure/ TaskTracker.Infrastructure/
COPY TaskTracker.Worker/ TaskTracker.Worker/
RUN dotnet restore TaskTracker.Worker/TaskTracker.Worker.csproj
RUN dotnet publish TaskTracker.Worker/TaskTracker.Worker.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "TaskTracker.Worker.dll"]
