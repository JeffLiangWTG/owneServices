using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ProductCarInfoUserControlTests : TestCaseWithFactory
	{
		public void TestPropertiesofCalcEdits()
		{
			AssertPropertiesofCalcEdit("ModelYearCalcEdit", 9999m);
			AssertPropertiesofCalcEdit("NumberOfDoorsCalcEdit", 9m);
			AssertPropertiesofCalcEdit("NumberOfSeatsCalcEdit", 99m);
			AssertPropertiesofCalcEdit("NumberOfCylindersCalcEdit", 99m);
			AssertPropertiesofCalcEdit("NumberOfGearCalcEdit", 99m);
		}

		void AssertPropertiesofCalcEdit(string controlName, decimal maxValue)
		{
			using (var control = new ProductCarInfoUserControl())
			{
				var calcEdit = control.FindSingleOrDefault<ZCalcEdit>(c => c.Name == controlName);
				Assert(calcEdit.ShowEmptyStringForEmptyValue);
				Assert(!calcEdit.ShowGroupSeparators);
				AssertEquals(maxValue, calcEdit.MaxValue);
			}
		}
	}
}
