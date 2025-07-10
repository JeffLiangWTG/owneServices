using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGCustomsNumberViewStmNumsSetting))]
	class SGCustomsNumberViewStmNumsSettingTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestSettings()
		{
			CombineAssertions(() =>
			{
				var setting = new SGCustomsNumberViewStmNumsSetting(Company, NumberRangeTypeList.Codes.SingaporeMessageNumber);
				AssertEquals("DefaultTypeRangeMax()", 9999L, setting.DefaultTypeRangeMax());
				AssertEquals("CanRollover()", true, setting.CanRollover());
				AssertEquals("AllowDuplicate()", false, setting.AllowDuplicate());
			}

			);
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SGCustomsNumberViewStmNumsSetting(Company, NumberRangeTypeList.Codes.SingaporeMessageNumber);
		}
	}
}
