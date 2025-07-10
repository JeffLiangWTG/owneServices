using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing
{
	sealed class DeclarationDetailsFallbackUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestFallbackReferenceTextBox()
		{
			var fallbackReferenceTextBox = control.FallbackReferenceTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(fallbackReferenceTextBox);
				AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.FallbackReference)}", fallbackReferenceTextBox.BindTo);
			});
		}

		public void TestFallbackDateTimeEdit()
		{
			var fallbackDateTimeEdit = control.FallbackDateTimeEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>(fallbackDateTimeEdit);
				AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.FallbackTime)}", fallbackDateTimeEdit.BindTo);
			});
		}

		protected override void SetUp()
		{
			control = new DeclarationDetailsFallbackUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DeclarationDetailsFallbackUserControl control;
	}
}
