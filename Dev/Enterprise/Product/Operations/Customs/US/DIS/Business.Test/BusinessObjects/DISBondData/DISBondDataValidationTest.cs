using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISBondDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckDefaultBondCode()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var bondDataListMock = new Mock<IDISBondDataDefault>();
			bondDataListMock.Setup(m => m.BondName).Returns(BondNameType.Single);
			bondDataListMock.Setup(m => m.BondNumber).Returns(new ZString("423987432"));
			bondDataListMock.Setup(m => m.SuretyCode).Returns(new ZString("345"));
			bondDataListMock.Setup(m => m.BondAmount).Returns(new ZDecimal(250.20m));
			bondDataListMock.Setup(m => m.Code).Returns(new ZString("1"));
			bondDataListMock.Setup(m => m.Description).Returns(new ZString("blah"));
			defaultValues.Setup(m => m.DefaultBondData).Returns(new IDISBondDataDefault[] { bondDataListMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var bondData = disDocument.BondData;
			bondData.DefaultBondCode = "~";
			AssertHasMessageErrorContaining(bondData.DefaultBondCodeInfo, ListValidation.InvalidCodeMessageError);
			bondData.DefaultBondCode = "1";
			AssertNoMessageErrorContaining(bondData.DefaultBondCodeInfo, ListValidation.InvalidCodeMessageError);

			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			bondDataListMock.VerifyAll();
		}

		public void TestCheckBondName()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var bondData = disDocument.BondData;
			bondData.BondName = "!";
			AssertHasMessageErrorContaining(bondData.BondNameInfo, ListValidation.InvalidCodeMessageError);
			bondData.BondName = BondNameTypeList.Codes.ISFBond;
			AssertNoMessageErrorContaining(bondData.BondNameInfo, ListValidation.InvalidCodeMessageError);
			usDISHost.VerifyAll();
		}
	}
}
