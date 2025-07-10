using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.AnnotationNode;
using Moq;
using NetworkVisualisation.GUI.Winzor.Test.Models;
using NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

namespace NetworkVisualisation.GUI.Winzor.Test.Services;

public class AnnotationNodeServiceTest : NetworkNodeModelTest<AnnotationNodeModel>
{
	static IEnumerable<TestCaseData> AnnotationProperties
	{
		get
		{
			yield return PropertyTestCase(nameof(NodeViewModel.X), () => 100d, m => m.Position.X, (_, e, x) => e.X = x, 2);
			yield return PropertyTestCase(nameof(NodeViewModel.Y), () => 100d, m => m.Position.Y, (_, e, x) => e.Y = x, 2);
			yield return PropertyTestCase(nameof(NodeViewModel.Width), () => 100d, m => m.Size!.Width,
				(_, e, x) => e.Width = x);
			yield return PropertyTestCase(nameof(NodeViewModel.Height), () => 100d, m => m.Size!.Height,
				(_, e, x) => e.Height = x);
		}
	}

	[TestCaseSource(nameof(AnnotationProperties))]
	public void OnlyTheChangedIsUpdatedOnPropertyChanged_AnnotationProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<AnnotationNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertOnlyTheChangedIsUpdatedOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue, expectedCallsToChanged);

	[TestCaseSource(nameof(AnnotationProperties))]
	public void EquivalentValuesAreIgnoredOnPropertyChanged_AnnotationProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<AnnotationNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertEquivalentValuesAreIgnoredOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue, expectedCallsToChanged);

	[TestCaseSource(nameof(AnnotationProperties))]
	public void PropertyChangedFromServerIsTriggeredOnPropertyChanged_AnnotationProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<AnnotationNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertPropertyChangedFromServerIsTriggeredOnPropertyChanged(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue);

	[TestCaseSource(nameof(AnnotationProperties))]
	public void EquivalentValuesDontTriggerPropertyChangedFromServer_AnnotationProperties<TValue>(string name, Func<TValue> expectationFactory,
		Func<AnnotationNodeModel, TValue> getModelValue, Action<Mock<NodeViewModel>, EntityForTest, TValue> setViewModelValue, int expectedCallsToChanged)
		=> ModelAssert.AssertEquivalentValuesDontTriggerPropertyChangedFromServer(CreateNodeModel, name, expectationFactory, getModelValue, setViewModelValue);

#pragma warning disable CA2000 // Dispose objects before losing scope
	protected override AnnotationNodeModel CreateNodeModel(NodeViewModel viewModel)
		=> new(new AnnotationNodeService(viewModel, new NetworkUserControlForTest(Renderer)));
#pragma warning restore CA2000 // Dispose objects before losing scope
}
