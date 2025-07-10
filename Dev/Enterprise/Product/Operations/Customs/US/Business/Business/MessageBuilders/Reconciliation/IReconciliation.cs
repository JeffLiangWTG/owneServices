using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IReconciliation
	{
		BusinessObjectFactory Factory { get; }
		void AddMessage(MQEDIMessage message);
		void LogCustomsCommencedIfNeeded();

		ZString MessageStatus { set; }
		ZGuid CompanyPK { get; }

		// B
		ZString EntryFilerCode { get; }
		ZString OfficeCode { get; }
		ZString PreparerDistrictPort { get; }

		// R10
		ZString EntryNumber { get; }
		ZString ProcessingDistrictPort { get; }
		ZString ImporterID { get; }
		ZString SuretyCode { get; } // Must match all entries being reconciled
		ZDate EstimatedReconciliationEntrySummaryDate { get; }
		// Reconciliation team is calculated from 'Port'
		ZString IssueCode { get; }

		bool AggregateReconciliationIndicator { get; }
		bool IsNoChangeAggregate { get; }
		bool IsWaiveRefund { get; }
		IEnumerable<ZString> AggregateRefundedFees { get; }

		/// <summary>
		/// 1, 2, or 3
		/// </summary>
		ZString IncreaseRefundIndicator { get; }
		/// <summary>
		/// Only required where IssueCode == NF
		/// </summary>
		ZDate EarliestImportDate { get; }
		/// <summary>
		/// Only required where IssueCode != NF
		/// </summary>
		ZDate EarliestEntrySummaryDate { get; }
		ZString AgentBrokerReferenceID { get; }
		ZString BrokerReferenceNumber { get; }

		ZString TeamNumber { get; }

		// R15

		ZInt ImportEntrySource { get; }
		ZString TextComment { get; }

		// R17

		ZString PaymentTypeIndicator { get; }
		ZDate PreliminaryStatementPrintDate { get; }
		ZString ClientBranchDesignation { get; }

		ZDecimal DutyPaymentAmount { get; }
		ZDecimal TaxPaymentAmount { get; }
		ZDecimal FeePaymentAmount { get; }
		ZDecimal InterestPaymentAmount { get; }

		// R20

		IEnumerable<IReconciliationImportEntry> ImportEntries { get; }

		//ARECR10
		ZString DesignatedNotifyParty4811Number { get; }
		ZBool PriorDisclosureIndicator { get; }

		//ARECR11
		ZString ContactName { get; }
		ZString ContactPhone { get; }
		ZString ContactEmail { get; }

		//ARECR15
		ZBool QualifyingGoodFreeTradeDec { get; }
		ZBool SummaryDocProvidedStatement { get; }
		ZBool NAFTA303ClaimStatement { get; }
		ZBool ProtestOrPetitionFiledStatement { get; }

		//ARECRD1 D2 D3
		ZString DocumentRecipientID { get; }
		ZDate DocumentProvidedDate { get; }
		JobDocAddress DocumentRecipientAddress { get; }

		//ARECRC1 C2 C3
		ZString ClaimentID { get; }
		ZDate ClaimDate { get; }
		ZString ClaimIdentifier { get; }
		JobDocAddress ClaimentAddress { get; }

		IEnumerable<IReconEntryLineGroup> EntryLineGroups { get; }
	}
}
