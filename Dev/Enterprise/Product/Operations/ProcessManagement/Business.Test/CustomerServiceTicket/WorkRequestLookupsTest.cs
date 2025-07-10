using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business.Test
{
	class WorkRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganisations()
		{
			var contact1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, organisationName: "Organisation 1 for TestOrganisations", contactName: "Contact 1");
			var contact2 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, organisationName: "Organisation 2 for TestOrganisations", contactName: "Contact 2");

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			AssertType<OrgHeaderCollection>(workRequest.Lookups.Organisations);

			var actualOrganisations = workRequest.Lookups.Organisations;
			actualOrganisations.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Contains, "TestOrganisations"));

			CombineAssertions("Organisations should find all organisations i.e. no filter", () =>
			{
				AssertNullOrEmpty("filter", actualOrganisations.CompleteFilter.LiteralTextADO);
				AssertEquals("count", 2, actualOrganisations.Count);
			});
		}

		public void TestClientsFilteredByOrganisation()
		{
			var contact1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, organisationName: "Organisation 1", contactName: "Contact 1 for TestClientsFilteredByOrganisation");
			var contact2 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, organisationName: "Organisation 2", contactName: "Contact 2 for TestClientsFilteredByOrganisation");

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			var org1 = contact1.ParentOrg;

			workRequest.OrganisationPK = org1.PK;
			var clients = workRequest.Lookups.ClientsFilteredByOrganisation;
			clients.Load();

			CombineAssertions("WHEN setting OrganisationPK field, THEN ClientsFilteredByOrganisation SHOULD be filter by OrganisationPK", () =>
			{
				AssertEquals(
					"filter",
					$"OC_OH = CONVERT('{org1.PK}', 'System.Guid')",
					workRequest.Lookups.ClientsFilteredByOrganisation.CompleteFilter.LiteralTextADO);
				AssertEquals("count", 1, clients.Count);
			});

			workRequest.OrganisationPK = ZGuid.Empty;
			clients = workRequest.Lookups.ClientsFilteredByOrganisation;
			clients.LoadWithMoreFiltering(new ZQuery(OrgContactSchema.OC_ContactName, SQLComparisonOperator.Contains, "TestClientsFilteredByOrganisation"));

			CombineAssertions("WHEN emptying OrganisationPK field, THEN ClientsFilteredByOrganisation filter SHOULD be cleared", () =>
			{
				AssertNullOrEmpty(
					"filter",
					workRequest.Lookups.ClientsFilteredByOrganisation.CompleteFilter.LiteralTextADO);
				AssertEquals("count", 2, clients.Count);
			});
		}

		public void TestSelectionCriteriaLookupLists_WhenRegistryItemsNotSet_ShouldUseEmptyLists()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertLookupListValues(ticket.WKR_SelectionCriteria1Info);
			AssertLookupListValues(ticket.WKR_SelectionCriteria2Info);
			AssertLookupListValues(ticket.WKR_SelectionCriteria3Info);
			AssertLookupListValues(ticket.WKR_SelectionCriteria4Info);
			AssertLookupListValues(ticket.WKR_SelectionCriteria5Info);
		}

		public void TestSelectionCriteriaLookupLists_ShouldUseRegistryConfig()
		{
			ProcessMgmtTestHelper.SetDummySelectionCriteriaValues();

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertLookupListValues(ticket.WKR_SelectionCriteria1Info, Tuple.Create("ENT", "CargoWise Two"), Tuple.Create("GLW", "GLOW"));
			AssertLookupListValues(ticket.WKR_SelectionCriteria2Info, Tuple.Create("PAV", "Productivity Acceleration and Visualisation Engine"), Tuple.Create("PER", "Performance and Deployment"));
			AssertLookupListValues(ticket.WKR_SelectionCriteria3Info, Tuple.Create("BUF", "Buffer Management"), Tuple.Create("APP", "Application Deployment"));
			AssertLookupListValues(ticket.WKR_SelectionCriteria4Info, Tuple.Create("PRD", "Product Enhancement"), Tuple.Create("FIX", "Defect Fix"));
			AssertLookupListValues(ticket.WKR_SelectionCriteria5Info, Tuple.Create("ALP", "Alpha"), Tuple.Create("GPR", "GPR or is GP1?? There's no way of knowing."));
		}

		static void AssertLookupListValues(ZPropertyInfo propertyInfo, params Tuple<string, string>[] expectedListItems)
		{
			var list = (CodeDescriptionPairList)MetaData.GetListDataSource(propertyInfo.BizObj, propertyInfo.PropertyDescriptor);

			ProcessMgmtTestHelper.AssertCodeDescriptionPairListContents("Lookup list for property " + propertyInfo.Name, list, expectedListItems);
		}
	}
}
