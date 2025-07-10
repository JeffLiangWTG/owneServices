using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI.Internal
{
	/// <summary>
	/// A control for editing JobDocAddress records.
	/// SetDataBinding() takes a dataSource and dataMember that point to JobDocAddress.OrganisationPK.
	/// For example, SetDataBinding(shipment, "ConsigneeDocAddress.OrganisationPK");
	/// </summary>
	internal class ZDocAddressOrganisationControl : ZOrganisationControl, IZAddressParent
	{
		public ZDocAddressOrganisationControl()
		{
			HideLinks();
			AddAddressAndContactControls();

			AddressBindTo = "";
			ContactBindTo = "";
			ContactBindToList = "";
			AdditionalAddressInfoBindTo = "";

			InitializeExtensions();

			ContactsLink.CaptionResourceString = Res.GetData("ZDocAddressOrganisationControl|3dc1c5f7-92ca-4c54-9796-b4489a7f1041", "Transport");
			AddressEdit.AllowOutsideOfParent();
			ContactEdit.AllowOutsideOfParent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				AdditionalAddressInfoTab.TabVisible = OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.Value;
			}
		}

		#region Adding new drop lists, hiding links, and resizing to fit

		void HideLinks()
		{
			AddressesLink.Visible = false;
			ContactsLink.Visible = false;
		}

		void AddAddressAndContactControls()
		{
			AddressEdit = new ZAddressDropEdit.Bare();
			ContactEdit = new ZDropEditWithFixedWidth.Bare();

			AddressEdit.ShowDescriptionBox = ContactEdit.ShowDescriptionBox = false;
			AddressEdit.PreBoundMaxLength = ContactEdit.PreBoundMaxLength = 34;
			AddressEdit.Location = ContactEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7);
			if (!DesignModeFinder.IsDesigning && Env.Instance.Registry.EnableAddressValidationWebService)
			{
				AddressEdit.SelectedIndexChanged += AddressEdit_SelectedIndexChanged;
				AddressEdit.MouseUp += AddressEdit_SelectedIndexChanged;
			}
			MoveControlsDown(AddressTab, ContactInfoTab);

			AddressTab.Controls.Add(AddressEdit);
			ContactInfoTab.Controls.Add(ContactEdit);
		}

		void AddressEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			Invalidate();
		}

		void MoveControlsDown(params ZTabPage[] tabs)
		{
			SuspendLayout();
			try
			{
				foreach (ZTabPage tab in tabs)
				{
					foreach (Control control in tab.Controls)
					{
						ControlDpiScalingHelper.SetTop(control, control.Top + AdditionalHeight, false);
					}
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		int AdditionalHeight
		{
			get
			{
				return AddressEdit != null ? AddressEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10) : 0;
			}
		}

		protected override int DetailsTabControlHeight
		{
			get { return base.DetailsTabControlHeight + AdditionalHeight - ControlDpiScalingHelper.ScaleToCurrentDpiY(10); }
		}

		public ZAddressDropEdit.Bare AddressEdit;
		public ZDropEditWithFixedWidth.Bare ContactEdit;

		#endregion

		#region Binding

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string AddressBindTo { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string ContactBindTo { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		public string ContactBindToList { get; set; }

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string AdditionalAddressInfoBindTo { get; set; }

		public void ResetBindingContext()
		{
			BindingContext = new BindingContext();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				ContactEdit.BindToList =
					string.IsNullOrEmpty(ContactBindToList) ?
					GetOrgHeaderBindingMember(dataSource, dataMember) + "+ContactsActive" :
					BindingHelper.GetDataMemberBeforeBindTo(BindTo, ContactBindToList);
				AddressEdit.BindToList = GetOrgHeaderBindingMember(dataSource, dataMember) + "+Address_List";
			}
			string dataMemberWithoutOrgPK = dataMember.Replace(".OrganisationPK", "").Replace("OrganisationPK", "");
			AddressEdit.SetDataBinding(dataSource, dataSource == null ? "" : new KBindingMemberInfo(dataMemberWithoutOrgPK, AddressBindTo).BindingMember);
			ContactEdit.SetDataBinding(dataSource, dataSource == null ? "" : new KBindingMemberInfo(dataMemberWithoutOrgPK, ContactBindTo).BindingMember);
			AdditionalAddressInfoControl.SetDataBinding(dataSource, dataSource == null ? "" : new KBindingMemberInfo(dataMemberWithoutOrgPK, AdditionalAddressInfoBindTo).BindingMember);
			base.SetDataBinding(dataSource, dataMember);
			DocAddressBindingManger = dataSource == null ? null : BindingContext[dataSource, new KBindingMemberInfo(dataMember).BindingPath];
		}

		BindingManagerBase DocAddressBindingManger
		{
			get { return docAddressBindingManger; }
			set
			{
				if (docAddressBindingManger != null)
				{
					docAddressBindingManger.CurrentChanged -= new EventHandler(DocAddressBindingManger_CurrentChanged);
				}
				docAddressBindingManger = value;
				if (docAddressBindingManger != null)
				{
					docAddressBindingManger.CurrentChanged += new EventHandler(DocAddressBindingManger_CurrentChanged);
				}
				UpdateDocAddress();
			}
		}

		BindingManagerBase docAddressBindingManger;

		void DocAddressBindingManger_CurrentChanged(object sender, EventArgs e)
		{
			UpdateDocAddress();
		}

		void UpdateDocAddress()
		{
			DocAddress = (DocAddressBindingManger == null || DocAddressBindingManger.Position == -1) ? null : DocAddressBindingManger.GetCurrent() as JobDocAddress;
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

		void OrganisationPKInfo_ValueChanged(object sender, EventArgs e)
		{
			SetOrgOnControlFromBusiness();
			OnOrgChanged();
		}

		void SetOrgOnControlFromBusiness()
		{
			OrganisationForBinding = DocAddress?.Organisation;
		}

		protected override ZString OrgAddress1Core
		{
			get { return (DocAddress != null) ? DocAddress.E2_Address1 : ZString.Empty; }
		}

		protected override ZString OrgAddress2Core
		{
			get { return (DocAddress != null) ? DocAddress.E2_Address2 : ZString.Empty; }
		}

		protected override ZString OrgCityCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_City : ZString.Empty; }
		}

		protected override ZString OrgStateCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_State : ZString.Empty; }
		}

		protected override ZString OrgPostcodeCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Postcode : ZString.Empty; }
		}

		protected override ZString OrgUNLOCOCore
		{
			get { return (DocAddress != null && DocAddress.Address != null) ? DocAddress.Address.OA_RL_NKRelatedPortCode : ZString.Empty; }
		}

		protected override ZString OrgFullNameCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_CompanyName : ZString.Empty; }
		}

		protected override ZString OrgEmailCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Email : ZString.Empty; }
		}

		protected override ZString OrgFaxCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Fax_Formatted : ZString.Empty; }
		}

		protected override ZString OrgMobileCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Mobile_Formatted : ZString.Empty; }
		}

		protected override ZString OrgPhoneCore
		{
			get { return (DocAddress != null) ? DocAddress.E2_Phone_Formatted : ZString.Empty; }
		}

		#endregion

		#region Hooking Organisation, OrgAddress and AddressOverride changes

		protected override OrgAddress OrgAddress => DocAddress?.Address;

		internal JobDocAddress DocAddress
		{
			get { return (docAddress == null || docAddress.IsDeleted) ? null : docAddress; }
			set
			{
				UnhookEvents();
				docAddress = value;

				if (ParentAddressControl != null)
				{
					ParentAddressControl.SetCompactAddressAndCompactFullNameLabels();
					ParentAddressControl.SetCompactContactDetailsLabel();
				}

				if (DocAddress != null)
				{
					HookEvents();

					SetAddressLabels();
					if (ParentAddressControl != null)
					{
						ParentAddressControl.SetOverrideAddressVisibility(DocAddress.CanOverride);
						ParentAddressControl.UpdateControlLayout();
						ParentAddressControl.RefreshValidationStatus();
					}
				}
			}
		}
		JobDocAddress docAddress;

		void UnhookEvents()
		{
			if (DocAddress != null)
			{
				DocAddress.OrganisationPKInfo.ValueChanged -= new EventHandler(OrganisationPKInfo_ValueChanged);
				DocAddress.E2_OA_AddressInfo.ValueChanged -= new EventHandler(E2_OA_AddressInfo_ValueChanged);
				DocAddress.E2_AddressOverrideInfo.ValueChanged -= new EventHandler(E2_AddressOverrideInfo_ValueChanged);
				DocAddress.E2_GovRegNumTypeInfo.ValueChanged -= new EventHandler(E2_GovRegNumTypeInfo_ValueChanged);
				DocAddress.E2_ContactInfo.ValueChanged -= new EventHandler(E2_ContactInfo_ValueChanged);
				DocAddress.OnDeleting -= new EventHandler(DocAddress_OnDeleting);
				DocAddress.E2_ValidationStatusInfo.ValueChanged -= E2_ValidationStatusInfo_ValueChanged;
				DocAddress.E2_RN_NKCountryCodeInfo.ValueChanged -= E2_RN_NKCountryCodeInfo_ValueChanged;
			}
		}

		void HookEvents()
		{
			DocAddress.OrganisationPKInfo.ValueChanged += new EventHandler(OrganisationPKInfo_ValueChanged);
			DocAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(E2_OA_AddressInfo_ValueChanged);
			DocAddress.E2_AddressOverrideInfo.ValueChanged += new EventHandler(E2_AddressOverrideInfo_ValueChanged);
			DocAddress.E2_GovRegNumTypeInfo.ValueChanged += new EventHandler(E2_GovRegNumTypeInfo_ValueChanged);
			DocAddress.E2_ContactInfo.ValueChanged += new EventHandler(E2_ContactInfo_ValueChanged);
			DocAddress.OnDeleting += new EventHandler(DocAddress_OnDeleting);
			DocAddress.E2_ValidationStatusInfo.ValueChanged += E2_ValidationStatusInfo_ValueChanged;
			DocAddress.E2_RN_NKCountryCodeInfo.ValueChanged += E2_RN_NKCountryCodeInfo_ValueChanged;
		}

		void E2_RN_NKCountryCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentAddressControl != null)
			{
				ParentAddressControl.RefreshValidationStatus();
			}
		}

		void E2_ValidationStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentAddressControl != null)
			{
				ParentAddressControl.RefreshValidationStatus();
			}
		}

		void E2_ContactInfo_ValueChanged(object sender, EventArgs e)
		{
			SetContactTab(!OrganisationIsEmptyOrNoCurrent);
			ParentAddressControl.SetCompactContactDetailsLabel();
		}

		void E2_GovRegNumTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ParentAddressControl.UpdateDetailsVisibility();
		}

		void DocAddress_OnDeleting(object sender, EventArgs e)
		{
			UnhookEvents();
		}

		void E2_OA_AddressInfo_ValueChanged(object sender, EventArgs e)
		{
			SetAddressLabels();
			GroupBox.Invalidate();
			if (ParentAddressControl != null)
			{
				ParentAddressControl.RefreshValidationStatus();
				ParentAddressControl.SetCompactAddressAndCompactFullNameLabels();
				ParentAddressControl.SetCompactContactDetailsLabel();
			}
		}

		#endregion

		#region Swapping GroupBoxes

		void E2_AddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			ParentAddressControl.UpdateControlLayout();
		}

		internal ZDocAddressControl ParentAddressControl
		{
			get { return (ZDocAddressControl)Parent; }
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		protected override CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			CompileTimeCheckBindingMemberCollection result = new CompileTimeCheckBindingMemberCollection();
			result.AddRange(base.GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember));
			if (!string.IsNullOrEmpty(AddressBindTo))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(ZGuid), BindingHelper.GetNestedControlDataMember(BindTo, dataMember, AddressBindTo)));
			}
			if (!string.IsNullOrEmpty(AdditionalAddressInfoBindTo))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(ZGuid), BindingHelper.GetNestedControlDataMember(BindTo, dataMember, AdditionalAddressInfoBindTo)));
			}
			if (!string.IsNullOrEmpty(ContactBindTo))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(ZGuid), BindingHelper.GetNestedControlDataMember(BindTo, dataMember, ContactBindTo)));
			}
			if (!string.IsNullOrEmpty(ContactBindToList))
			{
				result.Add(new ZCompileTimeCheckBindingMember(dataSourceType, typeof(OrgContactCollection), BindingHelper.GetNestedControlDataMember(BindTo, dataMember, ContactBindToList)));
			}
			return result;
		}

		#endregion

		#region IZAddressParent

		ZGuid IZAddressParent.ParseCode(string code)
		{
			if (DocAddress != null && DocAddress.Organisation != null)
			{
				foreach (OrgAddress address in DocAddress.Organisation.AddressesNoAutoCreate)
				{
					if (address.OA_Code == code)
					{
						return address.PK;
					}
				}
			}

			return ZGuid.Invalid;
		}

		void IZAddressParent.SetControlSize() { }

		bool IZAddressParent.CheckZAddressBindingSuffix => false;

		#endregion

		#region InitializeExtensions

		void InitializeExtensions()
		{
			if (AddressEdit.GetExtension<INotificationExtension>() == null)
			{
				AddressEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
			if (ContactEdit.GetExtension<INotificationExtension>() == null)
			{
				ContactEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
		}

		#endregion

		#region SetAddressTabIrrelevant

		public void SetAddressTabRelevant(ZBool relevant)
		{
			AddressTab.TabRelevant = relevant;
		}

		#endregion

		#region SetContactInfoTabIrrelevant

		public void SetContactInfoTabTabRelevant(ZBool relevant)
		{
			ContactInfoTab.TabRelevant = relevant;
		}

		#endregion

		#region SetContactInfoTabIrrelevant

		public void SetOrganisationDescriptionBoxVisibility(ZBool visible)
		{
			OrganisationFindBox.ShowDescriptionBox = visible;
		}

		#endregion

	}
}
