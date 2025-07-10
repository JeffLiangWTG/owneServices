# CargoWise.Analyzers

## 1. What is CargoWise.Analyzers

CargoWise.Analyzers is a collection of .NET Roslyn analyzers for internal WiseTech use. The analyzers are designed to help developers write code that is consistent with WiseTech's coding standards and prevent common errors. This set of analyzers covers WiseTech-specific rules and conventions.

CargoWise.Analyzers complements [WTG.Analyzers](https://github.com/WiseTechGlobal/WTG.Analyzers) which contains a collection of .NET Roslyn analyzers that are more general in nature.

A list of the analyzers included in CargoWise.Analyzers can be found here: https://devops.wisetechglobal.com/wtg/Content/_wiki/wikis/Content.wiki/6571/Roslyn-analyzers

The latest version of CargoWise.Analyzers can be found on the internal ProGet server: https://proget.wtg.zone/feeds/WTG-Internal/CargoWise.Analyzers/versions

<br>

## 2. How Dev consumes CargoWise.Analyzers

This package is deployed to the internal [WiseTech ProGet server](https://proget.wtg.zone/feeds/WTG-Internal/CargoWise.Analyzers/versions).

In Dev the package is specified in paket.dependencies:

```xml
nuget CargoWise.Analyzers
```

The version of CargoWise.Analyzers consumed is specified in the paket.lock file:

```xml
NUGET
  remote: https://proget.wtg.zone/nuget/WTG-Internal/v3/index.json
    CargoWise.Analyzers (25.2.1)
```

The Build.xml file copies the package into the Bin folder:

```xml
<Filename DeployToClients="false" CopyFrom="packages\build\CargoWise.Analyzers\analyzers\dotnet\cs">CargoWise.Analyzers.dll</Filename>
```

<br>

## 3. How to consume a new package in Dev

If a new version of CargoWise.Analyzers has been released, you can consume it by updating the version in the paket.lock file.
Example: If the old version was 25.1.1 and the new version is 25.2.1, you will need to make the following change to the paket.lock file from:

```xml
NUGET
  remote: https://proget.wtg.zone/nuget/WTG-Internal/v3/index.json
    CargoWise.Analyzers (25.1.1)
```

To:

```xml
NUGET
  remote: https://proget.wtg.zone/nuget/WTG-Internal/v3/index.json
    CargoWise.Analyzers (25.2.1)
```
