using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(WarehouseCustomsAddInfo))]
	sealed class WarehouseCustomsAddInfoTest : MultiLineAddInfos.Testing.CusAddInfoTest<WarehouseCustomsAddInfo>
	{
		public void TestParentCanBeLoaded()
		{
			var addInfo = (WarehouseCustomsAddInfo)GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfoInDiffFactory = newFactory.Load<CusAddInfo>(addInfo.PK);
			AssertEquals(addInfo.Parent.PK, addInfoInDiffFactory.Parent.PK);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var warehouseClient = factory.New<OrgHeader>();
			warehouseClient.OH_Code = "SD";
			warehouseClient.MainAddress.OA_Address1 = "SDF";

			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SD@#";
			part.OP_Desc = "PART DESC";
			part.RelatedOrganisations.AddOwner(warehouseClient);

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(factory);
			var whsWarehouse = helper.CreateWarehouse("AD!", warehouseClient.MainAddress, GlbBranch.CurrentBranch);
			var whsReceive = factory.New<IWhsReceive>();
			whsReceive.WD_WW_Whs = whsWarehouse.PK;
			whsReceive.WD_DocketID = "SD";
			whsReceive.WD_OH_Client = warehouseClient.PK;
			whsReceive.WD_DocketSubType = "CUS";
			var whsReceiveLine = factory.New<IWhsReceiveLine>();
			whsReceiveLine.WE_WD = whsReceive.PK;
			whsReceiveLine.WE_TransactionQuantity = 10m;
			whsReceiveLine.WE_OP = part.PK;
			var addInfo = factory.New<WarehouseCustomsAddInfo>();
			addInfo.Parent = (BusinessObject)whsReceiveLine;
			return addInfo;
		}
	}
}
