using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Edifact.D05B.Messages.CUSRES;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	class CusresMessageProcessor : CustomsMessageProcessor, IKeysForBlockingParallelProcessingProvider
	{
		public CusresMessageProcessor(LoggingInformation logger)
			: base(logger, EDIInterchange.ApplicationCodes.StowPlan, "Stow Plan response message")
		{
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendGroupPK; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendMode; }
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			var stowPlanMessage = message as StowPlanMessage;
			if (stowPlanMessage != null && stowPlanMessage.ReceiveContent is CUSRESMessage receiveContent)
			{
				JobVoyage voyage = null;

				var arrival = stowPlanMessage.EM_LinkedObject as ISailingEndPoint;
				if (arrival != null)
				{
					voyage = arrival.Direction == Core.Constants.PortDirection.Load ? ((VoyageOrigin)arrival).Voyage : ((VoyageDestination)arrival).Voyage;
				}

				var errors = new List<KeyValuePair<ZString, ZString>>();
				foreach (SegmentGroup4 group4 in receiveContent.Group4)
				{
					for (var i = 0; i < group4.ERC.Count; i++)
					{
						var code = group4.ERC[i].ApplicationErrorDetail.ApplicationErrorCode;
						var description = new ZStringBuilder();
						if (group4.FTX.Count > i)
						{
							description.AppendIfNotEmpty(group4.FTX[i].TextLiteral.FreeText1);
							description.AppendIfNotEmpty(group4.FTX[i].TextLiteral.FreeText2);
							description.AppendIfNotEmpty(group4.FTX[i].TextLiteral.FreeText3);
							description.AppendIfNotEmpty(group4.FTX[i].TextLiteral.FreeText4);
							description.AppendIfNotEmpty(group4.FTX[i].TextLiteral.FreeText5);
						}
						errors.Add(new KeyValuePair<ZString, ZString>(code, description.ToString().Trim()));
					}
				}

				var cusresStatus = ZString.Empty;
				if (errors.Count > 0)
				{
					cusresStatus = errors[errors.Count - 1].Key;
				}

				var voyageReference = ZString.Empty;
				if (voyage != null)
				{
					voyageReference = voyage.HumanReadableName + " at " + arrival.Port + "(" + arrival.EstimatedDate + ")";
				}

				if (arrival != null)
				{
					arrival.Messages.Add(message);
					((IStowPlanMessageAttachee)arrival).StowPlanMessageStatus = GetMessageStatus(cusresStatus);
				}

				HtmlTableCreator creator = null;
				if (errors.Count > 0)
				{
					creator = new HtmlTableCreator(new[] { "Code", "Type", "Description" });
					foreach (var error in errors)
					{
						creator.WriteRow(error.Key, GetErrorType(error.Key), error.Value);
					}
				}

				var subject = "{0} Stow Plan Response for {1}";
				var template = string.Empty;
				if (cusresStatus == CusresErrorCodeList.Codes.Rejection)
				{
					subject = string.Format(subject, "Rejected", voyageReference);
					template = EmailDefBuilder.HtmlTemplates.ErrorResponse;
				}
				else
				{
					subject = string.Format(subject, "Accepted", voyageReference);
					template = EmailDefBuilder.HtmlTemplates.AcceptedResponse;
				}

				var emailBuilder = new EmailDefBuilder(subject, template);
				emailBuilder.AddArgReplacementRange(GetJobLink(voyage), voyageReference, "Stow Plan");
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, creator != null ? creator.ToHtml() : string.Empty, true);

				if (cusresStatus == CusresErrorCodeList.Codes.Rejection)
				{
					SendErrorReport((BusinessObject)arrival, emailBuilder.ToEmail());
				}
				else if (!FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendErrorOnly)
				{
					SendAcknowledgementReport((BusinessObject)arrival, emailBuilder.ToEmail());
				}
			}

			return EDIMessage.Status.Received;
		}

		ZString GetMessageStatus(ZString cusresStatus)
		{
			switch (cusresStatus)
			{
				case CusresErrorCodeList.Codes.Acceptance:
					return MessageStatusListSTW.Codes.Acceptance;
				case CusresErrorCodeList.Codes.AcceptanceWithWarnings:
					return MessageStatusListSTW.Codes.AcceptanceWithWarnings;
				case CusresErrorCodeList.Codes.Rejection:
					return MessageStatusListSTW.Codes.Rejection;
				default:
					return ZString.Empty;
			}
		}

		ZString GetJobLink(JobVoyage voyage)
		{
			var jobNumber = ZString.Empty;
			if (voyage != null)
			{
				jobNumber = ((IJobNumber)voyage).JobNumber;
				if (jobNumber.IsEmpty)
				{
					jobNumber = "Voyage";
				}
			}
			return EmailDefBuilder.GetJobLink(voyage, jobNumber);
		}

		ZString GetErrorType(ZString code)
		{
			var result = "Rejected";
			if (WarningCodes.Contains(code))
			{
				result = "Warning";
			}
			else if (AcceptanceCodes.Contains(code))
			{
				result = "Accepted";
			}
			return result;
		}

		static IEnumerable<ZString> WarningCodes
		{
			get
			{
				yield return "SB6";
				yield return "SB7";
				yield return "SB8";
				yield return "SB9";
				yield return "SBA";
				yield return "SBB";
				yield return "SBT";
			}
		}

		static IEnumerable<ZString> AcceptanceCodes
		{
			get
			{
				yield return "S02";
				yield return "S03";
			}
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendGroupPK; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendMode; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendGroupPK; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return FreightDataRegistry.Instance.USAMSGroupNotification.Value.SendMode; }
		}

		#region IKeysForBlockingParallelProcessingProvider

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			var branchPK = message.EM_GB;
			var linkedObject = message.EM_LinkedObject;
			if (message is StowPlanMessage stowPlanMessage && stowPlanMessage.OriginalMessage is StowPlanMessage originalMessage)
			{
				if (originalMessage.EM_GB.IsValid)
				{
					branchPK = originalMessage.EM_GB;
				}
				if (originalMessage.EM_LinkedObject is BusinessObject originalMessageBizObj)
				{
					linkedObject = originalMessageBizObj;
				}
			}

			if (linkedObject != null)
			{
				return ProcessingResult.New(new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, branchPK, ZString.Empty), (NoResString)string.Empty);
			}
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, UCMPMessageProcessorFactory.GetUnableToFindTheLinkedJobMessage(message));
		}

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var keys = new HashSet<string>();
			var linkedObject = message.EM_LinkedObject;
			if (message is StowPlanMessage stowPlanMessage && stowPlanMessage.OriginalMessage is StowPlanMessage originalMessage)
			{
				if (originalMessage.EM_LinkedObject is BusinessObject originalMessageBizObj)
				{
					linkedObject = originalMessageBizObj;
				}
			}
			if (linkedObject is JobVoyage voyage)
			{
				keys.Add(voyage.PK.ToString());
			}
			else if (linkedObject is ISailingEndPoint arrival)
			{
				voyage = arrival.Direction == Core.Constants.PortDirection.Load ? ((VoyageOrigin)arrival).Voyage : ((VoyageDestination)arrival).Voyage;
				keys.Add(voyage.PK.ToString());
			}

			if (keys.Count == 0 || keys.All(string.IsNullOrEmpty))
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		#endregion
	}
}
