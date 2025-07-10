using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class AdditionalSupTariffsUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalTariffUserControlsVisibility()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			using (var form = new ZForm(invoiceLine))
			using (var userControl = new AdditionalSupTariffsUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(form.BusinessEntity, "");
				form.Show();

				userControl.OnSupTariffFormattedFieldTypeChanged(false);
				AssertEquals("SupAdditionalTariff1FindBox.Visible", true, userControl.SupAdditionalTariff1FindBox.Visible);
				AssertEquals("SupAdditionalTariff2FindBox.Visible", true, userControl.SupAdditionalTariff2FindBox.Visible);
				AssertEquals("SupAdditionalTariff3FindBox.Visible", true, userControl.SupAdditionalTariff3FindBox.Visible);
				AssertEquals("SupAdditionalTariff4FindBox.Visible", true, userControl.SupAdditionalTariff4FindBox.Visible);
				AssertEquals("SupAdditionalTariff5FindBox.Visible", true, userControl.SupAdditionalTariff5FindBox.Visible);
				AssertEquals("SupAdditionalTariff1DropEdit.Visible", false, userControl.SupAdditionalTariff1DropEdit.Visible);
				AssertEquals("SupAdditionalTariff2DropEdit.Visible", false, userControl.SupAdditionalTariff2DropEdit.Visible);
				AssertEquals("SupAdditionalTariff3DropEdit.Visible", false, userControl.SupAdditionalTariff3DropEdit.Visible);
				AssertEquals("SupAdditionalTariff4DropEdit.Visible", false, userControl.SupAdditionalTariff4DropEdit.Visible);
				AssertEquals("SupAdditionalTariff5DropEdit.Visible", false, userControl.SupAdditionalTariff5DropEdit.Visible);

				userControl.OnSupTariffFormattedFieldTypeChanged(true);
				AssertEquals("SupAdditionalTariff1FindBox.Visible", false, userControl.SupAdditionalTariff1FindBox.Visible);
				AssertEquals("SupAdditionalTariff2FindBox.Visible", false, userControl.SupAdditionalTariff2FindBox.Visible);
				AssertEquals("SupAdditionalTariff3FindBox.Visible", false, userControl.SupAdditionalTariff3FindBox.Visible);
				AssertEquals("SupAdditionalTariff4FindBox.Visible", false, userControl.SupAdditionalTariff4FindBox.Visible);
				AssertEquals("SupAdditionalTariff5FindBox.Visible", false, userControl.SupAdditionalTariff5FindBox.Visible);
				AssertEquals("SupAdditionalTariff1DropEdit.Visible", true, userControl.SupAdditionalTariff1DropEdit.Visible);
				AssertEquals("SupAdditionalTariff2DropEdit.Visible", true, userControl.SupAdditionalTariff2DropEdit.Visible);
				AssertEquals("SupAdditionalTariff3DropEdit.Visible", true, userControl.SupAdditionalTariff3DropEdit.Visible);
				AssertEquals("SupAdditionalTariff4DropEdit.Visible", true, userControl.SupAdditionalTariff4DropEdit.Visible);
				AssertEquals("SupAdditionalTariff5DropEdit.Visible", true, userControl.SupAdditionalTariff5DropEdit.Visible);

				Assert("Quantity for Sup Additional Tariff 1 is visible", userControl.Controls.Find("SupAdditionalTariff1CustomsQuantityCalcDropEdit", true)[0].Visible);
				Assert("Quantity for Sup Additional Tariff 2 is visible", userControl.Controls.Find("SupAdditionalTariff2CustomsQuantityCalcDropEdit", true)[0].Visible);
				Assert("Quantity for Sup Additional Tariff 3 is visible", userControl.Controls.Find("SupAdditionalTariff3CustomsQuantityCalcDropEdit", true)[0].Visible);
				Assert("Quantity for Sup Additional Tariff 4 is visible", userControl.Controls.Find("SupAdditionalTariff4CustomsQuantityCalcDropEdit", true)[0].Visible);
				Assert("Quantity for Sup Additional Tariff 5 is visible", userControl.Controls.Find("SupAdditionalTariff5CustomsQuantityCalcDropEdit", true)[0].Visible);

				Assert("Goods Value for Sup Additional Tariff 1 is not visible", !userControl.Controls.Find("SupAdditionalTariff1GoodsValueCalcDropEdit", true)[0].Visible);
				Assert("Goods Value for Sup Additional Tariff 2 is not visible", !userControl.Controls.Find("SupAdditionalTariff2GoodsValueCalcDropEdit", true)[0].Visible);
				Assert("Goods Value for Sup Additional Tariff 3 is not visible", !userControl.Controls.Find("SupAdditionalTariff3GoodsValueCalcDropEdit", true)[0].Visible);
				Assert("Goods Value for Sup Additional Tariff 4 is not visible", !userControl.Controls.Find("SupAdditionalTariff4GoodsValueCalcDropEdit", true)[0].Visible);
				Assert("Goods Value for Sup Additional Tariff 5 is not visible", !userControl.Controls.Find("SupAdditionalTariff5GoodsValueCalcDropEdit", true)[0].Visible);
			}
		}
	}
}
