using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class GovernmentAgencyGoodsItemManufacturer : IPartyDetails
	{
		public GovernmentAgencyGoodsItemManufacturer(TWJobDocAddress address)
		{
			this.twJobDocAddress = address;
		}

		public ZString ID => twJobDocAddress?.IDCode ?? ZString.Empty;

		public ZString Name
		{
			get
			{
				var name = ZString.Empty;
				if (twJobDocAddress != null)
				{
					var address = twJobDocAddress.Address;
					if (twJobDocAddress.E2_AddressOverride)
					{
						name = twJobDocAddress.E2_CompanyName;
					}
					else if (address != null)
					{
						if (address.OA_Language == Core.SharedConstants.Languages.English)
						{
							name = address.OA_CompanyNameOverride;
						}
						else
						{
							name = address.GetTranslatedAddressInSpecificLanguage(Core.SharedConstants.Languages.English)?.OTA_CompanyName ?? ZString.Empty;
						}
					}
				}
				return name;
			}
		}

		public ZString ChineseName => ZString.Empty;

		public ZString TypeCode => ID.IsEmpty ? ZString.Empty : new ZString(PartyIdentifierCodeList.Codes._58);

		public ZString CustomsControlID => ZString.Empty;

		public ZString PaymentOnAccountBusinessID => ZString.Empty;

		public ZString RoleCode => ZString.Empty;

		public ZString SubBoxID => ZString.Empty;

		public IAddress Address => null;

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public IEnumerable<ICommunication> Communications => null;

		public ZString ContactName => ZString.Empty;

		public ZString MainManufacturer => null;

		public ZString UndertakeCode => null;

		public ZString OwnerName => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

		readonly TWJobDocAddress twJobDocAddress;
	}
}
