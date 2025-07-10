using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.JobNode;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

namespace NetworkVisualisation.GUI.Winzor.Test.Models;

public class JobNodeModelTest : NetworkNodeModelTest<JobNodeModel>
{
#pragma warning disable CA2000 // Dispose objects before losing scope
	protected override JobNodeModel CreateNodeModel(NodeViewModel viewModel) => new (new JobNodeService(viewModel, new NetworkUserControlForTest(Renderer)));
#pragma warning restore CA2000 // Dispose objects before losing scope

	static IEnumerable<TestCaseData> JobProperties
	{
		get
		{
			yield return PropertyTestCase(nameof(NodeViewModel.X), () => 100d, m => m.Position.X, (_, e, x) => e.X = x, 3);
			yield return PropertyTestCase(nameof(NodeViewModel.Y), () => 100d, m => m.Position.Y, (_, e, x) => e.Y = x, 3);
			yield return PropertyTestCase(nameof(NodeViewModel.Width), () => 100d, m => m.Size!.Width,
				(_, e, x) => e.Width = x, 2);
			yield return PropertyTestCase(nameof(NodeViewModel.Height), () => 100d, m => m.Size!.Height,
				(_, e, x) => e.Height = x, 2);
			yield return PropertyTestCase(nameof(NodeViewModel.Notes), () => "Foo",
				e => e.Notes, (v, _, x) => v.Object.Notes = x);
			yield return PropertyTestCase(nameof(NodeViewModel.JobNumber), () => "Foo", m => m.JobNumber,
				(_, e, x) => e.JobNumber = x, 2);
			yield return PropertyTestCase(nameof(NodeViewModel.JobNumberReadableText),
				() => "Foo",
				m => m.JobNumberReadableText, (v, _, x) => v.SetupGet(v => v.JobNumberReadableText).Returns(x));
			yield return PropertyTestCase(nameof(NodeViewModel.JobName), () => "Foo", m => m.JobName,
				(_, e, x) => e.JobName = x);
			yield return PropertyTestCase(nameof(NodeViewModel.AppliedAttributesReadableText), () => "Foo",
				m => m.AppliedAttributesReadableText,
				(_, e, _) => e.AppliedAffinities =
					new ImpObservableCollection<IAffinity>(new[] { new DummyAffinity(Guid.Empty, "Foo", Color.Black) })
			);
			yield return PropertyTestCase(nameof(NodeViewModel.DurationReadableText), () => "1:00",
				m => m.DurationReadableText, (_, e, _) => e.ExplicitDurationMinutes = 60);
			yield return PropertyTestCase(nameof(NodeViewModel.CompletionCriteria), () => "Foo",
				m => m.CompletionCriteria, (_, e, x) => e.CompletionCriteria = x);
			yield return PropertyTestCase(nameof(NodeViewModel.CompletionCriteriaTextColor),
				() => Color.Red, m => m.CompletionCriteriaTextColor,
				(_, e, x) =>
				{
					e.CompletionCriteria = "Foo";
					e.ForeColor = x;
				});
			yield return PropertyTestCase(nameof(NodeViewModel.Description), () => "Foo", m => m.Description,
				(_, e, x) => e.Description = x);
			yield return PropertyTestCase(nameof(NodeViewModel.ShowScheduleDetails), () => true,
				m => m.ShowScheduleDetails, (v, _, x) =>
				{
					v.SetupGet(v => v.ShowScheduleDetails).Returns(x);
				});
			yield return PropertyTestCase(nameof(NodeViewModel.StartDateReadableText),
				() => DateTime.Today.FormatAsBestReadableDateTime(), m => m.StartDateReadableText, (v, e, x) =>
				{
					v.SetupGet(v => v.ShowScheduleDetails).Returns(true);
					e.ScheduledStartTimeLocal = DateTime.Today;
				});
			yield return PropertyTestCase(nameof(NodeViewModel.RemainingDurationReadableText),
				() => "1:00", m => m.RemainingDurationReadableText, (v, e, _) =>
				{
					v.SetupGet(v => v.ShowScheduleDetails).Returns(true);
					e.RemainingDurationMinutes = 60;
				});
			yield return PropertyTestCase(nameof(NodeViewModel.FinishDateReadableText),
				() => DateTime.Today.FormatAsBestReadableDateTime(), m => m.FinishDateReadableText, (v, e, _) =>
				{
					v.SetupGet(v => v.ShowScheduleDetails).Returns(true);
					e.ScheduledEndTimeLocal = DateTime.Today;
				});
			yield return PropertyTestCase(nameof(NodeViewModel.Status), () => WorkStatus.Complete, m => m.Status,
				(_, e, x) => e.Status = x, 3);
			yield return PropertyTestCase(nameof(NodeViewModel.Entity.IsOnCriticalPath), () => true,
				m => m.IsOnCriticalPath, (_, e, x) => e.IsOnCriticalPath = x);
			yield return PropertyTestCase(nameof(NodeViewModel.StatusTooltip), () => "Foo",
				m => m.StatusTooltip, (_, e, x) => e.StatusDescription = x);
			yield return PropertyTestCase(nameof(NodeViewModel.AppliedAttributesTooltip),
				() => "Channel and applied affinities",
				m => m.AppliedAttributesTooltip, (_, e, _) => e.DiagramChannels = new[] { new DummyChannel("Foo") });
		}
	}

