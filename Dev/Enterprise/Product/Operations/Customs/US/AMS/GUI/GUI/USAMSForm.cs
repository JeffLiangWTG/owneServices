using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class USAMSForm : ZTemplateForm
	{
		public USAMSForm()
		{
			InitializeComponent();
		}

		public USAMSForm(CusInBondHeader header)
			: base(header)
		{
			InitializeComponent();
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			WorkflowTabPage.Initialize(header);
			var isNVOCCHeader = header.IsNVOCCHeader;
			permitToTransferTabPage.TabVisible = !isNVOCCHeader;
			usamsInBondUserControl.Visible = !isNVOCCHeader;
			usamsNVOCCInBondUserControl.Visible = isNVOCCHeader;
			USAMSSplitContainer.Panel1Collapsed = isNVOCCHeader;
			USAMSSplitContainer.IsSplitterFixed = isNVOCCHeader;
			usamsMainUserControl.OverrideFreightDefaultsCheckBox.Visible = !isNVOCCHeader;
			var actionMenuItemIndex = this.MainMenu.MenuItems.IndexOf(ActionsMenuItem);
			var amsMenuItem = new AMSMainMenuItem(header);
			this.MainMenu.MenuItems.Add(actionMenuItemIndex + 1, amsMenuItem);
			if (!isNVOCCHeader)
			{
				this.usamsMainUserControl.OverrideFreightDefaultsCheckBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("4D69FFA3-E65C-4B34-A72B-97B460E90AAD", "Override Sailing Defaults");
				amsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("USAMSForm|ImportFromSailing", ImportBillsLinkedToSailing), ImportFromSailingClick));
				header.Sailings.CountChanged -= OnSailingsCountChanged;
				header.Sailings.CountChanged += OnSailingsCountChanged;
				OnSailingsCountChanged(null, null);
				this.usamsMainUserControl.OverrideFreightDefaultsCheckBox.Select();
				header.SynchroniseIfNeeded();
			}
		}
		internal const string ImportBillsLinkedToSailing = "Import Bills of Lading linked to the same sailing";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline, but refactored to only access .Count once")]
		void OnSailingsCountChanged(object sender, EventArgs e)
		{
			var sailings = BusinessEntity.Sailings;
			var sailingsExist = sailings.Count > 0;
			this.sailingUserControl.SailingStatisticsPanel.Visible = sailingsExist;
			this.usamsBillsUserControl.SailingStatisticPanel.Visible = sailingsExist;
			this.sailingUserControl.RefreshStatisticsButton.Visible = sailingsExist;
			this.usamsMainUserControl.OverrideFreightDefaultsCheckBox.Visible = sailingsExist;
		}

		public new CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.BusinessEntity; }
		}

		void ImportFromSailingClick(object sender, EventArgs e)
		{
			var header = BusinessEntity;
			if (header.Sailing != null)
			{
				var existingBillHandlers = new BillImportActionCollection(header.Bills);
				Action importAction = () => RunActionWithProcessBox(Res.GetString("USAMSForm|ImportFromSailing|E07CC74E-C888-4838-81FF-7DB3FB5A5FEB", "Importing bills from sailing..."), () => { header.ImportBillsOfLadingLinkedToTheSameSailing(existingBillHandlers); });
				if (existingBillHandlers.Count > 0)
				{
					using (var billAction = new ImportFromSailingForm(existingBillHandlers))
					{
						if (billAction.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							importAction();
						}
					}
				}
				else
				{
					importAction();
				}
			}
			else
			{
				Globals.Message.Show(NoSailingSelected);
			}
		}

		internal static string NoSailingSelected
		{
			get { return Res.GetString("F8588C0F-B19E-4B5F-B424-F761498BAE0C", "There is no Sailing Schedule to import Bills Of Lading."); }
		}

		public override string FormCaption
		{
			get
			{
				if (!this.IsDesignMode())
				{
					if (BusinessEntity.IsInDatabase)
					{
						return BusinessEntity.BH_JobReference;
					}
				}
				return BusinessEntity.IsNVOCCHeader ? Res.GetString("USAMSForm|NVOCCFormCaption", "NVOCC") : Res.GetString("USAMSForm|VOCCFormCaption", "VOCC");
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				var header = BusinessEntity;
				if (header != null)
				{
					header.Sailings.CountChanged -= OnSailingsCountChanged;
				}
			}
			base.Dispose(isNotFinalizing);
		}

		public void SelectAndShowBill(ZGuid bilPK)
		{
			if (BusinessEntity != null && billDetailsTabPage.TabVisible)
			{
				var bill = (CusInBondBill)BusinessEntity.Bills.FindByPK(bilPK);
				if (bill != null)
				{
					MainTabControl.SelectedTab = billDetailsTabPage;
					var manager = (CurrencyManager)usamsBillsUserControl.BindingContext[BusinessEntity, "Bills"];
					var index = manager.List.IndexOf(bill);
					if (index >= 0)
					{
						manager.Position = index;
					}
				}
				else
				{
					var oceanBill = BusinessEntity.OceanBill;
					if (oceanBill != null && oceanBill.PK == bilPK)
					{
						MainTabControl.SelectedTab = MainTabPage;
					}
				}
			}
		}
	}
}
