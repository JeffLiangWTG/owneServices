using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using static System.FormattableString;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSupplierPartForm : ZTemplateForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetByProductSupportColumnsAvailability();
		}

		protected override bool ShowAuditTab => true;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		protected virtual void UnsubscribeHandlers()
		{
			if (BillOfMaterialsGrid != null)
			{
				BillOfMaterialsGrid.CurrentCellChanged -= CurrentObjectChanged;
				BillOfMaterialsGrid.DoubleClick -= TryEditBO;
			}
		}

		#region Constructors

		public OrgSupplierPartForm()
			: base()
		{
		}

		public OrgSupplierPartForm(OrgSupplierPart part)
			: base(part)
		{
			this.Part = part;
			MinimumSize = Size;

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.LandedCostingHistoryView, 5);
			BottomTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.SupplierPart, Env.Security.CustomsSupplierPartModifyCustoms, 3);
			BottomTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.WhsConfigProduct, 4); // Same index as Aqis incase Aqis is unused

#if DEBUG
			TypeDescriptor.AddAttributes(PerPalletLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(CubeUnitLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(WeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif

			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItem);

			WorkflowTabPage.Initialize(part);

			if (ControllerID == null)
			{
				ControllerID = ControllerIDs.WhsConfigProduct;
			}
		}

		#endregion

		#region Export To Xml

		List<MenuItem> ExportToXmlMenuItem
		{
			get
			{
				return new ExportToXmlMenuItemSet<OrgSupplierPart>(() => { return Exporter; }, Part);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				return ObjectFactory.Get<IXmlDataTransferExporter>("ProductXmlDataTransferExporter");
			}
		}

		#endregion

		#region ZForm Overrides

		public override string FormCaption
		{
			get
			{
				string result = Res.GetString("OrgSupplierPartForm|FormCaption", "Product");
				if (!Part.OP_PartNum.IsEmpty)
				{
					result += ": " + Part.OP_PartNum;
				}

				if (!Part.OP_Desc.IsEmpty)
				{
					result += " - " + Part.OP_Desc;
				}

				return result;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				ChangeVisibilityOfControl(WeightPerLabel, !string.IsNullOrEmpty(WeightUnitLabel.Text));
				ChangeVisibilityOfControl(CubePerLabel, !string.IsNullOrEmpty(CubeUnitLabel.Text));
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Enterprise.Core.Constants.CountryCodes.UnitedStates
					&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Enterprise.Core.Constants.CountryCodes.Mexico
					&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Enterprise.Core.Constants.CountryCodes.Canada)
				{
					MainTabControl.TabPages.Remove(NMFCTabPage);
					NMFCTabPage.Dispose();
				}

				var controllerToSelect = ControllerIDs.Customs.SupplierPart;
				if (PlugInIDToSelectOnLoaded == ControllerIDs.WhsConfigProduct)
				{
					controllerToSelect = null;
				}
				if (controllerToSelect != null)
				{
					// CM - Hack until I can get Plugin to Bind if it is the first tab on TabControl
					var plugInToSelect = BottomTabControl.PlugIns.GetPlugIn(controllerToSelect);
					if (plugInToSelect != null)
					{
						plugInToSelect.SelectTabPage();
					}
				}
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			// LB - This hack  is try and resolve the initial focus problem
			OP_PartNumBoundTextBox.Focus();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			foreach (ZPlugIn plugIn in BottomTabControl.PlugIns.Instances)
			{
				if (plugIn.ShowPreSaveDialogs() == ContinueWithSave.No)
				{
					result = ContinueWithSave.No;
					break;
				}
			}

			return result;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			foreach (ZPlugIn plugIn in BottomTabControl.PlugIns.Instances)
			{
				plugIn.OnSaveCompletedOrAborted(result == ContinueWithSave.Yes);
			}
			return result;
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		protected ArrayList fControlRenamers = new ArrayList();

		void SetByProductSupportColumnsAvailability()
		{
			var isAvailable = ObjectFactory.Get<Enterprise.Integration.Customs.ISupportedForProcessing>().IsSupportedForProcessing();

			if (!isAvailable)
			{
				SecondaryProductsGroupBox.Visible = false;
				Splitter2.Visible = false;
			}
		}

		void WeightUnitLabel_TextChanged(object sender, EventArgs e)
		{
			ChangeVisibilityOfControl(WeightPerLabel, !string.IsNullOrEmpty(WeightUnitLabel.Text));
		}

		void CubeUnitLabel_TextChanged(object sender, EventArgs e)
		{
			ChangeVisibilityOfControl(CubePerLabel, !string.IsNullOrEmpty(CubeUnitLabel.Text));
		}

		void ChangeVisibilityOfControl(Control controlToChange, bool isVisible)
		{
			controlToChange.Visible = isVisible;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public ZGrid SelectAndReturnPickFaceGrid()
		{
			var plugInToSelect = BottomTabControl.PlugIns.GetPlugIn(ControllerIDs.WhsConfigProduct);
			if (plugInToSelect != null)
			{
				plugInToSelect.SelectTabPage();
			}
			Application.DoEvents();

			return (ZGrid)Controls.Find("zGrid1", true).First();
		}

		public string GetWarehouseTabPageName()
		{
			var plugInToSelect = BottomTabControl.PlugIns.GetPlugIn(ControllerIDs.WhsConfigProduct);

			return Invariant($"{MainTabPage.Name}+{plugInToSelect.TabPage.Name}");
		}

		void BillOfMaterialsGrid_AfterBind(object sender, EventArgs e)
		{
			BillOfMaterialsGrid.CurrentCellChanged += new EventHandler(CurrentObjectChanged);
			BillOfMaterialsGrid.DoubleClick += new EventHandler(TryEditBO);
			if (BillOfMaterialsGrid.List.Count > 0)
			{
				BillOfMaterialsGrid.CurrentRowIndex = 0;
				CurrentObjectChanged(sender, e);
			}
		}

		void BillOfMaterialsGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (BillOfMaterialsGrid.List.Count == 1)
			{
				Part.BillOfMaterialsView.Reload(null);
			}
			else
			{
				CurrentObjectChanged(sender, e);
			}
		}

		protected void TryEditBO(object sender, EventArgs e)
		{
			if (BillOfMaterialsGrid.CurrentRowIndex >= 0)
			{
				OrgPartBOM selectedBOM = (OrgPartBOM)BillOfMaterialsGrid.List[BillOfMaterialsGrid.CurrentRowIndex];
				if (selectedBOM.Component != null)
				{
					SupplierPartController.ShowEditForm(selectedBOM.Component);
				}
			}
		}

		protected ZController SupplierPartController
		{
			get
			{
				if (fSupplierPartController == null)
				{
					fSupplierPartController = ZControllerFactory.Create(ControllerIDs.Customs.SupplierPart);
				}

				return fSupplierPartController;
			}
		}

		ZController fSupplierPartController;

		void CurrentObjectChanged(object sender, EventArgs e)
		{
			if (BillOfMaterialsGrid.CurrentRowIndex >= 0)
			{
				OrgPartBOM selectedBOM = (OrgPartBOM)BillOfMaterialsGrid.List[BillOfMaterialsGrid.CurrentRowIndex];
				if (selectedBOM.OE_OP_Component != ZGuid.Empty)
				{
					if (selectedBOM.OE_OP_ComponentInfo.HasError(OrgPartBOMValidation.circularReferenceError))
					{
						Part.BillOfMaterialsView.Reload(null);
					}
					else
					{
						Part.BillOfMaterialsView.Reload(selectedBOM);
					}
				}
			}
		}

		protected override void ShowDeleteErrorInactiveMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("OrgSupplierPartForm|DeleteErrorInactiveMessage", "This record is in use by other records in the system and can't be deleted. You can mark the record as inactive instead of deleting it."));
		}

		#region Test
