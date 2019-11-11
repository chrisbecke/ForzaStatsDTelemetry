FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ForzaListener/ForzaListener.csproj ForzaListener/
RUN dotnet restore "ForzaListener/ForzaListener.csproj"
COPY . .
WORKDIR "/src/ForzaListener"
RUN dotnet build "ForzaListener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ForzaListener.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForzaListener.dll"]
