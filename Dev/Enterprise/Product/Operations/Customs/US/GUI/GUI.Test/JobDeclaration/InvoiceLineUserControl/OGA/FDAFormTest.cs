using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(FDAForm))]
	sealed class FDAFormTest : ZFormBasherTest
	{
		public void TestMIDParser()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new System.Random().Next(1000000).ToString();
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			using (var form = new FDAForm(FDA))
			{
				form.Show();
				AssertEquals(mainAddress.PK, ((ZAddressControl)form.Controls["DetailsGroupBox"].Controls["ManufacturerAddressAddressControl"]).Parse("MID234323"));
				Assert("Visible", form.FDAValueTextBox.Visible);
			}
		}

		public void TestValueFieldsVisibilityAndCaptions()
		{
			using (var form = new FDAForm(FDA))
			{
				form.Show();
				Assert("Visible", form.FDAValueTextBox.Visible);
				Assert("Visible", form.PFRTextBox.Visible);
				var control = form.Controls.Find("US_UC_NKFDAProductionCodeFindBox", true)[0] as IResCaptionedControl;
				AssertEquals("US_UC_NKFDAProductionCodeFindBox label", "Prod. Ctry/Rgn.", control.CaptionResourceString.Caption);
				control = form.Controls.Find("CSHCodeFindBox", true)[0] as IResCaptionedControl;
				AssertEquals("CSHCodeFindBox label", "Country/Region Of Shipping", control.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore() => new FDAForm(FDA);

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => control.Name == "USDCurrencyTextBox";

		FDA fda;
		FDA FDA
		{
			get
			{
				if (fda == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					fda = invoiceLine.FDAs.AddNew();
					Factory.Save();
				}

				return fda;
			}
		}
	}
}
