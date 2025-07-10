using CargoWise.Blazor.Common;
using CargoWise.NetworkVisualisation.GUI;

namespace NetworkVisualisation.GUI.Winzor.Test;

class DefaultNetworkRibbonResourcesTest
{
	[Test]
	public void ResourceStaticWebAssetsArePublishedWithAppServer()
	{
		var appServerWwwroot = Path.Combine(Directory.GetCurrentDirectory(), "..", BuildFileSystem.AppServerBin.PublishDirectoryPath, "wwwroot");
		Assert.Multiple(() =>
		{
			foreach (var icon in DefaultNetworkRibbonResources.Icons)
			{
				Assert.That(File.Exists(Path.Combine(appServerWwwroot, icon.Value)), $"The static asset {icon.Key} was not found.");
			}
		});
	}
}
