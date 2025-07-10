using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public enum ZDocAddressControlDisplayMode
	{
		ShowOverrideAndTabs = 0,
		HideOverrideAndTabs = 1,
		HideOverrideShowTabs = 2,
		SingleLineNoOverride = 3,
		SingleLineNoOverrideNoGroupBox = 4,
		HideOverrideAndAddressTab = 5,
		HideContactInfoTab = 6,
		Compact = 7,
		CompactWithOverride = 8,
		CompactWithContactTab = 9
	}

	/// <summary>
	/// A control for editing JobDocAddress records.
	/// SetDataBinding() takes a dataSource and dataMember that points to the JobDocAddress business object.
	/// For example, SetDataBinding(shipment, "ConsigneeDocAddress");
	/// </summary>
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class ZDocAddressControl : ZUserControl,
		IExtendedControl,
		IDontNeedExtraSpaceForLabel,
		IZAddressParent,
		IBindTo,
		IReadOnlyToggleControl,
		ISupportWebAddressValidationControl,
		IDynamicLayoutAddressControl
	{
		#region Construction

		public ZDocAddressControl()
		{
			InitializeComponent();

			OverrideGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(defaultControlWidth, 182, true);
			DefaultGroupBox.SetResourceStringIdentifyingControl(this);
			CutDownGroupBox.SetResourceStringIdentifyingControl(this);
			CutDownSingleLineGroupBox.SetResourceStringIdentifyingControl(this);
			OverrideGroupBox.SetResourceStringIdentifyingControl(this);
			CompactLayoutGroupBox.SetResourceStringIdentifyingControl(this);
			CompactOverrideLayoutGroupBox.SetResourceStringIdentifyingControl(this);

			DefaultGroupBox.Location = Point.Empty;
			CutDownGroupBox.Location = Point.Empty;
			CutDownSingleLineGroupBox.Location = Point.Empty;
			SingleLineNoGroupBoxPanel.Location = Point.Empty;
			SingleLineNoGroupBoxOverridePanel.Location = Point.Empty;
			OverrideGroupBox.Location = Point.Empty;
			CompactLayoutGroupBox.Location = Point.Empty;
			CompactOverrideLayoutGroupBox.Location = Point.Empty;
			CompactLayoutNameAndAddressPanel.Location = Point.Empty;

			// Tab index is hardcoded here as it is diabolically difficult to set
			DefaultGroupBox.TabIndex = 0;
			OverrideAddressCheckbox.TabIndex = 1;
			OverrideGroupBox.TabIndex = 2;
			SetControlSize();
			InitializeExtensions();

			((IVariableLengthCaptionRenderer)CutDownGroupBox).IsCaptionOverridden = false;
			((IVariableLengthCaptionRenderer)CutDownSingleLineGroupBox).IsCaptionOverridden = false;
			((IVariableLengthCaptionRenderer)OverrideGroupBox).IsCaptionOverridden = false;
			((IVariableLengthCaptionRenderer)CompactLayoutGroupBox).IsCaptionOverridden = false;
			((IVariableLengthCaptionRenderer)CompactOverrideLayoutGroupBox).IsCaptionOverridden = false;
			overrideAddressShouldBeVisible = true;

			LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DefaultGroupBox, false);
			convertToOrganizationButton.ToolTipCaption = ResString.GetMultilingualString("7147CC0A-C25E-400C-B19C-58CC99895602", "Convert To Organization");
			enableAddressValidationWebService = false;
			if (!DesignModeFinder.IsDesigning)
			{
				enableAddressValidationWebService = Env.Instance.Registry.EnableAddressValidationWebService;
				if (enableAddressValidationWebService)
				{
					cancellationToken = new CancellationTokenSource();
					this.HandleCreated += (o, e) =>
					{
						if (this.ParentForm != null)
						{
							this.ParentForm.FormClosed += (x, y) =>
							{
								if (cancellationToken != null)
								{
									cancellationToken.Cancel();
								}
							};
						}
					};
					DefaultGroupBox.VisibleChanged += DefaultGroupBox_VisibleChanged;
					HookISupportWebAddressValidationControlChangeFocusEvents();
				}
				else
				{
					ValidateAddressButton.Visible = false;
					SetAddressValidationStatusButtonVisibility(false);
				}

				if (ShouldSetOverrideToReadOnly)
				{
					HookReadOnlyChangeEvent();
				}

				CompanyTextBox.ManuallySetCaptionToolTip(Res.GetString("89f236bd-4ec4-41df-baea-42a0f0b8732a", "Company Name"));
				AdditionalAddressInformationTextBox.ManuallySetCaptionToolTip(Res.GetString("a001377e-2c31-4782-a84b-5caddcc64611", "Note"));
				AddressLine1TextBox.ManuallySetCaptionToolTip(Res.GetString("244a9975-610b-4dbb-b184-9262b87df3c7", "Address"));
				PostCodeTextBox.ManuallySetCaptionToolTip(Res.GetString("d62011da-366f-45fe-96e9-aac6e6c6fafa", "Postcode"));
				CountryFindBox.ManuallySetCaptionToolTip(Res.GetString("7bc4f6f7-2832-408d-baba-b702f2eb02c3", "Country/Region Code"));
				CompanyTextBox.PlaceHolderText = Res.GetString("cd6a6e1d-e11a-4247-9247-1721d07889d7", "Company Name");
				AdditionalAddressInformationTextBox.PlaceHolderText = Res.GetString("7f14a1a3-b846-487e-92fe-dff2ba68dfb1", "E.g. Leave at Reception");
				AddressLine1TextBox.PlaceHolderText = Res.GetString("3869fa7c-56dc-4a77-bf96-1f21d6d85bbe", "Address Line 1");
				AddressLine2TextBox.PlaceHolderText = Res.GetString("a0db7f2d-2fb6-42c2-97a1-fe99caf9f862", "Address Line 2");
				PostCodeTextBox.PlaceHolderText = Res.GetString("a0516fcd-8091-47f9-982a-99210c57cc4a", "Postcode");

				DefaultGroupBox.AllowOverlap(OverrideGroupBox);
				OverrideAddressCheckbox.AllowOutsideOfParent();
				OverrideAddressCheckbox.AllowOverlap(OverrideGroupBox);
				OverrideAddressCheckbox.AllowOverlap(DefaultGroupBox);
				OverrideAddressCheckbox.AllowOverlap(CompactLayoutGroupBox);
				OverrideAddressCheckbox.AllowOverlap(CompactOverrideLayoutGroupBox);
				ValidateAddressButton.AllowOverlap(DetailsTabControl);
				ValidateAddressButton.AllowOverlap(CompactLayoutGroupBox);
				convertToOrganizationButton.AllowOverlap(DetailsTabControl);
				AddressTypeDropEdit.AllowOverlap(AdditionalAddressInformationTextBox);
				CompactAddressDropEdit.AllowOverlap(OverrideAddressCheckbox);
				AddressValidationStatusButton.AllowOverlap(OverrideAddressCheckbox);
				CompactOverrideTabControl.AllowOverlap(OverrideAddressCheckbox);
				CompactLayoutNameAndAddressPanel.AllowOverlap(OverrideAddressCheckbox);
				DefaultGroupBox.AllowOverlap(convertToOrganizationButton);
				CutDownSingleLineNoGroupBoxAddressDropEdit.AllowOutsideOfParent();
			}
			ValidationJustForced = false;
		}

		protected ZDropEditWithFixedWidth AddressTypeDropEdit;
		internal ZGroupBox CompactLayoutGroupBox;
		internal ZOrganisationFindBox.Bare CompactOrganizationFindBox;
		internal ZAddressDropEdit.Bare CompactAddressDropEdit;
		internal ZPanel CompactLayoutNameAndAddressPanel;
		internal ZLabel CompactAddressLabel;
		internal ZLabel CompactFullNameLabel;
		readonly bool enableAddressValidationWebService;

		#endregion

		#region IReadOnlyToggleControl Members

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				AddressValidationStatusButton.ReadOnly = value;
				ClearFieldsButton.ReadOnly = value;
			}
		}
		bool readOnly;
		#endregion

		#region OrgChanged

		public event EventHandler OrgChanged
		{
			add { DefaultGroupBox.OrgChanged += value; }
			remove { DefaultGroupBox.OrgChanged -= value; }
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
			var expectedHeight = IsCustomHeight ? Height : ControlHeight;

			if (Width != ControlWidth || Height != expectedHeight)
			{
				this.Size = ControlDpiScalingHelper.NewScaledSize(ControlWidth, expectedHeight, false);
			}

			if (DefaultGroupBox.Height != Height)
			{
				ControlDpiScalingHelper.SetHeight(ref DefaultGroupBox, Height, false);
			}
			if (OverrideGroupBox.Height != Height)
			{
				ControlDpiScalingHelper.SetHeight(ref OverrideGroupBox, Height, false);
			}
		}

		public int ControlWidth
		{
			get
			{
				switch (DisplayMode)
				{
					case ZDocAddressControlDisplayMode.SingleLineNoOverride:
						return CutDownSingleLineGroupBox.Width;
					case ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox:
						return SingleLineNoGroupBoxPanel.Width;
					case ZDocAddressControlDisplayMode.Compact:
					case ZDocAddressControlDisplayMode.CompactWithContactTab:
						return CompactLayoutGroupBox.Width;
					case ZDocAddressControlDisplayMode.CompactWithOverride:
						return CompactOverrideLayoutGroupBox.Width;
					default:
						return ControlDpiScalingHelper.ScaleToCurrentDpiX(defaultControlWidth);
				}
			}
		}
		internal const int defaultControlWidth = 251;

		public int ControlHeight
		{
			get
			{
				switch (DisplayMode)
				{
					case ZDocAddressControlDisplayMode.HideOverrideAndTabs:
						return ControlDpiScalingHelper.ScaleToCurrentDpiY(HideOverrideAndTabsHeight);
					case ZDocAddressControlDisplayMode.SingleLineNoOverride:
						return CutDownSingleLineGroupBox.Height;
					case ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox:
						return SingleLineNoGroupBoxPanel.Height;
					case ZDocAddressControlDisplayMode.Compact:
					case ZDocAddressControlDisplayMode.CompactWithContactTab:
						return CompactLayoutGroupBox.Height;
					case ZDocAddressControlDisplayMode.CompactWithOverride:
						return CompactOverrideLayoutGroupBox.Height;
					default:
						return ControlDpiScalingHelper.ScaleToCurrentDpiY(MaxControlHeight);
				}
			}
		}

		[DefaultValue(false)]
		public bool IsCustomHeight
		{
			get { return DefaultGroupBox.IsCustomHeight; }
			set { DefaultGroupBox.IsCustomHeight = value; }
		}

		public int SingleLineNoGroupBoxPanelWidth
		{
			get { return ControlDpiScalingHelper.UnscaleFromCurrentDpiX(SingleLineNoGroupBoxPanel.Width); }
			set
			{
				if (DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox)
				{
					var scaledValue = ControlDpiScalingHelper.ScaleToCurrentDpiX(value);
					var widthToChange = scaledValue - SingleLineNoGroupBoxPanel.Width;
					ControlDpiScalingHelper.SetWidth(ref SingleLineNoGroupBoxPanel, scaledValue, false);
					CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength = 0;
					((ZDropEditInternals)CutDownSingleLineNoGroupBoxAddressDropEdit).SetControlWidth(CutDownSingleLineNoGroupBoxAddressDropEdit.Width + widthToChange);

					ControlDpiScalingHelper.SetWidth(ref SingleLineNoGroupBoxOverridePanel, scaledValue, false);
					ControlDpiScalingHelper.SetWidth(ref CutDownSingleLineNoGroupBoxOrgOverrideTextBox, scaledValue, false);

					CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width = CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
					CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Width = CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
				}
			}
		}

		[DefaultValue(false)]
		[Category(ZGUIConstants.DesignerCategory)]
		public bool ShowCompanyName
		{
			get { return showCompanyName; }
			set { showCompanyName = value; }
		}
		bool showCompanyName;

		public const int MaxControlHeight = 182;
		public const int HideOverrideAndTabsHeight = 68;
		ZFilterModule organizationModule;
		#endregion

		#region ShowResidentialAddressOnOverride

		[DefaultValue(false)]
		[Category(ZGUIConstants.DesignerCategory)]
		public bool ShowResidentialAddressOnOverride { get; set; }

		#endregion

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DocAddress?.Validation.ValidateE2_OA_Address();
		}

		#endregion

		#region Ctrl-O Shortcut

		public void SetOverrideAddressVisibility(bool isVisible)
		{
			overrideAddressShouldBeVisible = isVisible;
		}
		bool overrideAddressShouldBeVisible;

		[DefaultValue(ZDocAddressControlDisplayMode.ShowOverrideAndTabs)]
		[Category(ZGUIConstants.DesignerCategory)]
		public ZDocAddressControlDisplayMode DisplayMode
		{
			get { return displayMode; }
			set
			{
				displayMode = value;
				UpdateControlLayout();
			}
		}
		ZDocAddressControlDisplayMode displayMode;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.O))
			{
				if (OverrideAddressCheckbox.Visible && !OverrideAddressCheckbox.ReadOnly)
				{
					OverrideAddressCheckbox.Checked = !OverrideAddressCheckbox.Checked;
					return true;
				}
			}
			if (((ISupportWebAddressValidationControl)this).ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		bool ISupportWebAddressValidationControl.ProcessManuallyVerifyShortCutKey
			(ref Message msg, Keys keyData)
		{
			if (ShouldValidateAddress() && SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, ReadOnly, this, DocAddress))
			{
				return true;
			}
			return false;
		}

		#endregion

		#region Synchronising the GroupBox Captions

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				DefaultGroupBox.Text = value;
				CutDownGroupBox.Text = value;
				CutDownSingleLineGroupBox.Text = value;
				OverrideGroupBox.Text = value;
				CompactLayoutGroupBox.Text = value;
				CompactOverrideLayoutGroupBox.Text = value;
			}
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return DefaultGroupBox.CaptionResourceString; }
			set
			{
				DefaultGroupBox.CaptionResourceString = value;
				CutDownGroupBox.CaptionResourceString = value;
				CutDownSingleLineGroupBox.CaptionResourceString = value;
				OverrideGroupBox.CaptionResourceString = value;
				CompactLayoutGroupBox.CaptionResourceString = value;
				CompactOverrideLayoutGroupBox.CaptionResourceString = value;
			}
		}

		public void SetLabelCaptionVisible(bool value)
		{
			LabelCaptionRenderProvider.SetLabelCaptionVisible(DefaultGroupBox, value);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(CutDownGroupBox, value);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(CutDownSingleLineGroupBox, value);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(OverrideGroupBox, value);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(CompactLayoutGroupBox, value);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(CompactOverrideLayoutGroupBox, value);
		}

		#endregion

		#region Updating the Control Layout (based on DisplayMode or clicking "Override Address")

		bool ShouldSetOverrideToReadOnly => !Env.Security.JobDocAddressOverride.IsAllowed;

		void HookReadOnlyChangeEvent()
		{
			CompanyTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			AddressLine1TextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			AddressLine2TextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			CityTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			CityTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			PostCodeTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;

			ContactTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			EMailTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			PhoneNumberControl.NumberTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			MobilePhoneNumberControl.NumberTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
			FaxNumberControl.NumberTextBox.ReadOnlyChanged += TextBox_ReadOnlyChanged;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				UpdateControlLayout();
			}
		}

		protected virtual void SetAddressTabRelevant(ZBool relevant)
		{
			DefaultGroupBox.SetAddressTabRelevant(relevant);
		}

		protected virtual void SetOrganisationDescriptionBoxVisibility(ZBool visible)
		{
			DefaultGroupBox.SetOrganisationDescriptionBoxVisibility(visible);
		}

		protected virtual void SetAddressValidationStatusButtonVisibility(ZBool visible)
		{
			AddressValidationStatusButton.Visible = visible;
		}

		internal void UpdateControlLayout()
		{
			var docAddress = this.DocAddress;
			bool hasCurrentDocAddress = (docAddress != null);

			if (hasCurrentDocAddress)
			{
				SuspendLayout();
				try
				{
					if (docAddress.E2_AddressOverride)
					{
						StoreOrgAddress();
					}
					else
					{
						RestoreOrgAddress();
					}

					if (DisplayMode == ZDocAddressControlDisplayMode.HideOverrideAndTabs)
					{
						CutDownGroupBox.Visible = true;

						DefaultGroupBox.Visible = false;
						OverrideAddressCheckbox.Visible = false;
						CutDownSingleLineGroupBox.Visible = false;
						SingleLineNoGroupBoxPanel.Visible = false;
						SingleLineNoGroupBoxOverridePanel.Visible = false;
						CompactLayoutGroupBox.Visible = false;
						CompactOverrideLayoutGroupBox.Visible = false;
						CompactWithContactTabControl.Visible = false;
						SetAddressValidationStatusButtonVisibility(false);
					}
					else if (DisplayMode == ZDocAddressControlDisplayMode.HideOverrideShowTabs)
					{
						CutDownSingleLineGroupBox.Visible = false;
						SingleLineNoGroupBoxPanel.Visible = false;
						SingleLineNoGroupBoxOverridePanel.Visible = false;
						OverrideAddressCheckbox.Visible = false;
						CompactLayoutGroupBox.Visible = false;
						CompactOverrideLayoutGroupBox.Visible = false;
						CompactWithContactTabControl.Visible = false;
						DefaultGroupBox.Visible = !docAddress.E2_AddressOverride;
						OverrideGroupBox.Visible = docAddress.E2_AddressOverride;

						if (!DesignModeFinder.IsDesigning && ShouldValidateAddress())
						{
							SetAddressValidationStatusButtonVisibility(!docAddress.E2_AddressOverride);
						}

						if (ShouldSetOverrideToReadOnly)
						{
							SetOverrideToReadOnly();
						}
					}
					else if (DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverride)
					{
						ShowCutDownSingleLineGroupBox();
					}
					else if (DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox)
					{
						ShowCutDownSingleLineNoGroupBox();
					}
					else if (DisplayMode == ZDocAddressControlDisplayMode.HideOverrideAndAddressTab)
					{
						SetAddressTabRelevant(false);
						OverrideGroupBox.Visible = false;
						OverrideAddressCheckbox.Visible = false;
						CompactWithContactTabControl.Visible = false;
						SetAddressValidationStatusButtonVisibility(false);
						SetOrganisationDescriptionBoxVisibility(true);
						MatchOrganisationFindBoxWidthWithDetailTabControlWidth();
					}
					else if (DisplayMode == ZDocAddressControlDisplayMode.Compact)
					{
						OverrideAddressCheckbox.Visible = false;
						ShowCompactLayout();
					}
					else if (DisplayMode == ZDocAddressControlDisplayMode.CompactWithOverride || DisplayMode == ZDocAddressControlDisplayMode.CompactWithContactTab)
					{
						if (docAddress.E2_AddressOverride)
						{
							ShowCompactOverrideLayout();
						}
						else
						{
							var hasContactTab = DisplayMode == ZDocAddressControlDisplayMode.CompactWithContactTab;
							ShowCompactLayout(hasContactTab);
						}
						ShowAndRelocateOverrideCheckboxForCompactLayout();
						MoveValidateAddressButtonForCompactWithContactTabLayout();
					}
					else
					{
						if (DisplayMode == ZDocAddressControlDisplayMode.HideContactInfoTab)
						{
							DefaultGroupBox.SetContactInfoTabTabRelevant(false);
							ContactTabPage.TabVisible = false;
						}
						UpdateGovtRegistrationAndResidentialControls();

						CutDownSingleLineGroupBox.Visible = false;
						SingleLineNoGroupBoxPanel.Visible = false;
						SingleLineNoGroupBoxOverridePanel.Visible = false;
						CompactLayoutGroupBox.Visible = false;
						CompactOverrideLayoutGroupBox.Visible = false;
						DefaultGroupBox.Visible = !docAddress.E2_AddressOverride;
						OverrideGroupBox.Visible = docAddress.E2_AddressOverride;
						CompactWithContactTabControl.Visible = false;

						OverrideAddressCheckbox.Visible = overrideAddressShouldBeVisible;

						if (ShouldSetOverrideToReadOnly)
						{
							SetOverrideToReadOnly();
						}
					}
				}
				finally
				{
					ResumeLayout();
				}
			}
			else
			{
				if (DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverride)
				{
					ShowCutDownSingleLineGroupBox();
				}
				else if (DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox)
				{
					ShowCutDownSingleLineNoGroupBox();
				}
				else
				{
					bool cutDownGroupBoxVisible = DisplayMode == ZDocAddressControlDisplayMode.HideOverrideAndTabs;
					CutDownGroupBox.Visible = cutDownGroupBoxVisible;
					CutDownSingleLineGroupBox.Visible = false;
					SingleLineNoGroupBoxPanel.Visible = false;
					SingleLineNoGroupBoxOverridePanel.Visible = false;
					DefaultGroupBox.Visible = !cutDownGroupBoxVisible;
					OverrideGroupBox.Visible = !cutDownGroupBoxVisible;
					CompactLayoutGroupBox.Visible = false;
					CompactOverrideLayoutGroupBox.Visible = false;
					CompactWithContactTabControl.Visible = false;
					OverrideAddressCheckbox.Visible = !cutDownGroupBoxVisible && overrideAddressShouldBeVisible;
					SetAddressValidationStatusButtonVisibility(false);
					if (DisplayMode is ZDocAddressControlDisplayMode.Compact
						or ZDocAddressControlDisplayMode.CompactWithContactTab
						or ZDocAddressControlDisplayMode.CompactWithOverride)
					{
						OverrideAddressCheckbox.Visible = false;
						ShowCompactLayout();
					}
				}
				SetControlSize();
			}
			UpdateDetailsVisibility();
		}

		void ShowCompactOverrideLayout()
		{
			CutDownSingleLineGroupBox.Visible = false;
			CutDownGroupBox.Visible = false;
			DefaultGroupBox.Visible = false;
			OverrideGroupBox.Visible = false;
			CompactLayoutGroupBox.Visible = false;
			CompactOverrideLayoutGroupBox.Visible = true;
			CompactOverrideTabControl.Visible = true;
			CompactWithContactTabControl.Visible = false;
			CompactWithContactTabControl.Location = Point.Empty;
			CompactOverrideLayoutGroupBox.BringToFront();
		}

		void ShowCompactLayout(bool hasContactTab = false)
		{
			CutDownSingleLineGroupBox.Visible = false;
			CutDownGroupBox.Visible = false;
			DefaultGroupBox.Visible = false;
			OverrideGroupBox.Visible = false;
			CompactOverrideLayoutGroupBox.Visible = false;
			CompactLayoutGroupBox.Visible = true;
			CompactLayoutNameAndAddressPanel.Visible = true;
			CompactWithContactTabControl.Location = Point.Empty;
			SetAddressValidationStatusButtonVisibility(true);
			ShowAndRelocateAddressValidationButtonForCompactLayout();
			CompactAddressDropEdit.SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(120));

			if (hasContactTab)
			{
				CompactWithContactTabControl.Visible = true;

				CompactLayoutPanel.Controls.Add(CompactWithContactTabControl);
				CompactAddressTab.Controls.Add(CompactLayoutNameAndAddressPanel);

				CompactWithContactTabControl.Dock = DockStyle.Fill;

				ControlDpiScalingHelper.SetHeight(ref CompactLayoutNameAndAddressPanel, CompactAddressTab.Height, false);
				ControlDpiScalingHelper.SetHeight(ref CompactAddressLabel, CompactAddressTab.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(12), false);

				CompactLayoutNameAndAddressPanel.Location = Point.Empty;
				RelocateCompactAddressInfoLabels();
			}
			else
			{
				CompactLayoutPanel.Controls.Add(CompactLayoutNameAndAddressPanel);
				CompactLayoutNameAndAddressPanel.Location = ControlDpiScalingHelper.NewScaledPoint(3, 0, false);
				RelocateCompactAddressInfoLabels();
				CompactWithContactTabControl.Visible = false;
			}
		}

		void RelocateCompactAddressInfoLabels()
		{
			ControlDpiScalingHelper.SetTop(ref CompactAddressLabel, CompactFullNameLabel.Bottom, false);
		}

		void ShowAndRelocateAddressValidationButtonForCompactLayout()
		{
			AddressValidationStatusButton.BringToFront();
			CompactLayoutGroupBox.Controls.Add(AddressValidationStatusButton);
			ControlDpiScalingHelper.SetLeft(ref AddressValidationStatusButton, CompactAddressDropEdit.Right, false);
			ControlDpiScalingHelper.SetTop(ref AddressValidationStatusButton, CompactAddressDropEdit.Top - ControlDpiScalingHelper.OnePixel, false);
		}

		void ShowAndRelocateOverrideCheckboxForCompactLayout()
		{
			OverrideAddressCheckbox.Visible = true;
			OverrideAddressCheckbox.BringToFront();

			if (DocAddress.E2_AddressOverride)
			{
				CompactOverrideLayoutGroupBox.Controls.Add(OverrideAddressCheckbox);

				ControlDpiScalingHelper.SetLeft(ref OverrideAddressCheckbox, CompactOverrideLayoutGroupBox.Right - OverrideAddressCheckbox.Width, false);
				ControlDpiScalingHelper.SetTop(ref OverrideAddressCheckbox, CompactOverrideLayoutGroupBox.Top, false);
			}
			else
			{
				CompactLayoutGroupBox.Controls.Add(OverrideAddressCheckbox);

				ControlDpiScalingHelper.SetLeft(ref OverrideAddressCheckbox, CompactLayoutGroupBox.Right - OverrideAddressCheckbox.Width, false);
				ControlDpiScalingHelper.SetTop(ref OverrideAddressCheckbox, CompactLayoutGroupBox.Top, false);
			}
		}

		void MoveValidateAddressButtonForCompactWithContactTabLayout()
		{
			if (DocAddress.E2_AddressOverride && DisplayMode == ZDocAddressControlDisplayMode.CompactWithContactTab)
			{
				CompactOverrideLayoutGroupBox.Controls.Add(ValidateAddressButton);
				ValidateAddressButton.Location = ControlDpiScalingHelper.NewScaledPoint(223, 13);
			}
			else
			{
				OverrideGroupBox.Controls.Add(ValidateAddressButton);
				ValidateAddressButton.Location = ControlDpiScalingHelper.NewScaledPoint(192, 17);
			}
			ValidateAddressButton.BringToFront();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "The target value is already scaled")]
		protected void MatchOrganisationFindBoxWidthWithDetailTabControlWidth()
		{
			DefaultGroupBox.OrganisationFindBox.Width = DetailsTabControl.Width;
		}

		void SetOverrideToReadOnly()
		{
			OverrideGroupBox.SetReadOnlyIncludingChildren();
			OverrideAddressCheckbox.Enabled = false;
			ClearFieldsButton.Enabled = false;
			ValidateAddressButton.Enabled = false;
			convertToOrganizationButton.Enabled = false;
		}

		void TextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (sender is ZTextBox textBox && !textBox.ReadOnly)
			{
				textBox.ReadOnly = true;
			}
		}

		void ShowCutDownSingleLineGroupBox()
		{
			CutDownSingleLineGroupBox.Visible = true;

			CutDownGroupBox.Visible = false;
			DefaultGroupBox.Visible = false;
			OverrideGroupBox.Visible = false;
			OverrideAddressCheckbox.Visible = false;
			CompactLayoutGroupBox.Visible = false;
			CompactOverrideLayoutGroupBox.Visible = false;
			SetAddressValidationStatusButtonVisibility(false);
			SingleLineNoGroupBoxPanel.Visible = false;
			SingleLineNoGroupBoxOverridePanel.Visible = false;
			CompactWithContactTabControl.Visible = false;
			CompactWithContactTabControl.Location = Point.Empty;
		}

		void ShowCutDownSingleLineNoGroupBox()
		{
			var docAddress = this.DocAddress;
			var isOverridden = docAddress != null && docAddress.E2_AddressOverride;

			SingleLineNoGroupBoxPanel.Visible = !isOverridden;
			SingleLineNoGroupBoxOverridePanel.Visible = isOverridden;

			CutDownSingleLineNoGroupBoxOrgFindBox.ShowDescriptionBox = showCompanyName;
			if (showCompanyName)
			{
				CutDownSingleLineNoGroupBoxOrgFindBox.Size = ControlDpiScalingHelper.NewScaledSize(200, 20);
				CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength = 13;
				ControlDpiScalingHelper.SetLeft(ref CutDownSingleLineNoGroupBoxAddressDropEdit, CutDownSingleLineNoGroupBoxOrgFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);

				CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Size = ControlDpiScalingHelper.NewScaledSize(99, 20);
				CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Size = ControlDpiScalingHelper.NewScaledSize(119, 20);
			}

			CutDownSingleLineGroupBox.Visible = false;
			CutDownGroupBox.Visible = false;
			DefaultGroupBox.Visible = false;
			OverrideGroupBox.Visible = false;
			CompactLayoutGroupBox.Visible = false;
			CompactOverrideLayoutGroupBox.Visible = false;
			OverrideAddressCheckbox.Visible = false;
			CompactWithContactTabControl.Visible = false;
			CompactWithContactTabControl.Location = Point.Empty;
			SetAddressValidationStatusButtonVisibility(false);
		}

		protected virtual void UpdateGovtRegistrationAndResidentialControls()
		{
			UpdateGovtRegistrationAndResidentialControlsCore();
		}

		void UpdateGovtRegistrationAndResidentialControlsCore()
		{
			bool governmentRegistrationNumberIsVisible = false;

			if (DocAddress.E2_AddressOverride && DocAddress.Requirement != null && DocAddress.Requirement.GetRegistrationNumberResult != null)
			{
				RegistrationNumberResult regNumberResult = DocAddress.Requirement.GetRegistrationNumberResult(DocAddress);

				if (regNumberResult != null)
				{
					governmentRegistrationNumberIsVisible = regNumberResult.IsApplicable;
				}
			}

			GovernmentRegistrationTabPage.TabVisible = governmentRegistrationNumberIsVisible;

			AddressTypeDropEdit.Visible = ShowResidentialAddressOnOverride;
		}

		void DetailsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDetailsVisibility();
		}

		internal protected virtual void UpdateDetailsVisibility()
		{
			JobDocAddress docAddress = DocAddress;
			GovernmentRegistrationNumberTextBox.Visible = GovernmentRegistrationTabPage.TabVisible && docAddress != null && !docAddress.IsPassportIDGovRegNumType;
			bool passportDataIsVisible = GovernmentRegistrationTabPage.TabVisible && docAddress != null && docAddress.IsPassportIDGovRegNumType;
			PassportDataEditButton.Visible = passportDataIsVisible;
			PassportDataTextBox.Visible = passportDataIsVisible;
		}

		void PassportDataEditButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(new DocAddressPassportDetailForm(DocAddress), ParentForm);
		}

		protected virtual OrgHeader GenerateTemporaryOrganization(OrgHeader inputOrganization = null)
		{
			inputOrganization = inputOrganization ?? DocAddress.Factory.New<OrgHeader>();
			inputOrganization.OH_FullName = DocAddress.E2_CompanyName.Left(OrgHeader.Schema.OH_FullNameMaxLength);

			OrgAddress mainAddress = inputOrganization.MainAddress;
			mainAddress.OA_RN_NKCountryCode = DocAddress.E2_RN_NKCountryCode;
			mainAddress.OA_CompanyNameOverride = DocAddress.E2_CompanyName.Left(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength);
			mainAddress.OA_Address1 = DocAddress.E2_Address1;
			mainAddress.OA_Address2 = DocAddress.E2_Address2;
			mainAddress.OA_City = DocAddress.E2_City;
			mainAddress.OA_PostCode = DocAddress.E2_Postcode;
			mainAddress.OA_State = DocAddress.E2_State;
			mainAddress.OA_Email = DocAddress.E2_Email;
			mainAddress.OA_Phone = DocAddress.E2_Phone;
			mainAddress.OA_Mobile = DocAddress.E2_Mobile;
			mainAddress.OA_Fax = DocAddress.E2_Fax;
			mainAddress.PrimaryOrgAddressAdditionalInfoDetail = DocAddress.E2_AdditionalAddressInformation;

			return inputOrganization;
		}

		internal void ConvertToOrganizationButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.Organisation.IsAllowed)
			{
				Env.Security.Organisation.ShowError();
			}
			else if (DocAddress != null)
			{
				OrgHeader temporaryOrganization = GenerateTemporaryOrganization();
				try
				{
					bool isLikelyDuplicate = temporaryOrganization.IsLikelyDuplicate(false);
					bool toCreateNew = false;
					if (isLikelyDuplicate)
					{
#if DEBUG
						if (Globals.IsTest)
						{
							SimilarOrganizationShown = true;
							NewOrganizationCountryCode = temporaryOrganization.MainAddress.OA_RN_NKCountryCode;
						}
						else
#endif
						{
							using (var similarOrganizationSelectionForm = new SimilarOrganizationSelectionForm(temporaryOrganization))
							{
								ZFormModaliser.ShowDialogWithoutDispose(similarOrganizationSelectionForm);
								if (similarOrganizationSelectionForm.UserDecision == ContinueWithSave.Yes)
								{
									if (similarOrganizationSelectionForm.FirstSelectedOrganization == null)
									{
										Globals.Message.ShowError(Res.GetString("E0B84EE7-CAF3-4706-ADAF-9FEDB773D894", "No organization is selected."));
									}
									else
									{
										DocAddress.E2_AddressOverride = false;
										DocAddress.OrganisationPK = similarOrganizationSelectionForm.FirstSelectedOrganization.PK;
									}
								}
								else
								{
									toCreateNew = similarOrganizationSelectionForm.ToCreateNew;
								}
							}
						}
					}
					if (!isLikelyDuplicate || toCreateNew)
					{
#if DEBUG
						if (Globals.IsTest)
						{
							NewOrganizationShown = true;
							NewOrganizationCountryCode = temporaryOrganization.MainAddress.OA_RN_NKCountryCode;
						}
						else
#endif
						{
							if (!isLikelyDuplicate)
							{
								var dialogResult = Globals.Message.Show(Res.GetString("D6EBCF60-6C15-4FEA-B5A1-EE64F632C66C", "Would you like to create an Organization using the details from the overridden address details entered?"), Res.GetString("4A1CD8DD-8412-4358-81D1-70E6148C69DE", "No similar organizations are found"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
								if (dialogResult != DialogResult.Yes)
								{
									return;
								}
							}
							organizationModule = organizationModule ?? (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation);
							organizationModule.SetFormsModalTo(FindForm());
							var form = ((IShowNewForm)organizationModule).ShowNewForm();
							OrgHeader newOrganization = ((ZForm)form).BusinessEntity as OrgHeader;
							GenerateTemporaryOrganization(newOrganization);
							form.Closed += (_, x_) =>
							{
								OrgHeader organization = ((ZForm)form).BusinessEntity as OrgHeader;
								if (organization.IsInDatabase)
								{
									DocAddress.OrganisationPK = organization.PK;
									DocAddress.E2_AddressOverride = false;
								}
							};
						}
					}
				}
				finally
				{
					temporaryOrganization.Delete();
				}
			}
		}

		void StoreOrgAddress()
		{
			if (DocAddress != null) // will be null during bind
			{
				if (PkAndOaHash.Contains(DocAddress.PK))
				{
					PkAndOaHash.Remove(DocAddress.PK);
				}

				if (!DocAddress.E2_AddressOverride)
				{
					PkAndOaHash.Add(DocAddress.PK, DocAddress.E2_OA_Address);
				}
			}
		}

		void RestoreOrgAddress()
		{
			if (DocAddress != null) // will be null during bind
			{
				if (PkAndOaHash.Contains(DocAddress.PK))
				{
					DocAddress.E2_OA_Address = (ZGuid)PkAndOaHash[DocAddress.PK];
				}
			}
		}

		protected JobDocAddress DocAddress
		{
			get { return DefaultGroupBox.DocAddress; }
		}

		Hashtable PkAndOaHash
		{
			get { return fPkAndOaHash ?? (fPkAndOaHash = new Hashtable()); }
		}

		Hashtable fPkAndOaHash;
		internal ZButton AddressValidationStatusButton;
		protected ZButton ClearFieldsButton;

		#endregion

		#region SelectFromPopupForm

		public void SelectFromPopupForm()
		{
			if (DocAddress != null && !DocAddress.E2_AddressOverride)
			{
				SelectFormPopupFormCore();
			}
		}

		void SelectFormPopupFormCore()
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				fPopupShown = true;
			}
			else
