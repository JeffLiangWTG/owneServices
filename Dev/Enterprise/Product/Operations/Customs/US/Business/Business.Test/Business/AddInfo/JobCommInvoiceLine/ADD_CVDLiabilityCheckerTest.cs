using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ADD_CVDLiabilityCheckerTest : TestCaseWithFactory
	{
		public void TestValidateLiabilityForACE()
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD", "A9085290"));
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD", "A9085290"));
		}

		public void TestValidateLiabilityForEntrySummary()
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			AssertHasWarning(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD", "A9085290"));
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD", "A9085290"));
			declaration.US_EnableENS = true;
			invoiceLine.US_ADDCaseNo = ZString.Empty;
			AssertNoWarning(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD", "A9085290"));
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD", "A9085290"));
		}

		public void TestValidateLiabilityForCA()
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "CA";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			var messageError = string.Format(ADD_CVDLiabilityChecker.MayBeSubjectToADD_CVD, "ADD");
			AssertHasMessageError(invoiceLine.US_ADDCaseNoInfo, messageError);
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			AssertHasMessageError("XA should be same as Canada(CA)", invoiceLine.US_ADDCaseNoInfo, messageError);
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			AssertNoMessageError(invoiceLine.US_ADDCaseNoInfo, messageError);
		}
	}
}
