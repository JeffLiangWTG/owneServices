using CargoWise.Common;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class WhsDocketDataFormatterTest : WhsTestCaseWithFactory
	{
		public void TestFormatCSVData()
		{
			var line = new OCsvLine("test, description, bondedkey", ',');
			AssertEquals("Precondition: Should have 3 extracted fields", 3, line.FieldValues.Length);
			Formatter.FormatCSVData(line);
			for (int i = 0; i < line.FieldValues.Length; i++)
			{
				AssertEquals(line.FieldValues[i].ToUpper(), line.FieldValues[i]);
			}
		}

		public void TestFormatXsdDocket()
		{
			SetupDataForXsdDocket();
			Formatter.FormatXsdDocketData(XsdDocket);
			AssertFormattedXsdDocketData();
		}

		protected virtual void SetupDataForXsdDocket()
		{
			XsdDocket.Identifier.Reference = "order";
			XsdDocket.DocketDetail.WarehouseCode = "whs";
			XsdDocket.DocketDetail.CustomerReference = "customer ref";
			XsdDocket.DocketDetail.TransportReference = "transport ref";
			XsdDocket.DocketDetail.TransportServiceLevel = "d2d";
			XsdDocket.DocketDetail.ServiceLevel = "tsl";
		}

		protected virtual void AssertFormattedXsdDocketData()
		{
			AssertEquals(XsdDocket.Identifier.Reference.ToUpper(), XsdDocket.Identifier.Reference);
			AssertEquals(XsdDocket.DocketDetail.WarehouseCode.ToUpper(), XsdDocket.DocketDetail.WarehouseCode);
			AssertEquals(XsdDocket.DocketDetail.CustomerReference.ToUpper(), XsdDocket.DocketDetail.CustomerReference);
			AssertEquals(XsdDocket.DocketDetail.TransportReference.ToUpper(), XsdDocket.DocketDetail.TransportReference);
			AssertEquals(XsdDocket.DocketDetail.TransportServiceLevel.ToUpper(), XsdDocket.DocketDetail.TransportServiceLevel);
			AssertEquals(XsdDocket.DocketDetail.ServiceLevel.ToUpper(), XsdDocket.DocketDetail.ServiceLevel);
		}

		public void TestFormatXsdDocketLineData()
		{
			SetupDataForXsdDocketLine();
			Formatter.FormatXsdDocketLineData(XsdLine);
			AssertFormattedXsdDocketLineData();
		}

		protected virtual void SetupDataForXsdDocketLine()
		{
			XsdLine.Product = "product";
			XsdLine.Description = "description";
			XsdLine.ProductUQ = "kg";
			XsdLine.LineComments = "line comments";
			XsdLine.LineAttributes.BondedEntryKey = "bondedkey1";
			XsdLine.LineAttributes.PartAttribute1 = "attribute1";
			XsdLine.LineAttributes.PartAttribute2 = "attribute2";
			XsdLine.LineAttributes.PartAttribute3 = "attribute3";
			XsdLine.LineAttributes.CustomAttribute1 = "custom attribue1";
			XsdLine.LineAttributes.CustomAttribute2 = "custom attribue2";
			XsdLine.LineAttributes.CustomAttribute3 = "custom attribue3";
			XsdLine.LineAttributes.CustomAttribute1 = "custom attribue4";
			XsdLine.LineAttributes.CustomAttribute2 = "custom attribue5";
			XsdLine.LineAttributes.CustomAttribute3 = "custom attribue6";

			XsdLine.CustomsData.AddInfo = "addinfo";
			XsdLine.CustomsData.BondedWhsQuantityUnit = "bu";
			XsdLine.CustomsData.CountryOfOrigin = "us";
			XsdLine.CustomsData.CustomsQuantityUnit = "cu";
			XsdLine.CustomsData.EntryKey = "entrykey";
			XsdLine.CustomsData.TILVCurrency = "cur";
			XsdLine.CustomsData.DeclarationReference = "declarationref";
		}

		protected virtual void AssertFormattedXsdDocketLineData()
		{
			AssertEquals(XsdLine.Product.ToUpper(), XsdLine.Product);
			AssertEquals(XsdLine.Description.ToUpper(), XsdLine.Description);
			AssertEquals(XsdLine.ProductUQ.ToUpper(), XsdLine.ProductUQ);
			AssertEquals(XsdLine.LineComments.ToUpper(), XsdLine.LineComments);
			AssertEquals(XsdLine.LineAttributes.PartAttribute1.ToUpper(), XsdLine.LineAttributes.PartAttribute1);
			AssertEquals(XsdLine.LineAttributes.PartAttribute2.ToUpper(), XsdLine.LineAttributes.PartAttribute2);
			AssertEquals(XsdLine.LineAttributes.PartAttribute3.ToUpper(), XsdLine.LineAttributes.PartAttribute3);
			AssertEquals(XsdLine.LineAttributes.CustomAttribute1.ToUpper(), XsdLine.LineAttributes.CustomAttribute1);
			AssertEquals(XsdLine.LineAttributes.CustomAttribute2.ToUpper(), XsdLine.LineAttributes.CustomAttribute2);
			AssertEquals(XsdLine.LineAttributes.CustomAttribute3.ToUpper(), XsdLine.LineAttributes.CustomAttribute3);
			AssertEquals(XsdLine.LineAttributes.CustomAttribute4.ToUpper(), XsdLine.LineAttributes.CustomAttribute4);
			AssertEquals(XsdLine.LineAttributes.CustomAttribute5.ToUpper(), XsdLine.LineAttributes.CustomAttribute5);
			AssertEquals(XsdLine.LineAttributes.CustomAttribute6.ToUpper(), XsdLine.LineAttributes.CustomAttribute6);

			AssertEquals("ADDINFO", XsdLine.CustomsData.AddInfo);
			AssertEquals("BU", XsdLine.CustomsData.BondedWhsQuantityUnit);
			AssertEquals("US", XsdLine.CustomsData.CountryOfOrigin);
			AssertEquals("CU", XsdLine.CustomsData.CustomsQuantityUnit);
			AssertEquals("ENTRYKEY", XsdLine.CustomsData.EntryKey);
			AssertEquals("CUR", XsdLine.CustomsData.TILVCurrency);
			AssertEquals("DECLARATIONREF", XsdLine.CustomsData.DeclarationReference);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Formatter = GetNewDocketDataFormatter();
			XsdDockets = new Xsd.WhsDockets();
			XsdDocket = XsdDockets.WhsDocket.AddNew();
			XsdLine = XsdDocket.DocketLines.AddNew();
		}

		protected virtual WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsDocketDataFormatter();
		}

		protected Xsd.WhsDockets XsdDockets;
		protected Xsd.WhsDocket XsdDocket;
		protected Xsd.WhsDocketLine XsdLine;
		protected WhsDocketDataFormatter Formatter;

		#endregion
	}
}
