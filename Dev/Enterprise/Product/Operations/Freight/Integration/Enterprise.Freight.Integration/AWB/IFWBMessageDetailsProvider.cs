using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Integration.AWB
{
	public interface IFWBMessageDetailsProvider : IFBaseMessageDetailsProvider
	{
		ZString AirlinePrefix { get; }
		ZString AWBSerialNo { get; }

		ZString AlsoNotifyName { get; }
		ZString AlsoNotifyAddress { get; }
		ZString AlsoNotifyAddress2 { get; }
		ZString AlsoNotifyPlace { get; }
		ZString AlsoNotifyState { get; }
		ZString AlsoNotifyPostCode { get; }

		ZString Booking1stCarrier { get; }
		ZString Booking1stFlight { get; }
		ZString Booking1stFlightDate { get; }
		ZString Booking2ndCarrier { get; }
		ZString Booking2ndFlight { get; }
		ZString Booking2ndFlightDate { get; }

		ZString To1st { get; }
		ZString By1st { get; }
		ZString To2nd { get; }
		ZString By2nd { get; }
		ZString To3rd { get; }
		ZString By3rd { get; }

		ZString AgentIATACodeFormatted { get; }
		ZString AgentAccountNo { get; }
		ZString AgentParticipantIdentifier { get; }
		ZString AgentName { get; }
		ZString AgentPlace { get; }

		ZDecimal TotalWeightPPD { get; }
		ZDecimal ValuationPPD { get; }
		ZDecimal TaxesPPD { get; }
		ZDecimal OtherChargesDueAgentPPD { get; }
		ZDecimal OtherChargesDueCarrierPPD { get; }
		ZDecimal TotalPPD { get; }

		ZDecimal TotalWeightCOL { get; }
		ZDecimal ValuationCOL { get; }
		ZDecimal TaxesCOL { get; }
		ZDecimal OtherChargesDueAgentCOL { get; }
		ZDecimal OtherChargesDueCarrierCOL { get; }
		ZDecimal TotalCOL { get; }

		ZString NetRateCode { get; }

		ZString ShippersSignature { get; }

		ZDateTime AWBIssueDate { get; }
		ZString AWBIssuePlace { get; }
		ZString AWBAgentsSignature { get; }
		ZString KnownConsignorCode { get; }
		ZString ConsolNumber { get; }
		ZString SpecialHandlingCode { get; }

		ZString AgentApprovalCountryCode { get; }
		ZString AgentApprovalNumber { get; }
		ZDateTime AgentApprovalExpiryDate { get; }

		ZString SecurityStatusIssuedBy { get; }
		ZDateTime SecurityStatusIssueDate { get; }

		ZString AdditionalScreeningMethods { get; }

		ZString TSASecurityStatement { get; }

		IReadOnlyCollection<IAWBOtherChargesMessageDetailsProvider> AWBOtherCharges { get; }
		IReadOnlyCollection<IAWBAccountingInformationMessageDetailsProvider> AWBAccountingInformations { get; }
		IReadOnlyCollection<IAWBSpecialHandlingMessageDetailsProvider> AWBSpecialHandlingItems { get; }

		IReadOnlyCollection<IAWBSecurityStatusLineMessageDetailsProvider> CargoSecurityKnownShippers { get; }
		IReadOnlyCollection<ZString> CargoSecurityExemptionGrounds { get; }
		IReadOnlyCollection<ZString> CargoSecurityScreeningMethods { get; }
		IReadOnlyCollection<ZString> AdditionalSecurityInformations { get; }
	}
}
