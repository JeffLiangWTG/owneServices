using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class UniversalTariffHelperTest : TestCaseWithFactory
	{
		public void TestGetDescription()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tariff = Factory.New<NZCClassification>();
				tariff.U0_Tariff = "0000.00.00.00Z";
				tariff.U0_DateActiveFrom = ZDateTime.Today;
				tariff.U0_Description = "Description";
				Factory.Save();

				AssertEquals("Description", UniversalTariffHelper.GetDescription(Factory, "0000.00.00.00Z"));
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupTariffData(Factory);

				AssertEquals("Tariff Desc", UniversalTariffHelper.GetDescription(Factory, "123456789"));
			}
		}

		public void TestGetTariff()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tariff = Factory.New<NZCClassification>();
				tariff.U0_Tariff = "0000.00.00.00Z";
				tariff.U0_DateActiveFrom = ZDateTime.Today.AddDays(-1);
				tariff.U0_DateActiveTo = ZDateTime.Today.AddDays(1);
				Factory.Save();

				AssertEquals("0000.00.00.00Z", UniversalTariffHelper.GetTariff(Factory, "0000.00.00.00Z", ZDateTime.Today).Code);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupTariffData(Factory);

				AssertEquals("123456789", UniversalTariffHelper.GetTariff(Factory, "123456789", ZDateTime.Today).Code);
			}
		}

		public void TestGetStatisticalUnit()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tariff = Factory.New<NZCClassification>();
				tariff.U0_Tariff = "0000.00.00.00Z";
				tariff.U0_DateActiveFrom = ZDateTime.Today;
				tariff.U0_StatisticalUnit = "KGM";
				Factory.Save();

				AssertEquals("KGM", UniversalTariffHelper.GetStatisticalUnit(tariff));
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var tariff = SetupTariffData(Factory);

				AssertEquals("AAA", UniversalTariffHelper.GetStatisticalUnit(tariff));
			}
		}

		public void TestGetSupplementaryUnitForInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tariff = Factory.New<NZCClassification>();
				tariff.U0_Tariff = "0000.00.00.00Z";
				tariff.U0_DateActiveFrom = ZDateTime.Today;
				tariff.U0_SupplementaryUnit = "KGM";
				invoiceLine.JI_Tariff = "0000.00.00.00Z";
				Factory.Save();

				AssertEquals("KGM", UniversalTariffHelper.GetSupplementaryUnitForInvoiceLine(invoiceLine));
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var tariff = SetupTariffData(Factory);
				invoiceLine.JI_Tariff = "123456789";

				AssertEquals("BBB", UniversalTariffHelper.GetSupplementaryUnitForInvoiceLine(invoiceLine));
			}
		}

		public void TestGetTradeGroupsFromCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Australia");
			var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "IE", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Ireland");
			var tradeGroup3 = helper.LoadOrCreateTradeGroup("NZ", "TST", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Test group");
			helper.AddCountry(tradeGroup1, "AU", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			helper.AddCountry(tradeGroup2, "IE", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			helper.AddCountry(tradeGroup3, "IE", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));

			var codeList = UniversalTariffHelper.GetTradeGroupsFromCountry("AA", ZDateTime.Today, Factory);
			AssertEquals("codeList.Count", 0, codeList.Length);

			codeList = UniversalTariffHelper.GetTradeGroupsFromCountry("AU", ZDateTime.Today, Factory);
			AssertEquals("codeList.Count", 1, codeList.Length);
			AssertEquals("AU", codeList[0].ZZA_TradeGroup);

			codeList = UniversalTariffHelper.GetTradeGroupsFromCountry("IE", ZDateTime.Today, Factory);
			AssertEquals("codeList.Count", 2, codeList.Length);
			Assert(codeList.Any(x => x.ZZA_TradeGroup == "TST"));
			Assert(codeList.Any(x => x.ZZA_TradeGroup == "IE"));
		}

		public void TestGetConcessionList()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestCaseHelper.ClearTable(NZCConcessionClassificationLink.Schema.TableName);
				var tariff = NZCClassification.New(Factory);
				tariff.U0_Tariff = "0000.00.00.00Z";
				tariff.U0_DateActiveFrom = ZDateTime.Today;

				var concession1 = NZCConcession.New(Factory);
				concession1.U2_Code = "111111A";
				concession1.U2_DateActiveFrom = ZDateTime.Today;
				var concessionTariffLink1 = concession1.TariffsApplicable.AddNew();
				concessionTariffLink1.U3_TariffPortion = tariff.U0_Tariff.Left(10);

				var concession2 = NZCConcession.New(Factory);
				concession2.U2_Code = "222222B";
				concession2.U2_DateActiveFrom = ZDateTime.Today;
				var concessionTariffLink2 = concession2.TariffsApplicable.AddNew();
				concessionTariffLink2.U3_TariffPortion = tariff.U0_Tariff.Left(7);

				var concession3 = NZCConcession.New(Factory);
				concession3.U2_Code = "333333C";
				concession3.U2_DateActiveFrom = ZDateTime.Today;
				var concessionTariffLink3 = concession3.TariffsApplicable.AddNew();
				concessionTariffLink3.U3_TariffPortion = "1111";
				Factory.Save();

				var concessions = UniversalTariffHelper.GetConcessionList(Factory, "0000.00.00.00Z", ZDateTime.Today) as NonDependentNZCConcessionCollection;
				concessions.Load();
				AssertEquals("Count", 2, concessions.Count);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				SetupTariffData(Factory);

				AssertEquals("Count", 2, UniversalTariffHelper.GetConcessionList(Factory, "123456789", ZDateTime.Today).Count);
				AssertEquals("Count", 1, UniversalTariffHelper.GetConcessionList(Factory, "123456789", ZDateTime.Today, "AU").Count);
			}
		}

		public void TestGetTariffFetchHint()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var hint = UniversalTariffHelper.GetTariffFetchHint(Factory, "123456789");
				AssertEquals("Table type", NZCClassificationSchema.Instance, hint.table);
				AssertEquals("SQL filter", "U0_Tariff = '123456789'", hint.query.LiteralTextADO);

				hint = UniversalTariffHelper.GetTariffFetchHint(Factory, "123456789", new ZDateTime(2023, 05, 26));
				AssertEquals("Table type", NZCClassificationSchema.Instance, hint.table);
				AssertEquals("SQL filter", "U0_Tariff like '%123456789%' and U0_DateActiveFrom <= #2023-05-26 00:00:00.000# and (U0_DateActiveTo is null or U0_DateActiveTo >= #2023-05-26 00:00:00.000#)", hint.query.LiteralTextADO);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");

				var hint = UniversalTariffHelper.GetTariffFetchHint(Factory, "123456789", new ZDateTime(2023, 05, 26));
				AssertEquals("Table type", TariffViewSchema.Instance, hint.table);
				AssertEquals("SQL filter", "ZZ1_ZZZ_NKDataGrouping = 'NZ' and ZZ1_StartDate <= #2023-05-26 00:00:00.000# and ZZ1_EndDate >= #2023-05-26 00:00:00.000# and ZZ1_CRT_NKTariffVersion = '' and ZZ1_TariffCode = '123456789' and ZZ1_ZZI_NKTariffType = 'HSN'", hint.query.LiteralTextADO);
			}
		}

		public void TestGetLevyRateFetchHint()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateNewOrGetExistingRateType("NZ", "LVY");
			helper.LoadOrCreateNewCusRateCode(Factory, "AL", rateType.PK);

			var hint = UniversalTariffHelper.GetLevyRateFetchHint(Factory, ZGuid.BrettsGuid, new ZDateTime(2023, 05, 26));
			AssertEquals("Table type", RateViewSchema.Instance, hint.table);
			AssertStartsWith("SQL filter", "ZZ2_ZZ1_ParentTariffOrNationalCode = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid') and ZZ2_StartDate <= #2023-05-26 00:00:00.000# and ZZ2_EndDate >= #2023-05-26 00:00:00.000#", hint.query.LiteralTextADO);
		}

		public void TestGetRateFetchHint()
		{
			var hint = UniversalTariffHelper.GetRateFetchHint(ZGuid.BrettsGuid);
			AssertEquals("Table type", RateViewSchema.Instance, hint.table);
			AssertEquals("SQL filter", "ZZ2_ZZ1_ParentTariffOrNationalCode = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')", hint.query.LiteralTextADO);
		}

		public void TestGetConcessionFetchHint()
		{
			var hint = UniversalTariffHelper.GetConcessionFetchHint(ZGuid.BrettsGuid);
			AssertEquals("Table type", CusRefApplicabilityViewSchema.Instance, hint.table);
			AssertEquals("SQL filter", "ZZT_ZZ2_Rate = CONVERT('20dd961b-3e62-40e5-b60a-b1312b70f5ee', 'System.Guid')", hint.query.LiteralTextADO);
		}

		public void TestUseRefDatabaseData()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, UniversalTariffHelper.UseRefDatabaseData);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, UniversalTariffHelper.UseRefDatabaseData);
			}
		}

		public static TariffView SetupTariffData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
			var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTY", rateType.PK);
			var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
			helper.CreateTariffUOM(tariff, "CU1", "AAA");
			helper.CreateTariffUOM(tariff, "CU2", "BBB");
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Australia");
			var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "US", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "United States");
			helper.AddCountry(tradeGroup1, "AU");
			helper.AddCountry(tradeGroup2, "US");
			helper.CreateCusApplicability(rate, tradeGroup1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "100001A");
			helper.CreateCusApplicability(rate, tradeGroup2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "", "200002A");
			helper.CreateCusApplicability(rate, tradeGroup2, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-9), "", "300003A");

			return tariff;
		}
	}
}
