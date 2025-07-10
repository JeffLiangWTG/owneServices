using System.Drawing;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class NetworkNodeServiceTest
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
	public void TestGetNetworkNodeData()
	{
		var entity = Stub.Entity(e =>
		{
			e.X = 100;
			e.Y = 110;
			e.Width = 200;
			e.Height = 210;
			e.Notes = "Notes";
			e.CanUnlinkEntity = true;
			e.Name = "Diagram Name";
			e.EntityState = EntityState.Fixed;
			e.ZIndex = 5;
		});
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		nvm.LowerBar = new NodeViewModel.ProgressBar(Color.Yellow, 0.75);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);
		var data = service.GetNetworkNodeData();

		Assert.Multiple(() =>
		{
			Assert.That(data.X, Is.EqualTo(100));
			Assert.That(data.Y, Is.EqualTo(110));
			Assert.That(data.Width, Is.EqualTo(200));
			Assert.That(data.Height, Is.EqualTo(210));
			Assert.That(data.HasNotifications, Is.False);
			Assert.That(data.HasNotificationsForState, Is.False);
			Assert.That(data.Notes, Is.EqualTo("Notes"));
			Assert.That(data.NotesPlaceholder, Is.EqualTo("Notes"));
			Assert.That(data.StatusColors.ToStyle(), Is.EqualTo("background-image: linear-gradient(180deg, #FFFFFFFF 0%, #F5F5DCFF 60%);"));
			Assert.That(data.DeleteTooltip, Is.EqualTo("Remove shape from diagram and delete underlying entity."));
			Assert.That(data.DiagramName, Is.EqualTo("Diagram Name"));
			Assert.That(data.EntityPK.ToString(), Is.Not.Empty);
			Assert.That(data.EntityState, Is.EqualTo(EntityState.Fixed));
			Assert.That(data.EntityStateTooltip, Is.EqualTo("This shape's position and size are fixed"));
			Assert.That(data.ToolTip, Is.EqualTo("Diagram Name"));
			Assert.That(data.ZIndex, Is.EqualTo(5));
			Assert.That(data.IsSelected, Is.EqualTo(false));
			Assert.That(data.MinimunWidth, Is.EqualTo(100d));
			Assert.That(data.MinimumHeight, Is.EqualTo(50d));
			Assert.That(data.StatusTextWeight, Is.EqualTo(NodeFontWeight.Black));
			Assert.That(data.ForegroundColor, Is.EqualTo(Color.Black));
			Assert.That(data.IsLocked, Is.True);
			Assert.That(data.HasProgressBar, Is.True);
		});
	}

	[Test]
	public void TestGetTooltipForEntityState()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		Assert.Multiple(() =>
		{
			Assert.That(service.GetTooltipForEntityState(EntityState.Approved), Is.EqualTo("This shape has been approved"));
			Assert.That(service.GetTooltipForEntityState(EntityState.NotApproved), Is.EqualTo("This shape has not been approved"));
			Assert.That(service.GetTooltipForEntityState(EntityState.Fixed), Is.EqualTo("This shape's position and size are fixed"));
		});
	}

	[Test]
	public async Task TestUpdateDiagramNameAsync()
	{
		var entity = Stub.Entity(e => e.Name = "Diagram Name");
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.UpdateDiagramNameAsync("New Name");

		Assert.Multiple(() =>
		{
			Assert.That(nvm.DiagramName, Is.EqualTo("New Name"));
			Assert.That(nvm.ToolTip, Is.EqualTo("New Name"));

			Assert.That(events, Has.Count.EqualTo(2));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.ToolTip)));
			Assert.That(events[0].Value, Is.EqualTo(nvm.ToolTip));

			Assert.That(events[1].PropertyName, Is.EqualTo(nameof(NetworkNodeData.DiagramName)));
			Assert.That(events[1].Value, Is.EqualTo(nvm.DiagramName));
		});
	}

	[Test]
	public async Task TestUpdateIsSelectedAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.UpdateIsSelectedAsync(true);

		Assert.Multiple(() =>
		{
			Assert.That(nvm.IsSelected, Is.True);

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.IsSelected)));
			Assert.That(events[0].Value, Is.True);
		});
	}

	[Test]
	public async Task TestUpdateNotesAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.UpdateNotesAsync("My notes");

		Assert.Multiple(() =>
		{
			Assert.That(nvm.Notes, Is.EqualTo("My notes"));

			Assert.That(events, Has.Count.EqualTo(1));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.Notes)));
			Assert.That(events[0].Value, Is.EqualTo(nvm.Notes));
		});
	}

	[Test]
	public async Task TestResizeAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.ResizeAsync(x: 10, y: 20, width: 300, height: 150);

		Assert.Multiple(() =>
		{
			Assert.That(nvm.X, Is.EqualTo(10));
			Assert.That(nvm.Y, Is.EqualTo(20));
			Assert.That(nvm.Width, Is.EqualTo(300));
			Assert.That(nvm.Height, Is.EqualTo(150));

			Assert.That(events, Has.Count.EqualTo(8));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.X)));
			Assert.That(events[0].Value, Is.EqualTo(nvm.X));

			Assert.That(events[1].PropertyName, Is.EqualTo(nameof(NetworkNodeData.Y)));
			Assert.That(events[1].Value, Is.EqualTo(nvm.Y));

			Assert.That(events[2].PropertyName, Is.EqualTo(nameof(NetworkNodeData.StatusColors)));

			Assert.That(events[3].PropertyName, Is.EqualTo(nameof(NetworkNodeData.Width)));
			Assert.That(events[3].Value, Is.EqualTo(nvm.Width));

			Assert.That(events[4].PropertyName, Is.EqualTo(nameof(NetworkNodeData.Height)));
			Assert.That(events[4].Value, Is.EqualTo(nvm.Height));
		});
	}

	[Test]
	public async Task TestResizeAsync_WhenSizeSmallerThanMinimumAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		await service.ResizeAsync(x: 10, y: 20, width: 10, height: 5);

		Assert.Multiple(() =>
		{
			Assert.That(nvm.X, Is.EqualTo(10));
			Assert.That(nvm.Y, Is.EqualTo(20));
			Assert.That(nvm.Width, Is.EqualTo(100));
			Assert.That(nvm.Height, Is.EqualTo(50));
		});
	}

	[Test]
	public async Task TestResizeAsync_WhenPositionSmallerThanMinimumAsync()
	{
		var entity = Stub.Entity();
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		await service.ResizeAsync(x: -10, y: -5, width: 180, height: 80);

		Assert.Multiple(() =>
		{
			Assert.That(nvm.X, Is.EqualTo(0));
			Assert.That(nvm.Y, Is.EqualTo(0));
			Assert.That(nvm.Width, Is.EqualTo(180));
			Assert.That(nvm.Height, Is.EqualTo(80));
		});
	}

	[Test]
	public async Task TestMovedAsync()
	{
		var entity = Stub.Entity(e =>
		{
			e.Width = 200;
			e.Height = 100;
		});
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.MoveAsync(x: 15, y: 25);

		Assert.Multiple(() =>
		{
			Assert.That(nvm.X, Is.EqualTo(15));
			Assert.That(nvm.Y, Is.EqualTo(25));
			Assert.That(nvm.Width, Is.EqualTo(200));
			Assert.That(nvm.Height, Is.EqualTo(100));

			Assert.That(events, Has.Count.EqualTo(6));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.X)));
			Assert.That(events[0].Value, Is.EqualTo(nvm.X));

			Assert.That(events[1].PropertyName, Is.EqualTo(nameof(NetworkNodeData.Y)));
			Assert.That(events[1].Value, Is.EqualTo(nvm.Y));

			Assert.That(events[2].PropertyName, Is.EqualTo(nameof(NetworkNodeData.StatusColors)));
		});
	}

	[Test]
	public async Task TestMovedAsync_WhenPositionSmallerThanMinimumAsync()
	{
		var entity = Stub.Entity(e =>
		{
			e.Width = 200;
			e.Height = 100;
		});
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		await service.MoveAsync(x: -15, y: -25);

		Assert.Multiple(() =>
		{
			Assert.That(nvm.X, Is.EqualTo(0));
			Assert.That(nvm.Y, Is.EqualTo(0));
			Assert.That(nvm.Width, Is.EqualTo(200));
			Assert.That(nvm.Height, Is.EqualTo(100));
		});
	}

	[Test]
	public void TestGetProgressBarModelData()
	{
		var entity = Stub.Entity(e => e.Name = "Diagram Name");
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var nodeService = new NetworkNodeService(nvm, _mockDispatcher.Object);
		var service = nodeService.GetProgressBarService();

		Assert.That(service, Is.Not.Null);
		Assert.That(service.GetProgressBarModelData(), Is.Not.Null);
	}

	[TestCase(EntityState.HasWarnings, true)]
	[TestCase(EntityState.HasMessages, true)]
	[TestCase(EntityState.HasErrors, true)]
	[TestCase(EntityState.NotApproved, false)]
	[TestCase(EntityState.Approved, false)]
	public void TestHasNotificationsForState_WhenEntityStateChanges(EntityState entityState, bool expectedValue)
	{
		var entity = Stub.Entity(e => e.EntityState = EntityState.Fixed);
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		entity.EntityState = entityState;
		entity.TriggerPropertyChanged(nameof(NetworkNodeData.EntityState));

		Assert.Multiple(() =>
		{
			Assert.That(events, Has.Count.EqualTo(4));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.EntityState)));
			Assert.That(events[0].Value, Is.EqualTo(entityState));

			Assert.That(events[1].PropertyName, Is.EqualTo(nameof(NetworkNodeData.EntityStateTooltip)));

			Assert.That(events[2].PropertyName, Is.EqualTo(nameof(NetworkNodeData.HasNotificationsForState)));
			Assert.That(events[2].Value, Is.EqualTo(expectedValue));

			Assert.That(events[3].PropertyName, Is.EqualTo(nameof(NetworkNodeData.IsLocked)));
			Assert.That(events[3].Value, Is.False);
		});
	}

	[TestCase(EntityState.Fixed, true)]
	[TestCase(EntityState.Approved, false)]
	[TestCase(EntityState.NotApproved, false)]
	[TestCase(EntityState.HasWarnings, false)]
	[TestCase(EntityState.HasMessages, false)]
	[TestCase(EntityState.HasErrors, false)]
	public void TestIsLocked_WhenEntityStateChanges(EntityState entityState, bool expectedValue)
	{
		var entity = Stub.Entity(e => e.EntityState = EntityState.Fixed);
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new NetworkNodeService(nvm, _mockDispatcher.Object);

		var events = new List<PropertyValueChangedEventArgs>();
		service.PropertyValueChanged += (s, e) => events.Add(e);

		entity.EntityState = entityState;
		entity.TriggerPropertyChanged(nameof(NetworkNodeData.EntityState));

		Assert.Multiple(() =>
		{
			Assert.That(events, Has.Count.EqualTo(4));

			Assert.That(events[0].PropertyName, Is.EqualTo(nameof(NetworkNodeData.EntityState)));
			Assert.That(events[0].Value, Is.EqualTo(entityState));

			Assert.That(events[1].PropertyName, Is.EqualTo(nameof(NetworkNodeData.EntityStateTooltip)));

			Assert.That(events[2].PropertyName, Is.EqualTo(nameof(NetworkNodeData.HasNotificationsForState)));

			Assert.That(events[3].PropertyName, Is.EqualTo(nameof(NetworkNodeData.IsLocked)));
			Assert.That(events[3].Value, Is.EqualTo(expectedValue));
		});
	}
}
