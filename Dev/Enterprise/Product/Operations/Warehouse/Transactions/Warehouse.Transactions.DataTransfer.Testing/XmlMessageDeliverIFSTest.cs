using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	class XmlMessageDeliverIFSForTesting : XmlMessageDeliverIFS
	{
		public XmlMessageDeliverIFSForTesting(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action)
			: base(modes, bizObjToDeliver, dataAdapter, action)
		{
		}

		public new Stream GetXmlStreamToDeliver(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, INotifications context)
		{
			return base.GetXmlStreamToDeliver(bizObj, dataAdapter, context);
		}
	}

	sealed class XmlMessageDeilverIFSTest : TestCaseWithFactory
	{
		public void TestGetXmlStreamToDeliver()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var expected = resourceRetriever.GetString("Enterprise.Warehouse.Transactions.DataTransfer.Testing.TestFiles.XmlDeliverIFSTest.xml");
				var actual = GetActualXmlString();

				AssertXMLEquals(expected, actual);
			}
		}

		string GetActualXmlString()
		{
			var year = ZDateTime.Now.Year;
			var result = string.Empty;
			var data = new TestDataForIFS(Factory);
			data.CreateIfsOrders();
			data.Order1.WD_RequiredDate = new ZDateTimeOffset(year, 5, 6);

			var msgDeliver = new XmlMessageDeliverIFSForTesting(null, null, null, null);

			using (var msgStream = msgDeliver.GetXmlStreamToDeliver(data.Order1, new WhsOrderCartageValueObjectDataAdapterIFS(), null))
			using (var reader = new StreamReader(msgStream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}
	}
}
