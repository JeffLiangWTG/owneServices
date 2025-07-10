using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Services.Connection;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class ConnectionServiceTest
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
	public void TestGetConnectionModelData()
	{
		var connectionViewModel = new ConnectionViewModel();

		using var service = new ConnectionService(connectionViewModel, _mockDispatcher.Object);
		var data = service.GetConnectionModelData();

		Assert.Multiple(() =>
		{
			Assert.That(data.IsVisible, Is.True);
			Assert.That(data.IsResourceDependency, Is.False);
			Assert.That(data.BackColor, Is.Null);
			Assert.That(data.Appearance, Is.EqualTo(ArrowAppearance.Normal));
			Assert.That(data.DisplayText, Is.Null);
			Assert.That(data.DeleteTooltip, Is.EqualTo("Remove arrow from diagram and delete any underlying relationship."));
		});
	}

	[Test]
	public void TestGetConnectorPK()
	{
		var sourcePK = Guid.NewGuid();
		var targetPK = Guid.NewGuid();

		var nwvm = new NetworkViewModelBuilder().Build();
		var sourceNode = nwvm.CreateNodeViewModel(Stub.Entity(e => e.EntityPK = sourcePK));
		var targetNode = nwvm.CreateNodeViewModel(Stub.Entity(e => e.EntityPK = targetPK));

		var connectionViewModel = CreateConnection(sourceNode, targetNode);
		using var service = new ConnectionService(connectionViewModel, _mockDispatcher.Object);

		Assert.That(service.GetSourceConnectorPK(), Is.EqualTo(sourcePK));
		Assert.That(service.GetTargetConnectorPK(), Is.EqualTo(targetPK));
	}

	static ConnectionViewModel CreateConnection(NodeViewModel sourceNode, NodeViewModel targetNode)
	{
		sourceNode.OutputConnectors.Add(new ConnectorViewModel("out"));
		targetNode.InputConnectors.Add(new ConnectorViewModel("in"));
		return new ConnectionViewModel()
		{
			SourceConnector = sourceNode.OutputConnectors[0],
			DestConnector = targetNode.InputConnectors[0],
		};
	}
}
