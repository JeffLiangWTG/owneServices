using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class GuaranteeDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestGuaranteeDetailsGroupBoxTabOrder()
		{
			using (var form = new ZForm(guaranteeHeader))
			using (var control = new GuaranteeDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("GuaranteeTypeZDropEdit TabIndex", 0, control.FindSingle<ZDropEdit>("GuaranteeTypeZDropEdit").TabIndex);
					AssertEquals("GuaranteeSubTypeZDropEdit TabIndex", 1, control.FindSingle<ZDropEdit>("GuaranteeSubTypeZDropEdit").TabIndex);
					AssertEquals("GuaranteeHolderZGuidFindBox TabIndex", 2, control.FindSingle<ZGuidFindBox>("GuaranteeHolderZGuidFindBox").TabIndex);
					AssertEquals("CreationCountryZCodeFindBox TabIndex", 3, control.FindSingle<ZCodeFindBox>("CreationCountryZCodeFindBox").TabIndex);
					AssertEquals("GuaranteeNumberZTextBox TabIndex", 4, control.FindSingle<ZTextBox>("GuaranteeNumberZTextBox").TabIndex);
					AssertEquals("CurrencyZCodeFindBox TabIndex", 5, control.FindSingle<ZCodeFindBox>("CurrencyZCodeFindBox").TabIndex);
					AssertEquals("QtyValIndicatorZDropEdit TabIndex", 6, control.FindSingle<ZDropEdit>("QtyValIndicatorZDropEdit").TabIndex);
					AssertEquals("StartDateZDateEdit TabIndex", 7, control.FindSingle<ZDateEdit>("StartDateZDateEdit").TabIndex);
					AssertEquals("EndDateZDateEdit TabIndex", 8, control.FindSingle<ZDateEdit>("EndDateZDateEdit").TabIndex);
				});
			}
		}

		public void TestQtyFieldVisibility()
		{
			using (var form = new ZForm(guaranteeHeader))
			using (var control = new GuaranteeDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var qtyValIndicatorZDropEdit = control.FindSingle<ZDropEdit>("QtyValIndicatorZDropEdit");
				AssertEquals("Quantity field should not show", qtyValIndicatorZDropEdit.Visible, false);
			}
		}

		public void TestCurrencyField()
		{
			AssertType<RefCurrencyCollection>(guaranteeHeader.Lookups.Currencies);
			using (var form = new ZForm(guaranteeHeader))
			using (var control = new GuaranteeDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var currencyZCodeFindBox = control.FindSingle<ZCodeFindBox>("CurrencyZCodeFindBox");
				var subTypeZDropEdit = control.FindSingle<ZDropEdit>("GuaranteeSubTypeZDropEdit");
				AssertEquals("Currency field should be vertically aligned with Subtype field", currencyZCodeFindBox.Location.X, subTypeZDropEdit.Location.X);
			}
		}

		public void TestCreationCountryField()
		{
			using (var form = new ZForm(guaranteeHeader))
			using (var control = new GuaranteeDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var creationCountryZCodeFindBox = control.FindSingle<ZCodeFindBox>("CreationCountryZCodeFindBox");
				AssertEquals("CreationCountry field should be visible", true, creationCountryZCodeFindBox.Visible);

				var subTypeZDropEdit = control.FindSingle<ZDropEdit>("GuaranteeSubTypeZDropEdit");
				AssertEquals("CreationCountry field should be vertically aligned with Subtype field", creationCountryZCodeFindBox.Location.X, subTypeZDropEdit.Location.X);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
		}

		BaseCusGuaranteeHeader guaranteeHeader;
	}
}
