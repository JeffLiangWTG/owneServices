using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VoucherOfCorrectionValueAfterCollection<ZAMessage>))]
	sealed class VoucherOfCorrectionValueAfterCollectionTest : CusCodeDataCollectionTest<VoucherOfCorrectionValueAfter>
	{
		protected override CusCodeDataCollection<VoucherOfCorrectionValueAfter> GetCusCodeDataCollection() => Factory.New<ZAMessage>().VoucherOfCorrectionValueAfters;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<VoucherOfCorrectionValueAfter>();
	}
}
