using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IDrawbackSummary
	{
		BusinessObjectFactory Factory { get; }
		void AddMessage(MQEDIMessage message);

		ZString MessageStatus { get; set; }
		ZString EntryStatus { get; set; }
		bool HasChanges { get; }
		bool IsABIFiled { get; }

		// B
		ZString EntryFilerCode { get; }
		ZString ProcessingOfficeCode { get; }

		// D10
		ZString DeleteCode { get; }
		ZString ClaimNumber { get; }
		ZInt ClaimType { get; }
		ZString ClaimPort { get; }
		ZDate EstimatedClaimDate { get; }
		ZString ClaimantIdentification { get; }
		ZString DrawbackTeam { get; }
		ZString BondType { get; }
		ZString SuretyCode { get; }
		ZString NAFTAClaimIndicator { get; }
		ZString GovernmentClaimIndicator { get; }
		ZString AcceleratedClaimIndicator { get; }
		ZString ExporterSummaryProcedureIndicator { get; }
		ZString WaiverOfPriorNoticeIndicator { get; }
		ZString PreInspectionIndicator { get; }
		ZString AgentBroker { get; }
		ZString BrokerReferenceNumber { get; }
		ZString LicensePort { get; }

		// D11
		ZString FirstContractNumber { get; }
		ZString Description { get; }
		ZDate EarliestExportDate { get; }
		ZString PetroleumClaimIndicator { get; }
		ZString NAFTADrawbackCountryCode { get; }

		// D12
		IEnumerable<IDrawbackContractNumber> ExtraContractNumbers { get; }

		// D20
		IEnumerable<IDrawbackTrailerTariff> TrailerTariffs { get; }

		// D25
		IEnumerable<IDrawbackTrailerScheduleBNumber> TrailerScheduleBNumbers { get; }

		// D30
		IEnumerable<IDrawbackImportClaim> ImportClaims { get; }

		// D40
		IEnumerable<IDrawbackManufactureClaim> ManufactureClaims { get; }

		// D50
		IEnumerable<IDrawbackNAFTATariff> NAFTATariffs { get; }

		// D90
		ZDecimal TotalClaimDuty { get; }
		ZDecimal TotalClaimTax { get; }
		ZDecimal TotalNAFTACountryImportDuty { get; }
		ZDecimal TotalUSDollarEquivalentOfNAFTACountryDuty { get; }
	}
}
