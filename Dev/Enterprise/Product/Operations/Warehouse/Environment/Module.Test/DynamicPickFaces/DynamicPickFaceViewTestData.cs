using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class DynamicPickFaceViewTestData : EnvTestDataSimpleEnvironment
	{
		public WhsWarehouse Whs2 { get; }
		public OrgHeader Org2 { get; }

		public DynamicPickFaceViewTestData(BusinessObjectFactory factory)
			: base(factory, saveFactory_doNotUseForNewTests: false)
		{
			Whs2 = Helper.CreateWarehouse("2", "A");
			Org2 = Helper.CreateClient("222");
			Factory.Save();
		}

		public IWhsProductParamsByWhsAndClient AssignDynamicProductAndSave(OrgHeader client, OrgSupplierPart part, WhsArea area)
		{
			return AssignDynamicProductAndSave(Factory, client, part, area);
		}
		public static IWhsProductParamsByWhsAndClient AssignDynamicProductAndSave(BusinessObjectFactory factory, OrgHeader client, OrgSupplierPart part, WhsArea area)
		{
			var w3 = factory.New<IWhsProductParamsByWhsAndClient>();
			w3.W3_OH = client.PK;
			w3.W3_OP = part.PK;
			w3.W3_WA_DynamicPickFaceArea = area.PK;
			w3.W3_WW = area.Warehouse.PK;
			factory.Save();
			return w3;
		}

		public void AssignDynamicLocationAndSave(WhsLocation location, WhsArea dynamicPFArea)
		{
			var dynamicLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DLC")) ?? Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicPFArea.PK;
			Factory.Save();
		}

		public void CreateStockAndSave(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, WhsLocation location, string reference, decimal quantity)
		{
			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var receivePK = transactionHelper.CreateWhsReceive(client.PK, whs.PK, reference, new ZArchitecture.NotificationBuffer());
			transactionHelper.CreateWhsReceiveInventoryLine(receivePK, product.PK, quantity, location.PK);
			Factory.Save();
		}

		public void CreateABCCategoryAndSave(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, string category)
		{
			var cat = (BusinessObject)Factory.New<IWhsABCCategory>();
			cat[WhsABCCategorySchema.WJ_OP_Product] = product.PK;
			cat[WhsABCCategorySchema.WJ_OH_Client] = client.PK;
			cat[WhsABCCategorySchema.WJ_WW_Warehouse] = whs.PK;
			cat[WhsABCCategorySchema.WJ_Category] = category;
			cat[WhsABCCategorySchema.WJ_AnalysisDateTo] = ZDateTimeOffset.Today;
			cat[WhsABCCategorySchema.WJ_AnalysisDateFrom] = ZDateTimeOffset.Today;
			Factory.Save();
		}
	}
}
