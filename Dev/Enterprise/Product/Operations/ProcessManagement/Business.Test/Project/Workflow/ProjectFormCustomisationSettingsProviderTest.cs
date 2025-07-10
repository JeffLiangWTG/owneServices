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
	[TestedType(typeof(ProjectFormCustomisationSettingsProvider))]
	public class ProjectFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<ProjectFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				WorkProjectSchema.Constants.WKP_Type,
				WorkProjectSchema.Constants.WKP_SubType,
				WorkProjectSchema.Constants.WKP_Module,
				WorkProjectSchema.Constants.WKP_Priority,
			};

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(2, provider.DisplayTabs.Count);
			AssertEquals(ProjectFormCustomisationSettingsProvider.ControlNames.DetailsTabName, ((FormCustomisableElement)provider.DisplayTabs.ToArray()[0]).ElementName.ToString());
			AssertEquals(ProjectFormCustomisationSettingsProvider.ControlNames.AdditionalDetailsTabName, ((FormCustomisableElement)provider.DisplayTabs.ToArray()[1]).ElementName.ToString());
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();

			AssertEquals(2, provider.TabPlacementProhibitions.Length);
			AssertEquals(ProjectFormCustomisationSettingsProvider.ControlNames.DetailsTabName, provider.TabPlacementProhibitions[0].TabName);
			AssertEquals(ProjectFormCustomisationSettingsProvider.ControlNames.AdditionalDetailsTabName, provider.TabPlacementProhibitions[1].TabName);

			AssertContainsExactElementsInAnyOrder(new string[] { TabPlacement.Placements.BottomMiddle },
				provider.TabPlacementProhibitions[0].ProhibitedPlacements);
			AssertContainsExactElementsInAnyOrder(new string[] { TabPlacement.Placements.TopMiddle, TabPlacement.Placements.BottomMiddle },
				provider.TabPlacementProhibitions[1].ProhibitedPlacements);
		}

		public virtual void TestDisplayFields()
		{
			var provider = GetNewProvider();
			var statusFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == ProjectFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == ProjectFormCustomisationSettingsProvider.ControlNames.StatePanel);
			AssertEquals("Status group fields count", 8, statusFields.Count());
			AssertEquals("Status group fields have distinct row numbers", 8, statusFields.Select(x => x.RowNumber).Distinct().Count());

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = false;
			var releaseFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == ProjectFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == ProjectFormCustomisationSettingsProvider.ControlNames.CustomFieldsGroup);
			AssertEquals(0, releaseFields.Count());

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;
			provider = GetNewProvider();
			releaseFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkItemFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == ProjectFormCustomisationSettingsProvider.ControlNames.CustomFieldsGroup);
			AssertEquals(5, releaseFields.Count());
		}

		public override ProjectFormCustomisationSettingsProvider GetNewProvider()
		{
			return new ProjectFormCustomisationSettingsProvider();
		}
	}
}
