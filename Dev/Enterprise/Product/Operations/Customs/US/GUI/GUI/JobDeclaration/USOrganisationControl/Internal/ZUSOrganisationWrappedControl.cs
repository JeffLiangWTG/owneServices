using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.GUI.Internal
{
	partial class ZUSOrganisationWrappedControl : ZOrganisationControl
	{
		public ZUSOrganisationWrappedControl()
		{
			HideLinks();
			AddAddressAndContactControls();

			AddressBindTo = "";
			ContactBindTo = "";
			ContactBindToList = "";
			PhoneBindTo = "";
		}

		#region Adding new drop lists, hiding links, and resizing to fit

		void HideLinks()
		{
			AddressesLink.Visible = false;
			ContactsLink.Visible = false;
		}

		protected void AddAddressAndContactControls()
		{
			addressEdit = new ZGuidDropEditWithFixedWidth();
			addressEdit.Name = "AddressEdit";
			contactEdit = new ZDropEditWithFixedWidth();
			contactEdit.Name = "ContactEdit";
			phoneLabel = new ZLabel();
			phoneTextBox = new ZTextBox();
			phoneTextBox.Name = "PhoneTextBox";
			contactNameBreakDownLabel = new ZLabel();

			addressEdit.ShowDescriptionBox = contactEdit.ShowDescriptionBox = false;
			addressEdit.PreBoundMaxLength = contactEdit.PreBoundMaxLength = 29;
			addressEdit.Location = ControlDpiScalingHelper.NewScaledPoint(7, 7);
			contactEdit.Location = ControlDpiScalingHelper.NewScaledPoint(7, 2);

			phoneLabel.AutoSize = true;
			var xValue = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(contactEdit.Location.Y + contactEdit.Height) + 8;
			phoneLabel.Location = ControlDpiScalingHelper.NewScaledPoint(7, xValue);
			phoneLabel.Text = "Ph.:";

			var locationX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(phoneLabel.Location.X) + 29;
			var locationY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(phoneLabel.Location.Y) - 4;
			phoneTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(locationX, locationY);
			phoneTextBox.Size = ControlDpiScalingHelper.NewScaledSize(184, 20);

			contactNameBreakDownLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			contactNameBreakDownLabel.Name = "ContactNameBreakDownLabel";
			contactNameBreakDownLabel.Location = ControlDpiScalingHelper.NewScaledPoint(7, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(phoneTextBox.Location.Y + phoneTextBox.Height) + 4);
			contactNameBreakDownLabel.Size = ControlDpiScalingHelper.NewScaledSize(217, 40);
			contactNameBreakDownLabel.Text = ContactNameBreakDown;
			contactNameBreakDownLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			contactNameBreakDownLabel.UseMnemonic = false;

			MoveAddressTabControlsDown();
			HideContactInfoTabControls();

			AddressTab.Controls.Add(addressEdit);
			ContactInfoTab.Controls.Add(contactEdit);
			ContactInfoTab.Controls.Add(phoneLabel);
			ContactInfoTab.Controls.Add(phoneTextBox);
			ContactInfoTab.Controls.Add(contactNameBreakDownLabel);

			MissingResourceStringChecker.ExcludeFromTest(addressEdit);
			MissingResourceStringChecker.ExcludeFromTest(contactEdit);
			MissingResourceStringChecker.ExcludeFromTest(phoneTextBox);
		}

		void MoveAddressTabControlsDown()
		{
			SuspendLayout();
			try
			{
				foreach (Control control in AddressTab.Controls)
				{
					ControlDpiScalingHelper.SetTop(control, control.Top + AdditionalHeight, false);
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void HideContactInfoTabControls()
		{
			SuspendLayout();
			try
			{
				foreach (Control control in ContactInfoTab.Controls)
				{
					control.Visible = false;
					control.VisibleChanged += new EventHandler(ContactInfoControl_VisibleChanged);
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void ContactInfoControl_VisibleChanged(object sender, EventArgs e)
		{
			Control control = sender as Control;
			if (control != null && control.Visible)
			{
				control.Visible = false;
			}
		}

		protected int AdditionalHeight
		{
			get { return addressEdit != null ? addressEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10) : 0; }
		}

		protected override int DetailsTabControlHeight
		{
			get { return base.DetailsTabControlHeight + AdditionalHeight; }
		}

		ZGuidDropEditWithFixedWidth addressEdit;
		ZDropEditWithFixedWidth contactEdit;
		ZLabel phoneLabel;
		ZTextBox phoneTextBox;
		ZLabel contactNameBreakDownLabel;

		#endregion

		#region Binding

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string AddressBindTo { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string ContactBindTo { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string ContactBindToList { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string PhoneBindTo { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				contactEdit.BindToList =
					string.IsNullOrEmpty(ContactBindToList) ?
					GetOrgHeaderBindingMember(dataSource, dataMember) + "+ContactsActive" :
					BindingHelper.GetDataMemberBeforeBindTo(BindTo, ContactBindToList);
				addressEdit.BindToList = GetOrgHeaderBindingMember(dataSource, dataMember) + "+Addresses";
			}
			addressEdit.SetDataBinding(dataSource, dataSource == null ? "" : BindingHelper.GetNestedControlDataMember(BindTo, dataMember, AddressBindTo));
			contactEdit.SetDataBinding(dataSource, dataSource == null ? "" : BindingHelper.GetNestedControlDataMember(BindTo, dataMember, ContactBindTo));
			phoneTextBox.SetDataBinding(dataSource, dataSource == null ? "" : BindingHelper.GetNestedControlDataMember(BindTo, dataMember, PhoneBindTo));
			base.SetDataBinding(dataSource, dataMember);
			OrganisationBindingManger = dataSource == null ? null : BindingContext[dataSource, new KBindingMemberInfo(dataMember).BindingPath];
		}

		BindingManagerBase OrganisationBindingManger
		{
			get { return organisationBindingManger; }
			set
			{
				if (organisationBindingManger != null)
				{
					organisationBindingManger.CurrentChanged -= new EventHandler(OrganisationBindingManger_CurrentChanged);
				}
				this.organisationBindingManger = value;
				if (organisationBindingManger != null)
				{
					organisationBindingManger.CurrentChanged += new EventHandler(OrganisationBindingManger_CurrentChanged);
				}
				UpdateUSOrganisation();
			}
		}
		BindingManagerBase organisationBindingManger;

		void OrganisationBindingManger_CurrentChanged(object sender, EventArgs e)
		{
			UpdateUSOrganisation();
		}

		void UpdateUSOrganisation()
		{
			USOrganisation = (OrganisationBindingManger == null || OrganisationBindingManger.Position == -1) ? null : OrganisationBindingManger.GetCurrent() as USOrganisation;
			SetAddressLabels();
		}

		internal USOrganisation USOrganisation
		{
			get { return (usOrganisation == null || usOrganisation.IsDeleted) ? null : usOrganisation; }
			private set
			{
				if (USOrganisation != null)
				{
					USOrganisation.ZO_OH_OrganisationInfo.ValueChanged -= new EventHandler(ZO_OH_OrganisationInfo_ValueChanged);
					USOrganisation.ZO_OA_AddressInfo.ValueChanged -= new EventHandler(ZO_OA_AddressInfo_ValueChanged);
					USOrganisation.ZO_ContactInfo.ValueChanged -= new EventHandler(ZO_ContactInfo_ValueChanged);
				}
				usOrganisation = value;
				if (USOrganisation != null)
				{
					USOrganisation.ZO_OH_OrganisationInfo.ValueChanged += new EventHandler(ZO_OH_OrganisationInfo_ValueChanged);
					USOrganisation.ZO_OA_AddressInfo.ValueChanged += new EventHandler(ZO_OA_AddressInfo_ValueChanged);
					USOrganisation.ZO_ContactInfo.ValueChanged += new EventHandler(ZO_ContactInfo_ValueChanged);
				}
			}
		}
		USOrganisation usOrganisation;

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (ContactInfoTab != null && ContactInfoTab.Controls != null)
			{
				foreach (Control control in ContactInfoTab.Controls)
				{
					control.VisibleChanged -= new EventHandler(ContactInfoControl_VisibleChanged);
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Updating Address Labels

		#region Org Changed event

		public event EventHandler OrgChanged;

		void OnOrgChanged()
		{
			if (OrgChanged != null)
			{
				OrgChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		void ZO_OH_OrganisationInfo_ValueChanged(object sender, EventArgs e)
		{
			SetOrgOnControlFromBusiness();
			OnOrgChanged();
		}

		void ZO_OA_AddressInfo_ValueChanged(object sender, EventArgs e)
		{
			SetAddressLabels();
		}

		void ZO_ContactInfo_ValueChanged(object sender, EventArgs e)
		{
			SetAddressLabels();
		}

		void SetOrgOnControlFromBusiness()
		{
			OrganisationForBinding = USOrganisation.Organisation;
		}

		string ContactNameBreakDown
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				if (!FirstName.IsEmpty)
				{
					result.Append(FirstName + System.Environment.NewLine);
				}

				if (!MiddleName.IsEmpty)
				{
					result.Append(MiddleName + System.Environment.NewLine);
				}

				if (!LastName.IsEmpty)
				{
					result.Append(LastName);
				}

				return result.ToString();
			}
		}

		ZString FirstName
		{
			get { return "First: " + ((USOrganisation != null) ? USOrganisation.FirstName : ZString.Empty); }
		}

		ZString MiddleName
		{
			get { return "Mid.: " + ((USOrganisation != null) ? USOrganisation.MiddleName : ZString.Empty); }
		}

		ZString LastName
		{
			get { return "Last: " + ((USOrganisation != null) ? USOrganisation.LastName : ZString.Empty); }
		}

		#endregion

		protected override OrgAddress OrgAddress => (USOrganisation != null && USOrganisation.IsValid) ? USOrganisation.Address : null;

		protected override ZString OrgFullNameCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_CompanyNameOverride.IsEmpty ? OrgAddress.OA_CompanyNameOverride : base.OrgFullNameCore; }
		}

		protected override ZString OrgAddress1Core
		{
			get { return OrgAddress != null && !OrgAddress.OA_Address1.IsEmpty ? OrgAddress.OA_Address1 : base.OrgAddress1Core; }
		}

		protected override ZString OrgAddress2Core
		{
			get { return OrgAddress != null && !OrgAddress.OA_Address1.IsEmpty ? OrgAddress.OA_Address2 : base.OrgAddress2Core; }
		}

		protected override ZString OrgCityCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Address1.IsEmpty ? OrgAddress.OA_City : base.OrgCityCore; }
		}

		protected override ZString OrgStateCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Address1.IsEmpty ? OrgAddress.OA_State : base.OrgStateCore; }
		}

		protected override ZString OrgPostcodeCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Address1.IsEmpty ? OrgAddress.OA_PostCode : base.OrgPostcodeCore; }
		}

		protected override ZString OrgUNLOCOCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_RL_NKRelatedPortCode.IsEmpty ? OrgAddress.OA_RL_NKRelatedPortCode : base.OrgUNLOCOCore; }
		}

		protected override ZString OrgPhoneCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Phone.IsEmpty ? OrgAddress.OA_Phone_Formatted : base.OrgPhoneCore; }
		}

		protected override ZString OrgMobileCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Mobile.IsEmpty ? OrgAddress.OA_Mobile_Formatted : base.OrgMobileCore; }
		}

		protected override ZString OrgFaxCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Fax.IsEmpty ? OrgAddress.OA_Fax_Formatted : base.OrgFaxCore; }
		}

		protected override ZString OrgEmailCore
		{
			get { return OrgAddress != null && !OrgAddress.OA_Email.IsEmpty ? OrgAddress.OA_Email : base.OrgEmailCore; }
		}

		protected override void SetAddressLabels()
		{
			base.SetAddressLabels();
			contactNameBreakDownLabel.Text = ContactNameBreakDown;
		}
	}
}
