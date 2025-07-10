using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public enum PTTSendingOption
	{
		SendPTTMessage = 0,
		CancellPTTMessage = 1,
		SendPTTArrival = 2,
		SendPTTUnArrival = 3
	}

	public class PTTMessageSender : FTZRelatedMessageSender
	{
		public PTTMessageSender(JobDeclaration declaration, PTTSendingOption sendingOption)
			: base(declaration)
		{
			this.sendingOption = sendingOption;
		}
		readonly PTTSendingOption sendingOption;

		protected override ZString MessageTypeDescription
		{
			get
			{
				return SubTypeList.GetDescriptionFromCode(MessageManager.GetPTTMessageSubType(sendingOption));
			}
		}

		CodeDescriptionPairList SubTypeList => Job.Factory.GetCachedValue<EM_MessageSubTypeList>();

		protected override ZBool CanSendThisMessageCore()
		{
			var result = false;

			if (sendingOption == PTTSendingOption.CancellPTTMessage)
			{
				result = Job.IsPTTCleared;
			}
			else if (sendingOption == PTTSendingOption.SendPTTUnArrival)
			{
				result = Job.IsPTTArrivalCleared;
			}
			else
			{
				result = sendingOption == PTTSendingOption.SendPTTArrival || MessageManager.CanSendThisMessage();
			}

			return result;
		}

		protected override bool Prepare()
		{
			var okToSend = base.Prepare();
			if (okToSend && !Job.US_F_DirectDelivery && (Job.FTZEntry == null || Job.MergeManager.RequiresMerge))
			{
				okToSend = Job.DoMerge();
			}

			return okToSend;
		}

		protected override string CannotSendMessageConfirmationMessage
		{
			get
			{
				var result = ZString.Empty;

				if (sendingOption == PTTSendingOption.SendPTTUnArrival)
				{
					result = ValidationConstants.FTZ.CannotSendCancelPTTArrivalMessageConfirmation;
				}
				else if (sendingOption == PTTSendingOption.CancellPTTMessage)
				{
					result = ValidationConstants.FTZ.CannotSendCancelPTTMessageConfirmation;
				}
				else
				{
					result = base.CannotSendMessageConfirmationMessage;
				}

				return result;
			}
		}

		protected override string CannotSendMessageInformationMessage
		{
			get
			{
				var result = ZString.Empty;

				if (sendingOption == PTTSendingOption.SendPTTUnArrival)
				{
					result = ValidationConstants.FTZ.CannotSendCancelPTTArrivalMessageInformation;
				}
				else if (sendingOption == PTTSendingOption.CancellPTTMessage)
				{
					result = ValidationConstants.FTZ.CannotSendCancelPTTMessageInformation;
				}
				else
				{
					result = base.CannotSendMessageInformationMessage;
				}

				return result;
			}
		}

		protected override bool GenerateMessage()
		{
			MessageManager.PopulateMessage();
			return true;
		}

		protected override ValidationModes ValidationMode
		{
			get { return ValidationModes.FTZPTTValidationMode; }
		}

		PTTMessageManager MessageManager
		{
			get { return pttMessageManager ?? (pttMessageManager = new PTTMessageManager(Job, sendingOption)); }
		}
		PTTMessageManager pttMessageManager;
	}
}
