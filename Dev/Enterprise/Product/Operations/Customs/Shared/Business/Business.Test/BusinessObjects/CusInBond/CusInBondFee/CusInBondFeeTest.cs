using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class CusInBondFeeTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			AssertType<CusInBondFeeTypeDecider>(CusInBondFee.TypeDecider);
		}
	}

	internal class CusInBondFeeForTest : CusInBondFee
	{
		public CusInBondFeeForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
