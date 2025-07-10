using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class LinkableNodeServiceTest
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
	public void TestGetLinkableNodeData()
	{
		var entity = Stub.Entity(e =>
		{
			e.IsOnCriticalPath = true;
		});
		var nwvm = new NetworkViewModelBuilder().Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new LinkableNodeService(nvm, _mockDispatcher.Object);
		var data = service.GetLinkableNodeData();

		Assert.That(data.IsOnCriticalPath, Is.True);
	}
}
