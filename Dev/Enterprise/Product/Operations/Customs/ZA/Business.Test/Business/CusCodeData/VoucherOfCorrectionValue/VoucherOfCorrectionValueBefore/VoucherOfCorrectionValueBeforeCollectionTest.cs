using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VoucherOfCorrectionValueBeforeCollection<CusEntryHeader>))]
	sealed class VoucherOfCorrectionValueBeforeCollectionTest : CusCodeDataCollectionTest<VoucherOfCorrectionValueBefore>
	{
		protected override CusCodeDataCollection<VoucherOfCorrectionValueBefore> GetCusCodeDataCollection() => Factory.New<CusEntryHeader>().VoucherOfCorrectionValueBefores;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<VoucherOfCorrectionValueBefore>();
	}
}
