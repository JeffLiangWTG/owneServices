using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CensusWarningOverrideResponse)]
	class CensusWarningMessageProcessor : ACEABIProcessor
	{
		public override void Process()
		{
			CusEntryHeader entryHeader = CusEntryHeaderLinker.Link(Message);

			bool isFailure;

			string email = GenerateEmailBody(entryHeader, out isFailure);
			string uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entryHeader);

			if (entryHeader != null)
			{
				entryHeader.US_CWOStatus = isFailure ? ImportMessageStatusList.Codes.ErrorCensusWarningOverride : ImportMessageStatusList.Codes.ClearCensusWarningOverride;

				if (entryHeader.CH_Status == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings || entryHeader.CH_Status == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings)
				{
					bool allCWOsCleared = true;

					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						allCWOsCleared &= entryLine.US_CWOs.IsEmpty;

						if (!allCWOsCleared)
						{
							break;
						}
					}

					if (allCWOsCleared)
					{
						var isOriginal = entryHeader.CH_Status == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

						entryHeader.CH_Status = isOriginal ? ImportMessageStatusList.Codes.ClearEntrySummaryOriginal : ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
					}
				}
			}

			string jobNumber = entryHeader == null ? "Unknown" : entryHeader.Declaration.DeclarationReferenceAppendedByFormattedEntryNumber;
			var branch = entryHeader != null ? entryHeader.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(uri, jobNumber, "Census Warning Override", email, isFailure, branch, entryHeader);
		}

		string GenerateEmailBody(CusEntryHeader entry, out bool isFailure)
		{
			isFailure = false;

			HtmlTableCreator table = new HtmlTableCreator(new string[] { "Line #", "Condition Code", "Override Code", "Disposition Code", "Message" });

			Dictionary<CusEntryLine, List<string>> acceptedCWOs = new Dictionary<CusEntryLine, List<string>>();

			foreach (MessageBlock block in Message.MessageBlock.MessageBlocks)
			{
				ACWOCW03 cw03 = block as ACWOCW03;

				if (cw03 != null)
				{
					table.WriteRow(cw03.EntrySummaryLineItemIdentifier, cw03.CensusWarningCode, cw03.CensusOverrideCode, cw03.ConditionCode, cw03.NarrativeText);
					isFailure |= !cw03.Accepted;

					if (cw03.Accepted)
					{
						var entryLine = entry == null ? null : GetEntryLine(entry, ZInt.ParseSafe(cw03.EntrySummaryLineItemIdentifier, 0), cw03.CensusWarningCode);

						if (entryLine != null)
						{
							List<string> cwos;
							if (!acceptedCWOs.TryGetValue(entryLine, out cwos))
							{
								cwos = new List<string>();
								acceptedCWOs.Add(entryLine, cwos);
							}
							cwos.Add(cw03.CensusWarningCode);
						}
					}
				}
			}

			UpdateOutstandingCWOs(acceptedCWOs);

			return table.ToHtml();
		}

		void UpdateOutstandingCWOs(Dictionary<CusEntryLine, List<string>> acceptedCWOs)
		{
			foreach (CusEntryLine entryLine in acceptedCWOs.Keys)
			{
				List<string> list;

				if (acceptedCWOs.TryGetValue(entryLine, out list))
				{
					var outstandingCWs = new List<ZString>(entryLine.US_CWOs.Split(3));
					foreach (string acceptedCW in list)
					{
						outstandingCWs.Remove(acceptedCW);
					}

					entryLine.US_CWOs = new ZStringBuilder(outstandingCWs.ToArray()).ToString();
				}
			}
		}

		CusEntryLine GetEntryLine(CusEntryHeader entryHeader, int lineNumber, ZString cwCode)
		{
			CusEntryLine result = null;
			foreach (CusEntryLine entryLine in entryHeader.EntryLines)
			{
				if (entryLine.CL_LineNumber == lineNumber)
				{
					if (new List<ZString>(entryLine.US_CWOs.Split(3)).Contains(cwCode))
					{
						result = entryLine;
						break;
					}
					else
					{
						foreach (CusEntryLine secondaryLine in entryLine.ChildLines)
						{
							if (secondaryLine.IsSecondaryTariffLine)
							{
								if (new List<ZString>(secondaryLine.US_CWOs.Split(3)).Contains(cwCode))
								{
									result = secondaryLine;
									break;
								}
							}
						}
					}

					if (result == null)
					{
						result = entryLine;
					}
				}
			}

			return result;
		}
	}
}
