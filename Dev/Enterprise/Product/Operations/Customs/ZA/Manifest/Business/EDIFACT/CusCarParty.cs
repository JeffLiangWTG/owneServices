using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;
using AsycudaBillAddress = Enterprise.Customs.ManifestBase.AsycudaBillAddress;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	class CusCarPartyFromOrgAddress : ICusCarParty
	{
		public CusCarPartyFromOrgAddress(OrgAddress party, PartyType partyType, ZString messagingCountryCode)
		{
			this.partyType = partyType;
			this.oa = party;
			this.messagingCountryCode = messagingCountryCode;
		}

		readonly ZString messagingCountryCode;
		readonly OrgAddress oa;
		readonly PartyType partyType;

		public ZString Address1
		{
			get { return oa.OA_Address1; }
		}

		public ZString City
		{
			get { return oa.OA_City; }
		}

		public ZString Country
		{
			get { return oa.OA_RN_NKCountryCode; }
		}

		public ZString IdentificationCode
		{
			get
			{
				var cusCodeType = ZString.Empty;
				switch (PartyType)
				{
					// NB: _FZ not needed here

					case PartyType.ReportingCarrier_RL:
						cusCodeType = OrgCusCode.CodeTypes.CarrierCode; // CCC
						break;
					case PartyType.TransitPrincipalsAgentOrRep_AH:
						cusCodeType = OrgCusCode.CodeTypes.CarrierCode; // CCC
						break;
				}
				if (!cusCodeType.IsEmpty)
				{
					return oa.Header.CustomsCodes.GetCustomsRegNo(cusCodeType, messagingCountryCode);
				}
				return ZString.Empty;
			}
		}

		public ZString PartyName
		{
			get { return oa.Header.OH_FullName; }
		}

		public PartyType PartyType
		{
			get { return partyType; }
		}

		public ZString Postcode
		{
			get { return oa.OA_PostCode; }
		}

		public ZString State
		{
			get { return oa.OA_State; }
		}

		public ZString Street
		{
			get { return oa.OA_Address2; }
		}

		public OrgAddress PostalAddress
		{
			get
			{
				return oa.Header.Addresses.Cast<OrgAddress>().FirstOrDefault(x => x.AddressCapability.GetCapabilityEnabled(OrgAddressType.Postal.Code)) ?? oa;
			}
		}
	}

	class CusCarPartyFromIdentificationCode : ICusCarParty
	{
		public CusCarPartyFromIdentificationCode(string code, PartyType party, string country = "", string partyName = "")
		{
			this.identificationCode = code;
			this.partyType = party;
			this.country = country;
			this.partyName = partyName;
		}

		public ZString Address1
		{
			get { return ""; }
		}

		public ZString City
		{
			get { return ""; }
		}

		public ZString Country
		{
			get { return country; }
		}

		public ZString IdentificationCode
		{
			get { return identificationCode; }
		}

		public ZString PartyName
		{
			get { return partyName; }
		}

		public PartyType PartyType
		{
			get { return this.partyType; }
		}

		public ZString Postcode
		{
			get { return ""; }
		}

		public ZString State
		{
			get { return ""; }
		}

		public ZString Street
		{
			get { return ""; }
		}

		public OrgAddress PostalAddress => null;

		readonly string identificationCode;
		readonly string country;
		readonly PartyType partyType;
		readonly string partyName;
	}

	class CusCarPartyFromAsycudaBillAddress : ICusCarParty
	{
		public CusCarPartyFromAsycudaBillAddress(AsycudaBillAddress address)
		{
			this.address = address;
		}

		readonly AsycudaBillAddress address;

		public PartyType PartyType
		{
			get
			{
				switch (address.AsycudaBillAddressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return PartyType.Consignee_CN;
					case AsycudaBillAddress.AddressType.Shipper:
						return PartyType.Consignor_CZ;
					case AsycudaBillAddress.AddressType.NotifyParty:
						return PartyType.NotifyParty1_N1;
					case AsycudaBillAddress.AddressType.FreightForwarder:
						return PartyType.FreightForwarder_FW;
				}
				throw new NotSupportedException("Unknown AsycudaBillAddress type " + address.AsycudaBillAddressType);
			}
		}
		public ZString IdentificationCode
		{
			get
			{
				switch (PartyType)
				{
					case PartyType.Consignee_CN:
						throw new NotSupportedException("This implementation of ICusCarParty - CusCarPartyFromAsycudaBillConsignee - should be utilised only for bills, the messaging for which does not require the IdentificationCode. So don't ask for it.");
					case PartyType.Consignor_CZ:
						throw new NotSupportedException("This implementation of ICusCarParty - CusCarPartyFromAsycudaBillShipper - should be utilised only for bills, the messaging for which does not require the IdentificationCode. So don't ask for it.");
					case PartyType.NotifyParty1_N1:
						throw new NotSupportedException("This implementation of ICusCarParty - CusCarPartyFromAsycudaBillNotifyParty - should be utilised only for bills, the messaging for which does not require the IdentificationCode. So don't ask for it.");
					default:
						throw new NotSupportedException("PartyType not supported");
				}
			}
		}

		public ZString Address1
		{
			get { return address.Address1; }
		}
		public ZString PartyName
		{
			get { return address.CompanyName; }
		}

		public ZString Street
		{
			get { return address.Address2; }
		}

		public ZString City
		{
			get { return address.City; }
		}

		public ZString State
		{
			get { return address.State; }
		}

		public ZString Postcode
		{
			get { return address.Postcode; }
		}

		public ZString Country
		{
			get { return address.RN_NKCountryCode; }
		}

		public OrgAddress PostalAddress
		{
			get { return address.PostalAddress; }
		}
	}
}
