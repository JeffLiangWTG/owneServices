using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsBondedWarehouseTransactionLineTest : WhsTestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals("AddInfo", "AddInfo", Line.AddInfo);
			AssertEquals("BondedWarehouseQuantity", 10m, Line.BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "L", Line.BondedWarehouseQuantityUnit);
			AssertEquals("PartAttrib1", "Att1", Line.PartAttrib1);
			AssertEquals("PartAttrib2", "Att2", Line.PartAttrib2);
			AssertEquals("PartAttrib3", "Att3", Line.PartAttrib3);
			AssertEquals("CountryOfOrigin", CountryOfOrigin, Line.CountryOfOrigin);
			AssertEquals("CustomsQuantity", 10m, Line.CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", "L", Line.CustomsQuantityUnit);
			AssertEquals("CustomsSecondQuantity", 5m, Line.CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", "KG", Line.CustomsSecondQuantityUnit);
			AssertEquals("CustomsThirdQuantity", 2m, Line.CustomsThirdQuantity);
			AssertEquals("CustomsThirdQuantityUnit", "CU", Line.CustomsThirdQuantityUnit);
			AssertEquals("EntryDate", ZDateTime.Today.Date, Line.EntryDate);
			AssertEquals("EntryKey", "E10000", Line.EntryKey);
			AssertEquals("EntryLineNumber", (short)1, (short)Line.EntryLineNumber);
			AssertEquals("Product", Part, Line.Product);
			AssertEquals("Quantity", 10m, Line.Quantity);
			AssertEquals("QuantityUnit", "L", Line.QuantityUnit);
			AssertEquals("ValueForDuty", 10m, Line.ValueForDuty);
			AssertEquals("TILV Amount", 10m, Line.TILV.Amount);
			AssertEquals("TILV Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Line.TILV.Currency.Code);
			AssertEquals("Warehouse", Warehouse, Line.Warehouse);
			AssertEquals("UniqueKey", new ZGuid("B96B4AC2-3BC7-4d49-9A96-575AC7A7E05F"), Line.UniqueKey);
			AssertNotNull(Line.BondedWarehouseQuantityProblems);
		}

		public void TestCopy()
		{
			var copiedLine = WhsBondedWarehouseTransactionLine.Copy(IBondedLine);
			Assert("New Object", !ReferenceEquals(IBondedLine, copiedLine));
			AssertEquals("AddInfo", "AddInfo", copiedLine.AddInfo);
			AssertEquals("BondedWarehouseQuantity", 10m, copiedLine.BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "L", copiedLine.BondedWarehouseQuantityUnit);
			AssertEquals("PartAttrib1", "Att1", copiedLine.PartAttrib1);
			AssertEquals("PartAttrib2", "Att2", copiedLine.PartAttrib2);
			AssertEquals("PartAttrib3", "Att3", copiedLine.PartAttrib3);
			AssertEquals("CountryOfOrigin", CountryOfOrigin, copiedLine.CountryOfOrigin);
			AssertEquals("CustomsQuantity", 10m, copiedLine.CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", "L", copiedLine.CustomsQuantityUnit);
			AssertEquals("CustomsSecondQuantity", 5m, copiedLine.CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", "KG", copiedLine.CustomsSecondQuantityUnit);
			AssertEquals("CustomsThirdQuantity", 2m, copiedLine.CustomsThirdQuantity);
			AssertEquals("CustomsThirdQuantityUnit", "CU", copiedLine.CustomsThirdQuantityUnit);
			AssertEquals("EntryDate", ZDateTime.Today.Date, copiedLine.EntryDate);
			AssertEquals("EntryKey", "E10000", copiedLine.EntryKey);
			AssertEquals("EntryLineNumber", (short)1, (short)copiedLine.EntryLineNumber);
			AssertEquals("Product", Part, copiedLine.Product);
			AssertEquals("Quantity", 10m, copiedLine.Quantity);
			AssertEquals("QuantityUnit", "L", copiedLine.QuantityUnit);
			AssertEquals("ValueForDuty", 10m, copiedLine.ValueForDuty);
			AssertEquals("TILV Amount", 10m, copiedLine.TILV.Amount);
			AssertEquals("TILV Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency,
				copiedLine.TILV.Currency.Code);
			AssertEquals("Warehouse", Warehouse, copiedLine.Warehouse);
			AssertEquals("UniqueKey", new ZGuid("B96B4AC2-3BC7-4d49-9A96-575AC7A7E05F"), copiedLine.UniqueKey);
		}

		public void TestCopy_WithSerialNumber()
		{
			var line = new WhsBondedWarehouseTransactionLine();
			line.AddInfo = "AddInfo";
			line.BondedWarehouseQuantity = 1m;
			line.BondedWarehouseQuantityUnit = "L";
			line.SerialNumber = "SN1";
			line.EntryKey = "E10000";
			line.EntryLineNumber = 1;
			var part = OrgSupplierPart.New(Factory);
			line.Product = part;
			line.Quantity = 1m;
			line.QuantityUnit = "L";
			line.Warehouse = Factory.New<OrgAddress>();

			var copiedLine = WhsBondedWarehouseTransactionLine.Copy(line);
			Assert("New Object", !ReferenceEquals(line, copiedLine));
			AssertEquals("AddInfo", "AddInfo", copiedLine.AddInfo);
			AssertEquals("BondedWarehouseQuantity", 1m, copiedLine.BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "L", copiedLine.BondedWarehouseQuantityUnit);
			AssertEquals("SerialNumber", "SN1", copiedLine.SerialNumber);
			AssertEquals("EntryKey", "E10000", copiedLine.EntryKey);
			AssertEquals("EntryLineNumber", (short)1, copiedLine.EntryLineNumber);
			AssertEquals("Product", part, copiedLine.Product);
			AssertEquals("Quantity", 1m, copiedLine.Quantity);
			AssertEquals("QuantityUnit", "L", copiedLine.QuantityUnit);
			AssertEquals("Warehouse", line.Warehouse, copiedLine.Warehouse);
		}

		public void TestClone()
		{
			var clonedLine = Line.Clone();
			Assert("New Object", !Object.ReferenceEquals(IBondedLine, clonedLine));

			AssertEquals("AddInfo", "AddInfo", clonedLine.AddInfo);
			AssertEquals("BondedWarehouseQuantity", 10m, clonedLine.BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "L", clonedLine.BondedWarehouseQuantityUnit);
			Assert("Shallow Copy", Object.ReferenceEquals(Line.Product, clonedLine.Product));
		}

		public void TestClone_WithSerialNumber()
		{
			var line = new WhsBondedWarehouseTransactionLine();
			line.AddInfo = "AddInfo";
			line.BondedWarehouseQuantity = 1m;
			line.BondedWarehouseQuantityUnit = "L";
			line.SerialNumber = "SN1";
			line.EntryKey = "E10000";
			line.EntryLineNumber = 1;
			var part = OrgSupplierPart.New(Factory);
			line.Product = part;
			line.Quantity = 1m;
			line.QuantityUnit = "L";
			line.Warehouse = Factory.New<OrgAddress>();

			var clonedLine = line.Clone();
			Assert("New Object", !ReferenceEquals(line, clonedLine));

			AssertEquals("AddInfo", "AddInfo", clonedLine.AddInfo);
			AssertEquals("BondedWarehouseQuantity", 1m, clonedLine.BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "L", clonedLine.BondedWarehouseQuantityUnit);
			AssertEquals("SerialNumber", "SN1", clonedLine.SerialNumber);
			Assert("Shallow Copy", ReferenceEquals(line.Product, clonedLine.Product));
		}

		public void TestCopyArray()
		{
			var line1 = Line.Clone();
			Line.EntryLineNumber = 1;
			var line2 = Line.Clone();
			line2.EntryLineNumber = 2;
			var line3 = Line.Clone();
			line3.EntryLineNumber = 3;

			var bondedLines = new IWhsBondedWarehouseTransactionLine[] { line1, line2, line3 };
			var copiedLines = WhsBondedWarehouseTransactionLine.Copy(bondedLines);

			Assert("New Array", !Object.ReferenceEquals(bondedLines, copiedLines));
			Assert("New Element", !Object.ReferenceEquals(bondedLines[0], copiedLines[0]));
			Assert("New Element", !Object.ReferenceEquals(bondedLines[1], copiedLines[1]));
			Assert("New Element", !Object.ReferenceEquals(bondedLines[2], copiedLines[2]));

			AssertEquals("AddInfo", "AddInfo", copiedLines[1].AddInfo);
			AssertEquals("BondedWarehouseQuantity", 10m, copiedLines[1].BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", "L", copiedLines[1].BondedWarehouseQuantityUnit);
			AssertEquals("PartAttrib1", "Att1", copiedLines[1].PartAttrib1);
			AssertEquals("PartAttrib2", "Att2", copiedLines[1].PartAttrib2);
			AssertEquals("PartAttrib3", "Att3", copiedLines[1].PartAttrib3);
			AssertEquals("CountryOfOrigin", CountryOfOrigin, copiedLines[1].CountryOfOrigin);
			AssertEquals("CustomsQuantity", 10m, copiedLines[1].CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", "L", copiedLines[1].CustomsQuantityUnit);
			AssertEquals("CustomsSecondQuantity", 5m, copiedLines[1].CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", "KG", copiedLines[1].CustomsSecondQuantityUnit);
			AssertEquals("CustomsThirdQuantity", 2m, copiedLines[1].CustomsThirdQuantity);
			AssertEquals("CustomsThirdQuantityUnit", "CU", copiedLines[1].CustomsThirdQuantityUnit);
			AssertEquals("EntryDate", ZDateTime.Today.Date, copiedLines[1].EntryDate);
			AssertEquals("EntryKey", "E10000", copiedLines[1].EntryKey);
			AssertEquals("EntryLineNumber", (short)2, (short)copiedLines[1].EntryLineNumber);
			AssertEquals("Product", Part, copiedLines[1].Product);
			AssertEquals("Quantity", 10m, copiedLines[1].Quantity);
			AssertEquals("QuantityUnit", "L", copiedLines[1].QuantityUnit);
			AssertEquals("ValueForDuty", 10m, copiedLines[1].ValueForDuty);
			AssertEquals("TILV amount", 10m, copiedLines[1].TILV.Amount);
			AssertEquals("TILV Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency,
				copiedLines[1].TILV.Currency.Code);
			AssertEquals("Warehouse", Warehouse, copiedLines[1].Warehouse);
			AssertEquals("UniqueKey", new ZGuid("B96B4AC2-3BC7-4d49-9A96-575AC7A7E05F"), copiedLines[1].UniqueKey);
		}

		public void TestCopyArray_WithSerialNumber()
		{
			var line1 = new WhsBondedWarehouseTransactionLine();
			line1.AddInfo = "AddInfo";
			line1.BondedWarehouseQuantity = 1m;
			line1.BondedWarehouseQuantityUnit = "L";
			line1.SerialNumber = "SN1";
			line1.EntryKey = "E10000";
			line1.EntryLineNumber = 1;
			var part = OrgSupplierPart.New(Factory);
			line1.Product = part;
			line1.Quantity = 1m;
			line1.QuantityUnit = "L";
			line1.Warehouse = Factory.New<OrgAddress>();

			var line2 = line1.Clone();
			line2.EntryLineNumber = 2;
			line2.SerialNumber = "SN2";
			var line3 = line1.Clone();
			line3.EntryLineNumber = 3;
			line3.SerialNumber = "SN3";

			var bondedLines = new IWhsBondedWarehouseTransactionLine[] { line1, line2, line3 };
			var copiedLines = WhsBondedWarehouseTransactionLine.Copy(bondedLines);

			Assert("New Array", !ReferenceEquals(bondedLines, copiedLines));
			Assert("New Element", !ReferenceEquals(bondedLines[0], copiedLines[0]));
			Assert("New Element", !ReferenceEquals(bondedLines[1], copiedLines[1]));
			Assert("New Element", !ReferenceEquals(bondedLines[2], copiedLines[2]));

			AssertEquals("EntryLineNumber", (short)1, copiedLines[0].EntryLineNumber);
			AssertEquals("SerialNumber", "SN1", copiedLines[0].SerialNumber);
			AssertEquals("EntryLineNumber", (short)2, copiedLines[1].EntryLineNumber);
			AssertEquals("SerialNumber", "SN2", copiedLines[1].SerialNumber);
			AssertEquals("EntryLineNumber", (short)3, copiedLines[2].EntryLineNumber);
			AssertEquals("SerialNumber", "SN3", copiedLines[2].SerialNumber);
		}

		public void TestConstructor()
		{
			var line = new WhsBondedWarehouseTransactionLine();
			AssertEquals(Money.Empty, line.TILV);
		}

		public void TestDocketLineConstructor()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAA");

			var docket = Factory.New<WhsReceive>();
			var line = docket.Lines.AddNew();

			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = 10m;
			line.WE_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			line.WE_PartAttrib1 = "PARTATTRIB1";
			line.WE_PartAttrib2 = "PARTATTRIB2";
			line.WE_PartAttrib3 = "PARTATTRIB3";

			var whs = Helper.CreateWarehouse("WHS1");
			docket.WD_WW_Whs = whs.PK;

			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryKey = "ATTRIBUTEENTRYKEY";
			bond.WB_EntryLineNo = 2;
			bond.WB_ParentID = line.PK;
			bond.WB_ParentTableCode = WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode;
			bond.WB_EntryDate = ZDateTime.Today.Date;
			bond.WB_CustomsQty = 15m;
			bond.WB_CustomsUnitOfQty = "BT";
			bond.WB_CustomsSecondQuantity = 10m;
			bond.WB_CustomsSecondUnitQty = "KG";
			bond.WB_CustomsThirdQuantity = 5m;
			bond.WB_CustomsThirdUnitQty = "CU";
			bond.WB_BondedWhsQty = 11m;
			bond.WB_BondedWhsUnitOfQty = "KG";
			bond.WB_RN_NKCountryOfOrigin = "NZ";
			bond.WB_AddInfo = "~AddInfo~";
			bond.WB_TILV = 100m;
			bond.WB_RX_NKTILVCurrency = "USD";

			var bondedTransactionLine = new WhsBondedWarehouseTransactionLine(line);

			AssertEquals("Product", line.SupplierPart, bondedTransactionLine.Product);
			AssertEquals("Quantity", 10m, bondedTransactionLine.Quantity);
			AssertEquals("QuantityUnit", line.WE_F3_NKPackType, bondedTransactionLine.QuantityUnit);
			AssertEquals("PartAttrib1", line.WE_PartAttrib1, bondedTransactionLine.PartAttrib1);
			AssertEquals("PartAttrib2", line.WE_PartAttrib2, bondedTransactionLine.PartAttrib2);
			AssertEquals("PartAttrib3", line.WE_PartAttrib3, bondedTransactionLine.PartAttrib3);
			AssertEquals("UniqueKey", line.PK, bondedTransactionLine.UniqueKey);

			var ratio = line.WE_TransactionQuantity / line.CustomsData.WB_BondedWhsQty;
			AssertEquals("EntryDate", line.CustomsData.WB_EntryDate, bondedTransactionLine.EntryDate);
			AssertEquals("CustomsQuantity", Utilities.Round(line.CustomsData.WB_CustomsQty * ratio, 5),
				bondedTransactionLine.CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", line.CustomsData.WB_CustomsUnitOfQty,
				bondedTransactionLine.CustomsQuantityUnit);
			AssertEquals("CustomsSecondQuantity", line.CustomsData.WB_CustomsSecondQuantity,
				bondedTransactionLine.CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", line.CustomsData.WB_CustomsSecondUnitQty,
				bondedTransactionLine.CustomsSecondQuantityUnit);
			AssertEquals("CustomsThirdQuantity", line.CustomsData.WB_CustomsThirdQuantity,
				bondedTransactionLine.CustomsThirdQuantity);
			AssertEquals("CustomsThirdQuantityUnit", line.CustomsData.WB_CustomsThirdUnitQty,
				bondedTransactionLine.CustomsThirdQuantityUnit);
			AssertEquals("BondedWarehouseQuantity", Utilities.Round(line.CustomsData.WB_BondedWhsQty * ratio, 4),
				bondedTransactionLine.BondedWarehouseQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", line.CustomsData.WB_BondedWhsUnitOfQty,
				bondedTransactionLine.BondedWarehouseQuantityUnit);
			AssertEquals("ValueForDuty", Utilities.Round(line.CustomsData.WB_ValueForDuty * ratio, 5),
				bondedTransactionLine.ValueForDuty);
			AssertEquals("TILV Amount", Utilities.Round(line.CustomsData.WB_TILV * ratio, 4),
				bondedTransactionLine.TILV.Amount);
			AssertEquals("TILV Currency", line.CustomsData.WB_RX_NKTILVCurrency,
				bondedTransactionLine.TILV.Currency.Code);
			AssertEquals("CountryOfOrigin", line.CustomsData.CountryOfOrigin, bondedTransactionLine.CountryOfOrigin);
			AssertEquals("AddInfo", line.CustomsData.WB_AddInfo, bondedTransactionLine.AddInfo);

			AssertEquals("Warehouse", docket.Warehouse.WarehouseAddress, bondedTransactionLine.Warehouse);
			docket.WD_WW_Whs = ZGuid.Empty;
			bondedTransactionLine = new WhsBondedWarehouseTransactionLine(line);
			AssertEquals("Warehouse is null", null, bondedTransactionLine.Warehouse);

			docket.WD_WW_Whs = whs.PK;
			line.WE_WD = ZGuid.Empty;
			bondedTransactionLine = new WhsBondedWarehouseTransactionLine(line);
			AssertEquals("Warehouse", null, bondedTransactionLine.Warehouse);

			line.WE_WD = docket.PK;
			bondedTransactionLine = new WhsBondedWarehouseTransactionLine(line);
			AssertEquals("Warehouse", docket.Warehouse.WarehouseAddress, bondedTransactionLine.Warehouse);

			AssertEquals("Use WhsBondedWarehouseAttribute EntryKey", "ATTRIBUTEENTRYKEY",
				bondedTransactionLine.EntryKey);
			AssertEquals("Use WhsBondedWarehouseAttribute EntryLineNumber", (short)2,
				bondedTransactionLine.EntryLineNumber);

			docket.WD_DocketType = CodeLists.DocketType.Codes.Order;
			bondedTransactionLine = new WhsBondedWarehouseTransactionLine(line);
			AssertEquals("Use WhsDocketLine EntryKey", "LINEENTRYKEY", bondedTransactionLine.EntryKey);
			AssertEquals("Use WhsDocketLine EntryLineNumber", (short)1, bondedTransactionLine.EntryLineNumber);

			line.WE_WD = ZGuid.Empty;
			bondedTransactionLine = new WhsBondedWarehouseTransactionLine(line);
			AssertEquals("Use WhsBondedWarehouseAttribute EntryKey", "ATTRIBUTEENTRYKEY",
				bondedTransactionLine.EntryKey);
			AssertEquals("Use WhsBondedWarehouseAttribute EntryLineNumber", (short)2,
				bondedTransactionLine.EntryLineNumber);
		}

		public void TestCompareForBondedWhsQtyWithIsEmptyCheck()
		{
			var line1 = new WhsBondedWarehouseTransactionLine();
			var line2 = new WhsBondedWarehouseTransactionLine();

			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));

			var part = OrgSupplierPart.New(Factory);

			line2.EntryKey = "BEK";
			line2.EntryLineNumber = 1;
			line2.Product = part;
			line2.Quantity = 10m;
			line2.PartAttrib1 = "PA1";
			line2.PartAttrib2 = "PA2";
			line2.PartAttrib3 = "PA3";

			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));

			line1.EntryKey = "BEK";
			line1.EntryLineNumber = 1;
			line1.Product = part;
			line1.Quantity = 10m;
			line1.PartAttrib1 = "PA1";
			line1.PartAttrib2 = "PA2";
			line1.PartAttrib3 = "PA3";

			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));

			line2.EntryKey = "BK";
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.EntryKey = "";
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.EntryKey = "BEK";
			line2.EntryKey = "BEK";

			line2.EntryLineNumber = 2;
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.EntryLineNumber = 0;
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.EntryLineNumber = 1;
			line2.EntryLineNumber = 1;

			line2.Product = OrgSupplierPart.New(Factory);
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.Product = null;
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.Product = part;
			line2.Product = part;

			line2.Quantity = 15m;
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.Quantity = 0m;
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.Quantity = 10m;
			line2.Quantity = 10m;

			line2.PartAttrib1 = "P1";
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.PartAttrib1 = "";
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.PartAttrib1 = "PA1";
			line2.PartAttrib1 = "PA1";

			line2.PartAttrib2 = "P2";
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.PartAttrib2 = "";
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.PartAttrib2 = "PA2";
			line2.PartAttrib2 = "PA2";

			line2.PartAttrib3 = "PA33";
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.PartAttrib3 = "";
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.PartAttrib3 = "PA3";
			line2.PartAttrib3 = "PA3";
		}

		public void TestCompareForBondedWhsQtyWithIsEmptyCheck_WithSerialNumber()
		{
			var line1 = new WhsBondedWarehouseTransactionLine();
			var line2 = new WhsBondedWarehouseTransactionLine();

			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));

			var part = OrgSupplierPart.New(Factory);

			line2.EntryKey = "BEK";
			line2.EntryLineNumber = 1;
			line2.Product = part;
			line2.Quantity = 1m;
			line2.SerialNumber = "SN1";

			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));

			line1.EntryKey = "BEK";
			line1.EntryLineNumber = 1;
			line1.Product = part;
			line1.Quantity = 1m;
			line1.SerialNumber = "SN1";

			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));

			line2.SerialNumber = "SN2";
			AssertEquals(false, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
			line1.SerialNumber = "";
			AssertEquals(true, line1.CompareForBondedWhsQtyWithIsEmptyCheck(line2));
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			SetUpLine();
		}

		WhsBondedWarehouseTransactionLine Line;
		IWhsBondedWarehouseTransactionLine IBondedLine;
		RefCountry CountryOfOrigin;
		OrgSupplierPart Part;
		OrgAddress Warehouse;

		void SetUpLine()
		{
			Line = new WhsBondedWarehouseTransactionLine();
			Line.AddInfo = "AddInfo";
			Line.BondedWarehouseQuantity = 10m;
			Line.BondedWarehouseQuantityUnit = "L";
			Line.PartAttrib1 = "Att1";
			Line.PartAttrib2 = "Att2";
			Line.PartAttrib3 = "Att3";
			CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");
			Line.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");
			Line.CustomsQuantity = 10m;
			Line.CustomsQuantityUnit = "L";
			Line.CustomsSecondQuantity = 5m;
			Line.CustomsSecondQuantityUnit = "KG";
			Line.CustomsThirdQuantity = 2m;
			Line.CustomsThirdQuantityUnit = "CU";
			Line.EntryDate = ZDateTime.Today.Date;
			Line.EntryKey = "E10000";
			Line.EntryLineNumber = 1;
			Part = OrgSupplierPart.New(Factory);
			Line.Product = Part;
			Line.Quantity = 10m;
			Line.QuantityUnit = "L";
			Line.ValueForDuty = 10m;
			Line.TILV = new Money(10m, GlbCompany.CurrentCompany.LocalCurrency);
			Line.Warehouse = Warehouse = Factory.New<OrgAddress>();
			Line.UniqueKey = new ZGuid("B96B4AC2-3BC7-4d49-9A96-575AC7A7E05F");

			IBondedLine = Line;
		}

		#endregion
	}
}
