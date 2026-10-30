FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TMS/TMS.csproj TMS/
RUN dotnet restore TMS/TMS.csproj

COPY TMS/ TMS/
RUN dotnet publish TMS/TMS.csproj -c Release --no-restore -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /out/ ./

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
ENV Tms__DataDirectory=/tmp/tms

USER $APP_UID
EXPOSE 8080

# Honor the platform port. Vercel's project PORT setting must match this listener.
CMD ["sh", "-c", "exec dotnet TMS.dll --urls \"http://0.0.0.0:${PORT:-8080}\""]
