using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(UCC6ExportEntryInstructionValidationDecider))]
sealed class UCC6ExportEntryInstructionValidationDeciderTest : EntryInstructionValidationDeciderTest<UCC6ExportEntryInstructionValidationDecider>
{
	public void TestIRuleG0128Configuration_IsActive() => Assert(((IRuleG0128ForCEI_ProcedureDecider)validationDecider).IsActive);

	public void TestIRuleR0028EForCEI_SubStyleDecider_IsActive() => Assert(((IRuleR0028EForCEI_SubStyleDecider)validationDecider).IsActive);

	protected override bool ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult => false;

	protected override bool ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result => false;

	protected override bool ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult => false;

	protected override bool ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result => false;

	protected override bool ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult => false;

	protected override bool ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult => true;

	protected override bool ExpectedIsMaximumEntryLinesAllowedRuleActive => true;
}
