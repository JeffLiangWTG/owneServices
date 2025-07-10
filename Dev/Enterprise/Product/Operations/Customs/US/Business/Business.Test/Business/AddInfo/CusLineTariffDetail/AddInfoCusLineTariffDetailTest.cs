using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoCusLineTariffDetail))]
	sealed class AddInfoCusLineTariffDetailTest : AddInfoAbstractTest
	{
		public override void TestIsExport()
		{
			var addInfo = new AddInfoCusLineTariffDetail(Factory.New<CusLineTariffDetail>().BZ_NAddInfoInfo);
			AssertEquals("IsExport", false, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			var addInfo = new AddInfoCusLineTariffDetail(Factory.New<CusLineTariffDetail>().BZ_NAddInfoInfo);
			Assert("IsDrawback", !addInfo.IsDrawback);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoCusLineTariffDetailLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoCusLineTariffDetailValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var tariffDetail = Factory.New<CusLineTariffDetail>();
			return new AddInfoCusLineTariffDetail(tariffDetail.BZ_NAddInfoInfo);
		}
	}
}
