using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
{
	public CusFiscalReferenceValidation(EU.Business.Declaration.CusFiscalReference parent) : base(parent)
	{ }

	protected override void CheckOwnerOrgPK()
	{
		base.CheckOwnerOrgPK();
		var parent = Parent;
		if (parent.OwnerOrgPK.IsEmpty)
		{
			parent.OwnerOrgPKInfo.AddError(Res.GetString("34570D9A-0E05-45F2-9F79-DD966887BFEE", "Please Enter Fiscal Reference Owner"));
		}
		var organisation = Parent.Owner?.Header;
		if (organisation != null && organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.PolandCodeTypes.TIN).IsEmpty)
		{
			Parent.OwnerOrgPKInfo.AddError(Res.GetString("6CB5FC9B-64FE-498A-BC78-859B675D53C1", "The TIN number for selected organization is missing."));
		}
	}

	protected override void CheckCFR_Code()
	{
		base.CheckCFR_Code();

		var parent = Parent;
		var fiscalReferenceParent = parent.Parent;

		if (fiscalReferenceParent is CusEntryInstruction entryInstruction)
		{
			switch (parent.CFR_Code)
			{
				case FiscalReferenceCodeList.Codes.FR1_Importer:
					CheckRuleR1504(entryInstruction);
					break;
				case FiscalReferenceCodeList.Codes.FR5_Vendor:
					CheckRuleR1592(entryInstruction);
					break;
				case FiscalReferenceCodeList.Codes.FR7_Taxpayer:
					CheckRuleR1636(entryInstruction);
					break;
			}
		}
		else if (fiscalReferenceParent is JobComInvoiceLine invoiceLine)
		{
			switch (parent.CFR_Code)
			{
				case FiscalReferenceCodeList.Codes.FR1_Importer:
					CheckRuleR1504(invoiceLine);
					break;
				case FiscalReferenceCodeList.Codes.FR5_Vendor:
					CheckRuleR1592(invoiceLine);
					break;
				case FiscalReferenceCodeList.Codes.FR7_Taxpayer:
					CheckRuleR1636(invoiceLine);
					break;
			}
		}
	}

	void CheckRuleR1504(CusEntryInstruction entryInstruction)
	{
		var parent = Parent;

		if (IsProcedure42Or63(entryInstruction.CEI_Procedure))
		{
			var hasCodeFR3InEntryInstruction = entryInstruction.FiscalReferences.Cast<CusFiscalReference>().Any(IsPLTaxRepresentative);
			var hasCodeFR3InInvoiceLine = entryInstruction.InvoiceLines.Any(invLine => invLine.FiscalReferences.Cast<CusFiscalReference>().Any(IsPLTaxRepresentative));

			if (hasCodeFR3InEntryInstruction || hasCodeFR3InInvoiceLine)
			{
				parent.CFR_CodeInfo.AddMessageError(Res.GetString("063a9651-82c1-49dc-a2e6-385f46ef8927", "(R1504) Fiscal Role code FR1 cannot be present."));
			}
		}
	}

	void CheckRuleR1504(JobComInvoiceLine invoiceLine)
	{
		var parent = Parent;
		var instruction = invoiceLine.EntryInstruction;

		if (IsProcedure42Or63(instruction?.CEI_Procedure ?? ZString.Empty))
		{
			var hasCodeFR3InEntryInstruction = instruction?.FiscalReferences.Cast<CusFiscalReference>().Any(IsPLTaxRepresentative) ?? false;
			var hasCodeFR3InInvoiceLine = invoiceLine.FiscalReferences.Cast<CusFiscalReference>().Any(IsPLTaxRepresentative);

			if (hasCodeFR3InEntryInstruction || hasCodeFR3InInvoiceLine)
			{
				parent.CFR_CodeInfo.AddMessageError(Res.GetString("1a2dd27c-b073-405c-9998-46857bd1fe05", "(R1504) Fiscal Role code FR1 cannot be present."));
			}
		}
	}

	void CheckRuleR1592(CusEntryInstruction entryInstruction)
	{
		var parent = Parent;
		if (entryInstruction.CEI_SubStyle != Constants.SubStyleCodes.A)
		{
			parent.CFR_CodeInfo.AddMessageError(Res.GetString("c50526f3-3b47-4a9b-bc96-78049948ad7b", "(R1592) Fiscal role code FR5 is invalid for the declaration sub style entered."));
		}
	}

	void CheckRuleR1592(JobComInvoiceLine invoiceLine)
	{
		var parent = Parent;
		if (invoiceLine.EntryInstruction is CusEntryInstruction entryInstruction
			&& entryInstruction.CEI_SubStyle != Constants.SubStyleCodes.A)
		{
			parent.CFR_CodeInfo.AddMessageError(Res.GetString("a74e144e-77e3-4c83-8c38-7413ab2cdd49", "(R1592) Fiscal role code FR5 is invalid for the declaration sub style entered."));
		}
	}

	void CheckRuleR1636(CusEntryInstruction entryInstruction)
	{
		var parent = Parent;
		if (!IsR1636ProcedureCode(entryInstruction.CEI_Procedure))
		{
			parent.CFR_CodeInfo.AddMessageError(Res.GetString("5e5f2e87-9dfe-46a9-9a86-221f9aae8330", "(R1636) Fiscal role code FR7 is invalid for the requested procedure entered."));
		}
	}

	void CheckRuleR1636(JobComInvoiceLine invoiceLine)
	{
		var parent = Parent;
		if (invoiceLine.EntryInstruction is CusEntryInstruction entryInstruction
			&& !IsR1636ProcedureCode(entryInstruction.CEI_Procedure))
		{
			parent.CFR_CodeInfo.AddMessageError(Res.GetString("1a339ab7-e77c-4626-8fa9-c4f769377d42", "(R1636) Fiscal role code FR7 is invalid for the requested procedure entered."));
		}
	}

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();
		var parent = Parent;

		if (parent.CFR_Reference.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Poland)
		{
			var fiscalReferenceParent = parent.Parent;
			if (fiscalReferenceParent is JobComInvoiceLine invoiceLine)
			{
				CheckCFR_ReferenceRulesWhenNotStartWithPL(invoiceLine.EntryInstruction);
			}
			else if (fiscalReferenceParent is CusEntryInstruction entryInstruction)
			{
				CheckCFR_ReferenceRulesWhenNotStartWithPL(entryInstruction);
			}
		}
	}

	void CheckCFR_ReferenceRulesWhenNotStartWithPL(CusEntryInstruction entryInstruction)
	{
		var parent = Parent;
		var referenceCode = parent.CFR_Code;

		if (referenceCode == FiscalReferenceCodeList.Codes.FR1_Importer && IsProcedure42Or63(entryInstruction?.CEI_Procedure ?? ZString.Empty))
		{
			parent.CFR_ReferenceInfo.AddMessageError(Res.GetString("7c1f2bfc-1472-4383-aa78-53e7797bbb37", "(R1507) Tax number starting PL is required."));
		}
		else if (referenceCode == FiscalReferenceCodeList.Codes.FR7_Taxpayer)
		{
			parent.CFR_ReferenceInfo.AddMessageError(Res.GetString("a08eaad9-70ec-4258-8213-f65ac9b11ca9", "(R1621) Tax number starting PL is required."));
		}
	}

	static bool IsR1636ProcedureCode(ZString procedureCode) => procedureCode == Constants.ProcedureCodes._40
																|| procedureCode == Constants.ProcedureCodes._44
																|| procedureCode == Constants.ProcedureCodes._45
																|| procedureCode == Constants.ProcedureCodes._46
																|| procedureCode == Constants.ProcedureCodes._61
																|| procedureCode == Constants.ProcedureCodes._68;

	static bool IsPLTaxRepresentative(CusFiscalReference x) => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative && x.CFR_Reference.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Poland;

	static bool IsProcedure42Or63(ZString procedure) => procedure == Constants.ProcedureCodes._42 || procedure == Constants.ProcedureCodes._63;

	protected override void CheckCFR_ReferenceIsNotEmpty()
	{ }

	protected new CusFiscalReference Parent => (CusFiscalReference)base.Parent;
}
