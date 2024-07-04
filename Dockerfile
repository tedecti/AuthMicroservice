FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y iputils-ping
USER $APP_UID
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AuthMicroservice.csproj", "./"]
RUN dotnet restore "AuthMicroservice.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "AuthMicroservice.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthMicroservice.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthMicroservice.dll"]

#FROM build as migrations
#RUN dotnet tool install --version 8.0.6 --global dotnet-ef
#ENV PATH="$PATH:/root/.dotnet/tools"
#RUN dotnet ef migrations add Initial
#ENTRYPOINT dotnet ef database update --project AuthMicroservice.csproj

