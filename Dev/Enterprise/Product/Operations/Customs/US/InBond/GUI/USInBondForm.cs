using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBondForm : ZTemplateForm
	{
		public USInBondForm(CusInBondHeader header)
			: base(header)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			AddMessagingMenu();
			isAMSHBREffective = US.Business.ZZCustomsFunctionality.IsAMSHBREffective;
			if (BusinessEntity != null)
			{
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged += new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
				BusinessEntity.BondedWarehouseLicenceLogin += BusinessEntity_BondedWarehouseLicenceLogin;
			}
			WorkflowTabPage.Initialize(header);
			BH_ImportTransportModeInfo_ValueChanged(null, null);
		}
		readonly bool isAMSHBREffective;

		public override string FormCaption
		{
			get
			{
				string aCaption = Res.GetString("USInBondForm|FormCaption", "In-Bond");
				if (!this.IsDesignMode())
				{
					aCaption += " - " + BusinessEntity.HumanReadableName;
				}
				return aCaption;
			}
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = new USInBondSecurityAlertHelper(BusinessEntity).GetSecurityAlertMessage();
			}
			return result;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged -= new EventHandler(BH_ImportTransportModeInfo_ValueChanged);
				BusinessEntity.BondedWarehouseLicenceLogin -= BusinessEntity_BondedWarehouseLicenceLogin;
			}
			base.Dispose(isNotFinalizing);
		}

		public new CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.BusinessEntity; }
		}

		void AddMessagingMenu()
		{
			EDIMenu messagingMenutItem = new EDIMenu();
			messagingMenutItem.Header = BusinessEntity;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenutItem);
		}

		void BH_ImportTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlsVisibility();
		}

		void ControlsVisibility()
		{
			var isAir = BusinessEntity != null && BusinessEntity.IsAir;
			this.BillsTabPage.CheckForChildrenControlsVisibilityChange = !isAir;
			this.MovementDetailsTabPage.CheckForChildrenControlsVisibilityChange = !isAir;
			this.AirBillsAndMovementsDetailsTabPage.CheckForChildrenControlsVisibilityChange = isAir;

			this.BillsTabPage.CheckForNotifications = !isAir;
			this.MovementDetailsTabPage.CheckForNotifications = !isAir;
			this.AirBillsAndMovementsDetailsTabPage.CheckForNotifications = isAir;

			this.BillsTabPage.TabVisible = !isAir;
			this.MovementDetailsTabPage.TabVisible = !isAir;
			this.AirBillsAndMovementsDetailsTabPage.TabVisible = isAir;

			var showAMSHBRFields = isAMSHBREffective && BusinessEntity != null && BusinessEntity.IsSea;
			this.InBondBillsUserControl.UpdateAMSHBRFields(showAMSHBRFields);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void BusinessEntity_BondedWarehouseLicenceLogin(object sender, Customs.Business.LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(this);
		}

		protected readonly CusInBondHeader header;

		public bool? SkipRecentItems { get; set; }

		protected override void SaveToRecentItems()
		{
			if (!SkipRecentItems.HasValue || !SkipRecentItems.Value)
			{
				base.SaveToRecentItems();
			}
		}
	}
}
