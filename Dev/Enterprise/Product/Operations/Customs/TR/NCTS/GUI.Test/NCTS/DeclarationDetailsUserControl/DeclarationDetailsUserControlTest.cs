using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestLrnTextBox()
		{
			var lrnTextBox = control.LrnTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(lrnTextBox);
				AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.LrnRegistrationNumber)}", lrnTextBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			control = new DeclarationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DeclarationDetailsUserControl control;
	}
}
