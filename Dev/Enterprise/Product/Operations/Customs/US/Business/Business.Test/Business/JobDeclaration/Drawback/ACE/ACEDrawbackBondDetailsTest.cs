using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEDrawbackBondDetailsTest : TestCaseWithFactory
	{
		public void TestBondDetails()
		{
			var bondDetails = new ACEDrawbackBondDetails("8", "B", "001", 100m, "12345");
			var bondInfo = (IACEDrawbackBondInfo)bondDetails;
			AssertEquals("8", bondInfo.BondType);
			AssertEquals("B", bondInfo.BondDesignationTypeCode);
			AssertEquals("001", bondInfo.SuretyCode);
			AssertEquals(0m, bondInfo.BondAmount);
			AssertEquals(ZString.Empty, bondInfo.ProducerAccountNumber);

			bondDetails = new ACEDrawbackBondDetails("9", "B", "001", 100m, "12345");
			bondInfo = bondDetails;
			AssertEquals("9", bondInfo.BondType);
			AssertEquals("B", bondInfo.BondDesignationTypeCode);
			AssertEquals("001", bondInfo.SuretyCode);
			AssertEquals(100m, bondInfo.BondAmount);
			AssertEquals("12345", bondInfo.ProducerAccountNumber);
		}
	}
}
