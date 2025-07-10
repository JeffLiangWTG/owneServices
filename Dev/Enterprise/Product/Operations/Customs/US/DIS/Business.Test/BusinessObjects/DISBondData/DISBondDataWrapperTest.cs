using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISBondDataWrapperTest : TestCaseWithFactory
	{
		public void TestInterface()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var bondData = disDocument.BondData;
			bondData.BondName = BondNameTypeList.Codes.SingleBond;
			bondData.BondNumber = "342789";
			bondData.BondType = "9";
			bondData.SuretyCode = "342";
			bondData.AgentIDNumber = "3240983";
			bondData.BondAmount = 17890.50m;
			var bondDataWrapper = (IDISBondData)new DISBondDataWrapper(bondData, "SV9");
			AssertEquals(BondNameType.Single, bondDataWrapper.BondName);
			AssertEquals("342789", bondDataWrapper.BondNumber);
			AssertEquals("9", bondDataWrapper.BondType);
			AssertEquals("342", bondDataWrapper.SuretyCode);
			AssertEquals("3240983", bondDataWrapper.AgentIDNumber);
			AssertEquals(17890.50m, bondDataWrapper.BondAmount);
			usDISHost.VerifyAll();
		}
	}
}
