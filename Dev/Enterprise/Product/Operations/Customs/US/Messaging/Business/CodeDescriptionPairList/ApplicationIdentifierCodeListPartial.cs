using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	partial class ApplicationIdentifierCodeList
	{
#if DEBUG
		internal const string DummyForTesting1 = "₧ƒ";
		internal const string DummyForTesting2 = "₧ÿ";
#endif

		public static class AES
		{
			// Input Records Identifiers
			public const string CommodityShipment = "XP";

			// Output Records Identifiers
			public const string CommodityShipmentResponse = "XT";
			public const string CommodityShipmentWarningReminder = "XN";

			public static ZString[] GetAESCodes()
			{
				return new ZString[]
				{
					CommodityShipment,
					CommodityShipmentResponse,
					CommodityShipmentWarningReminder
				};
			}

			public static bool IsAESApplicationCode(string applicationCode)
			{
				ZString[] codes = GetAESCodes();
				foreach (ZString code in codes)
				{
					if (code == applicationCode)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static ZString[] GetApplicationCodesThatContainEntryStatus()
		{
			return new ZString[]
			{
				Codes.EntrySummaryResponse,
				Codes.CargoReleaseTransactionsResponse,
				Codes.BorderCargoReleaseResponse,
				Codes.CargoReleaseProcessingResults,
				Codes.QueryCurrentEntryStatusResponse };
		}

		public static bool IsApplicationCodeThatContainEntryStatus(string applicationCode)
		{
			ZString[] codes = GetApplicationCodesThatContainEntryStatus();
			foreach (ZString code in codes)
			{
				if (code == applicationCode)
				{
					return true;
				}
			}
			return false;
		}

		public static CodeDescriptionPair[] GetApplicationCodesForResponseQuery()
		{
			return new CodeDescriptionPair[]
			{
				new CodeDescriptionPair(Codes.QueryQuotaResponse, Descriptions.QueryQuotaResponse)
					, new CodeDescriptionPair(Codes.QueryErrorStatisticsResponse, Descriptions.QueryErrorStatisticsResponse)
					, new CodeDescriptionPair(Codes.QueryEntrySummaryResponse, Descriptions.QueryEntrySummaryResponse)
					, new CodeDescriptionPair(Codes.QueryHarmonizedSystem, Descriptions.QueryHarmonizedSystem)
					, new CodeDescriptionPair(Codes.AntidumpingCountervailingDutyQueryResponse, Descriptions.AntidumpingCountervailingDutyQueryResponse)
					, new CodeDescriptionPair(Codes.QueryCurrentEntryStatusResponse, Descriptions.QueryCurrentEntryStatusResponse)
					, new CodeDescriptionPair(Codes.ManufacturerNameandAddressQueryResponse, Descriptions.ManufacturerNameandAddressQueryResponse)
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse, ACEApplicationIdentifierCodeList.Descriptions.QueryImporterBondResponse)
					, new CodeDescriptionPair(Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse, Descriptions.FoodandDrugAdministrationEstablishmentIdentifierResponse)
					, new CodeDescriptionPair(Codes.UserStatistics, Descriptions.UserStatistics)
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse , ACEApplicationIdentifierCodeList.Descriptions.CensusWarningQueryResponse)
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, ACEApplicationIdentifierCodeList.Descriptions.CargoManifestEntryReleaseStatusQueryResponse)
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse, ACEApplicationIdentifierCodeList.Descriptions.QuotaQueryResponse)
			};
		}

		//input codes only
		public static CodeDescriptionPair[] GetApplicationCodesForQuery()
		{
			var isNewEntrySummaryQueryEffective = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
			var aceEntrySummaryQueryCode = isNewEntrySummaryQueryEffective ? ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery : Codes.ACEEntrySummaryQuery;
			return new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(Codes.QueryQuota, "Query Quota")
					, new CodeDescriptionPair(Codes.QueryErrorStatistics, "Query Error Statistics")
					, new CodeDescriptionPair(Codes.QueryEntrySummary, "Query Entry Summary")
					, new CodeDescriptionPair(aceEntrySummaryQueryCode, "Query Entry Summary (ACE)")
					, new CodeDescriptionPair(Codes.HarmonizedTariffScheduleQuery, "Harmonized Tariff Schedule")
					, new CodeDescriptionPair(Codes.AntidumpingCountervailingDutyQuery, "AntiDumping/Countervailing Case")
					, new CodeDescriptionPair(Codes.QueryCurrentEntryStatus, "Cargo/Manifest Query")
					, new CodeDescriptionPair(Codes.ManufacturerNameandAddressQuery, "Manufacturer Name And Address Query")
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, "Importer Bond Query")
					, new CodeDescriptionPair(Codes.FoodandDrugAdministrationEstablishmentIdentifier, "FDA Establishment Identifier Query")
					, new CodeDescriptionPair(Codes.UserStatistics, "User Statistics")
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery, "Census Warning Query")
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, "ACE Cargo/Manifest Query")
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.QuotaQuery, "ACE Query Quota")
					, new CodeDescriptionPair(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQuery, "Manufacturer Name And Address Query")
				};
		}

		public static string GetApplicationIdentifierCodeFrom(string bIRDApplication)
		{
			switch (bIRDApplication)
			{
				case BIRDApplicationCodeList.Codes.CourtesyNoticeOfLiquidation:
					return Codes.CourtesyNoticeofLiquidation;

				case BIRDApplicationCodeList.Codes.EntrySummaryQueryInput:
					return Codes.QueryEntrySummary;

				case BIRDApplicationCodeList.Codes.EntrySummaryQueryOutput:
					return Codes.QueryEntrySummaryResponse;

				default:
					return Codes.BIRDTransaction;
			}
		}

		public static string[] GetBIRDApplicationIdentifierCodes()
		{
			return new string[] { Codes.BIRDTransaction, Codes.BIRDEntrySummaryQuery };
		}

		public static string[] GetACEBIRDApplicationIdentifierCodes()
		{
			return new string[] { ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction };
		}

		public static void EnsureIsValidFormat(string applicationIdentifier)
		{
			if (applicationIdentifier.Length != 2 && applicationIdentifier != Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ApplicationIdentifierCodeList.Codes.AllMessages)
			{
				throw new ArgumentException("Length must be 2, but it is '" + applicationIdentifier + "'", nameof(applicationIdentifier));
			}
		}
	}
}
