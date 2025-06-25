# NuGetDeployer

This target provides a means to push nuget packages to our internal ProGet registry. It is designed to be consumed by build targets via nuget package reference, and specified as the build deployer type in that target's build.xml file. Therefore, using it requires that your build target only deploys nuget packages. It can deploy multiple packages via separate deployment targets (see below section), but it cannot deploy nuget packages and something else (e.g. a web application).

# Usage

1. Reference the `WTG.DevTools.NuGetDeployer` package using your chosen package management tool.

2. In your `build.xml` file, add the following:

```xml
<DatSettings
	BuildDeployerType="WTG.DevTools.NuGetDeployer.NuGetPackageDeployer, WTG.DevTools.NuGetDeployer"
	...>
```

3. And for every nuget package you wish to deploy, add a `DeploymentTarget` element as shown below.

```xml
<DatSettings
	BuildDeployerType="WTG.DevTools.NuGetDeployer.NuGetPackageDeployer, WTG.DevTools.NuGetDeployer"
	BuildDeployerRuntime="DotNet64"
	...>
	<DeploymentTargets>
		<DeploymentTarget BuildConfiguration="Release" Type="DeployOnDemand" DeploymentConfiguration="WTG.Dat.TestFramework.*" />
	</DeploymentTargets>
```

Where `"WTG.Dat.TestFramework.*"` corresponds to the path of your file(s) with `.nupkg` extension in your build output directory.

> [!IMPORTANT]
> The `.*` in the `DeploymentConfiguration` is necessary since the value is used to locate nupkg files, which will include version numbers in their names.
