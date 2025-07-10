using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class OpportunityViewControllerTest : TestCaseWithFactory
	{
		public void TestOrgChanged()
		{
			foreach (ICodeDescription pair in new SystemDefinedSalesProductList())
			{
				if (pair.Code != SystemDefinedSalesProductList.Codes.Warehouse)
				{
					var prod = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, pair.Code);
					AssertEquals("OrgOpportunity.DuplicateSharedTradeLanes assumes this", OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail, prod.AllowedAssociationTargets);
				}
			}

			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var warehouse = (BusinessObject)Factory.New<IWhsWarehouse>();
			warehouse.FillWithValidTestData();

			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var part2 = Factory.NewWithValidTestData<OrgSupplierPart>();

			part1.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);
			part1.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Both);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);
			part2.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Both);

			var org1Sales = (SalesHeaderCollection)org1.ProspectiveSalesHeaderCollection;
			var org1header = org1Sales.AddNew(product);
			var org1Sale1 = org1header.EntitySalesCollectionProductView.AddNew();
			org1Sale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			org1Sale1.OW_WW = warehouse.PK;
			org1Sale1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var org1Detail1 = org1Sale1.EntityTradeDetailsCollection.AddNew();
			org1Detail1.PA_OP = part1.PK;
			org1Detail1.EstimatedProfit = 47;

			var org1Detail2 = org1Sale1.EntityTradeDetailsCollection.AddNew();
			org1Detail2.PA_OP = part2.PK;
			org1Detail2.EstimatedProfit = 71;

			var opp1 = org2.SalesOpportunities.AddNew();
			var opp1Sales = (SalesHeaderCollection)opp1.ProspectiveSalesHeaderCollection;
			var opp1Header = opp1Sales.AddNew(product);
			var opp1Sale1 = opp1Header.EntitySalesCollectionProductView.AddNew();
			opp1Sale1.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			opp1Sale1.OW_WW = warehouse.PK;
			opp1Sale1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;

			var opp1Detail1 = opp1Sale1.EntityTradeDetailsCollection.AddNew();
			opp1Detail1.PA_OP = part1.PK;
			opp1Detail1.EstimatedProfit = 100;

			var opp1Detail2 = opp1Sale1.EntityTradeDetailsCollection.AddNew();
			opp1Detail2.PA_OP = part2.PK;
			opp1Detail2.EstimatedProfit = 99;

			Factory.Save();

			using (var control = new OpportunityForm(opp1))
			{
				var viewController = new OpportunityViewControllerForTest(control);

				opp1.P8_OH = org1.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var opp1reload = factory2.Load<OrgOpportunity>(opp1.PK);
				AssertEquals(1, opp1reload.ProspectiveSalesHeaderCollection.Count);
				var headerReload = ((SalesHeaderCollection)opp1reload.ProspectiveSalesHeaderCollection)[0];
				AssertEquals(1, headerReload.EntitySalesCollectionProductView.Count);
				var appendedSale = headerReload.EntitySalesCollectionProductView.Cast<EntitySalesWrapper>()
					.Single(x => x.PK == opp1Sale1.PK);

				AssertEquals(2, appendedSale.EntityTradeDetailsCollection.Count);

				var appendedDetail1 = appendedSale.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Single(x => x.PK == opp1Detail1.PK);
				var appendedDetail2 = appendedSale.EntityTradeDetailsCollection.Cast<EntityTradeDetailWrapper>().Single(x => x.PK == opp1Detail2.PK);

				AssertEquals("opp detail1 values unchanged", 100m, appendedDetail1.EstimatedProfit);
				AssertEquals("opp detail2 values unchanged", 99m, appendedDetail2.EstimatedProfit);
			}
		}

		public void TestShouldPromptToDoMigrationForUnsavedCopiedOpportunity()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.OnCopyOrgOpportunity(opp1);
			opp2.P8_OH = ZGuid.Empty;

			using (var control = new OpportunityForm(opp2))
			{
				var viewController = new OpportunityViewController(control);
				opp2.P8_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				opp2.OnCopyOrgTradePeriod(opp2.Factory.New<OrgTradePeriod>());
				AssertEquals(true, opp2.HasUnsavedCopiedOrgTradePeriods);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var dynMethod = viewController.GetType().GetMethod("OpportunityView_ValidatingForSave", BindingFlags.NonPublic | BindingFlags.Instance);
				dynMethod.Invoke(viewController, new object[] { null, null });

				AssertEquals("This opportunity has estimated values that will need to be migrated to the new Organization. Any values that are shared with other records will be copied and the shared association removed. Are you sure you wish to change the Organization and begin the migration?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
