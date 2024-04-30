FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Asteroids.Web/Asteroids.Web.csproj", "Asteroids.Web/"]
RUN pwd
COPY ["../Asteroids.Shared/Asteroids.Shared.csproj", "Asteroids.Shared/"]
RUN dotnet clean "./Asteroids.Web/./Asteroids.Web.csproj"
RUN dotnet restore "./Asteroids.Web/./Asteroids.Web.csproj"
COPY . .
WORKDIR "/src/Asteroids.Web"
RUN dotnet build "./Asteroids.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

ARG SIGNALR_HUB_URL_ARG
ENV SIGNALR_HUB_URL=$SIGNALR_HUB_URL_ARG
RUN sed -i "s|\${SIGNALR_HUB_URL}|$SIGNALR_HUB_URL|g" wwwroot/appsettings.json

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Asteroids.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine
WORKDIR /user/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY Asteroids.Web/nginx.conf /etc/nginx/nginx.conf