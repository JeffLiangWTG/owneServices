using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public class ReceiptProcessorTest : TestCaseWithFactory
	{
		public void TestProcessReceiptsSQLContainsCorrectColumns()
		{
			var sql = ReceiptProcessor.ProcessReceiptsSQL;
			CombineAssertions("Check to ensure that our raw SQL contains the correct column names (guards against column renames)", () =>
			{
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_IsCustomsControlled, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_RN_NKOrigin, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_CustomsEntryNumber, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_SystemLastEditTimeUtc, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_SystemLastEditUser, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_OP_Product, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_OH_ProductOwner, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_OwnerReference, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_TransactionType, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_Status, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_WOB_CusWHSTransactionBatch, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_IntoBondDate, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_IsFinal, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.WOT_Quantity, sql);
				AssertContains(CusWHSOperatorTransactionSchema.Constants.PK, sql);
				AssertContains(CusWHSOperatorTransactionBatchSchema.Constants.WOB_OA_Warehouse, sql);
				AssertContains(WhsBondedWarehouseAttributeSchema.Constants.WB_MatchingKey, sql);
				AssertContains(WhsBondedWarehouseAttributeSchema.Constants.WB_SystemCreateTimeUtc, sql);
				AssertContains(WhsBondedWarehouseAttributeSchema.Constants.WB_RN_NKCountryOfOrigin, sql);
				AssertContains(WhsBondedWarehouseAttributeSchema.Constants.WB_ParentID, sql);
				AssertContains(WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey, sql);
				AssertContains(WhsDocketLineSchema.Constants.PK, sql);
				AssertContains(WhsDocketLineSchema.Constants.WE_OP, sql);
				AssertContains(WhsDocketLineSchema.Constants.WE_WD, sql);
				AssertContains(WhsDocketLineSchema.Constants.WE_DocketLineStatus, sql);
				AssertContains(OrgSupplierPartSchema.Constants.PK, sql);
				AssertContains(WhsDocketSchema.Constants.PK, sql);
				AssertContains(WhsDocketSchema.Constants.WD_WW_Whs, sql);
				AssertContains(WhsDocketSchema.Constants.WD_DocketType, sql);
				AssertContains(WhsDocketSchema.Constants.WD_DocketSubType, sql);
				AssertContains(GlbBranchSchema.Constants.PK, sql);
				AssertContains(GlbBranchSchema.Constants.GB_GC, sql);
				AssertContains(WhsWarehouseSchema.Constants.PK, sql);
				AssertContains(WhsWarehouseSchema.Constants.WW_GB_RelatedCompanyBranch, sql);
				AssertContains(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, sql);
				AssertContains(JobComInvoiceLineSchema.Constants.JI_MatchingKey, sql);
				AssertContains(JobComInvoiceLineSchema.Constants.JI_ClusterKey, sql);
				AssertContains(JobComInvoiceLineSchema.Constants.JI_OP, sql);
				AssertContains(JobComInvoiceLineSchema.Constants.JI_MatchingKey, sql);
				AssertContains(CusEntryHeaderSchema.Constants.CH_ClusterKey, sql);
				AssertContains(CusEntryHeaderSchema.Constants.PK, sql);
				AssertContains(CusEntryHeaderSchema.Constants.CH_EntryReleaseDate, sql);
				AssertContains(JobDeclarationSchema.Constants.JE_GC, sql);
				AssertContains(CusEntryNumSchema.Constants.CE_ParentID, sql);
				AssertContains(CusEntryNumSchema.Constants.CE_EntryType, sql);
				AssertContains(CusEntryNumSchema.Constants.CE_RN_NKCountryCode, sql);
			});
		}
	}
}
