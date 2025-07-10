using System.ComponentModel;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding.Builders;

namespace NetworkVisualisation.GUI.Winzor.Test.Models;

static class ModelAssert
{
	static (EntityForTest entity, Mock<TViewModel> nodeViewModel, TModel nodeModel) SetupModel<TModel, TViewModel>(Func<TViewModel, TModel> createNodeModel)
		where TModel : NetworkNodeModel
		where TViewModel : NodeViewModel
	{
		var entity = new EntityForTest();
		var diagramEntityMock = new Mock<EntityForTest>() { CallBase = true };
		diagramEntityMock.SetupGet(x => x.DiagramChannels)
			.Returns(() => entity.DiagramChannels);
		var network = new NetworkBuilder().WithEntities(entity).WithDiagramEntity(diagramEntityMock.Object).Build();

		var provider = new Mock<NodeViewModelProvider>();
		provider
			.Setup(x => x.Create(It.IsAny<INetworkEntity>(), It.IsAny<NetworkViewModel>()))
			.Returns((INetworkEntity entity, NetworkViewModel networkViewModel) => new Mock<TViewModel>(entity, networkViewModel) { CallBase = true }.Object);

		var networkViewModel = new NetworkViewModelBuilder().WithNetwork(network).WithProvider(provider.Object).Build();
		var nodeViewModel = (TViewModel)networkViewModel.GetNodeForEntity(entity);
		var nodeModel = createNodeModel(nodeViewModel);
		return (entity, Mock.Get(nodeViewModel), nodeModel);
	}

	public static void AssertOnlyTheChangedIsUpdatedOnPropertyChanged<TModel, TViewModel, TValue>(
		Func<TViewModel, TModel> createNodeModel,
		string name,
		Func<TValue> expectationFactory,
		Func<TModel, TValue> getModelValue,
		Action<Mock<TViewModel>, EntityForTest, TValue> setViewModelValue,
		int expectedCallsToChanged
	)
		where TModel : NetworkNodeModel
		where TViewModel : NodeViewModel
	{
		var (entity, viewModel, model) = SetupModel(createNodeModel);
		var calls = 0;
		model.Changed += _ => ++calls;

		var expected = expectationFactory();
		setViewModelValue(viewModel, entity, expected);
		entity.TriggerPropertyChanged(name);

		var result = getModelValue(model);
		Assert.That(Properties.Equivalent(expected, result), Is.True,
			$"Expected: {expected}\nBut was: {result}");
		Assert.That(calls, Is.EqualTo(expectedCallsToChanged));
	}

	public static void AssertEquivalentValuesAreIgnoredOnPropertyChanged<TModel, TViewModel, TValue>(
		Func<TViewModel, TModel> createNodeModel,
		string name,
		Func<TValue> expectationFactory,
		Func<TModel, TValue> getModelValue,
		Action<Mock<TViewModel>, EntityForTest, TValue> setViewModelValue,
		int expectedCallsToChanged
	)
		where TModel : NetworkNodeModel
		where TViewModel : NodeViewModel
	{
		var (entity, viewModel, model) = SetupModel(createNodeModel);
		var calls = 0;
		model.Changed += _ => ++calls;
		var expected = getModelValue(model);

		// test that initial values, including nulls are correctly compared
		void FailBeforeChange(Model mo)
			=> Assert.Fail($"A change event was incorrectly raised before changes were made.");

		model.Changed += FailBeforeChange;

		entity.TriggerPropertyChanged(name);

		var result = getModelValue(model);

		Assert.That(result, Is.EqualTo(expected));
		Assert.That(calls, Is.Zero);

		// Setup real values to be ignored
		model.Changed -= FailBeforeChange;
		setViewModelValue(viewModel, entity, expectationFactory());
		entity.TriggerPropertyChanged(name);

		Assert.That(calls, Is.EqualTo(expectedCallsToChanged));

		expected = getModelValue(model);

		// test that real values are also correctly compared
		model.Changed += _ =>
			Assert.Fail($"A change event was incorrectly raised after reassigning the same value.");

		setViewModelValue(viewModel, entity, expectationFactory());
		entity.TriggerPropertyChanged(name);

		result = getModelValue(model);

		Assert.That(calls, Is.EqualTo(expectedCallsToChanged));
		Assert.That(result, Is.EqualTo(expected));
	}

	public static void AssertPropertyChangedFromServerIsTriggeredOnPropertyChanged<TModel, TViewModel, TValue>(
		Func<TViewModel, TModel> createNodeModel,
		string name,
		Func<TValue> expectationFactory,
		Func<TModel, TValue> getModelValue,
		Action<Mock<TViewModel>, EntityForTest, TValue> setViewModelValue
	)
		where TModel : NetworkNodeModel
		where TViewModel : NodeViewModel
	{
		var (entity, viewModel, model) = SetupModel(createNodeModel);
		var calls = 0;
		model.PropertyChangedFromServer += (_, _) => ++calls;

		var expected = expectationFactory();
		setViewModelValue(viewModel, entity, expected);
		entity.TriggerPropertyChanged(name);

		var result = getModelValue(model);
		Assert.That(Properties.Equivalent(expected, result), Is.True,
			$"Expected: {expected}\nBut was: {result}");
		Assert.That(calls, Is.GreaterThan(0));
	}

	public static void AssertEquivalentValuesDontTriggerPropertyChangedFromServer<TModel, TViewModel, TValue>(
		Func<TViewModel, TModel> createNodeModel,
		string name,
		Func<TValue> expectationFactory,
		Func<TModel, TValue> getModelValue,
		Action<Mock<TViewModel>, EntityForTest, TValue> setViewModelValue
	)
		where TModel : NetworkNodeModel
		where TViewModel : NodeViewModel
	{
		var (entity, viewModel, model) = SetupModel(createNodeModel);
		var calls = 0;
		model.PropertyChangedFromServer += (_, _) => ++calls;
		var expected = getModelValue(model);

		// test that initial values, including nulls are correctly compared
		void FailBeforePropertyChangedFromServer(object? sender, PropertyChangedEventArgs e)
			=> Assert.Fail($"A PropertyChangedFromServer event was incorrectly raised before changes were made.");

		model.PropertyChangedFromServer += FailBeforePropertyChangedFromServer;

		entity.TriggerPropertyChanged(name);

		var result = getModelValue(model);

		Assert.That(result, Is.EqualTo(expected));
		Assert.That(calls, Is.Zero);

		// Setup real values to be ignored
		model.PropertyChangedFromServer -= FailBeforePropertyChangedFromServer;
		setViewModelValue(viewModel, entity, expectationFactory());
		entity.TriggerPropertyChanged(name);

		Assert.That(calls, Is.GreaterThan(0));
		var originalCalls = calls;

		expected = getModelValue(model);

		// test that real values are also correctly compared
		model.PropertyChangedFromServer += (_, _) =>
			Assert.Fail($"A PropertyChangedFromServer event was incorrectly raised after reassigning the same value.");

		setViewModelValue(viewModel, entity, expectationFactory());
		entity.TriggerPropertyChanged(name);

		result = getModelValue(model);

		Assert.That(calls, Is.EqualTo(originalCalls));
		Assert.That(result, Is.EqualTo(expected));
	}
}
