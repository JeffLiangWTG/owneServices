using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing
{
	sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestFallbackProcedureCheckBox()
		{
			var fallbackProcedureCheckBox = control.FallbackProcedureCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>(fallbackProcedureCheckBox);
				AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.IsFallbackProcedure)}", fallbackProcedureCheckBox.BindTo);
			});
		}

		public void TestFallbackUserControl()
		{
			var fallbackUserControl = control.FallbackUserControl;
			AssertType<DeclarationDetailsFallbackUserControl>(fallbackUserControl);
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
