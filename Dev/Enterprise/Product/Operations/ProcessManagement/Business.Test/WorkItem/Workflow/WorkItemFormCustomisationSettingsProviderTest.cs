using System.Linq;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemFormCustomisationSettingsProvider))]
	class WorkItemFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<WorkItemFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				WorkItemSchema.Constants.WKI_WorkItemType,
				WorkItemSchema.Constants.WKI_WorkItemArea,
				WorkItemSchema.Constants.WKI_ActivityType,
				WorkItemSchema.Constants.WKI_ActivitySubtype,
				WorkItemSchema.Constants.WKI_Priority,
				WorkItemSchema.Constants.WKI_GE_AssignedDepartment,
				WorkItemSchema.Constants.WKI_PortOrCountry,
			};

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder("Copy-pasting production code into tests is such a fun exercise", expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(1, provider.DisplayTabs.Count);
			AssertEquals(WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName, ((FormCustomisableElement)provider.DisplayTabs.ToArray()[0]).ElementName.ToString());
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();

			AssertEquals(1, provider.TabPlacementProhibitions.Length);
			AssertEquals(WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName, provider.TabPlacementProhibitions[0].TabName);
			AssertContainsExactElementsInAnyOrder(new string[] { TabPlacement.Placements.BottomMiddle, TabPlacement.Placements.BottomLeft },
				provider.TabPlacementProhibitions[0].ProhibitedPlacements);
		}

		public void TestDisplayFields()
		{
			var provider = GetNewProvider();
			var detailsFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsPanel);
			AssertEquals("Details group fields count", 8, detailsFields.Count());
			AssertEquals("Details group fields have distinct row numbers", 8, detailsFields.Select(x => x.RowNumber).Distinct().Count());

			var statusFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == WorkItemFormCustomisationSettingsProvider.ControlNames.StatePanel);
			AssertEquals(8, statusFields.Count());
			AssertEquals("group fields have distinct row numbers", 8, statusFields.Select(x => x.RowNumber).Distinct().Count());

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = false;
			var releaseFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == WorkItemFormCustomisationSettingsProvider.ControlNames.CustomFieldsGroup);
			AssertEquals(1, releaseFields.Count());

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;
			provider = GetNewProvider();
			releaseFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == WorkItemFormCustomisationSettingsProvider.ControlNames.CustomFieldsGroup);
			AssertEquals(6, releaseFields.Count());
		}

		public override WorkItemFormCustomisationSettingsProvider GetNewProvider()
		{
			return new WorkItemFormCustomisationSettingsProvider();
		}
	}
}
