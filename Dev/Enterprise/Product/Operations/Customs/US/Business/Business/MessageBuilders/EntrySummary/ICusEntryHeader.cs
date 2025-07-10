using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface ICusEntryHeader : IHeaderCommon
	{
		// APLB
		bool IsRemoteLocationFiling { get; }
		/// To be used with remote entry filing only.
		ZString PreparerDistrictPort { get; }
		/// This code must equal entry filer code if filing a remote entry, otherwise leave blank.
		ZString PreparerFilerCode { get; }
		/// To be used with remote entry filing only.
		ZString PreparerOfficeCode { get; }

		// ENS10
		ZString UltimateConsigneeNumber { get; }
		ZString CBPF4811ReferenceNumber { get; }
		ZBool LiveEntry { get; }
		ZString MissingDocumentCodes { get; }
		ZDate EstimatedEntryDate { get; }
		ZString EntryNumber { get; }
		ZString SuretyCode { get; }
		ZString StateOfDestination { get; }
		ZBool OGALineReleaseIndicator { get; }
		ZBool IsElectronicInvoicing { get; }
		ZString ImportFTZNumber { get; }
		ZBool IsACECargoReleaseCertification { get; }

		// ENS20
		ZString ImportingVesselName { get; }
		ZString DistrictPortOfUnlading { get; }
		ZDate DateOfImportation { get; }
		ZString BrokerReferenceNumber { get; }
		ZString ClientBranchDesignation { get; }
		ZString VoyageNumber { get; }
		ZDate EstimatedDateOfArrival { get; }
		ZString LocationOfGoods { get; }
		ZBool NAFTAReconciliation { get; }
		ZString OtherReconciliationIndicator { get; }
		IEnumerable<IBillDetails> LowestBillDetails { get; }
		ZBool IsSelfCertification { get; }
		// Single Transaction Bond Data (ENS21)
		ZDecimal BondAmount { get; }
		ZString BondProducerAccountNumber { get; }
		ZBool IsSplitShipment { get; }
		ZBool IsNonAMS { get; }
		ZBool IsPerishable { get; }

		// ENS30
		ZString EntryFilerCodeOfWarehouseEntry { get; }
		ZString WarehouseEntryNumber { get; }
		ZString DistrictPortCodeOfWarehouseEntry { get; }
		ZBool FinalWarehouseIndicator { get; }
		ZString ConsolidatedInformalIndicator { get; }
		ZString DesignatedExamPort { get; }

		ZString CarrierCode { get; }
		ZString SplitShipmentReleaseCode { get; }

		// ENS34
		ZDecimal InformalFee { get; }
		ZDecimal DutiableMailFee { get; }
		ZDecimal ManualSurcharge { get; }

		// ENS35

		ZDecimal BondedADDDuty { get; }
		ZBool BondedADDIndicator { get; }
		ZDecimal PayableADDDuty { get; }
		ZDecimal BondedCVDDuty { get; }
		ZBool BondedCVDIndicator { get; }
		ZDecimal PayableCVDDuty { get; }
		ZString ADDCVDSuretyCode { get; }

		// ENS40
		IEnumerable<ICusEntryLine> EntryLines { get; }

		ZBool IsInvoiceByRequest { get; }

		// ENS89
		IEnumerable<IFee> Fees { get; }
		bool BuildEmpty89EvenIfNoFeeExists { get; }

		// ENS90

		ZDecimal TotalEstimatedDuty { get; }
		ZDecimal TotalEstimatedTax { get; }
		ZString DeferredTaxIndicator { get; }
		ZDecimal TotalCountervailingDuty { get; }
		ZDecimal TotalAntidumpingDuty { get; }
		ZDecimal GrandTotalFee { get; }
		ZDecimal GrandTotalOtherRevenueAmount { get; }

		// ENSH

		ZString PaymentTypeIndicator { get; }
		ZDate PreliminaryStatementPrintDate { get; }
		ZString PeriodicStatementMonth { get; }

		//For BIRD
		IEnumerable<IContainer> Containers { get; }

		IAddressDetails UltimateConsignee { get; }

		//For printing 7501 from message
		void CreateDocPrintingDetails(ZGuid outgoingMsgPK);

		// For Sending Delete Message
		ASESE10 LastCRAcceptedMessageBlock { get; }
		AENS10 LastENSAcceptedMessageBlock { get; }
	}

	public interface IContainer
	{
		ZString ContainerNumber { get; }
		ZString ContainerType { get; }
	}

	public interface IQueryMessageAttachee
	{
		ZString ProcessingDistrictPort { get; }
		ZString ProcessingOfficeCode { get; }
		IEnumerable<(ZString Code, ZString Number)> EntryFilerCodesAndNumbers { get; }
		BusinessObjectFactory Factory { get; }
		EDIMessageCollection Messages { get; }

		ZDateTime DateFrom { get; }
		ZDateTime DateTo { get; }

		Guid CompanyPK { get; }
	}

	public interface IEntrySummaryQueryMessageAttachee : IQueryMessageAttachee
	{
		ZString PreparerDistrictPort { get; }
		bool IsRemoteLocationFiling { get; }
		GlbBranch Branch { get; }

		ZString CriteriaCode { get; }
		ZBool ConsumptionEntrySummaries { get; }
		ZBool FTAReconSummaries { get; }
		ZBool OtherReconSummaries { get; }
		ZBool DrawbackSummaries { get; }
		ZBool NAFTADutyDeferralSummaries { get; }

		ZString CollectionBillInformationCode { get; }
	}

	public interface IACECusEntryHeader : ICusEntryHeader, ICusEntryHeaderMessageAttachee
	{
		ZBool IsPSC { get; }
		ZBool AcceleratedLiqReqIndicator { get; }

		ZBool IsPaid { get; }
		ZBool Consolidated { get; }
		ZBool BondWaivedOrNoBond { get; }
		ZString BondWaiverReasonCode { get; }

		ZString NotifyPartyNumber { get; }
		ZString LloydsCode { get; }
		ZDate ITDate { get; }
		// ENS 31
		ZBool ContinuousBondSuperseded { get; }
		ZString DesignationCode { get; }

		IEnumerable<ZString> ConsolidatedReleaseEntryNumbers { get; }

		ZString ADDCVDBondType { get; }
		ZDecimal ADDCVDSingleTransactionBondAmount { get; }
		ZString ADDCVDSingleTransactionBondAccNo { get; }

		new IEnumerable<IACECusEntryLine> EntryLines { get; }

		ZBool KnownImporterIndicator { get; }
		ZBool IsExpressConsignment { get; }
		ZString PGAExpeditedReleaseIndicator { get; }
	}

	public interface IPSCReasonCodeProvider
	{
		IEnumerable<ZString> PSCHeaderReasonCodes { get; }
		IEnumerable<ZString> GetPSCLineReasonCodes(ZString lineNumber);
	}
}
