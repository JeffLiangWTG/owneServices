using System.Drawing;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class JobDeclarationColorHelperTest : TestCaseWithDummy
	{
		public void TestGetColorForEntryStatusShouldReturnExpectedColor()
		{
			AssertState("REG", JobDeclarationColorHelper.ColorConstants.Registered);
			AssertState("QUE", JobDeclarationColorHelper.ColorConstants.Question);
			AssertState("SDO", JobDeclarationColorHelper.ColorConstants.SupportingDocument);
			AssertState("WRN", JobDeclarationColorHelper.ColorConstants.Warning);
			AssertState("RANDOM_VALUE", JobDeclarationColorHelper.ColorConstants.Undefined);
		}

		static void AssertState(string entryStatus, Color expectedColor)
		{
			var color = JobDeclarationColorHelper.GetColorForEntryStatus(entryStatus);
			AssertEquals($"Case: {entryStatus}", expectedColor, color);
		}
	}
}
