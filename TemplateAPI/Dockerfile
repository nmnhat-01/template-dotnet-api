# ==========================
# Stage 1: Build
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# copy toàn bộ source từ repo vào container
COPY .. .

# vào thư mục web api
WORKDIR /src/TemplateAPI

# restore + publish
RUN dotnet restore TemplateAPI.csproj
RUN dotnet publish TemplateAPI.csproj -c Release -o /app/publish

# ==========================
# Stage 2: Runtime
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TemplateAPI.dll"]