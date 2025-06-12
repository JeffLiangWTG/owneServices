using System.Xml;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class MHGatewayResponseHandlerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHandle_Success()
		{
			var responseXml =
@"<submitMessageResponse xmlns=""http://cargowise.com/ehub/products/sgcustoms/MHAccessGateway"" xmlns:ns2=""http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09"">
	<ns2:response>
		<ns2:status>success</ns2:status>
		<ns2:state>submit</ns2:state>
		<ns2:errorMessage/>
		<ns2:messages />
	</ns2:response>
</submitMessageResponse>";

			bool retry;
			string error;
			var succeed = MHGatewayResponseHandler.Handle(responseXml, "HYEDAUAYA", "AccountID1", out retry, out error);

			Assert.IsTrue(succeed);
			Assert.IsFalse(retry);
			Assert.AreEqual(string.Empty, error);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHandle_ServerConnectionError()
		{
			var responseXml =
@"<submitMessageResponse xmlns=""http://cargowise.com/ehub/products/sgcustoms/MHAccessGateway"" xmlns:ns2=""http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09"">
	<ns2:response>
		<ns2:status>fail</ns2:status>
		<ns2:state>login</ns2:state>
		<ns2:errorCode>10</ns2:errorCode>
		<ns2:errorMessage>Server connection error. Please check your internet connection</ns2:errorMessage>
		<ns2:messages />
	</ns2:response>
</submitMessageResponse>";

			bool retry;
			string error;
			var succeed = MHGatewayResponseHandler.Handle(responseXml, "HYEDAUAYA", "AccountID1", out retry, out error);

			Assert.IsFalse(succeed);
			Assert.IsTrue(retry);
			Assert.AreEqual("Status: fail; State: login; ErrorCode: 10; ErrorMessage: Server connection error. Please check your internet connection", error);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHandle_OtherError()
		{
			var responseXml =
@"<submitMessageResponse xmlns=""http://cargowise.com/ehub/products/sgcustoms/MHAccessGateway"" xmlns:ns2=""http://cargowise.com/ehub/products/sgcustoms/mhaccess/2017/09"">
	<ns2:response>
		<ns2:status>fail</ns2:status>
		<ns2:state>submit</ns2:state>
		<ns2:errorCode>103</ns2:errorCode>
		<ns2:errorMessage>Edi Error Msg:Input File Is Empty</ns2:errorMessage>
		<ns2:messages />
	</ns2:response>
</submitMessageResponse>";

			bool retry;
			string error;
			var succeed = MHGatewayResponseHandler.Handle(responseXml, "HYEDAUAYA", "AccountID1", out retry, out error);

			Assert.IsFalse(succeed);
			Assert.IsFalse(retry);
			Assert.AreEqual("Status: fail; State: submit; ErrorCode: 103; ErrorMessage: Edi Error Msg:Input File Is Empty", error);
		}
	}
}
