using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class MessageBlockDeserialiser
	{
		protected MessageBlockDeserialiser()
		{
		}

		public MessageBlock GetDeserialisedBlock(string[] applicationCodes, string applicationIdentifier, string eightyCharacterBlock, bool reportUnknown = false)
		{
			ApplicationIdentifierCodeList.EnsureIsValidFormat(applicationIdentifier);
			MessageBlock result = null;
			result = GetBlock(applicationCodes, applicationIdentifier, eightyCharacterBlock);
			if (result == null)
			{
				result = new UnknownMessageBlock() { Data = eightyCharacterBlock };
				if (reportUnknown)
				{
					var applicationCodesHumanReadable = new ZStringBuilder(applicationCodes);
					ErrorReporter.ReportOnce("Cannot deserialise '" + eightyCharacterBlock.TrimEnd() + "'" + System.Environment.NewLine + "Application Identifier : " + applicationIdentifier + " for Application Code(s) : " + applicationCodesHumanReadable.ToStringWithDelimiterBetweenAppends(", "));
				}
			}
			else
			{
				result.Deserialise(eightyCharacterBlock);
			}
			return result;
		}

		protected abstract IReadOnlyList<MessageBlockDictionary> Dictionaries { get; }

		public bool IsValid(string[] applicationCodes, string applicationID, string eightyByteBlock)
		{
			return GetMessageBlockType(applicationCodes, applicationID, eightyByteBlock) != null;
		}

		MessageBlock GetBlock(string[] applicationCodes, string applicationIdentifier, string eightyCharacterBlock)
		{
			Type typeOfBlock = GetMessageBlockType(applicationCodes, applicationIdentifier, eightyCharacterBlock);

			if (typeOfBlock != null)
			{
				return (MessageBlock)Activator.CreateInstance(typeOfBlock);
			}

			return null;
		}

		protected Type GetMessageBlockType(string[] applicationCodes, string applicationID, string eightyCharacterBlock)
		{
			Type messageBlockType = null;
			Type messageBlockTypeForEmptyApplicationIdentifier = null;
			GetMessageBlockTypeCore(applicationCodes, applicationID, eightyCharacterBlock, out messageBlockType, out messageBlockTypeForEmptyApplicationIdentifier);

			if (messageBlockType == null && messageBlockTypeForEmptyApplicationIdentifier == null && applicationCodes.Contains(CBPEDIInterchange.ApplicationCodes.USCustomsImport))
			{
				GetMessageBlockTypeCore(new string[] { MessageBlockDictionary.ACEApplicationCode }, applicationID, eightyCharacterBlock, out messageBlockType, out messageBlockTypeForEmptyApplicationIdentifier);
			}

			return messageBlockType ?? messageBlockTypeForEmptyApplicationIdentifier;
		}

		void GetMessageBlockTypeCore(string[] applicationCodes, string applicationID, string eightyCharacterBlock, out Type messageBlockType, out Type messageBlockTypeForEmptyApplicationIdentifier)
		{
			messageBlockType = null;
			messageBlockTypeForEmptyApplicationIdentifier = null;

			foreach (var applicationCode in applicationCodes)
			{
				var version = GetVersion(applicationCodes, applicationID, eightyCharacterBlock);
				messageBlockType = GetMessageBlockType(applicationID, eightyCharacterBlock, ref messageBlockTypeForEmptyApplicationIdentifier, applicationCode, version);
				if (messageBlockType != null)
				{
					break;
				}
				if (!string.IsNullOrEmpty(version))
				{
					ErrorReporter.ReportOnce(applicationCode + "_" + applicationID + "_" + eightyCharacterBlock.PadLeft(6).Substring(0, 6) + "_" + version, string.Format(DefaultCulture.Instance, "The following block is not currently supported (Application Code: {0}, Application ID: {1}, Version: {2}, Block: '{3}').", applicationCode, applicationID, version, eightyCharacterBlock));
					var fallbackVersion = GetFallbackVersion(applicationCodes, applicationID, eightyCharacterBlock);
					if (fallbackVersion != null)
					{
						messageBlockType = GetMessageBlockType(applicationID, eightyCharacterBlock, ref messageBlockTypeForEmptyApplicationIdentifier, applicationCode, fallbackVersion);
						if (messageBlockType != null)
						{
							break;
						}
					}
				}
			}
		}

		Type GetMessageBlockType(string applicationID, string eightyCharacterBlock, ref Type messageBlockTypeForEmptyApplicationIdentifier, string applicationCode, string version)
		{
			Type messageBlockType = null;
			foreach (MessageBlockDictionary dictionary in Dictionaries)
			{
				string mandatoryCharacters = eightyCharacterBlock.Substring(0, dictionary.MandatoryCharactersLength);
				if (dictionary.TryGetValue(applicationCode, applicationID, mandatoryCharacters, version, out messageBlockType))
				{
					break;
				}

				if (messageBlockTypeForEmptyApplicationIdentifier == null && dictionary.TryGetValue(applicationCode, MessageBlockDictionary.EmptyApplicationIdentifier, mandatoryCharacters, version, out messageBlockType))
				{
					messageBlockTypeForEmptyApplicationIdentifier = messageBlockType;
				}
			}
			return messageBlockType;
		}

		protected virtual string GetVersion(string[] applicationCodes, string applicationID, string eightyCharacterBlock)
		{
			return "";
		}

		protected virtual string GetFallbackVersion(string[] applicationCodes, string applicationID, string eightyCharacterBlock)
		{
			return null;
		}

		public string GetVersionForFTZBlocks(string applicationID, string eightyCharacterBlock)
		{
			var result = string.Empty;
			if (applicationID == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData ||
				applicationID == ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)
			{
				switch (eightyCharacterBlock.Substring(0, ControlIDMaxLength))
				{
					case "10":
						var expandedZoneIDIndicator = eightyCharacterBlock.Substring(ExpandedZoneIDIndicatorPosition, ExpandedZoneIDIndicatorLength);
						if (expandedZoneIDIndicator == "Y" || expandedZoneIDIndicator == "N")
						{
							result = LatestVersion;
						}
						break;
					case "40":
						ZString firmsIdentifier = eightyCharacterBlock.Substring(FirmsIdentifierPosition, FirmsIdentifierLength);
						if (firmsIdentifier.IsEmpty)
						{
							result = LatestVersion;
						}
						break;
				}
			}
			else if (applicationID == ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)
			{
				switch (eightyCharacterBlock.Substring(0, ControlIDMaxLength))
				{
					case "10":
						var expandedZoneIDIndicator = eightyCharacterBlock.Substring(ExpandedZoneIDIndicatorPosition, ExpandedZoneIDIndicatorLength);
						if (expandedZoneIDIndicator == "Y" || expandedZoneIDIndicator == "N")
						{
							result = LatestVersion;
						}
						break;
					case "40":
						ZString firmsIdentifier = eightyCharacterBlock.Substring(FirmsIdentifierPosition, FirmsIdentifierLength);
						if (firmsIdentifier.IsEmpty)
						{
							result = LatestVersion;
						}
						break;
					case "90":
						string directDeliveryIndicator = eightyCharacterBlock.Substring(DirectDeliveryIndicatorPosition, DirectDeliveryIndicatorLength);
						if (char.IsDigit(directDeliveryIndicator[0]))
						{
							result = LatestVersion;
						}
						break;
				}
			}
			return result;
		}

		public string GetVersionForEntrySummary(string eightyCharacterBlock)
		{
			var result = string.Empty;
			switch (eightyCharacterBlock.Substring(0, ASESEControlIDMaxLength))
			{
				case "SE60":
					result = LatestVersion;
					break;
			}
			return result;
		}

		public string GetVersionForEntrySummaryQueryResponse(string eightyCharacterBlock)
		{
			var result = string.Empty;
			switch (eightyCharacterBlock.Substring(0, ControlIDMaxLength))
			{
				case "JC":
					ZString oldExtensionSuspensionCode = eightyCharacterBlock.Substring(ExtensionSuspensionCodePosition, ExtensionSuspensionCodeLength);
					if (oldExtensionSuspensionCode.IsEmpty)
					{
						result = LatestVersion;
					}
					break;
			}
			return result;
		}

		public string GetVersionForNewEntrySummaryQueryResponse(string eightyCharacterBlock)
		{
			var result = string.Empty;
			switch (eightyCharacterBlock.Substring(0, ControlIDMaxLength))
			{
				case "JI":
					ZString jiEmptyPosition2 = eightyCharacterBlock.Substring(EmptyPosition2, EmptySpaceLength);
					ZString jiEmptyPosition6 = eightyCharacterBlock.Substring(EmptyPosition6, EmptySpaceLength);
					if (jiEmptyPosition2.IsEmpty && jiEmptyPosition6.IsEmpty)
					{
						result = LatestVersion;
					}
					break;
				case "JK":
					ZString jkEmptyWithNoBillingData = eightyCharacterBlock.Substring(EmptyPosition2, EmptySpaceLength);
					ZString jkNarrativeText = eightyCharacterBlock.Substring(NarrativeTextPosition, NarrativeTextLength);
					if (jkEmptyWithNoBillingData.IsEmpty && !jkNarrativeText.IsEmpty)
					{
						result = LatestVersion;
					}
					break;
				case "JL":
					ZString jlEmptyWithNoCollectionsData = eightyCharacterBlock.Substring(EmptyPosition2, EmptySpaceLength);
					ZString jlNarrativeText = eightyCharacterBlock.Substring(NarrativeTextPosition, NarrativeTextLength);
					if (jlEmptyWithNoCollectionsData.IsEmpty && !jlNarrativeText.IsEmpty)
					{
						result = LatestVersion;
					}
					break;
				case "JN":
					ZString jnEmptyPosition2 = eightyCharacterBlock.Substring(EmptyPosition2, EmptySpaceLength);
					ZString jnEmptyPosition6 = eightyCharacterBlock.Substring(EmptyPosition6, EmptySpaceLength);
					if (jnEmptyPosition2.IsEmpty && jnEmptyPosition6.IsEmpty)
					{
						result = LatestVersion;
					}
					break;
			}
			return result;
		}

		const int ControlIDMaxLength = 2;
		const int ASESEControlIDMaxLength = 4;
		const int ExpandedZoneIDIndicatorPosition = 22;
		const int ExpandedZoneIDIndicatorLength = 1;
		const int FirmsIdentifierPosition = 74;
		const int FirmsIdentifierLength = 4;
		const int DirectDeliveryIndicatorPosition = 25;
		const int DirectDeliveryIndicatorLength = 1;
		const int ExtensionSuspensionCodePosition = 26;
		const int ExtensionSuspensionCodeLength = 1;
		const int EmptyPosition2 = 2;
		const int EmptyPosition6 = 6;
		const int EmptySpaceLength = 1;
		const int NarrativeTextPosition = 3;
		const int NarrativeTextLength = 1;
		const string LatestVersion = "01";
	}
}
