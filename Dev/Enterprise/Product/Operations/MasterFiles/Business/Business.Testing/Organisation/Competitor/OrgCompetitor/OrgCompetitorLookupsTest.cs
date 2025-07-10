using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgCompetitorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganisationsLookup_Customs()
		{
			var expectedFilter = $"{OrgHeaderSchema.OH_IsBroker.Name} = 1 and {OrgHeaderSchema.OH_IsActive.Name} = 1";
			var expectedDefaults = new[] { new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True) };
			var expectedNotificationsWhenAdditionalFilterNotMet = "An Organization selected from here must have an Organization Type of Broker selected.";
			AssertOrganisationsLookup(CompetitorTypeList.Codes.Customs, expectedFilter, expectedDefaults, expectedNotificationsWhenAdditionalFilterNotMet);
		}

		public void TestOrganisationsLookup_Forwarding()
		{
			var expectedFilter = $"{OrgHeaderSchema.OH_IsForwarder.Name} = 1 and {OrgHeaderSchema.OH_IsActive.Name} = 1";
			var expectedDefaults = new[] { new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True) };
			var expectedNotificationsWhenAdditionalFilterNotMet = "An Organization selected from here must have an Organization Type of Forwarder selected.";
			AssertOrganisationsLookup(CompetitorTypeList.Codes.Forwarding, expectedFilter, expectedDefaults, expectedNotificationsWhenAdditionalFilterNotMet);
		}

		public void TestOrganisationsLookup_LandTransport()
		{
			var expectedFilter = $"{OrgHeaderSchema.OH_IsLocalTransport.Name} = 1 and {OrgHeaderSchema.OH_IsActive.Name} = 1";
			var expectedDefaults = new[] { new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.LocalTransport) };
			var expectedNotificationsWhenAdditionalFilterNotMet = "An Organization selected from here must have an Organization Type of Road Transport selected.";
			AssertOrganisationsLookup(CompetitorTypeList.Codes.LandTransport, expectedFilter, expectedDefaults, expectedNotificationsWhenAdditionalFilterNotMet);
		}

		public void TestOrganisationsLookup_Warehouse()
		{
			var expectedFilter = $"{OrgHeaderSchema.OH_IsWarehouseClient.Name} = 1 and {OrgHeaderSchema.OH_IsActive.Name} = 1";
			var expectedDefaults = new[] { new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True) };
			var expectedNotificationsWhenAdditionalFilterNotMet = "An Organization selected from here must have an Organization Type of Warehouse selected.";
			AssertOrganisationsLookup(CompetitorTypeList.Codes.Warehouse, expectedFilter, expectedDefaults, expectedNotificationsWhenAdditionalFilterNotMet);
		}

		public void TestOrganisationsLookup_UserDefinedCompetitorType()
		{
			var testCompetitorCode = "ABC";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.Add(testCompetitorCode, (NoResString)"Description for ABC", true);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var expectedFilter = $"{OrgHeaderSchema.OH_IsActive.Name} = 1";
				var expectedDefaults = Array.Empty<FilterBusinessObjectDefault>();
				var expectedNotificationsWhenAdditionalFilterNotMet = "This Organization cannot be chosen here. Please choose another Organization.";
				AssertOrganisationsLookup(testCompetitorCode, expectedFilter, expectedDefaults, expectedNotificationsWhenAdditionalFilterNotMet);
			}
		}

		public void TestOrganisationsLookup_EmptyCompetitorType()
		{
			var expectedFilter = $"{OrgHeaderSchema.OH_IsActive.Name} = 1";
			var expectedDefaults = Array.Empty<FilterBusinessObjectDefault>();
			var expectedNotificationsWhenAdditionalFilterNotMet = "This Organization cannot be chosen here. Please choose another Organization.";
			AssertOrganisationsLookup(string.Empty, expectedFilter, expectedDefaults, expectedNotificationsWhenAdditionalFilterNotMet);
		}

		void AssertOrganisationsLookup(string competitorType, string expectedFilter, FilterBusinessObjectDefault[] expectedDefaults, string expectedNotificationsWhenAdditionalFilterNotMet)
		{
			var organisationLookup = GetOrganisationLookup(competitorType);
			AssertEquals(expectedFilter, organisationLookup.CompleteFilter.LiteralTextADO);
			AssertContainsExactElementsInAnyOrder(
				d => $"FilterName: {d.FilterName}, PropertyName: {d.PropertyName}, Value: {d.Value}",
				expectedDefaults,
				organisationLookup.FilterBusinessObjectDefaults.OfType<FilterBusinessObjectDefault>()
			);
			AssertEquals(expectedNotificationsWhenAdditionalFilterNotMet, organisationLookup.GetAllNotificationsWhenAdditionalFilterNotMet(Factory.New<OrgHeader>()));
		}

		OrganisationsFindBoxCollection GetOrganisationLookup(string competitorType)
		{
			var competitor = Factory.New<OrgCompetitor>();
			competitor.OCP_Type = competitorType;
			return competitor.Lookups.Organisations;
		}
	}
}
