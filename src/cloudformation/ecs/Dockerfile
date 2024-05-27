FROM mcr.microsoft.com/dotnet/aspnet:6.0 as base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../../AnotherBlogEngine/AnotherBlogEngine/AnotherBlogEngine.csproj", "src/AnotherBlogEngine/"]
COPY ["../../AnotherBlogEngine/AnotherBlogEngine.Core/AnotherBlogEngine.Core.csproj", "src/AnotherBlogEngine.Core/"]
RUN dotnet restore "src/AnotherBlogEngine/AnotherBlogEngine.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "src/AnotherBlogEngine/AnotherBlogEngine.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "src/AnotherBlogEngine/AnotherBlogEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
ENV COMPlus_EnableDiagnostics=0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnotherBlogEngine.dll"]
