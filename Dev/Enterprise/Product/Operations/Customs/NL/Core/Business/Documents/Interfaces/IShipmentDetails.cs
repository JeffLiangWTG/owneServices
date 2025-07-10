using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IShipmentDetails
{
	ZString ID { get; }
	IAddressDocumentInformation ShipperSupplier { get; }
	IAddressDocumentInformation Consignee { get; }
	ZInt Packages { get; }
	ZDecimal Weight { get; }
	ZString Origin { get; }
	ZString Destination { get; }
	ZString Master { get; }
	ZString House { get; }
	ZString Containers { get; }
	ZString Mrn { get; }
	IDeclarationGoodsLocation GoodsLocation { get; }
}
