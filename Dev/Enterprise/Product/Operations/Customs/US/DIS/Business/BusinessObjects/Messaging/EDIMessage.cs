using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DIS.Business
{
	public class EDIMessage : Enterprise.Messaging.Business.EDIMessage, Integration.Customs.US.DIS.IEDIMessage
	{
		public static class Constants
		{
			public const string MessageHeader = "MessageHeader";
			public const string MessageType = "MessageType";
			public const string MessageBody = "MessageBody";
			public const string MessageID = "MessageID";
			public const string DocumentID = "DocumentID";
			public const string MessageProcessingResult = "MessageProcessingResult";
			public const string DocumentProcessingResult = "DocumentProcessingResult";
			public const string ProcessingEvent = "ProcessingEvent";
			public const string ProcessingStatus = "ProcessingStatus";
			public const string ProcessingLogText = "ProcessingLogText";
			public const string DocumentLabel = "DocumentLabel";
			public const string CompleteFileName = "CompleteFileName";
			public const string DocumentReviewResult = "DocumentReviewResult";
			public const string DocumentReviewStatus = "DocumentReviewStatus";
			public const string DocumentReviewComment = "DocumentReviewComment";
			public const string DocumentRejectReason = "DocumentRejectReason";
			public const string DocumentReviewResponse = "DocumentReviewResponse";
			public const string ISFNumber = "ISFNumber";
		}

		public EDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.MessageNumberStrategy = new MessageNumStrategyIncludingLicenceCode(() => this.Company ?? GlbCompany.CurrentCompany, () => Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EDIMessage.ApplicationCodes.USCustomsDIS), Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.USCustomsDIS;
		}

		public override ZString EM_MessageText
		{
			get
			{
				if (!Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
				{
					return Regex.Replace(base.EM_MessageText, @">[0-9]{3}-[0-9]{2}-[0-9]{4}<\/", ">***-**-****</");
				}
				return base.EM_MessageText;
			}
			set { base.EM_MessageText = value; }
		}

		public override ZString EM_MessageTextDetail => EM_MessageText;

		internal XElement MessageContent
		{
			get
			{
				if (messageContent == null && !EM_MessageText.IsEmpty)
				{
					messageContent = XElement.Parse(EM_MessageText);
				}
				return messageContent;
			}
		}
		XElement messageContent;

		internal void NullifyMessageContent()
		{
			messageContent = null;
		}

		public EDIMessage RelatedMessage
		{
			get
			{
				if (relatedMessage == null)
				{
					var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsDIS);
					query.AddToFilter(EDIMessageSchema.EM_MessageNum, EM_MessageNum);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, IsTransmitMessage ? EDIMessage.Direction.Receive : EDIMessage.Direction.Transmit);

					relatedMessage = Factory.LoadTop1<EDIMessage>(query);
				}
				return relatedMessage;
			}
		}
		EDIMessage relatedMessage;

		public JobRequiredDocumentAddInfo RequiredDocumentAddInfo
		{
			get { return (JobRequiredDocumentAddInfo)EM_LinkedObject; }
		}

		public ZString DocReviewRejectReason
		{
			get
			{
				var result = ZString.Empty;
				if (EM_MessageType == MessageTypeList.Codes.DocumentReviewResponse)
				{
					var documentReviewResult = MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewResult));
					if (documentReviewResult != null)
					{
						var rejectReason = documentReviewResult.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentRejectReason));
						result = rejectReason != null ? rejectReason.Value : "";
					}
				}
				return result;
			}
		}

		public ZString DocReviewComment
		{
			get
			{
				var result = ZString.Empty;
				if (EM_MessageType == MessageTypeList.Codes.DocumentReviewResponse)
				{
					var documentReviewResult = MessageContent.Descendants().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewResult));
					if (documentReviewResult != null)
					{
						var reviewComment = documentReviewResult.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewComment));
						result = reviewComment != null ? reviewComment.Value : "";
					}
				}
				return result;
			}
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsInDatabase && IsTransmitMessage && EM_MessageType == MessageTypeList.Codes.Submission)
			{
				if (MessageAttachments.Count > 0)
				{
					EM_MessageText = EM_MessageText.Replace(DISMessageBuilder.DocumentImagePlaceHolder, string.Format(DISMessageBuilder.DocumentImagePlaceHolder, MessageAttachments[0].PK));
				}
			}
		}

		protected override string MessageNumberPlaceHolderOverride
		{
			get { return DISMessageNumberPlaceHolder; }
		}

		internal const string DISMessageNumberPlaceHolder = "!MessageNumberPlaceHolder!";

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(JobRequiredDocumentAddInfo);
		}

		protected override void AfterNewObjectIsLinked(BusinessObject newBizObj)
		{
			base.AfterNewObjectIsLinked(newBizObj);

			var requiredDocumentAddInfo = newBizObj as JobRequiredDocumentAddInfo;
			if (requiredDocumentAddInfo != null)
			{
				var requiredDocument = requiredDocumentAddInfo.RequiredDocument;

				if (requiredDocument != null)
				{
					switch (requiredDocument.EQ_ParentTableCode)
					{
						case CusISFHeaderSchema.Constants.Prefix:
							requiredDocument.ParentType = ObjectFactory.GetType<Integration.Customs.US.ISF.ICusISFHeader>();
							break;
						default:
							requiredDocument.ParentType = ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingDocsAndCartage>();
							break;
					}
				}
			}
		}
	}
}
