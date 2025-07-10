using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.FDARecapPrinting.Testing
{
	[TestedType(typeof(FDARecapLine))]
	sealed class FDARecapLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice1 = declaration.Invoices.AddNew();

			var invoiceline1 = invoice1.InvoiceLines.AddNew();
			invoiceline1.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceline1.JI_LinePrice = 1200;
			invoiceline1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Italy;

			var fdaLine = invoiceline1.FDAs.AddNew();
			fdaLine.US_FDAValue = 1000m;
			fdaLine.US_FDALineNo = 1;
			fdaLine.US_FDAConfirmDate = ZDateTime.Today;
			fdaLine.US_PNC = "123456789654";
			fdaLine.US_FDAContainerDimType = CylindricalRectangularList.Codes.Cylindrical;
			fdaLine.US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals;
			fdaLine.US_ContainerDim1 = 10m;
			fdaLine.US_ContainerDim2 = 20m;
			fdaLine.US_ContainerDim3 = 30m;

			fdaLine.US_FDAQty1 = 1.3243424m;
			fdaLine.US_FDAMeasure1 = "1";

			fdaLine.US_FDAQty2 = 2.7887m;
			fdaLine.US_FDAMeasure2 = "2";

			fdaLine.US_FDAQty3 = 3.68754654m;
			fdaLine.US_FDAMeasure3 = "3";

			fdaLine.US_FDAQty4 = 4.112333m;
			fdaLine.US_FDAMeasure4 = "4";

			fdaLine.US_FDAQty5 = 5.87878m;
			fdaLine.US_FDAMeasure5 = "5";

			fdaLine.US_FDAQty6 = 6.78878m;
			fdaLine.US_FDAMeasure6 = "6";

			fdaLine.US_TradeBrandName = "TEST BRAND NAME";
			fdaLine.US_FDAProductCode = "123456";
			fdaLine.US_UC_NKFDAProduction = Core.Constants.CountryCodes.Mexico;
			fdaLine.US_FDACommercialDesc = "Dried bone";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new FDARecapLineCollection(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			var line = collection[0];

			AssertEquals(ZString.Empty, line.UltimateConsigneeAddress1);
			AssertEquals(ZString.Empty, line.UltimateConsigneeAddress2);
			AssertEquals(ZString.Empty, line.ShipperAddress1);
			AssertEquals(ZString.Empty, line.ShipperAddress2);
			AssertEquals(ZString.Empty, line.ManufacturerAddress1);
			AssertEquals(ZString.Empty, line.ManufacturerAddress2);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "N.L. SHUN AND PARTNER GMBH & CO KG  BAKEN-WURTTEMB";
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			var address = consignee.Addresses.AddNew(OrgAddressType.CustomsAddressOfRecord, true);
			address.OA_Address1 = "SCHEINBERGWEG 6-7 HERTZSTRASSE 175";
			address.OA_Address2 = "BAKEN-WURTTEMBERG 15-96 BAKEN-PYUJH";
			address.OA_City = "MELLRICHSTADT";
			address.OA_State = "JU";
			address.OA_PostCode = "50060";
			address.OA_RL_NKRelatedPortCode = "DEFRA";
			invoiceline1.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SUPPLIER";
			shipper.OH_FullName = "MULFORTER ZEUGRUCKEREI & FARBERET  GMBH & CO KG DS";
			address = shipper.MainAddress;
			address.OA_Address1 = "DUVENSTR 98-268 HERTZSTRASSE 175 HERTZSTRASSE 1754";
			address.OA_Address2 = "D-41199 MONCHENGLADBACH, GERMANY MONCHENGLADBACHYU";
			address.OA_City = "KARLSRUHE";
			address.OA_State = "HE";
			address.OA_PostCode = "76187";
			address.OA_RL_NKRelatedPortCode = "DEFRA";
			fdaLine.US_FDAShipperAddress = address.PK;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "OH1" + new Random().Next(1000000).ToString();
			manufacturer.OH_FullName = "L'OREAL PRODUKTION DEUTSCHLAND GMBH & CO KG";
			address = manufacturer.MainAddress;
			address.OA_Address1 = "HERTZSTRASSE 175";
			address.OA_Address2 = "BAKEN-WURTTEMBERG";
			address.OA_City = "KARLSRUHE";
			address.OA_State = "BW";
			address.OA_PostCode = "76187";
			address.OA_RL_NKRelatedPortCode = "DEFRA";
			fdaLine.US_FDAManufacturerAddress = address.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			collection = new FDARecapLineCollection(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			line = collection[0];

			AssertEquals("Brand Name", "TEST BRAND NAME", line.TradeBrandName);
			AssertEquals(Core.Constants.CountryCodes.Mexico, line.ProductionCountry);
			AssertEquals("123456", line.ProductCode);
			AssertEquals("Dried bone", line.CommercialDesc);
			AssertEquals("123456789654", line.ConfirmationNumber);
			AssertEquals(ZDateTime.Today, line.ConfirmationDate);
			AssertEquals("1200", line.FDAValue);
			AssertEquals("0001", line.FDALineNo);
			AssertEquals(ZString.Empty, line.CountryOfOriginForLine);

			AssertEquals(CylindricalRectangularList.Codes.Cylindrical, line.ContainerDimentionType);
			AssertEquals(FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals, line.ContainerDimentionUQ);
			AssertEquals(10m, line.ContainerDimention1);
			AssertEquals(20m, line.ContainerDimention2);
			AssertEquals(30m, line.ContainerDimention3);
			AssertEquals("N.L. SHUN AND PARTNER GMBH & CO KG  BAKEN-WURTTEMB SCHEINBERGWEG 6-7 HERTZSTRASSE 175", line.UltimateConsigneeAddress1);
			AssertEquals("BAKEN-WURTTEMBERG 15-96 BAKEN-PYUJH 50060 MELLRICHSTADT Germany", line.UltimateConsigneeAddress2);
			AssertEquals("MULFORTER ZEUGRUCKEREI & FARBERET  GMBH & CO KG DS DUVENSTR 98-268 HERTZSTRASSE 175 HERTZSTRASSE 1754", line.ShipperAddress1);
			AssertEquals("D-41199 MONCHENGLADBACH, GERMANY MONCHENGLADBACHYU 76187 KARLSRUHE Germany", line.ShipperAddress2);
			AssertEquals("L'OREAL PRODUKTION DEUTSCHLAND GMBH & CO KG HERTZSTRASSE 175", line.ManufacturerAddress1);
			AssertEquals("BAKEN-WURTTEMBERG 76187 KARLSRUHE Germany", line.ManufacturerAddress2);

			AssertFDAQty(line.Quantity1, "1.32", line.UQ1, "1");
			AssertFDAQty(line.Quantity2, "2.79", line.UQ2, "2");
			AssertFDAQty(line.Quantity3, "3.69", line.UQ3, "3");
			AssertFDAQty(line.Quantity4, "4.11", line.UQ4, "4");
			AssertFDAQty(line.Quantity5, "5.88", line.UQ5, "5");
			AssertFDAQty(line.Quantity6, "6.79", line.UQ6, "6");
		}

		protected override BusinessObject GetNewBusinessObject() => new FDARecapLine(Factory.NewWithValidTestData<FDA>(), false);

		void AssertFDAQty(ZString quantity, ZString expectedQty, ZString uQ, ZString expectedUQ)
		{
			AssertEquals(quantity, expectedQty);
			AssertEquals(uQ, expectedUQ);
		}
	}
}
