using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoCusClassification))]
	sealed class AddInfoCusClassificationTest : AddInfoAbstractTest
	{
		public override void TestIsExport()
		{
			AddInfoCusClassification addInfo = new AddInfoCusClassification(Factory.New<CusClassification>().CC_AddInfoInfo);
			AssertEquals("IsExport", false, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			AddInfoCusClassification addInfo = new AddInfoCusClassification(Factory.New<CusClassification>().CC_AddInfoInfo);
			Assert("IsDrawback", !addInfo.IsDrawback);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoCusClassificationLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoCusClassificationValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var classification = Factory.New<CusClassification>();
			return new AddInfoCusClassification(classification.CC_AddInfoInfo);
		}
	}
}
