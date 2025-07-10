using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingNameCollection : NonPersistentBusinessObjectCollection<SterlingName>
	{
		public SterlingNameCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			UpdateCollection(master);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingName();
		}

		void UpdateCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			Xsd.ShipmentShipmentDetails shipmentDetails = master.Shipment.ShipmentDetails;
			AddName(OrganisationTypeCode.ShipFrom, shipmentDetails.Pickup.CartageCompany, GetAddress(shipmentDetails.Pickup.CartageCompany, shipmentDetails, Xsd.DocAddressAddressType.CRG), null);
			AddName(OrganisationTypeCode.BillTo, shipmentDetails.LocalClient, shipmentDetails.LocalClient.OrganisationDetails.Addresses.GetMainOrFirstAddress(), GetContact(shipmentDetails.LocalClient.EDICode, ContactType.Receivables));
			AddName(OrganisationTypeCode.ShipTo, shipmentDetails.Deliver.CartageCompany, GetAddress(shipmentDetails.Deliver.CartageCompany, shipmentDetails, Xsd.DocAddressAddressType.CEG), null);
			AddName(OrganisationTypeCode.Consignee, shipmentDetails.Consignee, GetAddress(shipmentDetails.Consignee, shipmentDetails, Xsd.DocAddressAddressType.CED), GetContact(shipmentDetails.Consignee.EDICode, ContactType.Consignee));
			AddName(OrganisationTypeCode.Shipper, shipmentDetails.Consignor, GetAddress(shipmentDetails.Consignor, shipmentDetails, Xsd.DocAddressAddressType.CRD), GetContact(shipmentDetails.Consignor.EDICode, ContactType.Consignor));
			AddName(OrganisationTypeCode.NotifyParty, shipmentDetails.NotifyParty.Organisation, GetAddress(shipmentDetails.NotifyParty.Organisation, shipmentDetails, Xsd.DocAddressAddressType.NPP), GetContact(shipmentDetails.NotifyParty.Organisation.EDICode, ContactType.All));

			if (master.Consol != null)
			{
				AddName(OrganisationTypeCode.ConsolLevelCarrier, master.Consol.ConsolDetail.Carrier, master.Consol.ConsolDetail.Carrier.OrganisationDetails.Addresses.GetMainOrFirstAddress(), GetContact(master.Consol.ConsolDetail.Carrier.EDICode, ContactType.All));
			}
		}

		Xsd.OrgAddress GetAddress(Xsd.Organisation org, Xsd.ShipmentShipmentDetails shipmentDetails, Xsd.DocAddressAddressType type)
		{
			var result = new Xsd.OrgAddress();
			var docAddress = shipmentDetails.DocAddresses.DocAddress.GetAddressByType(type);
			if (docAddress == null)
			{
				result = GetFallBackAddressFromOrg(org.OrganisationDetails.Addresses, type);
			}
			else
			{
				var reference = docAddress.AddressReference;
				if (docAddress.IsSpecified && reference.Organisation.OrganisationDetails.Addresses.Count > 0)
				{
					result = reference.Organisation.OrganisationDetails.Addresses[reference.AddressSequenceRef - 1];
				}
				else
				{
					result.CompanyName = docAddress.CompanyName;
					result.AddressLine1 = docAddress.AddressLine1;
					result.AddressLine2 = docAddress.AddressLine2;
					result.CityOrSuburb = docAddress.CityOrSuburb;
					result.StateOrProvince = docAddress.StateOrProvince;
					result.PostCode = docAddress.PostCode;
					result.Location.Country = docAddress.CountryCode;
				}

				if (!org.IsSpecified)
				{
					org.EDICode = reference.Organisation.EDICode;
					org.OrganisationDetails.Name = reference.Organisation.OrganisationDetails.Name;
				}
			}

			return result;
		}

		#region GetFallBackAddressFromOrg

		/// <summary>
		/// Currently (01/07/14) Legacy XML only exports the main address but if that changes in the future
		/// this method should get the relevant fall back address for cartage related orgs.
		/// </summary>
		Xsd.OrgAddress GetFallBackAddressFromOrg(Xsd.OrgAddressCollection orgAddressCollection, Xsd.DocAddressAddressType type)
		{
			Xsd.OrgAddress address = null;

			if (type == Xsd.DocAddressAddressType.CRG || type == Xsd.DocAddressAddressType.CEG)
			{
				address = AddressMatchingCapability(orgAddressCollection, Xsd.AddressCapabilityAddressType.PAD);

				if (address == null)
				{
					address = type == Xsd.DocAddressAddressType.CRG
						? AddressMatchingCapability(orgAddressCollection, Xsd.AddressCapabilityAddressType.PIC)
						: AddressMatchingCapability(orgAddressCollection, Xsd.AddressCapabilityAddressType.DLV);
				}
			}

			if (address == null)
			{
				address = orgAddressCollection.GetMainOrFirstAddress();
			}

			return address ?? new Xsd.OrgAddress();
		}

		#endregion

		Xsd.OrgAddress AddressMatchingCapability(Xsd.OrgAddressCollection orgAddressCollection, Xsd.AddressCapabilityAddressType type)
		{
			return orgAddressCollection
				.Cast<Xsd.OrgAddress>()
				.FirstOrDefault(orgAddress => orgAddress.AddressCapabilities.HasCapabilityOfType(type));
		}

		OrgContact GetContact(ZString code, ContactType type)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader header = newFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));
			DefaultContactFinder finder = new DefaultContactFinder(header);
			return finder.DefaultContact(type);
		}

		void AddName(ZString nameType, Xsd.Organisation organisationName, Xsd.OrgAddress orgAddress, OrgContact orgContact)
		{
			if (organisationName.IsSpecified || ((nameType == OrganisationTypeCode.ShipTo || nameType == OrganisationTypeCode.ShipFrom) && orgAddress.IsSpecified))
			{
				AddNew().SetName(nameType, organisationName, orgAddress, orgContact);
			}
		}

		#region Sterling Name Types

		static class OrganisationTypeCode
		{
			public const string ShipFrom = "SF";
			public const string BillTo = "BT";
			public const string ShipTo = "ST";
			public const string Consignee = "CN";
			public const string Shipper = "SH";
			public const string NotifyParty = "NT";
			public const string ConsolLevelCarrier = "CR";
		}

		#endregion
	}
}

