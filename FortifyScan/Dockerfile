FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FortifyScan/FortifyScan.csproj", "FortifyScan/"]
RUN dotnet restore "FortifyScan/FortifyScan.csproj"
COPY . .
WORKDIR "/src/FortifyScan"
RUN dotnet build "FortifyScan.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FortifyScan.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FortifyScan.dll"]
