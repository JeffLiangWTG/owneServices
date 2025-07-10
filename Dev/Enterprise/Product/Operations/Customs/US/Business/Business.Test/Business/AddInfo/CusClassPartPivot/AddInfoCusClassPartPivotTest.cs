using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoCusClassPartPivot))]
	sealed class AddInfoCusClassPartPivotTest : AddInfoAbstractTest
	{
		public override void TestIsExport()
		{
			var addInfo = (AddInfoCusClassPartPivot)GetNewBusinessObject();
			Assert(!addInfo.IsExport);
			var pivot = addInfo.Parent;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert(addInfo.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			Assert(addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			var addInfo = (AddInfoCusClassPartPivot)GetNewBusinessObject();
			Assert(!addInfo.IsDrawback);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoCusClassPartPivotLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoCusClassPartPivotValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			return new AddInfoCusClassPartPivot(pivot.CI_AddInfoInfo);
		}
	}
}
