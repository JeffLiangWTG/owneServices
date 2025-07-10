using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Testing
{
	sealed class ApplicationIdentifierCodeListTest : NUnit.Framework.TestCase
	{
		public void TestGetApplicationIdentifierCodeFrom()
		{
			AssertEquals(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation, ApplicationIdentifierCodeList.GetApplicationIdentifierCodeFrom(BIRDApplicationCodeList.Codes.CourtesyNoticeOfLiquidation));
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, ApplicationIdentifierCodeList.GetApplicationIdentifierCodeFrom(BIRDApplicationCodeList.Codes.CargoRelease));
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse, ApplicationIdentifierCodeList.GetApplicationIdentifierCodeFrom(BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput));
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryEntrySummary, ApplicationIdentifierCodeList.GetApplicationIdentifierCodeFrom(BIRDApplicationCodeList.Codes.EntrySummaryQueryInput));
		}

		public void TestEnsureIsValidFormat()
		{
			CombineAssertions(delegate
			{
				AssertNoExceptionThrown(() => ApplicationIdentifierCodeList.EnsureIsValidFormat(ApplicationIdentifierCodeList.Codes.AllMessages));
				AssertNoExceptionThrown(() => ApplicationIdentifierCodeList.EnsureIsValidFormat(ApplicationIdentifierCodeList.Codes.AutomatedClearinghouse));
				AssertNoExceptionThrown(() => ApplicationIdentifierCodeList.EnsureIsValidFormat("12"));
#if NETFRAMEWORK
				AssertExceptionThrown(typeof(ArgumentException), "Length must be 2, but it is ''\r\nParameter name: applicationIdentifier", () => ApplicationIdentifierCodeList.EnsureIsValidFormat(""));
				AssertExceptionThrown(typeof(ArgumentException), "Length must be 2, but it is '1'\r\nParameter name: applicationIdentifier", () => ApplicationIdentifierCodeList.EnsureIsValidFormat("1"));
				AssertExceptionThrown(typeof(ArgumentException), "Length must be 2, but it is '123'\r\nParameter name: applicationIdentifier", () => ApplicationIdentifierCodeList.EnsureIsValidFormat("123"));
#else
				AssertExceptionThrown(typeof(ArgumentException), "Length must be 2, but it is '' (Parameter 'applicationIdentifier')", () => ApplicationIdentifierCodeList.EnsureIsValidFormat(""));
				AssertExceptionThrown(typeof(ArgumentException), "Length must be 2, but it is '1' (Parameter 'applicationIdentifier')", () => ApplicationIdentifierCodeList.EnsureIsValidFormat("1"));
				AssertExceptionThrown(typeof(ArgumentException), "Length must be 2, but it is '123' (Parameter 'applicationIdentifier')", () => ApplicationIdentifierCodeList.EnsureIsValidFormat("123"));
#endif
			});
		}

		public void TestGetBIRDApplicationIdentifierCode()
		{
			string[] codes = ApplicationIdentifierCodeList.GetBIRDApplicationIdentifierCodes();
			AssertEquals(2, codes.Length);
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, codes[0]);
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery, codes[1]);
		}

		public void TestGetApplicationCodesForResponseQuery()
		{
			CodeDescriptionPair[] result = ApplicationIdentifierCodeList.GetApplicationCodesForResponseQuery();
			AssertEquals(13, result.Length);
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse, result[0].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.QueryQuotaResponse, result[0].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryErrorStatisticsResponse, result[1].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.QueryErrorStatisticsResponse, result[1].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryEntrySummaryResponse, result[2].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.QueryEntrySummaryResponse, result[2].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem, result[3].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.QueryHarmonizedSystem, result[3].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse, result[4].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.AntidumpingCountervailingDutyQueryResponse, result[4].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse, result[5].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.QueryCurrentEntryStatusResponse, result[5].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQueryResponse, result[6].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.ManufacturerNameandAddressQueryResponse, result[6].Description);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse, result[7].Code);
			AssertEquals(ACEApplicationIdentifierCodeList.Descriptions.QueryImporterBondResponse, result[7].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, result[8].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.FoodandDrugAdministrationEstablishmentIdentifierResponse, result[8].Description);
			AssertEquals(ApplicationIdentifierCodeList.Codes.UserStatistics, result[9].Code);
			AssertEquals(ApplicationIdentifierCodeList.Descriptions.UserStatistics, result[9].Description);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse, result[10].Code);
			AssertEquals(ACEApplicationIdentifierCodeList.Descriptions.CensusWarningQueryResponse, result[10].Description);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, result[11].Code);
			AssertEquals(ACEApplicationIdentifierCodeList.Descriptions.CargoManifestEntryReleaseStatusQueryResponse, result[11].Description);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse, result[12].Code);
			AssertEquals(ACEApplicationIdentifierCodeList.Descriptions.QuotaQueryResponse, result[12].Description);
		}

		public void TestGetApplicationCodesForQuery()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, false))
			{
				CodeDescriptionPair[] result = ApplicationIdentifierCodeList.GetApplicationCodesForQuery();
				AssertEquals(15, result.Length);
				AssertEquals(ApplicationIdentifierCodeList.Codes.QueryQuota, result[0].Code);
				AssertEquals("Query Quota", result[0].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.QueryErrorStatistics, result[1].Code);
				AssertEquals("Query Error Statistics", result[1].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.QueryEntrySummary, result[2].Code);
				AssertEquals("Query Entry Summary", result[2].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.ACEEntrySummaryQuery, result[3].Code);
				AssertEquals("Query Entry Summary (ACE)", result[3].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, result[4].Code);
				AssertEquals("Harmonized Tariff Schedule", result[4].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery, result[5].Code);
				AssertEquals("AntiDumping/Countervailing Case", result[5].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus, result[6].Code);
				AssertEquals("Cargo/Manifest Query", result[6].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQuery, result[7].Code);
				AssertEquals("Manufacturer Name And Address Query", result[7].Description);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, result[8].Code);
				AssertEquals("Importer Bond Query", result[8].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier, result[9].Code);
				AssertEquals("FDA Establishment Identifier Query", result[9].Description);
				AssertEquals(ApplicationIdentifierCodeList.Codes.UserStatistics, result[10].Code);
				AssertEquals("User Statistics", result[10].Description);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery, result[11].Code);
				AssertEquals("Census Warning Query", result[11].Description);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, result[12].Code);
				AssertEquals("ACE Cargo/Manifest Query", result[12].Description);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.QuotaQuery, result[13].Code);
				AssertEquals("ACE Query Quota", result[13].Description);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQuery, result[14].Code);
				AssertEquals("Manufacturer Name And Address Query", result[14].Description);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				CodeDescriptionPair[] result = ApplicationIdentifierCodeList.GetApplicationCodesForQuery();
				AssertEquals(15, result.Length);
				AssertEquals(ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery, result[3].Code);
				AssertEquals("Query Entry Summary (ACE)", result[3].Description);
			}
		}

		public void TestGetApplicationCodesThatContainEntryStatus()
		{
			ZString[] result = ApplicationIdentifierCodeList.GetApplicationCodesThatContainEntryStatus();
			AssertEquals("Three application codes", 5, result.Length);
			AssertEquals(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, result[0]);
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse, result[1]);
			AssertEquals(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse, result[2]);
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults, result[3]);
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse, result[4]);
		}

		public void TestIsApplicationCodeThatContainEntryStatus()
		{
			AssertEquals(true, ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse));
			AssertEquals(true, ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse));
			AssertEquals(true, ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse));
			AssertEquals(true, ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults));
			AssertEquals(true, ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse));
			AssertEquals(false, ApplicationIdentifierCodeList.IsApplicationCodeThatContainEntryStatus(ApplicationIdentifierCodeList.Codes.EntrySummary));
		}

		public void TestGetAESCodes()
		{
			ZString[] codes = ApplicationIdentifierCodeList.AES.GetAESCodes();
			AssertEquals(3, codes.Length);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, codes[0]);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, codes[1]);
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder, codes[2]);
		}

		public void TestIsAESApplicationCode()
		{
			AssertEquals(true, ApplicationIdentifierCodeList.AES.IsAESApplicationCode(ApplicationIdentifierCodeList.AES.CommodityShipment));
			AssertEquals(true, ApplicationIdentifierCodeList.AES.IsAESApplicationCode(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse));
			AssertEquals(true, ApplicationIdentifierCodeList.AES.IsAESApplicationCode(ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder));
			AssertEquals(false, ApplicationIdentifierCodeList.AES.IsAESApplicationCode(ApplicationIdentifierCodeList.Codes.EntrySummary));
		}
	}
}
