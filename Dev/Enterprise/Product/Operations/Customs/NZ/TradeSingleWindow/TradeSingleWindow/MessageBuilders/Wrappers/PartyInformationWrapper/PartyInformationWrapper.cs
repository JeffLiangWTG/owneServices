using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class PartyInformationWrapper : IPartyInformation
	{
		public PartyInformationWrapper(ZString name, ZString city, ZString countryCode, ZString countryRegion, ZString address1, ZString address2, ZString postCode, ZString phone, OrgHeader organisation)
		{
			Name = name;
			City = city;
			CountryCode = countryCode;
			CountryRegion = countryRegion;
			this.address1 = address1;
			this.address2 = address2;
			postalCode = postCode;
			this.phone = phone;
			this.organisation = organisation;
		}

		readonly ZString address1;
		readonly ZString address2;
		readonly ZString postalCode;
		readonly ZString phone;
		readonly OrgHeader organisation;

		public ZString Name { get; }

		public ZString City { get; }

		public ZString CountryCode { get; }

		public ZString CountryRegion { get; }

		public ZString Address => OrgHeaderWrapper.GetStreetAddressDetails(address1, address2);

		public ZString PostCode => AdjustedPostCode;

		public ZString CustomsClientCode => organisation?.GetCustomsClientCode() ?? ZString.Empty;

		public IEnumerable<ICommunication> Communications
		{
			get
			{
				var communications = organisation?.GetCommunications();
				communications ??= phone.IsEmpty ? Enumerable.Empty<ICommunication>() : new Communication[] { new Communication(phone, CommunicationTypeList.Codes.TE) };
				return communications;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return organisation == null
					&& Name.IsEmpty
					&& City.IsEmpty
					&& CountryCode.IsEmpty
					&& CountryRegion.IsEmpty
					&& address1.IsEmpty
					&& address2.IsEmpty
					&& postalCode.IsEmpty
					&& phone.IsEmpty;
			}
		}

		ZString AdjustedPostCode
		{
			get
			{
				var result = postalCode;
				if (result.Length > OrgHeaderWrapper.Schema.PostCodeMaxLength)
				{
					result = result.KeepAlphanumericCharacters();
				}

				return result.Left(OrgHeaderWrapper.Schema.PostCodeMaxLength);
			}
		}
	}
}
