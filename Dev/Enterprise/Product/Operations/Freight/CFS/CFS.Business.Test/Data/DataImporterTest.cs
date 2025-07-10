using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Data
{
	sealed class DataImporterTest : TestCaseWithFactory
	{
		public void TestInstantiateDataImporter()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			DataImporter dataImporter = new DataImporter(shipment);
			Assertion.AssertNotNull(dataImporter);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestInstantiateDataImporterWithNullArgument()
		{
			DataImporter dataImporter = new DataImporter(null);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestReadExcelFile()
		{
			using (ExcelInterface excelDoc = new ExcelInterface())
			{
				excelDoc.LoadExcelFile(TestPath + "ExcelFileTest1.xls");
				ArrayList generatedList = Importer.ConvertXLSFileIntoArrays(excelDoc);
				ArrayList expectedList = GenerateExcelRecordList1();
				AssertEquals("Row Count", expectedList.Count, generatedList.Count);
				for (int i = 0; i < expectedList.Count; i++)
				{
					string[] expectedCol = (string[])expectedList[i];
					string[] generatedCol = (string[])generatedList[i];
					AssertEquals("Column Count", expectedCol.Length, generatedCol.Length);
					for (int j = 0; j < expectedCol.Length; j++)
					{
						AssertEquals(expectedCol[j], generatedCol[j]);
					}
				}
			}
		}

		public void TestValidateLine()
		{
			string[] line = "testing123,345,567".Split(',');

			var result = Importer.ValidateLine(line);
			AssertEquals("Record does not have all of the required fields", result);

			line = PackLine1.Split(',');
			result = Importer.ValidateLine(line);
			AssertEquals(null, result);

			line = PackLine2.Split(',');
			result = Importer.ValidateLine(line);
			AssertEquals(null, result);

			line = PackLine3.Split(',');
			result = Importer.ValidateLine(line);
			AssertEquals(null, result);

			line = PackLine4.Split(',');
			result = Importer.ValidateLine(line);
			AssertEquals("No reference provided. Pack line has to be associated with a shipment", result);

			line = PackLine5.Split(',');
			result = Importer.ValidateLine(line);
			AssertEquals("House Bill does not match with the shipment record selected", result);

			line = PackLine6.Split(',');
			result = Importer.ValidateLine(line);
			AssertEquals("Pack line cannot have two references of the same type", result);
		}

		public void TestPopulatePackLines()
		{
			CFSPackLine myPackLine = Shipment.OuterPackLines.AddNew();
			string[] line = PackLine1.Split(',');
			Importer.PopulatePackLine(myPackLine, line);
			AssertEquals("PKG", myPackLine.JL_F3_NKPackType);
			AssertEquals((ZInt)76, myPackLine.JL_PackageCount);
			AssertEquals((ZDecimal)5575, myPackLine.JL_ActualWeight);
			AssertEquals((ZDecimal)6, myPackLine.JL_Height);
			AssertEquals((ZDecimal)0, myPackLine.JL_Length);
			AssertEquals((ZDecimal)8, myPackLine.JL_Width);
			AssertEquals("CM", myPackLine.JL_UnitOfDimension);
			AssertEquals("KG", myPackLine.JL_ActualWeightUQ);
			AssertEquals((ZDecimal)0, myPackLine.JL_ActualVolume);
			AssertEquals("M3", myPackLine.JL_ActualVolumeUQ);

			line = PackLine2.Split(',');
			Importer.PopulatePackLine(myPackLine, line);
			AssertEquals("PKG", myPackLine.JL_F3_NKPackType);
			AssertEquals((ZInt)88, myPackLine.JL_PackageCount);
			AssertEquals((ZDecimal)6788, myPackLine.JL_ActualWeight);
			AssertEquals((ZDecimal)200, myPackLine.JL_Height);
			AssertEquals((ZDecimal)900, myPackLine.JL_Length);
			AssertEquals((ZDecimal)23, myPackLine.JL_Width);
			AssertEquals("CM", myPackLine.JL_UnitOfDimension);
			AssertEquals("KG", myPackLine.JL_ActualWeightUQ);
			AssertEquals(364.32, (Double)myPackLine.JL_ActualVolume);
			AssertEquals("M3", myPackLine.JL_ActualVolumeUQ);

			line = PackLine3.Split(',');
			Importer.PopulatePackLine(myPackLine, line);
			AssertEquals("PKG", myPackLine.JL_F3_NKPackType);
			AssertEquals((ZInt)88, myPackLine.JL_PackageCount);
			AssertEquals((ZDecimal)6788, myPackLine.JL_ActualWeight);
			AssertEquals((ZDecimal)200, myPackLine.JL_Height);
			AssertEquals((ZDecimal)900, myPackLine.JL_Length);
			AssertEquals((ZDecimal)23, myPackLine.JL_Width);
			AssertEquals("CM", myPackLine.JL_UnitOfDimension);
			AssertEquals("KG", myPackLine.JL_ActualWeightUQ);
			AssertEquals(364.32, (Double)myPackLine.JL_ActualVolume);
			AssertEquals("M3", myPackLine.JL_ActualVolumeUQ);
		}

		public void TestProcessPackLines()
		{
			var result = "";
			CFSPackLine generatedPackLine;

			string[] line = PackLine1.Split(',');
			generatedPackLine = GeneratePackLine(line);
			result = Importer.ProcessLineFromFile(line);
			AssertEquals(null, result);
			AssertPackLine(generatedPackLine, Importer.GetShipmentInternal().OuterPackLines[0]);

			line = PackLine2.Split(',');
			generatedPackLine = GeneratePackLine(line);
			result = Importer.ProcessLineFromFile(line);
			AssertEquals(null, result);
			AssertPackLine(generatedPackLine, Importer.GetShipmentInternal().OuterPackLines[1]);

			line = PackLine5.Split(',');
			result = Importer.ProcessLineFromFile(line);
			AssertEquals("House Bill does not match with the shipment record selected", result);
			AssertEquals(2, Importer.GetShipmentInternal().OuterPackLines.Count);
		}

		public void TestReuseExistingContainerLegs()
		{
			CFSPackLine packLineA = Shipment.OuterPackLines.AddNew();
			Importer.PopulatePackLine(packLineA, PackLine7.Split(','));

			CFSPackLine packLineB = Shipment.OuterPackLines.AddNew();
			Importer.PopulatePackLine(packLineB, PackLine7.Split(','));

			AssertEquals("Shipment should have 1 ContainerLeg", 1, Shipment.OriginCFSArrivals.Count);
			CommonPickupDeliveryConfirm leg1 = Shipment.OriginCFSArrivals[0];
			AssertEquals("Leg1.DriversName", "Drivers Name", leg1.EU_DriversName);
			AssertEquals("Leg1.TruckRegistration", "Rego", leg1.EU_VehicleRegistration);
			AssertEquals("Leg1.JU_DeliverTimeIn", new ZDateTime(2005, 01, 28, 04, 37, 0), leg1.EU_PickupDeliveryTime);

			CFSPackLine packLineC = Shipment.OuterPackLines.AddNew();
			Importer.PopulatePackLine(packLineC, PackLine8.Split(','));

			AssertEquals("Shipment should have 2 ContainerLegs", 2, Shipment.OriginCFSArrivals.Count);
			CommonPickupDeliveryConfirm leg2 = Shipment.OriginCFSArrivals[1];
			AssertEquals("Leg2.DriversName", "Fread", leg2.EU_DriversName);
			AssertEquals("Leg2.TruckRegistration", "Finger1", leg2.EU_VehicleRegistration);
			AssertEquals("Leg2.JU_DeliverTimeIn", new ZDateTime(2005, 01, 28, 04, 37, 0), leg1.EU_PickupDeliveryTime);

			CommonConfirmDivot divotA1 = leg1.GetDivot(packLineA);
			CommonConfirmDivot divotA2 = leg2.GetDivot(packLineA);
			CommonConfirmDivot divotB1 = leg1.GetDivot(packLineB);
			CommonConfirmDivot divotB2 = leg2.GetDivot(packLineB);
			CommonConfirmDivot divotC1 = leg1.GetDivot(packLineC);
			CommonConfirmDivot divotC2 = leg2.GetDivot(packLineC);

			AssertEquals("DivotA1.J8_PackagesDelivered", 99, divotA1.J8_PackagesDelivered);
			AssertEquals("DivotA2.J8_PackagesDelivered", 0, divotA2.J8_PackagesDelivered);
			AssertEquals("DivotB1.J8_PackagesDelivered", 99, divotB1.J8_PackagesDelivered);
			AssertEquals("DivotB2.J8_PackagesDelivered", 0, divotB2.J8_PackagesDelivered);
			AssertEquals("DivotC1.J8_PackagesDelivered", 0, divotC1.J8_PackagesDelivered);
			AssertEquals("DivotC2.J8_PackagesDelivered", 88, divotC2.J8_PackagesDelivered);
		}

		public void TestDontReuseSavedContainerLegs()
		{
			CFSPackLine packLineA = Shipment.OuterPackLines.AddNew();
			Importer.PopulatePackLine(packLineA, PackLine8.Split(','));

			Factory.Save();

			CFSPackLine packLineB = Shipment.OuterPackLines.AddNew();
			Importer.PopulatePackLine(packLineB, PackLine8.Split(','));

			AssertEquals("Should have 2 seporate containerLegs", 2, Shipment.OriginCFSArrivals.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportFileNotExist()
		{
			ZBool result = Importer.ImportPackLinesFromFile(TestPath + "NoFile.csv");
			AssertEquals(ZBool.False, result);
			AssertEquals(string.Format("The File '{0}NoFile.csv' does not exist", TestPath), Importer.ErrorMessage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPackLinesFromCSVFile()
		{
			ZBool result = Importer.ImportPackLinesFromFile(TestPath + "CFSShipmentSample1.csv");
			AssertEquals(ZBool.True, result);
			AssertEquals(3, Importer.GetShipmentInternal().OuterPackLines.Count);

			CFSPackLine line1 = Importer.GetShipmentInternal().OuterPackLines[0];
			AssertEquals("PKG", line1.JL_F3_NKPackType);
			AssertEquals((ZInt)76, line1.JL_PackageCount);
			AssertEquals((ZDecimal)5575, line1.JL_ActualWeight);
			AssertEquals((ZDecimal)6, line1.JL_Height);
			AssertEquals((ZDecimal)0, line1.JL_Length);
			AssertEquals((ZDecimal)8, line1.JL_Width);
			AssertEquals("CM", line1.JL_UnitOfDimension);
			AssertEquals("KG", line1.JL_ActualWeightUQ);
			AssertEquals((Decimal)0, line1.JL_ActualVolume);
			AssertEquals("M3", line1.JL_ActualVolumeUQ);

			CFSPackLine line2 = Importer.GetShipmentInternal().OuterPackLines[1];
			AssertEquals("PKG", line2.JL_F3_NKPackType);
			AssertEquals((ZInt)100, line2.JL_PackageCount);
			AssertEquals((ZDecimal)5530, line2.JL_ActualWeight);
			AssertEquals((ZDecimal)200, line2.JL_Height);
			AssertEquals((ZDecimal)200, line2.JL_Length);
			AssertEquals((ZDecimal)200, line2.JL_Width);
			AssertEquals("CM", line2.JL_UnitOfDimension);
			AssertEquals("KG", line2.JL_ActualWeightUQ);
			AssertEquals((ZDecimal)800, line2.JL_ActualVolume);
			AssertEquals("M3", line2.JL_ActualVolumeUQ);

			CFSPackLine line3 = Importer.GetShipmentInternal().OuterPackLines[2];
			AssertEquals("PKG", line3.JL_F3_NKPackType);
			AssertEquals((ZInt)8, line3.JL_PackageCount);
			AssertEquals((ZDecimal)4848, line3.JL_ActualWeight);
			AssertEquals((ZDecimal)8, line3.JL_Height);
			AssertEquals((ZDecimal)8, line3.JL_Length);
			AssertEquals((ZDecimal)8, line3.JL_Width);
			AssertEquals("CM", line3.JL_UnitOfDimension);
			AssertEquals("KG", line3.JL_ActualWeightUQ);
			AssertEquals(0.004, (Double)line3.JL_ActualVolume);
			AssertEquals("M3", line3.JL_ActualVolumeUQ);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPackLinesFromCSV1FileWithErrors()
		{
			ZBool result = Importer.ImportPackLinesFromFile(TestPath + "CFSShipmentSample2.csv");
			AssertEquals(ZBool.True, result);
			AssertEquals(8, Importer.GetShipmentInternal().OuterPackLines.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPackLinesFromCSV3FileWithErrors()
		{
			ZBool result = Importer.ImportPackLinesFromFile(TestPath + "CFSShipmentSample3.txt");
			AssertEquals(ZBool.True, result);
			AssertEquals(8, Importer.GetShipmentInternal().OuterPackLines.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPackLinesFromXLSFileWithErrors()
		{
			ZBool result = Importer.ImportPackLinesFromFile(TestPath + "CFSShipmentSample2.xls");
			AssertEquals(ZBool.True, result);
			AssertEquals(8, Importer.GetShipmentInternal().OuterPackLines.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<CFSShipment>();
			Shipment.JS_InterimReceipt = "INT101";
			Shipment.JS_HouseBill = "HB101";
			Importer = new DataImporter(Shipment);
		}

		void AssertPackLine(CFSPackLine expectedPackLine, CFSPackLine generatedPackLine)
		{
			AssertEquals(expectedPackLine.JL_F3_NKPackType, generatedPackLine.JL_F3_NKPackType);
			AssertEquals(expectedPackLine.JL_PackageCount, generatedPackLine.JL_PackageCount);
			AssertEquals(expectedPackLine.JL_ActualWeight, generatedPackLine.JL_ActualWeight);
			AssertEquals(expectedPackLine.JL_Height, generatedPackLine.JL_Height);
			AssertEquals(expectedPackLine.JL_Length, generatedPackLine.JL_Length);
			AssertEquals(expectedPackLine.JL_Width, generatedPackLine.JL_Width);
			AssertEquals(expectedPackLine.JL_UnitOfDimension, generatedPackLine.JL_UnitOfDimension);
			AssertEquals(expectedPackLine.JL_ActualWeightUQ, generatedPackLine.JL_ActualWeightUQ);
			AssertEquals(expectedPackLine.JL_ActualVolume, generatedPackLine.JL_ActualVolume);
			AssertEquals(expectedPackLine.JL_ActualVolumeUQ, generatedPackLine.JL_ActualVolumeUQ);
		}

		CFSPackLine GeneratePackLine(string[] line)
		{
			CFSPackLine packLine = Factory.New<CFSPackLine>();
			packLine.JL_F3_NKPackType = "PKG";
			Importer.SetValue(packLine.JL_PackageCountInfo, line, DataImporter.Constants.PackingLineFields.PackageCount);
			Importer.SetValue(packLine.JL_ActualWeightInfo, line, DataImporter.Constants.PackingLineFields.Weight);
			Importer.SetValue(packLine.JL_WidthInfo, line, DataImporter.Constants.PackingLineFields.Width);
			Importer.SetValue(packLine.JL_LengthInfo, line, DataImporter.Constants.PackingLineFields.Length);
			Importer.SetValue(packLine.JL_HeightInfo, line, DataImporter.Constants.PackingLineFields.Height);
			Importer.SetValue(packLine.JL_UnitOfDimensionInfo, line, DataImporter.Constants.PackingLineFields.UM);
			Importer.SetValue(packLine.JL_ActualWeightUQInfo, line, DataImporter.Constants.PackingLineFields.UW);
			return packLine;
		}

		ArrayList GenerateExcelRecordList1()
		{
			ArrayList recordList = new ArrayList();
			recordList.Add(new string[] { "S1R1C1", "S1R1C2", "S1R1C3", "S1R1C4", "S1R1C5", "S1R1C6", "S1R1C7", "S1R1C8", "S1R1C9", "S1R1C10", "S1R1C11" });
			recordList.Add(new string[] { "S1R2C1", "", "", "", "", "", "", "", "", "", "" });
			recordList.Add(new string[] { "S1R3C1", "", "", "", "", "", "", "", "", "", "" });
			recordList.Add(new string[] { "A1", "B1" });
			recordList.Add(new string[] { "A2", "B2" });
			recordList.Add(new string[] { "A3", "B3" });
			recordList.Add(new string[] { "A4", "B4" });
			recordList.Add(new string[] { "A5", "B5" });
			recordList.Add(new string[] { "A6", "B6" });
			recordList.Add(new string[] { "A7", "B7" });
			recordList.Add(new string[] { "A8", "B8" });
			recordList.Add(new string[] { "A9", "B9" });
			recordList.Add(new string[] { "A10", "B10" });
			recordList.Add(new string[] { "A11", "B11" });
			recordList.Add(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18" });
			return recordList;
		}

		CFSShipment Shipment;
		DataImporter Importer;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		readonly string TestPath = BaseSourcePath + @"Enterprise\Product\Operations\Freight\CFS\CFS.Business\Data\";
		const string PackLine1 = "ME,,200409201708,HB101,HBL,,,,76,X,8,6,CM,5575,KG";
		const string PackLine2 = "2,603GWM,200401010022,INT101,INT,HB101,HBL,,88,900,23,200,CM,6788,KG";
		const string PackLine3 = "2,603GWM,200401010022,,,INT101,INT,,88,900,23,200,CM,6788,KG";
		const string PackLine4 = "2,603GWM,200401010022,,,,,,88,900,23,200,CM,6788,KG";
		const string PackLine5 = "2,603GWM,200401010022,101,HBL,INT101,INT,,88,900,23,200,CM,6788,KG";
		const string PackLine6 = "2,603GWM,200401010022,101,HBL,INT101,HBL,,88,900,23,200,CM,6788,KG";
		const string PackLine7 = "Drivers Name,Rego,200501280437,101,HBL,INT101,INT,,99,900,23,200,CM,6788,KG";
		const string PackLine8 = "Fread,Finger1,200501280437,101,HBL,INT101,INT,,88,900,23,200,CM,6788,KG";

		#endregion
	}
}
