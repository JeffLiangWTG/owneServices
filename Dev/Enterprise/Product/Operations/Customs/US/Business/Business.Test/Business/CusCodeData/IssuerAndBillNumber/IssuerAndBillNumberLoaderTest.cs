using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(IssuerAndBillNumber.Loader))]
	sealed class IssuerAndBillNumberLoaderTest : LoaderTestCase
	{
		public void TestLoadBillsOfLadingForMessage()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			IssuerAndBillNumber billOfLading = Factory.New<IssuerAndBillNumber>();
			billOfLading.CY_Data = "AAAA12584";
			billOfLading.CY_ParentID = message.PK;
			IssuerAndBillNumber billOfLading1 = Factory.New<IssuerAndBillNumber>();
			billOfLading1.CY_Data = "BBBB12584";
			billOfLading1.CY_ParentID = message.PK;
			AssertEquals("AMS Bills of Lading loaded for message", billOfLading, new IssuerAndBillNumber.Loader(Factory).LoadTop1(message, "AAAA12584"));
			AssertEquals(billOfLading, new IssuerAndBillNumber.Loader(Factory).LoadTop1(message, "AAAA12584"));
			IssuerAndBillNumber[] billsOfLading = new IssuerAndBillNumber.Loader(Factory).Load(message);
			AssertEquals(2, billsOfLading.Length);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new IssuerAndBillNumber.Loader(Factory);
	}
}
