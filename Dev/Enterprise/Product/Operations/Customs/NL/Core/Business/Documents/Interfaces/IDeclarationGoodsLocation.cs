using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IDeclarationGoodsLocation
{
	ZString TypeCode { get; }

	IAddressDocumentInformation Address { get; }

	ZString IdentificationTypeCode { get; }
}
