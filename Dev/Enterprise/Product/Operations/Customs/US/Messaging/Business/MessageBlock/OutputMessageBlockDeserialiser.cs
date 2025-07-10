using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class OutputMessageBlockDeserialiser : MessageBlockDeserialiser
	{
		public OutputMessageBlockDeserialiser()
		{
		}

		protected override IReadOnlyList<MessageBlockDictionary> Dictionaries
		{
			get { return dictionaries ?? (dictionaries = new MessageBlockDictionaryGenerator().GenerateBlockDictionaries(typeof(OutputBlockAttribute))); }
		}

		[ThreadStatic]
		static IReadOnlyList<MessageBlockDictionary> dictionaries;
		
		protected override string GetVersion(string[] applicationCodes, string applicationID, string eightyCharacterBlock)
		{
			string result = null;
			if (applicationCodes.Contains(CBPEDIInterchange.ApplicationCodes.USCustomsImport))
			{
				switch (applicationID)
				{
					case ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification:
					case ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus:
						result = GetVersionForCargoReleaseStatus(eightyCharacterBlock);
						break;
					case ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse:
						result = GetVersionForCargoManifestEntryReleaseStatusQuery(eightyCharacterBlock);
						break;
					case ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse:
						result = GetVersionForEntrySummaryQueryResponse(eightyCharacterBlock);
						break;
					case ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement:
						result = GetVersionForPeriodicMonthlyStatement(eightyCharacterBlock);
						break;
					case ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator:
					case ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone:
						result = GetVersionForFTZBlocks(applicationID, eightyCharacterBlock);
						break;
				}
			}

			if (applicationCodes.Contains(MessageBlockDictionary.ACEApplicationCode))
			{
				switch (applicationID)
				{
					case ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse:
						result = GetVersionForNewEntrySummaryQueryResponse(eightyCharacterBlock);
						break;
				}
			}
			return result;
		}

		protected override string GetFallbackVersion(string[] applicationCodes, string applicationID, string eightyCharacterBlock)
		{
			string result = null;
			if (applicationCodes.Contains(CBPEDIInterchange.ApplicationCodes.USCustomsImport))
			{
				switch (applicationID)
				{
					case ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification:
					case ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus:
						result = GetFallbackVersionForCargoReleaseStatus(eightyCharacterBlock);
						break;
					case ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse:
						result = GetFallbackVersionForCargoManifestEntryReleaseStatusQuery(eightyCharacterBlock);
						break;
				}
			}
			return result;
		}

		string GetVersionForPeriodicMonthlyStatement(string eightyCharacterBlock)
		{
			string result = null;
			switch (eightyCharacterBlock.Substring(15, 2))
			{
				case "  ":
					result = VersionConstants.PeriodicMonthlyStatementLatestVersion;
					break;
			}
			return result;
		}

		string GetVersionForCargoManifestEntryReleaseStatusQuery(string eightyCharacterBlock)
		{
			string result = null;
			if (eightyCharacterBlock.Substring(0, 4) == "WO70")
			{
				result = eightyCharacterBlock.Substring(78, 2).Trim();
			}
			else
			{
				switch (eightyCharacterBlock.Substring(0, 3))
				{
					case "WR4":
						result = eightyCharacterBlock.Substring(77, 1).Trim();
						break;
					case "WSC":
						result = eightyCharacterBlock.Substring(77, 1).Trim();
						break;
				}
			}
			return result;
		}

		string GetFallbackVersionForCargoManifestEntryReleaseStatusQuery(string eightyCharacterBlock)
		{
			string result = null;
			switch (eightyCharacterBlock.Substring(0, 4))
			{
				case "WO70":
					result = FallbackVersionConstants.ACEQWO70LatestVersion;
					break;
			}
			return result;
		}

		string GetVersionForCargoReleaseStatus(string eightyCharacterBlock)
		{
			string result = null;
			switch (eightyCharacterBlock.Substring(0, 4))
			{
				case "SO70":
					result = eightyCharacterBlock.Substring(78, 2).Trim();
					break;
			}
			return result;
		}

		string GetFallbackVersionForCargoReleaseStatus(string eightyCharacterBlock)
		{
			string result = null;
			switch (eightyCharacterBlock.Substring(0, 4))
			{
				case "SO70":
					result = FallbackVersionConstants.ASESSO70LatestVersion;
					break;
			}
			return result;
		}
	}
}
