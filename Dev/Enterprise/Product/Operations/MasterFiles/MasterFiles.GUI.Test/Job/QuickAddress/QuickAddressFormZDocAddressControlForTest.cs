using System.Threading.Tasks;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class QuickAddressFormZDocAddressControlForTest : QuickAddressFormZDocAddressControl
	{
		public async Task ValidateAddressForTest()
		{
			await ValidateAddress();
		}

		public async Task GetCityTownAsyncForTest()
		{
			await GetCityTownAsync();
		}

		public void CloseSuggestionFormsForTest()
		{
			CloseSuggestionForms();
		}
	}
}
