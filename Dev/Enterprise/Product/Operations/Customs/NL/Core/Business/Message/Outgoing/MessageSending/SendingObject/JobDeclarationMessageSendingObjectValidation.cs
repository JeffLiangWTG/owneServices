using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business.Customs.EU;

namespace Enterprise.Customs.NL.Business;

public class JobDeclarationMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
{
	public JobDeclarationMessageSendingObjectValidation(JobDeclarationMessageSendingObject parent) : base(parent)
	{
	}
	public new JobDeclarationMessageSendingObject Parent => (JobDeclarationMessageSendingObject)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();

		ValidateReasonForInvalidation();
		ValidateExitType();
		ValidateZG_TypeOfSecurity();
		ValidateRepresentative();
	}

	#region MessageType
	protected override void CheckMessageType()
	{
		base.CheckMessageType();
		if (Parent.MessageType.IsEmpty)
		{
			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo, Parent.MessageTypeInfo.Description);
		}
		else
		{
			if (Parent.Lookups.MessageTypeList.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo, Parent.Lookups.MessageTypeList);
			}
			if ((Parent.MessageType == ExportSendMessageTypes.Codes.DEC || Parent.MessageType == ExportSendMessageTypes.Codes.AMD) && Parent.Header.EntryInstruction is Declaration.CusEntryInstruction instruction && instruction.ZG_IsHighValueOvrd)
			{
				if (instruction.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5 }) && instruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_ValuationCode.Equals(ValuationMethodList.Codes._1)))
				{
					Parent.MessageTypeInfo.AddError(Res.GetString("39e8cfda-2ece-4b7d-ae45-72599316812e", "You cannot send VI (valuation indicators aka DV1) if there is an invoice line with valuation method different from 1 for declaration types H1, H4 or H5."));
				}
				if (!instruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.DeferredPayment))
				{
					Parent.MessageTypeInfo.AddError(Res.GetString("742314d7-0a40-4f02-91df-db1d63b8cb41", "You cannot sent VI (valuation indicators aka DV1) if there is no authorization with code DPO registered in the Entries Instruction tab."));
				}
			}
		}
	}
	#endregion

	#region ReasonForInvalidation
	protected void CheckReasonForInvalidation()
	{
		if (Parent.IsMessageTypeCAN)
		{
			MandatoryValidation.CheckEntered(Parent.ReasonForInvalidationInfo);
		}
	}

	public void ValidateReasonForInvalidation()
	{
		ValidateCalculatedProperty(Parent.ReasonForInvalidationInfo);
	}
	#endregion

	#region ExitType
	public void ValidateExitType()
	{
		ValidateCalculatedProperty(Parent.ExitTypeInfo);
	}

	protected void CheckExitType()
	{
	}
	#endregion

	#region ZG_TypeOfSecurity
	public void ValidateZG_TypeOfSecurity()
	{
		ValidateCalculatedProperty(Parent.ZG_TypeOfSecurityInfo);
	}

	protected void CheckZG_TypeOfSecurity()
	{
		if (Parent.ZG_TypeOfSecurity.IsEmpty)
		{
			switch (Parent.Header.EntryInstruction?.CEI_Style ?? ZString.Empty)
			{
				case DeclarationTypeList.Codes.B1:
				case DeclarationTypeList.Codes.B2:
					Parent.ZG_TypeOfSecurityInfo.AddMessageError(Res.GetString("2D62F4DB-1007-47B6-9381-B518174D6968", "Security is required for this declaration type"));
					break;
				case DeclarationTypeList.Codes.C1:
					Parent.ZG_TypeOfSecurityInfo.AddWarning(Res.GetString("A9BD4A1B-2B20-4F8A-81C0-84AA84501377", "Security may be required for this declaration type"));
					break;
			}
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_TypeOfSecurityInfo);
		}
	}
	#endregion

	#region Representative
	void ValidateRepresentative()
	{
		var errorMessage = Res.GetString("37F7DA44-AE76-4B7F-9914-84F3F92F1B1B", "EORI number from Representative is required.");
		Parent.RemoveRowError(errorMessage);

		var representative = Parent.Declaration?.Representative;
		if (representative != null && string.IsNullOrEmpty(representative.GetEORI()))
		{
			Parent.AddRowError(errorMessage);
		}
	}
	#endregion
}
