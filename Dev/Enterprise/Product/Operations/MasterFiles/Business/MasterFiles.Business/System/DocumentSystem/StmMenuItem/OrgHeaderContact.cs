using Enterprise.DocumentEngineCore;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	/// <summary>
	/// Wrapper class that captures either an OrgHeader or OrgContact.
	/// e.g. for NotifyParty contact type, you need to return an orgContact instead of OrgHeader.
	/// </summary>
	public class OrgHeaderContact : IDocumentDeliveryContact
	{
		public OrgHeaderContact(OrgHeader orgHeader, OrgHeader relatedOrgHeader, OrgAddress orgAddress)
		{
			OrgHeader = orgHeader;
			RelatedOrgHeader = relatedOrgHeader;
			OrgAddress = orgAddress;
		}

		public OrgHeaderContact(OrgHeader orgHeader, OrgAddress orgAddress)
		{
			OrgHeader = orgHeader;
			OrgAddress = orgAddress;
		}

		public OrgHeaderContact(OrgContact orgContact)
		{
			OrgContact = orgContact;
		}

		public readonly OrgHeader OrgHeader;
		public readonly OrgContact OrgContact;

		/// <summary>
		/// Autodelivery requires a related organisation, such that if the contact type is Consignee, then you
		/// need to also provide the Consignor as related organisation, vise versa.
		/// </summary>
		public readonly OrgHeader RelatedOrgHeader;

		/// <summary>
		/// The OrgAddress is required to default the delivery language.
		/// E.g. On a shipment we can select an organisation and an address for the consignee.
		/// When delivering a shipment document to a consignee and the system is configured to give priority to address' language, it will use the language from the address defined above.
		/// If your context does not have an OrgAddress, you can pass in null.
		/// </summary>
		public readonly OrgAddress OrgAddress;

		#region IDocumentDeliveryContact Members

		IOrgContact IDocumentDeliveryContact.OrgContact
		{
			get { return OrgContact; }
		}

		IOrgHeader IDocumentDeliveryContact.OrgHeader
		{
			get { return OrgHeader; }
		}

		IOrgHeader IDocumentDeliveryContact.RelatedOrgHeader
		{
			get { return RelatedOrgHeader; }
		}

		IOrgAddress IDocumentDeliveryContact.OrgAddress
		{
			get { return OrgAddress; }
		}

		#endregion
	}
}
