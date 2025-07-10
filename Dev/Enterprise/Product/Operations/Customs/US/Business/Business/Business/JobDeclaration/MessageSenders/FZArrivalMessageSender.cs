using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class FZArrivalMessageSender : FTZRelatedMessageSender
	{
		public FZArrivalMessageSender(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString MessageTypeDescription
		{
			get { return EM_MessageSubTypeList.Descriptions.FZArrival; }
		}

		protected override ZBool CanSendThisMessageCore()
		{
			return Header.Bills.Any();
		}

		protected override ZBool DoesSendWithMessageErrorsCore(ZString messageType)
		{
			return false;
		}

		protected override bool Prepare()
		{
			var canSend = Header.Bills.Any();
			if (canSend)
			{
				return true;
			}
			else
			{
				Job.MessageInitiator.NotifyUserOfAnInvalidOperation(CannotSendArrival);
				return false;
			}
		}
		internal const string CannotSendArrival = "System cannot send an Arrival Of Goods message, because no Bills are entered.";

		protected override bool GenerateMessage()
		{
			var header = Header;
			var builder = new FZArrivalMessageBuilder(header);
			var message = builder.PopulateMessage();
			header.AddMessage(message);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZArrival;
			header.SetMessageStatus(EM_MessageSubTypeList.Codes.FZArrival, new FTZMessageStatusCalculator().Calculate(message, false, false));

			return true;
		}

		protected override ValidationModes ValidationMode
		{
			get { return ValidationModes.FTZArrivalValidationMode; }
		}

		IFZEventHeader Header
		{
			get { return Job; }
		}
	}
}
