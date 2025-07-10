using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VoucherOfCorrectionValueBefore))]
	sealed class VoucherOfCorrectionValueBeforeTest : Customs.Business.Testing.CusCodeDataTest<VoucherOfCorrectionValueBefore>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var message = factory.New<CUSDECEDIMessage>();
			return message.VoucherOfCorrectionValueBefores.AddNew();
		}
	}
}
