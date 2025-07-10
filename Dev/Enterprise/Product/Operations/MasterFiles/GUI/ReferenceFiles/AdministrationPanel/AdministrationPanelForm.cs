using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AdministrationPanelForm : ZForm
	{
		public AdministrationPanelForm(AdministrationPanelManager manager)
			: base(manager)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			InitializeTabs();
			InitializeMenuItem();
		}

		internal AdministrationPanelForm(AdministrationPanelManager manager, DeduplicationTargetType dedupeDefaultsTargetType, FilterBusinessObjectDefaults dedupeDefaults)
			: base(manager)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			InitializeTabs(dedupeDefaultsTargetType, dedupeDefaults);
			InitializeMenuItem();
		}

		public override string FormVerb => String.Empty;

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				WindowState = FormWindowState.Normal;
			}

			BringToFront();
			AddressUserControl?.CheckAndSaveChangedData(e);
		}

		void InitializeTabs()
		{
			InitializeTabs(DeduplicationTargetType.Organization, null);
		}

		void InitializeTabs(DeduplicationTargetType dedupeDefaultsTargetType, FilterBusinessObjectDefaults dedupeDefaults)
		{
			if (SystemDataRegistry.Instance.MdmAdministrationPanelAddressTabEnabled.Value)
			{
				InitializeAddressesTab();
			}

			if (SystemDataRegistry.Instance.MdmAdministrationPanelDuplicatesTabEnabled.Value)
			{
				InitializeDuplicationTab(dedupeDefaultsTargetType, dedupeDefaults);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				RememberSplitterLayout = false;

				if (this.Height < ControlDpiScalingHelper.ScaleToCurrentDpiY(DefaultFormHeight))
				{
					this.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(DefaultFormHeight);
				}

				InitializeButtonUserControl();
			}
		}

		const int DefaultFormHeight = 780;

		#region InitializeTabPages

		void InitializeDuplicationTab(DeduplicationTargetType dedupeDefaultsTargetType, FilterBusinessObjectDefaults dedupeDefaults)
		{
			if (CheckSecurityRightsForTab(Env.Security.MdmAdministrationPanelDuplicatesEdit, DuplicatesTabPage))
			{
				CreateDuplicatesSubTabControl();
				if (SystemDataRegistry.Instance.MdmAdministrationPanelOrganizationDuplicatesTabEnabled.Value)
				{
					deduplicationOrganizationTabPage = CreateDeDuplicationTabPage(
						Res.GetData("9821e2b1-06f5-4a76-af10-1f013f03cf28", "Organizations"),
						GetNewDuplicationOrganizationsUserControl(),
						dedupeDefaultsTargetType == DeduplicationTargetType.Organization ? dedupeDefaults : null);
				}

				if (SystemDataRegistry.Instance.MdmAdministrationPanelPersonDuplicatesTabEnabled.Value
					&& Env.Security.PersonIntelligence.IsAllowed)
				{
					deduplicationPersonTabPage = CreateDeDuplicationTabPage(
					Res.GetData("471bb71a-56aa-4f94-a803-8961f3d70112", "Persons"),
					GetNewDuplicationPersonsUserControl(),
					dedupeDefaultsTargetType == DeduplicationTargetType.Person ? dedupeDefaults : null);
				}
			}
		}

		bool CheckSecurityRightsForTab(SecurityCheckpoint checkpoint, ZTabPage tabPage)
		{
			tabPage.TabVisible = true;
			var result = checkpoint.IsAllowed;
			if (!result)
			{
				CoverControl(checkpoint.ErrorMessageForNotAllowed, tabPage);
			}

			return result;
		}

		protected virtual ZUserControl GetNewDuplicationOrganizationsUserControl()
		{
			return ObjectFactory.Get<IMasterDataProviderGUI>().GetNewDeduplicationOrganizationsUserControl();
		}

		protected virtual ZUserControl GetNewDuplicationPersonsUserControl()
		{
			return ObjectFactory.Get<IMasterDataProviderGUI>().GetNewDeduplicationPersonUserControl();
		}

		void InitializeAddressesTab()
		{
			if (CheckSecurityRightsForTab(Env.Security.MdmAdministrationPanelAddressesEdit, AddressesTabPage))
			{
				AddressUserControl = new AddressUserControl(Manager);
				AddressUserControl.Name = "AddressUserControl";
				AddressUserControl.Dock = DockStyle.Fill;
				AddressUserControl.ParentAdminForm = this;
				AddressUserControl.CaptionRenderingEnabled = true;
				AddressesTabPage.Controls.Add(AddressUserControl);
			}
		}

		void CoverControl(string errorMessage, ZTabPage page)
		{
			var coveringLabel = new ZLabel
			{
				Name = "coveringLabel",
				Dock = DockStyle.Fill,
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
				Text = errorMessage
			};

			page.Controls.Add(coveringLabel);
		}

		ZTabPage deduplicationOrganizationTabPage;
		ZTabPage deduplicationPersonTabPage;

		internal ZTabPage CreateDeDuplicationTabPage(ResourceStringData caption, ZUserControl userControl, FilterBusinessObjectDefaults dedupeDefaults)
		{
			var deduplicationTabPage = new ZTabPage
			{
				CaptionResourceString = caption,
				Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true),
				TabVisible = true,
				Padding = ControlDpiScalingHelper.NewScaledPadding(3, true),
				Size = ControlDpiScalingHelper.NewScaledSize(986, 711, true)
			};

			deduplicationTabPage.RunWhenBindingOrFirstShown(delegate
			{
				deduplicationTabPage.Controls.Add(userControl);
				if (dedupeDefaults != null)
				{
					ShowDuplicatesTabWithDefaults(deduplicationTabPage, dedupeDefaults);
				}
			});

			deduplicationTabPage.Disposed += delegate
			{
				userControl.Dispose();
			};

			userControl.Dock = DockStyle.Fill;
			DuplicatesSubTabControl.Controls.Add(deduplicationTabPage);

			if (dedupeDefaults != null)
			{
				ShowDuplicatesTab();
			}

			return deduplicationTabPage;
		}

		internal void SwitchToDuplicatesTabWithDefaults(DeduplicationTargetType deduplicationTabPageType, FilterBusinessObjectDefaults dedupeDefaults)
		{
			var deduplicationTabPage = GetDeduplicationTabPage(deduplicationTabPageType);
			ShowDuplicatesTabWithDefaults(deduplicationTabPage, dedupeDefaults, true);
		}

		ZTabPage GetDeduplicationTabPage(DeduplicationTargetType deduplicationTabPageType)
		{
			switch (deduplicationTabPageType)
			{
				case DeduplicationTargetType.Organization:
					return deduplicationOrganizationTabPage;
				case DeduplicationTargetType.Person:
					return deduplicationPersonTabPage;
				default:
					return deduplicationOrganizationTabPage;
			}
		}

		public enum DeduplicationTargetType
		{
			Organization,
			Person
		}

		void ShowDuplicatesTabWithDefaults(ZTabPage deduplicationTabPage, FilterBusinessObjectDefaults dedupeDefaults, bool adminPanelAlreadyOpen = false)
		{
			if (deduplicationTabPage != null)
			{
				ShowDuplicatesTab();
				var deduplicationUserControl = deduplicationTabPage.Controls.Cast<Control>().First(c => c is IDeduplicationBusinessObjectUserControl) as IDeduplicationBusinessObjectUserControl;
				deduplicationUserControl.ResetFilterStripAndPerformDefaultCodeSearch(dedupeDefaults, waitforLoad: !adminPanelAlreadyOpen);
			}
		}

		void CreateDuplicatesSubTabControl()
		{
			DuplicatesSubTabControl = new ZTemplateTabControl();
			DuplicatesSubTabControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			DuplicatesSubTabControl.Name = "DuplicatesSubTabControl";
			DuplicatesSubTabControl.SelectedIndex = 0;
			DuplicatesSubTabControl.Dock = DockStyle.Fill;
			DuplicatesSubTabControl.TabIndex = 1;
			DuplicatesSubTabControl.BackColor = System.Drawing.SystemColors.Control;
			DuplicatesSubTabControl.TabOrderExtendedToTabPages = true;
			DuplicatesTabPage.Controls.Add(DuplicatesSubTabControl);
		}

		#endregion

		void InitializeButtonUserControl()
		{
			ControlDpiScalingHelper.SetTop(ref ButtonsUserControl, MainStatusBar.Top - ButtonsUserControl.Height - ControlDpiScalingHelper.OnePixel, false);
			fPostButton.Visible = false;
			fApplyButton.Visible = false;
		}

		void InitializeMenuItem()
		{
			BackgroundValidationMenuItem.Enabled = AddressesTabPage.TabVisible && Env.Instance.Registry.EnableAddressValidationWebService;
			SuspendBackgroundValidation = !BackgroundValidationMenuItem.Enabled || SystemDataRegistry.Instance.BackgroundValidationSuspended.Value;
			BackgroundValidationMenuItem.Click += BackgroundValidationMenuItemClick;
			FileMenuItem.MenuItems.InsertRange(3, new[] { BackgroundValidationMenuItem });

			CopyCompanyInformationMenuItem.Caption = CopyCompanyInformationCaption;
			CopyCompanyInformationMenuItem.Shortcut = Shortcut.CtrlI;
			CopyCompanyInformationMenuItem.ShowShortcut = true;
			CopyCompanyInformationMenuItem.Enabled = false;
			CopyCompanyInformationMenuItem.Click += delegate
			{
				AddressDetailControl?.CopyCompanyInformation();
			};

			SearchCompanyOnlineMenuItem.Caption = SearchCompanyOnlineCaption;
			SearchCompanyOnlineMenuItem.Shortcut = Shortcut.CtrlG;
			SearchCompanyOnlineMenuItem.ShowShortcut = true;
			SearchCompanyOnlineMenuItem.Enabled = false;
			SearchCompanyOnlineMenuItem.Click += delegate
			{
				AddressDetailControl?.SearchCompanyOnline();
			};

			CopyAddressInformationMenuItem.Caption = CopyAddressInformationCaption;
			CopyAddressInformationMenuItem.Shortcut = Shortcut.CtrlO;
			CopyAddressInformationMenuItem.ShowShortcut = true;
			CopyAddressInformationMenuItem.Enabled = false;
			CopyAddressInformationMenuItem.Click += delegate
			{
				AddressDetailControl?.CopyAddressInformation();
			};

			SearchAddressOnlineMenuItem.Caption = SearchAddressOnlineCaption;
			SearchAddressOnlineMenuItem.Shortcut = Shortcut.CtrlH;
			SearchAddressOnlineMenuItem.ShowShortcut = true;
			SearchAddressOnlineMenuItem.Enabled = false;
			SearchAddressOnlineMenuItem.Click += delegate
			{
				AddressDetailControl?.SearchAddressOnline();
			};

			ActionsMenuItem.MenuItems.InsertRange(0, new[] { CopyCompanyInformationMenuItem, SearchCompanyOnlineMenuItem, CopyAddressInformationMenuItem, SearchAddressOnlineMenuItem });

			MainTabControl.SelectedIndexChanged += delegate
			{
				if (MainTabControl.SelectedTab == AddressesTabPage)
				{
					AddressDetailControl?.RefreshLinkControl();
				}
				else
				{
					if (CopyCompanyInformationMenuItem != null)
					{
						CopyCompanyInformationMenuItem.Enabled = false;
					}

					if (SearchCompanyOnlineMenuItem != null)
					{
						SearchCompanyOnlineMenuItem.Enabled = false;
					}

					if (CopyAddressInformationMenuItem != null)
					{
						CopyAddressInformationMenuItem.Enabled = false;
					}

					if (SearchAddressOnlineMenuItem != null)
					{
						SearchAddressOnlineMenuItem.Enabled = false;
					}
				}
			};
		}

		SingleAddressValidationControl AddressDetailControl => AddressUserControl?.AddressDetailControl;

		void BackgroundValidationMenuItemClick(object sender, EventArgs e)
		{
			if (SuspendBackgroundValidation)
			{
				SuspendBackgroundValidation = false;
				AddressUserControl.StartBackgroudValidation();
			}
			else
			{
				SuspendBackgroundValidation = true;
				AddressUserControl.CancelBackgroundValidation();
			}
		}

		public void ShowDuplicatesTab()
		{
			MainTabControl.SelectedTab = DuplicatesTabPage;
		}

		internal bool SuspendBackgroundValidation
		{
			get => suspendBackgroundValidation;
			set
			{
				suspendBackgroundValidation = value;
				BackgroundValidationMenuItem.Caption = suspendBackgroundValidation ? StartValidationCaption : SuspendValidationCaption;
				if (BackgroundValidationMenuItem.Enabled)
				{
					MessageStatusBarPanel.Text = suspendBackgroundValidation ? TurnOffValidationText : TurnOnValidationText;
				}
			}
		}
		bool suspendBackgroundValidation;

		internal ResourceString StartValidationCaption = ResString.GetMultilingualString("9F302191-3167-4D24-BAC1-99FB7B043FC6", "&Start Background Validation");
		internal ResourceString SuspendValidationCaption = ResString.GetMultilingualString("831D00B8-1714-4A3A-A491-26280A6D9B9D", "Suspend &Background Validation");
		internal ResourceString CopyCompanyInformationCaption = ResString.GetMultilingualString("831D00B8-1714-4A3B-A491-26280A6D9B9D", "Copy &Company Information");
		internal ResourceString SearchCompanyOnlineCaption = ResString.GetMultilingualString("831D00B8-1714-4A3C-A491-26280A6D9B9D", "Search Company &Online");
		internal ResourceString TurnOffValidationText = ResString.GetMultilingualString("1A690C03-6CFB-4B7C-AA9F-A74C2C5AB0EE", "You have turned off [Background Validation]. If the validation status of your retrieved address is [To Be Verified], the address will not be verified automatically.");
		internal ResourceString TurnOnValidationText = ResString.GetMultilingualString("1a690C03-6CFB-4B6C-AA9F-A74C2C5AB0EE", "You have turned on [Background Validation]. If the validation status of your retrieved address is [To Be Verified], the address will be verified automatically.");
		internal ResourceString CopyAddressInformationCaption = ResString.GetMultilingualString("A449F8B2-1B0F-4855-9C82-1EDC1E1C11DA", "Copy &Address Information");
		internal ResourceString SearchAddressOnlineCaption = ResString.GetMultilingualString("3554FDF0-D742-4040-A70F-DF332668B315", "&Search Address Online");

		internal AdministrationPanelManager Manager => manager ?? (manager = (AdministrationPanelManager)BusinessEntity);
		AdministrationPanelManager manager;
		protected AddressUserControl AddressUserControl;
		protected ZTemplateTabControl DuplicatesSubTabControl;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		IContainer components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			AddressUserControl?.Dispose();

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			var masterDataProviderGUI = ObjectFactory.Get<IMasterDataProviderGUI>();
			masterDataProviderGUI.PersistenceFiltersValue(BusinessEntity.Factory);
			masterDataProviderGUI.ClearAdvancedFilterPanelCache();

			base.Dispose(disposing);
		}
	}
}
