using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.SADH;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.SADH.Testing
{
	[TestedType(typeof(SADHEntryForm))]
	sealed class SADHEntryFormTest : ZFormBasherTest
	{
		public void TestModeOfTransportVisiblityShowsAndHidesCorrectly()
		{
			FormDataManagerForTesting manager = new FormDataManagerForTesting(new SADHFormData(Factory, Factory.New<BaseJobDeclaration>()));
			using (SADHEntryForm form = new SADHEntryForm(manager))
			{
				form.Show();
				form.FormDataManager.FormData.D1_ModeOfTransportAtTheBorder = TransportTypeList.Codes.Air;
				AssertEquals("form.Section21FlightNoLabel.Visible", true, form.Section21FlightNoLabel.Visible);
				AssertEquals("form.Section21FlightNoTextBox.Visible", true, form.Section21FlightNoTextBox.Visible);
				AssertEquals("form.Section21FlightDateLabel.Visible", true, form.Section21FlightDateLabel.Visible);
				AssertEquals("form.Section21FlightDateDateEdit.Visible", true, form.Section21FlightDateDateEdit.Visible);
				AssertEquals("form.Section21VesselLabel.Visible", false, form.Section21VesselLabel.Visible);
				AssertEquals("form.Section21VesselCodeFindBox.Visible", false, form.Section21VesselCodeFindBox.Visible);
				form.FormDataManager.FormData.D1_ModeOfTransportAtTheBorder = TransportTypeList.Codes.Sea;
				AssertEquals("form.Section21FlightNoLabel.Visible", false, form.Section21FlightNoLabel.Visible);
				AssertEquals("form.Section21FlightNoTextBox.Visible", false, form.Section21FlightNoTextBox.Visible);
				AssertEquals("form.Section21FlightDateLabel.Visible", false, form.Section21FlightDateLabel.Visible);
				AssertEquals("form.Section21VesselLabel.Visible", true, form.Section21VesselLabel.Visible);
				AssertEquals("form.Section21VesselCodeFindBox.Visible", true, form.Section21VesselCodeFindBox.Visible);
				form.FormDataManager.FormData.D1_ModeOfTransportAtTheBorder = ZString.Empty;
				AssertEquals("form.Section21FlightNoLabel.Visible", false, form.Section21FlightNoLabel.Visible);
				AssertEquals("form.Section21FlightNoTextBox.Visible", false, form.Section21FlightNoTextBox.Visible);
				AssertEquals("form.Section21FlightDateLabel.Visible", false, form.Section21FlightDateLabel.Visible);
				AssertEquals("form.Section21FlightDateDateEdit.Visible", false, form.Section21FlightDateDateEdit.Visible);
				AssertEquals("form.Section21VesselLabel.Visible", false, form.Section21VesselLabel.Visible);
				AssertEquals("form.Section21VesselCodeFindBox.Visible", false, form.Section21VesselCodeFindBox.Visible);
			}
		}

		public void TestFormCaptionIsExactlyWhatIWant()
		{
			Env.Registry.ShowBranchName = false;
			Env.Registry.ShowCompanyName = false;
			Env.Registry.ShowDepartmentName = false;
			Env.Registry.ShowUserName = false;
			FormDataManagerForTesting manager = new FormDataManagerForTesting(new SADHFormData(Factory, Factory.New<BaseJobDeclaration>()));
			using (SADHEntryForm form = new SADHEntryForm(manager))
			{
				form.Show();
				AssertEquals("form.Text", "SAD/H Data Entry", form.Text);
				AssertEquals("form.FullCaption", "SAD/H Data Entry", form.TextIncludingSuffix);
				AssertEquals("form.FormHeading", "SAD/H Data Entry", form.FormHeading);
			}
		}

		public void TestOKButton()
		{
			FormDataManagerForTesting manager = new FormDataManagerForTesting(new SADHFormData(Factory, Factory.New<BaseJobDeclaration>()));
			using (SADHEntryForm form = new SADHEntryForm(manager))
			{
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);
				form.Show();
				AssertEquals("Precondition: FormClosed", false, formClosed);
				form.OKButton.PerformClick();
				AssertEquals("FormClosed", true, formClosed);
			}

			AssertEquals("manager.WriteDataWasCalled", true, manager.WriteDataWasCalled);
		}

		public void TestCancelButton()
		{
			FormDataManagerForTesting manager = new FormDataManagerForTesting(new SADHFormData(Factory, Factory.New<BaseJobDeclaration>()));
			using (SADHEntryForm form = new SADHEntryForm(manager))
			{
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);
				form.Show();
				AssertEquals("Precondition: FormClosed", false, formClosed);
				form.CancelButton.PerformClick();
				AssertEquals("FormClosed", true, formClosed);
			}

			AssertEquals("manager.WriteDataWasCalled", false, manager.WriteDataWasCalled);
		}

		protected override Form GetFormToBashCore() => new SADHEntryForm(new FormDataManagerForTesting(new SADHFormData(Factory, Factory.New<BaseJobDeclaration>())));

		bool formClosed;
		void form_FormClosed(object sender, FormClosedEventArgs e)
		{
			formClosed = true;
		}

		sealed class FormDataManagerForTesting : ISADHFormDataManager
		{
			public FormDataManagerForTesting(SADHFormData formData)
			{
				FormData = formData;
			}

			public SADHFormData FormData { get; private set; }

			public void WriteData()
			{
				WriteDataWasCalled = true;
			}

			public bool WriteDataWasCalled;

			public bool ExecutedSuccessfully { get; set; }
		}
	}
}
