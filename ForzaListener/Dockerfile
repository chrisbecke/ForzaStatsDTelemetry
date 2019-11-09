FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["ForzaListner/ForzaListner.csproj", "ForzaListner/"]
RUN dotnet restore "ForzaListner/ForzaListner.csproj"
COPY . .
WORKDIR "/src/ForzaListner"
RUN dotnet build "ForzaListner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ForzaListner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ForzaListner.dll"]