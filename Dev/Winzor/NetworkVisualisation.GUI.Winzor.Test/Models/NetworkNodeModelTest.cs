using System.Drawing;
using Blazor.Diagrams.Core.Events;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using Moq.Protected;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using Size = Blazor.Diagrams.Core.Geometry.Size;

namespace NetworkVisualisation.GUI.Winzor.Test.Models;

public abstract class NetworkNodeModelTest<T> : BunitTestContext where T : NetworkNodeModel
{
	protected abstract T CreateNodeModel(NodeViewModel viewModel);

	protected static TestCaseData PropertyTestCase<U>(string name, Func<U> expectationFactory,
		Func<T, U?> query,
		Action<Mock<NodeViewModel>, EntityForTest, U> update, int expectedCallsToChanged = 1)
	{
		return new TestCaseData(name, expectationFactory, query, update, expectedCallsToChanged).SetName("{m}({0})");
	}

	static IEnumerable<TestCaseData> CommonProperties
	{
		get
		{
			yield return PropertyTestCase(nameof(NodeViewModel.DiagramName), () => "Foo",
				m => m.DiagramName, (_, e, x) => e.Name = x);
			yield return PropertyTestCase(nameof(NodeViewModel.StatusTextWeight),
				() => NodeFontWeight.Medium,
				m => m.StatusTextWeight, (_, e, _) => e.Status = WorkStatus.Complete);
			yield return PropertyTestCase(nameof(NodeViewModel.ForegroundColor),
				() => Color.Red, m => m.ForegroundColor, (_, e, x) => e.ForeColor = x);
			yield return PropertyTestCase(nameof(NodeViewModel.StatusColors),
				() => new NodeColors(new ColorOffset[] { new (Color.Red, 0) }, 90d, 1d),
				m => m.StatusColors,
				(v, _, _) => v.Protected().Setup<IEnumerable<ColorOffset>>("GetStatusColors")
					.Returns(new[] { new ColorOffset(Color.Red, 0) }));
			yield return PropertyTestCase(nameof(NodeViewModel.ToolTip), () => "Foo", m => m.Tooltip,
				(_, e, x) => e.Name = x);
			yield return PropertyTestCase(nameof(NodeViewModel.ZIndex), () => 100, m => m.ZIndex,
				(v, _, x) => v.Object.ZIndex = x);
			yield return PropertyTestCase(nameof(NodeViewModel.IsSelected), () => true, m => m.IsSelected,
				(v, _, x) => v.Object.IsSelected = x);
		}
	}

