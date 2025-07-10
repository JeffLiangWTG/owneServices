using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IAddressDocumentInformation
{
	ZString Name { get; }
	ZString Address { get; }
	ZString PostCode { get; }
	ZString City { get; }
	ZString CountryCode { get; }
	ZString EORINumber { get; }
	ZString VATNumber { get; }
}
