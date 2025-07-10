using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business
{
	public static class PGADispositionProviderExtensionMethods
	{
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")] // ZInt parse returns default of zero if parse fails
		public static void AddOrUpdateCusDispositionOnInvoiceLine(this JobDeclaration declaration, IEnumerable<IPGADispositionProvider> pgaStatusList)
		{
			if (declaration != null)
			{
				var entryHeader = declaration.GetEntryHeaderWithPGADetail();
				AddOrUpdateCusDispositionOnInvoiceLine(entryHeader, pgaStatusList);
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")] // ZInt parse returns default of zero if parse fails
		public static void AddOrUpdateCusDispositionOnInvoiceLine(CusEntryHeader entryHeader, IEnumerable<IPGADispositionProvider> pgaStatusList)
		{
			foreach (var dispositionProvider in pgaStatusList)
			{
				int beginCBPLine, endCBPLine, beginningTariffPosition, endingTariffPosition;
				int.TryParse(dispositionProvider.BeginningCBPLineNo, out beginCBPLine);
				int.TryParse(dispositionProvider.EndingCBPLineNo, out endCBPLine);
				int.TryParse(dispositionProvider.BeginningTariffPosition, out beginningTariffPosition);
				int.TryParse(dispositionProvider.EndingTariffPosition, out endingTariffPosition);
				var matchEntryLines = MatchEntryLines(entryHeader, beginCBPLine, endCBPLine, beginningTariffPosition, endingTariffPosition);

				foreach (var entryLine in matchEntryLines)
				{
					foreach (var invoiceLine in entryLine.InvoiceLines.Cast<JobComInvoiceLine>())
					{
						var pgalines = GetPGALinesForCreateOrUpdateCusDisposition(invoiceLine, dispositionProvider);
						pgalines.ForEach(x => x.PGALineCusDispositions.AddOrUpdateCusDisposition(dispositionProvider, x.PGALineNumber));
					}
				}
			}
		}

		static MQEDIMessage GetLastENSClearedMessageWithPGA(JobDeclaration declaration, bool withPGA = true) =>
			declaration?.FormalEntry.GetLastENSClearedMessageWithPGA(withPGA);

		public static MQEDIMessage GetLastENSClearedMessageWithPGA(this CusEntryHeader entryHeader, bool withPGA = true)
		{
			MQEDIMessage outgoingEntrySummaryMessage = null;
			if (entryHeader != null && !entryHeader.HasBeenWithdrawn)
			{
				var responseMessages = entryHeader.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse }, EDIMessage.Direction.Receive)
						.Cast<MQEDIMessage>()
						.Where(x => x.IsENSCleared)
						.OrderByDescending(x => x.EM_SystemCreateTimeUtc);

				foreach (var message in responseMessages)
				{
					var originalMessage = message.OriginalMessage;
					if (originalMessage != null && originalMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary)
					{
						if (withPGA)
						{
							if (originalMessage.MessageBlock.MessageBlocks.Any(x => x is AENSOI))
							{
								outgoingEntrySummaryMessage = originalMessage;
								break;
							}
						}
						else
						{
							outgoingEntrySummaryMessage = originalMessage;
							break;
						}
					}
				}
			}

			return outgoingEntrySummaryMessage;
		}

		public static MQEDIMessage GetLastENSClearedMessage(JobDeclaration declaration)
		{
			return GetLastENSClearedMessageWithPGA(declaration, false);
		}

		public static MQEDIMessage GetLastCRClearedMessageWithPGA(JobDeclaration declaration, bool withPGA = true)
		{
			MQEDIMessage outgoingCargoReleaseMessage = null;

			var entryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			if (entryHeader != null && !entryHeader.HasBeenWithdrawn)
			{
				var responseMessages = entryHeader.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USCustomsImport, new ZString[] { ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse }, EDIMessage.Direction.Receive)
						.Cast<MQEDIMessage>()
						.Where(x => x.MessageBlock.MessageBlocks.OfType<ASESE90>().Any(y => SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(y.MessageTypeCode)))
						.OrderByDescending(x => x.EM_SystemCreateTimeUtc);

				foreach (var message in responseMessages)
				{
					var originalMessage = message.OriginalMessage;
					if (originalMessage != null && originalMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoRelease)
					{
						if (withPGA)
						{
							if (originalMessage.MessageBlock.MessageBlocks.Any(x => x is AENSOI))
							{
								outgoingCargoReleaseMessage = originalMessage;
								break;
							}
						}
						else
						{
							outgoingCargoReleaseMessage = originalMessage;
							break;
						}
					}
				}
			}

			return outgoingCargoReleaseMessage;
		}

		public static MQEDIMessage GetLastCRClearedMessage(JobDeclaration declaration)
		{
			return GetLastCRClearedMessageWithPGA(declaration, false);
		}

		static IEnumerable<CusEntryLine> MatchEntryLines(CusEntryHeader entryHeader, int beginCBPLine, int endCBPLine, int beginningTariffPosition, int endingTariffPosition)
		{
			if (entryHeader != null)
			{
				var matchEntryLines = entryHeader.MergedLines.Cast<CusEntryLine>().Where(x => MatchingPGALineNumber(x.CL_LineNumber, beginCBPLine, endCBPLine) && MatchingPGALineNumber(x.US_ChildLineNum + 1, beginningTariffPosition, endingTariffPosition));
				foreach (var matchEntryLine in matchEntryLines)
				{
					yield return matchEntryLine;
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")] // ZInt parse returns default of zero if parse fails
		static IEnumerable<IPGALineStatus> GetPGALinesForCreateOrUpdateCusDisposition(JobComInvoiceLine invoiceLine, IPGADispositionProvider dispositionProvider)
		{
			var agencyCode = dispositionProvider.OtherAgencyQuotaIdentifier;
			var programCode = dispositionProvider.GovernmentAgencyProgramCode;
			int beginPGALine, endPGALine;
			int.TryParse(dispositionProvider.BeginningOGALineNo, out beginPGALine);
			int.TryParse(dispositionProvider.EndingOGALineNo, out endPGALine);
			if (beginPGALine != 0 && !agencyCode.IsEmpty && !programCode.IsEmpty)
			{
				return invoiceLine.PGADataCorrections.Where(x => x.PGALineStatusAgencyCode == agencyCode && MatchingPGALineNumber(x.PGALineNumber, beginPGALine, endPGALine));
			}
			return System.Array.Empty<IPGALineStatus>();
		}

		public static CusEntryHeader GetEntryHeaderWithPGADetail(this JobDeclaration declaration)
		{
			CusEntryHeader result = GetRelevantEntry(declaration);
			if (result == null)
			{
				var lastENSClearedMessage = GetLastENSClearedMessageWithPGA(declaration);
				var lastCRClearedMessage = GetLastCRClearedMessageWithPGA(declaration);
				result = GetRelevantEntryForMessage(declaration, lastENSClearedMessage, lastCRClearedMessage);
			}

			return result;
		}

		public static CusEntryHeader GetRelevantEntry(JobDeclaration declaration)
		{
			CusEntryHeader result = null;
			var formalEntry = declaration.FormalEntry;
			var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			if (formalEntry != null && simplifiedEntry == null)
			{
				result = formalEntry;
			}
			else if (formalEntry == null && simplifiedEntry != null)
			{
				result = simplifiedEntry;
			}

			return result;
		}

		static CusEntryHeader GetRelevantEntryForMessage(JobDeclaration declaration, MQEDIMessage lastENSClearedMessage, MQEDIMessage lastCRClearedMessage)
		{
			CusEntryHeader result = null;
			var formalEntry = declaration.FormalEntry;
			var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			if (lastENSClearedMessage != null || lastCRClearedMessage != null)
			{
				if (lastENSClearedMessage != null && lastCRClearedMessage == null)
				{
					result = formalEntry;
				}
				else if (lastENSClearedMessage == null && lastCRClearedMessage != null)
				{
					result = simplifiedEntry;
				}
				else
				{
					if (lastENSClearedMessage.EM_SystemCreateTimeUtc > lastCRClearedMessage.EM_SystemCreateTimeUtc)
					{
						result = formalEntry;
					}
					else
					{
						result = simplifiedEntry;
					}
				}
			}

			if (result == null)
			{
				var formalPGASentMessage = formalEntry?.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.PGADataCorrection, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, EM_MessageSubTypeList.Codes.PGADataCorrection);
				var simplifiedPGASentMessage = simplifiedEntry?.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.PGADataCorrection, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, EM_MessageSubTypeList.Codes.PGADataCorrection);

				var pgaSentMessage = formalPGASentMessage;
				if (pgaSentMessage == null || (simplifiedPGASentMessage != null && pgaSentMessage.EM_SystemCreateTimeUtc < simplifiedPGASentMessage.EM_SystemCreateTimeUtc))
				{
					pgaSentMessage = simplifiedPGASentMessage;
				}
				result = (CusEntryHeader)(pgaSentMessage?.EM_LinkedObject);
			}

			return result;
		}

		public static CusEntryHeader GetEntryHeaderForPGACorrection(this JobDeclaration declaration)
		{
			CusEntryHeader result = GetRelevantEntry(declaration);
			if (result == null)
			{
				var lastENSClearedMessage = GetLastENSClearedMessage(declaration);
				var lastCRClearedMessage = GetLastCRClearedMessage(declaration);
				result = GetRelevantEntryForMessage(declaration, lastENSClearedMessage, lastCRClearedMessage);
			}

			return result;
		}

		static bool MatchingPGALineNumber(int lineNumber, int beginLineFromMessage, int endLineFromMessage)
		{
			return (endLineFromMessage != 0 && (lineNumber >= beginLineFromMessage && lineNumber <= endLineFromMessage)) ||
					(endLineFromMessage == 0 && lineNumber == beginLineFromMessage);
		}

		public static ZString GetStatus(this IPGALineStatus pgaLine, short pgaSequence = -1)
		{
			var disposition = pgaLine.PGALineCusDispositions.Cast<CusDisposition>().FirstOrDefault(x => x.CDI_StatusKey == pgaLine.PGALineStatusAgencyCode && (pgaSequence == -1 || x.CDI_Sequence == (ZShort)pgaSequence));
			return disposition != null ? disposition.CDI_Status : ZString.Empty;
		}

		public static ZDateTime GetStatusDate(this IPGALineStatus pgaLine, short pgaSequence = -1)
		{
			var disposition = pgaLine.PGALineCusDispositions.Cast<CusDisposition>().FirstOrDefault(x => x.CDI_StatusKey == pgaLine.PGALineStatusAgencyCode && (pgaSequence == -1 || x.CDI_Sequence == (ZShort)pgaSequence));
			return disposition != null ? disposition.CDI_StatusDate : ZDateTime.Empty;
		}

		public static ZString GetDescriptionFromZZRefCusCodeList(BusinessObjectFactory factory, ZString code, ZString codeType)
		{
			var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.UnitedStates,
				codeType, ZDateTime.Today);
			if (zzd != null)
			{
				return zzd.ZZD_Description;
			}

			return ZString.Empty;
		}
	}
}
