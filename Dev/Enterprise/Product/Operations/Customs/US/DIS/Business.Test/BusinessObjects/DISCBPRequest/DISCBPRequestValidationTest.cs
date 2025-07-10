using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISCBPRequestValidationTest : TestCaseWithFactory
	{
		public void TestCheckID()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			defaultValues.Setup(m => m.DefaultCBPRequests).Returns(System.Array.Empty<IDISCBPRequestDefault>());
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var request = disDocument.CBPRequest;
			request.ID = ZString.Empty;
			AssertHasMessageErrorContaining(request.IDInfo, MandatoryValidation.YouHaveNotEntered);
			request.ID = "~~~";
			AssertNoMessageErrorContaining(request.IDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(request.IDInfo, ListValidation.InvalidCodeMessageError);
			request.ID = MiscCBPRequestIDList.Codes.Unsolicited;
			AssertNoMessageErrorContaining(request.IDInfo, ListValidation.InvalidCodeMessageError);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}

		public void TestCheckType()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var request = disDocument.CBPRequest;
			request.Type = "~~~";
			AssertHasMessageErrorContaining(request.TypeInfo, ListValidation.InvalidCodeMessageError);
			request.Type = CBPRequestTypeList.Codes.ACEActionNumber;
			AssertNoMessageErrorContaining(request.TypeInfo, ListValidation.InvalidCodeMessageError);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
		}
	}
}
