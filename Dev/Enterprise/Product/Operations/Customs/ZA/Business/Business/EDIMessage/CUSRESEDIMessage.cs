using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CUSRESEDIMessage : SARSEDIMessage, IDocumentSupportable
	{
		public CUSRESEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : Messaging.Business.EDIMessage.Schema
		{
			public const string MRNNumber = "MRNNumber";
			public const string ReceivingProfile = "ReceivingProfile";
			public const string AgentCode = "AgentCode";
			public const string CustomsOfficeCode = "CustomsOfficeCode";
			public const string TransportDocumentNumber = "TransportDocumentNumber";
			public const string ContainerNumbers = "ContainerNumbers";
			public const string LinkedObjectReference = "LinkedObjectReference";
		}

		#endregion

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new CUSRESEDIMessageDocumentSupporter(this));

		DocumentSupporter documentSupporter;

		#region Override Properties

		public override ZString EM_MessageText
		{
			get { return base.EM_MessageText; }
			set
			{
				base.EM_MessageText = value;
				if (!IsCopying)
				{
					RefreshCUSRESHelper();
				}
			}
		}

		public override ZString EM_MessageType
		{
			get { return base.EM_MessageType; }
			set
			{
				var oldValue = EM_MessageType;
				base.EM_MessageType = value;
				if (!IsCopying && oldValue != EM_MessageType)
				{
					RefreshCUSRESHelper();
				}
			}
		}

		public override ZGuid EM_EI
		{
			get { return base.EM_EI; }
			set
			{
				var oldValue = EM_EI;
				base.EM_EI = value;
				if (!IsCopying && oldValue != EM_EI)
				{
					RefreshCUSRESHelper();
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				return !EM_MessageText.IsEmpty ? (ZString)GetHumanFriendlyMessageInterpretation(CUSRESHelper) : base.EM_MessageInterpretation;
			}
			set
			{
				if (!EM_MessageText.IsEmpty)
				{
					throw new NotSupportedException("Setting EM_MessageInterpretation is not supported for CUSRES message");
				}
				base.EM_MessageInterpretation = value;
			}
		}

		public override ZString ParentMessageNumber
		{
			get
			{
				if (!parentMessageNumber.HasValue)
				{
					parentMessageNumber = CUSRESHelper.OutgoingMessageNumber;
				}

				return parentMessageNumber.Value;
			}
		}
		ZString? parentMessageNumber;

		#endregion

		#region New Properties

		#region EntryStatus

		public override ZString EntryStatus
		{
			get
			{
				if (!entryStatus.HasValue)
				{
					entryStatus = CUSRESHelper?.EntryStatus ?? ZString.Empty;
				}
				return entryStatus.Value;
			}
		}
		ZString? entryStatus;

		#endregion

		#region EntryStatusDescription

		public override ZString EntryStatusDescription => entryStatusDescription ?? (entryStatusDescription = EntryStatusList.GetDescriptionFromCode(EntryStatus));
		string entryStatusDescription;

		#endregion

		#region LocalReferenceNumber

		public override ZString LocalReferenceNumber
		{
			get
			{
				if (!lrn.HasValue)
				{
					lrn = CUSRESHelper?.LRNNumber ?? ZString.Empty;
				}
				return lrn.Value;
			}
		}
		ZString? lrn;

		#endregion

		#region MRNNumber

		public ZString MRNNumber
		{
			get
			{
				if (!mrn.HasValue)
				{
					mrn = CUSRESHelper?.MRNNumber ?? ZString.Empty;
				}
				return mrn.Value;
			}
		}
		ZString? mrn;

		public ZPropertyInfo MRNNumberInfo => this.GetZPropertyInfo(Schema.MRNNumber);

		#endregion

		#region AgentCode

		public ZString AgentCode
		{
			get
			{
				if (!agentCode.HasValue)
				{
					agentCode = CUSRESHelper?.AgentCode ?? ZString.Empty;
				}
				return agentCode.Value;
			}
		}
		ZString? agentCode;

		public ZPropertyInfo AgentCodeInfo => this.GetZPropertyInfo(Schema.AgentCode);

		#endregion

		#region CustomsOfficeCode

		public ZString CustomsOfficeCode
		{
			get
			{
				if (!customsOfficeCode.HasValue)
				{
					customsOfficeCode = CUSRESHelper?.CustomsOfficeCode ?? ZString.Empty;
				}
				return customsOfficeCode.Value;
			}
		}
		ZString? customsOfficeCode;

		public ZPropertyInfo CustomsOfficeCodeInfo => this.GetZPropertyInfo(Schema.CustomsOfficeCode);

		#endregion

		#region ReceivingProfile

		public ZString ReceivingProfile
		{
			get
			{
				if (!receivingProfile.HasValue)
				{
					var interchange = Interchange;
					if (interchange != null)
					{
						var unbSegment = new Edifact.Generic.UNBSegment();
						unbSegment.Parse(CharacterSet, interchange.EI_HeaderText);
						receivingProfile = unbSegment.InterchangeRecipient.RecipientIdentification;
					}
					else
					{
						receivingProfile = ZString.Empty;
					}
				}
				return receivingProfile.Value;
			}
		}
		ZString? receivingProfile;

		public ZPropertyInfo ReceivingProfileInfo => this.GetZPropertyInfo(Schema.ReceivingProfile);

		#endregion

		#region TransportDocumentNumber

		public ZString TransportDocumentNumber
		{
			get
			{
				if (!transportDocumentNumber.HasValue)
				{
					transportDocumentNumber = CUSRESHelper?.TransportDocumentNumber ?? ZString.Empty;
				}
				return transportDocumentNumber.Value;
			}
		}
		ZString? transportDocumentNumber;

		public ZPropertyInfo TransportDocumentNumberInfo => this.GetZPropertyInfo(Schema.TransportDocumentNumber);

		#endregion

		#region ContainerNumbers

		public ZString ContainerNumbers
		{
			get
			{
				if (!containerNumbers.HasValue)
				{
					containerNumbers = CUSRESHelper?.Containers ?? ZString.Empty;
				}
				return containerNumbers.Value;
			}
		}
		ZString? containerNumbers;

		public ZPropertyInfo ContainerNumbersInfo => this.GetZPropertyInfo(Schema.ContainerNumbers);

		#endregion

		#region LinkedObjectReference

		public ZString LinkedObjectReference
		{
			get { return ((EM_LinkedObject as IEDIFACTMessageAttachee)?.TopLevelBusinessObject as CusEntryHeader)?.Declaration?.JobNumber ?? ZString.Empty; }
		}

		public ZPropertyInfo LinkedObjectReferenceInfo => this.GetZPropertyInfo(Schema.LinkedObjectReference);

		#endregion

		public CUSRESMessageHelper CUSRESHelper
		{
			get { return cusresHelper ?? (cusresHelper = CUSRESMessageHelper.New(this)); }
		}
		CUSRESMessageHelper cusresHelper;

		#endregion

		CodeDescriptionPairList EntryStatusList => ZARefCusCodeListTypes.GetCustomsStatusList(Factory);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.CUSRES;
			EM_ReceiveTransmit = Direction.Receive;
		}

		void RefreshCUSRESHelper()
		{
			cusresHelper = null;
		}

		#region Implementation

		protected string GetHumanFriendlyMessageInterpretation(CUSRESMessageHelper helper)
		{
			var sb = new ZStringBuilder();
			if (helper != null)
			{
				if (!helper.EntryStatus.IsEmpty)
				{
					sb.AppendFormat("<p><b><u>Status Code {0} - {1}</u></b></p>", helper.EntryStatus, helper.EntryStatusDescription);
				}

				if (helper.HeaderCustomsStatusFreeTextsCount > 0)
				{
					sb.Append("<p><b><u>Entry Header Message/s:</u></b></p>");
					foreach (CustomsStatusFreeTextWrapper freeTextWrapper in helper.HeaderCustomsStatusFreeTexts)
					{
						sb.AppendFormat("<p>{0}</p>", freeTextWrapper.FreeText);
					}
				}

				if (helper.LineCustomsStatusFreeTextsCount > 0)
				{
					sb.Append("<p><b><u>Entry Line Message/s:</u></b></p>");
					foreach (CustomsStatusFreeTextWrapper freeTextWrapper in helper.LineCustomsStatusFreeTexts)
					{
						sb.AppendFormat("<p><b>Line {0}:</b> {1}</p>", freeTextWrapper.Line, freeTextWrapper.FreeText);
					}
				}

				if (helper.SummaryCustomsStatusFreeTextsCount > 0)
				{
					sb.Append("<p><b><u>Entry Summary Message/s:</u></b></p>");
					foreach (CustomsStatusFreeTextWrapper freeTextWrapper in helper.SummaryCustomsStatusFreeTexts)
					{
						sb.AppendFormat("<p>{0}</p>", freeTextWrapper.FreeText);
					}
				}
			}
			else
			{
				sb.Append("Message could not be interpreted correctly.");
			}
			return sb.ToString();
		}

		#endregion

	}
}
