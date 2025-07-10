using Enterprise.DataTransfer.GUI.Testing;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class OrderXMLDataTransferDirectorTest : XmlDataTransferDirectorTest
	{
		public void TestGetOrderXMLDataTransferDirectorSerializer()
		{
			var director = new OrderXMLDataTransferDirector(new OrderValueObjectDataAdapter(), true);
			AssertEquals("Incorrect serialiser", true, director.Serializer is OrderXMLValueObjectSerializer);
		}
	}
}
