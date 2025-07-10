using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

[DependentBusinessObject(typeof(CusEntryInstruction), nameof(CusEntryInstruction.CusAuthorizationUsages))]
public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage
{
	public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusAuthorizationUsage.Schema
	{
		public const string CustomsCode = nameof(CusAuthorizationUsage.CustomsCode);
	}

	public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;

	protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);

	public new CusAuthorizationUsageValidation Validation => (CusAuthorizationUsageValidation)base.Validation;

	protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation() => Instruction == null
		? GetNewInvoiceLineAuthorisationUsageValidation()
		: GetNewEntryInstructionAuthorisationUsageValidation();

	EU.Business.CusAuthorizationUsageValidation GetNewInvoiceLineAuthorisationUsageValidation() => IsExport
		? new ExportInvoiceLineCusAuthorizationUsageValidation(this)
		: new CusAuthorizationUsageValidation(this);

	EU.Business.CusAuthorizationUsageValidation GetNewEntryInstructionAuthorisationUsageValidation() => IsExport
		? new ExportEntryInstructionCusAuthorizationUsageValidation(this)
		: (CusAuthorizationUsageValidation)new ImportEntryInstructionCusAuthorizationUsageValidation(this);

	public new CusEntryInstruction Instruction => (CusEntryInstruction)base.Instruction;

	public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	public ZBool IsExport => Instruction is CusEntryInstruction instruction && (instruction.JobDeclaration?.IsExport ?? ZBool.False)
							|| InvoiceLine is JobComInvoiceLine invoiceLine && (invoiceLine.Declaration?.IsExport ?? ZBool.False);

	protected override bool UseEffectiveReferenceNumberCore => true;
}
