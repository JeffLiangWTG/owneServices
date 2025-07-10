using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ManualRegistrationNoEntryForm))]
	class ManualRegistrationNoEntryTest : ZFormBasherTest
	{
		public void TestUpdateRegistrationNoButton()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.FindSingle<Button>("btnOk").PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCloseButton()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.FindSingle<Button>("btnClose").PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var manualRegistryNoEntry = new ManualRegistrationNoEntry(CargoWise.Types.ZString.Empty, CargoWise.Types.ZDateTime.BrettsBirthday);
			var form = new ManualRegistrationNoEntryForm(manualRegistryNoEntry);
			return form;
		}
	}
}
