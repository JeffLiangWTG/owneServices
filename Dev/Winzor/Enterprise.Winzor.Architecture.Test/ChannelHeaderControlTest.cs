using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ChannelHeaderControlTest
{
	[Test, WithPlaywrightPage]
	public async Task ControlsInsideFadePanelShouldHaveCorrectZOrder()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var group = VisualBoardsTestHelper.CreateGroup(factory, "AAA");
			var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Container", releaseGroupPK: group.PK);
			var system = VisualBoardsTestHelper.CreateSystem(factory, "XYZ");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
			var channel = VisualBoardsTestHelper.CreatePrimaryChannelForSection(section);
			var cell = new CellContent(0, 0, CellContentType.ChannelHeading);
			var staff = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(factory);
			cell.Channel = viewModel.CreateChannelForTest(staff);
			var channelHeaderControl = new ChannelHeaderControl(cell, viewModel);

			var form = new ZForm(system);
			form.Controls.Add(channelHeaderControl);
			return form;
		});

		// make every child inside as big as the panel, so that we can pick any point to see the element z-order using document.elementsFromPoint(x, y)
		var containerPanel = page.Locator(".fadepanel > .panel");
		await Assertions.Expect(containerPanel).ToBeVisibleAsync();

		await containerPanel.EvaluateAsync(@"e => {
			Array.prototype.forEach.call(e.children, item => {
				item.style.left = 0;
				item.style.top = 0;
				item.style.width = e.style.width;
				item.style.height = e.style.height;
			});
		}");

		// get ordered list of elements from point (0, 0) and only retrieve those we care
		var elementsInZOrder = await Page.EvaluateHandleAsync(@"e => document.elementsFromPoint(0, 0).filter(item => item.matches('.label, .panel div:has(.picturebox)'))");

		foreach (var index in Enumerable.Range(0, 5))
		{
			var elementInZOrder = await elementsInZOrder.EvaluateAsync<string>($"items => items[{index}].outerHTML");
			var elementInCollectionOrder = await containerPanel.EvaluateAsync<string>($"e => e.children[{index}].outerHTML");

			// bacause in WinForms, Z-Order is effectively the same thing as collection order
			Assert.That(elementInZOrder, Is.EqualTo(elementInCollectionOrder));
		}
	}
}
