using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarParty
	{
		PartyType PartyType { get; }
		ZString IdentificationCode { get; }
		ZString Address1 { get; }
		ZString PartyName { get; }

		ZString Street { get; }
		ZString City { get; }
		ZString State { get; }
		ZString Postcode { get; }
		ZString Country { get; }
		OrgAddress PostalAddress { get; }
	}

	public enum PartyType
	{
		// For header
		ReportingCarrier_RL,
		GroupingCentre_FZ,
		MessageSender_MS,
		TransitPrincipalsAgentOrRep_AH,
		Driver_DR,
		CrewMember_FM,
		Passenger_FL,
		ReportingCarrier_DEG,

		// For lines:
		Consignor_CZ,
		Consignee_CN,
		FreightForwarder_FW,
		NotifyParty1_N1,
		NotifyParty2_N2
	}
}