#if DEBUG

		public void SelectMainTabPageForTest()
		{
			MainTabControl.SelectedTab = MainTabPage;
		}

		public void SelectPartRelationTabPageForTest()
		{
			BottomTabControl.SelectedTab = RelatedOrgsTabPage;
		}

		public void SelectFirstRelationAndClickLinkLabelTesting()
		{
			relatedOrganizationsControl1.SelectTheFirstRelationAndClickLinkLabelForTesting();
		}

		public void SelectTheRelationAtForTesting(int row)
		{
			relatedOrganizationsControl1.SelectTheRelationAtForTesting(row);
		}

		public void FocusTheRelationsGridForTesting()
		{
			relatedOrganizationsControl1.FocusTheRelationsGridForTesting();
		}

		public OrgPartRelation GetCurrentlySelectedRelationRowForTesting()
		{
			return relatedOrganizationsControl1.GetCurrentlySelectedRelationRowForTesting();
		}

		public bool IsWarehouseTabPageSelected()
		{
			if (TopLevelTabControl.SelectedTab == MainTabPage)
			{
				var plugInToSelect = BottomTabControl.PlugIns.GetPlugIn(ControllerIDs.WhsConfigProduct);
				if (BottomTabControl.SelectedTab == plugInToSelect.TabPage)
				{
					return true;
				}
			}
			return false;
		}

#endif
		#endregion

		#region Dispose

		IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnsubscribeHandlers();
				foreach (CustomLabelControlRenamer renamer in this.fControlRenamers)
				{
					renamer.Dispose();
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
