FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG TARGETPLATFORM
ARG BUILDPLATFORM
ARG TARGETARCH
WORKDIR /app

COPY . /app
RUN dotnet restore -a $TARGETARCH  Analysis

RUN dotnet publish -a $TARGETARCH --no-restore -c Release --property:PublishDir=/app/out Analysis

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out /app/.docker/cmd.sh /app

USER 999999

VOLUME csv_destination

CMD ["/app/cmd.sh"]
