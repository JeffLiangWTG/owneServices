using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Warehouse.Transactions.DataTransfer.WhsReceiveDataLoad;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	[TestedType(typeof(WhsReceiveDataLoad))]
	internal class WhsReceiveDataLoadTest : DataLoadTestCase<WhsReceiveDataLoad>
	{
		#region TestImport_WithEmptyArrivalDate

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2005, 1, 1, 3, 0, 0)]
		public void TestImport_WithEmptyArrivalDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			var receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestImport("", "BOND"));
			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Precondition: Receive is in DB.", receive.IsInDatabase);
			AssertEquals("ArrivalDate should not remain empty since it must be assigned for Putaway docket lines.", false, receive.WD_ArrivalDate.IsEmpty);
			AssertEquals("BookingDate should set to current date time.", new ZDateTimeOffset(2005, 1, 1, 14, 0, 0, TimeSpan.FromHours(11)), receive.WD_BookingDate);
		}

		[TestDate(2005, 1, 1, 3, 0, 0)]
		public void TestImport_WithEmptyArrivalDate_UsingTimeZoneOfWarehouse()
		{
			var receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);

			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			WhsBonded.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			Factory.Save();
			ImportCsvData(GetTestImport("", "BOND"));
			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Precondition: Receive is in DB.", receive.IsInDatabase);
			AssertEquals("ArrivalDate is empty.", false, receive.WD_ArrivalDate.IsEmpty);
			AssertEquals("BookingDate should set to current date time.", new ZDateTimeOffset(2005, 1, 1, 11, 0, 0, TimeSpan.FromHours(8)), receive.WD_BookingDate);
		}

		#endregion

		#region TestImport_WithEmptyArrivalDateAndPendingStatus

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2005, 1, 1, 3, 0, 0)]
		public void TestImport_WithEmptyArrivalDateAndPendingStatus()
		{
			TestDateAttribute.UseUNLOCO = true;
			var receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestImport("", ""));
			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Precondition: Receive is in DB.", receive.IsInDatabase);
			AssertEquals("ArrivalDate should remain empty.", true, receive.WD_ArrivalDate.IsEmpty);
			AssertEquals("BookingDate should set to current date time.", new ZDateTimeOffset(2005, 1, 1, 14, 0, 0, TimeSpan.FromHours(11)), receive.WD_BookingDate);
		}

		#endregion

		#region TestImport_WithNonEmptyArrivalDate

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestImport_WithNonEmptyArrivalDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			var receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestImport("20120115", "BOND"));
			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Precondition: Receive is in DB.", receive.IsInDatabase);

			var warehouse = receive.Warehouse;
			var arrivalDate = warehouse.GetWarehouseBranchLocalDateTimeOffset(new ZDateTime(2012, 1, 15));

			AssertEquals("Non_Empty ArrivalDate should be imported.", arrivalDate, receive.WD_ArrivalDate);
			AssertEquals("BookingDate should set to Arrival date.", arrivalDate, receive.WD_BookingDate);
		}

		#endregion

		#region TestImport_ExistingReceiveWithASNLines

		public void TestImport_ExistingReceiveWithASNLines()
		{
			var receive = Helper.CreateWhsReceive(BondedClient, WhsBonded, "REF1005");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, Part1, 1m);
			Factory.Save();

			receive.PopulateASNLines();
			Factory.Save();

			AssertEquals("Precondition: Receive has ASN lines.", true, receive.AsnLines.Count > 0);
			AssertEquals("Precondition: Receive has 1 receive line.", 1, receive.Lines.Count);

			ImportCsvData(GetTestImport("", ""));
			receive.Lines.RefreshFromDb();

			var newReceiveLine = receive.Lines.Single(line => line.WE_TransactionQuantity == 10m);
			AssertEquals("Receive has ASN lines, WE_ClientOrderedUnits is set to 0.", 0m, newReceiveLine.WE_ClientOrderedUnits);
		}

		#endregion

		#region TestImport_ExistingReceiveWithoutASNLines

		public void TestImport_ExistingReceiveWithoutASNLines()
		{
			var receive = Helper.CreateWhsReceive(BondedClient, WhsBonded, "REF1005");
			Factory.Save();

			AssertEquals("Precondition: Receive has no ASN lines.", false, receive.AsnLines.Count > 0);

			ImportCsvData(GetTestImport("", ""));
			receive.Lines.RefreshFromDb();

			var newReceiveLine = receive.Lines.Single(line => line.WE_TransactionQuantity == 10m);
			AssertEquals("Receive has no ASN lines, WE_ClientOrderedUnits is set to WE_TransactionQuantity.", 10m, newReceiveLine.WE_ClientOrderedUnits);
		}

		#endregion

		#region GetTestImport

		string[,] GetTestImport(ZString arrivalDate, ZString location)
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",      "556542" },
						{ "ClientCode",          "BONDCLI" },
						{ "Warehouse",           "BONDWHS" },
						{ "Reference",           "REF1005" },
						{ "ArrivalDate",         arrivalDate },
						{ "ProductCode",         "P01" },
						{ "Quantity",            "10" },
						{ "QuantityUQ",          "KG" },
						{ "Pallets",             "0001" },
						{ "Location",            location },
						{ "Attribute1",          "A1" },
						{ "Attribute2",          "A2" },
						{ "Attribute3",          "A3" },
						{ "ExpiryDate",          "20100101" },
						{ "PackingDate",         "20010101" },
						{ "CustomsEntryLineNo",  "1" },
						{ "CustomsEntryDate",    "20050506" },
						{ "CustomsAddInfo",      "ADDINFO" },
						{ "CustomsQty",          "20" },
						{ "CustomsUQ",           "KG" },
						{ "CtryOfOrigin",        "AU" },
						{ "ValueForDuty",        "1" },
						{ "BondedWhsQty",        "30" },
						{ "BondedWhsUQ",         "KG" },
						{ "TILV",                "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",        "BONDMANU" },
						{ "ZoneStatus",        "D" },
						{ "IsFromOtherFTZWarehouse",        "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",     "" }
			};
		}

		#endregion

		#region TestImport_BondedReceiveDataMapping

		[TestDate(2005, 1, 1)]
		public void TestImport_BondedReceiveDataMapping()
		{
			var receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestDataImportBondedReceiveData());

			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Receive in DB", receive.IsInDatabase);

			var custCodeList = (OrgCusCode[])receive.Client.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCD"));
			AssertEquals("ClientCode", "BONDCLI", custCodeList[0].OK_CustomsRegNo);
			custCodeList = (OrgCusCode[])receive.Client.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCP"));
			AssertEquals("Warehouse", "BONDWHS", custCodeList[0].OK_CustomsRegNo);
			AssertEquals("Reference", "REF1005", receive.WD_ExternalReference);
			AssertEquals("ArrivalDate", new ZDateTimeOffset(2005, 05, 05, 0, 0, 0, TimeSpan.FromHours(10)), receive.WD_ArrivalDate);

			AssertEquals("ProductCode", "P01", receive.Lines[0].SupplierPart.OP_PartNum);
			AssertEquals("Quantity", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("QuantityUQ", "KG", receive.Lines[0].WE_F3_NKPackType);

			//Assert("Pallets", true);
			AssertEquals("Location", "BOND", receive.Lines[0].LocationString);

			AssertEquals("Attribute1", "A1", receive.Lines[0].WE_PartAttrib1);
			AssertEquals("Attribute2", "A2", receive.Lines[0].WE_PartAttrib2);
			AssertEquals("Attribute3", "A3", receive.Lines[0].WE_PartAttrib3);
			AssertEquals("SerialNumber", "SN", receive.Lines[0].WE_SerialNumber);

			AssertEquals("Docket Line Type should be Receive", Transactions.CodeLists.DocketType.Codes.Receive, receive.Lines[0].WE_DocketLineType);
			AssertEquals("One Docket Line should be created", 1, receive.Lines.Count);

			AssertEquals("ExpiryDate", new ZDateTime(2010, 01, 01), receive.Lines[0].WE_ExpiryDate);
			AssertEquals("PackingDate", new ZDateTime(2001, 01, 01), receive.Lines[0].WE_PackingDate);

			AssertEquals("WE_BondedEntryKey", WhsBondedWarehouseAttribute.BuildKey("556542", 1), receive.Lines[0].WE_BondedEntryKey);
			AssertEquals("CustomsEntryNo", "556542", receive.Lines[0].CustomsData.WB_EntryKey);
			AssertEquals("CustomsEntryLineNo", (short)1, receive.Lines[0].CustomsData.WB_EntryLineNo);
			AssertEquals("CustomsEntryDate", new ZDateTime(2005, 05, 06), receive.Lines[0].CustomsData.WB_EntryDate);
			AssertEquals("CustomsAddInfo", "ADDINFO", receive.Lines[0].CustomsData.WB_AddInfo);
			AssertEquals("CustomsQty", 20m, receive.Lines[0].CustomsData.WB_CustomsQty);
			AssertEquals("CustomsUQ", "KG", receive.Lines[0].CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CtryOfOrigin", "AU", receive.Lines[0].CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("BondedWhsQty", 30m, receive.Lines[0].CustomsData.WB_BondedWhsQty);
			AssertEquals("BondedWhsUQ", "KG", receive.Lines[0].CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("ValueForDuty", 1m, receive.Lines[0].CustomsData.WB_ValueForDuty);
			AssertEquals("TILV", 5.1m, receive.Lines[0].CustomsData.WB_TILV);
			AssertEquals("CustomsSecondQuantity", 24m, receive.Lines[0].CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsSecondUnitQty", "GRM", receive.Lines[0].CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("Tariff", "TRF", receive.Lines[0].CustomsData.WB_Tariff);
			AssertEquals("PrimaryPreference", "STANDARD", receive.Lines[0].CustomsData.WB_PrimaryPreference);
			AssertEquals("ParentID", receive.Lines[0].PK, receive.Lines[0].CustomsData.WB_ParentID);
			AssertEquals("ParentTableCode", WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode, receive.Lines[0].CustomsData.WB_ParentTableCode);
			AssertEquals("CustomsThirdQuantity", 12m, receive.Lines[0].CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsThirdUnitQty", "GR", receive.Lines[0].CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("ManufacturerCode", "BONDMANU", receive.Lines[0].CustomsData.ManufacturerCode);
			// add test for new fields 
		}

		string[,] GetTestDataImportBondedReceiveData()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",   "556542" },
						{ "ClientCode",     "BONDCLI" },
						{ "Warehouse",      "BONDWHS" },
						{ "Reference",      "REF1005" },
						{ "ArrivalDate",      "20050505" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "KG" },
						{ "Pallets",        "0001" },
						{ "Location",     "BOND" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20100101" },
						{ "PackingDate",      "20010101" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO" },
						{ "CustomsQty",     "20" },
						{ "CustomsUQ",      "KG" },
						{ "CtryOfOrigin",   "AU" },
						{ "ValueForDuty",   "1" },
						{ "BondedWhsQty",   "30" },
						{ "BondedWhsUQ",      "KG" },
						{ "TILV",       "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",        "BONDMANU" },
						{ "ZoneStatus",        "D" },
						{ "IsFromOtherFTZWarehouse",        "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber",     "SN" },
						{ "ReceiveCategory",       "" }
							};
		}

		#endregion

		#region TestImport_BondedWithMinimumData

		public void TestImport_BondedWithMinimumData()
		{
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());

			receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNotNull("Receive not created", receive);

			receive.Lines.ApplySort(WhsDocketLineSchema.Constants.WE_TransactionQuantity, ListSortDirection.Ascending);

			OrgCusCode[] custCodeList = (OrgCusCode[])receive.Client.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCD"));
			AssertEquals("ClientCode", "BONDCLI", custCodeList[0].OK_CustomsRegNo);
			custCodeList = (OrgCusCode[])receive.Client.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCP"));
			AssertEquals("Warehouse", "BONDWHS", custCodeList[0].OK_CustomsRegNo);
			AssertEquals("WD_ExternalReference", "INWARDS W00000001", receive.WD_ExternalReference);
			AssertEquals("ArrivalDate", ZDateTime.Empty, receive.WD_ArrivalDate.Date);

			AssertEquals("ProductCode", "P01", receive.Lines[0].SupplierPart.OP_PartNum);
			AssertEquals("Quantity", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("QuantityUQ", "KG", receive.Lines[0].WE_F3_NKPackType);

			//Assert("Pallets", true);
			AssertEquals("Location", "", receive.Lines[0].LocationString);

			AssertEquals("Attribute1", "", receive.Lines[0].WE_PartAttrib1);
			AssertEquals("Attribute2", "", receive.Lines[0].WE_PartAttrib2);
			AssertEquals("Attribute3", "", receive.Lines[0].WE_PartAttrib3);
			AssertEquals("SerialNumber", "", receive.Lines[0].WE_SerialNumber);
			AssertEquals("ExpiryDate", ZDateTime.Empty, receive.Lines[0].WE_ExpiryDate);
			AssertEquals("PackingDate", ZDateTime.Empty, receive.Lines[0].WE_PackingDate);

			AssertEquals("WE_BondedEntryKey", WhsBondedWarehouseAttribute.BuildKey("123456", 1), receive.Lines[0].WE_BondedEntryKey);
			AssertEquals("CustomsEntryKey", "123456", receive.Lines[0].CustomsData.WB_EntryKey);
			AssertEquals("CustomsEntryLineNo", (short)1, receive.Lines[0].CustomsData.WB_EntryLineNo);
			AssertEquals("CustomsEntryDate", receive.WD_ArrivalDate.ToZDateTime(), receive.Lines[0].CustomsData.WB_EntryDate);
			AssertEquals("CustomsAddInfo", "", receive.Lines[0].CustomsData.WB_AddInfo);
			AssertEquals("CustomsQty", 10m, receive.Lines[0].CustomsData.WB_CustomsQty);
			AssertEquals("CustomsUQ", "KG", receive.Lines[0].CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CtryOfOrigin", "", receive.Lines[0].CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("BondedWhsQty", 0m, receive.Lines[0].CustomsData.WB_BondedWhsQty);
			AssertEquals("BondedWhsUQ", "", receive.Lines[0].CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("ValueForDuty", 0m, receive.Lines[0].CustomsData.WB_ValueForDuty);
			AssertEquals("TILV", 0m, receive.Lines[0].CustomsData.WB_TILV);
			AssertEquals("ParentID", receive.Lines[0].PK, receive.Lines[0].CustomsData.WB_ParentID);
			AssertEquals("ParentTableCode", WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode, receive.Lines[0].CustomsData.WB_ParentTableCode);
			AssertEquals("CustomsSecondQuantity", 0m, receive.Lines[0].CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsSecondUnitQty", "", receive.Lines[0].CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("Tariff", "", receive.Lines[0].CustomsData.WB_Tariff);
			AssertEquals("PrimaryPreference", "", receive.Lines[0].CustomsData.WB_PrimaryPreference);

			AssertEquals("CustomsThirdQuantity", 0m, receive.Lines[0].CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsThirdUnitQty", "", receive.Lines[0].CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("ManufacturerCode", "BONDMANU", receive.Lines[0].CustomsData.ManufacturerAddress.Header.OH_Code);
			// add test for new fields 

			Assert("Receive in database", receive.IsInDatabase);
			AssertEquals("Receive.Lines.Count", 2, receive.Lines.Count);
		}

		string[,] GetTestImportBondedWithMinimumData()
		{
			return new string[ReceiveHeader.ColumnCount, 3]

			//		Header					Line1			Line2
			//		------------------------------------------------------
			{
						{ "CustomsEntryNo",    "123456",   "123456" },
						{ "ClientCode",      "BONDCLI",    "BONDCLI" },
						{ "Warehouse",     "BONDWHS",    "BONDWHS" },
						{ "Reference",     "",       "" },
						{ "ArrivalDate",     "",       "" },
						{ "ProductCode",     "P01",      "P01" },
						{ "Quantity",      "10",     "25" },
						{ "QuantityUQ",      "",       "" },
						{ "Pallets",       "",       "" },
						{ "Location",      "",       "" },
						{ "Attribute1",      "",       "" },
						{ "Attribute2",      "",       "" },
						{ "Attribute3",      "",       "" },
						{ "ExpiryDate",      "",       "" },
						{ "PackingDate",     "",       "" },
						{ "CustomsEntryLineNo",  "1",      "1" },
						{ "CustomsEntryDate",  "",       "" },
						{ "CustomsAddInfo",    "",       "" },
						{ "CustomsQty",      "",       "" },
						{ "CustomsUQ",     "",       "" },
						{ "CtryOfOrigin",    "",       "" },
						{ "ValueForDuty",    "",       "" },
						{ "BondedWhsQty",    "",       "" },
						{ "BondedWhsUQ",     "",       "" },
						{ "TILV",        "",       "" },
						{ "CustomsSecondQuantity","",   "" },
						{ "CustomsSecondUnitQty","",  "" },
						{ "Tariff","",   "" },
						{ "PrimaryPreference","",   "" },
						{ "CustomsThirdQuantity","",   "" },
						{ "CustomsThirdUnitQty","",  "" },
						{ "ManufacturerCode",    "BONDMANU",     "BONDMANU" },
						{ "ZoneStatus", "" , "" },
						{ "IsFromOtherFTZWarehouse", "N" , "N" },
						{ "OutwardType", "", "" },
						{ "SerialNumber", "", "" },
						{ "ReceiveCategory","",      "" }
			};
		}

		#endregion

		#region TestImport_CCPAddress

		public void TestImport_CCPAddress()
		{
			var whs2 = Helper.CreateWarehouse("WHS2");
			whs2.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded;
			var temp = Helper.CreateClient("CLI2", "CLI2");
			whs2.WW_OA_WarehouseAddress = temp.Addresses.DefaultAddressOfType(OrgAddressType.Office, false).PK;

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			AssertEquals("Address = Org Office Address", WhsBonded.PK, Loader.CurrentReceive.WD_WW_Whs);

			var tempCusCode = (OrgCusCode)BondedClient.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCP"))[0];
			tempCusCode.OK_OA_PremisesAddress = whs2.WW_OA_WarehouseAddress;

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			AssertEquals("Address = Org CPP Premises Address", whs2.PK, Loader.CurrentReceive.WD_WW_Whs);

			tempCusCode = (OrgCusCode)BondedClient.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCP"))[0];
			tempCusCode.OK_OA_PremisesAddress = ZGuid.Empty;

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			AssertEquals("Back to Address = Org Office Address", WhsBonded.PK, Loader.CurrentReceive.WD_WW_Whs);
		}

		#endregion

		#region TestImport_BondedWarehouse

		public void TestImport_NotBondedWarehouse()
		{
			TestImport_Warehouse(hasCCPCusCode: false, shouldImport: false);
		}

		public void TestImport_NotBondedWarehouse_WithCCPCusCode()
		{
			TestImport_Warehouse(hasCCPCusCode: true, shouldImport: false);
		}

		public void TestImport_BondedWarehouse()
		{
			TestImport_Warehouse(
				(whs) => whs.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded,
				hasCCPCusCode: false,
				shouldImport: true);
		}

		public void TestImport_BondedWarehouse_WithCCPCusCode()
		{
			TestImport_Warehouse(
				(whs) => whs.Areas[0].WA_AreaType = AreaTypes.Codes.Bonded,
				hasCCPCusCode: true,
				shouldImport: true);
		}

		public void TestImport_ExciseWarehouse()
		{
			TestImport_Warehouse(
				(whs) => whs.Areas[0].WA_AreaType = AreaTypes.Codes.Excise,
				hasCCPCusCode: false,
				shouldImport: true);
		}

		public void TestImport_ExciseWarehouse_WithCCPCusCode()
		{
			TestImport_Warehouse(
				(whs) => whs.Areas[0].WA_AreaType = AreaTypes.Codes.Excise,
				hasCCPCusCode: true,
				shouldImport: true);
		}

		void TestImport_Warehouse(bool hasCCPCusCode, bool shouldImport)
		{
			TestImport_Warehouse((whs) => { }, hasCCPCusCode, shouldImport);
		}

		void TestImport_Warehouse(Action<WhsWarehouse> modifyWhs, bool hasCCPCusCode, bool shouldImport)
		{
			WhsBonded.Delete();
			var someWarehouse = Helper.CreateWarehouse("SOMEWHS");
			modifyWhs(someWarehouse);

			if (hasCCPCusCode)
			{
				someWarehouse.WW_WarehouseName = "OTHERNAME";

				((OrgCusCode)(BondedClient.CustomsCodes.Find(new ZQuery(OrgCusCodeSchema.OK_CodeType, "CCP"))[0])).OK_CustomsRegNo = "SOMEWHS";
				someWarehouse.WW_OA_WarehouseAddress = BondedClient.Addresses.DefaultAddressOfType(OrgAddressType.Office, false).PK;
			}

			Factory.Save();

			ImportCsvData(TestImport_Bonded_Warehouse());
			AssertEquals(!shouldImport, Loader.ErrorOccurredInThisRun);

			if (shouldImport)
			{
				AssertEquals(someWarehouse.PK, Loader.CurrentReceive.WD_WW_Whs);
			}
		}

		string[,] TestImport_Bonded_Warehouse()
		{
			return new string[ReceiveHeader.ColumnCount, 3]

			//		Header					Line1			Line2
			//		------------------------------------------------------
			{
						{ "CustomsEntryNo",   "123456",   "123456" },
						{ "ClientCode",     "BONDCLI",    "BONDCLI" },
						{ "Warehouse",      "SOMEWHS",    "SOMEWHS" },
						{ "Reference",      "REF2000",    "REF2000" },
						{ "ArrivalDate",      "",       "" },
						{ "ProductCode",      "P01",      "P01" },
						{ "Quantity",     "10",     "25" },
						{ "QuantityUQ",     "",       "" },
						{ "Pallets",        "",       "" },
						{ "Location",     "",       "" },
						{ "Attribute1",     "",       "" },
						{ "Attribute2",     "",       "" },
						{ "Attribute3",     "",       "" },
						{ "ExpiryDate",     "",       "" },
						{ "PackingDate",      "",       "" },
						{ "CustomsEntryLineNo", "1",      "1" },
						{ "CustomsEntryDate", "",       "" },
						{ "CustomsAddInfo",   "",       "" },
						{ "CustomsQty",     "",       "" },
						{ "CustomsUQ",      "",       "" },
						{ "CtryOfOrigin",   "",       "" },
						{ "ValueForDuty",   "",       "" },
						{ "BondedWhsQty",   "",       "" },
						{ "BondedWhsUQ",      "",       "" },
						{ "TILV",       "",       "" },
						{ "CustomsSecondQuantity","",   "" },
						{ "CustomsSecondUnitQty","",  "" },
						{ "Tariff","",   "" },
						{ "PrimaryPreference","",   "" },
						{ "CustomsThirdQuantity","",   "" },
						{ "CustomsThirdUnitQty","",  "" },
						{ "ManufacturerCode",    "", "" },
						{ "ZoneStatus",        "" , "" },
						{ "IsFromOtherFTZWarehouse",        "N" , "N" },
						{ "OutwardType", "", "" },
						{ "SerialNumber", "", "" },
						{ "ReceiveCategory","",           "" }
				};
		}

		#endregion

		#region TestImport_DocketExistAppendLinesToIt

		[TestDate(2005, 1, 1)]
		public void TestImport_DocketExistAppendLinesToIt()
		{
			WhsReceive receive = Helper.CreateWhsReceive(FreeStoreClient, WhsFreeStore);
			receive.WD_ExternalReference = "REF2222";
			var line = Helper.CreateWhsReceiveInventoryLine(receive, Part1, 10m);

			Factory.Save();
			ImportCsvData(GetTestDataAppendingToExistingDocket());

			receive = LoadReceiveCreatedByImport("REF1111");
			Assert("Receive in DB", receive.IsInDatabase);
			AssertEquals("Num Of Lines", 2, receive.Lines.Count);

			receive = LoadReceiveCreatedByImport("REF2222");
			Assert("Receive in DB", receive.IsInDatabase);
			//AssertEquals("Num Of Lines", 3, Inwards.Lines.Count);
		}

		string[,] GetTestDataAppendingToExistingDocket()
		{
			return new string[ReceiveHeader.ColumnCount, 5]

			//	Header					Line1		Line2		Line3		Line4		
			//	------------------------------------------------------------------------
			{
					{ "CustomsEntryNo",   "",     "",     "",     "" },
					{ "ClientCode",     "CLIFREE",  "CLIFREE",  "CLIFREE",  "CLIFREE" },
					{ "Warehouse",      "WHSFREE",  "WHSFREE",  "WHSFREE",  "WHSFREE" },
					{ "Reference",      "REF1111",  "REF2222",  "REF1111",  "REF2222" },
					{ "ArrivalDate",      "20050505", "20050505", "20050505", "20050505" },
					{ "ProductCode",      "P01",    "P01",    "P01",    "P01" },
					{ "Quantity",     "10",   "10",   "10",   "10" },
					{ "QuantityUQ",     "KG",   "KG",   "KG",   "KG" },
					{ "Pallets",        "0001",   "0001",   "0001",   "0001" },
					{ "Location",     "A-1",    "A-1",    "A-1",    "A-1" },
					{ "Attribute1",     "A1",   "A1",   "A1",   "A1" },
					{ "Attribute2",     "A2",   "A2",   "A2",   "A2" },
					{ "Attribute3",     "A3",   "A3",   "A3",   "A3" },
					{ "ExpiryDate",     "20100101", "20100101", "20100101", "20100101" },
					{ "PackingDate",      "20010101", "20010101", "20010101", "20010101" },
					{ "CustomsEntryLineNo", "1",    "1",    "1",    "1" },
					{ "CustomsEntryDate", "20050506", "20050506", "20050506", "20050506" },
					{ "CustomsAddInfo",   "ADDINFO",  "ADDINFO",  "ADDINFO",  "ADDINFO" },
					{ "CustomsQty",     "20",   "20",   "20",   "20" },
					{ "CustomsUQ",      "KG",   "KG",   "KG",   "KG" },
					{ "CtryOfOrigin",   "AU",   "AU",   "AU",   "AU" },
					{ "ValueForDuty",   "1",    "1",    "1",    "1" },
					{ "BondedWhsQty",   "30",   "30",   "30",   "30" },
					{ "BondedWhsUQ",      "KG",   "KG",   "KG",   "KG" },
					{ "TILV",       "5.1",    "6.2",    "7.3",    "8.4" },
					{ "CustomsSecondQuantity",  "24",   "24", "24",   "24" },
					{ "CustomsSecondUnitQty","GRM",  "GRM","GRM",  "GRM" },
					{ "Tariff","TRF","TRF","TRF","TRF" },
					{ "PrimaryPreference","STANDARD","STANDARD","STANDARD","STANDARD" },
					{ "CustomsThirdQuantity",   "12",   "12",   "12",   "12" },
					{ "CustomsThirdUnitQty","GR",  "GR","GR",  "GR" },
					{ "ManufacturerCode",    "MANUFREE", "MANUFREE", "MANUFREE", "MANUFREE" },
					{ "ZoneStatus", "D" , "D", "D" , "D" },
					{ "IsFromOtherFTZWarehouse", "Y" , "Y" , "Y" , "Y" },
					{ "OutwardType", "CNN" , "CNN" , "CNN" , "CNN" },
					{ "SerialNumber", "SN" , "SN" , "SN" , "SN" },
					{ "ReceiveCategory",   "",     "",     "",     "" }
			};
		}

		#endregion

		#region TestImport_FreeStoreReceiveDataMapping

		[TestDate(2005, 1, 1)]
		public void TestImport_FreeStoreReceiveDataMapping()
		{
			WhsReceive receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestDataImportFreeStoreReceiveData());

			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Receive in DB", receive.IsInDatabase);

			AssertEquals("ClientCode", "CLIFREE", receive.Client.OH_Code);
			AssertEquals("Warehouse", "WHSFREE", receive.Warehouse.WW_WarehouseName);
			AssertEquals("Reference", "REF1005", receive.WD_ExternalReference);
			AssertEquals("ArrivalDate", new ZDateTimeOffset(2005, 05, 05, 0, 0, 0, TimeSpan.FromHours(10)), receive.WD_ArrivalDate);

			AssertEquals("ProductCode", "P01", receive.Lines[0].SupplierPart.OP_PartNum);
			AssertEquals("Quantity", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("QuantityUQ", "KG", receive.Lines[0].WE_F3_NKPackType);

			//Assert("Pallets", true);
			AssertEquals("Location", "A-1", receive.Lines[0].LocationString);
			AssertEquals("Attribute1", "A1", receive.Lines[0].WE_PartAttrib1);
			AssertEquals("Attribute2", "A2", receive.Lines[0].WE_PartAttrib2);
			AssertEquals("Attribute3", "A3", receive.Lines[0].WE_PartAttrib3);
			AssertEquals("SerialNumber", "SN", receive.Lines[0].WE_SerialNumber);
			AssertEquals("ExpiryDate", new ZDateTime(2010, 01, 01), receive.Lines[0].WE_ExpiryDate);
			AssertEquals("PackingDate", new ZDateTime(2001, 01, 01), receive.Lines[0].WE_PackingDate);
		}

		string[,] GetTestDataImportFreeStoreReceiveData()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		---------------------------------------
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "REF1005" },
						{ "ArrivalDate",      "20050505" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "KG" },
						{ "Pallets",        "0001" },
						{ "Location",     "A-1" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20100101" },
						{ "PackingDate",      "20010101" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO" },
						{ "CustomsQty",     "20" },
						{ "CustomsUQ",      "KG" },
						{ "CtryOfOrigin",   "AU" },
						{ "ValueForDuty",   "1" },
						{ "BondedWhsQty",   "30" },
						{ "BondedWhsUQ",      "KG" },
						{ "TILV",       "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",       "" }
			};
		}

		#endregion

		#region TestImport_FreeStoreWithMinimumData

		public void TestImport_FreeStoreWithMinimumData()
		{
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Precondition - No Receive", receive);

			Factory.Save();
			ImportCsvData(GetTestDataImportFreeStoreWithMinimumData());

			receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNotNull("Receive Created", receive);
			Assert("Receive in database", receive.IsInDatabase);
			AssertEquals("Receive.Lines.Count", 2, receive.Lines.Count);

			AssertEquals("ClientCode", "CLIFREE", receive.Client.OH_Code);
			AssertEquals("Warehouse", "WHSFREE", receive.Warehouse.WW_WarehouseName);
			AssertEquals("Reference", "INWARDS W00000001", receive.WD_ExternalReference);
			AssertEquals("ArrivalDate", ZDateTime.Empty, receive.WD_ArrivalDate.Date);

			AssertEquals("ProductCode", "P01", receive.Lines[0].SupplierPart.OP_PartNum);

			receive.Lines.ApplySort(WhsDocketLineSchema.Constants.WE_TransactionQuantity, ListSortDirection.Ascending);
			AssertEquals("Quantity", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("QuantityUQ", "KG", receive.Lines[0].WE_F3_NKPackType);

			//Assert("Pallets", true);
			AssertEquals("Location", "", receive.Lines[0].LocationString);
			AssertEquals("Attribute1", "", receive.Lines[0].WE_PartAttrib1);
			AssertEquals("Attribute2", "", receive.Lines[0].WE_PartAttrib2);
			AssertEquals("Attribute3", "", receive.Lines[0].WE_PartAttrib3);
			AssertEquals("SerialNumber", "", receive.Lines[0].WE_SerialNumber);
			AssertEquals("ExpiryDate", ZDateTime.Empty, receive.Lines[0].WE_ExpiryDate);
			AssertEquals("PackingDate", ZDateTime.Empty, receive.Lines[0].WE_PackingDate);
		}

		string[,] GetTestDataImportFreeStoreWithMinimumData()
		{
			return new string[ReceiveHeader.ColumnCount, 3]

			//		Header					Line1		Line2
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",   "",     "" },
					{ "ClientCode",     "CLIFREE",  "CLIFREE" },
					{ "Warehouse",      "WHSFREE",  "WHSFREE" },
					{ "Reference",      "",     "" },
					{ "ArrivalDate",      "",     "" },
					{ "ProductCode",      "P01",    "P01" },
					{ "Quantity",     "10",   "25" },
					{ "QuantityUQ",     "",     "" },
					{ "Pallets",        "",     "" },
					{ "Location",     "",     "" },
					{ "Attribute1",     "",     "" },
					{ "Attribute2",     "",     "" },
					{ "Attribute3",     "",     "" },
					{ "ExpiryDate",     "",     "" },
					{ "PackingDate",      "",     "" },
					{ "CustomsEntryLineNo", "",     "" },
					{ "CustomsEntryDate", "",     "" },
					{ "CustomsAddInfo",   "",     "" },
					{ "CustomsQty",     "",     "" },
					{ "CustomsUQ",      "",     "" },
					{ "CtryOfOrigin",   "",     "" },
					{ "ValueForDuty",   "",     "" },
					{ "BondedWhsQty",   "",     "" },
					{ "BondedWhsUQ",      "",     "" },
					{ "TILV",       "",     "" },
					{ "CustomsSecondQuantity","",   "" },
					{ "CustomsSecondUnitQty","",  "" },
					{ "Tariff","",   "" },
					{ "PrimaryPreference","",   "" },
					{ "CustomsThirdQuantity","",   "" },
					{ "CustomsThirdUnitQty","",  "" },
					{ "ManufacturerCode",    "MANUFREE", "MANUFREE" },
					{ "ZoneStatus", "" , "" },
					{ "IsFromOtherFTZWarehouse", "N" , "N" },
					{ "OutwardType", "", "" },
					{ "SerialNumber", "", "" },
					{ "ReceiveCategory","",           "" }
			};
		}

		#endregion

		#region TestImport_IgnoresReceiveUQOnProduct

		[TestDate(2005, 1, 1)]
		public void TestImport_IgnoresReceiveUQOnProduct()
		{
			Helper.CreateProductParamsByWhsAndClient(Part1, FreeStoreClient, WhsFreeStore, 0, 0, "PLT");
			Helper.CreateProductUnit(Part1, "KG", "PLT", 10);
			Factory.Save();

			AssertNull("Precondition - No Receive should exist.", LoadReceiveCreatedByImport("REF1005"));

			ImportCsvData(GetTestDataImportFreeStoreReceiveData());
			WhsReceive receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Receive in DB", receive.IsInDatabase);
			AssertEquals("KG", receive.Lines[0].WE_F3_NKPackType);
			AssertEquals(10.0m, receive.Lines[0].WE_PackQuantity);
		}

		#endregion

		#region TestImport_MoreThanOneDocket

		[TestDate(2005, 1, 1)]
		public void TestImport_MoreThanOneDocket()
		{
			WhsReceive receive = LoadReceiveCreatedByImport("REF1111");
			AssertNull("Precond - Docket Does Not Exist", receive);

			receive = LoadReceiveCreatedByImport("REF2222");
			AssertNull("Precond - Docket Does Not Exist", receive);

			receive = LoadReceiveCreatedByImport("REF3333");
			AssertNull("Precond - Docket Does Not Exist", receive);

			Factory.Save();
			ImportCsvData(GetTestDataMoreThanOneDocket());

			receive = LoadReceiveCreatedByImport("REF1111");
			Assert("Receive in DB", receive.IsInDatabase);
			AssertEquals("Num Of Lines", 2, receive.Lines.Count);

			receive = LoadReceiveCreatedByImport("REF2222");
			Assert("Receive in DB", receive.IsInDatabase);
			AssertEquals("Num Of Lines", 2, receive.Lines.Count);

			receive = LoadReceiveCreatedByImport("REF3333");
			Assert("Receive in DB", receive.IsInDatabase);
			AssertEquals("Num Of Lines", 1, receive.Lines.Count);
		}

		string[,] GetTestDataMoreThanOneDocket()
		{
			return new string[ReceiveHeader.ColumnCount, 6]

			//		Header					Line1		Line2		Line3		Line4		Line 5
			//		-----------------------------------------------------------------------------------
			{
						{ "CustomsEntryNo",   "",     "",     "20050506", "",     "" },
						{ "ClientCode",     "CLIFREE",  "CLIFREE",  "BONDCLI",  "CLIFREE",  "CLIFREE" },
						{ "Warehouse",      "WHSFREE",  "WHSFREE",  "BONDWHS",  "WHSFREE",  "WHSFREE" },
						{ "Reference",      "REF1111",  "REF2222",  "REF3333",  "REF2222",  "REF1111" },
						{ "ArrivalDate",      "20050505", "20050505", "20050505", "20050505", "20050505" },
						{ "ProductCode",      "P01",    "P01",    "P01",    "P01",    "P01" },
						{ "Quantity",     "10",   "10",   "10",   "10",   "10" },
						{ "QuantityUQ",     "KG",   "KG",   "KG",   "KG",   "KG" },
						{ "Pallets",        "0001",   "0001",   "0001",   "0001",   "0001" },
						{ "Location",     "A-1",    "A-1",    "BOND",   "A-1",    "A-1" },
						{ "Attribute1",     "A1",   "A1",   "A1",   "A1",   "A1" },
						{ "Attribute2",     "A2",   "A2",   "A2",   "A2",   "A2" },
						{ "Attribute3",     "A3",   "A3",   "A3",   "A3",   "A3" },
						{ "ExpiryDate",     "20100101", "20100101", "20100101", "20100101", "20100101" },
						{ "PackingDate",      "20010101", "20010101", "20010101", "20010101", "20010101" },
						{ "CustomsEntryLineNo", "1",    "1",    "1",    "1",    "1" },
						{ "CustomsEntryDate", "20050506", "20050506", "20050506", "20050506", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO",  "ADDINFO",  "ADDINFO",  "ADDINFO",  "ADDINFO" },
						{ "CustomsQty",     "20",   "20",   "20",   "20",   "20" },
						{ "CustomsUQ",      "KG",   "KG",   "KG",   "KG",   "KG" },
						{ "CtryOfOrigin",   "AU",   "AU",   "AU",   "AU",   "AU" },
						{ "ValueForDuty",   "1",    "1",    "1",    "1",    "1" },
						{ "BondedWhsQty",   "30",   "30",   "30",   "30",   "30" },
						{ "BondedWhsUQ",      "KG",   "KG",   "KG",   "KG",   "KG" },
						{ "TILV",       "5.1",    "6.2",    "7.3",    "8.4",    "9.5" },
						{ "CustomsSecondQuantity", "24",   "24", "24",   "24",   "24" },
						{ "CustomsSecondUnitQty","GRM",  "GRM","GRM",  "GRM",  "GRM" },
						{ "Tariff","TRF","TRF","TRF","TRF","TRF" },
						{ "PrimaryPreference","STANDARD","STANDARD","STANDARD","STANDARD","STANDARD" },
						{ "CustomsThirdQuantity", "12",   "12", "12",   "12",   "12" },
						{ "CustomsThirdUnitQty","GR",  "GR","GR",  "GR",  "GR" },
						{ "ManufacturerCode",    "MANUFREE", "MANUFREE", "MANUFREE", "MANUFREE", "MANUFREE" },
						{ "ZoneStatus", "D" , "D", "D" , "D", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" , "Y" , "Y" , "Y", "Y" },
						{ "OutwardType", "CNN" , "CNN" , "CNN" , "CNN", "CNN" },
						{ "SerialNumber", "SN" , "SN" , "SN" , "SN", "SN" },
						{ "ReceiveCategory", "",        "",         "",         "",         "" }
			};
		}

		#endregion

		#region TestImport_CreatesProductRelation

		#region TestImport_CreatesProductRelation_NoRelationExist

		public void TestImport_CreatesProductRelation_WithNoRelationExist()
		{
			var clientA = CreateClient("CA", WhsPickMode.Codes.AttributeSpecified);
			var clientB = CreateClient("CB", WhsPickMode.Codes.AttributeSpecified);
			var productClientA = CreateAndSetupProductRelation(clientA, "W1", "ZP01", OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			AssertClientProductRelation("Only client 'CA' should be in RelatedOrganization list for product 'ZP01'", 1, productClientA, new[] { OrgPartRelation.RelationshipTypes.Owner }, clientA.PK);
			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ImportCsvData(GetTestReceiveDataNoClientProductRelation("W1", "CB", "ZP01", "A1"));
				var receive = LoadReceiveCreatedByImport(new BusinessObjectFactory(), "INWARDS W00000001");

				AssertNotNull("Receive should have created by Import.", receive);
				AssertClientProductRelation("Client - Product relation should be created for Client 'CB' during import.", 2, receive.Lines[0].SupplierPart,
					new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Owner },
					clientA.PK, clientB.PK);
				AssertRelationProperties(clientB, receive, OrgPartRelation.RelationshipTypes.Owner);
			}
		}

		#endregion

		#region TestImport_DoesNotCreateProductRelation_WithSupplierRelation

		public void TestImport_DoesNotCreateProductRelation_WithSupplierRelation()
		{
			var clientA = CreateClient("CA", WhsPickMode.Codes.AttributeSpecified);
			var productClientA = CreateAndSetupProductRelation(clientA, "W1", "ZP01", OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();

			AssertClientProductRelation("Client 'CA' should be in RelatedOrganization list for product 'ZP01'", 1, productClientA, new[] { OrgPartRelation.RelationshipTypes.Supplier }, clientA.PK);
			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ImportCsvData(GetTestReceiveDataNoClientProductRelation("W1", "CA", "ZP01", "A1"));
				var receive = LoadReceiveCreatedByImport(new BusinessObjectFactory(), "INWARDS W00000001");

				AssertNotNull("Receive should have created by Import.", receive);
				AssertClientProductRelation("Client - Product relation should be updated to 'Both' for Client 'CA' during import.", 1, receive.Lines[0].SupplierPart, new[] { OrgPartRelation.RelationshipTypes.Both }, clientA.PK);
				AssertRelationProperties(clientA, receive, OrgPartRelation.RelationshipTypes.Both);
			}
		}

		#endregion

		#region TestImport_CreatesProductRelation_WithWarehouseConsigneeRelation

		public void TestImport_CreatesProductRelation_WithWarehouseConsigneeRelation()
		{
			var clientA = CreateClient("CA", WhsPickMode.Codes.AttributeSpecified);
			var productClientA = CreateAndSetupProductRelation(clientA, "W1", "ZP01", OrgPartRelation.RelationshipTypes.WarehouseConsignee);
			Factory.Save();

			AssertClientProductRelation("Client 'CA' should be in RelatedOrganization list for product 'ZP01'", 1, productClientA, new[] { OrgPartRelation.RelationshipTypes.WarehouseConsignee }, clientA.PK);
			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ImportCsvData(GetTestReceiveDataNoClientProductRelation("W1", "CA", "ZP01", "A1"));
				var receive = LoadReceiveCreatedByImport(new BusinessObjectFactory(), "INWARDS W00000001");

				AssertNotNull("Receive should have created by Import.", receive);
				AssertClientProductRelation("Client - Product relation should be created for Client 'CA' during import.", 2, receive.Lines[0].SupplierPart,
					new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.WarehouseConsignee },
					clientA.PK, clientA.PK);
				AssertRelationProperties(clientA, receive, OrgPartRelation.RelationshipTypes.Owner);
			}
		}

		#endregion

		#region TestImport_DoesNotCreateProductRelation_WithBothRelation

		public void TestImport_DoesNotCreateProductRelation_WithBothRelation()
		{
			var clientA = CreateClient("CA", WhsPickMode.Codes.AttributeSpecified);
			var productClientA = CreateAndSetupProductRelation(clientA, "W1", "ZP01", OrgPartRelation.RelationshipTypes.Both);
			Factory.Save();

			AssertClientProductRelation("Client 'CA' should be in RelatedOrganization list for product 'ZP01'", 1, productClientA, new[] { OrgPartRelation.RelationshipTypes.Both }, clientA.PK);
			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ImportCsvData(GetTestReceiveDataNoClientProductRelation("W1", "CA", "ZP01", "A1"));
				var receive = LoadReceiveCreatedByImport(new BusinessObjectFactory(), "INWARDS W00000001");

				AssertNotNull("Receive should have created by Import.", receive);
				AssertClientProductRelation("Client - Product relation should not be created for Client 'CA' during import.", 1, receive.Lines[0].SupplierPart, new[] { OrgPartRelation.RelationshipTypes.Both }, clientA.PK);
				AssertRelationProperties(clientA, receive, OrgPartRelation.RelationshipTypes.Both);
			}
		}

		#endregion

		#region TestImport_DoesNotCreateProductRelation_WithOwnerRelation

		public void TestImport_DoesNotCreateProductRelation_WithOwnerRelation()
		{
			var clientA = CreateClient("CA", WhsPickMode.Codes.AttributeSpecified);
			var productClientA = CreateAndSetupProductRelation(clientA, "W1", "ZP01", OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			AssertClientProductRelation("Client 'CA' should be in RelatedOrganization list for product 'ZP01'", 1, productClientA, new[] { OrgPartRelation.RelationshipTypes.Owner }, clientA.PK);
			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ImportCsvData(GetTestReceiveDataNoClientProductRelation("W1", "CA", "ZP01", "A1"));
				var receive = LoadReceiveCreatedByImport(new BusinessObjectFactory(), "INWARDS W00000001");

				AssertNotNull("Receive should have created by Import.", receive);
				AssertClientProductRelation("Client - Product relation should not be created for Client 'CA' during import.", 1, receive.Lines[0].SupplierPart, new[] { OrgPartRelation.RelationshipTypes.Owner }, clientA.PK);
				AssertRelationProperties(clientA, receive, OrgPartRelation.RelationshipTypes.Owner);
			}
		}

		#endregion

		#region Helper

		static void AssertRelationProperties(OrgHeader client, WhsReceive receive, string relationType)
		{
			var relation = receive.Lines[0].SupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().Single(r => r.OU_OH == client.PK && r.OU_Relationship == relationType);
			AssertEquals("OU_UsePartAttrib1 should return true", true, relation.OU_UsePartAttrib1);
			AssertEquals("OU_UsePartAttrib2 should return false", false, relation.OU_UsePartAttrib2);
			AssertEquals("OU_UsePartAttrib3 should return false", false, relation.OU_UsePartAttrib3);
			AssertEquals("OU_UseSerialNumber should return false", false, relation.OU_UseSerialNumber);
			AssertEquals("OU_UseExpiryDate should return true", true, relation.OU_UseExpiryDate);
			AssertEquals("OU_UsePackingDate should return true", true, relation.OU_UsePackingDate);
			AssertEquals("OU_PickMode should return 'ASP'", WhsPickMode.Codes.AttributeSpecified, relation.OU_PickMode);
		}

		static void AssertClientProductRelation(string message, int relatedOrgCount, OrgSupplierPart part, string[] relationTypes, params ZGuid[] clientPKs)
		{
			var relatedOrganisations = part.RelatedOrganisations;
			AssertEquals(message, relatedOrgCount, relatedOrganisations.Count);
			AssertContainsExactElementsInAnyOrder(clientPKs, relatedOrganisations.Cast<OrgPartRelation>().Select(r => r.OU_OH));
			AssertContainsExactElementsInAnyOrder(relationTypes, relatedOrganisations.Cast<OrgPartRelation>().Select(r => r.OU_Relationship));
		}

		OrgSupplierPart CreateAndSetupProductRelation(OrgHeader client, string warehouseName, string productCode, string relationType)
		{
			var warehouse = Helper.CreateWarehouse(warehouseName, "A", 1, 1);
			var product = Helper.CreateProduct(client, productCode);

			var relation = product.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation.OU_UsePartAttrib1 = true;
			if (relationType != OrgPartRelation.RelationshipTypes.Owner)
			{
				relation.OU_Relationship = relationType;
			}

			return product;
		}

		OrgHeader CreateClient(string clientCode, string defaultPickMode)
		{
			var client = Helper.CreateClient(clientCode, clientCode);
			var miscServ = client.MiscServ;
			miscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			miscServ.OM_IMPartAttrib1Name = "Color";
			miscServ.OM_WhsDefaultWarehousePickMode = defaultPickMode;

			return client;
		}

		string[,] GetTestReceiveDataNoClientProductRelation(string warehouseCode, string clientCode, string productCode, string attribute1)
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
						{ "CustomsEntryNo",     "" },
						{ "ClientCode",         clientCode },
						{ "Warehouse",            warehouseCode },
						{ "Reference",            "" },
						{ "ArrivalDate",          "" },
						{ "ProductCode",          productCode },
						{ "Quantity",           "10" },
						{ "QuantityUQ",         "" },
						{ "Pallets",              "" },
						{ "Location",           "" },
						{ "Attribute1",         attribute1 },
						{ "Attribute2",         "" },
						{ "Attribute3",         "" },
						{ "ExpiryDate",         "20160101" },
						{ "PackingDate",          "20150101" },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate",   "" },
						{ "CustomsAddInfo",     "" },
						{ "CustomsQty",         "" },
						{ "CustomsUQ",            "" },
						{ "CtryOfOrigin",       "" },
						{ "ValueForDuty",       "" },
						{ "BondedWhsQty",       "" },
						{ "BondedWhsUQ",          "" },
						{ "TILV",               "" },
						{ "CustomsSecondQuantity",  "" },
						{ "CustomsSecondUnitQty", "" },
						{ "Tariff",            "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity",   "" },
						{ "CustomsThirdUnitQty",    "" },
						{ "ManufacturerCode",    "" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "" },
						{ "ReceiveCategory",      "" }
			};
		}

		#endregion

		#endregion

		#region TestImport_PartAttributesAreSetupForClientAndPartWhenNoExistingInventory

		public void TestImport_PartAttributesAreSetupForClientAndPartWhenNoExistingInventory()
		{
			Factory.Save();
			ImportCsvData(GetTestPartAttributeSettings());
			WhsReceive receive = LoadReceiveCreatedByImport(new BusinessObjectFactory(), "INWARDS W00000001");

			// all following asserts are pretty much preconditions
			AssertNotNull("Receive Created", receive);
			Assert("Receive in database", receive.IsInDatabase);
			AssertEquals("Receive.Lines.Count", 2, receive.Lines.Count);
			AssertEquals("ClientCode", "CLIFREE", receive.Client.OH_Code);
			AssertEquals("Warehouse", "WHSFREE", receive.Warehouse.WW_WarehouseName);

			receive.Lines.ApplySort(new ProductSorter());
			AssertEquals("ProductCode", "P01", receive.Lines[0].SupplierPart.OP_PartNum);
			AssertEquals("ProductCode", "P02", receive.Lines[1].SupplierPart.OP_PartNum);
			AssertEquals("Quantity", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("Quantity", 25m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("Attribute1", "A1", receive.Lines[0].WE_PartAttrib1);
			AssertEquals("Attribute2", "A2", receive.Lines[0].WE_PartAttrib2);
			AssertEquals("Attribute3", "A3", receive.Lines[0].WE_PartAttrib3);
			AssertEquals("SerialNumber", "SN", receive.Lines[0].WE_SerialNumber);
			AssertEquals("ExpiryDate", new ZDateTime(2010, 01, 01), receive.Lines[0].WE_ExpiryDate);
			AssertEquals("PackingDate", new ZDateTime(2001, 01, 01), receive.Lines[0].WE_PackingDate);
			AssertEquals("Attribute1", "", receive.Lines[1].WE_PartAttrib1);
			AssertEquals("Attribute2", "A2", receive.Lines[1].WE_PartAttrib2);
			AssertEquals("Attribute3", "", receive.Lines[1].WE_PartAttrib3);
			AssertEquals("SerialNumber", "", receive.Lines[1].WE_SerialNumber);
			AssertEquals("ExpiryDate", ZDateTime.Empty, receive.Lines[1].WE_ExpiryDate);
			AssertEquals("PackingDate", ZDateTime.Empty, receive.Lines[1].WE_PackingDate);

			// now the real tests

			// organisation level settings
			AssertEquals("Attribute1 should be used by organisation", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByOrganisation(1));
			AssertEquals("Attribute2 should be used by organisation", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByOrganisation(2));
			AssertEquals("Attribute3 should be used by organisation", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByOrganisation(3));
			AssertEquals("Serial Number should be used by organisation", true, receive.Client.PartAttributeManager.IsSerialNumberUsedByOrganisation);
			AssertEquals("Expiry Date should be used by organisation", true, receive.Client.PartAttributeManager.IsExpiryDateUsedByOrganisation);
			AssertEquals("Packing Date should be used by organisation", true, receive.Client.PartAttributeManager.IsPackingDateUsedByOrganisation);

			AssertEquals("MiscServ.Attribute1 should be named 'Attribute 1'", "Attribute 1", receive.Client.MiscServ.OM_IMPartAttrib1Name);
			AssertEquals("MiscServ.Attribute2 should be named 'Attribute 2'", "Attribute 2", receive.Client.MiscServ.OM_IMPartAttrib2Name);
			AssertEquals("MiscServ.Attribute3 should be named 'Attribute 3'", "Attribute 3", receive.Client.MiscServ.OM_IMPartAttrib3Name);
			AssertEquals("MiscServ.Attribute1 should be typed Non Mandatory", PartAttributeTypeList.Codes.NonMandatory, receive.Client.MiscServ.OM_IMPartAttrib1Type);
			AssertEquals("MiscServ.Attribute2 should be typed Non Mandatory", PartAttributeTypeList.Codes.NonMandatory, receive.Client.MiscServ.OM_IMPartAttrib2Type);
			AssertEquals("MiscServ.Attribute3 should be typed Non Mandatory", PartAttributeTypeList.Codes.NonMandatory, receive.Client.MiscServ.OM_IMPartAttrib3Type);

			// product level settings
			AssertEquals("Attribute1 should be used by product", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByProduct(receive.Lines[0].SupplierPart, 1));
			AssertEquals("Attribute2 should be used by product", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByProduct(receive.Lines[0].SupplierPart, 2));
			AssertEquals("Attribute3 should be used by product", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByProduct(receive.Lines[0].SupplierPart, 3));
			AssertEquals("Serial Number should be used by product", true, receive.Client.PartAttributeManager.IsSerialNumberUsedByProduct(receive.Lines[0].SupplierPart));
			AssertEquals("Expiry Date should be used by product", true, receive.Client.PartAttributeManager.IsExpiryDateUsedByProduct(receive.Lines[0].SupplierPart));
			AssertEquals("Packing Date should be used by product", true, receive.Client.PartAttributeManager.IsPackingDateUsedByProduct(receive.Lines[0].SupplierPart));

			AssertEquals("Attribute1 should NOT be used by product", false, receive.Client.PartAttributeManager.IsPartAttributeUsedByProduct(receive.Lines[1].SupplierPart, 1));
			AssertEquals("Attribute2 should be used by product", true, receive.Client.PartAttributeManager.IsPartAttributeUsedByProduct(receive.Lines[1].SupplierPart, 2));
			AssertEquals("Attribute3 should NOT be used by product", false, receive.Client.PartAttributeManager.IsPartAttributeUsedByProduct(receive.Lines[1].SupplierPart, 3));
			AssertEquals("Serial Number should NOT be used by product", false, receive.Client.PartAttributeManager.IsSerialNumberUsedByProduct(receive.Lines[1].SupplierPart));
			AssertEquals("Expiry Date should NOT be used by product", false, receive.Client.PartAttributeManager.IsExpiryDateUsedByProduct(receive.Lines[1].SupplierPart));
			AssertEquals("Packing Date should NOT be used by product", false, receive.Client.PartAttributeManager.IsPackingDateUsedByProduct(receive.Lines[1].SupplierPart));
		}

		string[,] GetTestPartAttributeSettings()
		{
			return new string[ReceiveHeader.ColumnCount, 3]

			//		Header					Line1			Line2
			//		------------------------------------------------------
			{
						{ "CustomsEntryNo",   "",     "" },
						{ "ClientCode",     "CLIFREE",  "CLIFREE" },
						{ "Warehouse",      "WHSFREE",  "WHSFREE" },
						{ "Reference",      "",     "" },
						{ "ArrivalDate",      "",     "" },
						{ "ProductCode",      "P01",    "P02" },
						{ "Quantity",     "10",   "25" },
						{ "QuantityUQ",     "",     "" },
						{ "Pallets",        "",     "" },
						{ "Location",     "",     "" },
						{ "Attribute1",     "A1",   "" },
						{ "Attribute2",     "A2",   "A2" },
						{ "Attribute3",     "A3",   "" },
						{ "ExpiryDate",     "20100101", "" },
						{ "PackingDate",      "20010101", "" },
						{ "CustomsEntryLineNo", "",     "" },
						{ "CustomsEntryDate", "",     "" },
						{ "CustomsAddInfo",   "",     "" },
						{ "CustomsQty",     "",     "" },
						{ "CustomsUQ",      "",     "" },
						{ "CtryOfOrigin",   "",     "" },
						{ "ValueForDuty",   "",     "" },
						{ "BondedWhsQty",   "",     "" },
						{ "BondedWhsUQ",      "",     "" },
						{ "TILV",       "",     "" },
						{ "CustomsSecondQuantity","",   "" },
						{ "CustomsSecondUnitQty","",  "" },
						{ "Tariff","",   "" },
						{ "PrimaryPreference","",   "" },
						{ "CustomsThirdQuantity","",   "" },
						{ "CustomsThirdUnitQty","",  "" },
						{ "ManufacturerCode",    "", "" },
						{ "ZoneStatus", "" , "" },
						{ "IsFromOtherFTZWarehouse", "N" , "N" },
						{ "OutwardType", "", "" },
						{ "SerialNumber", "SN", "" },
						{ "ReceiveCategory","",     "" }
			};
		}

		#endregion

		#region TestImport_WhenOneRecordFails

		[TestDate(2005, 1, 1)]
		public void TestImport_WhenOneRecordFails()
		{
			WhsReceive receive = LoadReceiveCreatedByImport("REF1111");
			AssertNull("Precond - Docket Does Not Exist", receive);

			receive = LoadReceiveCreatedByImport("REF2222");
			AssertNull("Precond - Docket Does Not Exist", receive);

			receive = LoadReceiveCreatedByImport("REF3333");
			AssertNull("Precond - Docket Does Not Exist", receive);

			Factory.Save();
			ImportCsvData(GetTestWhenOneRecordFails());

			Assert("Run Failed", Loader.ErrorOccurredInThisRun);

			receive = LoadReceiveCreatedByImport("REF1111");
			AssertNull("No Records Created", receive);

			receive = LoadReceiveCreatedByImport("REF2222");
			AssertNull("No Records Created", receive);

			receive = LoadReceiveCreatedByImport("REF3333");
			AssertNull("No Records Created", receive);
		}

		string[,] GetTestWhenOneRecordFails()
		{
			return new string[ReceiveHeader.ColumnCount, 6]

			//		Header					Line1		Line2		Line3		Line4		Line 5
			//		-----------------------------------------------------------------------------------
			{
						{ "CustomsEntryNo",   "",     "",     "20050506", "",     "" },
						{ "ClientCode",     "CLIFREE",  "",     "CLIFREE",  "CLIFREE",  "CLIFREE" },
						{ "Warehouse",      "WHSFREE",  "WHSFREE",  "BONDWHS",  "WHSFREE",  "WHSFREE" },
						{ "Reference",      "REF1111",  "REF2222",  "REF3333",  "REF2222",  "REF1111" },
						{ "ArrivalDate",      "20050505", "20050505", "20050505", "20050505", "20050505" },
						{ "ProductCode",      "P01",    "P01",    "P01",    "P01",    "P01" },
						{ "Quantity",     "10",   "10",   "10",   "10",   "10" },
						{ "QuantityUQ",     "KG",   "KG",   "KG",   "KG",   "KG" },
						{ "Pallets",        "0001",   "0001",   "0001",   "0001",   "0001" },
						{ "Location",     "A-1",    "A-1",    "BOND",   "A-1",    "A-1" },
						{ "Attribute1",     "A1",   "A1",   "A1",   "A1",   "A1" },
						{ "Attribute2",     "A2",   "A2",   "A2",   "A2",   "A2" },
						{ "Attribute3",     "A3",   "A3",   "A3",   "A3",   "A3" },
						{ "ExpiryDate",     "20100101", "20100101", "20100101", "20100101", "20100101" },
						{ "PackingDate",      "20010101", "20010101", "20010101", "20010101", "20010101" },
						{ "CustomsEntryLineNo", "1",    "1",    "1",    "1",    "1" },
						{ "CustomsEntryDate", "20050506", "20050506", "20050506", "20050506", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO",  "ADDINFO",  "ADDINFO",  "ADDINFO",  "ADDINFO" },
						{ "CustomsQty",     "20",   "20",   "20",   "20",   "20" },
						{ "CustomsUQ",      "KG",   "KG",   "KG",   "KG",   "KG" },
						{ "CtryOfOrigin",   "AU",   "AU",   "AU",   "AU",   "AU" },
						{ "ValueForDuty",   "1",    "1",    "1",    "1",    "1" },
						{ "BondedWhsQty",   "30",   "30",   "30",   "30",   "30" },
						{ "BondedWhsUQ",      "KG",   "KG",   "KG",   "KG",   "KG" },
						{ "TILV",       "5.1",    "6.2",    "7.3",    "8.4",    "9.5" },
						{ "CustomsSecondQuantity", "24",   "24", "24",   "24",   "24" },
						{ "CustomsSecondUnitQty","GRM",  "GRM","GRM",  "GRM",  "GRM" },
						{ "Tariff","TRF","TRF","TRF","TRF","TRF" },
						{ "PrimaryPreference","STANDARD","STANDARD","STANDARD","STANDARD","STANDARD" },
						{ "CustomsThirdQuantity", "12",   "12", "12",   "12",   "12" },
						{ "CustomsThirdUnitQty","GR",  "GR","GR",  "GR",  "GR" },
						{ "ManufacturerCode",    "MANUFREE", "MANUFREE", "MANUFREE", "MANUFREE", "MANUFREE" },
						{ "ZoneStatus", "D" , "D", "D" , "D", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" , "Y" , "Y" , "Y", "Y" },
						{ "OutwardType", "CNN" , "CNN" , "CNN" , "CNN", "CNN" },
						{ "SerialNumber", "SN" , "SN" , "SN" , "SN", "SN" },
						{ "ReceiveCategory", "",        "",         "",         "",         "" }
			};
		}

		#endregion

		#region TestImport_WithLargeValueOnProductMasterFile

		public void TestImport_WithLargeVolumeOnProductMasterFile()
		{
			TestImport_WithLargeValueOnProductMasterFile("Calculated Total Line Volume exceeded the valid length.", (product) => product.OP_Cubic = 50000);
		}

		public void TestImport_WithLargeWeightOnProductMasterFile()
		{
			TestImport_WithLargeValueOnProductMasterFile("Calculated Total Line Weight exceeded the valid length.", (product) => product.OP_Weight = 50000);
		}

		[TestDate(2005, 1, 1)]
		void TestImport_WithLargeValueOnProductMasterFile(string expectedErrorMessage, Action<OrgSupplierPart> setValue)
		{
			var product = Helper.CreateProduct(FreeStoreClient, "DODGYBIGPRODUCT");
			setValue(product);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportFreeStoreReceiveDataForLargeProduct()));
			AssertLogContainsErrorMessage(expectedErrorMessage);
			AssertNull("Should not have created receive.", LoadReceiveCreatedByImport("REF1005"));
		}

		string[,] GetTestDataImportFreeStoreReceiveDataForLargeProduct()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		---------------------------------------
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "REF1005" },
						{ "ArrivalDate",      "20050505" },
						{ "ProductCode",      "DODGYBIGPRODUCT" },
						{ "Quantity",     "100" },
						{ "QuantityUQ",     "UNT" },
						{ "Pallets",        "0001" },
						{ "Location",     "A-1" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20100101" },
						{ "PackingDate",      "20010101" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO" },
						{ "CustomsQty",     "20" },
						{ "CustomsUQ",      "KG" },
						{ "CtryOfOrigin",   "AU" },
						{ "ValueForDuty",   "1" },
						{ "BondedWhsQty",   "30" },
						{ "BondedWhsUQ",      "KG" },
						{ "TILV",       "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "CNN" },
						{ "ReceiveCategory",       "" }
			};
		}

		#endregion

		#region TestImport_WithWrongDateFormat

		public void TestImport_WithWrongDateFormat()
		{
			ImportCsvData(GetDataWithWrongDateFormat());
			AssertLogContainsErrorMessage_DateFormat("Arrival Date");
			AssertLogContainsErrorMessage_DateFormat("Expiry Date");
			AssertLogContainsErrorMessage_DateFormat("Packing Date");
			AssertLogContainsErrorMessage_DateFormat("Customs Entry Date");
		}

		void AssertLogContainsErrorMessage_DateFormat(string dateName)
		{
			AssertLogContainsErrorMessage(string.Format(string.Format("Row 2 Error....Wrong format of {0}. Date format should be 'yyyymmdd'. Please check the documentation.", dateName)));
		}

		void AssertLogContainsErrorMessage(string errorMessage)
		{
			bool result = false;
			foreach (string currentString in Loader.Log)
			{
				if (currentString.Equals(errorMessage))
				{
					result = true;
					break;
				}
			}
			AssertEquals("Should contain error message:", true, result);
		}

		string[,] GetDataWithWrongDateFormat()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "" },
						{ "ArrivalDate",      "08042007" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "" },
						{ "Pallets",        "" },
						{ "Location",     "" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "15012004" },
						{ "PackingDate",      "16012004" },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate", "17012004" },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "" },
						{ "ReceiveCategory","" }
				};
		}

		#endregion

		#region TestDateFormat

		public void TestDateFormat()
		{
			var property = typeof(WhsReceiveDataLoad).GetProperty("DateFormat", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			AssertEquals(ExpectedDateFormat, property.GetValue(GetNewDataLoader()));
		}

		string ExpectedDateFormat
		{
			get { return "yyyyMMdd"; }
		}

		#endregion

		#region TestImport_WithOutOfRangeDate

		public void TestImport_WithOutOfRangeDate()
		{
			ImportCsvData(GetDataWithOutOfRangeDate());
			AssertLogContainsErrorMessage_OutOfRangeDate("Arrival Date");
			AssertLogContainsErrorMessage_OutOfRangeDate("Expiry Date");
			AssertLogContainsErrorMessage_OutOfRangeDate("Packing Date");
			AssertLogContainsErrorMessage_OutOfRangeDate("Customs Entry Date");
		}

		void AssertLogContainsErrorMessage_OutOfRangeDate(string dateName)
		{
			AssertLogContainsErrorMessage(string.Format("Row 2 Error....{0} is out of date range. Date should be between '{1}' and '{2}'.", dateName, ZDateTime.MinSmallDateTimeValue.ToString(ExpectedDateFormat), ZDateTime.MaxSmallDateTimeValue.ToString(ExpectedDateFormat)));
		}

		string[,] GetDataWithOutOfRangeDate()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "" },
						{ "ArrivalDate",      ZDateTime.MaxSmallDateTimeValue.AddDays(1).ToString(ExpectedDateFormat) },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "" },
						{ "Pallets",        "" },
						{ "Location",     "" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     ZDateTime.MaxSmallDateTimeValue.AddDays(1).ToString(ExpectedDateFormat) },
						{ "PackingDate",      ZDateTime.MinSmallDateTimeValue.AddDays(-1).ToString(ExpectedDateFormat) },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate", ZDateTime.MinSmallDateTimeValue.AddDays(-1).ToString(ExpectedDateFormat) },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory","" }
			};
		}

		#endregion

		#region TestImport_WithNonExistingProducts

		public void TestImport_WithNonExistingProducts_SettingAllowProductFalse()
		{
			Helper.CreateProductParamsByWhsAndClient(Part1, FreeStoreClient, WhsFreeStore, 0, 0, "PLT");
			//FreeStoreClient doesn't have any product related attributes set on it.

			Factory.Save();

			// Test the import with allow product false 

			SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false); // Doesnot allow create new product
			ImportCsvData(GetDataWithNonExistingProduct());
			AssertCollectionContains("Row 2 Error.... product code (BUP-0010) could not be found.", Loader.Log);
		}

		public void TestImport_WithNonExistingProducts_SettingAllowProductTrue()
		{
			Helper.CreateProductParamsByWhsAndClient(Part1, FreeStoreClient, WhsFreeStore, 0, 0, "PLT");
			//FreeStoreClient doesn't have any product related attributes set on it.

			Factory.Save();

			// Test the import with allow product true
			SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true); // Allow create new product
			ImportCsvData(GetDataWithNonExistingProduct());
			AssertCollectionNotContains("Row 2 Error.... product code (BUP-0010) could not be found.", Loader.Log);
			WhsReceive receive = LoadReceiveCreatedByImport("REF1111");
			AssertNotNull("Receive should be created", receive);
		}

		public void TestImport_WithNonExistingProducts_SettingAllowProductTrueAndEmptyProductCode()
		{
			// test the import with allow product ture and empty product code.
			SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true); // Allow create new product
			ImportCsvData(GetDataWithBlankProductCode());
			AssertCollectionContains("Row 2 Error.... product code () could not be found.", Loader.Log);
		}

		string[,] GetDataWithNonExistingProduct()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "REF1111" },
						{ "ArrivalDate",      "20120115" },
						{ "ProductCode",      "BUP-0010" }, // product code doesnot exist
						{ "Quantity",     "1" },
						{ "QuantityUQ",     "CTN" },
						{ "Pallets",        "" },
						{ "Location",     "A-1" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20120130" },
						{ "PackingDate",      "20120115" },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate", "" },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory","" }
			};
		}

		string[,] GetDataWithBlankProductCode()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "REF1111" },
						{ "ArrivalDate",      "20120115" },
						{ "ProductCode",      "" }, // product code is empty
						{ "Quantity",     "1" },
						{ "QuantityUQ",     "CTN" },
						{ "Pallets",        "" },
						{ "Location",     "A-1" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20120130" },
						{ "PackingDate",      "20120115" },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate", "" },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",       "" }
			};
		}

		#endregion

		#region TestImport_FactorySaveException

		public void TestImport_FactorySaveException()
		{
			var location = WhsFreeStore.DefaultLocation;
			location.WLV_MaxQuantity = 20;
			Factory.Save();

			AssertNoExceptionThrown("Exception is caught.", () => ImportCsvData(GetTestDataImportFreeStoreWithInvalidQuantity(location.WLV_LocationString))); // factory save exception due to OverFlowTrigger for WE_TransactionQuantity
			AssertLogContainsErrorMessage("An exception occurred while saving the record(s) in the database.");
			AssertLogContainsErrorMessage("Attempt to overflow the location unit capacity.");
			AssertLogContainsErrorMessage("Please correct the relevant data in the file and then close and open the form to try again.");
			AssertLogContainsErrorMessage("Saving Failed.");
		}

		string[,] GetTestDataImportFreeStoreWithInvalidQuantity(string locationString)
		{
			return new string[ReceiveHeader.ColumnCount, 3]

				//		Header					Line1		Line2
				//		------------------------------------------------------
				{
						{ "CustomsEntryNo",   "",     "" },
						{ "ClientCode",     "CLIFREE",  "CLIFREE" },
						{ "Warehouse",      "WHSFREE",  "WHSFREE" },
						{ "Reference",      "RECV1234",     "RECV1235" },
						{ "ArrivalDate",      "",     "" },
						{ "ProductCode",      "P01",    "P01" },
						{ "Quantity",     "10",   "25" },
						{ "QuantityUQ",     "",     "" },
						{ "Pallets",        "",     "" },
						{ "Location",     $"{locationString}",     $"{locationString}" },
						{ "Attribute1",     "",     "" },
						{ "Attribute2",     "",     "" },
						{ "Attribute3",     "",     "" },
						{ "ExpiryDate",     "",     "" },
						{ "PackingDate",      "",     "" },
						{ "CustomsEntryLineNo", "",     "" },
						{ "CustomsEntryDate", "",     "" },
						{ "CustomsAddInfo",   "",     "" },
						{ "CustomsQty",     "",     "" },
						{ "CustomsUQ",      "",     "" },
						{ "CtryOfOrigin",   "",     "" },
						{ "ValueForDuty",   "",     "" },
						{ "BondedWhsQty",   "",     "" },
						{ "BondedWhsUQ",      "",     "" },
						{ "TILV",       "",     "" },
						{ "CustomsSecondQuantity","",   "" },
						{ "CustomsSecondUnitQty","",  "" },
						{ "Tariff","",   "" },
						{ "PrimaryPreference","",   "" },
						{ "CustomsThirdQuantity","",   "" },
						{ "CustomsThirdUnitQty","",  "" },
						{ "ManufacturerCode",    "MANUFREE", "MANUFREE" },
						{ "ZoneStatus", "" , "" },
						{ "IsFromOtherFTZWarehouse", "N" , "N" },
						{ "OutwardType", "", "" },
						{ "SerialNumber", "", "" },
						{ "ReceiveCategory",  "",     "" }
				};
		}

		#endregion

		#region TestValidationOfFile

		[ExpectException(typeof(FileNotFoundException))]
		public void TestValidationOfFile()
		{
			Loader.ImportReceiveData("non-existant file");
		}

		#endregion

		#region TestDocketDataFormatter

		public void TestDocketDataFormatter()
		{
			AssertNotNull(Loader.DocketDataFormatter);
			AssertEquals(typeof(WhsDocketDataFormatter), Loader.DocketDataFormatter.GetType());
		}

		#endregion

		#region TestDocketDataFormatterBeingUsed

		[TestDate(2005, 1, 1)]
		public void TestDocketDataFormatterBeingUsed()
		{
			AssertEquals("Precondition: TESTWasFormatCSVDataCalled is false ", false, Loader.DocketDataFormatter.TESTWasFormatCSVDataCalled);
			WhsReceive receive = LoadReceiveCreatedByImport("REF1005");
			Factory.Save();

			ImportCsvData(GetTestDataImportBondedReceiveData());
			AssertEquals(true, Loader.DocketDataFormatter.TESTWasFormatCSVDataCalled);
		}

		#endregion

		#region TestClientValidationQuantityUnit

		[TestDate(2005, 1, 1)]
		public void TestClientValidationQuantityUnit()
		{
			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportQuantityUQInValidValue()));
			AssertEquals("Should contain error: Quantity Unit (KKK) is not a valid Unit", true, Loader.Log.Any(entry => entry == "Row 2 Error.... Quantity Unit (KKK) is not a valid Unit."));

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportQuantityUQExceedMaximumLength()));
			AssertEquals("Should contain error: The maximum length for Quantity Unit (KILOGRAM) has been exceeded", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Quantity Unit (KILOGRAM) has been exceeded. The maximum length of this property is 3 characters."));

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportCustomsUQExceedMaximumLength()));
			AssertEquals("Should contain error: The maximum length for Customs Unit Quantity (KILOGRAM) has been exceeded", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Customs Unit Quantity (KILOGRAM) has been exceeded. The maximum length of this property is 6 characters."));

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportCustomsSecondUQExceedMaximumLength()));
			AssertEquals("Should contain error: The maximum length for Customs Second Unit Quantity (KILOGRAM) has been exceeded", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Customs Second Unit Quantity (KILOGRAM) has been exceeded. The maximum length of this property is 6 characters."));

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportBondedWhsUQExceedMaximumLength()));
			AssertEquals("Should contain error: The maximum length for Bonded Warehouse Unit Quantity (KILOGRAM) has been exceeded", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Bonded Warehouse Unit Quantity (KILOGRAM) has been exceeded. The maximum length of this property is 4 characters."));
		}

		string[,] GetTestDataImportQuantityUQInValidValue()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",   "556542" },
						{ "ClientCode",     "BONDCLI" },
						{ "Warehouse",      "BONDWHS" },
						{ "Reference",      "REF1005" },
						{ "ArrivalDate",      "20050505" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "KKK" },
						{ "Pallets",        "0001" },
						{ "Location",     "BOND" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20100101" },
						{ "PackingDate",      "20010101" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO" },
						{ "CustomsQty",     "20" },
						{ "CustomsUQ",      "KG" },
						{ "CtryOfOrigin",   "AU" },
						{ "ValueForDuty",   "1" },
						{ "BondedWhsQty",   "30" },
						{ "BondedWhsUQ",      "KG" },
						{ "TILV",       "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",       "" }
				};
		}

		string[,] GetTestDataImportQuantityUQExceedMaximumLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",   "556542" },
						{ "ClientCode",     "BONDCLI" },
						{ "Warehouse",      "BONDWHS" },
						{ "Reference",      "REF1005" },
						{ "ArrivalDate",      "20050505" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "KiloGram" },
						{ "Pallets",        "0001" },
						{ "Location",     "BOND" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20100101" },
						{ "PackingDate",      "20010101" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO" },
						{ "CustomsQty",     "20" },
						{ "CustomsUQ",      "KG" },
						{ "CtryOfOrigin",   "AU" },
						{ "ValueForDuty",   "1" },
						{ "BondedWhsQty",   "30" },
						{ "BondedWhsUQ",      "KG" },
						{ "TILV",       "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",       "" }
				};
		}

		string[,] GetTestDataImportCustomsUQExceedMaximumLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",    "556542" },
						{ "ClientCode",      "BONDCLI" },
						{ "Warehouse",     "BONDWHS" },
						{ "Reference",     "REF1005" },
						{ "ArrivalDate",     "20050505" },
						{ "ProductCode",     "P01" },
						{ "Quantity",      "10" },
						{ "QuantityUQ",      "KG" },
						{ "Pallets",       "0001" },
						{ "Location",      "BOND" },
						{ "Attribute1",      "A1" },
						{ "Attribute2",      "A2" },
						{ "Attribute3",      "A3" },
						{ "ExpiryDate",      "20100101" },
						{ "PackingDate",     "20010101" },
						{ "CustomsEntryLineNo",  "1" },
						{ "CustomsEntryDate",  "20050506" },
						{ "CustomsAddInfo",    "ADDINFO" },
						{ "CustomsQty",      "20" },
						{ "CustomsUQ",     "Kilogram" },
						{ "CtryOfOrigin",    "AU" },
						{ "ValueForDuty",    "1" },
						{ "BondedWhsQty",    "30" },
						{ "BondedWhsUQ",     "KG" },
						{ "TILV",        "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",      "" }
				};
		}

		string[,] GetTestDataImportCustomsSecondUQExceedMaximumLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",    "556542" },
						{ "ClientCode",      "BONDCLI" },
						{ "Warehouse",     "BONDWHS" },
						{ "Reference",     "REF1005" },
						{ "ArrivalDate",     "20050505" },
						{ "ProductCode",     "P01" },
						{ "Quantity",      "10" },
						{ "QuantityUQ",      "KG" },
						{ "Pallets",       "0001" },
						{ "Location",      "BOND" },
						{ "Attribute1",      "A1" },
						{ "Attribute2",      "A2" },
						{ "Attribute3",      "A3" },
						{ "ExpiryDate",      "20100101" },
						{ "PackingDate",     "20010101" },
						{ "CustomsEntryLineNo",  "1" },
						{ "CustomsEntryDate",  "20050506" },
						{ "CustomsAddInfo",    "ADDINFO" },
						{ "CustomsQty",      "20" },
						{ "CustomsUQ",     "KG" },
						{ "CtryOfOrigin",    "AU" },
						{ "ValueForDuty",    "1" },
						{ "BondedWhsQty",    "30" },
						{ "BondedWhsUQ",     "KG" },
						{ "TILV",        "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "Kilogram" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "Gram" },
						{ "ManufacturerCode",    "MANUFREE" },
						{ "ZoneStatus", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",      "" }
				};
		}

		string[,] GetTestDataImportBondedWhsUQExceedMaximumLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1		
			//		-----------------------------------------
			{
						{ "CustomsEntryNo",   "556542" },
						{ "ClientCode",     "BONDCLI" },
						{ "Warehouse",      "BONDWHS" },
						{ "Reference",      "REF1005" },
						{ "ArrivalDate",      "20050505" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "KG" },
						{ "Pallets",        "0001" },
						{ "Location",     "BOND" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "20100101" },
						{ "PackingDate",      "20010101" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "20050506" },
						{ "CustomsAddInfo",   "ADDINFO" },
						{ "CustomsQty",     "20" },
						{ "CustomsUQ",      "KG" },
						{ "CtryOfOrigin",   "AU" },
						{ "ValueForDuty",   "1" },
						{ "BondedWhsQty",   "30" },
						{ "BondedWhsUQ",      "Kilogram" },
						{ "TILV",       "5.1" },
						{ "CustomsSecondQuantity",    "24" },
						{ "CustomsSecondUnitQty",     "GRM" },
						{ "Tariff",     "TRF" },
						{ "PrimaryPreference",     "STANDARD" },
						{ "CustomsThirdQuantity",    "12" },
						{ "CustomsThirdUnitQty",     "GR" },
						{ "ManufacturerCode",    "BONDMANU" },
						{ "ZoneStatus", "D" },
						{ "IsFromOtherFTZWarehouse", "Y" },
						{ "OutwardType", "CNN" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",       "" }
				};
		}

		#endregion

		#region TestClientValidationPartAttributes

		public void TestClientValidationPartAttributes()
		{
			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportPartAttributesValidValue()));
			foreach (var logEntry in Loader.Log)
			{
				AssertEquals("Should not contain errors", false, logEntry.Contains("The maximum length for Attribute"));
			}

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportPartAttributesInvalidValue()));
			AssertEquals("Should contain error", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Attribute 1 (A1-123456789012345678901234567890) has been exceeded. The maximum length of this property is 30 characters."));
			AssertEquals("Should contain error", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Attribute 2 (A2-123456789012345678901234567890) has been exceeded. The maximum length of this property is 30 characters."));
			AssertEquals("Should contain error", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Attribute 3 (A3-123456789012345678901234567890) has been exceeded. The maximum length of this property is 30 characters."));
			AssertEquals("Should contain error", true, Loader.Log.Any(entry => entry == "Row 2 Error.... The maximum length for Serial Number (SN-123456789012345678901234567890123456789012345678901234567890) has been exceeded. The maximum length of this property is 50 characters."));
		}

		string[,] GetTestDataImportPartAttributesValidValue()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "123456" },
					{ "ClientCode",          "BONDCLI" },
					{ "Warehouse",           "BONDWHS" },
					{ "Reference",           "" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         "P01" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "" },
					{ "Pallets",             "" },
					{ "Location",            "" },
					{ "Attribute1",          "A1" },
					{ "Attribute2",          "A2" },
					{ "Attribute3",          "A3" },
					{ "ExpiryDate",          "" },
					{ "PackingDate",         "" },
					{ "CustomsEntryLineNo",  "1" },
					{ "CustomsEntryDate",    "" },
					{ "CustomsAddInfo",      "" },
					{ "CustomsQty",          "" },
					{ "CustomsUQ",           "" },
					{ "CtryOfOrigin",        "" },
					{ "ValueForDuty",        "" },
					{ "BondedWhsQty",        "" },
					{ "BondedWhsUQ",         "" },
					{ "TILV",                "" },
					{ "CustomsSecondQuantity", "" },
					{ "CustomsSecondUnitQty",  "" },
					{ "Tariff",             "" },
					{ "PrimaryPreference",  "" },
					{ "CustomsThirdQuantity", "" },
					{ "CustomsThirdUnitQty",  "" },
					{ "ManufacturerCode",    "BONDMANU" },
					{ "ZoneStatus", "" },
					{ "IsFromOtherFTZWarehouse", "N" },
					{ "OutwardType", "" },
					{ "SerialNumber", "SN" },
					{ "ReceiveCategory",     "" }
			};
		}

		string[,] GetTestDataImportPartAttributesInvalidValue()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header					Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "123456" },
					{ "ClientCode",          "BONDCLI" },
					{ "Warehouse",           "BONDWHS" },
					{ "Reference",           "" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         "P01" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "" },
					{ "Pallets",             "" },
					{ "Location",            "" },
					{ "Attribute1",          "A1-123456789012345678901234567890" },
					{ "Attribute2",          "A2-123456789012345678901234567890" },
					{ "Attribute3",          "A3-123456789012345678901234567890" },
					{ "ExpiryDate",          "" },
					{ "PackingDate",         "" },
					{ "CustomsEntryLineNo",  "1" },
					{ "CustomsEntryDate",    "" },
					{ "CustomsAddInfo",      "" },
					{ "CustomsQty",          "" },
					{ "CustomsUQ",           "" },
					{ "CtryOfOrigin",        "" },
					{ "ValueForDuty",        "" },
					{ "BondedWhsQty",        "" },
					{ "BondedWhsUQ",         "" },
					{ "TILV",                "" },
					{ "CustomsSecondQuantity", "" },
					{ "CustomsSecondUnitQty",  "" },
					{ "Tariff",             "" },
					{ "PrimaryPreference",  "" },
					{ "CustomsThirdQuantity", "" },
					{ "CustomsThirdUnitQty",  "" },
					{ "ManufacturerCode",    "BONDMANU" },
					{ "ZoneStatus", "" },
					{ "IsFromOtherFTZWarehouse", "N" },
					{ "OutwardType", "" },
					{ "SerialNumber", "SN-123456789012345678901234567890123456789012345678901234567890" },
					{ "ReceiveCategory",     "" }
			};
		}

		#endregion

		#region TestImport_UnitConversion

		public void TestImport_UnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithUnitConversion(data.Org1.OH_Code, data.Whs1.WW_WarehouseName, data.Part1.OP_PartNum)));

			var loadedReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive imported", 1, loadedReceives.Length);

			CombineAssertions(() =>
			{
				var receiveLine = loadedReceives[0].Lines[0];
				AssertEquals("Correct receiveLine WE_ClientOrderedUnits", 250m, receiveLine.WE_ClientOrderedUnits);
				AssertEquals("Correct receiveLine WE_PackQuantity", 25m, receiveLine.WE_PackQuantity);
				AssertEquals("Correct receiveLine WE_F3_NKPackType", "PLT", receiveLine.WE_F3_NKPackType);
				AssertEquals("Correct receiveLine WE_TransactionQuantity", 250m, receiveLine.WE_TransactionQuantity);
			});
		}

		string[,] GetTestDataImportWithUnitConversion(string clientCode, string whs, string productCode)
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "" },
					{ "ClientCode",          $"{clientCode}" },
					{ "Warehouse",           $"{whs}" },
					{ "Reference",           "CONVERTTEST" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         $"{productCode}" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "PLT" },
					{ "Pallets",             "" },
					{ "Location",            "" },
					{ "Attribute1",          "" },
					{ "Attribute2",          "" },
					{ "Attribute3",          "" },
					{ "ExpiryDate",          "" },
					{ "PackingDate",         "" },
					{ "CustomsEntryLineNo",  "" },
					{ "CustomsEntryDate",    "" },
					{ "CustomsAddInfo",      "" },
					{ "CustomsQty",          "" },
					{ "CustomsUQ",           "" },
					{ "CtryOfOrigin",        "" },
					{ "ValueForDuty",        "" },
					{ "BondedWhsQty",        "" },
					{ "BondedWhsUQ",         "" },
					{ "TILV",                "" },
					{ "CustomsSecondQuantity", "" },
					{ "CustomsSecondUnitQty",  "" },
					{ "Tariff",             "" },
					{ "PrimaryPreference",  "" },
					{ "CustomsThirdQuantity", "" },
					{ "CustomsThirdUnitQty",  "" },
					{ "ManufacturerCode",    "" },
					{ "ZoneStatus", "" },
					{ "IsFromOtherFTZWarehouse", "" },
					{ "OutwardType", "" },
					{ "SerialNumber", "" },
					{ "ReceiveCategory",     "" }
			};
		}

		#endregion

		#region TestImport_CategoryCode

		public void TestImport_CategoryCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithCategoryCode(data.Org1.OH_Code, data.Whs1.WW_WarehouseName, data.Part1.OP_PartNum)));

			var loadedReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive imported", 1, loadedReceives.Length);

			var receive = loadedReceives[0];
			AssertEquals("Correct receive WD_ReceiveCategory", "ABC", receive.WD_ReceiveCategory);
		}

		string[,] GetTestDataImportWithCategoryCode(string clientCode, string whs, string productCode)
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "" },
					{ "ClientCode",          $"{clientCode}" },
					{ "Warehouse",           $"{whs}" },
					{ "Reference",           "CONVERTTEST" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         $"{productCode}" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "PLT" },
					{ "Pallets",             "" },
					{ "Location",            "" },
					{ "Attribute1",          "" },
					{ "Attribute2",          "" },
					{ "Attribute3",          "" },
					{ "ExpiryDate",          "" },
					{ "PackingDate",         "" },
					{ "CustomsEntryLineNo",  "" },
					{ "CustomsEntryDate",    "" },
					{ "CustomsAddInfo",      "" },
					{ "CustomsQty",          "" },
					{ "CustomsUQ",           "" },
					{ "CtryOfOrigin",        "" },
					{ "ValueForDuty",        "" },
					{ "BondedWhsQty",        "" },
					{ "BondedWhsUQ",         "" },
					{ "TILV",                "" },
					{ "CustomsSecondQuantity", "" },
					{ "CustomsSecondUnitQty",  "" },
					{ "Tariff",             "" },
					{ "PrimaryPreference",  "" },
					{ "CustomsThirdQuantity", "" },
					{ "CustomsThirdUnitQty",  "" },
					{ "ManufacturerCode",    "" },
					{ "ZoneStatus", "" },
					{ "IsFromOtherFTZWarehouse", "" },
					{ "OutwardType", "" },
					{ "SerialNumber", "" },
					{ "ReceiveCategory",     "ABC" }
			};
		}

		public void TestImport_CategoryCodeExceedingMaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithCategoryCodeExceedingMaxLength(data.Org1.OH_Code, data.Whs1.WW_WarehouseName, data.Part1.OP_PartNum)));

			AssertLogContainsErrorMessage("Row 2 Error.... The maximum length for Receive Category (ABCDEFGH) has been exceeded. The maximum length of this property is 3 characters.");

			var loadedReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive imported", 0, loadedReceives.Length);
		}

		string[,] GetTestDataImportWithCategoryCodeExceedingMaxLength(string clientCode, string whs, string productCode)
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "" },
					{ "ClientCode",          $"{clientCode}" },
					{ "Warehouse",           $"{whs}" },
					{ "Reference",           "CONVERTTEST" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         $"{productCode}" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "PLT" },
					{ "Pallets",             "" },
					{ "Location",            "" },
					{ "Attribute1",          "" },
					{ "Attribute2",          "" },
					{ "Attribute3",          "" },
					{ "ExpiryDate",          "" },
					{ "PackingDate",         "" },
					{ "CustomsEntryLineNo",  "" },
					{ "CustomsEntryDate",    "" },
					{ "CustomsAddInfo",      "" },
					{ "CustomsQty",          "" },
					{ "CustomsUQ",           "" },
					{ "CtryOfOrigin",        "" },
					{ "ValueForDuty",        "" },
					{ "BondedWhsQty",        "" },
					{ "BondedWhsUQ",         "" },
					{ "TILV",                "" },
					{ "CustomsSecondQuantity", "" },
					{ "CustomsSecondUnitQty",  "" },
					{ "Tariff",             "" },
					{ "PrimaryPreference",  "" },
					{ "CustomsThirdQuantity", "" },
					{ "CustomsThirdUnitQty",  "" },
					{ "ManufacturerCode",    "" },
					{ "ZoneStatus", "" },
					{ "IsFromOtherFTZWarehouse", "" },
					{ "OutwardType", "" },
					{ "SerialNumber", "" },
					{ "ReceiveCategory",     "ABCDEFGH" }
			};
		}

		#endregion

		#region TestImport_StartedReceiving_NoUnitConversion

		public void TestImport_StartedReceiving_NoUnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "CONVERTTEST");
			var initialRecLine = Helper.CreateWhsReceiveLine(receive, Part1, 1m);
			initialRecLine.WE_F3_NKPackType = Constants.PkgUnit.Unit;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithUnitConversion(data.Org1.OH_Code, data.Whs1.WW_WarehouseName, data.Part1.OP_PartNum)));
			Factory.Save();

			var loadedReceives = new BusinessObjectFactory().Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive imported", 1, loadedReceives.Length);

			CombineAssertions(() =>
			{
				var loadedReceive = loadedReceives[0];
				var receiveLine0 = loadedReceive.Lines[0];
				AssertEquals("Correct receiveLine0 WE_ClientOrderedUnits", 1m, receiveLine0.WE_ClientOrderedUnits);
				AssertEquals("Correct receiveLine0 WE_PackQuantity", 1m, receiveLine0.WE_PackQuantity);
				AssertEquals("Correct receiveLine0 WE_F3_NKPackType", "UNT", receiveLine0.WE_F3_NKPackType);
				AssertEquals("Correct receiveLine0 WE_TransactionQuantity", 1m, receiveLine0.WE_TransactionQuantity);
				var receiveLine1 = loadedReceive.Lines[1];
				AssertEquals("Correct receiveLine1 WE_ClientOrderedUnits", 25m, receiveLine1.WE_ClientOrderedUnits);
				AssertEquals("Correct receiveLine1 WE_PackQuantity", 25m, receiveLine1.WE_PackQuantity);
				AssertEquals("Correct receiveLine1 WE_F3_NKPackType", "PLT", receiveLine1.WE_F3_NKPackType);
				AssertEquals("Correct receiveLine1 WE_TransactionQuantity", 25m, receiveLine1.WE_TransactionQuantity);
			});
		}

		#endregion

		#region TestImport_WithExceedingMaxLengthOnExternalReference

		public void TestImport_WithExceedingMaxLengthOnExternalReference()
		{
			// saves objects from SetUp()
			Factory.Save();

			AssertEquals("Precondition", 0, Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive)).Length);
			AssertNoExceptionThrown("No exceeding max length exception is thrown.", () => ImportCsvData(GetDataWithExternalReferenceExceedingMaxLength()));
			AssertLogContainsErrorMessage("Row 2 Error.... The maximum length for Reference (ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890) has been exceeded. The maximum length of this property is 35 characters.");
			AssertEquals("No receives are imported.", 0, Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive)).Length);
		}

		string[,] GetDataWithExternalReferenceExceedingMaxLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890" },
						{ "ArrivalDate",      "" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "" },
						{ "Pallets",        "" },
						{ "Location",     "" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "" },
						{ "PackingDate",      "" },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate", "" },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",       "" }
			};
		}

		#endregion

		#region TestImport_WithExceedingMaxLengthOnProductCode

		public void TestImport_WithExceedingMaxLengthOnProductCode()
		{
			// saves objects from SetUp()
			Factory.Save();

			AssertEquals("Precondition", 0, Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive)).Length);
			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown("No exceeding max length exception is thrown.", () => ImportCsvData(GetDataWithProductCodeExceedingMaxLength()));
				AssertLogContainsErrorMessage("Row 2 Error.... The maximum length for Product Code (ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890) has been exceeded. The maximum length of this property is 35 characters.");
				AssertEquals("No receives are imported.", 0, Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive)).Length);
			}
		}

		string[,] GetDataWithProductCodeExceedingMaxLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "" },
						{ "ClientCode",     "CLIFREE" },
						{ "Warehouse",      "WHSFREE" },
						{ "Reference",      "ABCD" },
						{ "ArrivalDate",      "" },
						{ "ProductCode",      "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "" },
						{ "Pallets",        "" },
						{ "Location",     "" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "" },
						{ "PackingDate",      "" },
						{ "CustomsEntryLineNo", "" },
						{ "CustomsEntryDate", "" },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory","" }
			};
		}

		#endregion

		#region TestImport_WithExceedingMaxLengthOnBondedEntryKey

		public void TestImport_WithExceedingMaxLengthOnBondedEntryKey()
		{
			// saves objects from SetUp()
			Factory.Save();

			AssertEquals("Precondition", 0, Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive)).Length);
			AssertNoExceptionThrown("No exceeding max length exception is thrown.", () => ImportCsvData(GetDataWithBondedEntryKeyMaxLength()));
			AssertLogContainsErrorMessage("Row 2 Error.... The maximum length for Customs Entry Key (ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-1) has been exceeded. The maximum length of this property is 35 characters.");
			AssertEquals("No receives are imported.", 0, Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive)).Length);
		}

		string[,] GetDataWithBondedEntryKeyMaxLength()
		{
			return new string[ReceiveHeader.ColumnCount, 2]
			{
						{ "CustomsEntryNo",   "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890" },
						{ "ClientCode",     "CLIBOND" },
						{ "Warehouse",      "WHSBONDED" },
						{ "Reference",      "ABCD" },
						{ "ArrivalDate",      "" },
						{ "ProductCode",      "P01" },
						{ "Quantity",     "10" },
						{ "QuantityUQ",     "" },
						{ "Pallets",        "" },
						{ "Location",     "" },
						{ "Attribute1",     "A1" },
						{ "Attribute2",     "A2" },
						{ "Attribute3",     "A3" },
						{ "ExpiryDate",     "" },
						{ "PackingDate",      "" },
						{ "CustomsEntryLineNo", "1" },
						{ "CustomsEntryDate", "" },
						{ "CustomsAddInfo",   "" },
						{ "CustomsQty",     "" },
						{ "CustomsUQ",      "" },
						{ "CtryOfOrigin",   "" },
						{ "ValueForDuty",   "" },
						{ "BondedWhsQty",   "" },
						{ "BondedWhsUQ",      "" },
						{ "TILV",       "" },
						{ "CustomsSecondQuantity", "" },
						{ "CustomsSecondUnitQty",  "" },
						{ "Tariff",             "" },
						{ "PrimaryPreference",  "" },
						{ "CustomsThirdQuantity", "" },
						{ "CustomsThirdUnitQty",  "" },
						{ "ManufacturerCode",    "BONDMANU" },
						{ "ZoneStatus", "" },
						{ "IsFromOtherFTZWarehouse", "N" },
						{ "OutwardType", "" },
						{ "SerialNumber", "SN" },
						{ "ReceiveCategory",    "" }
			};
		}

		#endregion

		#region TestImport_HeaderValidation

		public void TestImport_HeaderValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithValidHeader(data.Org1.OH_Code, data.Whs1.WW_WarehouseName, data.Part1.OP_PartNum)));

			var loadedReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive imported", 1, loadedReceives.Length);
		}

		public void TestImport_HeaderValidation_InvalidCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithInvalidHeader_WrongNumber(data.Org1.OH_Code, data.Whs1.WW_WarehouseName, data.Part1.OP_PartNum)));
			AssertLogContainsErrorMessage("File Header information is incorrect. The import of Inventory data requires a specific .CSV format file.");

			var loadedReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive not imported", 0, loadedReceives.Length);
		}

		public void TestImport_HeaderValidation_InvalidHeadings()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			AssertNoExceptionThrown(() => ImportCsvData(GetTestDataImportWithInvalidHeader_WrongHeadings()));
			AssertLogContainsErrorMessage("File Header information is incorrect. The import of Inventory data requires a specific .CSV format file.");

			var loadedReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive));
			AssertEquals("Receive not imported", 0, loadedReceives.Length);
		}

		string[,] GetTestDataImportWithValidHeader(string clientCode, string whs, string productCode)
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "" },
					{ "ClientCode",          $"{clientCode}" },
					{ "Warehouse",           $"{whs}" },
					{ "Reference",           "HeaderTEST" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         $"{productCode}" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "PLT" },
					{ "Pallets",             "" },
					{ "Location",            "" },
					{ "Attribute1",          "" },
					{ "Attribute2",          "" },
					{ "Attribute3",          "" },
					{ "ExpiryDate",          "" },
					{ "PackingDate",         "" },
					{ "CustomsEntryLineNo",  "" },
					{ "CustomsEntryDate",    "" },
					{ "CustomsAddInfo",      "" },
					{ "CustomsQty",          "" },
					{ "CustomsUQ",           "" },
					{ "CtryOfOrigin",        "" },
					{ "ValueForDuty",        "" },
					{ "BondedWhsQty",        "" },
					{ "BondedWhsUQ",         "" },
					{ "TILV",                "" },
					{ "CustomsSecondQuantity", "" },
					{ "CustomsSecondUnitQty",  "" },
					{ "Tariff",             "" },
					{ "PrimaryPreference",  "" },
					{ "CustomsThirdQuantity", "" },
					{ "CustomsThirdUnitQty",  "" },
					{ "ManufacturerCode",    "" },
					{ "ZoneStatus", "" },
					{ "IsFromOtherFTZWarehouse", "" },
					{ "OutwardType", "" },
					{ "SerialNumber", "" },
					{ "ReceiveCategory",     "" }
			};
		}

		string[,] GetTestDataImportWithInvalidHeader_WrongNumber(string clientCode, string whs, string productCode)
		{
			return new string[8, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
					{ "CustomsEntryNo",      "" },
					{ "ClientCode",          $"{clientCode}" },
					{ "Warehouse",           $"{whs}" },
					{ "Reference",           "HeaderTEST" },
					{ "ArrivalDate",         "" },
					{ "ProductCode",         $"{productCode}" },
					{ "Quantity",            "25" },
					{ "QuantityUQ",          "PLT" },
			};
		}

		string[,] GetTestDataImportWithInvalidHeader_WrongHeadings()
		{
			return new string[ReceiveHeader.ColumnCount, 2]

			//		Header								Line1
			//		------------------------------------------------------
			{
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
					{ "ABC",                             "" },
			};
		}

		#endregion

		#region TestImport_BookingDate_UseTimezoneOfWarehouse

		public void TestImport_BookingDate_UseTimezoneOfWarehouse()
		{
			var receive = LoadReceiveCreatedByImport("REF1005");
			AssertNull("Precondition - No Receive", receive);

			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			WhsBonded.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			Factory.Save();
			ImportCsvData(GetTestImport("20120115", "BOND"));
			receive = LoadReceiveCreatedByImport("REF1005");
			Assert("Precondition: Receive is in DB.", receive.IsInDatabase);
			AssertEquals("Non_Empty ArrivalDate should be imported.", new ZDateTimeOffset(2012, 1, 15, 0, 0, 0, TimeSpan.FromHours(8)), receive.WD_ArrivalDate);
			AssertEquals("BookingDate should set to Arrival date.", new ZDateTimeOffset(2012, 1, 15, 0, 0, 0, TimeSpan.FromHours(8)), receive.WD_BookingDate);
		}

		#endregion

		#region Client Validation

		public void TestClientValidationFindsOHCode()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			BondedClient.OH_Code = "BONDCLI";
			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNotNull("Receive should be created", receive);
		}

		public void TestClientValidationFindsLegacyCode()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.LegacySystemCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNotNull("Receive should be created", receive);
		}

		public void TestClientValidationFindsABNCode()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.GSTCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNotNull("Receive should be created", receive);
		}

		public void TestClientValidationGivesErrorIfNoClientCodeFound()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertEquals("Should contain error", true, Loader.Log.Any(entry => entry == "Row 2 Error.... client code (BONDCLI) could not be found."));
		}

		public void TestClientValidationGivesErrorIfDuplicateLegacyCodeFound()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.LegacySystemCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.LegacySystemCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the legacy client code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
		}

		public void TestClientValidationGivesErrorIfDuplicateABNCodeFound()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.GSTCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.GSTCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the ABN code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
		}

		public void TestClientValidationGivesErrorIfDuplicateCustomsClientCodeFound()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.CustomsClientCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the customs client code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
		}

		public void TestClientValidationGivesMultipleDuplicateCodeErrors()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.LegacySystemCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.LegacySystemCode, "BONDCLI");
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.GSTCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.GSTCode, "BONDCLI");
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.CustomsClientCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData());
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the legacy client code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the ABN code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the customs client code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
		}

		public void TestClientValidationForDeliveranceConversionGivesErrorIfNoClientCodeFound()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData(), true);
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertEquals("Should contain error", true, Loader.Log.Any(entry => entry == "Row 2 Error.... client code (BONDCLI) could not be found."));
		}

		public void TestClientValidationForDeliveranceConversionGivesErrorIfDuplicateClientCodeFound()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.DeliveranceCode, "BONDCLI");
			AddClientCode(BondedClient, OrgCusCode.CodeTypes.DeliveranceCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData(), true);
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNull("Receive should not be created", receive);
			AssertCollectionContains("Row 2 Error.... more than one organization was found with the client deliverance code (BONDCLI). Please remove or change the duplicate code.", Loader.Log);
		}

		public void TestClientValidationForDeliveranceConversion()
		{
			RemoveClientCode(BondedClient, OrgCusCode.CodeTypes.CustomsClientCode);
			AddClientCode(FreeStoreClient, OrgCusCode.CodeTypes.DeliveranceCode, "BONDCLI");

			Factory.Save();
			ImportCsvData(GetTestImportBondedWithMinimumData(), true);
			WhsReceive receive = LoadReceiveCreatedByImport("INWARDS W00000001");
			AssertNotNull("Receive should be created", receive);
		}

		#endregion

		#region ProductSorter

		class ProductSorter : IComparer<WhsDocketLine>
		{
			#region IComparer<WhsDocketLine> Members

			public int Compare(WhsDocketLine x, WhsDocketLine y)
			{
				{ return x.SupplierPart.OP_PartNum.CompareTo(y.SupplierPart.OP_PartNum); }
			}
			#endregion
		}

		#endregion

		#region Implementation

		protected override WhsReceiveDataLoad GetNewDataLoader()
		{
			return new WhsReceiveDataLoad();
		}

		void AddClientCode(OrgHeader org, string code, string desc)
		{
			OrgCusCode custCode = org.CustomsCodes.AddNew();
			custCode.OK_CustomsRegNo = desc;
			custCode.OK_CodeType = code;
		}

		void RemoveClientCode(OrgHeader org, string code)
		{
			for (int i = org.CustomsCodes.Count - 1; i >= 0; i--)
			{
				OrgCusCode cusCode = org.CustomsCodes[i];
				if (code == cusCode.OK_CodeType)
				{
					org.CustomsCodes.RemoveAndDelete(cusCode);
					break;
				}
			}
		}

		WhsReceive LoadReceiveCreatedByImport(string externalReference)
		{
			return LoadReceiveCreatedByImport(Factory, externalReference);
		}

		WhsReceive LoadReceiveCreatedByImport(BusinessObjectFactory factory, string externalReference)
		{
			var filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, externalReference);
			var receiveList = new WhsReceiveCollection(factory, filter);
			receiveList.RefreshFromDb();

			return receiveList.Count > 0 ? receiveList[0] : null;
		}

		void ImportCsvData(string[,] data)
		{
			ImportCsvData(data, false);
		}

		void ImportCsvData(string[,] data, bool isDeliveranceConversion)
		{
			using (TempFile testFileName = GetCsvTestFile(data))
			{
				Loader.ImportReceiveData(testFileName.Filename, isDeliveranceConversion);
			}
		}

		int GetNumberOfColumnsInRectangularArray(string[,] data)
		{
			return data.GetUpperBound(1) + 1;
		}

		int GetNumberOfRowsInRectangularArray(string[,] data)
		{
			return data.GetUpperBound(0) + 1;
		}

		ZString GetCsvLine(string[,] data, int lineNo)
		{
			ZString csvLine = "";
			int rowLength = GetNumberOfRowsInRectangularArray(data);
			int column = lineNo;

			for (int row = 0; row < rowLength; row++)
			{
				if (row != rowLength - 1)
				{
					csvLine = csvLine + data[row, column] + ",";
				}
				else
				{
					csvLine = csvLine + data[row, column];
				}
			}
			return csvLine;
		}

		TempFile GetCsvTestFile(string[,] data)
		{
			TempFile file = TempFile.New();
			int totalLines = GetNumberOfColumnsInRectangularArray(data);

			using (StreamWriter sw = new StreamWriter(file.Filename))
			{
				for (int line = 0; line < totalLines; line++)
				{
					sw.WriteLine(GetCsvLine(data, line));
				}
				sw.Flush();
			}

			return file;
		}

		#endregion

		#region SetUp

		void SetUpEnvironment()
		{
			WhsFreeStore = Helper.CreateWarehouse("WHSFREE");
			WhsBonded = Helper.CreateWarehouse("WHSBONDED");
			Helper.CreateRowAndGenerateLocations(WhsFreeStore, "A", 5, 1);
			Helper.CreateRowAndGenerateLocations(WhsBonded, "BOND", 1, 1);
			Helper.EnableWarehouseForBond(WhsBonded, true);
		}

		void SetUpClientAndProduct()
		{
			BondedClient = Helper.CreateClient("CLIBOND", "CLIBOND");

			OrgCusCode custCode = BondedClient.CustomsCodes.AddNew();
			custCode.OK_CustomsRegNo = "BONDCLI";
			custCode.OK_CodeType = "CCD";

			custCode = BondedClient.CustomsCodes.AddNew();
			custCode.OK_CustomsRegNo = "BONDWHS";
			custCode.OK_CodeType = "CCP";

			WhsBonded.WW_OA_WarehouseAddress = BondedClient.Addresses.DefaultAddressOfType(OrgAddressType.Office, false).PK;

			FreeStoreClient = Helper.CreateClient("CLIFREE", "CLIFREE");
			Part1 = Helper.CreateProduct(BondedClient, "P01");
			Part1.OP_StockKeepingUnit = "KG";
			Helper.CreateProductClientRelationShip(FreeStoreClient, Part1);

			Part2 = Helper.CreateProduct(BondedClient, "P02");
			Part2.OP_StockKeepingUnit = "KG";
			Helper.CreateProductClientRelationShip(FreeStoreClient, Part2);
		}

		void SetupManufacturer()
		{
			Manufacturer = Helper.CreateClient("BONDMANU");
		}

		protected override void SetUp()
		{
			base.SetUp();
			Loader = new WhsReceiveDataLoad();
			SetUpEnvironment();
			SetUpClientAndProduct();
			SetupManufacturer();
		}

		WhsWarehouse WhsFreeStore;
		WhsWarehouse WhsBonded;
		OrgHeader BondedClient;
		OrgHeader FreeStoreClient;
		OrgSupplierPart Part1;
		OrgSupplierPart Part2;
		WhsReceiveDataLoad Loader;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupManufacturer")]
		OrgHeader Manufacturer;

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
