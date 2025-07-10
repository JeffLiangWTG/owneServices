#if DEBUG
using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.Testing
{
	// This fails in unit tests if we do not run SGen etc., which is not done on the test project.
	// See WI00655623 - Partial Test Extraction - Enterprise.MasterFiles.Business -ValueObject,Warehouse.
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	[XmlType(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRoot(ElementName = "TestElement", Namespace = "http://www.edi.com.au/EnterpriseService/")]
	public class TestValueObject : ValueObject
	{
		public string Value;
	}
}
#endif
