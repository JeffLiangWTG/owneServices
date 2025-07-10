using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ImportInvoiceLinePackageValidation : InvoiceLinePackageValidation
{
	public ImportInvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
		: base(package, invoiceLine)
	{
	}

	protected override void CheckPackQtyCore()
	{
		base.CheckPackQtyCore();

		CheckForRuleR491();
	}

	public void CheckForRuleR491()
	{
		if (InvoiceLine.EntryInstruction is CusEntryInstruction entryInstruction)
		{
			var procedure = entryInstruction.CEI_Procedure;
			var packType = Parent.Package?.CW_PackType ?? ZString.Empty;
			if (!PackageHelper.IsBulkCode(packType, Parent.Factory)
				&& procedure != Constants.ProcedureCodes._71
				&& procedure != Constants.ProcedureCodes._76
				&& !EntryInstructionPackageCountIsGreaterThan0(entryInstruction))
			{
				Parent.PackQtyInfo.AddMessageError(Res.GetString("PLImportInvoiceLinePackageValidation|CheckForRuleR491", "(R491) – For the requested procedure code, number of packs > 0 is required for the Entry Instruction."));
			}
		}
	}

	bool EntryInstructionPackageCountIsGreaterThan0(CusEntryInstruction entryInstruction) => entryInstruction?.InvoiceLines
																								.Any(invoiceLine => invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>()
																									.Any(package => package.CHC_NumberOfPacks > 0))
																							?? false;
}
