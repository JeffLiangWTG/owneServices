using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolDepotCollection))]
	public class CFSLoadListConsolCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestAdditionalFilter

		public void TestAdditionalFilterContainsCurrentCompany()
		{
			OrgHeader currentCompanyOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompanyOrgProxy.OH_IsMiscFreightServices = true;
			currentCompanyOrgProxy.OH_IsPackDepot = true;
			currentCompanyOrgProxy.OH_IsUnpackDepot = true;

			Factory.Save();

			BusinessObjectCollection collection = GetCollectionToTest();
			collection.Load();

			AssertCollectionContains("Contains current company Org Proxy", currentCompanyOrgProxy, collection);
			AssertEquals("No other Orgs fit the bill", 1, collection.Count);
		}

		public void TestAdditionalFilterContainsCurrentBranch()
		{
			OrgHeader currentBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);
			currentBranchOrgProxy.OH_IsMiscFreightServices = true;
			currentBranchOrgProxy.OH_IsPackDepot = true;
			currentBranchOrgProxy.OH_IsUnpackDepot = true;

			Factory.Save();

			BusinessObjectCollection collection = GetCollectionToTest();
			collection.Load();

			AssertCollectionContains("Contains current branch Org Proxy", currentBranchOrgProxy, collection);
			AssertEquals("No other Orgs fit the bill", 1, collection.Count);
		}

		#endregion

		#region TestNotifications

		public void TestNotifications()
		{
			BusinessObjectCollection collection = GetCollectionToTest();

			ZQuery depotQuery = new ZQuery();
			depotQuery.AddToFilter(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			depotQuery.AddToFilter(OrgHeaderSchema.OH_IsPackDepot, true);
			depotQuery.AddToFilter(OrgHeaderSchema.OH_IsUnpackDepot, true);

			OrgHeader depot = Factory.LoadTop1<OrgHeader>(depotQuery);

			AssertContains("Only Organizations which are defined in System -> Companies or System -> Branches can be chosen here.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(depot));

			OrgHeader nonDepotCompany = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.OrgProxy.PK));

			AssertContains("An Organization selected from here must have an Organization type of Services selected and must have either a Pack or an Unpack CFS selected.", collection.GetAllNotificationsWhenAdditionalFilterNotMet(nonDepotCompany));
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CFSLoadListConsolDepotCollection(Factory);
		}

		#endregion
	}
}
