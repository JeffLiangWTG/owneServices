using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract class WhsDocketXmlExportToFileDirector<TDocket> : WhsXmlExportToFileDirector<TDocket, Xsd.WhsDocket>
		where TDocket : WhsDocket
	{
		protected WhsDocketXmlExportToFileDirector(WhsDocketValueObjectDataAdapter<TDocket> adapter)
			: base(adapter)
		{
		}
	}
}
