using System;
using System.Collections;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Microsoft.XLANGs.Core;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class ExceptionHandlerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHandleEdifactLoopbackException()
		{
			var error = @"An error occurred while processing the message, refer to the details section for more information 
Message ID: {8B4A0104-E9D0-4A1E-8444-D1528727903C}
Instance ID: {A5063359-725F-4BAF-8B45-D9B913112154}
Error Description: There was a failure executing the response(receive) pipeline: ""Microsoft.BizTalk.DefaultPipelines.PassThruReceive, Microsoft.BizTalk.DefaultPipelines, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"" Source: ""Pipeline "" Send Port: ""SGC_MHAccess_AssembleLoopback"" URI: ""loopback://sgc_mhaccess_edifactassembleloopback/"" Reason: Error: 1 (Miscellaneous error)
	505: The element 'IDT2' has an invalid structure

Error: 2 (Miscellaneous error)
	505: The element 'IDT' has an invalid structure 

   at Microsoft.BizTalk.XLANGs.BTXEngine.BTXPortBase.VerifyTransport(Envelope env, Int32 operationId, Context ctx)
   at Microsoft.XLANGs.Core.Subscription.Receive(Segment s, Context ctx, Envelope& env, Boolean topOnly)
   at Microsoft.XLANGs.Core.PortBase.GetMessageId(Subscription subscription, Segment currentSegment, Context cxt, Envelope& env, CachedObject location)
   at CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Send.Orchestrations.eHub2MHAccess.eHub2MHAccessOrchestration.segment2(StopConditions stopOn)
   at Microsoft.XLANGs.Core.SegmentScheduler.RunASegment(Segment s, StopConditions stopCond, Exception& exp)";

			var errorMessageTrackingID = "8B4A0104-E9D0-4A1E-8444-D1528727903C";
			var outboxMessageTrackingIDArrayList = new ArrayList();
			outboxMessageTrackingIDArrayList.Add(errorMessageTrackingID);

			var stubXlangMessage = MockRepository.GenerateStub<XLANGMessage>();
			var stubSoapException = MockRepository.GenerateStub<SoapException>();
			var mockXlangSoapException = MockRepository.GenerateMock<XlangSoapException>(stubXlangMessage, stubSoapException);
			mockXlangSoapException.Expect(_ => _.ToString()).Return(error);

			var isCalled = false;
			ExceptionHandler.FailMessages = (string senderID, string recipientID, string error1, ArrayList errorMessageTrackingIDArrayList) => { isCalled = true; };

			ExceptionHandler.HandleEdifactLoopbackException(mockXlangSoapException, "HYEDAUAYA", "SGCustomsTest", errorMessageTrackingID, ref outboxMessageTrackingIDArrayList);

			mockXlangSoapException.VerifyAllExpectations();
			Assert.IsTrue(isCalled);
			Assert.AreEqual(0, outboxMessageTrackingIDArrayList.Count);
		}
	}
}
