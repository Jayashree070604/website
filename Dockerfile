FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ProjectKnowledgePortal/ProjectKnowledgePortal.csproj", "ProjectKnowledgePortal/"]
RUN dotnet restore "ProjectKnowledgePortal/ProjectKnowledgePortal.csproj"
COPY . .
WORKDIR "/src/ProjectKnowledgePortal"
RUN dotnet publish "ProjectKnowledgePortal.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProjectKnowledgePortal.dll"]
