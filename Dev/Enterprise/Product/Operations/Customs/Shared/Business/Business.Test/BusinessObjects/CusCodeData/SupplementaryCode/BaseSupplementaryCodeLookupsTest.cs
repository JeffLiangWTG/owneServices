using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseSupplementaryCodeLookups))]
	sealed class BaseSupplementaryCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			SetupTariffAndRate();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = Factory.New<JobComInvLineWithSuppCodeSupporter>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "1234512345";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_SecondaryPreference = "";
			invoiceLine.JI_ConcessionOrder = "";

			var loader = new BaseSupplementaryCode.Loader(Factory);
			var code = loader.LoadOrCreate<BaseSupplementaryCode, JobComInvLineWithSuppCodeSupporter>(invoiceLine, 1);
			var list = code.Lookups.CY_CodeList;
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode("additionalcode"));
			AssertEquals("Additional Code 1 Descriptions", list.GetDescriptionFromCode("additionalcode"));
		}

		void SetupTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountry, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountry, Universal.Constants.TariffTypes.HarmonizedSystem);
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "A00", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", currentCountry);

			var cusTariff = testHelper.CreateTariff(currentCountry, hsnTariffType.PK, "1234512345", date1, date4, "dummy Description 0");
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "additionalcode", "ordernumber");
			testHelper.CreateCusCodeType("ADDCD", "Additional Codes");
			testHelper.CreateCusCodeList(currentCountry, "ADDCD", "additionalcode", "Additional Code 1 Descriptions", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();
		}

		sealed class JobComInvLineWithSuppCodeSupporter : BaseJobComInvoiceLine, ISupplementaryCodeSupporter
		{
			public JobComInvLineWithSuppCodeSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			TariffView ICusCodeDataWithOrderSupporter.Tariff => UniversalTariff;

			IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => AllApplicableRatesSelectionCriteria;

			ZString ICusCodeDataWithOrderSupporter.GetCountryCodeForCodeProvider() => GlbCompany.CurrentCompany.Country.Code;

			CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => Lookups.CachedListOfAdditionalCodeDescriptions;

			void ICusCodeDataWithOrderSupporter.OnCodesChanged()
			{
				throw new System.NotImplementedException();
			}

			IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => throw new System.NotImplementedException();

			ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => throw new System.NotImplementedException();

			ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => throw new System.NotImplementedException();

			ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => throw new System.NotImplementedException();

			ZString ISupplementaryCodeSupporter.GetCountryCodeFromAdditionalCode(ZString additionalCode)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
