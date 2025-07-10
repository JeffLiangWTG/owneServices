using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public class FWBMessageDetails : FBaseMessageDetails, IFWBMessageDetailsProvider
	{
		public FWBMessageDetails(ExportAWBHeader parent)
			: base(parent)
		{
		}

		#region IFWBMessageDetailsProvider Members

		public ZString AirlinePrefix => parent.EH_AirlinePrefix;

		public ZString AWBSerialNo => parent.EH_AWBSerialNo;

		public ZString AlsoNotifyName => parent.EH_AlsoNotifyName;

		public ZString AlsoNotifyAddress => parent.EH_AlsoNotifyAddress;

		public ZString AlsoNotifyAddress2 => parent.EH_AlsoNotifyAddress2;

		public ZString AlsoNotifyPlace => parent.EH_AlsoNotifyPlace;

		public ZString AlsoNotifyState => parent.EH_AlsoNotifyState;

		public ZString AlsoNotifyPostCode => parent.EH_AlsoNotifyPostCode;

		public ZString Booking1stCarrier => parent.EH_Booking1stCarrier;

		public ZString Booking1stFlight => parent.EH_Booking1stFlight;

		public ZString Booking1stFlightDate => parent.EH_Booking1stFlightDate;

		public ZString Booking2ndCarrier => parent.EH_Booking2ndCarrier;

		public ZString Booking2ndFlight => parent.EH_Booking2ndFlight;

		public ZString Booking2ndFlightDate => parent.EH_Booking2ndFlightDate;

		public ZString To1st => parent.EH_To1st;

		public ZString By1st => parent.EH_By1st;

		public ZString To2nd => parent.EH_To2nd;

		public ZString By2nd => parent.EH_By2nd;

		public ZString To3rd => parent.EH_To3rd;

		public ZString By3rd => parent.EH_By3rd;

		public ZString AgentIATACodeFormatted => parent.EH_AgentIATACodeFormatted;

		public ZString AgentAccountNo => parent.EH_AgentAccountNo;

		public ZString AgentParticipantIdentifier => parent.EH_AgentParticipantIdentifier;

		public ZString AgentName => parent.EH_AgentName;

		public ZString AgentPlace => parent.EH_AgentPlace;

		public ZDecimal TotalWeightPPD => parent.EH_TotalWeightPPD;

		public ZDecimal ValuationPPD => parent.EH_ValuationPPD;

		public ZDecimal TaxesPPD => parent.EH_TaxesPPD;

		public ZDecimal OtherChargesDueAgentPPD => parent.EH_OtherChargesDueAgentPPD;

		public ZDecimal OtherChargesDueCarrierPPD => parent.EH_OtherChargesDueCarrierPPD;

		public ZDecimal TotalPPD => parent.EH_TotalPPD;

		public ZDecimal TotalWeightCOL => parent.EH_TotalWeightCOL;

		public ZDecimal ValuationCOL => parent.EH_ValuationCOL;

		public ZDecimal TaxesCOL => parent.EH_TaxesCOL;

		public ZDecimal OtherChargesDueAgentCOL => parent.EH_OtherChargesDueAgentCOL;

		public ZDecimal OtherChargesDueCarrierCOL => parent.EH_OtherChargesDueCarrierCOL;

		public ZDecimal TotalCOL => parent.EH_TotalCOL;

		public ZString NetRateCode => parent.EH_NetRateCode;

		public ZString ShippersSignature => parent.EH_ShippersSignature;

		public ZDateTime AWBIssueDate => parent.EH_AWBIssueDate;

		public ZString AWBIssuePlace => parent.EH_AWBIssuePlace;

		public ZString AWBAgentsSignature => parent.EH_AWBAgentsSignature;

		public ZString KnownConsignorCode => parent.EH_KnownConsignorCode;

		public ZString ConsolNumber => parent.EH_ConsolNumber;

		public ZString SpecialHandlingCode => parent.EH_SpecialHandlingCode;

		public ZString AgentApprovalCountryCode => parent.EH_RN_NKAgentApprovalCountryCode;

		public ZString AgentApprovalNumber => parent.EH_AgentApprovalNumber;

		public ZDateTime AgentApprovalExpiryDate => parent.EH_AgentApprovalExpiryDate;

		public ZString SecurityStatusIssuedBy => parent.EH_SecurityStatusIssuedBy;

		public ZDateTime SecurityStatusIssueDate => parent.EH_SecurityStatusIssueDate;

		public ZString AdditionalScreeningMethods => parent.EH_AdditionalScreeningMethods;

		public ZString TSASecurityStatement => parent.TSASecurityStatement;

		public IReadOnlyCollection<IAWBOtherChargesMessageDetailsProvider> AWBOtherCharges => parent.AWBOtherCharges.Cast<ExportAWBOtherCharges>().ToList().AsReadOnly();

		public IReadOnlyCollection<IAWBAccountingInformationMessageDetailsProvider> AWBAccountingInformations => parent.AWBAccountingInformations.Cast<ExportAWBAccountingInformation>().ToList().AsReadOnly();

		public IReadOnlyCollection<IAWBSpecialHandlingMessageDetailsProvider> AWBSpecialHandlingItems => parent.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().ToList().AsReadOnly();

		public IReadOnlyCollection<IAWBSecurityStatusLineMessageDetailsProvider> CargoSecurityKnownShippers => parent.CargoSecurityKnownShippers.Cast<ExportAWBSecurityStatusLine>().ToList().AsReadOnly();

		public IReadOnlyCollection<ZString> CargoSecurityExemptionGrounds => parent.CargoSecurityExemptionGrounds.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ExemptionGround).ToList().AsReadOnly();

		public IReadOnlyCollection<ZString> CargoSecurityScreeningMethods => parent.CargoSecurityScreeningMethods.Cast<ExportAWBSecurityStatusLine>().Select(x => x.EAS_ScreeningMethod).ToList().AsReadOnly();

		public IReadOnlyCollection<ZString> AdditionalSecurityInformations => parent.AdditionalSecurityInformations.AsReadOnly();

		#endregion
	}
}
