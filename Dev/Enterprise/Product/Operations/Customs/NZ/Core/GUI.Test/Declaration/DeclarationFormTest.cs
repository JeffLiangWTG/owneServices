using System;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class DeclarationFormTest : TestCaseWithFactory
	{
		public void TestWithMinimumDataVersionRequired()
		{
			Business.EDITariff_ReferenceFiles_NZ.Testing.NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (DeclarationForm form = new DeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("Pre-condition: UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pre-condition: form.Visible", true, form.Visible);
			}

			NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			using (DeclarationForm form = new DeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("form.Visible", true, form.Visible);
			}

			NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired - 1);
			using (DeclarationForm form = new DeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", string.Format(@"NZ Tariff minimum data version [{0}] requirement not met. The current Tariff data version is [{1}].
Please update your Tariff data by running ediTariff -> {2} -> Export NZ data to {2}.", NZCTariffVersionLoader.MinimumDataVersionRequired, NZCTariffVersionLoader.MinimumDataVersionRequired - 1, BrandingFactory.Instance.ProductName), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("form.Visible", false, form.Visible);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired + 1);
			using (DeclarationForm form = new DeclarationForm(declaration))
			{
				form.Show();
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("form.Visible", true, form.Visible);
			}
		}

		public void TestStmDataExists()
		{
			NZCTariffVersionLoaderTest.DropStmDataTable();

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (DeclarationForm form = new DeclarationForm(declaration))
				{
					form.Show();
					AssertEquals("No user notification when UseRefDatabaseData is true.", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form visible when UseRefDatabaseData is true.", true, form.Visible);
				}
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (DeclarationForm form = new DeclarationForm(declaration))
				{
					form.Show();

					var expectedErrorMessage = NZCTariffVersionLoaderTest.GetMinimumDataVersionMessage(NZCTariffVersionLoader.MinimumDataVersionRequired, 0);
					AssertEquals("User notification matches expected error message when UseRefDatabaseData is false.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form to not be visible when UseRefDatabaseData is false.", false, form.Visible);
				}
			}
		}
	}
}
