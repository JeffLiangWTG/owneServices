using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VoucherOfCorrectionValueAfter))]
	sealed class VoucherOfCorrectionValueAfterTest : Customs.Business.Testing.CusCodeDataTest<VoucherOfCorrectionValueAfter>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var message = factory.New<CUSDECEDIMessage>();
			return message.VoucherOfCorrectionValueAfters.AddNew();
		}
	}
}
