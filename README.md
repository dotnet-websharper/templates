# WebSharper.Templates

Dotnet `dotnet new` templates for WebSharper.

All templates default to `net11.0`. Use `--framework` to select an older target framework.

## Building

This repository produces NuGet packages implementing `dotnet new` templates. It does not require Visual Studio or local vsix tools to build; use the included build script to create release packages.

Build release packages:

```powershell
.\build CI-Release
```

`CI-Release` also runs `TestTemplatesRelease`, which uses `test-templates.fsx` to install the built package into a separate template hive, instantiate every template under `build/tests`, build them in Release, and check that the web projects serve their pages and scripts. To test an existing package without the rest of the build:

```powershell
dotnet fsi test-templates.fsx <path-to-nupkg> build/tests
```

Install and test a generated package locally:

```powershell
dotnet new install <path-to-nupkg>
dotnet new websharper-web -n MyApp -lang F#
```
