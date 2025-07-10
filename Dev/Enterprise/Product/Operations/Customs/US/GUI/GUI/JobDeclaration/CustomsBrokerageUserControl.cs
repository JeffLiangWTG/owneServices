using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using IInvoicesProvider = Enterprise.Customs.US.Business.IInvoicesProvider;

namespace Enterprise.Customs.US.GUI
{
	public partial class CustomsBrokerageUserControl : Customs.GUI.BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
			StatusTabPage.ExcludeFromBindingOnSave = true;

			MiscOptionsTabPage.Enter += MiscOptionsTabPage_Enter;

			InvoiceGroupingTabPage.Text = "Inv Groups";
			MessagesTabPage.Text = "Messages";
			WHSPacksTabPage.LazyCreateControls += WHSPacksTabPage_LazyCreateControls;
			ReOrderTabPages();
		}

		void MiscOptionsTabPage_Enter(object sender, EventArgs e)
		{
			JobDeclaration.RecalculateReconIndicators();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		protected override void OtherRegisters()
		{
			if (LockManager != null)
			{
				var declarationUserControl = fBaseCustomsEntryUserControl as USJobDeclarationUserControl;
				if (declarationUserControl != null)
				{
					LockManager.Register(Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationOrganizations, declarationUserControl.USOrganisationsTabPage);
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JE_MessageTypeInfo_ValueChanged(this, EventArgs.Empty);
		}

		#region WHS Packs Tab

		public Customs.GUI.BaseDeclarationTabPage WHSPacksTabPage;

		void WHSPacksTabPage_LazyCreateControls(object sender, EventArgs e)
		{
			LoadWHSPacksUserControl();
		}

		void LoadWHSPacksUserControl()
		{
			if (whsPacksUserControl == null && WHSPacksTabPage.Controls.Count == 0)
			{
				whsPacksUserControl = new WHSPacksUserControl();
				whsPacksUserControl.Dock = DockStyle.Fill;
				WHSPacksTabPage.Controls.Add(whsPacksUserControl);
				whsPacksUserControl.SetDataBinding(JobDeclaration, "");
			}
		}
		WHSPacksUserControl whsPacksUserControl;

		#endregion

		#region Change User Control on Sub Message

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			if (declaration != null)
			{
				JobDeclaration usDeclaration = (JobDeclaration)declaration;
				usDeclaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				usDeclaration.US_TariffTypeInfo.ValueChanged += US_TariffTypeInfo_ValueChanged;
				usDeclaration.US_EntryTypeInfo.ValueChanged += US_EntryTypeInfo_ValueChanged;
				usDeclaration.JE_ApplicationCodeInfo.ValueChanged += JE_ApplicationCodeInfo_ValueChanged;
				usDeclaration.OnBondedWarehouseRelatedFieldChanged += ShowOrHideWhsPacksTabPage;
				ShowOrHideWhsPacksTabPage(this, EventArgs.Empty);
			}
		}

		void US_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHidePackingTabPage();
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			RemoveControl(InvoiceLinesTabPage);
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				JobDeclaration usDeclaration = (JobDeclaration)declaration;
				usDeclaration.US_TariffTypeInfo.ValueChanged -= US_TariffTypeInfo_ValueChanged;
				usDeclaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				usDeclaration.US_EntryTypeInfo.ValueChanged -= US_EntryTypeInfo_ValueChanged;
				usDeclaration.JE_ApplicationCodeInfo.ValueChanged -= JE_ApplicationCodeInfo_ValueChanged;
				usDeclaration.OnBondedWarehouseRelatedFieldChanged -= ShowOrHideWhsPacksTabPage;
			}
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		void US_TariffTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RemoveControl(InvoiceLinesTabPage);
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideStatusTabPage();
			ShowOrHideInvoiceGroupTabPage();
		}

		#region WHS Packs Tab Visiblity

		void ShowOrHideWhsPacksTabPage(object sender, EventArgs e)
		{
			if (WHSPacksTabPage != null)
			{
				WHSPacksTabPage.TabRelevant = JobDeclaration != null && JobDeclaration.IsInwardBondedWarehousingEnabled;
			}
		}

		#endregion

		#region Group Invoice Tab Visiblity

		void ShowOrHideStatusTabPage()
		{
			StatusTabPage.TabRelevant = JobDeclaration != null && !JobDeclaration.IsExport;
		}

		void ShowOrHideInvoiceGroupTabPage()
		{
			InvoiceGroupingTabPage.TabRelevant = JobDeclaration != null && !JobDeclaration.IsExport;
		}

		#endregion

		void ReOrderTabPages()
		{
			List<TabPage> tabPages = new List<TabPage>();
			foreach (TabPage tabPage in MainTabControl.Controls)
			{
				tabPages.Add(tabPage);
			}
			foreach (TabPage tabPage in tabPages)
			{
				MainTabControl.Controls.Remove(tabPage);
				MainTabControl.Controls.Add(tabPage);
			}
		}

		protected override void RemoveUserControlOfEachTabPage()
		{
			base.RemoveUserControlOfEachTabPage();
			RemoveControl(PackingTabPage);
		}

		#endregion

		#region Create New User Controls for each tab

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

			if (JobDeclaration.IsExport)
			{
				result = new USExportSupplierHeaderUserControl();
			}
			else
			{
				result = new USImportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;

			if (JobDeclaration.IsExport)
			{
				result = new USExportInvoiceLineUserControl();
			}
			else
			{
				if (JobDeclaration.IsACE)
				{
					result = new USACEImportInvoiceLineUserControl();
				}
				else
				{
					result = new USACSImportInvoiceLineUserControl();
				}
			}

			return result;
		}

		public override void LoadInvoiceLinesTabPage()
		{
			base.LoadInvoiceLinesTabPage();

			if (MainTabControl.SelectedTab == InvoiceLinesTabPage && JobDeclaration != null && JobDeclaration.Invoices.Count == 0)
			{
				((IInvoicesProvider)JobDeclaration).AddDefaultInvoice();
				JobDeclaration.HasChanges = true;
			}
		}

		protected override Customs.GUI.BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new USJobDeclarationUserControl();
		}

		protected override Customs.GUI.BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl()
		{
			return new GroupInvoiceUserControl();
		}

		protected override Customs.GUI.IBasePackingControl GetPackingUserControl()
		{
			if (JobDeclaration.IsImport)
			{
				return new ImportPackingUserControl();
			}
			else
			{
				return new Customs.GUI.BasePackingControl();
			}
		}

		protected override Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl()
		{
			if (JobDeclaration.IsExport)
			{
				return new AESMessageUserControl();
			}
			else
			{
				return new ImportMessagesUserControl();
			}
		}

		protected override Customs.GUI.BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new ContainerUserControl();
		}

		protected override Customs.GUI.BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new MiscOptionsUserControl();
		}

		protected override void MiscOptionsUserControlShown(BaseJobDeclaration jobDeclaration)
		{
			var declaration = jobDeclaration as JobDeclaration;
			if (declaration != null && declaration.IsReconIndicatorsDirty)
			{
				ReconIssueCalculator.RecalculateIndicators(declaration);
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				WHSPacksTabPage.LazyCreateControls -= WHSPacksTabPage_LazyCreateControls;
				MiscOptionsTabPage.Enter -= MiscOptionsTabPage_Enter;
			}

			base.Dispose(disposing);
		}
	}
}
