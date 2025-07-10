using System.Linq;
using System.Threading.Tasks;
using Enterprise.DocumentScanning.GUI.AllocateEDocs;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

public class FileBrowserUserControlTest
{
	[Test, WithPlaywrightPage]
	public async Task TextboxUpdateDirectoryWhenATreeNodeIsClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new FileBrowserUserControl());
		var nodes = await page.Locator(".treeview__node").AllAsync();
		var input = page.Locator("input");

		foreach (var node in nodes.Skip(1))
		{
			var nodeText = node.Locator(".treeview__nodetext");
			var nodeName = await nodeText.TextContentAsync();
			await nodeText.ClickAsync(new LocatorClickOptions { Delay = 100 });
			Assert.That(await input.InputValueAsync(), Does.Contain(nodeName));

			if (node.Locator(".treeview__nodebutton") != null)
			{
				break;
			}
		}
	}
}
