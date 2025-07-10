using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public enum AWBAddressType : int
	{
		Shipper = 0,
		Consignee,
		AlsoNotify
	}

	[DefaultProperty("Caption"), ToolboxData("<{0}:AWBAddressControl runat=server></{0}:AWBAddressControl>")]
	public class AWBAddressControl : AWBBaseControl
	{
		#region Constructors

		public AWBAddressControl()
			: base()
		{
			UseCaptionForCompany = false;
		}

		#endregion

		#region Properties

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public bool UseCaptionForCompany { get; set; }

		[Category("Appearance"), DefaultValue(""), Browsable(true)]
		public string AddressLineCssClass { get; set; }

		string AddressPartialField
		{
			get { return AddressType == AWBAddressType.Shipper ? (NoResString)"Shipper" : (AddressType == AWBAddressType.Consignee ? (NoResString)"Consignee" : "AlsoNotify"); }
		}

		string GetBindToField(string suffix)
		{
			return string.Format("EH_{0}{1}", AddressPartialField, suffix);
		}

		[Category("Appearance"), DefaultValue(AWBAddressType.Shipper), Browsable(true)]
		public AWBAddressType AddressType { get; set; }

		[Category("Appearance"), DefaultValue("Name and Address"), Browsable(true)]
		public string Caption { get; set; }

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			AddressLine1Box = new ZTextBox();
			AddressLine1Box.ID = "AddressLine1";

			AddressLine2Box = new ZTextBox();
			AddressLine2Box.ID = "AddressLine2";

			NameBox = new ZTextBox();
			NameBox.ID = (NoResString)"Name";

			CityBox = new ZTextBox();
			CityBox.ID = (NoResString)"City";

			StateBox = new ZTextBox();
			StateBox.ID = (NoResString)"State";

			PostalCodeBox = new ZTextBox();
			PostalCodeBox.ID = (NoResString)"Zip";

			CountryBox = new ZTextBox();
			CountryBox.ID = (NoResString)"Country";

			PhoneBox = new ZTextBox();
			PhoneBox.ID = (NoResString)"Phone";

			ContactMethodBox = new ZDropDownList();
			ContactMethodBox.ShowEmptyItem = true;
			ContactMethodBox.ID = "ContactMethod";

			AddressLine1CaptionLabel = new ZTextLabel();
			AddressLine2CaptionLabel = new ZTextLabel();
			NameCaptionLabel = new ZTextLabel();
			CityCaptionLabel = new ZTextLabel();
			CountryCaptionLabel = new ZTextLabel();
			PhoneCaptionLabel = new ZTextLabel();
			ControlCaption = new ZTextLabel();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			AddressLine1Box.BindTo = GetBindToField((NoResString)"Address");
			AddressLine2Box.BindTo = GetBindToField("Address2");
			NameBox.BindTo = GetBindToField((NoResString)"Name");
			CityBox.BindTo = GetBindToField((NoResString)"Place");
			StateBox.BindTo = GetBindToField((NoResString)"State");
			PostalCodeBox.BindTo = GetBindToField("PostCode");
			CountryBox.BindTo = GetBindToField("CountryCode");
			PhoneBox.BindTo = GetBindToField("ContactDetail");
			ContactMethodBox.BindTo = GetBindToField("ContactCode");

			AddressLine1Box.CssClass = AddressLineCssClass;
			AddressLine2Box.CssClass = AddressLineCssClass;
			NameBox.CssClass = AddressLineCssClass;
			CityBox.CssClass = "AWBCity";
			PhoneBox.CssClass = "AWBPhone";
			StateBox.CssClass = "AWBState";
			CountryBox.CssClass = "AWBCountry";
			PostalCodeBox.CssClass = "AWBPostalCode";
			ContactMethodBox.CssClass = "AWBContactMethod";

			AddressLine1CaptionLabel.Text = Res.GetString("3d8dc014-a623-4291-89c9-00b98d8a5b0f", "Address 1:");
			AddressLine2CaptionLabel.Text = Res.GetString("bbbff7ec-320e-41c7-b2c1-ca9c3b6af9e3", "Address 2:");
			NameCaptionLabel.Text = (UseCaptionForCompany ? Caption : Res.GetString("6ff44a80-932e-46f1-adc8-5db12e2c3f0b", "Company:"));
			CityCaptionLabel.Text = "City/State/Zip:";
			CountryCaptionLabel.Text = Res.GetString("8312cb92-6740-4f34-af12-c749a2aa3f2f", "Country/Region:");
			PhoneCaptionLabel.Text = Res.GetString("f7a581ab-2a5f-4ee2-95d9-2b4a1df2fd63", "Contact:");

			ControlCaption.Text = Caption;

			Table layoutTable = new Table();
			layoutTable.CellPadding = 0;
			layoutTable.CellSpacing = 0;

			TableRow rowCaption = new TableRow();
			TableCell cellCaption = new TableCell();
			cellCaption.ColumnSpan = 2;
			cellCaption.Controls.Add(ControlCaption);
			cellCaption.CssClass = "AWBSectionTitle";
			rowCaption.Cells.Add(cellCaption);
			if (!UseCaptionForCompany)
			{
				layoutTable.Rows.Add(rowCaption);
			}

			TableRow nameRow = new TableRow();
			TableCell nameCaptionCell = new TableCell();
			nameCaptionCell.CssClass = "AWBCaption";
			if (UseCaptionForCompany)
			{
				nameCaptionCell.CssClass += " AWBSectionTitle";
			}
			nameCaptionCell.Controls.Add(NameCaptionLabel);
			TableCell nameCell = new TableCell();
			nameCell.CssClass = "AWBSectionData";
			nameCell.Controls.Add(NameBox);
			nameRow.Cells.Add(nameCaptionCell);
			nameRow.Cells.Add(nameCell);
			layoutTable.Rows.Add(nameRow);

			TableRow addressLine1Row = new TableRow();
			TableCell addressLine1CaptionCell = new TableCell();
			addressLine1CaptionCell.CssClass = "AWBCaption";
			addressLine1CaptionCell.Controls.Add(AddressLine1CaptionLabel);
			TableCell addressLine1Cell = new TableCell();
			addressLine1Cell.CssClass = "AWBSectionData";
			addressLine1Cell.Controls.Add(AddressLine1Box);
			addressLine1Row.Cells.Add(addressLine1CaptionCell);
			addressLine1Row.Cells.Add(addressLine1Cell);
			layoutTable.Rows.Add(addressLine1Row);

			TableRow addressLine2Row = new TableRow();
			TableCell addressLine2CaptionCell = new TableCell();
			addressLine2CaptionCell.CssClass = "AWBCaption";
			addressLine2CaptionCell.Controls.Add(AddressLine2CaptionLabel);
			TableCell addressLine2Cell = new TableCell();
			addressLine2Cell.CssClass = "AWBSectionData";
			addressLine2Cell.Controls.Add(AddressLine2Box);
			addressLine2Row.Cells.Add(addressLine2CaptionCell);
			addressLine2Row.Cells.Add(addressLine2Cell);
			layoutTable.Rows.Add(addressLine2Row);

			#region City State Zip

			Table cityStateZipTable = new Table();
			cityStateZipTable.CellPadding = 0;
			cityStateZipTable.CellSpacing = 0;
			TableRow cityRow = new TableRow();
			TableCell cityCell = new TableCell();
			cityCell.CssClass = "AWBSectionData";
			cityCell.Controls.Add(CityBox);

			TableCell stateCell = new TableCell();
			stateCell.CssClass = "AWBSectionData";
			stateCell.Controls.Add(StateBox);

			TableCell postalCodeCell = new TableCell();
			postalCodeCell.CssClass = "AWBSectionData";
			postalCodeCell.Controls.Add(PostalCodeBox);

			cityRow.Cells.Add(cityCell);
			cityRow.Cells.Add(stateCell);
			cityRow.Cells.Add(postalCodeCell);
			cityStateZipTable.Rows.Add(cityRow);

			#endregion

			TableRow cityStateZipRow = new TableRow();
			TableCell cityCaptionCell = new TableCell();
			cityCaptionCell.CssClass = "AWBCaption";
			cityCaptionCell.Controls.Add(CityCaptionLabel);
			cityStateZipRow.Cells.Add(cityCaptionCell);
			TableCell cityStateZipCell = new TableCell();
			cityStateZipCell.Controls.Add(cityStateZipTable);
			cityStateZipRow.Cells.Add(cityStateZipCell);
			layoutTable.Rows.Add(cityStateZipRow);

			#region Country Phone

			Table countryPhoneTable = new Table();
			countryPhoneTable.CellPadding = 0;
			countryPhoneTable.CellSpacing = 0;
			TableRow countryRow = new TableRow();
			TableCell countryCell = new TableCell();
			countryCell.CssClass = "AWBSectionData";
			countryCell.Controls.Add(CountryBox);

			TableCell phoneCaptionCell = new TableCell();
			phoneCaptionCell.CssClass = (NoResString)"AWBCaption AWBPhoneCaption";
			phoneCaptionCell.Controls.Add(PhoneCaptionLabel);

			TableCell contactMethodCell = new TableCell();
			contactMethodCell.CssClass = "AWBSectionData";
			contactMethodCell.Controls.Add(ContactMethodBox);

			TableCell phoneCell = new TableCell();
			phoneCell.CssClass = "AWBSectionData";
			phoneCell.Controls.Add(PhoneBox);
			countryRow.Cells.Add(countryCell);
			countryRow.Cells.Add(phoneCaptionCell);
			countryRow.Cells.Add(contactMethodCell);
			countryRow.Cells.Add(phoneCell);
			countryPhoneTable.Rows.Add(countryRow);

			#endregion

			TableRow countryPhoneRow = new TableRow();
			TableCell countryCaptionCell = new TableCell();
			countryCaptionCell.CssClass = "AWBCaption";
			countryCaptionCell.Controls.Add(CountryCaptionLabel);
			countryPhoneRow.Cells.Add(countryCaptionCell);
			TableCell countryPhoneCell = new TableCell();
			countryPhoneCell.Controls.Add(countryPhoneTable);
			countryPhoneRow.Cells.Add(countryPhoneCell);
			layoutTable.Rows.Add(countryPhoneRow);

			Controls.Clear();
			Controls.Add(layoutTable);
		}

		#endregion

		#region Implementation

		ZTextBox AddressLine1Box;
		ZTextBox AddressLine2Box;
		ZTextBox NameBox;
		ZTextBox CityBox;
		ZTextBox StateBox;
		ZTextBox PostalCodeBox;
		ZTextBox CountryBox;
		ZTextBox PhoneBox;
		ZDropDownList ContactMethodBox;

		ZTextLabel AddressLine1CaptionLabel;
		ZTextLabel AddressLine2CaptionLabel;
		ZTextLabel NameCaptionLabel;
		ZTextLabel CityCaptionLabel;
		ZTextLabel CountryCaptionLabel;
		ZTextLabel PhoneCaptionLabel;

		ZTextLabel ControlCaption;

		#endregion
	}
}
