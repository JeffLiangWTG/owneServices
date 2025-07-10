using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public abstract class WhsDocketXmlExportToFileDirectorTest<TDocket> : WhsXmlExportToFileDirectorTest<TDocket, Xsd.WhsDocket>
		where TDocket : WhsDocket
	{
	}
}
