using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FormCustomisationSettingsProviderTest : TestCaseWithFactory
	{
		public void TestUseCaching()
		{
			FormCustomisationSettingsProviderForTest providerForTest = new FormCustomisationSettingsProviderForTest();
			providerForTest.GetPropertiesThatAffectWorkflowTemplateImplementation = () => new[] { "P0_SubType1" };
			AssertEquals(false, providerForTest.UseCaching);

			providerForTest = new FormCustomisationSettingsProviderForTest();

			AssertEquals(true, providerForTest.UseCaching);

			providerForTest.GetDisplayTabsImplementation = () =>
			{
				FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();
				tabs.Add((NoResString)"tab desc", "tab name");
				return tabs;
			};

			providerForTest.GetDisplayFieldsImplementation = () =>
			{
				FormCustomisableElementCollection fields = new FormCustomisableElementCollection();
				fields.Add((NoResString)"field desc", "field name");
				return fields;
			};

			AssertEquals(1, providerForTest.DisplayTabs.Count);
			AssertEquals(1, providerForTest.DisplayFields.Count);

			AssertEquals(1, providerForTest.DisplayTabs.Count);
			AssertEquals(1, providerForTest.DisplayFields.Count);

			providerForTest.GetDisplayTabsImplementation = providerForTest.GetDisplayFieldsImplementation = null;

			AssertEquals(1, providerForTest.DisplayTabs.Count);
			AssertEquals(1, providerForTest.DisplayFields.Count);
		}
	}
}
