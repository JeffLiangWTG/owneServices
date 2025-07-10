using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusLineTariffDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTariff()
		{
			CusLineTariffDetail.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			Validation.ValidateBZ_Tariff();
			AssertEquals(true, CusLineTariffDetail.BZ_TariffInfo.HasMessageErrors());
			AssertEquals(true, cusLineTariffDetail.BZ_TariffInfo.HasMessageError("You have not entered a Commodity Code."));
			CusLineTariffDetail.BZ_Tariff = "TEST";
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasMessageErrors());
			cusLineTariffDetail.BZ_Tariff = "OVR";
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasMessageErrors());
			cusLineTariffDetail.BZ_Tariff = "MV";
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasMessageErrors());
		}

		public void TestQty1()
		{
			Validation.ValidateBZ_Qty1();
			AssertEquals(true, CusLineTariffDetail.BZ_Qty1Info.HasWarning("You have not entered a quantity for this product code."));
			CusLineTariffDetail.BZ_Qty1 = -1m;
			Validation.ValidateBZ_Qty1();
			AssertEquals(true, CusLineTariffDetail.BZ_Qty1Info.HasWarning("You have not entered a quantity for this product code."));
			CusLineTariffDetail.BZ_Qty1 = 1m;
			Validation.ValidateBZ_Qty1();
			AssertEquals(false, CusLineTariffDetail.BZ_Qty1Info.HasWarning("You have not entered a quantity for this product code."));
		}

		[TestDate(2018, 6, 23)]
		public void TestLegacyUQ1()
		{
			Validation.ValidateBZ_UQ1();
			AssertEquals("UQ should only error when Qty has been entered", false, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			CusLineTariffDetail.BZ_Qty1 = 1m;
			Validation.ValidateBZ_UQ1();
			AssertEquals(true, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			CusLineTariffDetail.BZ_UQ1 = "ABC";
			Validation.ValidateBZ_UQ1();
			AssertEquals(true, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			CusLineTariffDetail.BZ_UQ1 = "KGS";
			Validation.ValidateBZ_UQ1();
			AssertEquals(false, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			var invLine = Factory.New<JobComInvoiceLine>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invLine.JI_JZ = invoiceHeader.PK;
			CusLineTariffDetail.BZ_ParentID = invLine.PK;
			CusLineTariffDetail.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invLine.JI_Tariff = "12119019";
			cusLineTariffDetail.BZ_Tariff = "HSACHD01000";
			AssertEquals("UQ should default", "KGM", cusLineTariffDetail.BZ_UQ1);
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			cusLineTariffDetail.BZ_UQ1 = "NMB";
			AssertEquals("HSA Complimentary Health Product codes can only have KGM for UQ", true, cusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			AssertEquals(true, cusLineTariffDetail.BZ_UQ1Info.HasMessageError("Unit of Qty must be 'KGM' for all HSA Complimentary Health Product codes."));
		}

		public void TestUQ1()
		{
			Validation.ValidateBZ_UQ1();
			AssertEquals("UQ should only error when Qty has been entered", false, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			CusLineTariffDetail.BZ_Qty1 = 1m;
			Validation.ValidateBZ_UQ1();
			AssertEquals(true, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			CusLineTariffDetail.BZ_UQ1 = "ABC";
			Validation.ValidateBZ_UQ1();
			AssertEquals(true, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			CusLineTariffDetail.BZ_UQ1 = "KGS";
			Validation.ValidateBZ_UQ1();
			AssertEquals(false, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
			var invLine = Factory.New<JobComInvoiceLine>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invLine.JI_JZ = invoiceHeader.PK;
			CusLineTariffDetail.BZ_ParentID = invLine.PK;
			CusLineTariffDetail.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invLine.JI_Tariff = "12119019";
			cusLineTariffDetail.BZ_Tariff = "HSAIPU";
			AssertEquals("UQ should default", "TNE", cusLineTariffDetail.BZ_UQ1);
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_UQ1Info.HasMessageErrors());
		}

		public void TestCusLineProducts()
		{
			JobComInvoiceLine invLine = Factory.New<JobComInvoiceLine>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invLine.JI_JZ = invoiceHeader.PK;
			CusLineTariffDetail.BZ_ParentID = invLine.PK;
			CusLineTariffDetail.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invLine.JI_Tariff = "01063100";
			CusLineTariffDetail.BZ_Tariff = TariffCommodities[0].ZZ1_TariffCode;
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasWarnings());
			invLine.JI_Tariff = "01029020";
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasWarnings());
			invLine.JI_Tariff = "";
			CusLineTariffDetail.BZ_Tariff = "TestValue";
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasWarnings());
			CusLineTariffDetail.BZ_UQ1 = "";
			CusLineTariffDetail.BZ_Qty1 = 0;
			invLine.JI_Tariff = "90189090";
			invLine.JI_InvoiceQuantity = 265m;
			invLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			CusLineTariffDetail.BZ_Tariff = "CodeNotInList";
			Validation.ValidateBZ_Tariff();
			AssertEquals(true, CusLineTariffDetail.BZ_TariffInfo.HasWarnings());
			AssertEquals("", CusLineTariffDetail.BZ_UQ1);
			CusLineTariffDetail.BZ_Tariff = "MISC";
			Validation.ValidateBZ_Tariff();
			AssertEquals(false, CusLineTariffDetail.BZ_TariffInfo.HasWarnings());
			AssertEquals("NMB", cusLineTariffDetail.BZ_UQ1);
		}

		public void TestNACWCLicenseError()
		{
			JobComInvoiceLine invLine = Factory.New<JobComInvoiceLine>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invLine.JI_JZ = invoiceHeader.PK;
			CusLineTariffDetail.BZ_ParentID = invLine.PK;
			CusLineTariffDetail.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invLine.JI_Tariff = "29049100";
			CusLineTariffDetail.BZ_Tariff = TariffCommodities[0].ZZ1_TariffCode;
			AssertEquals("Product Code should be", "S3AC02", CusLineTariffDetail.BZ_Tariff);
			Validation.ValidateBZ_Tariff();
			AssertHasMessageError(CusLineTariffDetail.BZ_TariffInfo, "A National Authority (Chemical Weapons Convention), NA(CWC), Licence is required before Import or Export of these goods. Enter your license number in the CA License grid on the Declaration Details tab.");
			CALicenceNumber caLicence = Factory.NewWithValidTestData<CALicenceNumber>();
			caLicence.CY_ParentID = declaration.PK;
			caLicence.CY_ParentTableCode = "JE";
			caLicence.CY_Data = "C837499J";
			declaration.CALicences.Add(caLicence);
			Validation.ValidateBZ_Tariff();
			AssertNoMessageError(CusLineTariffDetail.BZ_TariffInfo, "A National Authority (Chemical Weapons Convention), NA(CWC), Licence is required before Import or Export of these goods. Enter your license number in the CA License grid on the Declaration Details tab.");
		}

		[TestDate(2018, 6, 23)]
		public void TestHSAComplimentaryChineseMedicine()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invLine.JI_JZ = invoiceHeader.PK;
			CusLineTariffDetail.BZ_ParentID = invLine.PK;
			CusLineTariffDetail.BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			invLine.JI_Tariff = "12119019";
			CusLineTariffDetail.BZ_Tariff = TariffCommodities[0].ZZ1_TariffCode;
			AssertEquals("Product Code should now be sorted", "HSACHD01000", CusLineTariffDetail.BZ_Tariff);
			Validation.ValidateBZ_Tariff();
			AssertHasMessageError(CusLineTariffDetail.BZ_TariffInfo, "For Chinese proprietary medicines (CPM), the importer's licence number is to be indicated in the 'Licence Number', (on Details tab: CA Licences), and CPM product listing number is to be indicated under 'CA/SC Code 1', (on Invoice Lines Additional Details tab).");
			var caLicence = Factory.NewWithValidTestData<CALicenceNumber>();
			caLicence.CY_ParentID = declaration.PK;
			caLicence.CY_ParentTableCode = "JE";
			caLicence.CY_Data = "CPMI0099";
			declaration.CALicences.Add(caLicence);
			Validation.ValidateBZ_Tariff();
			AssertNoMessageError(CusLineTariffDetail.BZ_TariffInfo, "For Chinese proprietary medicines (CPM), the importer's licence number is to be indicated in the 'Licence Number', (on Details tab: CA Licences), and CPM product listing number is to be indicated under 'CA/SC Code 1', (on Invoice Lines Additional Details tab).");
			AssertHasMessageError(CusLineTariffDetail.BZ_TariffInfo, "For Chinese proprietary medicines (CPM), the CPM product listing number needs to be indicated under 'CA/SC Code 1', (on Invoice Lines Additional Details tab).");
			var cascCode = Factory.New<CASCCode1>();
			cascCode.CY_ParentID = invLine.PK;
			cascCode.CY_ParentTableCode = "JI";
			cascCode.CY_Data = "CPM349238";
			invLine.CASCCode1s.Add(cascCode);
			Validation.ValidateBZ_Tariff();
			AssertNoMessageError(CusLineTariffDetail.BZ_TariffInfo, "For Chinese proprietary medicines (CPM), the CPM product listing number needs to be indicated under 'CA/SC Code 1', (on Invoice Lines Additional Details tab).");
			CusLineTariffDetail.BZ_Tariff = TariffCommodities[1].ZZ1_TariffCode;
			AssertEquals("Next HSA Product Code should be", "HSACHD02000", CusLineTariffDetail.BZ_Tariff);
		}

		#region CusLineTariffDetail
		CusLineTariffDetail CusLineTariffDetail
		{
			get
			{
				return cusLineTariffDetail ?? (cusLineTariffDetail = Factory.New<CusLineTariffDetail>());
			}
		}

		CusLineTariffDetail cusLineTariffDetail;
		TariffView[] TariffCommodities => tariffCommodities ?? (tariffCommodities = CusLineTariffDetail.InvoiceLine?.UniversalTariff?.GetTariffCommodities(ZDateTime.Now));
		TariffView[] tariffCommodities;
		#endregion
		#region Validation
		CusLineTariffDetailValidation Validation
		{
			get
			{
				return (CusLineTariffDetailValidation)CusLineTariffDetail.Validation;
			}
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(tariffType, "12119019");
			var commodity1 = helper.CreateCommodity(tariff, "HSACHD01000", new ZDateTime(2014, 6, 1), new ZDateTime(2018, 6, 23, 23, 59, 0));
			helper.CreateTariffUOM(commodity1, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.KGM);
			helper.CreateCommodity(tariff, "HSACHD02000", new ZDateTime(2014, 6, 1), new ZDateTime(2018, 6, 23, 23, 59, 0));
			var commodity2 = helper.CreateCommodity(tariff, "HSAIPU");
			helper.CreateTariffUOM(commodity2, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.TNE);
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "01063100");
			helper.CreateCommodity(tariff1, "1234");
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "01029020");
			helper.CreateCommodity(tariff2, "2134");
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "90189090");
			helper.CreateCommodity(tariff3, "MISC");
			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "29049100");
			helper.CreateCommodity(tariff4, "S3AC02");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
	}
}
