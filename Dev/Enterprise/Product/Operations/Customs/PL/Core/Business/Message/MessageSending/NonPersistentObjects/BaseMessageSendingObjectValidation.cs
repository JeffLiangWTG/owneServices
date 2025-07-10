using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using ExportSecurityTypeList = Enterprise.Customs.EU.Business.ExportSecurityTypeList;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
{
	public BaseMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	public new BaseMessageSendingObject Parent => base.Parent as BaseMessageSendingObject;

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateAction();
		ValidateDeclarationDate();
		ValidateEntryNumber();
		ValidateSecurity();
		ValidateAmendmentInvalidationReason();
		ValidateCorrectionAcceptance();
		ValidateAcceptanceComment();
		ValidateResponseMessage();
	}

	public void ValidateAction()
	{
		ValidateCalculatedProperty(Parent.ActionInfo);
	}

	protected void CheckAction()
	{
		if (Parent.ShouldSend)
		{
			MandatoryValidation.CheckEntered(Parent.ActionInfo);
			if (!Parent.Action.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ActionInfo);
			}
		}
	}

	public void ValidateDeclarationDate()
	{
		ValidateCalculatedProperty(Parent.DeclarationDateInfo);
	}

	protected void CheckDeclarationDate()
	{
		if (Parent.ShouldSend
			&& Parent.DeclarationDate != ZDate.Today
			&& (Parent.Action == Constants.MessageSendingObjectActionCodes.ZC415
				|| Parent.Action == Constants.MessageSendingObjectActionCodes.CC515))
		{
			Parent.DeclarationDateInfo.AddError(Res.GetString("PLJobDeclarationMessageSendingObjectValidation|CheckDeclarationDateError",
				@"Invalid declaration date – must be TODAY"));
		}
	}

	public void ValidateEntryNumber()
	{
		ValidateCalculatedProperty(Parent.EntryNumberInfo);
	}

	protected virtual void CheckEntryNumber()
	{
	}

	public void ValidateAmendmentInvalidationReason()
	{
		ValidateCalculatedProperty(Parent.AmendmentInvalidationReasonInfo);
	}

	protected void CheckAmendmentInvalidationReason()
	{
		var parent = Parent;
		MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyHasValue(parent.AmendmentInvalidationReasonInfo, parent.ActionInfo, (ZString)Constants.MessageSendingObjectActionCodes.CC514);
	}

	public void ValidateSecurity()
	{
		ValidateCalculatedProperty(Parent.SecurityInfo);
	}

	protected virtual void CheckSecurity()
	{
		var parent = Parent;

		if (parent.Action != Constants.MessageSendingObjectActionCodes.CC513
			&& parent.Action != Constants.MessageSendingObjectActionCodes.CC515)
		{
			return;
		}

		if (parent.Header?.Declaration is JobDeclaration declaration
			&& declaration.JE_EntryStyle != EntryStyleListExportUCC.Codes.ExportToSpecialTerritory)
		{
			parent.SecurityInfo.ValidCodeCheckAndAddErrorIfEmpty();
		}

		CheckRuleR211();
	}

	public void ValidateCorrectionAcceptance()
	{
		ValidateCalculatedProperty(Parent.CorrectionAcceptanceInfo);
	}

	protected virtual void CheckCorrectionAcceptance()
	{
		var parent = Parent;

		if (parent.Action == Constants.MessageSendingObjectActionCodes.CC566)
		{
			parent.CorrectionAcceptanceInfo.ValidCodeCheckAndAddErrorIfEmpty();
		}
	}

	public void ValidateAcceptanceComment()
	{
		ValidateCalculatedProperty(Parent.AcceptanceCommentInfo);
	}

	protected virtual void CheckAcceptanceComment()
	{
		var parent = Parent;

		if (parent.Action == Constants.MessageSendingObjectActionCodes.CC566)
		{
			MandatoryValidation.CheckEntered(parent.AcceptanceCommentInfo);
		}
	}

	public void ValidateResponseMessage()
	{
		ValidateCalculatedProperty(Parent.ResponseMessageInfo);
	}

	protected virtual void CheckResponseMessage()
	{
		var parent = Parent;

		if (parent.Action == Constants.MessageSendingObjectActionCodes.CC566
			&& parent.ResponseMessage.IsEmpty)
		{
			parent.ResponseMessageInfo.AddWarning(Res.GetString("PLJobDeclarationMessageSendingObjectValidation|CheckResponseMessage",
				"Linked Control Notification doesn't contain discrepancies and PDW request."));
		}
	}

	void CheckRuleR211()
	{
		var parent = Parent;
		if (parent.Security == ExportSecurityTypeList.Codes.EXS
			&& parent.Header.Declaration.ItineraryCountries.Count == 0)
		{
			parent.SecurityInfo.AddMessageError(Res.GetString("PLJobDeclarationMessageSendingObjectValidation|CheckRuleR211",
				"[C0211] IF Security is '2' Itinerary Countries are required."));
		}
	}
}

