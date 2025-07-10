using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderReferences : IReferencesParent
	{
		internal ZGuid BuyerPK;
		internal ZString OrderNumber;
		internal ZByte? OrderNumberSplit;
		internal ZString MBOLNumber;
		internal ZString MAWBNumber;
		internal ZString HBOLNumber;
		internal ZString HAWBNumber;
		internal ZString ShippersReference;
		internal ZString InvoiceNumber;
		internal ZString OriginUNLOCO;
		internal ZString DestinationUNLOCO;
	}
}
