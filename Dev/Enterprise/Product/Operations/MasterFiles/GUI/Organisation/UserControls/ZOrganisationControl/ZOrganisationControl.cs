using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// A control for selecting an organisation.
	/// SetDataBinding() takes a dataSource and dataMember that point to the organisation.
	/// For example, SetDataBinding(shipment, "JS_OH_Consignee");
	/// </summary>
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZOrganisationControl : ZUserControl,
		IFetchHintGenerator,
		IBindingMemberForCompileTimeCheckProvider,
		IExtendedControl,
		IDontNeedExtraSpaceForLabel,
		IBindTo,
		IVariableLengthCaptionRenderer,
		IDynamicLayoutAddressControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "Baseline")]
		public ZOrganisationControl()
		{
			InitializeComponent();
			InitializeExtensions();

			GroupBox.SetResourceStringIdentifyingControl(this);

			ContactsLink.TabStop = false;
			AddressesLink.TabStop = false;

			if (!DesignModeFinder.IsDesigning)
			{
				UserIdleWorker.QueueWorkItem(this, (NoResString)"Setting contact/addresses link images", 0, new MethodInvoker(delegate
				{
					try
					{
						ResourceManager resources = new ResourceManager(typeof(ZOrganisationControl).Namespace + ".Organisation.UserControls.ZOrganisationControl.ZOrganisationControlIcons", typeof(ZOrganisationControl).Assembly);
						ContactsLink.Image = (System.Drawing.Image)resources.GetObject("ContactsLink.Image");
						AddressesLink.Image = (System.Drawing.Image)resources.GetObject("AddressesLink.Image");
					}
					catch (ArgumentException)
					{
						// Parameter is not valid. Possibly caused by OOM/GDI object leak/faulty .NET installation/bad codec/video driver/???.
						// Or sometimes the Resources in resx fail to load
					}
				}));

				AddressLabel.AllowOutsideOfParent();
				WebLabel.AllowOutsideOfParent();
			}

#if DEBUG
			addressLabelProvider = TypeDescriptor.AddAttributes(AddressLabel, new SuppressFormsLocalizedTestAttribute());
			faxLabelProvider = TypeDescriptor.AddAttributes(FaxLabel, new SuppressFormsLocalizedTestAttribute());
			emailLabelProvider = TypeDescriptor.AddAttributes(EmailLabel, new SuppressFormsLocalizedTestAttribute());
			webLinkProvider = TypeDescriptor.AddAttributes(WebLink, new SuppressFormsLocalizedTestAttribute());
			webLabelProvider = TypeDescriptor.AddAttributes(WebLabel, new SuppressFormsLocalizedTestAttribute());
			fullNameProvider = TypeDescriptor.AddAttributes(FullNameLabel, new SuppressFormsLocalizedTestAttribute());
			phoneLabelProvider = TypeDescriptor.AddAttributes(PhoneLabel, new SuppressFormsLocalizedTestAttribute());
			mobileLabelProvider = TypeDescriptor.AddAttributes(MobileLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		[Browsable(false)]
		public ZGuidFindBox OrganisationFindBox
		{
			get { return fOrganisationFindBox; }
		}

		public bool ReadOnly
		{
			get { return OrganisationFindBox.ReadOnly; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AdditionalAddressInfoTab.TabVisible = false;
		}

		#region Organisation Messages

		internal static class OrganisationMessages
		{
			public static string NoFullNameFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoFullNameFoundOnFile", "*NO NAME FOUND*"); } }
			public static string NoAddressFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoAddressFoundOnFile", "*NO ADDRESS FOUND*"); } }
			public static string NoPhoneFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoPhoneFoundOnFile", "Ph: *NOT FOUND*"); } }
			public static string NoMobileFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoMobileFoundOnFile", "Mob: *NOT FOUND*"); } }
			public static string NoFaxFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoFaxFoundOnFile", "Fax: *NOT FOUND*"); } }
			public static string NoEmailFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoEmailFoundOnFile", "Em: *NOT FOUND*"); } }
			public static string NoWebFoundOnFile { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoWebFoundOnFile", "Web: *NOT FOUND*"); } }
			public static string NoOrgIsSelected { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.NoOrgIsSelected", "* NO ORGANIZATION IS SELECTED"); } }
			public static string OrgIsNull { get { return Res.GetString("ZOrganisationControl.OrganisationMessages.OrgIsNull", "* Organization is null, cannot show form"); } }
		}

		#endregion

		#region Details

		[Category(ZGUIConstants.DesignerCategory), DefaultValue(OrganisationDetails.All)]
		public OrganisationDetails Details
		{
			get { return fDetails; }
			set
			{
				if (fDetails != value)
				{
					fDetails = value;

					if (fDetails == OrganisationDetails.All)
					{
						DetailsTabControl.Visible = true;
						FullNameOnlyLabel.Visible = false;
					}
					else if (fDetails == OrganisationDetails.FullName)
					{
						DetailsTabControl.Visible = false;
						FullNameOnlyLabel.Visible = true;
					}
					else
					{
						DetailsTabControl.Visible = false;
						FullNameOnlyLabel.Visible = false;
					}

					SetControlSize();
				}
			}
		}
		OrganisationDetails fDetails = OrganisationDetails.All;

		#endregion

		#region Caption

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				GroupBox.Text = value;
			}
		}

		public string[] Captions
		{
			get { return ((IVariableLengthCaptionRenderer)GroupBox).Captions; }
			set { ((IVariableLengthCaptionRenderer)GroupBox).Captions = value; }
		}

		public bool IsCaptionOverridden
		{
			get { return ((IVariableLengthCaptionRenderer)GroupBox).IsCaptionOverridden; }
			set { ((IVariableLengthCaptionRenderer)GroupBox).IsCaptionOverridden = value; }
		}

		[Conditional("DEBUG")]
		public void ExcludeGroupBoxCaptionFromMissingResourceStringCheckerTest()
		{
			MissingResourceStringChecker.ExcludeFromTest(GroupBox);
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return GroupBox.CaptionResourceString; }
			set { GroupBox.CaptionResourceString = value; }
		}

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		#endregion

		#region Popup Caption

		[Category(ZGUIConstants.DesignerCategory)]
		public string PopupCaption
		{
			get { return OrganisationFindBox.PopupCaption; }
			set { OrganisationFindBox.PopupCaption = value; }
		}

		#endregion

		#region Bind to Organisations

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindToList")]
		[ExcludeFromBindToAttributesTest]
		public string BindToOrganisations
		{
			get { return OrganisationFindBox.BindToList; }
			set { OrganisationFindBox.BindToList = value; }
		}

		[DefaultValue(AddressType.MainAddress)]
		public AddressType AddressType { get; set; }

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		public void CorrectBindToOrgList(string prefix)
		{
			if (!BindToOrganisations.StartsWith(prefix, StringComparison.Ordinal))
			{
				BindToOrganisations = prefix + BindToOrganisations;
			}
		}

		#endregion

		#region Set Control Size

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			SetControlSize();
		}

		protected void SetControlSize()
		{
			if (AddressesLink != null && GroupBox != null && DetailsTabControl != null)
			{
				SuspendLayout();
				try
				{
					const int RightGap = 2;
					ControlDpiScalingHelper.SetWidth(this, AddressesLink.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(RightGap), false);

					if (DetailsTabControl.Visible)
					{
						ControlDpiScalingHelper.SetHeight(ref DetailsTabControl, DetailsTabControlHeight, false);
					}

					if (!IsCustomHeight && this.Height != AdjustedHeight)
					{
						ControlDpiScalingHelper.SetHeight(this, AdjustedHeight, false);
					}

					ControlDpiScalingHelper.SetHeight(ref GroupBox, this.Height, false);
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		[DefaultValue(false)]
		public bool IsCustomHeight
		{
			get;
			set;
		}

		protected const int BottomGap = 1;

		protected virtual int AdjustedHeight
		{
			get
			{
				int result;

				if (Details == OrganisationDetails.All)
				{
					result = DetailsTabControl.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(BottomGap);
				}
				else if (Details == OrganisationDetails.FullName)
				{
					result = FullNameOnlyLabel.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(BottomGap);
				}
				else
				{
					result = OrganisationFindBox.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2 + BottomGap);
				}

				return result;
			}
		}

		protected virtual int DetailsTabControlHeight
		{
			get { return ControlDpiScalingHelper.ScaleToCurrentDpiY(112); }
		}

		#endregion

		#region Updating Address Labels

		protected virtual bool IsUpdateAddressLabelsRequired()
		{
			if (DetailsTabControl.SelectedTab != lastTabPageForAddressLabels ||
				Organisation != lastOrganization)
			{
				lastTabPageForAddressLabels = DetailsTabControl.SelectedTab;
				lastOrganization = Organisation;
				return true;
			}

			return false;
		}
		ZTabPage lastTabPageForAddressLabels;
		IOrgHeader lastOrganization;

		protected internal void UpdateAddressLabelsIfRequired()
		{
			if (IsUpdateAddressLabelsRequired())
			{
				SetAddressLabels();
			}
		}

		internal void UpdateAddressLabels()
		{
			SetAddressLabels();
		}

		protected virtual void SetAddressLabels()
		{
			bool isValidOrg = Organisation != null;

			if (Details == OrganisationDetails.All)
			{
				SetContactTab(isValidOrg);
				SetAddressTab(isValidOrg);
				SetAdditionalAddressTab(isValidOrg);
			}
			else if (Details == OrganisationDetails.FullName)
			{
				SetFullName(FullNameOnlyLabel, isValidOrg);
			}

			ContactsLink.Enabled = isValidOrg;
			AddressesLink.Enabled = isValidOrg;
		}

		void SetAdditionalAddressTab(bool isValidOrg)
		{
			if (!isValidOrg)
			{
				AdditionalAddressInfoControl.AdditionalInfoLabel.Text = OrganisationMessages.NoOrgIsSelected;
			}
			else
			{
				AdditionalAddressInfoControl.AdditionalInfoLabel.Text = string.Empty;
			}

			AdditionalAddressInfoControl.AdditionalInfoLabel.Enabled = isValidOrg;
		}

		protected void SetContactTab(bool isValidOrg)
		{
			if (isValidOrg)
			{
				PhoneLabel.Text = OrgPhone;
				MobileLabel.Text = OrgMobile;
				FaxLabel.Text = OrgFax;
				EmailLabel.Text = OrgEmail;

				string webAddress = OrgWeb.Trim();
				bool isValidWeb = (webAddress != OrganisationMessages.NoWebFoundOnFile);

				if (isValidWeb)
				{
					WebLabel.Text = (NoResString)"Web: ";
					WebLink.Text = webAddress;
					WebLink.Links.Clear();
					WebLink.Links.Add(0, webAddress.Length, webAddress);
					WebLink.Visible = true;
				}
				else
				{
					WebLabel.Text = webAddress;
					WebLink.Visible = false;
				}
			}
			else
			{
				PhoneLabel.Text = OrganisationMessages.NoOrgIsSelected;
				WebLink.Visible = false;
			}

			FaxLabel.Visible = isValidOrg;
			EmailLabel.Visible = isValidOrg;
			WebLabel.Visible = isValidOrg;
			MobileLabel.Visible = isValidOrg;

			PhoneLabel.Enabled = isValidOrg;
		}

		void SetAddressTab(bool isValidOrg)
		{
			AddressLabel.MouseHover -= new EventHandler(AddressLabel_MouseHover);

			SetFullName(FullNameLabel, isValidOrg);

			if (isValidOrg)
			{
				AddressLabel.Text = OrgAddressFormattedForAddressLabel;

				if (IsAddressLabelToolTipRequired)
				{
					AddressLabel.MouseHover += new EventHandler(AddressLabel_MouseHover);
				}
			}

			AddressLabel.Visible = isValidOrg;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		protected void AddressLabel_MouseHover(object sender, EventArgs e)
		{
			if (ToolTipService.GetToolTip(AddressLabel) != OrgAddressFormatted)
			{
				ToolTipService.SetToolTip(AddressLabel, OrgAddressFormatted);
			}
		}

		void SetFullName(ZLabel label, bool isValidOrg)
		{
			label.Text = isValidOrg ? OrgFullName : OrganisationMessages.NoOrgIsSelected;

			label.Enabled = isValidOrg;
			label.IsFontBold = isValidOrg;
		}

		#region Org Details

		string OrgFullName
		{
			get { return OrgFullNameCore.IsEmpty ? OrganisationMessages.NoFullNameFoundOnFile : (string)OrgFullNameCore; }
		}

		protected virtual OrgAddress OrgAddress => ShowCustomsAddress ? (OrgAddress)CustomsAddress : (OrgAddress)Organisation.MainAddress;

		protected internal string OrgAddressFormatted
		{
			get
			{
				var orgAddressFormatted = string.Empty;

				if (Organisation != null)
				{
					var addressFormatter = OrgAddressFormatter?.Invoke(Organisation.Factory, OrgAddress) ?? new AddressFormatter(Organisation.Factory, OrgAddress);
					orgAddressFormatted = addressFormatter.PostalAddressWithoutCompanyName();
				}

				return string.IsNullOrEmpty(orgAddressFormatted) ? OrganisationMessages.NoAddressFoundOnFile : orgAddressFormatted;
			}
		}

		public Func<BusinessObjectFactory, OrgAddress, AddressFormatter> OrgAddressFormatter
		{
			get;
			set;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected bool IsAddressLabelToolTipRequired => OrgAddressFormattedForAddressLabel != OrgAddressFormatted;

		protected string OrgAddressFormattedForAddressLabel => FormatOrgAddressForAddressLabel(OrgAddressFormatted, AddressLabel);

		public static string FormatOrgAddressForAddressLabel(string orgAddress, ZLabel label)
		{
			var orgAddressFormattedForAddressLabel = "";
			var isTooManyLines = false;
			var linesToDisplay = orgAddress.SplitByLine();
			var linesToDisplayCount = linesToDisplay.Count();

			if (linesToDisplayCount > 5)
			{
				isTooManyLines = true;
				linesToDisplay = linesToDisplay.Take(5);
				linesToDisplayCount = linesToDisplay.Count();
			}

			var currentLineIndex = 0;
			foreach (var line in linesToDisplay)
			{
				var lineToDisplay = line.TruncateToFit(label.Font, label.Width);

				if (currentLineIndex != linesToDisplayCount - 1)
				{
					lineToDisplay = lineToDisplay + "\n";
				}
				else if (isTooManyLines)
				{
					lineToDisplay = (lineToDisplay == line) ? line + "..." : lineToDisplay;
				}

				orgAddressFormattedForAddressLabel += lineToDisplay;
				currentLineIndex++;
			}

			return orgAddressFormattedForAddressLabel;
		}

		string OrgPhone
		{
			get
			{
				return OrgPhoneCore.IsEmpty
					? OrganisationMessages.NoPhoneFoundOnFile
					: Res.GetString("c96c3cbc-f49a-4bc9-9aa1-529525a68dce", "Ph: {0}", OrgPhoneCore);
			}
		}

		string OrgMobile
		{
			get
			{
				return OrgMobileCore.IsEmpty
					? OrganisationMessages.NoMobileFoundOnFile
					: Res.GetString("832e859e-0018-4faa-8f33-b8270d28ac59", "Mob: {0}", OrgMobileCore);
			}
		}

		string OrgFax
		{
			get
			{
				return OrgFaxCore.IsEmpty
					? OrganisationMessages.NoFaxFoundOnFile
					: Res.GetString("d2bbda5c-25fe-4352-a968-6275d5fb6de7", "Fax: {0}", OrgFaxCore);
			}
		}

		string OrgEmail
		{
			get
			{
				return OrgEmailCore.IsEmpty
					? OrganisationMessages.NoEmailFoundOnFile
					: Res.GetString("5bff9735-0c83-42d8-9b71-ab4af909b70d", "Em: {0}", OrgEmailCore);
			}
		}

		string OrgWeb
		{
			get { return OrgWebCore.IsEmpty ? OrganisationMessages.NoWebFoundOnFile : (string)OrgWebCore; }
		}

		#region Implementation

		IOrgAddress CustomsAddress
		{
			get { return OrganisationForBinding == null ? null : OrganisationForBinding.CustomsAddress; }
		}

		bool ShowCustomsAddress
		{
			get { return AddressType == AddressType.CustomsAddress && CustomsAddress != null; }
		}

		protected virtual ZString OrgFullNameCore
		{
			get { return Organisation.FullName; }
		}

		protected virtual ZString OrgAddress1Core
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_Address1 : Organisation.Address1; }
		}

		protected virtual ZString OrgAddress2Core
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_Address2 : Organisation.Address2; }
		}

		protected virtual ZString OrgCityCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_City : Organisation.City; }
		}

		protected virtual ZString OrgStateCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_State : Organisation.State; }
		}

		protected virtual ZString OrgPostcodeCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_PostCode : Organisation.Postcode; }
		}

		protected virtual ZString OrgUNLOCOCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_RL_NKRelatedPortCode : Organisation.UNLOCO; }
		}

		protected virtual ZString OrgPhoneCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_Phone_Formatted : Organisation.Phone_Formatted; }
		}

		protected virtual ZString OrgMobileCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_Mobile_Formatted : Organisation.Mobile_Formatted; }
		}

		protected virtual ZString OrgFaxCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_Fax_Formatted : Organisation.Fax_Formatted; }
		}

		protected virtual ZString OrgEmailCore
		{
			get { return ShowCustomsAddress ? CustomsAddress.OA_Email : Organisation.Email; }
		}

		protected virtual ZString OrgWebCore
		{
			get { return Organisation.Web; }
		}

		#endregion

		#endregion

		#endregion

		#region Link Button Clicks

		void ContactsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			IOrganisationController controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
			if ((BusinessObject)Organisation != null)
			{
				controller.ShowForm((BusinessObject)Organisation, OrganisationTabPages.Contacts, FormAction.Edit);
			}
			else
			{
				Globals.Message.ShowError(OrganisationMessages.OrgIsNull);
			}
		}

		void AddressesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			IOrganisationController controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
			controller.ShowForm((BusinessObject)Organisation, OrganisationTabPages.Address, FormAction.Edit);
		}

		void WebLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string target = e.Link.LinkData as string;

			if (target != null)
			{
				ShowWebLink(target);
			}
		}

		protected void ShowWebLink(string target)
		{
			WebUrlLauncher.Launch(target);
		}

		#endregion

		#region IFetchHintGenerator Members
		void IFetchHintGenerator.AddFetchHint(object dataSource, string dataMember)
		{
			if ((string.IsNullOrEmpty(dataMember) || dataMember == BindTo) &&
				!BindTo.Contains(".") && !BindTo.Contains("+"))
			{
				BusinessObject bizO = dataSource as BusinessObject;
				if (bizO != null)
				{
					try
					{
						ZGuid organisationGuid = (ZGuid)bizO[BindTo];
						bizO.Factory.AddFetchHint(OrgHeaderSchema.PK, organisationGuid);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var message = FormattableString.Invariant($@"{ex.Message}
Control Path: {ControlDescription.GetControlPath(this)}");
						ErrorReporter.ReportOnce("Exception occurred whilst calculating fetch hint - Automatic fetch hint ignored", message, ex);
					}
				}
			}
		}
		#endregion

		#region IDataBoundControl

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataBindings.RemoveBinding(nameof(OrganisationForBinding));
				OrganisationFindBox.SetDataBinding(null, "");
			}

			this.dataSource = dataSource;
			this.dataMember = dataMember;
			OrganisationFindBox.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				string relatedBizObjBindingMember = GetOrgHeaderBindingMember(DataSource, dataMember);
				DataBindings.Add(new KBinding(nameof(OrganisationForBinding), dataSource, relatedBizObjBindingMember, false, DataSourceUpdateMode.Never));
			}
		}

		protected override object DataSourceCore
		{
			get { return dataSource; }
		}
		object dataSource;

		protected override string DataMemberCore
		{
			get { return dataMember; }
		}
		string dataMember;

		protected string GetOrgHeaderBindingMember(object dataSource, string fullOrgHeaderPKMember)
		{
			KBindingMemberInfo bindingMember = new KBindingMemberInfo(fullOrgHeaderPKMember);
			BindingManagerBase bindingManager = BindingContext[dataSource, bindingMember.BindingPath];

			PropertyDescriptor property = bindingManager.GetItemProperties()[bindingMember.BindingField];
			string relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(property)
				?? throw new ArgumentNullException(nameof(fullOrgHeaderPKMember), "Unable to locate the [RelatedBusinessObject()] attribute on property '" + BindTo + "'.");
			return new KBindingMemberInfo(bindingMember.BindingPath, relatedBizObjName ?? new KBindingMemberInfo(fullOrgHeaderPKMember).BindingField).BindingMember;
		}

		public override Type DataSourceType
		{
			get { return typeof(ZGuid); }
		}

		#endregion

		#region Organisation

		IOrgHeader Organisation
		{
			get { return OrganisationIsEmptyOrNoCurrent ? null : OrganisationForBinding; }
		}

		protected bool OrganisationIsEmptyOrNoCurrent
		{
			get
			{
				return
					OrganisationForBinding == null ||
					((BusinessObject)OrganisationForBinding).IsNull;
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual IOrgHeader OrganisationForBinding
		{
			get
			{
				IOrgHeader result = fOrganisationForBinding;
				IBusiness dataSource = this.DataSource as IBusiness;
				if (result == null && dataSource != null)
				{
					result = (IOrgHeader)dataSource.Factory.GetNull(ObjectFactory.GetType<IOrgHeader>());
				}
				return result;
			}
			set
			{
				IOrgHeader previousOrg = Organisation;

				if (previousOrg != null)
				{
					previousOrg.AddressChanged -= new EventHandler(OrganisationAddressChanged);
				}

				fOrganisationForBinding = value;

				IOrgHeader currentOrg = Organisation;
				if (currentOrg != null)
				{
					currentOrg.AddressChanged += new EventHandler(OrganisationAddressChanged);
				}

				OnOrganisationForBindingChanged();
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			UpdateAddressLabelsIfRequired();
		}

		void OrganisationAddressChanged(object sender, EventArgs e)
		{
			UpdateAddressLabelsIfRequired();
		}

		void OnOrganisationForBindingChanged()
		{
			if (OrganisationForBindingChanged != null)
			{
				OrganisationForBindingChanged(this, EventArgs.Empty);
			}
			UpdateAddressLabelsIfRequired();
		}

		public event EventHandler OrganisationForBindingChanged;
		IOrgHeader fOrganisationForBinding;

		#endregion

		#region IBindTo Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (Organisation != null)
				{
					Organisation.AddressChanged -= new EventHandler(OrganisationAddressChanged);
				}

#if DEBUG
				TypeDescriptor.RemoveProvider(addressLabelProvider, AddressLabel);
				TypeDescriptor.RemoveProvider(faxLabelProvider, FaxLabel);
				TypeDescriptor.RemoveProvider(emailLabelProvider, EmailLabel);
				TypeDescriptor.RemoveProvider(webLinkProvider, WebLink);
				TypeDescriptor.RemoveProvider(webLabelProvider, WebLabel);
				TypeDescriptor.RemoveProvider(fullNameProvider, FullNameLabel);
				TypeDescriptor.RemoveProvider(phoneLabelProvider, PhoneLabel);
				TypeDescriptor.RemoveProvider(mobileLabelProvider, MobileLabel);
#endif

				if (components != null)
				{
					components.Dispose();
				}

				Extensions.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

#if DEBUG
		readonly TypeDescriptionProvider addressLabelProvider, faxLabelProvider, emailLabelProvider, webLinkProvider, webLabelProvider, fullNameProvider, phoneLabelProvider, mobileLabelProvider;
#endif

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZOrganisationControl>()
	.Property("ReadOnly", false, false)
	.Property<BusinessObject>("OrganisationForBinding", null)
	.Property("IsVisibleForBinding", ZBool.True)
	.Result;
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider Members

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			return GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember);
		}

		protected virtual CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			return new CompileTimeCheckBindingMemberCollection();
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);

			if (OrganisationFindBox.GetExtension<INotificationExtension>() == null)
			{
				OrganisationFindBox.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
		}

		#endregion
	}

	#region Organisation Details

	public enum OrganisationDetails
	{
		None,
		FullName,
		All
	}

	#endregion

	#region Address Details

	public enum AddressType
	{
		MainAddress,
		CustomsAddress
	}

	#endregion
}
