using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceLineValidation(JobComInvoiceLine parent) : AutoPLJobComInvoiceLineValidation(parent)
{
	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateJI_MarkModel();
		ValidateProcedureCodeBase();
		ValidatePreviousProcedureCode();
		ValidateJI_AdditionalSupplements();
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
	protected CusEntryInstruction EntryInstruction => Parent.EntryInstruction;

	protected bool HasSupportingDocumentCodes(IReadOnlyCollection<ZString> supportingDocumentCsiCodes) =>
		Parent.HasSupportingDocumentCode(supportingDocumentCsiCodes)
		|| (Parent.InvoiceHeader?.HasSupportingDocumentCode(supportingDocumentCsiCodes) ?? false)
		|| (Parent.Declaration?.HasSupportingDocumentCode(supportingDocumentCsiCodes) ?? false);

	public string DateIsInTheFutureWarningMsg = Res.GetString("17594832-A216-47A6-8774-F6DF2ADEB9A0", "Date is in the future");

	#region JI_Procedure

	protected override void CheckJI_Procedure()
	{
		base.CheckJI_Procedure();
		var targetInfo = Parent.JI_ProcedureInfo;
		CheckEntryInstructionGuarantee(targetInfo);
	}

	#endregion

	#region JI_CEI

	protected override void CheckJI_CEI()
	{
		var targetInfo = Parent.JI_CEIInfo;
		CheckEntryInstructionIsNotEmpty(targetInfo);
		CheckEntryInstructionGuarantee(targetInfo);
		CheckUniqueCurrencyUsed(targetInfo);
	}

	void CheckEntryInstructionIsNotEmpty(ZPropertyInfo targetInfo)
	{
		if (Parent.JI_CEI.IsEmpty)
		{
			targetInfo.AddMessageError(Res.GetString("6b78409e-195b-427f-8a9b-2553bab96222",
				"You have not selected an Entry Instruction"));
		}
	}

	void CheckEntryInstructionGuarantee(ZPropertyInfo targetInfo)
	{
		var procedure = Parent.JI_Procedure;
		if (procedure.StartsWith(ProcedureCodes._71) && (EntryInstruction?.HasGuarantees() ?? false))
		{
			targetInfo.AddMessageError(Res.GetString("53544829-D7DE-4358-88D4-583D26B85AAE",
				"When procedure code starts with 71, the associated entry instruction can't have guarantee(s)"));
		}
	}

	void CheckUniqueCurrencyUsed(ZPropertyInfo targetInfo)
	{
		if (!Parent.EntryInstruction?.UniqueCurrencyUsed ?? false)
		{
			targetInfo.AddMessageError(Res.GetString("PLJobComInvoiceLineValidation|CheckUniqueCurrencyUsed",
				"All Invoices on an Entry Instruction must have the same '[22] Currency'."));
		}
	}

	#endregion

	protected override void CheckJI_DateForDutyOverride()
	{
		if (Parent.JI_DateForDutyOverride > ZDateTime.Now)
		{
			Parent.JI_DateForDutyOverrideInfo.AddWarning(DateIsInTheFutureWarningMsg);
		}
	}

	protected override void CheckJI_ValuationDateOverride()
	{
		if (Parent.JI_ValuationDateOverride > ZDateTime.Now)
		{
			Parent.JI_ValuationDateOverrideInfo.AddWarning(DateIsInTheFutureWarningMsg);
		}
	}

	#region JI_MarkModel

	public void ValidateJI_MarkModel()
	{
		ValidateCalculatedProperty(Parent.JI_MarkModelInfo);
	}

	protected virtual void CheckJI_MarkModel()
	{
	}

	#endregion

	public void ValidateProcedureCodeBase()
	{
		ValidateCalculatedProperty(Parent.ProcedureCodeBaseInfo);
	}

	protected virtual void CheckProcedureCodeBase()
	{
		var propertyInfo = Parent.ProcedureCodeBaseInfo;
		var value = Parent.ProcedureCodeBase;

		ListValidation.MessageErrorIfInvalidCode(propertyInfo, ResString.GetMultilingualString("PLJobComInvoiceLineValidation|InvalidProcedureCodeBase",
			"Requested Customs Procedure Code is invalid."));

		if (Parent.EntryInstruction != null && !Parent.EntryInstruction.CEI_Procedure.IsEmpty && value != Parent.EntryInstruction.CEI_Procedure)
		{
			propertyInfo.AddMessageError(Res.GetString("PLJobComInvoiceLineValidation|InvalidCPCProcedureCodeBase",
				"Requested Procedure Code must be the same as the Entry Instruction CPC."));
		}
	}

	public void ValidatePreviousProcedureCode()
	{
		ValidateCalculatedProperty(Parent.PreviousProcedureCodeInfo);
	}

	protected virtual void CheckPreviousProcedureCode()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.PreviousProcedureCodeInfo, ResString.GetMultilingualString("PLJobComInvoiceLineValidation|InvalidPreviousProcedureCode",
			"Previous Customs Procedure Code is invalid."));
	}

	protected override void CheckJI_CustomsQuantity()
	{
		base.CheckJI_CustomsQuantity();
		if (Parent.CustomsWeight.InKilogramsSafe > Parent.EffectiveGrossWeight.InKilogramsSafe)
		{
			Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("PLJobComInvoiceLineValidation|CheckJI_CustomsQuantity", "[38] Customs Qty must be less or equal [35] GWT"));
		}
	}

	protected override bool AdditionalProcedureCodesCheckFirst4Characters => false;

	protected override void CheckJI_CountryOfOrigin()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
	}

	public void ValidateJI_AdditionalSupplements()
	{
		ValidateCalculatedProperty(Parent.JI_AdditionalSupplementsInfo);
	}

	protected void CheckJI_AdditionalSupplements()
	{
		var additionalProcedureCodesList = Parent.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().ToList();
		additionalProcedureCodesList.ForEach(x => Parent.JI_AdditionalSupplementsInfo.AddAllNotificationsFrom(x.CY_CodeInfo));
	}

	protected override void CheckJI_CustomsSecondUnitQty()
	{
		base.CheckJI_CustomsSecondUnitQty();

		var parent = Parent;
		CheckDuplicateUOMExists(parent.JI_CustomsSecondUnitQty, parent.JI_CustomsSecondUnitQtyInfo);
	}

	protected override void CheckJI_CustomsThirdUnitQty()
	{
		base.CheckJI_CustomsThirdUnitQty();

		var parent = Parent;
		CheckDuplicateUOMExists(parent.JI_CustomsThirdUnitQty, parent.JI_CustomsThirdUnitQtyInfo);
	}

	protected override void CheckJI_CustomsFourthUnitQty()
	{
		base.CheckJI_CustomsFourthUnitQty();

		var parent = Parent;
		CheckDuplicateUOMExists(parent.JI_CustomsFourthUnitQty, parent.JI_CustomsFourthUnitQtyInfo);
	}

	protected override void CheckJI_CustomsFifthUnitQty()
	{
		base.CheckJI_CustomsFifthUnitQty();

		var parent = Parent;
		CheckDuplicateUOMExists(parent.JI_CustomsFifthUnitQty, parent.JI_CustomsFifthUnitQtyInfo);
	}

	void CheckDuplicateUOMExists(ZString unitQty, ZPropertyInfo propertyInfo)
	{
		if (!unitQty.IsEmpty
			&& UnitList.Count(x => x == unitQty) >= 2)
		{
			propertyInfo.AddMessageError(Res.GetString("PLJobComInvoiceLineValidation|CheckDuplicateUOMExists", "The UOM is duplicated for the invoice line."));
		}
	}

	ZString[] UnitList => [Parent.JI_CustomsSecondUnitQty, Parent.JI_CustomsThirdUnitQty, Parent.JI_CustomsFourthUnitQty, Parent.JI_CustomsFifthUnitQty];

	protected override bool HasValidPackagePivots => true;
}
