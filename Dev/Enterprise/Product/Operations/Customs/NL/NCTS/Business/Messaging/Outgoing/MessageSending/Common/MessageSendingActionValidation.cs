using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class MessageSendingActionValidation : NctsHeaderMessageSendingObjectValidation
{
	public MessageSendingActionValidation(MessageSendingAction parent) : base(parent)
	{
	}

	protected new MessageSendingAction Parent => (MessageSendingAction)base.Parent;

	public override Type AutoValidationType => typeof(MessageSendingActionValidation);
	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateLRN();
		ValidateMRN();
	}

	protected override void CheckMessageType()
	{
		base.CheckMessageType();
		CheckAvailableBalance();
	}

	void CheckAvailableBalance()
	{
		if (Parent.ShouldSend && Parent.MessageType.In<ZString>(NctsMessageTypeListNL.Codes.Declaration, NctsMessageTypeListNL.Codes.Amendment))
		{
			var nctsHeader = Parent.Header;
			var guarantees = nctsHeader.GetEffectiveGuarantees();

			foreach (var nctsGuarantee in guarantees)
			{
				var cusGuarantee = nctsGuarantee.CusGuarantee;

				if (cusGuarantee != null)
				{
					var remainingBalance = cusGuarantee.CPH_Calc_TotalBalanceIncludingPending.Amount;
					var bondAmount = nctsGuarantee.PW_BondAmount;

					if (remainingBalance < bondAmount)
					{
						Parent.MessageTypeInfo.AddError(Res.GetString("96C8A371-0A7D-4970-836E-0E0B75175917", "The available guarantee balance is {0} but {1} is required.", Utilities.FormatNumberNationalWithGroupSeparators((decimal)remainingBalance, 2), Utilities.FormatNumberNationalWithGroupSeparators((decimal)bondAmount, 2)));
					}
				}
			}
		}
	}

	public void ValidateLRN()
	{
		ValidateCalculatedProperty(Parent.LRNInfo);
	}

	protected void CheckLRN()
	{
		if (Parent.Header.IsDepartureMovement && Parent.LRN.IsEmpty)
		{
			Parent.LRNInfo.AddError(Res.GetString("F9248A36-00D9-4BD1-B0F4-7B6FF4D6CDB9", "Customer Reference (LRN) is mandatory. If 'Customer Reference' is not enabled, check if the logon company has an EORI number."));
		}
	}

	public void ValidateMRN()
	{
		ValidateCalculatedProperty(Parent.MRNInfo);
	}

	protected void CheckMRN()
	{
		if (Parent.Header.IsArrivalMovement && Parent.MRN.IsEmpty)
		{
			Parent.MRNInfo.AddError(Res.GetString("8BA76D3B-899D-47E7-8D8A-74B3304E7344", "MRN is mandatory."));
		}
	}
}
