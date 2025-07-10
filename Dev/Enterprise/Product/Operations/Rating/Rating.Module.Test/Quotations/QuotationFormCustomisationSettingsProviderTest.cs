using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Module.Quotations;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Test.Quotations
{
	[TestedType(typeof(QuotationFormCustomisationSettingsProvider))]
	public class QuotationFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<QuotationFormCustomisationSettingsProvider>
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new QuotationFormCustomisationSettingsProvider(null));
			AssertNoExceptionThrown(() => new QuotationFormCustomisationSettingsProvider(new QuotationWorkflowDescriptor()));
		}
		public override QuotationFormCustomisationSettingsProvider GetNewProvider()
		{
			return new QuotationFormCustomisationSettingsProvider(new QuotationWorkflowDescriptor());
		}

		public override void TestDisplayTabs()
		{
			QuotationFormCustomisationSettingsProvider provider = GetNewProvider();
			FormCustomisableElementCollection tabs = provider.DisplayTabs;
			string[] expectedTabNames = new[] { "CustomFieldsTabPage" };

			AssertEquals("Expected 1 tab", 1, tabs.Count);
			AssertContainsExactElementsInAnyOrder(expectedTabNames, new string[] { tabs[0].ElementName });
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var serviceProvider = new QuotationFormCustomisationSettingsProvider(new QuotationWorkflowDescriptor());
			AssertEquals(1, serviceProvider.PropertiesThatAffectWorkflow.Length);
		}

		public override void TestTabPlacementProhibitions()
		{
			var serviceProvider = new QuotationFormCustomisationSettingsProvider(new QuotationWorkflowDescriptor());
			AssertEquals(0, serviceProvider.TabPlacementProhibitions.Length);
		}
	}
}
