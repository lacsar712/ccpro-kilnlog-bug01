FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY KilnLog.Api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8700
EXPOSE 8700
ENTRYPOINT ["dotnet", "KilnLog.Api.dll"]
