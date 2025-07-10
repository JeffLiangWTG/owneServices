using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public abstract class WhsDocketXmlExportToEmailDirectorTest<TDocket> : WhsXmlExportToEmailDirectorTest<TDocket, Xsd.WhsDocket>
	where TDocket : WhsDocket
{
}
