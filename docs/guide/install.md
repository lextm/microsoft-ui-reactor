# Installing Microsoft.UI.Reactor

There are two paths to start a Reactor app, depending on whether you want a fresh project or are adding Reactor to an existing one.

> **Status:** Reactor packages are pre-release. Until v1.0 every version is a prerelease, and you must opt in by allowing prereleases in your `<PackageReference>` (e.g. `Version="0.1.0-*"`).

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or newer)
- Windows 10 19041 / Windows 11

## Option 1 — `dotnet new` (recommended)

Install the template package once:

```bash
dotnet new install Microsoft.UI.Reactor.Templates
```

Then scaffold an app:

```bash
dotnet new reactor -n MyApp
cd MyApp
dotnet run
```

This produces a single-file program with one `<PackageReference>` to `Microsoft.UI.Reactor`. No clone, no source enlistment.

Override the framework version at scaffold time:

```bash
dotnet new reactor -n MyApp --ReactorVersion 0.1.0-pr.42.abc1234
```

## Option 2 — add to an existing project

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows10.0.22621.0</TargetFramework>
    <Platforms>x64;ARM64</Platforms>
    <RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.UI.Reactor" Version="0.1.0-*" />
  </ItemGroup>
</Project>
```

> `RuntimeIdentifiers` must be declared in the consumer csproj (NuGet restore can't pick it up from the package's transitive props). The `Microsoft.UI.Reactor` props auto-select an appropriate `RuntimeIdentifier` so `dotnet run` and `dotnet build` work without `-p:Platform=x64`.

The package transitively brings in `Microsoft.WindowsAppSDK` 2.0.0-experimental6, the Roslyn analyzers, and the localization source generator. Defaults like `UseWinUI=true` and `WindowsPackageType=None` are applied automatically by `build/Microsoft.UI.Reactor.props`.

## Feeds (during preview)

Until Reactor is on NuGet.org, packages are produced as PR / CI artifacts. Either download the `nupkg-*.zip` from a [GitHub Actions run](https://github.com/microsoft/microsoft-ui-reactor/actions), or build them locally from a Reactor enlistment:

```powershell
# from the repo root
.\pack.ps1                              # produces 0.1.0-local nupkgs in artifacts\nupkg
.\pack.ps1 -Version 0.1.0-preview.7     # versioned
.\pack.ps1 -OutputPath C:\reactor-feed  # write straight into a local feed
```

Then add the output directory as a NuGet feed:

```bash
dotnet nuget add source C:\reactor-feed --name reactor-local
```

After that, `dotnet new install Microsoft.UI.Reactor.Templates` and `dotnet add package Microsoft.UI.Reactor` will resolve from the local feed.

## Hello, world

```csharp
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

ReactorApp.Run<App>("My App", width: 900, height: 600);

class App : Component
{
    public override Element Render()
    {
        var (name, setName) = UseState("World");

        return VStack(
            Heading($"Hello, {name}!"),
            TextField(name, setName, placeholder: "Your name")
        );
    }
}
```

## Contributors — building from source

If you're working on Reactor itself, keep cloning the repo and using a `<ProjectReference>`. The `mur --create` CLI supports both modes:

```bash
mur --create MyApp                              # PackageReference (default)
mur --create MyApp --from-source                # sibling ProjectReference
mur --create MyApp --reactor-version 0.1.0-*    # custom version
```

See [spec 022 — Packaging and Distribution](../specs/022-packaging-and-distribution.md) for the full distribution plan.
