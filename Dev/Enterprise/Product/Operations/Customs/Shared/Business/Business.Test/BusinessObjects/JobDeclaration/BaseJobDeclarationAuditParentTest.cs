using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration))]
	class BaseJobDeclarationAuditParentTest : AuditParentTest<BaseJobDeclaration>
	{
		protected override BaseJobDeclaration NewTestAuditParent() => Factory.New<BaseJobDeclaration>();

		public void TestRelatedAuditChildren()
		{
			var declaration = (IAuditParent)Factory.New<BaseJobDeclaration>();
			AssertContainsExactElementsInAnyOrder([
				new(CusContainerSchema.CO_ClusterKey, CusContainerSchema.CO_ContainerNumber),
				new(CusDecHouseBillSchema.CU_ClusterKey, CusDecHouseBillSchema.CU_BillNum),
				new(CusDecHouseContainerPackSchema.CW_ClusterKey, null),
				new(CusDecHouseContainerPivotSchema.CR_ClusterKey, null),
				new(CusEntryHeaderSchema.CH_ClusterKey, CusEntryHeaderSchema.CH_BGMReference),
				new(CusEntryHeaderChargesSchema.C1_ClusterKey, null),
				new(CusEntryInstructionSchema.CEI_ClusterKey, null),
				new(CusEntryLineSchema.CL_ClusterKey, null),
				new(CusEntryLineFeeSchema.CF_ClusterKey, null),
				new(CusEntryPayInfoSchema.C9_ClusterKey, null),
				new(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_ClusterKey, null),
				new(CusHouseContPackInvoiceLinePivotSchema.CHC_ClusterKey, null),
				new(CusUnderbondDecSchema.BU_ClusterKey, null),
				new(JobComInvLineRefsSchema.JG_ClusterKey, null),
				new(JobComInvoiceHeaderSchema.JZ_ClusterKey, JobComInvoiceHeaderSchema.JZ_InvoiceNumber),
				new(JobComInvoiceHeaderRefsSchema.J2_ClusterKey, null),
				new(JobComInvoiceLineSchema.JI_ClusterKey, JobComInvoiceLineSchema.JI_MatchingKey)
			], declaration.RelatedAuditChildren);
		}
	}
}
