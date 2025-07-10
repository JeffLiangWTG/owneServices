using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsWarehouseTransactionLineTest : WhsTestCaseWithFactory
	{
		public void TestProperties()
		{
			var warehouse = Factory.New<OrgAddress>();
			var part = Factory.New<OrgSupplierPart>();
			var quantity = 10m;
			var quantityUnit = "KG";
			var partAttrib1 = "PA1";
			var partAttrib2 = "PA2";
			var partAttrib3 = "PA3";
			var serialNumber = "SERN";
			var entryKey = "E1";
			short entryKeyLine = 2;

			var line = new WhsWarehouseTransactionLine();
			line.Warehouse = warehouse;
			line.Product = part;
			line.Quantity = quantity;
			line.QuantityUnit = quantityUnit;
			line.PartAttrib1 = partAttrib1;
			line.PartAttrib2 = partAttrib2;
			line.PartAttrib3 = partAttrib3;
			line.SerialNumber = serialNumber;
			line.EntryKey = entryKey;
			line.EntryLineNumber = entryKeyLine;

			AssertEquals("Warehouse", warehouse, line.Warehouse);
			AssertEquals("Product", part, line.Product);
			AssertEquals("Quantity", quantity, line.Quantity);
			AssertEquals("QuantityUnit", quantityUnit, line.QuantityUnit);
			AssertEquals("PartAttrib1", partAttrib1, line.PartAttrib1);
			AssertEquals("PartAttrib2", partAttrib2, line.PartAttrib2);
			AssertEquals("PartAttrib3", partAttrib3, line.PartAttrib3);
			AssertEquals("SerialNumber", serialNumber, line.SerialNumber);
			AssertEquals("EntryKey", entryKey, line.EntryKey);
			AssertEquals("EntryLineNumber", entryKeyLine, line.EntryLineNumber);

			AssertNotNull(line.WarehouseProblems);
			AssertNotNull(line.QuantityProblems);
			AssertNotNull(line.PartAttrib1Problems);
			AssertNotNull(line.PartAttrib2Problems);
			AssertNotNull(line.PartAttrib3Problems);
			AssertNotNull(line.SerialNumberProblems);
			AssertNotNull(line.EntryKeyProblems);
		}
	}
}