	[TestCaseSource(nameof(CommonProperties))]
	public void OnlyTheChangedIsUpdatedOnPropertyChanged_CommonProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<T, TValue> getModelValue,  Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertOnlyTheChangedIsUpdatedOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue, expectedCallsToChanged);

	[TestCaseSource(nameof(CommonProperties))]
	public void EquivalentValuesAreIgnoredOnPropertyChanged_CommonProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<T, TValue> getModelValue,  Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertEquivalentValuesAreIgnoredOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue, expectedCallsToChanged);

	[TestCaseSource(nameof(CommonProperties))]
	public void PropertyChangedFromServerIsTriggeredOnPropertyChanged_CommonProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<T, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertPropertyChangedFromServerIsTriggeredOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue);

	[TestCaseSource(nameof(CommonProperties))]
	public void EquivalentValuesDontTriggerPropertyChangedFromServer_CommonProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<T, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertEquivalentValuesDontTriggerPropertyChangedFromServer(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue);

	[Test]
	public void TestUpdatingPositionClientSide_OverridesBothXAndYOnServer()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity();
		var viewModel = new NodeViewModel(entity, network)
		{
			X = 10,
			Y = 20
		};
		var model = CreateNodeModel(viewModel);
		var intendedPosition = new Point(300, 400);

		model.SetPosition(intendedPosition.X, intendedPosition.Y);
		model.TriggerMoved();

		Assert.That(model.Position, Is.EqualTo(intendedPosition));
		Assert.That(viewModel.X, Is.EqualTo(intendedPosition.X));
		Assert.That(viewModel.Y, Is.EqualTo(intendedPosition.Y));
	}

	[Test]
	public void UpdatingSizeClientSide_OverridesBothWidthAndHeightOnServer()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity();
		var viewModel = new NodeViewModel(entity, network)
		{
			Width = 10,
			Height = 20
		};
		var model = CreateNodeModel(viewModel);
		var intendedSize = new Size(300, 400);

		model.Size = intendedSize;
		model.TriggerResized();

		Assert.That(model.Size, Is.EqualTo(intendedSize));
		Assert.That(model.Size, Is.EqualTo(intendedSize));
		Assert.That(viewModel.Width, Is.EqualTo(intendedSize.Width));
		Assert.That(viewModel.Height, Is.EqualTo(intendedSize.Height));
	}

	[Test]
	public void UpdatingPositionServerSide_OverridesBothXAndYOnClient()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity();
		var viewModel = new NodeViewModel(entity, network)
		{
			X = 10,
			Y = 20
		};
		var model = CreateNodeModel(viewModel);
		var intendedPosition = new Point(300, 400);

		viewModel.X = intendedPosition.X;
		viewModel.Y = intendedPosition.Y;
		Assert.That(model.Position, Is.EqualTo(intendedPosition));
		Assert.That(viewModel.X, Is.EqualTo(intendedPosition.X));
		Assert.That(viewModel.Y, Is.EqualTo(intendedPosition.Y));
	}

	[Test]
	public void UpdatingSizeServerSide_OverridesBothWidthAndHeightOnClient()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity();
		var viewModel = new NodeViewModel(entity, network)
		{
			Width = 10,
			Height = 20
		};
		var model = CreateNodeModel(viewModel);
		var intendedSize = new Size(300, 400);

		viewModel.Width = intendedSize.Width;
		viewModel.Height = intendedSize.Height;
		Assert.That(model.Size, Is.EqualTo(intendedSize));
		Assert.That(viewModel.Width, Is.EqualTo(intendedSize.Width));
		Assert.That(viewModel.Height, Is.EqualTo(intendedSize.Height));
	}

	[Test]
	public void InvalidNodePositionOnViewModelUpdatesCorrectlyOnNodeModel()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity();
		var viewModel = new NodeViewModel(entity, network)
		{
			X = 0,
			Y = 0
		};
		var model = CreateNodeModel(viewModel);

		model.SetPosition(-100, -100);
		model.TriggerMoved();

		Assert.That(() => model.Position, Is.EqualTo(Point.Zero).After(3000, 100));
		Assert.That(viewModel.X, Is.EqualTo(0));
		Assert.That(viewModel.Y, Is.EqualTo(0));
	}

	[Test]
	public async Task InvalidNodeSizeOnViewModelUpdatesCorrectlyOnNodeModelAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity() { Width = 150, Height = 150, X = 350, Y = 110 };
		var rendered = await ctx.RenderControlOnFormAsync(() => TestHelpers.SetupNetworkUserControl(new[] { entity }, true, false));

		var jobNode = rendered.FindComponent<InDiagramNetworkNode<JobNodeModel, JobNode>>();

		var jobNodeModel = jobNode.Instance.Node!;
		var diagramModel = jobNode.Instance.Diagram!;

		await ctx.Renderer.Dispatcher.InvokeAsync(() => diagramModel.SelectModel(jobNodeModel, false));
		rendered.WaitForState(() => diagramModel.Controls.GetFor(jobNodeModel)!.Where(c => c is NodeResizeControl).Count() == 4, TimeSpan.FromSeconds(3));
		var resizerControl = (NodeResizeControl)diagramModel.Controls.GetFor(jobNodeModel)!.First(c => c is NodeResizeControl rc && rc.Class!.Contains("bottomright"))!;

		await ctx.Renderer.Dispatcher.InvokeAsync(async () =>
		{
			var eventArgs = new PointerEventArgs(0, 0, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			await resizerControl!.OnPointerDown(diagramModel, jobNodeModel, eventArgs);
			eventArgs = new PointerEventArgs(-150, -150, 0, 0, false, false, false, 1, 1, 1, 1, 1, 1, "arrow", true);
			diagramModel!.TriggerPointerMove(null, eventArgs);
			diagramModel!.TriggerPointerUp(null, eventArgs);
		});

		jobNodeModel.TriggerResized();

		Assert.That(jobNodeModel.Size, Is.EqualTo(new Size(NodeViewModel.MinAllowedNodeWidth, NodeViewModel.MinAllowedNodeHeight)));
		Assert.That(jobNodeModel.Size!.Width, Is.EqualTo(NodeViewModel.MinAllowedNodeWidth));
		Assert.That(jobNodeModel.Size!.Height, Is.EqualTo(NodeViewModel.MinAllowedNodeHeight));
	}

	[Test]
	public void TestResizeNodeWithChild()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 1000, Height = 1000, X = 0, Y = 0, Name = "Parent" };
		var childEntity = new Entity() { Width = 100, Height = 100, X = 850, Y = 850, Name = "Child" };

		parentEntity.AddChildEntities(new[] { childEntity });
		var parentViewModel = new NodeViewModel(parentEntity, network);
		var childViewModel = new NodeViewModel(childEntity, network);

		network.ScheduledNodes.Add(parentViewModel);
		network.ScheduledNodes.Add(childViewModel);
		var model = CreateNodeModel(parentViewModel);

		model.Position = new Point(700, 700);
		model.Size = new Size(300, 300);

		model.TriggerResized();

		Assert.That(() => parentViewModel.Width, Is.EqualTo(300).After(1000, 100));
		Assert.That(() => parentViewModel.Height, Is.EqualTo(300).After(1000, 100));
		Assert.That(() => parentViewModel.X, Is.EqualTo(700).After(1000, 100));
		Assert.That(() => parentViewModel.Y, Is.EqualTo(700).After(1000, 100));
	}

	[Test]
	public void TestStatusColor_AdjustForEntityInChannelWithColour()
	{
		var diagramEntity = new Entity { IsDiagramScaled = true };
		var network = new DummyNetwork { DiagramEntity = diagramEntity };

		var channel1 = new DummyChannel("ch1", 600, Color.Blue);
		var channel2 = new DummyChannel("ch2", 600, Color.Red);

		diagramEntity.DiagramChannels = new[] { channel1, channel2 };

		var entity = new Entity { Width = 150, Height = 150, X = 200, Y = 590 };

		network.Entities.Add(entity);

		var networkViewModel = new NetworkViewModel(network);
		var entityViewModel = new NodeViewModel(entity, networkViewModel);
		var networknodeModel = CreateNodeModel(entityViewModel);

		entityViewModel.AdjustToScale();

		Assert.That(networknodeModel.StatusColors.colors.First().color, Is.EqualTo(Color.Red));
	}

	[Test]
	public void ConstructNodeModel_ControlledSizeIsTrue()
	{
		var network = new NetworkViewModel(new DummyNetwork());
		var entity = new Entity();
		var viewModel = new NodeViewModel(entity, network);
		var model = CreateNodeModel(viewModel);

		Assert.That(model.ControlledSize, Is.True);
	}
}
