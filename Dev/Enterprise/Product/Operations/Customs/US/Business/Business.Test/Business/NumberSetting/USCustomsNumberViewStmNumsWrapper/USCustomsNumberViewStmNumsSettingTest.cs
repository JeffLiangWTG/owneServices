using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCustomsNumberViewStmNumsSetting))]
	sealed class USCustomsNumberViewStmNumsSettingTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestSettings()
		{
			var codes = new[] { NumberRangeTypeList.Codes.CustomsEntry };
			foreach (ICodeDescription pair in new NumberRangeTypeList())
			{
				var currentCode = pair.Code;
				CombineAssertions(() =>
				{
					var setting = new USCustomsNumberViewStmNumsSetting(Company, currentCode);
					AssertEquals($"{currentCode}: GetThresholdRunOutWarning()", 100L, setting.GetThresholdRunOutWarning());
					var stmNums = Factory.New<CustomsNumberViewStmNums>();
					AssertEquals($"{currentCode}: SN_Type", ZString.Empty, stmNums.SN_Type);
					setting.DefaultDataOnSettingOwner(stmNums);
					AssertEquals($"{currentCode}, defaultedDataOnSettingsOwner: SN_Type", currentCode, stmNums.SN_Type);
					if (codes.Contains(currentCode))
					{
						AssertEquals($"{currentCode} is contained: RequiredDigit()", 7, setting.RequiredDigit());
						AssertEquals($"{currentCode} is contained: DefaultTypeRangeMax()", 9999999L, setting.DefaultTypeRangeMax());
					}
					else
					{
						AssertEquals($"{currentCode} is not contained: RequiredDigit()", 8, setting.RequiredDigit());
						AssertEquals($"{currentCode} is not contained: DefaultTypeRangeMax()", 99999999L, setting.DefaultTypeRangeMax());
					}
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject() => new USCustomsNumberViewStmNumsSetting(Company, NumberRangeTypeList.Codes.CustomsEntry);

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
