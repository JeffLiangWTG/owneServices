using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestFormCustomisationSettingsProvider))]
	class WorkRequestFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<WorkRequestFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				WorkRequestSchema.Constants.WKR_SelectionCriteria1,
				WorkRequestSchema.Constants.WKR_SelectionCriteria2,
				WorkRequestSchema.Constants.WKR_SelectionCriteria3,
				WorkRequestSchema.Constants.WKR_SelectionCriteria4,
				WorkRequestSchema.Constants.WKR_SelectionCriteria5,
				WorkRequestSchema.Constants.WKR_GE_Department,
				WorkRequestSchema.Constants.WKR_GB_Branch,
				WorkRequestSchema.Constants.WKR_RN_NKCountry,
				WorkRequestSchema.Constants.WKR_OC_Client,
				WorkRequestSchema.Constants.WKR_RequestNumber,
			};

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(1, provider.DisplayTabs.Count);
			AssertEquals(WorkRequestFormCustomisationSettingsProvider.ControlNames.DetailsTabName, ((FormCustomisableElement)provider.DisplayTabs.ToArray()[0]).ElementName.ToString());
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();

			AssertEquals(1, provider.TabPlacementProhibitions.Length);
			AssertEquals(WorkRequestFormCustomisationSettingsProvider.ControlNames.DetailsTabName, provider.TabPlacementProhibitions[0].TabName);
			AssertContainsExactElementsInAnyOrder(new string[]
			{
				TabPlacement.Placements.BottomMiddle,
				TabPlacement.Placements.BottomLeft,
				TabPlacement.Placements.BottomRight,
				TabPlacement.Placements.TopRight
			},
			provider.TabPlacementProhibitions[0].ProhibitedPlacements);
		}

		public override WorkRequestFormCustomisationSettingsProvider GetNewProvider()
		{
			return new WorkRequestFormCustomisationSettingsProvider();
		}

		public void TestDisplayFields()
		{
			var provider = GetNewProvider();
			var detailsFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkRequestFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == WorkRequestFormCustomisationSettingsProvider.ControlNames.DetailsPanel);

			CombineAssertions(() =>
			{
				AssertEquals(1, detailsFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.SelectionCriterion1 &&
					x.Placement == TabPlacement.Placements.TopLeft));

				AssertEquals(1, detailsFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.SelectionCriterion2 &&
					x.Placement == TabPlacement.Placements.TopLeft));

				AssertEquals(1, detailsFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.SelectionCriterion3 &&
					x.Placement == TabPlacement.Placements.TopLeft));

				AssertEquals(1, detailsFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.SelectionCriterion4 &&
					x.Placement == TabPlacement.Placements.TopLeft));

				AssertEquals(1, detailsFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.SelectionCriterion5 &&
					x.Placement == TabPlacement.Placements.TopLeft));
			});

			var statusFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == WorkRequestFormCustomisationSettingsProvider.ControlNames.DetailsTabName && x.ElementGroup == WorkRequestFormCustomisationSettingsProvider.ControlNames.StatusPanel);
			CombineAssertions(() =>
			{
				AssertEquals(1, statusFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.Branch &&
					x.Placement == TabPlacement.Placements.TopMiddle));

				AssertEquals(1, statusFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.Department &&
					x.Placement == TabPlacement.Placements.TopMiddle));

				AssertEquals(1, statusFields.Count(x =>
					x.ElementName == WorkRequestFormCustomisationSettingsProvider.ControlNames.Country &&
					x.Placement == TabPlacement.Placements.TopMiddle));
			});
		}
	}
}
