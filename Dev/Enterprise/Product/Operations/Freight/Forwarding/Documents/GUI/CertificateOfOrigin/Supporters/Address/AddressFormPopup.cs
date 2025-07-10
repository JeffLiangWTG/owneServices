using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Freight.Forwarding.Documents.GUI.CertificateOfOrigin
{
	public partial class AddressFormPopup : ZChildForm
	{
		readonly IFindBox findBox;
		readonly string Unknown = "UNKNOWN";
		readonly Dictionary<AddressType, AddressBusinessObject> addressDictionary = new Dictionary<AddressType, AddressBusinessObject>
		{
			{ AddressType.EXPORTER, new AddressBusinessObject() },
			{ AddressType.PRODUCER, new AddressBusinessObject() }
		};

		public override string FormHeading => Res.GetString("25D23FFA-D92D-49BA-8C57-D96B6BDB1B99", "Producer Address");

		public AddressFormPopup(IFindBox findBox) : base(findBox.ListProvider?.List as AddressBusinessObjectConfiguration)
		{
			this.findBox = findBox;
			var addressState = ExtractValidAddressStateFromFindBox(findBox);

			InitializeComponent();

			if (findBox.ListProvider?.List is AddressBusinessObjectConfiguration addressBusinessObjectConfiguration)
			{
				PopulateAddressDictionary(addressBusinessObjectConfiguration);
				ApplyConfiguration(addressBusinessObjectConfiguration);
				TryAndParseCodeIntoFields(addressBusinessObjectConfiguration, addressState);
			}
		}

		void PopulateAddressDictionary(AddressBusinessObjectConfiguration addressBusinessObjectConfiguration)
		{
			if (addressBusinessObjectConfiguration.Count > 0)
			{
				// .Net won't let me use "var" here as it recognises addressBusinessObjectConfiguration as a collection of BusinessObject rather than AddressBusinessObject
				foreach (AddressBusinessObject addressBusinessObject in addressBusinessObjectConfiguration)
				{
					addressDictionary[addressBusinessObject.AddressType] = addressBusinessObject;
				}
			}
		}

		void ApplyConfiguration(AddressBusinessObjectConfiguration addressBusinessObjectConfiguration)
		{
			excludeFromCertificateCheckbox.CaptionResourceString = excludeFromCertificateCheckbox.CaptionResourceString.Format(addressBusinessObjectConfiguration.CertificateName);
			unknownCheckbox.Enabled = addressBusinessObjectConfiguration.EnableUnknown;
		}

		void TryAndParseCodeIntoFields(AddressBusinessObjectConfiguration addressBusinessObjectConfiguration, AddressState addressState)
		{
			if (addressState.IsSameAsExporter)
			{
				sameAsExporterCheckbox.Checked = true;
				PopulateForm(addressDictionary[AddressType.EXPORTER]);
			}
			else if (addressState.IsUnknown)
			{
				unknownCheckbox.Checked = true;
				PopulateForm(name: Unknown);
			}
			else
			{
				PopulateForm(addressDictionary[AddressType.PRODUCER]);
			}

			excludeFromCertificateCheckbox.Checked = addressState.ExcludeFromPDF;
		}

		void CancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void OkBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				if (string.IsNullOrEmpty(nameTextBox.Text))
				{
					Globals.Message.ShowError(Res.GetString("61821FE7-0B7B-4AAB-92C0-44C3B0B2B5C2", "Producer Name is mandatory for JEVS submissions. If producer is unknown, select Unknown checkbox."));
					e.Cancel = true;
					return;
				}

				findBox.Code = AddressStateHelper.GetAddressStateString(
					sameAsExporter: sameAsExporterCheckbox.Checked,
					unknown: unknownCheckbox.Checked,
					excludeFromCertificate: excludeFromCertificateCheckbox.Checked);
			}

			base.OnClosing(e);
		}

		void sameAsExporterCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (sameAsExporterCheckbox.Checked)
			{
				unknownCheckbox.Checked = false;
				PopulateForm(addressDictionary[AddressType.EXPORTER]);
			}
			else
			{
				PopulateForm(addressDictionary[AddressType.PRODUCER]);
			}
		}

		void unknownCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (unknownCheckbox.Checked)
			{
				sameAsExporterCheckbox.Checked = false;
				PopulateForm(name: Unknown);
			}
			else
			{
				PopulateForm(addressDictionary[AddressType.PRODUCER]);
			}
		}

		void PopulateForm(AddressBusinessObject address)
		{
			nameTextBox.Text = address.Name;
			address1TextBox.Text = address.AddressLine1;
			address2TextBox.Text = address.AddressLine2;
			cityTextBox.Text = address.City;
			stateTextBox.Text = address.State;
			postCodeTextBox.Text = address.PostCode;
			countryTextBox.Text = address.CountryName;
		}

		void PopulateForm(string name = "", string address1 = "", string address2 = "", string city = "", string state = "", string postcode = "", string country = "")
		{
			nameTextBox.Text = name;
			address1TextBox.Text = address1;
			address2TextBox.Text = address2;
			cityTextBox.Text = city;
			stateTextBox.Text = state;
			postCodeTextBox.Text = postcode;
			countryTextBox.Text = country;
		}

		static AddressState ExtractValidAddressStateFromFindBox(IFindBox findBox)
		{
			var addressState = AddressStateHelper.GetAddressStateObjectFromString(findBox?.Code);

			if (addressState is null)
			{
				Globals.Message.ShowError(Res.GetString("2C23FAF5-1948-4D4F-8485-6776999F6EFA", "Something went wrong, Cannot load Producer Address data."));
			}

			return addressState ?? new AddressState();
		}
	}
}
