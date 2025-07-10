using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(CIMEDIMessageDependentCollection))]
	sealed class CIMEDIMessageDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetLatestTransmittedMessage()
		{
			var collection = new CIMEDIMessageDependentCollection(Factory.New<ExportAWBHeader>());
			collection.AddNew().EM_ReceiveTransmit = CIMEDIMessage.Direction.Receive;
			Thread.Sleep(100);
			Factory.Save();
			collection.AddNew().EM_ReceiveTransmit = CIMEDIMessage.Direction.Transmit;
			Thread.Sleep(100);
			Factory.Save();
			collection.AddNew().EM_ReceiveTransmit = CIMEDIMessage.Direction.Receive;
			Thread.Sleep(100);
			Factory.Save();
			var lastTransmitMessage = collection.AddNew();
			lastTransmitMessage.EM_ReceiveTransmit = CIMEDIMessage.Direction.Transmit;
			Thread.Sleep(100);
			Factory.Save();
			collection.AddNew().EM_ReceiveTransmit = CIMEDIMessage.Direction.Receive;
			Thread.Sleep(100);
			Factory.Save();

			AssertEquals("collection.GetLatestTransmittedMessage()", lastTransmitMessage, collection.GetLatestTransmittedMessage());
		}

		public void TestReadOnly()
		{
			var collection = new CIMEDIMessageDependentCollection(Factory.New<ExportAWBHeader>());
			var message = collection.AddNew();
			AssertEquals("message.ReadOnly", true, message.ReadOnly);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CIMEDIMessageDependentCollection(Factory.New<ExportAWBHeader>());
		}

		#endregion
	}
}
