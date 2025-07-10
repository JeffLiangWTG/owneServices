using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCase.Loader))]
	class USCACCaseLoaderTest : LoaderTestCase
	{
		public void TestCheckADDCVDExistenceForTariffsWithSamePrefix()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000102";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000103";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9999999";
			case1.U5_ISOCountryCode = Core.Constants.CountryCodes.China;
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.Today;

			var accTariff = Factory.New<USCACCaseTariff>();
			accTariff.U9_TariffNumber = "00000001";
			accTariff.U9_CaseNumber = "A9999999";
			Factory.Save();

			var aCase1 = new USCACCase.Loader(Factory).Exists(tariff1.UE_Tariff, Core.Constants.CountryCodes.China, "A");
			var aCase2 = new USCACCase.Loader(Factory).Exists(tariff2.UE_Tariff, Core.Constants.CountryCodes.China, "A");
			AssertEquals(true, aCase1);
			AssertEquals(true, aCase2);
		}

		public void TestCheckADDCVDExistenceForTariffsWithSamePrefixWhenValidateAll()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "0000000102";
			tariff1.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff1.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000103";
			tariff2.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff2.UE_DateTo = ZDateTime.Today.AddYears(1);

			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9999999";
			case1.U5_ISOCountryCode = Core.Constants.CountryCodes.China;
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.Today;

			var accTariff = Factory.New<USCACCaseTariff>();
			accTariff.U9_TariffNumber = "00000001";
			accTariff.U9_CaseNumber = "A9999999";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			invoiceLine1.JI_Tariff = tariff1.UE_Tariff;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDec = newFactory.Load<JobDeclaration>(declaration.PK);

			newDec.RunPreSaveValidationWithFetchHints();

			var newLine1 = newDec.InvoiceLines.Where(x => x.JI_Tariff == tariff1.UE_Tariff).First() as JobComInvoiceLine;
			var newLine2 = newDec.InvoiceLines.Where(x => x.JI_Tariff == tariff2.UE_Tariff).First() as JobComInvoiceLine;

			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(newLine1.US_ADDCaseNoInfo, "This invoice line may be subject to ADD.");
				AssertHasMessageErrorContaining(newLine2.US_ADDCaseNoInfo, "This invoice line may be subject to ADD.");
			});
		}

		public void TestThereIsNoException_LoadData_ISOCountryCodeIsUppercase_CountryOfOriginIsLowercase()
		{
			AssertNoException("CN", "cn");
		}

		public void TestThereIsNoException_LoadData_ISOCountryCodeIsLowercase_CountryOfOriginIsUppercase()
		{
			AssertNoException("cn", "CN");
		}

		void AssertNoException(ZString isoCountryCode, ZString countryOfOrigin)
		{
			CreateTestData(isoCountryCode);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = countryOfOrigin;
			AssertNoExceptionThrown("There is no KeyNotFoundException", () =>
			{
				invoiceLine.JI_Tariff = "3333333333";
			});
		}

		public void TestThereIsNoException_CallAddToNeedToLoadIfNeededBeforeCallExists()
		{
			CreateTestData("CN");

			new USCACCase.Loader(Factory).AddToNeedToLoadIfNeeded("3333333333", "cn", "A");
			AssertNoExceptionThrown("There is no KeyNotFoundException", () =>
			{
				new USCACCase.Loader(Factory).Exists("3333333333", "cn", "A");
			});
		}

		void CreateTestData(ZString isoCountryCode)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3333333333";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = isoCountryCode;
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "3333333333";
			tariff1.U9_CaseNumber = "A9085290";
			Factory.Save();
		}

		public void TestFactoryCachedUsedWhenCheckExists()
		{
			//A matched case for 333333
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3333333333";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "333333";
			tariff1.U9_CaseNumber = "A9085290";
			Factory.Save();

			var loader = new USCACCase.Loader(Factory);
			loader.AddToNeedToLoadIfNeeded("3333333333", "KR", "A");
			var aCase = loader.Exists("3333333333", "KR", "A");
			AssertEquals("First call.", true, aCase);

			Factory.TryGetValueFromCacheOnly("3333333333|KR|A", out aCase);
			AssertEquals("Get result from cache.", true, aCase);

			DynamicBusinessObjectCollection collection = null;
			Assert(Factory.TryGetValueFromCacheOnly("ACCase_A", out collection));
			AssertEquals(1, collection.Count);
		}

		public void TestIControllerIDProviderMembers()
		{
			var case1 = Factory.New<USCACCase>();
			IControllerIDProvider provider = case1;
			AssertEquals("ControllerID", ControllerIDs.Customs.US.USCACCase, provider.ControllerID);
			AssertEquals("BusinessObjectPK", case1.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestLoadWithTariffAndCountryOfOrigin()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			//A matched case for 0000000000
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = Factory.New<USCACCaseTariff>();
			tariff1.U9_TariffNumber = "0000000000";
			tariff1.U9_CaseNumber = "A9085290";

			//A unmatched case due to C/O
			var case2 = Factory.New<USCACCase>();
			case2.U5_CaseNumber = "A9085291";
			case2.U5_ISOCountryCode = "JP";
			case2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case2.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var tariff2 = Factory.New<USCACCaseTariff>();
			tariff2.U9_TariffNumber = "0000000000";
			tariff2.U9_CaseNumber = "A9085291";

			//A unmatched case due to case status
			var case3 = Factory.New<USCACCase>();
			case3.U5_CaseNumber = "A9085292";
			case3.U5_ISOCountryCode = "KR";
			case3.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			case3.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff3 = Factory.New<USCACCaseTariff>();
			tariff3.U9_TariffNumber = "0000000000";
			tariff3.U9_CaseNumber = "A9085292";
			//A unmatched case due to tariff
			var case4 = Factory.New<USCACCase>();
			case4.U5_CaseNumber = "A9085293";
			case4.U5_ISOCountryCode = "KR";
			case4.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case4.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff4 = Factory.New<USCACCaseTariff>();
			tariff4.U9_TariffNumber = "0000000010";
			tariff4.U9_CaseNumber = "A9085293";
			Factory.Save();

			new USCACCase.Loader(Factory).AddToNeedToLoadIfNeeded("0000000000", "KR", "A");
			var aCase = new USCACCase.Loader(Factory).Exists("0000000000", "KR", "A");
			AssertEquals(true, aCase);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new USCACCase.Loader(Factory);
		}
	}
}