#endif
			{
				this.DefaultGroupBox.OrganisationFindBox.SelectFromPopupForm();
			}
		}

#if DEBUG

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public bool PopupShown
		{
			get { return fPopupShown; }
			set { fPopupShown = value; }
		}
		bool fPopupShown;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public bool SimilarOrganizationShown { get; private set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public bool NewOrganizationShown { get; private set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public string NewOrganizationCountryCode { get; private set; }

#endif
		#endregion

		void AddressTextBox_TextChanged(object sender, EventArgs e)
		{
			if (SuggestionWindowParentControl != null)
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(SuggestionWindowParentControl.Controls);
			}
		}

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(DocAddress, this);
		}

		void OverrideAddressCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (!OverrideAddressCheckbox.Checked)
			{
				CloseSuggestionForms();
			}
		}

		void DocAddressNotificationsChanged(object obj, EventArgs args)
		{
			if (GovernmentRegistrationTabPage.TabVisible)
			{
				var bizO = obj as BusinessObject;
				if (bizO != null && bizO.Notifications != null && bizO.Notifications.Cast<PropertyNotification>().Any(n => n.PropertyName.StartsWith("E2_", StringComparison.Ordinal)))
				{
					AddressTabPage.Text = AddressTabPage.CaptionResourceString.ShortCaption;
					ContactTabPage.Text = ContactTabPage.CaptionResourceString.ShortCaption;
					GovernmentRegistrationTabPage.Text = GovernmentRegistrationTabPage.CaptionResourceString.ShortCaption;
				}
				else
				{
					AddressTabPage.Text = AddressTabPage.CaptionResourceString.Caption;
					ContactTabPage.Text = ContactTabPage.CaptionResourceString.Caption;
					GovernmentRegistrationTabPage.Text = GovernmentRegistrationTabPage.CaptionResourceString.Caption;
				}
			}
		}

		protected virtual void CloseSuggestionForms()
		{
			if (ParentForm != null)
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(ParentForm.Controls);
				CityTownSuggestionControlHelper.CloseSuggestionForm(ParentForm.Controls);
			}
		}

		void CodeBox_TextChanged(object sender, EventArgs e)
		{
			if (DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverride ||
				DisplayMode == ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox ||
				DisplayMode == ZDocAddressControlDisplayMode.HideOverrideAndTabs ||
				DisplayMode == ZDocAddressControlDisplayMode.HideOverrideAndAddressTab ||
				string.IsNullOrEmpty(DefaultGroupBox.AddressEdit.CodeBox.Text) ||
				!ShouldValidateAddress() ||
				OverrideAddressCheckbox.Checked ||
				(DocAddress.Organisation != null && DocAddress.Organisation.IsSystemDefinedOrganisation)
				)
			{
				SetAddressValidationStatusButtonVisibility(false);
			}
			else
			{
				SetAddressValidationStatusButtonVisibility(true);
			}
		}

		#region Address Validation

