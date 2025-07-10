using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class AddressDetailsControlWithLanguageTest : TransactionedTestCase
	{
		public void TestAddAndRemoveFromMainForm()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;

			using (var form = new ZForm())
			{
				using (var docAddress = new AddressDetailsControlWithLanguageForTesting())
				{
					form.Controls.Add(docAddress);

					form.Show();

					Application.DoEvents();
					docAddress.ParentForm.Dispose();
					var result = Task.Factory.StartNew(() =>
						docAddress.GetCityTownAsyncExposed().ConfigureAwait(false)
					).Result;
					form.Controls.Remove(docAddress);

					AssertNoExceptionThrown("GetCityTownAsync() should not throw any exception", () => result.GetAwaiter().GetResult());
				}
				AssertNoExceptionThrown("It should not throw any exception since it's being disposed correctly", form.Close);
			}
		}
	}

	class AddressDetailsControlWithLanguageForTesting : AddressDetailsControlWithLanguage
	{
		public async Task GetCityTownAsyncExposed() => await GetCityTownAsync();
	}
}
