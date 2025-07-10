using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business
{
	public class PSCReasonCodeCollection : NonPersistentBusinessObjectCollection<PSCReasonCode>, IPSCReasonCodeProvider
	{
		public PSCReasonCodeCollection(CusEntryHeader entry)
			: base(entry.Factory)
		{
			this.entry = entry;
			DefaultFromTheFailedLatestTransmission();
		}

		readonly CusEntryHeader entry;

		public void SaveLastPSCReasonCodes()
		{
			EmptyHeaderPSCReasonCodes();
			EmptyLinePSCReasonCodes();
			foreach (PSCReasonCode pscReasonCode in this)
			{
				pscReasonCode.CopyPSCReasonCodes();
			}
		}

		void EmptyHeaderPSCReasonCodes()
		{
			var pscReasonCodes = entry.PSCReasonCodes;
			if (pscReasonCodes != null)
			{
				pscReasonCodes.CY_Data = ZString.Empty;
			}
		}

		void EmptyLinePSCReasonCodes()
		{
			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				var pscReasonCodes = entryLine.PSCReasonCodes;
				if (pscReasonCodes != null)
				{
					pscReasonCodes.CY_Data = ZString.Empty;
				}
			}
		}

		void DefaultFromTheFailedLatestTransmission()
		{
			if (entry != null && entry.Declaration.US_PSC && entry.HasBeenLodgedAtCustoms)
			{
				if (entry.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal || entry.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryReplace)
				{
					var message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
					if (message != null)
					{
						var ens35 = message.MessageBlock.MessageBlocks.OfType<AENS35>().FirstOrDefault();
						if (ens35 != null)
						{
							AddAndDefaultReason(PSCReasonCodeParentTypeList.Codes.Entry,
								"0",
								new ZString[]
								{
									ens35.PostSummaryCorrectionHeaderReasonCode1,
									ens35.PostSummaryCorrectionHeaderReasonCode2,
									ens35.PostSummaryCorrectionHeaderReasonCode3,
									ens35.PostSummaryCorrectionHeaderReasonCode4,
									ens35.PostSummaryCorrectionHeaderReasonCode5
								});
						}

						AENS40 ens40 = null;

						foreach (MessageBlock block in message.MessageBlock.MessageBlocks)
						{
							var blockAs40 = block as AENS40;

							if (blockAs40 != null)
							{
								ens40 = blockAs40;
							}
							else
							{
								var blockAs63 = block as AENS63;

								if (blockAs63 != null && ens40 != null)
								{
									AddAndDefaultReason(PSCReasonCodeParentTypeList.Codes.EntryLine, ens40.LineItemIdentifier,
									new ZString[]
									{
										blockAs63.PostSummaryCorrectionLineReasonCode1,
										blockAs63.PostSummaryCorrectionLineReasonCode2,
										blockAs63.PostSummaryCorrectionLineReasonCode3,
										blockAs63.PostSummaryCorrectionLineReasonCode4,
										blockAs63.PostSummaryCorrectionLineReasonCode5
									});

									ens40 = null;
								}
							}
						}
					}
				}
			}
		}

		void AddAndDefaultReason(ZString parentType, ZString lineNumber, IEnumerable<ZString> reasons)
		{
			var newElement = AddNew();

			newElement.ParentTypeIndicator = parentType;
			newElement.LineNumber = lineNumber;

			int index = 0;
			foreach (ZString reason in reasons)
			{
				if (!reason.IsEmpty)
				{
					switch (index)
					{
						case 0:
							newElement.Reason1 = reason;
							break;
						case 1:
							newElement.Reason2 = reason;
							break;
						case 2:
							newElement.Reason3 = reason;
							break;
						case 3:
							newElement.Reason4 = reason;
							break;
						case 4:
							newElement.Reason5 = reason;
							break;
					}
				}
				index++;
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PSCReasonCode(entry, this);
		}

		#endregion

		internal PSCReasonCode GetHeaderReasonCode()
		{
			foreach (PSCReasonCode reasonCode in this)
			{
				if (reasonCode.ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.Entry)
				{
					return reasonCode;
				}
			}
			return null;
		}

		internal PSCReasonCode GetLineReasonCode(ZString lineNumber)
		{
			foreach (PSCReasonCode reasonCode in this)
			{
				if (reasonCode.ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.EntryLine && reasonCode.LineNumber == lineNumber)
				{
					return reasonCode;
				}
			}
			return null;
		}

		IEnumerable<ZString> IPSCReasonCodeProvider.PSCHeaderReasonCodes
		{
			get
			{
				var reasonCode = GetHeaderReasonCode();
				return reasonCode != null ? reasonCode.GetReasonCodes() : System.Array.Empty<ZString>();
			}
		}

		IEnumerable<ZString> IPSCReasonCodeProvider.GetPSCLineReasonCodes(ZString lineNumber)
		{
			var reasonCode = GetLineReasonCode(lineNumber);
			return reasonCode != null ? reasonCode.GetReasonCodes() : System.Array.Empty<ZString>();
		}
	}
}