#if DEBUG
		internal
#endif
		CancellationTokenSource cancellationToken;

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, DocAddress, this);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			ValidationJustForced = true;
			await ValidateAddress();
		}

		#region Address Validation

		protected virtual Control ValidationReferenceControl => ValidateButton;

		protected bool ShouldValidateAddress()
		{
			return
				DocAddress != null &&
				DocAddress.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(DocAddress.Country.PK.ToGuid(), DocAddress.ValidationSection);
		}

		protected virtual async Task ValidateAddress()
		{
			await ValidateAddress(false);
		}

		async Task ValidateAddress(bool isFirstLoad)
		{
			try
			{
				if (AddressValidationService.IsAddressNeedValidation(DocAddress))
				{
					CleanseAction cleanseAction;

					var parentForm = ParentForm;
					if (!ValidationJustForced && parentForm != null)
					{
						AddressSuggestionControlHelper.CloseSuggestionForm(parentForm.Controls);
						cleanseAction = CleanseAction.QuickValidate;
					}
					else
					{
						cleanseAction = CleanseAction.ValidateAndSuggest;
						ValidationJustForced = false;
					}

					if (parentForm != null)
					{
						await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, DocAddress, this, parentForm, parentForm.Controls, RefreshValidationStatus, null, 0, cleanseAction, ValidationReferenceControl, isFirstLoad);
					}
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This is helpful for workitem WI00183116, please contact the ROPE team. The NullReferenceException was caught from ValidateAddress().", e);
			}
		}

		protected virtual async Task GetCityTownAsync()
		{
			if (DocAddress != null && AddressLine1TextBox != null && ParentForm != null && (!string.IsNullOrEmpty(DocAddress.City) || !string.IsNullOrEmpty(DocAddress.Postcode)))
			{
				var maxWidth = AddressLine1TextBox.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, DocAddress, this, ParentForm, ParentForm.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		void DocAddress_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			RefreshValidationStatus();
			if (AddressValidationService.IsAddressInValidStatus(DocAddress))
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
			}
		}

		public void RefreshValidationStatus()
		{
			if (IsDisposing)
			{
				return;
			}

			if (DocAddress != null)
			{
				if (ShouldValidateAddress())
				{
					if (DocAddress.E2_AddressOverride)
					{
						AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, DocAddress.ValidationStatus, DocAddress.IsErrorSuppressed);
						AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, DocAddress.ValidationStatus);
						if (DocAddress.ValidationStatus == "VAD")
						{
							StateControl.Extensions?.Get<IValidationExtension>().Validate();
						}

						ValidateAddressButton.Invalidate();
						ValidateAddressButton.Refresh();
						OverrideGroupBox.Invalidate();
						ValidateAddressButton.Visible = true;
						SetAddressValidationStatusButtonVisibility(false);
					}
					else if (DocAddress.Address != null)
					{
						AddressValidationUIHelper.SetButtonValidationStatus(AddressValidationStatusButton, DocAddress.Address.ValidationStatus, DocAddress.Address.IsErrorSuppressed);
						AddressValidationUIHelper.SetAddressDropEditValidationStatus(DefaultGroupBox.AddressEdit, DocAddress.Address.ValidationStatus);
						AddressValidationStatusButton.Invalidate();
						AddressValidationStatusButton.Refresh();
						DefaultGroupBox.Invalidate();
						DefaultGroupBox.GroupBox.Invalidate();
						DefaultGroupBox.UpdateAddressLabels();

						CompactLayoutGroupBox.Invalidate();
						CompactOverrideLayoutGroupBox.Invalidate();
						SetCompactAddressAndCompactFullNameLabels();
						AddressValidationUIHelper.SetAddressDropEditValidationStatus(CompactAddressDropEdit, DocAddress.Address.ValidationStatus);

						ValidateAddressButton.Visible = false;
						SetAddressValidationStatusButtonVisibility(true);
					}

					UpdateControlLayout();
					ClearFieldsButton.Visible = ClearFieldsButtonEffective;
				}
				else
				{
					if (DocAddress.E2_AddressOverride)
					{
						AddressValidationUIHelper.ResetAddressFieldState(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl);
					}
					else if (DocAddress.Address != null)
					{
						AddressValidationUIHelper.ResetAddressDropEditState(DefaultGroupBox.AddressEdit);
						AddressValidationUIHelper.ResetAddressDropEditState(CompactAddressDropEdit);
					}

					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}
			}
		}

		protected virtual ZBool ClearFieldsButtonEffective => ZBool.True;

		#endregion

		void DefaultGroupBox_VisibleChanged(object sender, EventArgs e)
		{
			if (DefaultGroupBox.Visible)
			{
				AddressValidationStatusButton.BringToFront();
			}
		}

		protected void AddressValidationStatusButton_Click(object sender, EventArgs e)
		{
			if (DocAddress != null && DocAddress.Organisation != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Organisation);
				if (controller != null)
				{
					var form = ((IOrganisationController)controller).ShowForm(DocAddress.Organisation, OrganisationTabPages.Address, FormAction.Edit);
					var orgForm = form as ZOrganisationsForm;

					if (orgForm != null)
					{
						if (orgForm.OrganisationsTabControl.SelectedTab != orgForm.AddressesTabPage)
						{
							orgForm.OrganisationsTabControl.SelectedTab = orgForm.AddressesTabPage;
						}

						orgForm.AddressesPageControl2.OrgAddressBoundGrid.SelectSingleElement(DocAddress.Address);
						form.Closed += form_Closed;
					}
				}
			}
		}

		void form_Closed(object sender, EventArgs e)
		{
			RefreshValidationStatus();
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZDocAddressControl>().Result;
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			UnhookDocAddress();

			try
			{
				base.SetDataBinding(dataSource, dataMember);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				var controlPath = GetControlPath();
				var errorMessage = FormattableString.Invariant($"Control Path:{controlPath}\r\nName:{Name}\r\nDataSource Type:{dataSource?.GetType()}\r\nDataMember:{dataMember}\r\n");
				throw new ArgumentOutOfRangeException(errorMessage, ex);
			}

			DefaultGroupBox.SetDataBinding(dataSource, dataSource == null ? "" : new KBindingMemberInfo(dataMember, "OrganisationPK").BindingMember);
			BindCutDownControls(dataSource, dataMember);
			BindCompactControls(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);

			HookDocAddress();
			SetAddressFormatter();
			RefreshValidationStatus();
		}

		public void SetCompactAddressAndCompactFullNameLabels()
		{
			var isValidOrg = DocAddress?.Organisation != null;

			if (DefaultGroupBox.Details == OrganisationDetails.All)
			{
				CompactAddressLabel.MouseHover -= CompactAddressLabelOnMouseHover;

				if (isValidOrg)
				{
					var orgAddressFormatted = OrgAddressFormatted;
					var orgAddressFormattedForAddressLabel = ZOrganisationControl.FormatOrgAddressForAddressLabel(OrgAddressFormatted, CompactAddressLabel);

					CompactAddressLabel.Text = orgAddressFormattedForAddressLabel;

					if (orgAddressFormatted != orgAddressFormattedForAddressLabel)
					{
						CompactAddressLabel.MouseHover += CompactAddressLabelOnMouseHover;
					}
				}
			}

			if (DefaultGroupBox.Details == OrganisationDetails.All || DefaultGroupBox.Details == OrganisationDetails.FullName)
			{
				if (isValidOrg)
				{
					ZString fullName = DocAddress?.E2_CompanyName ?? ZString.Empty;
					CompactFullNameLabel.Text = !fullName.IsEmpty
						? (string)fullName
						: ZOrganisationControl.OrganisationMessages.NoFullNameFoundOnFile;
				}
				else
				{
					CompactFullNameLabel.Text = ZOrganisationControl.OrganisationMessages.NoOrgIsSelected;
				}

				CompactFullNameLabel.Enabled = isValidOrg;
				CompactFullNameLabel.IsFontBold = isValidOrg;
			}

			CompactAddressLabel.Visible = isValidOrg;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The formatted contact data is already localised")]
		public void SetCompactContactDetailsLabel()
		{
			var isValidContact = DocAddress?.Contact != null;
			var emailLabelText = ZString.Empty;
			var phoneLabelText = ZString.Empty;
			if (isValidContact)
			{
				var contact = DocAddress.Contact;
				emailLabelText = Res.GetString("A60F9C4B-99ED-498D-B70D-A01570E97355", "Em: {0}", contact.Email);
				phoneLabelText = Res.GetString("88746A46-9FB4-400B-B99C-20BC4B7B04DD", "Ph: {0}", contact.OC_Phone_Formatted);
			}

			CompactContactEmailAddressLabel.Text = emailLabelText;
			CompactContactPhoneNumberLabel.Text = phoneLabelText;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The formatted address is already localised")]
		void CompactAddressLabelOnMouseHover(object sender, EventArgs e)
		{
			if (ToolTipService.GetToolTip(CompactAddressLabel) != OrgAddressFormatted)
			{
				ToolTipService.SetToolTip(CompactAddressLabel, OrgAddressFormatted);
			}
		}

		string GetControlPath()
		{
			var controlPath = new StringBuilder();
			var parent = Parent;
			while (parent != null)
			{
				controlPath.Append((NoResString)"Name:").Append(parent.Name).Append((NoResString)" Type:").AppendLine(parent.GetType().ToString());
				if (parent is ZForm)
				{
					return controlPath.ToString();
				}
				parent = parent.Parent;
			}

			return controlPath.ToString();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			RefreshValidationStatus();
		}

		void BindCutDownControls(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				var orgBinding = DefaultGroupBox.DataBindings["OrganisationForBinding"];
				CutDownAddressDropEdit.BindToList = orgBinding.BindingMemberInfo.BindingMember + "+Addresses";
				CutDownSingleLineAddressDropEdit.BindToList = orgBinding.BindingMemberInfo.BindingMember + "+Addresses";
				CutDownSingleLineNoGroupBoxAddressDropEdit.BindToList = orgBinding.BindingMemberInfo.BindingMember + "+Addresses";
			}

			string addressBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, JobDocAddressSchema.Constants.E2_OA_Address);
			string orgBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, "OrganisationPK");

			CutDownAddressDropEdit.SetDataBinding(DataSource, addressBindingMember);
			CutDownOrganisationFindBox.SetDataBinding(DataSource, orgBindingMember);

			CutDownSingleLineAddressDropEdit.SetDataBinding(DataSource, addressBindingMember);
			CutDownSingleLineOrgFindBox.SetDataBinding(DataSource, orgBindingMember);

			CutDownSingleLineNoGroupBoxAddressDropEdit.SetDataBinding(DataSource, addressBindingMember);
			CutDownSingleLineNoGroupBoxOrgFindBox.SetDataBinding(DataSource, orgBindingMember);
		}

		void BindCompactControls(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				var orgBinding = DefaultGroupBox.DataBindings["OrganisationForBinding"];
				CompactAddressDropEdit.BindToList = orgBinding.BindingMemberInfo.BindingMember + "+Addresses";
			}

			string addressBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, JobDocAddressSchema.Constants.E2_OA_Address);
			string orgBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, "OrganisationPK");
			CompactOrganizationFindBox.SetDataBinding(DataSource, orgBindingMember);
			CompactAddressDropEdit.SetDataBinding(DataSource, addressBindingMember);

			string dataMemberWithoutOrgPK = dataMember.Replace(".OrganisationPK", "").Replace("OrganisationPK", "");
			CompactAddressDropEdit.SetDataBinding(dataSource, dataSource == null ? "" : new KBindingMemberInfo(dataMemberWithoutOrgPK, "E2_OA_Address").BindingMember);
		}

		public override Type DataSourceType
		{
			get { return typeof(JobDocAddress); }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToOrganisations
		{
			get { return DefaultGroupBox.BindToOrganisations; }
			set
			{
				DefaultGroupBox.BindToOrganisations = value;
				CutDownOrganisationFindBox.BindToList = value;
				CutDownSingleLineOrgFindBox.BindToList = value;
				CutDownSingleLineNoGroupBoxOrgFindBox.BindToList = value;
				CompactOrganizationFindBox.BindToList = value;
				DefaultGroupBox.Invalidate();
			}
		}

		public void CorrectBindToOrgList(string prefix)
		{
			if (!BindToOrganisations.StartsWith(prefix, StringComparison.Ordinal))
			{
				BindToOrganisations = prefix + BindToOrganisations;
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToContacts
		{
			get { return DefaultGroupBox.ContactBindToList; }
			set { DefaultGroupBox.ContactBindToList = value; }
		}

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}

		ControlBindingMemberHelper bindingMemberHelper;

		#endregion

		#region Hook / Unhook DocAddress

		bool firstLoad = true;

		async void HookDocAddress()
		{
			if (DocAddress != null)
			{
				DocAddress.E2_AddressOverrideInfo.ValueChanged += new EventHandler(ValidationStatusInfo_ValueChanged);
				DocAddress.NotificationsChanged += DocAddressNotificationsChanged;
				DocAddress.Factory.Saved += Factory_Saved;

				if (enableAddressValidationWebService)
				{
					if (ShouldValidateAddress())
					{
						DocAddress.NotificationsChanged += DocAddress_NotificationsChanged;
						var topLevelZForm = this.TopLevelControl as ZForm;
						if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly && DocAddress.E2_AddressOverride && !string.IsNullOrEmpty(DocAddress.Address1) && DocAddress.ValidationStatus != AddressValidationStatus.ManuallyVerified && !DocAddress.E2_Address1Info.ReadOnly)
						{
							if (firstLoad)
							{
								firstLoad = false;
								await ValidateAddress(true);
							}
							else
							{
								await ValidateAddress(false);
							}
						}
					}

					//The form may have closed or the binding may have changed by the time the address has been validated, nothing to do if null.
					if (DocAddress != null)
					{
						if (DocAddress.Address != null)
						{
							DocAddress.Address.OA_ValidationStatusInfo.ValueChanged += ValidationStatusInfo_ValueChanged;
						}

						DocAddress.AddressValidationStatusChanged += DocAddress_AddressValidationStatusChanged;
						AddressSuggestionControlHelper.RegisterPropertyChangedEvent(DocAddress, ValidateAddress);
						CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(DocAddress, GetCityTownAsync);
					}
				}
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (DocAddress == null)
			{
				DefaultGroupBox.ResetBindingContext();
				SetDataBinding(DataSource, DataMember);
			}
		}

		void DocAddress_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			NotificationBroadcaster.Instance.BroadcastNotificationsChange(e.SourceOfNotificationChange.GetHighestSeverityNotificationType(), AddressLine1TextBox);
		}

		void UnhookDocAddress()
		{
			if (DocAddress != null)
			{
				DocAddress.E2_AddressOverrideInfo.ValueChanged -= new EventHandler(ValidationStatusInfo_ValueChanged);
				DocAddress.NotificationsChanged -= DocAddressNotificationsChanged;
				DocAddress.Factory.Saved -= Factory_Saved;

				if (enableAddressValidationWebService)
				{
					if (DocAddress.Address != null)
					{
						DocAddress.Address.OA_ValidationStatusInfo.ValueChanged -= ValidationStatusInfo_ValueChanged;
					}
					DocAddress.AddressValidationStatusChanged -= DocAddress_AddressValidationStatusChanged;
					DocAddress.NotificationsChanged -= DocAddress_NotificationsChanged;
				}

				DocAddress.ClearWebAddressValidationHandler();
				DocAddress.ClearWebGetCityTownHandler();
			}
		}

		void ValidationStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
				UpdateControlLayout();
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				Extensions.Dispose();
				UnhookDocAddress();
				if (cancellationToken != null)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
					cancellationToken = null;
				}
			}
			base.Dispose(isNotFinalizing);
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
			AddressLine2TextBox.Extensions.Remove<ILabelCaptionRenderer>();

			if (CutDownOrganisationFindBox.GetExtension<INotificationExtension>() == null)
			{
				CutDownOrganisationFindBox.Extensions.Add(new NotificationExtension()); // For validation notification icons
				CutDownAddressDropEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
			if (CutDownSingleLineOrgFindBox.GetExtension<INotificationExtension>() == null)
			{
				CutDownSingleLineOrgFindBox.Extensions.Add(new NotificationExtension()); // For validation notification icons
				CutDownSingleLineAddressDropEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
			if (CutDownSingleLineNoGroupBoxOrgFindBox.GetExtension<INotificationExtension>() == null)
			{
				CutDownSingleLineNoGroupBoxOrgFindBox.Extensions.Add(new NotificationExtension()); // For validation notification icons
				CutDownSingleLineNoGroupBoxAddressDropEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}

			if (CompactOrganizationFindBox.GetExtension<INotificationExtension>() == null)
			{
				CompactOrganizationFindBox.Extensions.Add(new NotificationExtension());
				CompactAddressDropEdit.Extensions.Add(new NotificationExtension());
			}
		}

		#endregion

		#region ISupportWebAddressValidationControl

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ZTextBox CityControl
		{
			get { return CityTextBox; }
		}

		public ZTextBox PostcodeControl
		{
			get { return PostCodeTextBox; }
		}

		public ZDropEdit StateControl
		{
			get { return StateDropEdit; }
		}

		public ZTextBox Address1Control
		{
			get { return AddressLine1TextBox; }
		}

		public ZTextBox Address2Control
		{
			get { return AddressLine2TextBox; }
		}

		public ZCodeFindBox CountryControl
		{
			get { return CountryFindBox; }
		}

		public ZButton ValidateButton
		{
			get { return ValidateAddressButton; }
		}

		public Control SuggestionWindowParentControl
		{
			get { return ParentForm; }
		}

		public bool ValidationJustForced { get; set; }

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return DocAddress; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		#endregion

		#region Address Formatter

		void SetAddressFormatter()
		{
			var addressFormatter = GetAddressFormatter();
			if (addressFormatter != null)
			{
				DefaultGroupBox.OrgAddressFormatter = addressFormatter;
			}
		}

		protected virtual Func<BusinessObjectFactory, OrgAddress, AddressFormatter> GetAddressFormatter() => null;

		string OrgAddressFormatted
		{
			get
			{
				var factory = DefaultGroupBox.OrganisationForBinding.Factory;
				var addressFormatter = GetAddressFormatter()?.Invoke(factory, DocAddress.Address) ?? new AddressFormatter(factory, DocAddress.Address);
				var orgAddressFormatted = addressFormatter.PostalAddressWithoutCompanyName();

				return string.IsNullOrEmpty(orgAddressFormatted) ? ZOrganisationControl.OrganisationMessages.NoAddressFoundOnFile : orgAddressFormatted;
			}
		}

		#endregion
	}
}
