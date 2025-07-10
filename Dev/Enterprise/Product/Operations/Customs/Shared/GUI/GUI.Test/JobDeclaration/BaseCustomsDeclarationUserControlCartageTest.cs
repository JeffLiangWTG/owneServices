using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseCustomsDeclarationUserControlCartageTest : Freight.Business.Testing.BaseFreightTest
	{
		public void TestNumbersUserControl()
		{
			using (var control = new BaseCustomsDeclarationUserControl())
			{
				AssertEquals(true, control.NumbersTabPage.ControlHasChildOrIsType(typeof(NumbersUserControl)));
			}
		}

		public void TestCustomFields()
		{
			var value = new CaptionAndHint();
			value.Caption = "TEST";
			value.Hint = "Hint";

			using (FreightDataRegistry.Instance.ShipmentCustomText1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (FreightDataRegistry.Instance.ShipmentCustomDate1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (FreightDataRegistry.Instance.ShipmentCustomFlag1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (var form = new BaseJobDeclarationForm(Factory.New<BaseJobDeclaration>()))
			{
				form.Show();
				form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.RightTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.OrganisationsTabPage;

				var tabControl = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.FindSingle<ZTabControl>("RightTabControl");
				var customTabPage = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting.ShipmentCustomFieldsPage;
				tabControl.SelectedTab = customTabPage;
				var rowLayoutPanel = customTabPage.FindSingle<Control>("rowLayoutPanel");

				CombineAssertions(() =>
				{
					foreach (Control control in rowLayoutPanel.Controls)
					{
						AssertEquals(control.GetType() + ".Location.Y is negative", true, control.Location.Y >= 0);
					}
				});
			}
		}
	}
}
