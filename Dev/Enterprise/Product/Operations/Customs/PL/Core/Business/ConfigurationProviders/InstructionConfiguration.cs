using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class InstructionConfiguration : EU.Business.InstructionConfiguration
{
	protected override ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => declaration.Configuration.IsUCC6(declaration);

	protected override ZBool SealsSupportCore(JobDeclaration declaration) => false;

	protected override ZBool SpecialProceduresSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => entryInstruction?.CEI_Style.In(new ZString[] { "H1", "H3", "H4" }) ?? false;

	protected override ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => true;

	protected override ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => true;

	protected override IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => null;

	protected override IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => new Declaration.UCC6ExportEntryInstructionValidationDecider();

	protected override ICusAuthorizationUsageValidationDecider UCC6ExportCusAuthorizationUsageValidationDecider => new UCC6ExportCusAuthorizationUsageValidationDecider();
}
