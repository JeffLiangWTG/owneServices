using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.Freight.Forwarding.Documents.GUI.CertificateOfOrigin;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.GUI.CertificateOfOrigin.Address
{
	[TestedType(typeof(AddressFormPopup))]
	class AddressFormPopupTest : ZFormBasherTest
	{
		public void Test_TryAndParseCodeIntoFields_PopulatesFormWithProducerAddress_When_DefaultState()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				TestFormFields(form, ProducerAddressBusinessObject);
			}
		}

		public void Test_TryAndParseCodeIntoFields_PopulatesFormWithExporterAddress_When_IsSameAsExporter()
		{
			var findBox = new DummyFindBox()
			{
				Code = "True\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				TestFormFields(form, ExporterAddressBusinessObject, sameAsExporter: true);
			}
		}

		public void Test_TryAndParseCodeIntoFields_PopulatesFormWithUnknown_When_IsUnknown()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nTrue\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				TestFormFields(form, name: Unknown, unknown: true);
			}
		}

		public void Test_TryAndParseCodeIntoFields_ChecksExcludeFromCertificateCheckbox_When_ExcludeFromPDF()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nTrue",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				TestFormFields(form, ProducerAddressBusinessObject, excludeFromCertificate: true);
			}
		}

		public void Test_TryAndParseCodeIntoFields_ReturnsError_When_FindboxCodeIsImparsable()
		{
			var findBox = new DummyFindBox() { Code = string.Empty };

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				AssertEquals("Something went wrong, Cannot load Producer Address data.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				TestFormFields(form);
			}
		}

		public void Test_AddressForm_ReturnsExpectedCode_OnClosing()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();

				form.SameAsExporterCheckbox.Checked = true;
				form.ExcludeFromCertificateCheckbox.Checked = true;
				form.OkButton.PerformClick();

				AssertEquals("FindBox Code when Same As Exporter and ExcludeFromCertificate should be checked", "True\nFalse\nTrue", findBox.Code);
			}

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();

				form.UnknownCheckbox.Checked = true;
				form.ExcludeFromCertificateCheckbox.Checked = true;
				form.OkButton.PerformClick();

				AssertEquals("FindBox Code when Unknown and ExcludeFromCertificate should be checked", "False\nTrue\nTrue", findBox.Code);
			}
		}

		public void Test_SameAsExporterCheckbox_And_UnknownCheckbox_Are_MutuallyExclusive()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();

				form.UnknownCheckbox.Checked = true;
				form.SameAsExporterCheckbox.Checked = true;

				TestFormFields(form, ExporterAddressBusinessObject, sameAsExporter: true);
			}

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();

				form.SameAsExporterCheckbox.Checked = true;
				form.UnknownCheckbox.Checked = true;

				TestFormFields(form, name: Unknown, unknown: true);
			}
		}

		public void Test_AddressForm_ReturnsError_When_CompanyNameIsEmpty()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				form.NameTextBox.Text = string.Empty;
				form.OkButton.PerformClick();
				AssertEquals("Producer Name is mandatory for JEVS submissions. If producer is unknown, select Unknown checkbox.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void Test_SameAsExporterCheckbox_UpdatesForm()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				form.SameAsExporterCheckbox.Checked = true;
				TestFormFields(form, ExporterAddressBusinessObject, sameAsExporter: true);
			}
		}

		public void Test_SetCertificateName()
		{
			var configuration = new AddressBusinessObjectConfiguration(certificateName: "Swimming", enableUnknown: true) { ProducerAddressBusinessObject, ExporterAddressBusinessObject };
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = configuration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				AssertEquals("ExcludeFromNzcftaCheckbox label should contain certificateName", "Exclude from Swimming PDF", form.ExcludeFromCertificateCheckbox.Text);
				TestFormFields(form, ProducerAddressBusinessObject);
			}
		}

		public void Test_SetDisableUnkown_True()
		{
			var configuration = new AddressBusinessObjectConfiguration(certificateName: "NZCFTA", enableUnknown: false) { ProducerAddressBusinessObject, ExporterAddressBusinessObject };
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = configuration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				AssertEquals("Expected UknownCheckbox.Enable to be false", expected: false, form.UnknownCheckbox.Enabled);
			}
		}

		public void Test_SetDisableUnkown_False()
		{
			var configuration = new AddressBusinessObjectConfiguration(certificateName: "NZCFTA", enableUnknown: true) { ProducerAddressBusinessObject, ExporterAddressBusinessObject };
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = configuration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				AssertEquals("Expected UknownCheckbox.Enable to be true", expected: true, form.UnknownCheckbox.Enabled);
			}
		}

		public void Test_UnknownCheckbox_UpdatesForm()
		{
			var findBox = new DummyFindBox()
			{
				Code = "False\nFalse\nFalse",
				ListProvider = addressBusinessObjectConfiguration
			};

			using (var form = new AddressFormPopupForTest(findBox))
			{
				form.Show();
				form.UnknownCheckbox.Checked = true;
				TestFormFields(form, name: Unknown, unknown: true);
			}
		}

		void TestFormFields(AddressFormPopupForTest form, AddressBusinessObject addressBusinessObject, bool sameAsExporter = false, bool unknown = false, bool excludeFromCertificate = false)
		{
			AssertEquals("Name", addressBusinessObject.Name, form.NameTextBox.Text);
			AssertEquals("AddressLine1", addressBusinessObject.AddressLine1, form.Address1TextBox.Text);
			AssertEquals("AddressLine2", addressBusinessObject.AddressLine2, form.Address2TextBox.Text);
			AssertEquals("City", addressBusinessObject.City, form.CityTextBox.Text);
			AssertEquals("State", addressBusinessObject.State, form.StateTextBox.Text);
			AssertEquals("PostCode", addressBusinessObject.PostCode, form.PostCodeTextBox.Text);
			AssertEquals("CountryName", addressBusinessObject.CountryName, form.CountryTextBox.Text);
			AssertEquals("SameAsExporterCheckbox", sameAsExporter, form.SameAsExporterCheckbox.Checked);
			AssertEquals("UnknownCheckBox", unknown, form.UnknownCheckbox.Checked);
			AssertEquals("ExcludeFromCertificateCheckbox", excludeFromCertificate, form.ExcludeFromCertificateCheckbox.Checked);
		}

		void TestFormFields(AddressFormPopupForTest form, string name = "", string address1 = "", string address2 = "", string city = "", string state = "", string postcode = "", string countryName = "",
			bool sameAsExporter = false, bool unknown = false, bool excludeFromCertificate = false)
		{
			AssertEquals("Name", name, form.NameTextBox.Text);
			AssertEquals("AddressLine1", address1, form.Address1TextBox.Text);
			AssertEquals("AddressLine2", address2, form.Address2TextBox.Text);
			AssertEquals("City", city, form.CityTextBox.Text);
			AssertEquals("State", state, form.StateTextBox.Text);
			AssertEquals("PostCode", postcode, form.PostCodeTextBox.Text);
			AssertEquals("CountryName", countryName, form.CountryTextBox.Text);
			AssertEquals("SameAsExporterCheckbox", sameAsExporter, form.SameAsExporterCheckbox.Checked);
			AssertEquals("UnknownCheckBox", unknown, form.UnknownCheckbox.Checked);
			AssertEquals("ExcludeFromCertificateCheckbox", excludeFromCertificate, form.ExcludeFromCertificateCheckbox.Checked);
		}

		protected override Form GetFormToBashCore() => new AddressFormPopup(new DummyFindBox());

		class AddressFormPopupForTest : AddressFormPopup
		{
			public AddressFormPopupForTest(IFindBox findBox) : base(findBox)
			{
			}

			public Button OkButton => okButton;
			public new Button CancelButton => cancelButton;
			public ZArchitecture.ZTextBox NameTextBox => nameTextBox;
			public ZArchitecture.ZTextBox Address1TextBox => address1TextBox;
			public ZArchitecture.ZTextBox Address2TextBox => address2TextBox;
			public ZArchitecture.ZTextBox CityTextBox => cityTextBox;
			public ZArchitecture.ZTextBox StateTextBox => stateTextBox;
			public ZArchitecture.ZTextBox PostCodeTextBox => postCodeTextBox;
			public ZArchitecture.ZTextBox CountryTextBox => countryTextBox;
			public ZCheckBox SameAsExporterCheckbox => sameAsExporterCheckbox;
			public ZCheckBox UnknownCheckbox => unknownCheckbox;
			public ZCheckBox ExcludeFromCertificateCheckbox => excludeFromCertificateCheckbox;
		}

		#region TestData

		static class ProducerAddress
		{
			public const string CompanyName = "MANUFACTURER";
			public const string AddressLine1 = "83 ULSTER ROAD";
			public const string AddressLine2 = "UNIT52";
			public const string City = "AUCKLAND";
			public const string State = "AUK";
			public const string Postcode = "1010";
			public const string CountryName = "NEW ZEALAND";
		}

		static class ExporterAddress
		{
			public const string CompanyName = "CONSIGNOR";
			public const string AddressLine1 = "84 ULSTER ROAD";
			public const string AddressLine2 = "UNIT53";
			public const string City = "SYDNEY";
			public const string State = "NSW";
			public const string Postcode = "1212";
			public const string CountryName = "AUSTRALIA";
		}

		static readonly AddressBusinessObject ProducerAddressBusinessObject = new AddressBusinessObject()
		{
			Name = ProducerAddress.CompanyName,
			AddressLine1 = ProducerAddress.AddressLine1,
			AddressLine2 = ProducerAddress.AddressLine2,
			City = ProducerAddress.City,
			State = ProducerAddress.State,
			PostCode = ProducerAddress.Postcode,
			CountryName = ProducerAddress.CountryName,
			AddressType = AddressType.PRODUCER
		};

		static readonly AddressBusinessObject ExporterAddressBusinessObject = new AddressBusinessObject()
		{
			Name = ExporterAddress.CompanyName,
			AddressLine1 = ExporterAddress.AddressLine1,
			AddressLine2 = ExporterAddress.AddressLine2,
			City = ExporterAddress.City,
			State = ExporterAddress.State,
			PostCode = ExporterAddress.Postcode,
			CountryName = ExporterAddress.CountryName,
			AddressType = AddressType.EXPORTER
		};

		readonly AddressBusinessObjectConfiguration addressBusinessObjectConfiguration = new AddressBusinessObjectConfiguration(certificateName: "NZCFTA", enableUnknown: true) { ProducerAddressBusinessObject, ExporterAddressBusinessObject };

		readonly string Unknown = "UNKNOWN";

		#endregion
	}
}
