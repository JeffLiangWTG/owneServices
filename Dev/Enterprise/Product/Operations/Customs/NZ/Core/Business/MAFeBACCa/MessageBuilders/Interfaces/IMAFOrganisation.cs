namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	public interface IMAFOrganisation
	{
		ZString OrganisationCode { get; } // O 9 - Customs ClientID
		ZString OrganisationName { get; } // M 50
		ZString AddressLine1 { get; } // M 50
		ZString AddressLine2 { get; } // O 50
		ZString City { get; } // M 40
		ZString PostalCode { get; } // O 7
		ZString Country { get; } // M 2 - ISO Country Code
		ZString Phone { get; }
		ZString Fax { get; }
		ZString Email { get; } // C - Fax or Email at least must be provided when used as a Broker.
		ZString ContactName { get; } // C 50/50 (First/Last) - Required for Broker
		ZString ContactPhone { get; }
		ZString ContactFax { get; }
		ZString ContactEmail { get; }
	}
}
