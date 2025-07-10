using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class PartyDocWrapper : NonPersistentBusinessObject
	{
		public PartyDocWrapper(ISimplifiedEntryOrganisationDetails party, bool printSSNOnDocument = false)
		{
			this.party = party;
			this.printSocialSecurityNumberOnDocument = printSSNOnDocument;
		}
		readonly ISimplifiedEntryOrganisationDetails party;
		readonly ZBool printSocialSecurityNumberOnDocument;

		public ZString IsManufacturer
		{
			get { return party != null ? party.EntityCode == EntityCodeList.Codes.ManufacturerSupplier ? "X" : "" : ""; }
		}

		public ZString IsConsignee
		{
			get { return party != null ? party.EntityCode == EntityCodeList.Codes.Consignee ? "X" : "" : ""; }
		}

		public ZString IsBuyingParty
		{
			get { return party != null ? party.EntityCode == EntityCodeList.Codes.BuyingParty ? "X" : "" : ""; }
		}

		public ZString IsSellingParty
		{
			get { return party != null ? party.EntityCode == EntityCodeList.Codes.SellingParty ? "X" : "" : ""; }
		}

		public ZString PartyIdentifier
		{
			get { return party.EntityIdentifier; }
		}

		public ZString PartyIdentifierForDocument
		{
			get
			{
				var result = PartyIdentifier;
				if (SocialSecurityNumberValidator.IsValidSSN(result) && !printSocialSecurityNumberOnDocument)
				{
					result = ZString.Empty;
				}
				return result;
			}
		}

		public ZString IsIRS
		{
			get { return party != null ? party.EntityIdentifierQualifier == EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber ? "X" : "" : ""; }
		}

		public ZString IsSSN
		{
			get { return party != null && printSocialSecurityNumberOnDocument ? party.EntityIdentifierQualifier == EntityIdentifierQualifierList.Codes.SocialSecurityNumber ? "X" : "" : ""; }
		}

		public ZString IsCBP
		{
			get { return party != null ? party.EntityIdentifierQualifier == EntityIdentifierQualifierList.Codes.CBPAssignedNumber ? "X" : "" : ""; }
		}

		public ZString Name
		{
			get { return party != null ? party.CompanyName : ZString.Empty; }
		}

		public ZString AddressLine1
		{
			get { return party != null ? party.AddressLine1 : ZString.Empty; }
		}

		public ZString AddressLine2 => party != null ? party.AddressLine2 : ZString.Empty;

		public ZString CityStatePostCodeCountry => CityStatePostCodeCountryCalculated;

		ZString CityStatePostCodeCountryCalculated
		{
			get
			{
				var result = ZString.Empty;

				if (party != null)
				{
					result = party.City + "   " +
							 party.State + "   " +
							 party.PostCode + "   " +
							 party.Country;
					result = result.TrimStart();
				}

				return result;
			}
		}

		public ZString City => party != null ? party.City : ZString.Empty;

		public ZString State => party != null ? party.State : ZString.Empty;

		public ZString PostCode => party != null ? party.PostCode : ZString.Empty;
	}
}
