# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TaskTracker.Domain/ TaskTracker.Domain/
COPY TaskTracker.Infrastructure/ TaskTracker.Infrastructure/
COPY TaskTracker.Api/ TaskTracker.Api/
RUN dotnet restore TaskTracker.Api/TaskTracker.Api.csproj
RUN dotnet publish TaskTracker.Api/TaskTracker.Api.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]
