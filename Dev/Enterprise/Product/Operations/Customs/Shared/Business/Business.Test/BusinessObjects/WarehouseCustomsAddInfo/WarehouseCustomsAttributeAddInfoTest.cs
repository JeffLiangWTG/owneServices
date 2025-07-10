using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(WarehouseCustomsAttributeAddInfo))]
	sealed class WarehouseCustomsAttributeAddInfoTest : MultiLineAddInfos.Testing.CusAddInfoTest<WarehouseCustomsAttributeAddInfo>
	{
		public void TestParentCanBeLoaded()
		{
			var addInfo = (WarehouseCustomsAttributeAddInfo)GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfoInDiffFactory = newFactory.Load<WarehouseCustomsAttributeAddInfo>(addInfo.PK);
			AssertEquals(addInfo.Parent.PK, addInfoInDiffFactory.Parent.PK);
		}

		public void TestReportIfIncorrectTableCode()
		{
			var addInfo = Factory.New<WarehouseCustomsAttributeAddInfo>();
			AssertEquals("Default B7_ParentTableCode", WhsBondedWarehouseAttributeSchema.Constants.Prefix, addInfo.B7_ParentTableCode);
			addInfo.B7_ParentTableCode = "K!";
			AssertEquals("LastMessageReported", "Table Code must only be 'WB' but instead it is 'K!'.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
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
			var attribute = factory.New<IWhsBondedWarehouseAttribute>();
			attribute.WB_ParentID = whsReceiveLine.PK;
			attribute.WB_ParentTableCode = ((BusinessObject)whsReceiveLine).TablePrefix;
			attribute.WB_AddInfo = "SD";
			var addInfo = factory.New<WarehouseCustomsAttributeAddInfo>();
			addInfo.B7_Type = "CCT";
			addInfo.Parent = (BusinessObject)attribute;
			return addInfo;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WarehouseCustomsAttributeAddInfo>();
		}
	}
}
