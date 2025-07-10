using Enterprise.Freight.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ConsolValueObjectDataAdapterForTest<TBusinessObject> : ConsolValueObjectDataAdapter<TBusinessObject, CommonShipment, Xsd.Consol>
			where TBusinessObject : CommonConsol
	{
		public ConsolValueObjectDataAdapterForTest()
		{
		}
	}
}
