using System.Windows.Forms;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.Winzor.Architecture.Test;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.Components;

public abstract class LinkableNodeTest<TNodeViewModel, TNetworkNode, TNetworkNodeModel> : NetworkNodeTest<TNodeViewModel, TNetworkNode, TNetworkNodeModel>
	   where TNodeViewModel : NodeViewModel
	   where TNetworkNode : NetworkNode<TNetworkNodeModel>
	   where TNetworkNodeModel : LinkableNodeModel
{
	[Test]
	public void TestHasPortsInDiagram()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var linkableNode = RenderNodeInDiagram(entity);

		Assert.That(linkableNode.Instance.Node!.Ports, Has.Count.EqualTo(2));
		var leftPort = linkableNode.Find(".node__port.left");
		var rightPort = linkableNode.Find(".node__port.right");
		Assert.That(leftPort, Is.Not.Null);
		Assert.That(rightPort, Is.Not.Null);
	}

	[Test]
	public void TestHasNoPortsOutsideOfDiagram()
	{
		var entity = Stub.Entity(e => e.ShapeType = ShapeType);
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var ports = linkableNode.FindAll(".node__port");
		Assert.That(ports, Is.Empty);
	}

	[TestCase(EntityState.None)]
	[TestCase(EntityState.Inactive)]
	public void TestNoEntityStateIcon(EntityState entityState)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.EntityState = entityState; });
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__entitystateicon']";

		Assert.Throws<ElementNotFoundException>(() => linkableNode.Find(selector));
	}

	[TestCase(EntityState.Fixed, "pin_in.png")]
	[TestCase(EntityState.Approved, "ThumbUp.png")]
	[TestCase(EntityState.NotApproved, "ThumbUpQuestionMark.png")]
	[TestCase(EntityState.HasErrors, "Error.png")]
	[TestCase(EntityState.HasWarnings, "Warning.png")]
	[TestCase(EntityState.HasMessages, "Message.png")]
	public void TestEntityStateIcon(EntityState entityState, string iconFile)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.EntityState = entityState; });
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__entitystateicon']";

		var icon = linkableNode.Find(selector);
		Assert.That(icon.GetAttribute("src"), Does.EndWith(iconFile));
	}

	[TestCase(EntityState.HasErrors | EntityState.HasWarnings | EntityState.HasMessages | EntityState.Approved | EntityState.NotApproved | EntityState.Fixed | EntityState.Inactive | EntityState.None, "Error.png")]
	[TestCase(EntityState.HasWarnings | EntityState.HasMessages | EntityState.Approved | EntityState.NotApproved | EntityState.Fixed | EntityState.Inactive | EntityState.None, "Warning.png")]
	[TestCase(EntityState.HasMessages | EntityState.Approved | EntityState.NotApproved | EntityState.Fixed | EntityState.Inactive | EntityState.None, "Message.png")]
	[TestCase(EntityState.Approved | EntityState.NotApproved | EntityState.Fixed | EntityState.Inactive | EntityState.None, "ThumbUp.png")]
	[TestCase(EntityState.NotApproved | EntityState.Fixed | EntityState.Inactive | EntityState.None, "ThumbUpQuestionMark.png")]
	[TestCase(EntityState.Fixed | EntityState.Inactive | EntityState.None, "pin_in.png")]
	public void TestEntityStateIconAccordingToPriorityWhenMultipleStates(EntityState entityState, string iconFile)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.EntityState = entityState; });
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__entitystateicon']";

		var icon = linkableNode.Find(selector);
		Assert.That(icon.GetAttribute("src"), Does.EndWith(iconFile));
	}

	[TestCase(EntityState.HasErrors, EntityNotifcationType.Error, "Error")]
	[TestCase(EntityState.HasWarnings, EntityNotifcationType.Warning, "Warning")]
	[TestCase(EntityState.HasMessages, EntityNotifcationType.Message, "Message")]
	public void TestEntityStateIconHasTooltipWhenHasNotification(EntityState entityState, EntityNotifcationType entityNotifcationType, string notificationType)
	{
		var entity = Stub.Entity(e => {
			e.ShapeType = ShapeType;
			e.EntityState = entityState;
			e.EntityNotifications = new[] { new EntityNotification(entityNotifcationType, "abc") };
		});
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__entitystateicon']";

		var icon = linkableNode.Find(selector);

		Assert.That(icon.GetAttribute("title"), Does.Contain($"{notificationType}: abc"));
	}

	[TestCase(EntityState.Fixed)]
	[TestCase(EntityState.Approved)]
	[TestCase(EntityState.NotApproved)]
	public void TestEntityStateIconHasTooltipWhenNoNotification(EntityState entityState)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.EntityState = entityState; });
		var model = CreateNodeModel(entity);
		var linkableNode = RenderNode(model);

		var selector = "[class$='__entitystateicon']";
		var icon = linkableNode.Find(selector);

		Assert.That(icon.GetAttribute("title"), Is.EqualTo(model.EntityStateTooltip));
	}

	[Test]
	public void TestDescription()
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.Description = "Description"; });
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__description']";

		var description = linkableNode.Find(selector).InnerHtml;
		Assert.That(description, Is.EqualTo("Description"));
	}

	[TestCase(EntityState.None)]
	[TestCase(EntityState.Inactive)]
	public void TestNoNotificationHighlighter(EntityState entityState)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.EntityState = entityState; e.HasNotifications = true; });
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__notification']";

		Assert.Throws<ElementNotFoundException>(() => linkableNode.Find(selector));
	}

	[TestCase(EntityState.HasWarnings, "Orange")]
	[TestCase(EntityState.HasErrors, "Red")]
	[TestCase(EntityState.HasMessages, "DodgerBlue")]
	public void TestHighlighter(EntityState entityState, string brushColor)
	{
		var entity = Stub.Entity(e => { e.ShapeType = ShapeType; e.EntityState = entityState; e.HasNotifications = true; });
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__notification']";

		var notification = linkableNode.Find(selector);
		Assert.That(notification.GetAttribute("style"), Does.Contain(brushColor));
	}

	[TestCase(EntityState.HasWarnings, EntityNotifcationType.Warning, "Orange", "Warning")]
	[TestCase(EntityState.HasErrors, EntityNotifcationType.Error, "Red", "Error")]
	[TestCase(EntityState.HasMessages, EntityNotifcationType.Message, "DodgerBlue", "Message")]
	public void TestHighlighterToolTip(EntityState entityState, EntityNotifcationType entityNotifcationType, string brushColor, string notificationType)
	{
		var entity = Stub.Entity(e => {
			e.ShapeType = ShapeType;
			e.EntityState = entityState;
			e.HasNotifications = true;
			e.EntityNotifications = new[] { new EntityNotification(entityNotifcationType, "abc") };
		});
		var linkableNode = RenderNode(CreateNodeModel(entity));

		var selector = "[class$='__notification']";

		var notification = linkableNode.Find(selector);
		Assert.That(notification.GetAttribute("style"), Does.Contain(brushColor));

		Assert.That(notification.GetAttribute("title"), Does.Contain($"{notificationType}: abc"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestHighlighterCorrectCursorAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity = Stub.Entity(e => {
			e.ShapeType = ShapeType;
			e.EntityState = EntityState.HasWarnings;
			e.HasNotifications = true;
			e.EntityNotifications = new[] { new EntityNotification(EntityNotifcationType.Warning, "abc") };
		});
		var network = new NetworkBuilder().WithEntities(entity).Build();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 1250, Height = 700 };
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;

			form.Controls.Add(networkUserControl);
			return form;
		});

		var notification = page.Locator("[class$='__notification']");
		var box = (await notification.BoundingBoxAsync())!;

		var elementUnderCursor = (await page.EvaluateHandleAsync(
				$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
			))
			.AsElement()!;

		Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("default"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestWarningCorrectCursorAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var entity = Stub.Entity(e => {
			e.ShapeType = ShapeType;
			e.EntityState = EntityState.HasWarnings;
			e.HasNotifications = true;
			e.EntityNotifications = new[] { new EntityNotification(EntityNotifcationType.Warning, "abc") };
		});
		var network = new NetworkBuilder().WithEntities(entity).Build();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 1250, Height = 700 };
			var networkUserControl = new NetworkUserControlBuilder().WithNetwork(network).Build();
			networkUserControl.Width = 1250;
			networkUserControl.Height = 700;

			form.Controls.Add(networkUserControl);
			return form;
		});

		var notification = page.Locator("[class$='jobnode__entitystateicon']");
		var box = (await notification.BoundingBoxAsync())!;

		var elementUnderCursor = (await page.EvaluateHandleAsync(
				$"() => document.elementFromPoint({box.X + box.Width / 2}, {box.Y + box.Height / 2})"
			))
			.AsElement()!;

		Assert.That(await elementUnderCursor.GetComputedStyleAsync("cursor"), Is.EqualTo("default"));
	}
}
