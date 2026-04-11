FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

# Copy project files and configs for NuGet restore caching
COPY ./NuGet.config .
COPY ./Directory.Build.props .
COPY ./sub/Directory.Build.props ./sub/
COPY ./sub/MyOptiAlloySite/MyOptiAlloySite/Directory.Build.props ./sub/MyOptiAlloySite/MyOptiAlloySite/
COPY ./sub/MyOptiAlloySite/MyOptiAlloySite/nuget.config ./sub/MyOptiAlloySite/MyOptiAlloySite/
COPY ./sub/MyOptiAlloySite/MyOptiAlloySite/MyOptiAlloySite.csproj ./sub/MyOptiAlloySite/MyOptiAlloySite/
COPY ./src/OptiPowerTools.Hangfire.Tools/OptiPowerTools.Hangfire.Tools.csproj ./src/OptiPowerTools.Hangfire.Tools/
COPY ./src/OptiPowerTools.Hangfire/OptiPowerTools.Hangfire.csproj ./src/OptiPowerTools.Hangfire/
COPY ./src/OptiPowerTools.Hangfire.Web/OptiPowerTools.Hangfire.Web.csproj ./src/OptiPowerTools.Hangfire.Web/

RUN dotnet restore src/OptiPowerTools.Hangfire.Web/OptiPowerTools.Hangfire.Web.csproj

WORKDIR /src/src/OptiPowerTools.Hangfire.Web

EXPOSE 80
EXPOSE 443

ENTRYPOINT dotnet run --no-launch-profile
