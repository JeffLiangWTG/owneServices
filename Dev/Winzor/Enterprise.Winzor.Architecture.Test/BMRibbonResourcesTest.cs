using System.IO;
using CargoWise.Blazor.Common;
using Enterprise.BufferManagement.NetworkVisualisation.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class BMRibbonResourcesTest
{
	[Test]
	public void ResourceStaticWebAssetsArePublishedWithAppServer()
	{
		var appServerWwwroot = Path.Combine(Directory.GetCurrentDirectory(), "..", BuildFileSystem.AppServerBin.PublishDirectoryPath, "wwwroot");
		Assert.Multiple(() =>
		{
			foreach (var icon in BMRibbonResources.Icons)
			{
				Assert.That(File.Exists(Path.Combine(appServerWwwroot, icon.Value)), $"The static asset {icon.Key} was not found.");
			}
		});
	}
}
