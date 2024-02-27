# Image to build the project
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /app

COPY ./src ./

RUN dotnet build --configuration Release
RUN mkdir -p ./bin/Release/netcoreapp3.1/libretro/cores
COPY ./cores/* ./bin/Release/netcoreapp3.1/libretro/cores/
COPY ./ROMs/* ./bin/Release/netcoreapp3.1/
COPY ./token ./bin/Release/netcoreapp3.1/

# Image to run the project
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
# to persist the save data over reboots
VOLUME /app

# For a ROM named emerald.gba and a core named mgba_libretro.so
# The full name is not needed, they get automatically appended
ARG ROM=emerald
ARG CORE=mgba

COPY --from=build-env  /app/bin/Release/netcoreapp3.1 .

RUN test -f /app/libretro/cores/${CORE}_libretro.so || (echo "ERROR: Core not found. Please download a libretro core and save it in Dew/cores" && exit 1)
RUN test -f /app/$ROM.gba || (echo "ERROR: ROM not found. Please download a ROM and save it in Dew/ROMs" && exit 1)
RUN test -f /app/token || (echo "ERROR: Discord token not found. Please save your token in Dew/token" && exit 1)

RUN apt update && apt install -y libc-dev

ENTRYPOINT ["./Dew"]
