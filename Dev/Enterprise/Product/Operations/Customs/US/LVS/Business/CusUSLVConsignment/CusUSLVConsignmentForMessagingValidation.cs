using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentForMessagingValidation : ZValidation
	{
		public CusUSLVConsignmentForMessagingValidation(CusUSLVConsignmentForMessaging parent)
			: base(parent)
		{
			Parent = parent;
		}

		CusUSLVConsignmentForMessaging Parent { get; }

		public override Type AutoValidationType => typeof(CusUSLVConsignmentForMessaging);

		public void ValidateReasonCode()
		{
			ValidateCalculatedProperty(Parent.ReasonCodeInfo);
		}

		protected void CheckReasonCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ReasonCodeInfo);
		}

		public void ValidateSendToCustoms()
		{
			ValidateCalculatedProperty(Parent.SendToCustomsInfo);
		}

		protected void CheckSendToCustoms()
		{
			var parent = Parent;
			if (parent.SendToCustoms)
			{
				if (parent.Consignment.Lookups.ULB_MessageStatusList.IsWaitingForResponse(parent.Consignment.ULB_MessageStatus))
				{
					parent.SendToCustomsInfo.AddMessageError(ShouldNotSendWhenWaitingForResponse);
				}
			}
		}

		internal const string ShouldNotSendWhenWaitingForResponse = "Message results still pending from last transmission. Please wait for results to be returned before resending.";

		public override void ValidateAll()
		{
			ValidateReasonCode();
			ValidateSendToCustoms();
		}
	}
}
