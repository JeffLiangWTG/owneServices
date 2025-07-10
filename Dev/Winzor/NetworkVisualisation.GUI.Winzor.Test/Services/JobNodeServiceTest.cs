using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.GUI.Services.JobNode;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class JobNodeServiceTest
{
	Mock<IWinzorDispatch> _mockDispatcher = null!;

	[SetUp]
	public void SetUp()
	{
		_mockDispatcher = new Mock<IWinzorDispatch>();
		_mockDispatcher.Setup(d => d.IsWinzorThread).Returns(true);
		_mockDispatcher.Setup(d => d.InvokeWinzorDispatcherAsync(It.IsAny<Action>())).Callback<Action>(a => a());
	}

	[Test]
	public void TestGetJobNodeData()
	{
		var affinity = new Mock<IAffinity>();
		affinity.Setup(a => a.Name).Returns("Affinity Name");

		var link = new Mock<IEntityRelationship>();

		var child1 = Stub.Entity(e => e.EntityPK = Guid.NewGuid());
		var child2 = Stub.Entity(e => e.EntityPK = Guid.NewGuid());

		var entity = Stub.Entity(e =>
		{
			e.JobNumber = "AB1234";
			e.JobName = "Job Name";
			e.AppliedAffinities.Add(affinity.Object);
			e.ExplicitDurationMinutes = 20;
			e.CompletionCriteria = "CC1";
			e.Description = "Job description";
			e.ScheduledStartTimeLocal = new DateTime(2024, 6, 1);
			e.ScheduledEndTimeLocal = new DateTime(2024, 7, 15);
			e.RemainingDurationMinutes = 5;
			e.Status = WorkStatus.Working;
			e.StatusDescription = "Working hard";
			e.HasLinkedEntity = true;
			e.PostRequisiteLinks = new[] { link.Object };
			e.Children = new[] { child1, child2 };
		});

		var diagramEntity = Stub.Entity(e =>
		{
			e.IsDiagramScaled = true;
		});

		var nw = new NetworkBuilder()
			.WithEntities(child1, child2)
			.WithDiagramEntity(diagramEntity)
			.Build();

		var nwvm = new NetworkViewModelBuilder()
			.WithNetwork(nw)
			.Build();

		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new JobNodeService(nvm, _mockDispatcher.Object);
		var data = service.GetJobNodeData();

		Assert.Multiple(() =>
		{
			Assert.That(data.JobNumber, Is.EqualTo("AB1234"));
			Assert.That(data.JobNumberReadableText, Is.EqualTo("AB1234"));
			Assert.That(data.JobName, Is.EqualTo("Job Name"));
			Assert.That(data.AppliedAttributesReadableText, Is.EqualTo("Affinity Name"));
			Assert.That(data.DurationReadableText, Is.EqualTo("0:20"));
			Assert.That(data.CompletionCriteria, Is.EqualTo("CC1"));
			Assert.That(data.CompletionCriteriaPlaceholder, Is.EqualTo("Completion Criteria"));
			Assert.That(data.Description, Is.EqualTo("Job description"));
			Assert.That(data.StartDateReadableText, Is.EqualTo("01 Jun 2024 00:00"));
			Assert.That(data.RemainingDurationReadableText, Is.EqualTo("0:05"));
			Assert.That(data.FinishDateReadableText, Is.EqualTo("15 Jul 2024 00:00"));
			Assert.That(data.DurationTooltip, Is.EqualTo("Planned Duration"));
			Assert.That(data.StartDateToolTip, Is.EqualTo("Scheduled Start"));
			Assert.That(data.RemainingDurationToolTip, Is.EqualTo("Remaining Duration"));
			Assert.That(data.FinishDateToolTip, Is.EqualTo("Scheduled Finish"));
			Assert.That(data.StatusTooltip, Is.EqualTo("Working hard"));
			Assert.That(data.JobNumberTooltip, Is.EqualTo("Job number"));
			Assert.That(data.AppliedAttributesTooltip, Is.EqualTo("Applied affinities"));
			Assert.That(data.ShowJobBar, Is.True);
			Assert.That(data.ShowScheduleDetails, Is.True);
			Assert.That(data.ShowJobBarAffinities, Is.True);
			Assert.That(data.HasLinkedEntity, Is.True);
			Assert.That(data.HasPostReqLinks, Is.True);
			Assert.That(data.CompletionCriteriaTextColor, Is.EqualTo(System.Drawing.Color.Black));
			Assert.That(data.Status, Is.EqualTo(WorkStatus.Working));
			Assert.That(data.ChildNodesPK, Has.Exactly(2).Items);
		});
	}

	[Test]
	public async Task TestUpdateJobNameAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new JobNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.UpdateJobNameAsync("My job");

		Assert.Multiple(() =>
		{
			Assert.That(nvm.JobName, Is.EqualTo("My job"));

			Assert.That(events, Has.Count.EqualTo(5));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(JobNodeData.JobName)));
			Assert.That(events[0].Value, Is.EqualTo("My job"));

			Assert.That(events[1].PropertyName, Is.EqualTo(nameof(JobNodeData.HasLinkedEntity)));
			Assert.That(events[1].Value, Is.False);

			Assert.That(events[2].PropertyName, Is.EqualTo(nameof(JobNodeData.ShowJobBar)));
			Assert.That(events[2].Value, Is.False);

			Assert.That(events[3].PropertyName, Is.EqualTo(nameof(JobNodeData.ShowJobBarAffinities)));
			Assert.That(events[3].Value, Is.False);

			Assert.That(events[4].PropertyName, Is.EqualTo(nameof(NetworkNodeData.DeleteTooltip)));
			Assert.That(events[4].Value, Is.EqualTo("Remove shape from diagram."));
		});
	}

	[Test]
	public async Task TestUpdateCompletionCriteriaAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new JobNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.UpdateCompletionCriteriaAsync("My completion criteria");

		Assert.Multiple(() =>
		{
			Assert.That(nvm.CompletionCriteria, Is.EqualTo("My completion criteria"));

			Assert.That(events, Has.Count.EqualTo(1));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(JobNodeData.CompletionCriteria)));
			Assert.That(events[0].Value, Is.EqualTo("My completion criteria"));
		});
	}
}
