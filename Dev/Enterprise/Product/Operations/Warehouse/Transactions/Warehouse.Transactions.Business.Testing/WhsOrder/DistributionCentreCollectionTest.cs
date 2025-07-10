using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business
{
	[TestedType(typeof(DistributionCentreCollection))]
	class DistributionCentreCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		#region TestAllowNewTemporaryOrganisations

		public void TestAllowNewTemporaryOrganisations()
		{
			var distributionCentreCollection = new DistributionCentreCollection(Factory);
			AssertEquals(true, distributionCentreCollection.AllowNewTemporaryOrganisations);
		}

		#endregion

		#region TestNewChildDefaults

		public void TestNewChildDefaults()
		{
			var distributionCentreCollection = new DistributionCentreCollection(Factory);
			var newDistributionCentre = distributionCentreCollection.AddNew();
			AssertEquals("New Org should have serviced tick box ticked.", true, newDistributionCentre.OH_IsMiscFreightServices);
			AssertEquals("New Org should be a Distribution centre", true, newDistributionCentre.OH_IsDistributionCentre);
		}

		#endregion

		#region TestCollectionReturnsOnlyDistributionCentres

		public void TestCollectionReturnsOnlyDistributionCentres()
		{
			var nonServiceNonDistributionCentre = CreateDistributionCentre("O1", false, false);
			var serviceButNonDistributionCentre = CreateDistributionCentre("O2", true, false);
			var serviceDistributionCentre = CreateDistributionCentre("O3", true, true);
			Factory.Save();

			var distributionCentres = new DistributionCentreCollection(new BusinessObjectFactory());
			AssertEquals("Precondtion - Distribution centre collection should not be loaded yet.", 0, distributionCentres.Count);

			distributionCentres.Load();
			AssertEquals("It should load only distribution centres", serviceDistributionCentre.PK, distributionCentres.Cast<OrgHeader>().Single().PK);
		}

		OrgHeader CreateDistributionCentre(string clientCode, bool isService, bool isDistributionCentre)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsMiscFreightServices = isService;
			client.OH_IsDistributionCentre = isDistributionCentre;
			return client;
		}

		#endregion

		#region TestSetFilterBusinessObjectDefaults

		public void TestSetFilterBusinessObjectDefaults()
		{
			var distributionCentreCollection = new DistributionCentreCollection(Factory);
			Assert(distributionCentreCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property9"));
			Assert(distributionCentreCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Secondary Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		#endregion

		#region TestAddNotificationWhenAdditionalFilterNotMet

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			string expectedNotificationMessage = "An Organization selected from here must have an Organization type of Distribution Center selected under Services tab.";
			var distributionCentreCollection = new DistributionCentreCollection(Factory);
			var org = Helper.CreateClient("O1");
			AssertEquals(expectedNotificationMessage, distributionCentreCollection.GetAllNotificationsWhenAdditionalFilterNotMet(org).ToString());
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DistributionCentreCollection(Factory);
		}

		#endregion
	}
}
