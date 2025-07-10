using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISCBPRequestWrapperTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestInterface()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.DefaultCBPRequests).Returns(System.Array.Empty<IDISCBPRequestDefault>());
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var request = disDocument.CBPRequest;
			request.ID = "324809";
			request.RequestDate = ZDateTime.BrettsBirthday;
			request.Type = "ABC";
			var wrapper = (IDISCBPRequest)new DISCBPRequestWrapper(request);
			AssertEquals("324809", wrapper.ID);
			AssertEquals(ZDateTime.BrettsBirthday, wrapper.RequestDate);
			AssertEquals("ABC", wrapper.Type);
			request.ID = MiscCBPRequestIDList.Codes.Unknown;
			AssertEquals(MiscCBPRequestIDList.Descriptions.Unknown, wrapper.ID);
			request.ID = MiscCBPRequestIDList.Codes.Unsolicited;
			AssertEquals(MiscCBPRequestIDList.Descriptions.Unsolicited, wrapper.ID);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}
	}
}