	[TestCaseSource(nameof(JobProperties))]
	public void OnlyTheChangedIsUpdatedOnPropertyChanged_JobProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<JobNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertOnlyTheChangedIsUpdatedOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue, expectedCallsToChanged);

	[TestCaseSource(nameof(JobProperties))]
	public void EquivalentValuesAreIgnoredOnPropertyChanged_JobProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<JobNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertEquivalentValuesAreIgnoredOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue, expectedCallsToChanged);

	[TestCaseSource(nameof(JobProperties))]
	public void PropertyChangedFromServerIsTriggeredOnPropertyChanged_JobProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<JobNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertPropertyChangedFromServerIsTriggeredOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue);

	[TestCaseSource(nameof(JobProperties))]
	public void EquivalentValuesDontTriggerPropertyChangedFromServer_JobProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<JobNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertEquivalentValuesDontTriggerPropertyChangedFromServer(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue);

	[Test]
	public void TestGetAllChildNodes()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };
		var childEntity = new Entity() { Width = 100, Height = 200 };
		var child2Entity = new Entity() { Width = 100, Height = 200 };

		parentEntity.AddChildEntity(childEntity);
		childEntity.AddChildEntity(child2Entity);
		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var childViewModel = new NodeViewModel(childEntity, networkModel);
		var child2ViewModel = new NodeViewModel(child2Entity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		networkModel.ScheduledNodes.Add(childViewModel);
		networkModel.ScheduledNodes.Add(child2ViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;
		var childNode = jobNodes[1].Instance.Node!;
		var child2Node = jobNodes[2].Instance.Node!;

		var result = parentNode.GetAllChildNodes();
		Assert.That(result.Count, Is.EqualTo(2));
		Assert.DoesNotThrow(() => result.Single(n => n == childNode));
		Assert.DoesNotThrow(() => result.Single(n => n == child2Node));

		result = childNode.GetAllChildNodes();
		Assert.That(result.Count, Is.EqualTo(1));
		Assert.DoesNotThrow(() => result.Single(n => n == child2Node));

		result = child2Node.GetAllChildNodes();
		Assert.That(result.Count, Is.EqualTo(0));
	}

	[Test, WithTransaction]
	public async Task TestChildRemovedAsync()
	{
		var networkModel = new NetworkViewModel(new DummyNetwork());
		var parentEntity = new Entity() { Width = 100, Height = 200 };
		var childEntity = new Entity() { Width = 100, Height = 200 };
		var child2Entity = new Entity() { Width = 100, Height = 200 };

		parentEntity.AddChildEntity(childEntity);
		childEntity.AddChildEntity(child2Entity);
		var parentViewModel = new NodeViewModel(parentEntity, networkModel);
		var childViewModel = new NodeViewModel(childEntity, networkModel);
		var child2ViewModel = new NodeViewModel(child2Entity, networkModel);
		var userControl = new NetworkUserControlForTest(Renderer, networkModel);
		networkModel.ScheduledNodes.Add(parentViewModel);
		networkModel.ScheduledNodes.Add(childViewModel);
		networkModel.ScheduledNodes.Add(child2ViewModel);
		using var diagramModel = new NCNDiagramModel(networkModel.ScheduledNodes, networkModel.ScheduledConnections, userControl);

		var diagram = RenderComponent<NCNDiagramForTest>(parameters =>
		{
			parameters.Add(p => p.DiagramModel, diagramModel);
		});

		var jobNodes = diagram.FindComponents<JobNode>();
		var parentNode = jobNodes[0].Instance.Node!;
		var childNode = jobNodes[1].Instance.Node!;
		var child2Node = jobNodes[2].Instance.Node!;

		var result = parentNode.GetAllChildNodes();
		Assert.That(result.Count, Is.EqualTo(2));
		Assert.DoesNotThrow(() => result.Single(n => n == childNode));
		Assert.DoesNotThrow(() => result.Single(n => n == child2Node));

		await diagramModel.DeleteNodeFromViewModelAsync(childNode);

		result = parentNode.GetAllChildNodes();
		Assert.That(result.Count, Is.EqualTo(0));
	}

	[TestCase("CC1\nCC2\nCC3")]
	[TestCase("CC1\nCC2\r\nCC3")]
	[TestCase("CC1\r\nCC2\nCC3")]
	[TestCase("CC1\r\nCC2\r\nCC3")]
	public async Task CompletionCriteria_NewLineAsync(string input)
	{
		var entity = Stub.Entity(e => e.ShapeType = WinzorShapeTypes.Shape);
		var nodeModel = new JobNodeModelBuilder().WithEntity(entity).Build();

		nodeModel.CompletionCriteria = input;
		await nodeModel.Diagram_CompletionCriteriaChangedAsync();

		var newLine = Environment.NewLine;
		var expected = $"CC1{newLine}CC2{newLine}CC3";

		Assert.That(entity.CompletionCriteria, Is.EqualTo(expected));
		Assert.That(nodeModel.CompletionCriteria, Is.EqualTo(expected));
	}
}
