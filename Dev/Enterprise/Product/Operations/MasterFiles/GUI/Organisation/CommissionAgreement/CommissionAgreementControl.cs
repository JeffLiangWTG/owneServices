using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CommissionAgreementControl : ZUserControl
	{
		#region New

		public static CommissionAgreementControl New()
		{
			var overriddenDelegate = OverridableNewDelegate.Value;
			return overriddenDelegate != null ? overriddenDelegate() : new CommissionAgreementControl();
		}

		protected delegate CommissionAgreementControl NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		protected CommissionAgreementControl()
		{
			InitializeComponent();
			IsApprovedCheckBox.AllowOverlap(detailsSplitContainer);
		}

		#region CurrentDataItem

		new OrgCommissionAgreement CurrentDataItem
		{
			get { return (OrgCommissionAgreement)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var currentAgreement = CurrentDataItem;
			if (currentAgreement != null)
			{
				currentAgreement.StatusInfo.ValueChanged -= StatusInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var currentAgreement = CurrentDataItem;
			if (currentAgreement != null)
			{
				currentAgreement.StatusInfo.ValueChanged += StatusInfo_ValueChanged;

				currentAgreement.NotificationsChanged -= AgreementNotificationsChanged;
				currentAgreement.NotificationsChanged += AgreementNotificationsChanged;
				currentAgreement.Validation.ValidateAll();
				if (hasLoaded)
				{
					currentAgreement.Recipients.EnableValidationOnCountChange = true;
				}
			}

			RefreshAgreementDetails();
		}

		protected void AgreementNotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			var notification = CurrentDataItem?.Notifications?.FirstOrDefault(n => n.Message.EndsWith(OrgCommissionAgreementValidation.WolfPackMemberRequiredMessage));

			if (notification != null)
			{
				SetErrorOnRecipientsGrid(notification.Message, ZNotificationsExtensions.NotificationTypeName(notification.Type, CargoWise.ResourceStrings.Grammar.PluralState.NonPlural).ToUpper() == "ERROR" ? IconTypes.Error : IconTypes.Warning);
			}
			else
			{
				SetErrorOnRecipientsGrid(string.Empty, IconTypes.Warning);
			}
		}

		#endregion

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				SetupIsApprovedCheckEdit();
				SetupAgreementDetails();
				SetupRecipientsGrid();
				SetupRatesGrid();

				if (CurrentDataItem != null)
				{
					CurrentDataItem.Validation.ValidateAll();
					CurrentDataItem.Recipients.EnableValidationOnCountChange = true;
				}
			}
			hasLoaded = true;
		}

		bool hasLoaded;

		#endregion

		#region Navigate

		public void NavigateToCommissionAgreementRecipientRate(OrgCommissionAgreementRecipientRate commissionAgreementRecipientRate)
		{
			RecipientsGrid.UnSelectAll();
			RatesGrid.UnSelectAll();

			var recipient = commissionAgreementRecipientRate.CommissionAgreementRecipient;
			if (recipient != null)
			{
				var foundRecipient = false;

				for (int i = 0; i < RecipientsGrid.List.Count; i++)
				{
					if (((OrgCommissionAgreementRecipient)RecipientsGrid.List[i]).GetMainVersion().PK == recipient.PK)
					{
						RecipientsGrid.ListManager.Position = i;
						RecipientsGrid.Select(i);
						foundRecipient = true;
						break;
					}
				}

				if (foundRecipient)
				{
					for (int i = 0; i < RatesGrid.List.Count; i++)
					{
						if (((OrgCommissionAgreementRecipientRate)RatesGrid.List[i]).GetMainVersion().PK == commissionAgreementRecipientRate.PK)
						{
							RatesGrid.ListManager.Position = i;
							RatesGrid.Select(i);
							break;
						}
					}
				}
			}
		}

		#endregion

		#region IsApprovedCheckEdit

		void SetupIsApprovedCheckEdit()
		{
			ControlDpiScalingHelper.SetLeft(IsApprovedCheckBox, detailsGroupBox.Width - IsApprovedCheckBox.Width - ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
		}

		#endregion

		#region Agreement Details

		void SetupAgreementDetails()
		{
			RefreshAgreementDetails();
		}

		void RefreshAgreementDetails()
		{
			RefreshStatusDescriptionLabel();
			RefreshCommissionBasisDropEdit();
		}

		#region CommissionBasisDropEdit

		void RefreshCommissionBasisDropEdit()
		{
			var currentAgreement = CurrentDataItem;
			CA0_CommissionBasisDropEdit.Visible = currentAgreement != null && currentAgreement.IsViewRecipientsAdditionalInformationAllowed;
		}

		#endregion

		#region StatusDescriptionLabel

		void StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshStatusDescriptionLabel();
		}

		void RefreshStatusDescriptionLabel()
		{
			var statusColours = GetStatusColors(CurrentDataItem);

			StatusDescriptionLabel.BackColor = statusColours.Item1;
			StatusDescriptionLabel.ForeColor = statusColours.Item2;
		}

		static Tuple<Color, Color> GetStatusColors(OrgCommissionAgreement agreement)
		{
			if (agreement == null)
			{
				return Tuple.Create(Color.Transparent, Color.Black);
			}

			switch (agreement.Status)
			{
				case OrgCommissionAgreementStatusList.Codes.Active:
					return Tuple.Create(Color.LimeGreen, Color.White);

				case OrgCommissionAgreementStatusList.Codes.Inactive:
					return Tuple.Create(Color.Yellow, Color.Black);

				case OrgCommissionAgreementStatusList.Codes.Expired:
					return Tuple.Create(Color.LightGray, Color.Black);

				case OrgCommissionAgreementStatusList.Codes.Reversed:
					return Tuple.Create(Color.Red, Color.White);
			}

			return Tuple.Create(Color.Transparent, Color.Black);
		}

		#endregion

		#endregion

		#region RecipientsGrid

		#region RecipientsGridErrorProvider

		protected ErrorProvider RecipientsGridErrorProvider
		{
			get
			{
				if (fRecipientsGridErrorProvider == null)
				{
					fRecipientsGridErrorProvider = new ErrorProvider();
					fRecipientsGridErrorProvider.SetIconAlignment(RecipientsGrid, ErrorIconAlignment.TopLeft);
					fRecipientsGridErrorProvider.SetIconPadding(RecipientsGrid, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(-11));
					fRecipientsGridErrorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
				}
				return fRecipientsGridErrorProvider;
			}
		}
		ErrorProvider fRecipientsGridErrorProvider;

		protected void SetErrorOnRecipientsGrid(string errorMessage, IconTypes icon)
		{
			RecipientsGridErrorProvider.Icon = Icons.GetIcon(icon);
			RecipientsGridErrorProvider.SetError(RecipientsGrid, errorMessage);
#if WINZOR
			RecipientsGridErrorProvider.NotificationType = icon.ToString();
			RecipientsGrid.SetErrorProvider(RecipientsGridErrorProvider);
#endif
		}

		#endregion
		void SetupRecipientsGrid()
		{
			AddRecipientsGridContextMenuItems();
		}

		#region RecipientsGrid Context Menu

		void AddRecipientsGridContextMenuItems()
		{
			recipientsGridRatesMenuItem = new ZMenuItem(ResString.GetMultilingualString("da6157a0-0927-4f28-bda3-86d9d60b0e4b", "Commission Rates"));
			recipientsGridOverrideRatesMenuItem = GetNewOverrideRatesMenuItem();
			recipientsGridUseDefaultRatesMenuItem = GetNewUseDefaultRatesMenuItem();

			recipientsGridRatesMenuItem.MenuItems.Add(recipientsGridOverrideRatesMenuItem);
			recipientsGridRatesMenuItem.MenuItems.Add(recipientsGridUseDefaultRatesMenuItem);
			RecipientsGrid.ContextMenu.MenuItems.Add(recipientsGridRatesMenuItem);
			RecipientsGrid.ContextMenu.Popup += RecipientsGridContextMenu_Popup;
		}

		void RecipientsGridContextMenu_Popup(object sender, EventArgs e)
		{
			RefreshRateMenuItems(recipientsGridRatesMenuItem, recipientsGridOverrideRatesMenuItem, recipientsGridUseDefaultRatesMenuItem);
		}

		ZMenuItem recipientsGridRatesMenuItem;
		ZMenuItem recipientsGridOverrideRatesMenuItem;
		ZMenuItem recipientsGridUseDefaultRatesMenuItem;

		#endregion

		#endregion

		#region RatesGrid

		void SetupRatesGrid()
		{
			AddRatesGridContextMenuItems();
			RefreshRatesGridSecurityOverlayLabel();
			RecipientsGrid.ListManager.CurrentChanged += RecipientsGrid_CurrentChanged;
			if (CurrentDataItem != null)
			{
				CurrentDataItem.EffectiveDateInfo.ValueChanged += (o, e) => RatesGrid.Refresh();
				CurrentDataItem.CA0_CommissionTriggerTypeInfo.ValueChanged += (o, e) => RatesGrid.Refresh();
			}
		}

		#region RatesGrid Security Overlay

		void AddRatesGridSecurityOverlayLabel()
		{
			RatesGrid.Visible = false;
			CAR_EndDateEdit.Visible = false;
			if (ratesGridSecurityOverlayLabel == null)
			{
				ratesGridSecurityOverlayLabel = new ZLabel();
				ratesGridSecurityOverlayLabel.Dock = DockStyle.Fill;
				ratesGridSecurityOverlayLabel.BackColor = Color.Transparent;
				ratesGridSecurityOverlayLabel.TextAlign = ContentAlignment.MiddleCenter;
				ratesGridSecurityOverlayLabel.Text = Env.Security.CommissionAgreementViewAny.ErrorMessageForNotAllowed;
				ratesGroupBox.Controls.Add(ratesGridSecurityOverlayLabel);
			}
			else
			{
				ratesGridSecurityOverlayLabel.Visible = true;
			}
		}

		void RemoveRatesGridSecurityOverlayLabel()
		{
			if (ratesGridSecurityOverlayLabel != null)
			{
				ratesGridSecurityOverlayLabel.Visible = false;
			}
			RatesGrid.Visible = true;
			CAR_EndDateEdit.Visible = true;
		}

		ZLabel ratesGridSecurityOverlayLabel;

		void RefreshRatesGridSecurityOverlayLabel()
		{
			var currentRecipient = RecipientsGrid.ListManager != null ? RecipientsGrid.ListManager.GetCurrent() as OrgCommissionAgreementRecipient : null;
			if (currentRecipient != null)
			{
				if (currentRecipient.IsViewRatesAllowed)
				{
					RemoveRatesGridSecurityOverlayLabel();
				}
				else
				{
					AddRatesGridSecurityOverlayLabel();
				}
			}
		}

		void RecipientsGrid_CurrentChanged(object sender, EventArgs e)
		{
			if (currentlyHookedRecipientStaffInfo != null)
			{
				currentlyHookedRecipientStaffInfo.ValueChanged -= RecipientStaff_CAR_GS_NKStaffInfo_ValueChanged;
				currentlyHookedRecipientStaffInfo = null;
			}

			RefreshRatesGridSecurityOverlayLabel();

			var recipient = RecipientsGrid.ListManager != null ? RecipientsGrid.ListManager.GetCurrent() as OrgCommissionAgreementRecipient : null;
			if (recipient != null)
			{
				currentlyHookedRecipientStaffInfo = recipient.CAR_GS_NKStaffInfo;
				currentlyHookedRecipientStaffInfo.ValueChanged += RecipientStaff_CAR_GS_NKStaffInfo_ValueChanged;
			}
		}

		ZPropertyInfo currentlyHookedRecipientStaffInfo;
		void RecipientStaff_CAR_GS_NKStaffInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshRatesGridSecurityOverlayLabel();
		}

		#endregion

		#region RatesGrid Context Menu

		void AddRatesGridContextMenuItems()
		{
			ratesGridRatesMenuItem = new ZMenuItem(ResString.GetMultilingualString("da6157a0-0927-4f28-bda3-86d9d60b0e4b", "Commission Rates"));
			ratesGridOverrideRatesMenuItem = GetNewOverrideRatesMenuItem();
			ratesGridUseDefaultRatesMenuItem = GetNewUseDefaultRatesMenuItem();

			ratesGridRatesMenuItem.MenuItems.Add(ratesGridOverrideRatesMenuItem);
			ratesGridRatesMenuItem.MenuItems.Add(ratesGridUseDefaultRatesMenuItem);
			RatesGrid.ContextMenu.MenuItems.Add(ratesGridRatesMenuItem);
			RatesGrid.ContextMenu.Popup += RatesGridContextMenu_Popup;
		}

		void RatesGridContextMenu_Popup(object sender, EventArgs e)
		{
			RefreshRateMenuItems(ratesGridRatesMenuItem, ratesGridOverrideRatesMenuItem, ratesGridUseDefaultRatesMenuItem);
		}

		ZMenuItem ratesGridRatesMenuItem;
		ZMenuItem ratesGridOverrideRatesMenuItem;
		ZMenuItem ratesGridUseDefaultRatesMenuItem;

		#endregion

		#endregion

		#region Context Menu Common

		ZMenuItem GetNewOverrideRatesMenuItem()
		{
			return new ZMenuItem(ResString.GetMultilingualString("5731c150-d73c-4678-b784-f3a4dfc0a767", "Override"), OnOverrideRatesMenuItemClicked);
		}

		ZMenuItem GetNewUseDefaultRatesMenuItem()
		{
			return new ZMenuItem(ResString.GetMultilingualString("0182ab5b-f511-4e2c-9540-4d8b8b47b1d4", "Use Default"), OnUseDefaultsRatesMenuItemClicked);
		}

		void OnOverrideRatesMenuItemClicked(object sender, EventArgs args)
		{
			var currentRecipient = RecipientsGrid.ListManager != null ? RecipientsGrid.ListManager.GetCurrent() as OrgCommissionAgreementRecipient : null;
			if (currentRecipient != null)
			{
				if (!currentRecipient.IsEditRatesAllowed)
				{
					Env.Security.CommissionAgreementOverrideAny.ShowError();
				}
				else
				{
					currentRecipient.CAR_IsCommissionRateOverriden = true;
				}
			}
		}

		void OnUseDefaultsRatesMenuItemClicked(object sender, EventArgs args)
		{
			var currentRecipient = RecipientsGrid.ListManager != null ? RecipientsGrid.ListManager.GetCurrent() as OrgCommissionAgreementRecipient : null;
			if (currentRecipient != null)
			{
				if (!currentRecipient.IsEditRatesAllowed)
				{
					Env.Security.CommissionAgreementOverrideAny.ShowError();
				}
				else
				{
					if (currentRecipient.Rates.Any(x => x.HasCommissionLine))
					{
						Globals.Message.ShowError(Res.GetString("8739f299-e92e-44fc-ba67-241130e7faab", "Can not re-default commission rates as existing rates have already been used for a commission pay out."), UseDefaultRatesCaption);
					}
					else if (!currentRecipient.HasResponsibleStaffCommissionRule)
					{
						if (!currentRecipient.CAR_GS_NKStaff.IsEmpty)
						{
							Globals.Message.ShowError(Res.GetString("2deda42e-1ed0-4d69-8269-e3ff3e6d44e2", "This staff does not have any applicable commission rules setup for this agreement. Please add an applicable commission rule to this staff, or manually enter their commission rates for this agreement."), UseDefaultRatesCaption);
						}
						else if (!currentRecipient.CAR_OH_Party.IsEmpty)
						{
							Globals.Message.ShowError(Res.GetString("1a6c236b-238c-4e2f-a442-77cbd8a7f2f1", "Commission Rates must be entered manually for organizations as they do not have any default commission rules."), UseDefaultRatesCaption);
						}
						else
						{
							Globals.Message.ShowError(Res.GetString("c4ad534c-aee3-4252-b46a-acf795169da4", "Please enter a staff or organization first"), UseDefaultRatesCaption);
						}
					}
					else
					{
						currentRecipient.CAR_IsCommissionRateOverriden = false;
					}
				}
			}
		}

		static string UseDefaultRatesCaption
		{
			get { return Res.GetString("f3a23d0c-9dbb-4097-a3eb-4611166a7d2f", "Use Default Rates"); }
		}

		void RefreshRateMenuItems(ZMenuItem ratesParentMenuItem, ZMenuItem overrideRatesMenuItem, ZMenuItem useDefaultRatesMenuItem)
		{
			var isListEditable = RecipientsGrid.ListManager != null && RecipientsGrid.List.AllowEdit;
			ratesParentMenuItem.Visible = isListEditable;

			if (ratesParentMenuItem.Visible)
			{
				var currentRecipient = RecipientsGrid.ListManager != null ? RecipientsGrid.ListManager.GetCurrent() as OrgCommissionAgreementRecipient : null;
				var rateMenuItemsShouldBeEnabled = (currentRecipient != null) && !currentRecipient.CAR_GS_NKStaff.IsEmpty;

				ratesParentMenuItem.Enabled = rateMenuItemsShouldBeEnabled;
				overrideRatesMenuItem.Enabled = rateMenuItemsShouldBeEnabled;
				overrideRatesMenuItem.Checked = overrideRatesMenuItem.Enabled && currentRecipient.CAR_IsCommissionRateOverriden;
				useDefaultRatesMenuItem.Enabled = rateMenuItemsShouldBeEnabled;
				useDefaultRatesMenuItem.Checked = useDefaultRatesMenuItem.Enabled && !currentRecipient.CAR_IsCommissionRateOverriden;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (currentlyHookedRecipientStaffInfo != null)
				{
					currentlyHookedRecipientStaffInfo.ValueChanged -= RecipientStaff_CAR_GS_NKStaffInfo_ValueChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
