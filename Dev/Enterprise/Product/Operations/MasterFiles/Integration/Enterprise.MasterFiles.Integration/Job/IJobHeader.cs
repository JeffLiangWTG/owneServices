using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IJobHeader : IBusiness, IDisposable
	{
		ZDateTime JH_A_JCL { get; set; }
		ZDateTime JH_A_JOP { get; set; }
		ZDecimal JH_AgentChargesCFX { get; set; }
		ZString JH_ARInvoiceReference { get; set; }
		ZString JH_ClientContractNumber { get; set; }
		ZString JH_Description { get; set; }
		ZBool JH_ExcludeFromPeriodicRating { get; set; }
		ZGuid JH_GB { get; set; }
		ZGuid JH_GC { get; set; }
		ZGuid JH_GE { get; set; }
		ZString JH_GS_NKRepOps { get; set; }
		ZString JH_GS_NKRepSales { get; set; }
		ZString JH_HeaderType { get; set; }
		ZString JH_HoldReason { get; set; }
		ZBool JH_IsProfitSharePosted { get; set; }
		ZGuid JH_JH_ParentJob { get; set; }
		ZByte JH_JobBufferPercentOverride { get; set; }
		ZString JH_JobLocalReference { get; set; }
		ZString JH_JobNum { get; set; }
		ZDateTime JH_JobPlannedStartDate { get; set; }
		ZDecimal JH_LocalChargesCFX { get; set; }
		ZString JH_LocalClientInvoicingStyle { get; set; }
		ZString JH_Name { get; set; }
		ZGuid JH_OA_AgentCollectAddr { get; set; }
		ZGuid JH_OA_LocalChargesAddr { get; set; }
		ZGuid JH_OC_LocalBillingContact { get; set; }
		ZGuid JH_ParentID { get; set; }
		ZString JH_ParentTableCode { get; set; }
		ZString JH_PaymentCollectionStatus { get; set; }
		ZString JH_ProfitLossReasonCode { get; set; }
		ZGuid JH_ProfitShareInvoice { get; set; }
		ZBool JH_RatingHasBeenRun { get; set; }
		ZDateTime JH_RevenueRecognizedDate { get; set; }
		ZBool JH_SingleAgentsInvoicePerConsol { get; set; }
		ZString JH_Status { get; set; }
		ZDateTime JH_SystemCreateTimeUtc { get; set; }
		ZString JH_SystemCreateUser { get; set; }
		ZDateTime JH_SystemLastEditTimeUtc { get; set; }
		ZString JH_SystemLastEditUser { get; set; }
		ZString JH_TH_NKQuoteNumber { get; set; }
		ZShort JH_UniqueJobInvoiceNumber { get; set; }
		ZGuid PK { get; }
		IJobHeaderParentCore Parent { get; }
		void ReverseTransactionCommissions();
		void SetOrgForCommissionsReversal(ZGuid orgPk);
	}
}
