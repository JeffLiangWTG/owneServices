using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	interface IReconDeclarationAddInfo
	{
		ZString US_EntryFilerCode { get; set; }
		ZPropertyInfo US_EntryFilerCodeInfo { get; }
		ZString US_ImportEntrySource { get; set; }
		ZPropertyInfo US_ImportEntrySourceInfo { get; }
		ZString US_SchDEntry { get; set; }
		ZPropertyInfo US_SchDEntryInfo { get; }
		ZDateTime US_DocProvidedDate { get; set; }
		ZPropertyInfo US_DocProvidedDateInfo { get; }
		ZDateTime US_ClaimDate { get; set; }
		ZPropertyInfo US_ClaimDateInfo { get; }
		ZString US_ClaimID { get; set; }
		ZPropertyInfo US_ClaimIDInfo { get; }
		ZString US_SuretyCode { get; set; }
		ZPropertyInfo US_SuretyCodeInfo { get; }
		ZDateTime US_PaymentDate { get; set; }
		ZPropertyInfo US_PaymentDateInfo { get; }
		ZDateTime US_EstimatedEntryDate { get; set; }
		ZPropertyInfo US_EstimatedEntryDateInfo { get; }
		ZString US_TeamNo { get; set; }
		ZPropertyInfo US_TeamNoInfo { get; }
		ZString US_IssueCode { get; set; }
		ZPropertyInfo US_IssueCodeInfo { get; }
		ZBool US_IsAggregate { get; set; }
		ZPropertyInfo US_IsAggregateInfo { get; }
		ZBool US_R_Waive { get; set; }
		ZPropertyInfo US_R_WaiveInfo { get; }
		ZBool US_R_IsNoChangeAgg { get; set; }
		ZPropertyInfo US_R_IsNoChangeAggInfo { get; }
		ZString US_Comment { get; set; }
		ZPropertyInfo US_CommentInfo { get; }
		ZString US_PaymentType { get; set; }
		ZPropertyInfo US_PaymentTypeInfo { get; }
		ZDateTime US_PreliminaryStatementPrintDate { get; set; }
		ZPropertyInfo US_PreliminaryStatementPrintDateInfo { get; }
		ZString US_ClientBranchDesignation { get; set; }
		ZPropertyInfo US_ClientBranchDesignationInfo { get; }
		ZDecimal US_R_AggregateInterest { get; set; }
		ZPropertyInfo US_R_AggregateInterestInfo { get; }
		ZString US_R_ImporterIDLodged { get; set; }
		ZPropertyInfo US_R_ImporterIDLodgedInfo { get; }
		ZString US_R_TeamNoLodged { get; set; }
		ZPropertyInfo US_R_TeamNoLodgedInfo { get; }
		ZDateTime US_R_EntrySumDateLodged { get; set; }
		ZPropertyInfo US_R_EntrySumDateLodgedInfo { get; }
		ZString US_ENSAction { get; set; }
		ZPropertyInfo US_ENSActionInfo { get; }
		bool HasChangesSinceLastSaving(SchemaColumn addInfoColumn);
	}
}
