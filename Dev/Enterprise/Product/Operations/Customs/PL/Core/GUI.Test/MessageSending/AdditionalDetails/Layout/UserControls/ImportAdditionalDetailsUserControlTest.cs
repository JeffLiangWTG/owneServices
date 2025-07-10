using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ImportAdditionalDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new ImportAdditionalDetailsUserControl())
		{
			AssertEquals(typeof(BaseMessageSendingObject), control.DataSourceType);
		}
	}

	public void TestControls()
	{
		var expectedControlsAmount = 0;
		using (var control = new ImportAdditionalDetailsUserControl())
		{
			AssertEquals($"ImportAdditionalDetailsUserControl should countain only {expectedControlsAmount} controls", expectedControlsAmount, control.Controls.Count);
		}
	}
}
