using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class MessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(MessageSendingObject parent) : base(parent)
		{
		}

		public new MessageSendingObject Parent => base.Parent as MessageSendingObject;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateAction();
		}

		public void ValidateAction()
		{
			ValidateCalculatedProperty(Parent.ActionInfo);
		}
		protected virtual void CheckAction()
		{
			if (Parent is MessageSendingObject parent && parent.ShouldSend && parent.Header is CusEntryHeader header && header.Declaration is JobDeclaration declaration && !header.CusDispositions.Cast<CusDisposition>().Any(c => c.CDI_StatusKey == EntryStatusCodeList.Codes.ARM && c.CDI_Type == Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber && c.CDI_Status == CPT_016_RejectionReasons.Codes.F88))
			{
				var targetInfo = parent.ActionInfo;
				var action = parent.Action;
				if (action.IsEmpty)
				{
					targetInfo.AddError(Res.GetString("79B93ABF-1930-43F2-A662-9DAF650DD948", "Action is required if Send is ticked."));
				}
				else
				{
					var hasBeenLodgedAtCustoms = header.HasBeenLodgedAtCustoms;
					if (action == ActionCodeList.Codes.Create && hasBeenLodgedAtCustoms)
					{
						targetInfo.AddMessageError(ValidationConstants.MessageSendingObject.ActionCodeIsInvalidWhenReceived);
					}
					else if (action == ActionCodeList.Codes.Update && !hasBeenLodgedAtCustoms)
					{
						targetInfo.AddMessageError(ValidationConstants.MessageSendingObject.ActionCodeIsInvalidWhenNotReceived);
					}
					else
					{
						ListValidation.ErrorIfInvalidCode(targetInfo, parent.ActionList);
					}
					if (action == ActionCodeList.Codes.Create)
					{
						var dateForDuty = header.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;
						if (dateForDuty.Date != ZDateTime.Today.Date)
						{
							targetInfo.AddWarning(ValidationConstants.MessageSendingObject.DeclarationDateShouldBeToday);
						}
					}
				}
			}
		}

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				if (Parent.MessageType.IsEmpty)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("E2714334-FDE0-4B5E-A6F1-4DE538293DAD", "Message should not be selected to send with missing Message Type.Please check the Declaration Shipment Type."));
				}
			}
		}
	}
}
