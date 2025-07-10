using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Services.BufferNode;
using CargoWise.PAVE.Common.Interfaces;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class BufferNodeServiceTest
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
	public void TestGetBufferNodeData()
	{
		var entity = Stub.Entity(e =>
		{
			e.X = 100;
			e.Y = 110;
			e.Width = 200;
			e.Height = 210;
			e.ShapeType = WinzorShapeTypes.Buffer;
			e.Description = "Buffer description";
			e.AdditionalDetail = "Additional detail";
			e.Status = WorkStatus.Working;
			e.StatusDescription = "Working hard";
		});

		var nvmProvider = new NodeViewModelProviderForTest();
		var nwvm = new NetworkViewModelBuilder().WithProvider(nvmProvider).Build();
		var nvm = nwvm.CreateNodeViewModel(entity);

		using var service = new BufferNodeService(nvm, _mockDispatcher.Object);
		var data = service.GetBufferNodeModelData();

		Assert.Multiple(() =>
		{
			Assert.That(data.Description, Is.EqualTo("Buffer description"));
			Assert.That(data.AdditionalDetail, Is.EqualTo("Additional detail"));
			Assert.That(data.Status, Is.EqualTo(WorkStatus.Working));
			Assert.That(data.StatusTooltip, Is.EqualTo("Working hard"));
			Assert.That(data.PenetrationPercentLabel, Is.Empty);
		});
	}
}
