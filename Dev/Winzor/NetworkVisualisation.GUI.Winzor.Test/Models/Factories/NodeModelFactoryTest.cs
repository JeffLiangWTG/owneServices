using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using Moq;
using NetworkVisualisation.GUI.Winzor.Models.Factories;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Extentions;

namespace NetworkVisualisation.GUI.Winzor.Test.Models.Factories;

public class NodeModelFactoryTest
{
	NetworkViewModel _networkViewModel = null!;
	NodeModelFactory _factory = null!;

	[SetUp]
	public void SetUp()
	{
		var mockDispatcher = new Mock<IWinzorDispatch>();
		mockDispatcher.Setup(d => d.IsWinzorThread).Returns(true);
		mockDispatcher.Setup(d => d.InvokeWinzorDispatcherAsync(It.IsAny<Action>())).Callback<Action>(a => a());

		_factory = new NodeModelFactory(mockDispatcher.Object);

		var nvmProvider = new NodeViewModelProviderForTest();
		_networkViewModel = new NetworkViewModelBuilder().WithProvider(nvmProvider).Build();
	}

	[TestCaseSource(nameof(GetNodeModelTestCases))]
	public void Test_GetNodeModel(string shapeType, Type nodeModelType)
	{
		var entity = Stub.Entity(e => e.ShapeType = shapeType);
		var nodeViewModel = _networkViewModel.CreateNodeViewModel(entity);

		Assert.That(_factory.GetNodeModel(nodeViewModel), Is.TypeOf(nodeModelType));
	}

	[TestCaseSource(nameof(GetNodeModelTestCases))]
	public async Task Test_GetNodeModelAsync(string shapeType, Type nodeModelType)
	{
		var entity = Stub.Entity(e => e.ShapeType = shapeType);
		var nodeViewModel = _networkViewModel.CreateNodeViewModel(entity);

		Assert.That(await _factory.GetNodeModelAsync(nodeViewModel), Is.TypeOf(nodeModelType));
	}

	static IEnumerable<object[]> GetNodeModelTestCases
	{
		get
		{
			yield return new object[] { WinzorShapeTypes.Annotation, typeof(AnnotationNodeModel) };
			yield return new object[] { WinzorShapeTypes.Buffer, typeof(BufferNodeModel) };
			yield return new object[] { WinzorShapeTypes.Shape, typeof(JobNodeModel) };
		}
	}
}
