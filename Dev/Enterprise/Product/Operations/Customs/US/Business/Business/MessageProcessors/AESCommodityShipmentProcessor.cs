using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, CBPEDIInterchange.ApplicationCodes.USCustomsExport)]
	public class AESCommodityShipmentProcessor : AESTIRProcessor<AESCommShipAXT, AESCommShipBXT, AESCommShipYXT, AESCommShipES1XT>
	{
		public override void Process()
		{
			base.Process();
			ProcessDispositionCodes();
		}

		void ProcessDispositionCodes()
		{
			if (Message.EM_LinkedObject is CusEntryHeader entry)
			{
				var entryES1List = new List<AESCommShipES1XT>();
				var entryLineES1Dic = new Dictionary<AESCommShipCL1XT, List<AESCommShipES1XT>>();

				AESCommShipCL1XT lastProcessedCL1XTBlock = null;
				foreach (var block in messageBlocks)
				{
					if (block is AESCommShipES1XT es1)
					{
						if (es1.FinalDispositionIndicator.IsEmpty)
						{
							if (lastProcessedCL1XTBlock == null)
							{
								entryES1List.Add(es1);
							}
							else
							{
								if (!entryLineES1Dic.TryGetValue(lastProcessedCL1XTBlock, out var es1List))
								{
									es1List = new List<AESCommShipES1XT>();
									entryLineES1Dic.Add(lastProcessedCL1XTBlock, es1List);
								}

								es1List.Add(es1);
							}
						}
						else
						{
							entryES1List.Add(es1);
						}
					}
					else if (block is AESCommShipCL1XT cl1)
					{
						lastProcessedCL1XTBlock = cl1;
					}
				}

				var messageDateTime = Message.EM_MessageDateTime;
				entry.AESCusDispositions.RemoveAndDeleteAll();
				entry.AllEntryLines.OfType<CusEntryLine>().ForEach(x => x.AESCusDispositions.RemoveAndDeleteAll());
				ZString blank = "Space";
				entryES1List.ForEach(es1 =>
				{
					entry.AESCusDispositions.AddCusDisposition(es1.SeverityIndicator.IsEmpty ? blank : es1.SeverityIndicator, messageDateTime, es1.ResponseCode);
				});

				foreach (var linePair in entryLineES1Dic)
				{
					var cl1 = linePair.Key;
					var entryLine = entry.AllEntryLines.FindByLineNumber(cl1.LineNumber);
					if (entryLine != null)
					{
						var es1List = linePair.Value;
						es1List.ForEach(es1 =>
						{
							entryLine.AESCusDispositions.AddCusDisposition(es1.SeverityIndicator.IsEmpty ? blank : es1.SeverityIndicator, messageDateTime, es1.ResponseCode);
						});
					}
				}
			}
		}
	}

	public abstract class AESTIRProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY, AESTIRES1MessageBlock> : Processor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IAESControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IAESControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IAESControlMessageBlockY, new()
		where AESTIRES1MessageBlock : MessageBlock, IAESTIRES1MessageBlock
	{
		public new MQEDIMessage Message
		{
			get { return (MQEDIMessage)base.Message; }
			set { base.Message = value; }
		}

		public override void Process()
		{
			var entry = LinkToEntryAndUpdateMessageSubType();

			var jobNumber = "Unknown";
			var uSPPI = ZString.Empty;
			var subject = ResponseEmailSubject;
			var url = "";
			if (entry != null)
			{
				jobNumber = entry.CH_BGMReference;
				var dec = entry.Declaration;
				if (dec != null && dec.JE_DeclarationReference != jobNumber)
				{
					jobNumber = dec.JE_DeclarationReference + " / " + jobNumber;
				}
				uSPPI = GeneratePPICompanyName(entry);
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(entry);
			}

			HtmlTableCreator creator = null;
			var html = new StringBuilder();
			var isFailure = false;
			var errorCodes = new List<ZString>();
			var itn = ZString.Empty;
			var highestSeverity = USCAESResponseCode.SeverityType.NotApplicable;
			var responseCode = ZString.Empty;

			foreach (var block in messageBlocks)
			{
				if (block is AESTIRES1MessageBlock es1)
				{
					responseCode = es1.ResponseCode;
					if (!errorCodes.Contains(responseCode))
					{
						errorCodes.Add(responseCode);
					}

					isFailure |= IsRejectedCode(es1.FinalDispositionIndicator);

					if (!es1.AESInternalTransactionNumberITN.IsEmpty)
					{
						itn = es1.AESInternalTransactionNumberITN;
					}

					var severity = AESSeverityIndicatorList.GetSeverityType(es1.SeverityIndicator);
					if (severity > highestSeverity)
					{
						highestSeverity = severity;
					}

					if (creator == null)
					{
						creator = new HtmlTableCreator(GetColumnTitles());
					}

					AddColumnValues(creator, es1);

					if (!itn.IsEmpty)
					{
						AddITNValues(creator, itn);
					}
				}
			}

			if (creator != null)
			{
				html.Append(creator.ToHtml());
			}

			if (USCustomsDataRegistry.Instance.AESSendNotifications != Core.Constants.EmailTo.NoEmails)
			{
				if (uSPPI != "")
				{
					subject = subject + " (USPPI: " + uSPPI + ")";
				}
				EmailDef email;
				var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
				if (new HtmlResponseEmailGenerator().TryGenerateEmail(url, jobNumber, subject, html.ToString(), AppendixAReferenceLink, isFailure, out email, branch))
				{
					SendNotification(email, entry);
				}
			}

			if (entry != null)
			{
				CalculateStatus(entry, highestSeverity, isFailure, responseCode);
				if (!itn.IsEmpty)
				{
					if (entry.CH_EntryStatus == AESDirectCustomsEntryStatus.Codes.DeleteSEDClear)
					{
						entry.EntryNumber = ZString.Empty;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						entry.Logs.AddNew(Events.DeletedARecordInTheSystem, "SED Delete: [" + itn + "]");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
					else
					{
						entry.EntryNumber = itn;
					}
				}
			}
		}

		protected override List<ZString> ExcludedErrorCodes
		{
			get
			{
				if (excludedErrorCodes == null)
				{
					excludedErrorCodes = new List<ZString>
					{
						AESCommodityFilingFinalDispositionsCodes.Codes._970 // SHIPMENT REJECTED; RESOLVE & RETRANSMIT
					};
				}
				return excludedErrorCodes;
			}
		}
		List<ZString> excludedErrorCodes;

		ZString GeneratePPICompanyName(CusEntryHeader entry)
		{
			var uSPPI = ZString.Empty;
			var usPPIOrg = entry.USPPI;

			if (usPPIOrg.USOrganisationDocAddress is USOrganisationDocAddress docAddress && !docAddress.IsDeleted && docAddress.E2_AddressOverride)
			{
				uSPPI = docAddress.E2_CompanyName;
			}
			if (uSPPI.IsEmpty)
			{
				uSPPI = usPPIOrg.Address?.OA_CompanyNameOverride ?? ZString.Empty;
			}
			if(uSPPI.IsEmpty)
			{
				uSPPI = usPPIOrg.Organisation?.OH_FullNameTruncated ?? ZString.Empty;
			}
			if (uSPPI.IsEmpty)
			{
				var originalMessage = entry.Messages.OfType<MQEDIMessage>().Where(x => x.EM_ReceiveTransmit == MQEDIMessage.Direction.Transmit && x.EM_MessageType == ApplicationIdentifierCodeList.AES.CommodityShipment).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
				if (originalMessage != null)
				{
					if (originalMessage.MessageBlock.B is IAESControlMessageBlockB originalBBlock)
					{
						uSPPI = originalBBlock.USPPIName;
					}
				}
			}
			return uSPPI;
		}

		ZString[] GetUserToNotifyEmailAddresses(CusEntryHeader entry)
		{
			var result = new List<ZString>();
			var originalMessage = Message.OriginalMessage;
			if (originalMessage == null)
			{
				if (entry != null)
				{
					foreach (var message in entry.Messages.OfType<MQEDIMessage>()
						.Where(x => x.EM_ApplicationCode == EDIMessage.ApplicationCodes.USCustomsExport
						&& x.EM_MessageType == ApplicationIdentifierCodeList.AES.CommodityShipment
						&& x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
						&& x.EM_Status == EDIMessage.Status.Sent
						&& x.EM_SystemCreateTimeUtc < Message.EM_SystemCreateTimeUtc))
					{
						var staff = message.UserWhoQueuedThisRecord;
						if (staff != null && staff.GS_IsActive && !staff.GS_IsSystemAccount && !staff.GS_EmailAddress.IsEmpty)
						{
							result.Add(staff.GS_EmailAddress);
						}
					}
				}
			}
			else
			{
				var userWhoQueuedThisRecord = originalMessage.UserWhoQueuedThisRecord;
				if (userWhoQueuedThisRecord != null)
				{
					result.Add(userWhoQueuedThisRecord.GS_EmailAddress);
				}
			}

			return result.Distinct().ToArray();
		}

		void SendNotification(EmailDef email, CusEntryHeader entry)
		{
			var recipientCalculator = new EmailRecipientCalculator(USCustomsDataRegistry.Instance.AESSendNotifications, USCustomsDataRegistry.Instance.AESSendNotificationsToGroup, GetUserToNotifyEmailAddresses(entry), ZGuid.Empty);
			recipientCalculator.SendNotifications(Factory, email, USCustomsDataRegistry.Instance.AESSendNotificationsToGroupItem);
		}

		Enterprise.Messaging.Business.EDIMessage GetLastXPMessage(EDIMessageCollection collection) => collection.GetLastMessage(CBPEDIInterchange.ApplicationCodes.USCustomsExport, ApplicationIdentifierCodeList.AES.CommodityShipment, MQEDIMessage.Direction.Transmit);

		CusEntryHeader LinkToEntryAndUpdateMessageSubType()
		{
			CusEntryHeader result = null;

			if (!Message.EM_MessageNum.IsEmpty)
			{
				result = CusEntryHeaderLinker.Link(Message);
				var originalMessage = Message.OriginalMessage;
				if (originalMessage != null)
				{
					Message.EM_MessageSubType = originalMessage.EM_MessageSubType;
				}
			}

			if (result == null)
			{
				var sc1Block = messageBlocks.OfType<AESCommShipSC1XP>().FirstOrDefault();
				result = FindEntryHeaderByReferenceNumberAndLinkMessage(sc1Block?.ShipmentReferenceNumber ?? ZString.Empty, (reference) => CusEntryHeader.LoadForBGMReference(Factory, reference) as CusEntryHeader);
			}

			if (result == null)
			{
				var es1Block = messageBlocks.OfType<AESCommShipES1XT>().FirstOrDefault();
				result = FindEntryHeaderByReferenceNumberAndLinkMessage(es1Block?.AESInternalTransactionNumberITN ?? ZString.Empty, (reference) => new CusEntryHeader.Loader(Message.Factory).FindByEntryNumberAndMessageType(GlbCompany.CurrentCompany.PK, reference, CusEntryHeaderMessageTypeList.Codes.Export));
			}

			return result;

			CusEntryHeader FindEntryHeaderByReferenceNumberAndLinkMessage(ZString reference, Func<ZString, CusEntryHeader> loadEntryHeaderFunc)
			{
				if (!reference.IsEmpty && loadEntryHeaderFunc != null)
				{
					var entryHeader = loadEntryHeaderFunc(reference);
					if (entryHeader != null)
					{
						Message.EM_LinkedObject = entryHeader;
						var lastOriginalMessage = GetLastXPMessage(entryHeader.Messages);
						if (lastOriginalMessage != null)
						{
							Message.EM_MessageSubType = lastOriginalMessage.EM_MessageSubType;
						}

						return entryHeader;
					}
				}

				return null;
			}
		}

		internal const string ResponseEmailSubject = "Shippers Export Declaration";
		internal const string AppendixAReferenceLink = @"For more information on each code see <a href=""http://www.cbp.gov/document/guidance/aestir-appendix-commodity-filing-response-messages"">Appendix A</a>.";

		protected override bool IsRejectedCode(string code)
		{
			return code == USConstants.AESRejectionDisposition;
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.AESSendNotificationsToGroupItem;
		}

		void CalculateStatus(CusEntryHeader entry, USCAESResponseCode.SeverityType highestSeverity, bool isFailure, ZString responseCode)
		{
			if (entry != null)
			{
				var status = ZString.Empty;

				if (!isFailure && Message.EM_MessageSubType == EM_MessageSubTypeList.Codes.SEDDelete)
				{
					status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
				}
				else
				{
					switch (highestSeverity)
					{
						case USCAESResponseCode.SeverityType.Fatal:
							status = AESDirectCustomsEntryStatus.Codes.Error;
							break;
						case USCAESResponseCode.SeverityType.NotApplicable:
						case USCAESResponseCode.SeverityType.Notification:
						case USCAESResponseCode.SeverityType.Informational:
							var entryStatus = AESCommodityFilingFinalDispositionsCodes.GetEntryStatus(responseCode);
							if (!string.IsNullOrEmpty(entryStatus))
							{
								status = entryStatus;
							}
							else
							{
								switch (Message.EM_MessageSubType)
								{
									case EM_MessageSubTypeList.Codes.SEDReplace:
										status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
										break;
									default:
										status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
										break;
								}
							}
							break;
						case USCAESResponseCode.SeverityType.Verify:
							status = AESDirectCustomsEntryStatus.Codes.Verification;
							break;
						case USCAESResponseCode.SeverityType.Compliance:
							status = AESDirectCustomsEntryStatus.Codes.Compliance;
							break;
						case USCAESResponseCode.SeverityType.Warning:
							status = AESDirectCustomsEntryStatus.Codes.WarningCorrectRetransmit;
							break;
						default:
							status = AESDirectCustomsEntryStatus.Codes.Unknown;
							break;
					}
				}

				if (!status.IsEmpty)
				{
					entry.CH_Status = status;
					entry.CH_EntryStatus = status;
				}
			}
		}

		void AddColumnValues(HtmlTableCreator creator, AESTIRES1MessageBlock es1)
		{
			creator.WriteRow(es1.ResponseCode, GetSeverityDescription(es1.SeverityIndicator), es1.NarrativeText);
		}

		void AddITNValues(HtmlTableCreator creator, string iTN)
		{
			creator.WriteRow("ITN", "", iTN);
		}

		string GetSeverityDescription(ZString severityIndicator)
		{
			return Factory.GetCachedValue<AESSeverityIndicatorList>().GetDescriptionFromCode(severityIndicator) ?? "";
		}

		IEnumerable<string> GetColumnTitles()
		{
			return new string[] { "Code", "Severity", "Message" };
		}

		protected new MessageErrorCalculator MessageCalculator
		{
			get { return (MessageErrorCalculator)base.MessageCalculator; }
		}

		protected override Messaging.Business.MessageErrorCalculator GetNewMessageCalculator()
		{
			return new MessageErrorCalculator(Factory);
		}
	}
}
