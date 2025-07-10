using System.Xml.Serialization;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	#region Base

	[XmlSerializerAssembly("Enterprise.Customs.SG.V4.Business.XmlSerializers")]
	public partial class TradenetDeclaration : ITradeNetSectionParent
	{
		int ITradeNetSectionParent.TotalNumbers { get => TotalNumberOfDeclaration; set => TotalNumberOfDeclaration = value; }
	}

	[XmlSerializerAssembly("Enterprise.Customs.SG.V4.Business.XmlSerializers")]
	public partial class TradenetResponse : ITradeNetSectionParent
	{
		int ITradeNetSectionParent.TotalNumbers { get => TotalNumberOfResponse; set => TotalNumberOfResponse = value; }
	}

	#endregion

	#region In Sections

	public partial class CertificateOfOrigin : ITradeNetInSectionWithCertificate { }

	public partial class InNonPayment : ITradeNetInSectionWithCargo, ITradeNetInSectionWithLicence, ITradeNetInSectionWithInvoice { }

	public partial class InPayment : ITradeNetInSectionWithCargo, ITradeNetInSectionWithLicence, ITradeNetInSectionWithInvoice { }

	public partial class OutwardDeclaration : ITradeNetInSectionWithCargo, ITradeNetInSectionWithLicence, ITradeNetInSectionWithCertificate { }

	public partial class TranshipmentMovement : ITradeNetInSectionWithCargo, ITradeNetInSectionWithLicence { }

	#endregion

	#region Header Sections

	public partial class Header : ITradeNetHeaderSection { }

	public partial class CancellationCancellationHeader : ITradeNetHeaderSection { }

	public partial class RefundOnlyRefundHeader : ITradeNetHeaderSection { }

	#endregion

	#region Out Sections

	public partial class RejectionMessage : ITradeNetOutSection { }

	public partial class ApprovalMessage : ITradeNetOutSection { }

	public partial class FeeMessage : ITradeNetOutSection { }

	public partial class ErrorMessage : ITradeNetOutSection { }

	public partial class CustomsExchangeRate : ITradeNetOutSection { }

	public partial class CustomsCommonCode : ITradeNetOutSection { }

	public partial class CertificateOfOriginApproval : ITradeNetOutSection { }

	#endregion

	#region Out Permit Sections

	public partial class TranshipmentMovementPermit : ITradeNetOutPermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class OutwardPermit : ITradeNetOutPermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class InPaymentPermit : ITradeNetOutPermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class InNonPaymentPermit : ITradeNetOutPermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class InPaymentUpdatePermit : ITradeNetOutUpdatePermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class InNonPaymentUpdatePermit : ITradeNetOutUpdatePermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class OutwardUpdatePermit : ITradeNetOutUpdatePermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	public partial class TranshipmentMovementUpdatePermit : ITradeNetOutUpdatePermitSection
	{
		ITradeNetInSection ITradeNetOutPermitSection.Declaration => Declaration;
	}

	#endregion

	#region Party Sectons

	public partial class ImporterParty : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}

	public partial class ExporterPartyPartyDetail : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}
	public partial class HandlingAgentParty : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}

	public partial class InwardCarrierAgentParty : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}

	public partial class OutwardCarrierAgentParty : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}

	public partial class DeclaringAgentParty : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}

	public partial class ManufacturerPartyPartyDetail : ITradeNetParty
	{
		ITradeNetPartyIdentification ITradeNetParty.PartyIdentification => PartyIdentification;
	}

	public partial class ImporterPartyPartyIdentification : ITradeNetPartyIdentification { }

	public partial class ExporterPartyPartyDetailPartyIdentification : ITradeNetPartyIdentification { }

	public partial class HandlingAgentPartyPartyIdentification : ITradeNetPartyIdentification { }

	public partial class InwardCarrierAgentPartyPartyIdentification : ITradeNetPartyIdentification { }

	public partial class OutwardCarrierAgentPartyPartyIdentification : ITradeNetPartyIdentification { }

	public partial class DeclaringAgentPartyPartyIdentification : ITradeNetPartyIdentification { }

	public partial class ManufacturerPartyPartyDetailPartyIdentification : ITradeNetPartyIdentification { }

	#endregion

	#region Approval Condition

	public partial class PermitCAApprovalCondition : IApprovalCondition
	{
	}

	public partial class PermitSCApprovalCondition : IApprovalCondition
	{
	}

	#endregion
}
