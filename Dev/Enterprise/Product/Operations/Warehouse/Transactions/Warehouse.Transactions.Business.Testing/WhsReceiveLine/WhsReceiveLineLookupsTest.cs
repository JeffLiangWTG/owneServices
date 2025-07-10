using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveLineLookupsTest : WhsDocketLineLookupsTest<WhsReceive, WhsReceiveLine>
	{
		#region TestSupplierParts_DefaultsFields

		public void TestSupplierParts_DefaultsFields()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine1 = receive.Lines.AddNew();
			var inventoryLine2 = receive.Lines.AddNew();
			var inventoryLine3 = receive.Lines.AddNew();

			inventoryLine1.ProductCode = "NewProduct1";
			inventoryLine2.ProductCode = "NewProduct2";
			inventoryLine3.ProductCode = "NewProduct1"; // same code, nothing else populated

			inventoryLine1.ProductDesc = "NewDescription";
			inventoryLine2.ProductDesc = "OtherDescription";
			inventoryLine1.ProductUQ = "PLT";
			inventoryLine2.ProductUQ = "CTN";
			inventoryLine1.CommodityCode = "1234";
			inventoryLine2.CommodityCode = "5678";
			inventoryLine1.WE_PartAttrib1 = "PA1";
			inventoryLine1.WE_PartAttrib2 = "PA2";
			inventoryLine1.WE_PartAttrib3 = "PA3";
			inventoryLine1.WE_ExpiryDate = ZDate.Today;
			inventoryLine1.WE_PackingDate = ZDate.Today;

			var newPart1 = inventoryLine1.Lookups.SupplierParts.AddNew();
			inventoryLine1.SetupSupplierPart(newPart1);
			AssertEquals("NEWPRODUCT1", newPart1.OP_PartNum);
			AssertEquals("NewDescription", newPart1.OP_Desc);
			AssertEquals("PLT", newPart1.OP_StockKeepingUnit);
			AssertEquals("1234", newPart1.OP_RH_NKCommodityCode);
			AssertEquals(true, newPart1.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(true, newPart1.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals("Should be false, because client does not have this attribute set.", false, newPart1.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(true, newPart1.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(true, newPart1.RelatedOrganisations[0].OU_UsePackingDate);

			var newPart2 = inventoryLine2.Lookups.SupplierParts.AddNew();
			inventoryLine2.SetupSupplierPart(newPart2);
			AssertEquals("NEWPRODUCT2", newPart2.OP_PartNum);
			AssertEquals("OtherDescription", newPart2.OP_Desc);
			AssertEquals("CTN", newPart2.OP_StockKeepingUnit);
			AssertEquals("5678", newPart2.OP_RH_NKCommodityCode);
			AssertEquals(false, newPart2.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, newPart2.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, newPart2.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, newPart2.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, newPart2.RelatedOrganisations[0].OU_UsePackingDate);

			var newPart3 = inventoryLine3.Lookups.SupplierParts.AddNew();
			inventoryLine3.SetupSupplierPart(newPart3);
			AssertEquals("NEWPRODUCT1", newPart3.OP_PartNum);
			AssertEquals("", newPart3.OP_Desc);
			AssertEquals("UNT", newPart3.OP_StockKeepingUnit);
			AssertEquals("", newPart3.OP_RH_NKCommodityCode);
			AssertEquals(false, newPart3.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, newPart3.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, newPart3.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(false, newPart3.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(false, newPart3.RelatedOrganisations[0].OU_UsePackingDate);
		}

		#endregion
	}
}
