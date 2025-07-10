using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsDocketXmlExporter<TDocket> : WhsXmlExporter<TDocket, Xsd.WhsDocket>
		where TDocket : WhsDocket
	{
		protected WhsDocketXmlExporter(WhsValueObjectDataAdapter<TDocket, Xsd.WhsDocket> adapter)
			: base(adapter)
		{
		}
	}
}
