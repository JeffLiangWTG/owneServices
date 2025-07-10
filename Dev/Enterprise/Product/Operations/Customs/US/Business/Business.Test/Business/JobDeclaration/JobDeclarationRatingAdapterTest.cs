using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Customs.US.Business.Testing
{
	public class JobDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		public void TestTariffsPerShipment()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var autoRating = (IAutoRatingCustomsInfo)declaration.RatingAdapter;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "87654321";
			invoiceLine1.US_SupTariff = string.Empty;

			AssertEquals(1, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(1, autoRating.TariffsPerShipment[0].EntryLines);

			invoiceLine1.US_SupTariff = "85671030";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "75167201";
			invoiceLine2.US_SupTariff = "00102321";

			AssertEquals(4, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(4, autoRating.TariffsPerShipment[0].EntryLines);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "75167201";
			invoiceLine3.US_SupTariff = "00102321";

			AssertEquals(4, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(6, autoRating.TariffsPerShipment[0].EntryLines);

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "87654321";
			invoiceLine4.US_SupTariff = "87654321";

			AssertEquals(4, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(8, autoRating.TariffsPerShipment[0].EntryLines);

			invoiceLine1.SupFormattedAdditionalTariff1 = "11111111";
			AssertEquals(5, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(9, autoRating.TariffsPerShipment[0].EntryLines);

			invoiceLine1.SupFormattedAdditionalTariff2 = "22222222";
			AssertEquals(6, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(10, autoRating.TariffsPerShipment[0].EntryLines);

			invoiceLine1.SupFormattedAdditionalTariff3 = "11111111";
			AssertEquals(6, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(11, autoRating.TariffsPerShipment[0].EntryLines);

			invoiceLine2.SupFormattedAdditionalTariff4 = "33333333";
			AssertEquals(7, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(12, autoRating.TariffsPerShipment[0].EntryLines);

			invoiceLine2.SupFormattedAdditionalTariff5 = "22222222";
			AssertEquals(7, autoRating.TariffsPerShipment[0].InvoiceLines);
			AssertEquals(13, autoRating.TariffsPerShipment[0].EntryLines);
		}

		public void TestTariffsPerInvoice()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			var autoRating = (IAutoRatingCustomsInfo)declaration.RatingAdapter;

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "87654321";
			invoiceLine1.US_SupTariff = "00000001";
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "00102321";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "87654321";
			invoiceLine3.US_SupTariff = "00000002";
			var invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "00102321";
			var invoiceLine5 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "00102006";

			AssertEquals(2, autoRating.TariffsPerInvoice.Count);

			AssertEquals(3, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(4, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);

			invoiceLine1.SupFormattedAdditionalTariff1 = "11111111";
			AssertEquals(4, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(4, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);

			invoiceLine2.SupFormattedAdditionalTariff2 = "11111111";
			AssertEquals(4, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(4, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);

			invoiceLine3.SupFormattedAdditionalTariff3 = "22222222";
			AssertEquals(4, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(5, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);

			invoiceLine4.SupFormattedAdditionalTariff4 = "33333333";
			AssertEquals(4, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(6, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);

			invoiceLine5.SupFormattedAdditionalTariff5 = "22222222";
			AssertEquals(4, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(6, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);
		}

		public void TestGetMeasurementsForAutoRating_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.DOTs.AddNew();

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.FCCs.AddNew();

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.FDAs.AddNew();
			invoiceLine3.FCCs.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("FDA count", 3m, rateableMeasures.GetActual(MeasureType.FDALine));
				AssertEquals("FCC count", 2m, rateableMeasures.GetActual(MeasureType.FCCLine));
				AssertEquals("DOT count", 1m, rateableMeasures.GetActual(MeasureType.DOTLine));
				AssertEquals("No Lacey measure", false, rateableMeasures.HasMeasureType(MeasureType.LaceyLine));
			});
		}

		public void TestGetMeasurementsForAutoRating_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.DOTs.AddNew();
			invoiceLine.FCCs.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("FDA count: Not relevant for Export", false, rateableMeasures.HasMeasureType(MeasureType.FDALine));
				AssertEquals("FCC count: Not relevant for Export", false, rateableMeasures.HasMeasureType(MeasureType.FCCLine));
				AssertEquals("DOT count: Not relevant for Export", false, rateableMeasures.HasMeasureType(MeasureType.DOTLine));
				AssertEquals("Lacey count: Not relevant for Export", false, rateableMeasures.HasMeasureType(MeasureType.LaceyLine));
			});
		}

		public void TestGetMeasurementsForPGAs()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			var fdaOne = invoiceLine.ACE_FDALines.AddNew();
			fdaOne.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fdaOne.US_FDAForcePN = true;
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.OMCHeaders.AddNew();
			invoiceLine.CPSCHeaders.AddNew();
			invoiceLine.DEAHeaders.AddNew();
			invoiceLine.NHTSALines.AddNew();
			invoiceLine.LaceyActLines.AddNew();

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.FCCs.AddNew();
			var fdaTwo = invoiceLine2.ACE_FDALines.AddNew();
			fdaTwo.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fdaTwo.US_FDAForcePN = true;
			fdaTwo.US_PNC = "PNC001002001";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var fdaThree = invoiceLine3.ACE_FDALines.AddNew();
			fdaThree.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fdaThree.US_FDAForcePN = true;
			fdaThree.US_PNC = "PNC001002002";
			fdaThree.US_IsPNCFromMsg = true;
			invoiceLine3.OMCHeaders.AddNew();
			invoiceLine3.CPSCHeaders.AddNew();
			invoiceLine3.DEAHeaders.AddNew();
			invoiceLine3.FCCs.AddNew();
			invoiceLine3.LaceyActLines.AddNew();

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_OMCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_DEAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_FCCIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("FDA count", 4m, rateableMeasures.GetActual(MeasureType.FDALine));
				AssertEquals("PNFDA count", 2m, rateableMeasures.GetActual(MeasureType.PNFDALine));
				AssertEquals("OMC count", 2m, rateableMeasures.GetActual(MeasureType.OMCLine));
				AssertEquals("CPSC count", 2m, rateableMeasures.GetActual(MeasureType.CPSCLine));
				AssertEquals("DEA count", 2m, rateableMeasures.GetActual(MeasureType.DEALine));
				AssertEquals("FCC count", 2m, rateableMeasures.GetActual(MeasureType.FCCLine));
				AssertEquals("DOT/NHTSA count", 1m, rateableMeasures.GetActual(MeasureType.DOTLine));
				AssertEquals("Lacey count", 2m, rateableMeasures.GetActual(MeasureType.LaceyLine));
				AssertEquals("DOT Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.DOTDisclaim));
				AssertEquals("FDA Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.FDADisclaim));
				AssertEquals("OMC Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.OMCDisclaim));
				AssertEquals("DEA Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.DEADisclaim));
				AssertEquals("FCC Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.FCCDisclaim));
				AssertEquals("CPSC Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.CPSCDisclaim));
				AssertEquals("Lacey Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.LaceyDisclaim));
			});
		}

		public void TestGetMeasurementsForOtherPGAs()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			var nmfs370Line = invoiceLine.NMFSLines.AddNew();
			nmfs370Line.US_ProgramType = NMFSProgramCodeList.Codes._370;

			var nmfsCOALine = invoiceLine.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			var nmfsAMRLine = invoiceLine.NMFSLines.AddNew();
			nmfsAMRLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;

			var nmfsHMSLine = invoiceLine.NMFSLines.AddNew();
			nmfsHMSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;

			var nmfsSIMLine = invoiceLine.NMFSLines.AddNew();
			nmfsSIMLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;

			invoiceLine.AMSLines.AddNew();
			var or1Line = invoiceLine.AMSLines.AddNew();
			or1Line.US_Program = AMSProgramList.Codes.OR1;
			invoiceLine.APHISHeaders.AddNew();
			invoiceLine.ATFLines.AddNew();
			invoiceLine.FSISLines.AddNew();
			invoiceLine.FWSHeaders.AddNew();
			invoiceLine.PSTLines.AddNew();
			invoiceLine.USHFCHeaders.AddNew();
			invoiceLine.VehicleLines.AddNew();
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ODSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;

			var ttbLine1 = invoiceLine.TTBLines.AddNew();
			ttbLine1.COLAAndCertificates.AddNew();
			ttbLine1.COLAAndCertificates.AddNew();

			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			var invoiceLine3 = declaration.InvoiceLines.AddNew();

			nmfs370Line = invoiceLine3.NMFSLines.AddNew();
			nmfs370Line.US_ProgramType = NMFSProgramCodeList.Codes._370;

			nmfsCOALine = invoiceLine3.NMFSLines.AddNew();
			nmfsCOALine.US_ProgramType = NMFSProgramCodeList.Codes.COA;

			nmfsAMRLine = invoiceLine3.NMFSLines.AddNew();
			nmfsAMRLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsAMRLine.US_AMLRPermitNumber = "test1";

			nmfsAMRLine = invoiceLine3.NMFSLines.AddNew();
			nmfsAMRLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsAMRLine.US_AMLRPermitNumber = "test2";

			nmfsHMSLine = invoiceLine3.NMFSLines.AddNew();
			nmfsHMSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;

			nmfsSIMLine = invoiceLine3.NMFSLines.AddNew();
			nmfsSIMLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;

			invoiceLine3.AMSLines.AddNew();
			var or2Line = invoiceLine3.AMSLines.AddNew();
			or2Line.US_Program = AMSProgramList.Codes.OR2;
			invoiceLine3.APHISHeaders.AddNew();
			invoiceLine3.ATFLines.AddNew();
			invoiceLine3.FSISLines.AddNew();
			invoiceLine3.FWSHeaders.AddNew();
			invoiceLine3.PSTLines.AddNew();
			invoiceLine3.USHFCHeaders.AddNew();
			invoiceLine3.VehicleLines.AddNew();
			invoiceLine3.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine3.US_TSCAInd = OGAIndicatorList.Codes.Declared;

			var ttbLine2 = invoiceLine3.TTBLines.AddNew();
			ttbLine2.COLAAndCertificates.AddNew();

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_ODSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_TSCAInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_AMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_FSISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_PSTIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_HFCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine4.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("NMFS 370 count", 2m, rateableMeasures.GetActual(MeasureType.NMFS370));
				AssertEquals("NMFS COA count", 2m, rateableMeasures.GetActual(MeasureType.NMFSCOA));
				AssertEquals("NMFS AMR count", 3m, rateableMeasures.GetActual(MeasureType.NMFSAMR));
				AssertEquals("NMFS HMS count", 2m, rateableMeasures.GetActual(MeasureType.NMFSHMS));
				AssertEquals("NMFS SIM count", 2m, rateableMeasures.GetActual(MeasureType.NMFSSIM));
				AssertEquals("AMS count", 2m, rateableMeasures.GetActual(MeasureType.AMS));
				AssertEquals("NOP count", 2m, rateableMeasures.GetActual(MeasureType.AMS));
				AssertEquals("APHIS count", 2m, rateableMeasures.GetActual(MeasureType.APHIS));
				AssertEquals("ATF count", 2m, rateableMeasures.GetActual(MeasureType.ATF));
				AssertEquals("FSIS count", 2m, rateableMeasures.GetActual(MeasureType.FSIS));
				AssertEquals("FWS count", 2m, rateableMeasures.GetActual(MeasureType.FWS));
				AssertEquals("PST count", 2m, rateableMeasures.GetActual(MeasureType.PST));
				AssertEquals("HFC count", 2m, rateableMeasures.GetActual(MeasureType.HFC));
				AssertEquals("TTB count", 2m, rateableMeasures.GetActual(MeasureType.TTB));
				AssertEquals("TCC count", 3m, rateableMeasures.GetActual(MeasureType.TCC));
				AssertEquals("VNE count", 2m, rateableMeasures.GetActual(MeasureType.VNE));
				AssertEquals("DDTC count", 2m, rateableMeasures.GetActual(MeasureType.DDTC));
				AssertEquals("ODS count", 1m, rateableMeasures.GetActual(MeasureType.ODS));
				AssertEquals("TSCA count", 2m, rateableMeasures.GetActual(MeasureType.TSCA));
				AssertEquals("TTB Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.TTBDisclaim));
				AssertEquals("AMS NOP Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.AMSNOPDisclaim));
				AssertEquals("ODS Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.ODSDisclaim));
				AssertEquals("TSCA Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.TSCADisclaim));
				AssertEquals("AMS Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.AMSDisclaim));
				AssertEquals("FWS Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.FWSDisclaim));
				AssertEquals("FSIS Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.FSISDisclaim));
				AssertEquals("APHIS Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.APHISDisclaim));
				AssertEquals("VNE Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.VNEDisclaim));
				AssertEquals("PST Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.PSTDisclaim));
				AssertEquals("HFC Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.HFCDisclaim));
				AssertEquals("NMFS 370 Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.NMFS370Disclaim));
				AssertEquals("NMFS AMR Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.NMFSAMRDisclaim));
				AssertEquals("NMFS HMS Disclaim count", 1m, rateableMeasures.GetActual(MeasureType.NMFSHMSDisclaim));
			});
		}

		public void TestGetMeasurementsForFTZAutoRating1()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.FDAs.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals("FDA count", 2m, rateableMeasures.GetActual(MeasureType.FDALine));
		}

		public void TestGetMeasurementsForFTZAutoRating2()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.O;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals("FDA count", 4m, rateableMeasures.GetActual(MeasureType.FDALine));
		}

		public void TestGetAmountsForAutoRating()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 5000m;
			declaration.JE_InsuranceValue = 1000m;
			declaration.JE_RX_NKInsuranceCurrency = "USD";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			var amounts = declaration.RatingAdapter.MonetaryValues;
			AssertEquals(10000m, amounts.GetMoney(MoneyType.ValueType.GoodsValue, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);
			AssertEquals(5000m, amounts.GetMoney(MoneyType.ValueType.SingleTransactionBondAmount, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);
			AssertEquals(1000m, amounts.GetMoney(MoneyType.ValueType.InsuranceValue, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			amounts = declaration.RatingAdapter.MonetaryValues;
			AssertEquals(10000m, amounts.GetMoney(MoneyType.ValueType.GoodsValue, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);
			AssertEquals(0m, amounts.GetMoney(MoneyType.ValueType.SingleTransactionBondAmount, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);
			AssertEquals(0m, amounts.GetMoney(MoneyType.ValueType.InsuranceValue, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_InsuranceValue = 2000m;
			shipment.JS_RX_NKInsuranceCurrency = "USD";
			declaration.JE_JS = shipment.PK;
			amounts = declaration.RatingAdapter.MonetaryValues;
			AssertEquals(0m, amounts.GetMoney(MoneyType.ValueType.InsuranceValue, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			amounts = declaration.RatingAdapter.MonetaryValues;
			AssertEquals(2000m, amounts.GetMoney(MoneyType.ValueType.InsuranceValue, JobDeclaration.GetLocalCurrency(), invoice.CurrencyConverter).Amount);
		}

		public void TestGetAmountOfDeliveryOrderForAutoRating()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.DeliveryOrders));

			declaration.DeliveryOrderHeaders.AddNew();
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.DeliveryOrders));

			declaration.DeliveryOrderHeaders.AddNew();
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.DeliveryOrders));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.DeliveryOrders));
		}

		public void TestGetAmountOfUniqueLicensesForAutoRating()
		{
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.SteelLicenses, LicencePermitTypeList.Codes._01);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.SG_TPLCertificate, LicencePermitTypeList.Codes._02);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.CA_NAFTA_TPLCertificate, LicencePermitTypeList.Codes._03);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.MX_NAFTA_TPLCertificate, LicencePermitTypeList.Codes._04);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.BeefExportCertificate, LicencePermitTypeList.Codes._05);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.DiamondCertificate, LicencePermitTypeList.Codes._06);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.ATPDEACertificate, LicencePermitTypeList.Codes._07);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.AU_FTA_ExportCertificate, LicencePermitTypeList.Codes._08);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.MXCementLicense, LicencePermitTypeList.Codes._09);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.CAFTA_TPLCertificate, LicencePermitTypeList.Codes._10);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.ALBCertificate, LicencePermitTypeList.Codes._11);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.CottonShirtingFabricLicense, LicencePermitTypeList.Codes._12);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.HaitiEarnedAllowance, LicencePermitTypeList.Codes._13);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.AgriculturalLicense, LicencePermitTypeList.Codes._14);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.CAExportSugarCertificate, LicencePermitTypeList.Codes._16);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.WoolLicense, LicencePermitTypeList.Codes._17);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.CBTPACertificate, LicencePermitTypeList.Codes._18);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.AGOATextileProvisionNumber, LicencePermitTypeList.Codes._19);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.OtherNonStandardVisa, LicencePermitTypeList.Codes._20);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.USDASugarCertificate, LicencePermitTypeList.Codes._21);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.OrganicProductExemptionCertificate, LicencePermitTypeList.Codes._22);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.AMSCertificateOfExemption, LicencePermitTypeList.Codes._23);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate, LicencePermitTypeList.Codes._25);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.MexicanSugarExportLicense, LicencePermitTypeList.Codes._26);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.GeneralNote15cWaiverCertificate, LicencePermitTypeList.Codes._27);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.AluminumLicenses, LicencePermitTypeList.Codes._28);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.CanadianUSMCA_TPLCertificate, LicencePermitTypeList.Codes._29);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.MexicanUSMCA_TPLCertificate, LicencePermitTypeList.Codes._30);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense, LicencePermitTypeList.Codes._31);
			TestGetAmountOfUniqueLicensesForAutoRating(MeasureType.KRExportSteelCertificate, LicencePermitTypeList.Codes.KR);
		}

		void TestGetAmountOfUniqueLicensesForAutoRating(MeasureType measureType, string code)
		{
			var otherCode = LicencePermitTypeList.Codes._01;
			if (code == otherCode)
			{
				otherCode = LicencePermitTypeList.Codes._02;
			}
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(measureType));

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var license1 = invoiceLine1.LicenceAndPermits.AddNew();
			license1.CY_Code = code;
			license1.CY_Data = "Data1";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(measureType));

			var license2 = invoiceLine1.LicenceAndPermits.AddNew();
			license2.CY_Code = otherCode;
			license2.CY_Data = "Data2";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(measureType));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			var license3 = invoiceLine2.LicenceAndPermits.AddNew();
			license3.CY_Code = code;
			license3.CY_Data = "Data3";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(measureType));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			var license4 = invoiceLine3.LicenceAndPermits.AddNew();
			license4.CY_Code = code;
			license4.CY_Data = "Data4";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(measureType));

			var license5 = invoiceLine3.LicenceAndPermits.AddNew();
			license5.CY_Code = code;
			license5.CY_Data = "Data1";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(measureType));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(measureType));
		}

		public void TestGetAmountOfVISANumbersForAutoRating()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.VISANumbers));

			invoiceLine1.US_VisaNo = "Test001";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.VISANumbers));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.US_VisaNo = "Test002";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.VISANumbers));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.US_VisaNo = "Test002";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.VISANumbers));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.VISANumbers));
		}

		public void TestGetAmountOfSpecificProvProgTariffForAutoRating()
		{
			TestGetAmountOfSpecificProvProgTariffForAutoRatingCore("9902", MeasureType.HTS9902Line);
			TestGetAmountOfSpecificProvProgTariffForAutoRatingCore("9903", MeasureType.HTS9903Line);
		}

		void TestGetAmountOfSpecificProvProgTariffForAutoRatingCore(string prefix, MeasureType type)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(type));

			invoiceLine1.SupTariffFormatted = prefix + ".01.01";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(type));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.SupTariffFormatted = prefix + ".01.02";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(type));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.SupTariffFormatted = prefix + ".02.01";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(type));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(type));
		}

		public void TestGetAmountOfSpecificTariffAndAddTariffForAutoRating()
		{
			TestGetAmountOfSpecificAddTariffForAutoRatingCore("9902", MeasureType.HTS9902Line);
			TestGetAmountOfSpecificAddTariffForAutoRatingCore("9903", MeasureType.HTS9903Line);
		}

		public void TestGetAmountOfSpecificAddTariffForAutoRatingCore(string prefix, MeasureType type)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(type));

			invoiceLine1.SupFormattedAdditionalTariff1 = prefix + ".01.01";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(type));

			invoiceLine1.SupFormattedAdditionalTariff2 = prefix + ".01.02";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(type));

			invoiceLine1.JI_Tariff = prefix + ".01.04";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(type));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.SupFormattedAdditionalTariff3 = prefix + ".02.01";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(4m, rateableMeasures.GetActual(type));

			invoiceLine2.SupFormattedAdditionalTariff4 = prefix + ".02.02";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(type));

			invoiceLine2.SupFormattedAdditionalTariff5 = prefix + ".02.03";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(6m, rateableMeasures.GetActual(type));

			invoiceLine2.JI_Tariff = prefix + ".02.04";
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(7m, rateableMeasures.GetActual(type));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(type));
		}

		public void TestGetAmountOfPGALinesForAutoRating()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.ACE_FDALines.AddNew();
			invoiceLine1.ACE_FDALines.AddNew();
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttbLine1 = invoiceLine1.TTBLines.AddNew();
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PGALines));

			invoiceLine1.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PGALines));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine2.ACE_FDALines.AddNew();
			invoiceLine2.ACE_FDALines.AddNew();
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PGALines));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine3.ACE_FDALines.AddNew();
			invoiceLine3.ACE_FDALines.AddNew();
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.PGALines));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.PGALines));
		}

		public void TestGetAmountOfPGADisclaimsForAutoRating()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGADisclaims));

			invoiceLine1.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PGADisclaims));

			invoiceLine1.US_TTBInd = OGAIndicatorList.Codes.Declared;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.PGADisclaims));

			invoiceLine1.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(1m, rateableMeasures.GetActual(MeasureType.PGADisclaims));

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.PGADisclaims));

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(3m, rateableMeasures.GetActual(MeasureType.PGADisclaims));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			rateableMeasures = (RateableMeasureSet)declaration.RatingAdapter.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.PGADisclaims));
		}

		public void TestAutoRatingServiceProvider()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;

			var externalBroker = Factory.New<OrgHeader>();
			declaration.JE_OH_ExternalBroker = externalBroker.PK;

			var autoRating = declaration.RatingAdapter;
			AssertEquals(true, autoRating.Creditors.AllOrgs.Contains(externalBroker));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, autoRating.Creditors.AllOrgs.Contains(externalBroker));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
