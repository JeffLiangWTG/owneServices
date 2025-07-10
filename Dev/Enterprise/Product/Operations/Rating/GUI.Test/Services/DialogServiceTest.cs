using System.Linq;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing.Services
{
	public class DialogServiceTest : TestCase
	{
		public void TestSelectSingleCarrierContractNumber_ShouldShowDialogWithNotPopulatingOption()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				var dialogService = new DialogService(parentForm);
				dialogService.SelectSingleCarrierContractNumber(new[] { "Non-Blank", "" });

				var dialog = ZFormModaliser.LastFormShownDialogForTest as SingleValueSelectForm;

				var actualDescriptions = dialog.CheckedListBox_ExposedForTest.BindingItems
					.Select(x => x.Description.ToString())
					.ToArray();

				var expectedDescriptions = new[]
				{
					"",
					"Non-Blank",
					"NOT Populating Carrier Contract Number"
				};

				AssertArrayEqualsByElements(expectedDescriptions, actualDescriptions);
			}
		}
	}
}
