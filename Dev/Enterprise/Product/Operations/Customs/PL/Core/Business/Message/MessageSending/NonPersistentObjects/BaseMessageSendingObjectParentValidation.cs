using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObjectParentValidation : ZValidation
{
	public BaseMessageSendingObjectParentValidation(BaseMessageSendingObjectParent parent) : base(parent)
	{
		Parent = parent;
		parentListInternals = parent;
	}

	public BaseMessageSendingObjectParent Parent
	{
		[System.Diagnostics.DebuggerStepThrough]
		get;
	}

	public override Type AutoValidationType => typeof(BaseMessageSendingObjectParentValidation);

	public override void ValidateAll()
	{
		using (parentListInternals.SuspendListChanged())
		{
			ValidateAllCore();
		}
	}

	public void ValidateCustomsOffice()
	{
		ValidateCalculatedProperty(Parent.CustomsOfficeInfo);
	}

	public void ValidatePurposeOfSending()
	{
		ValidateCalculatedProperty(Parent.PurposeOfSendingInfo);
	}

	public void ValidateRefNumber()
	{
		ValidateCalculatedProperty(Parent.RefNumberInfo);
	}

	public void ValidateMrnNumber()
	{
		ValidateCalculatedProperty(Parent.MrnNumberInfo);
	}

	public void ValidateProcedure()
	{
		ValidateCalculatedProperty(Parent.ProcedureInfo);
	}

	public void ValidateEnquiryInformationCode()
	{
		ValidateCalculatedProperty(Parent.EnquiryInformationCodeInfo);
	}

	public void ValidateOfficeOfExitActual()
	{
		ValidateCalculatedProperty(Parent.OfficeOfExitActualInfo);
	}

	public void ValidateExitDate()
	{
		ValidateCalculatedProperty(Parent.ExitDateInfo);
	}

	protected virtual void ValidateAllCore()
	{
		ValidateCustomsOffice();
		ValidatePurposeOfSending();
		ValidateRefNumber();
		ValidateMrnNumber();
		ValidateProcedure();
		ValidateEnquiryInformationCode();
		ValidateOfficeOfExitActual();
		ValidateExitDate();
	}

	protected void CheckCustomsOffice()
	{
		if (HasEDocs)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CustomsOfficeInfo);
		}
	}

	protected void CheckPurposeOfSending()
	{
		if (HasEDocs)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PurposeOfSendingInfo);
		}
	}

	protected void CheckRefNumber()
	{
		if (HasEDocs)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.RefNumberInfo);
		}
	}

	protected void CheckMrnNumber()
	{
		if (HasEDocs && Parent.PurposeOfSending.EqualsIgnoringCase(MessageSendingPurposeOfSendingList.Codes._1))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.MrnNumberInfo);
		}
	}

	protected void CheckProcedure()
	{
		if (HasEDocs)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ProcedureInfo);
		}
	}

	bool HasEDocs => Parent.HasEDocs;

	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	readonly ISingleElementListInternal parentListInternals;
}
