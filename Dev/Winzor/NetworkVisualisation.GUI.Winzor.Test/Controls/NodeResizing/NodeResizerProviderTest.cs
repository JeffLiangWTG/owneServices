using Enterprise.Winzor.Architecture.Test;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using Entity = CargoWise.NetworkVisualisation.Business.Entity;

namespace NetworkVisualisation.GUI.Winzor.Test.Controls.NodeResizing;

class NodeResizerProviderTest : BunitTestContext
{
	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			yield return new TestCaseData("topleft", 300).SetName("TopLeftResizer");
			yield return new TestCaseData("topright", 300).SetName("TopRightResizer");
			yield return new TestCaseData("bottomleft", 100).SetName("BottomLeftResizer");
			yield return new TestCaseData("bottomright", 100).SetName("BottomRightResizer");
		}
	}

	[Test, WithPlaywrightPage, TestCaseSource(nameof(TestCases))]
	public async Task TestResizeNodeWhileScrollingAsync(string resizerName, int expectedHeightAfterResize)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var entities = new[] {
				new Entity()
				{
					Name = "Test",
					JobName = "Test",
					Width = 200,
					Height = 200,
					X = 10,
					Y = 200,
				}
			};

			var diagramEntity = Stub.DiagramEntity(supportDiagramVisualStyles: true, isDiagramScaled: false, showNonscheduledSection: false);
			var network = new NetworkBuilder().WithEntities(entities).WithDiagramEntity(diagramEntity).Build();
			return new NetworkUserControlBuilder().WithNetwork(network).Build();
		});

		var node = page.Locator(selector: "div.networknode--indiagram.jobnode--indiagram[title=\"Node: Test\"]");
		await node.ScrollIntoViewIfNeededAsync();
		await node.ClickAsync();
		var resizer = page.Locator(selector: $".networknode__resizer.{resizerName}");

		var nodeBoundingbox = await node.BoundingBoxAsync();
		Assert.That(nodeBoundingbox, Is.Not.Null);
		Assert.That(nodeBoundingbox.Height, Is.EqualTo(200).Within(10));

		var resizerBoundingBox = await resizer.BoundingBoxAsync();
		Assert.That(resizerBoundingBox, Is.Not.Null);

		await page.Mouse.MoveAsync(resizerBoundingBox.X + 3, resizerBoundingBox.Y + 3);
		await page.Mouse.DownAsync();

		var scrollCompleted = IPageExtensions.WaitForTheScrollToFinishAsync(page, ".diagramareausercontrol", 100);

		await page.Mouse.WheelAsync(0, -100);

		await scrollCompleted;

		await page.Mouse.UpAsync();

		var boxAfterScroll = await node.BoundingBoxAsync();
		Assert.That(boxAfterScroll, Is.Not.Null);

		Assert.That(() => boxAfterScroll.Height, Is.EqualTo(expectedHeightAfterResize).Within(10));
	}

	public static IEnumerable<TestCaseData> TestCasesWithChildren
	{
		get
		{
			yield return new TestCaseData("topleft", 300).SetName("TopLeftResizerWithChilren");
			yield return new TestCaseData("topright", 300).SetName("TopRightResizerWithChilren");
			yield return new TestCaseData("bottomleft", 145).SetName("BottomLeftResizerWithChilren");
			yield return new TestCaseData("bottomright", 145).SetName("BottomRightResizerWithChilren");
		}
	}

	[Test, WithPlaywrightPage, TestCaseSource(nameof(TestCasesWithChildren))]
	public async Task TestResizeNodeWhileScrollingDoesNotResizeChildrenAsync(string resizerName, int expectedParentHeightAfterResize)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var parentEntity = new Entity()
			{
				Name = "Test",
				JobName = "Test",
				Width = 200,
				Height = 200,
				X = 10,
				Y = 200,
				CanHaveChildren = true
			};

			var childEntity = new Entity()
			{
				Name = "Test Child",
				JobName = "Test Child",
				Width = 100,
				Height = 100,
				X = 10,
				Y = 240,
			};
			var entities = new[] {
			parentEntity, childEntity
		};

			parentEntity.AddChildEntity(childEntity);

			var diagramEntity = Stub.DiagramEntity(supportDiagramVisualStyles: true, isDiagramScaled: false, showNonscheduledSection: false);
			var network = new NetworkBuilder().WithEntities(entities).WithDiagramEntity(diagramEntity).Build();
			return new NetworkUserControlBuilder().WithNetwork(network).Build();
		});

		var parentNode = page.Locator("div.networknode--indiagram.jobnode--indiagram[title=\"Node: Test\"]");
		var childNode = page.Locator("div.networknode--indiagram.jobnode--indiagram[title=\"Node: Test Child\"]");
		await parentNode.ScrollIntoViewIfNeededAsync();
		await parentNode.ClickAsync();
		var resizer = page.Locator($".networknode__resizer.{resizerName}").First;

		var parentBoundingBox = await parentNode.BoundingBoxAsync();
		var childBoundingBox = await childNode.BoundingBoxAsync();
		Assert.That(parentBoundingBox, Is.Not.Null);
		Assert.That(childBoundingBox, Is.Not.Null);
		Assert.That(parentBoundingBox.Height, Is.EqualTo(200).Within(10));
		Assert.That(childBoundingBox.Height, Is.EqualTo(100).Within(10));

		var resizerBoundingBox = await resizer.BoundingBoxAsync();
		Assert.That(resizerBoundingBox, Is.Not.Null);

		await page.Mouse.MoveAsync(resizerBoundingBox.X + 3, resizerBoundingBox.Y + 3);
		await page.Mouse.DownAsync();

		var scrollCompleted = IPageExtensions.WaitForTheScrollToFinishAsync(page, ".diagramareausercontrol", 100);

		await page.Mouse.WheelAsync(0, -100);

		await scrollCompleted;

		await page.Mouse.UpAsync();

		var parentBoxAfterScroll = await parentNode.BoundingBoxAsync();
		var childBoxAfterScroll = await childNode.BoundingBoxAsync();
		Assert.That(parentBoxAfterScroll, Is.Not.Null);
		Assert.That(childBoxAfterScroll, Is.Not.Null);

		Assert.That(parentBoxAfterScroll.Height, Is.EqualTo(expectedParentHeightAfterResize).Within(10));
		Assert.That(childBoxAfterScroll.Height, Is.EqualTo(100).Within(10));
	}
}
