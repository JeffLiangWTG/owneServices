using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISCBPRequest))]
	sealed class DISCBPRequestTest : XmlSerializableNonPersistentBusinessObjectTest<DISCBPRequest>
	{
		public void TestDefaultCBPRequests()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var defaultRequestMock = new Mock<IDISCBPRequestDefault>();
			defaultRequestMock.Setup(m => m.ID).Returns(new ZString("423987432"));
			defaultRequestMock.Setup(m => m.RequestDate).Returns(ZDateTime.BrettsBirthday);
			defaultRequestMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultCBPRequests).Returns(new IDISCBPRequestDefault[] { defaultRequestMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var request = disDocument.CBPRequest;
			var list = request.DefaultCBPRequests;
			AssertEquals(3, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			usDISHost.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper((IUSDISHost)jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			return new DISCBPRequest(disDocument);
		}
	}
}
