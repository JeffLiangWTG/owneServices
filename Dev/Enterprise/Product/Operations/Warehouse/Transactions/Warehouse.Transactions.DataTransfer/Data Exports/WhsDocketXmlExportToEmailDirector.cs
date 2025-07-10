using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsDocketXmlExportToEmailDirector<TDocket> : WhsXmlExportToEmailDirector<TDocket, Xsd.WhsDocket>
		where TDocket : WhsDocket
	{
		protected WhsDocketXmlExportToEmailDirector(WhsDocketValueObjectDataAdapter<TDocket> adapter)
			: base(adapter)
		{
		}
	}
}
