using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Organisation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CommunicationGrid : ZUserControl
	{
		public CommunicationGrid()
		{
			InitializeComponent();

			InnerGrid.ColourDeciding += InnerGrid_ColourDeciding;
		}

		#region Appearance

		void InnerGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var communication = (OrgSalesCall)e.ObjectAtRow;
			if (communication.IsClosed)
			{
				e.Colour = Color.LightGray;
			}
			else
			{
				e.Colour = Color.White;
			}
		}

		#endregion

		#region Properties

		new OrgSalesCallCollection DataSource
		{
			get { return base.DataSource as OrgSalesCallCollection; }
		}

		public ZGridWithoutColumnStylesSerialisation InnerGrid
		{
			get { return moduleButtonGrid.InnerGrid; }
		}

		#endregion

		#region Load / Dispose

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetupNewButton();
			SetupEditButton();
			AddPopupContextMenu();

			//the CommunicationGrid is not derived from ZModuleButtonGrid.
			//so the DataSource.ReadOnly is not set automatically and sequentially.
			//we have to handle the 'ParentForm.Shown' event to read DataSource.ReadOnly correctly.
			//the DataSource.ReadOnly is false when SetDataBinding() is called
			if (ParentForm != null)
			{
				ParentForm.Shown += (sender, evt) =>
				{
					var isReadOnly = false;

					if (DataSource != null)
					{
						isReadOnly = DataSource.ReadOnly;
					}
					else
					{
						var bizObj = base.DataSource as BusinessObject; //we have to try base.DataSource, the CommunicationGrid.DataSource may not get the DataSource correctly.
						isReadOnly = (bizObj != null && bizObj.ReadOnly);
					}

					if (isReadOnly)
					{
						editButton.Enabled = NewButton.Enabled = false;
					}
				};
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (universalCopyManager != null)
				{
					universalCopyManager.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (!DesignModeFinder.IsDesigning)
			{
				LoadShowNotesSettings();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				SaveShowNotesSettings();
			}

			base.OnHandleDestroyed(e);
		}

		#endregion

		#region New

		void SetupNewButton()
		{
			NewButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
			NewButton.DropDown.Items.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("4c9f1df4-b511-41a3-b1f9-93e19e27207e", "New"), newMenuItem_Click));
			NewButton.DropDown.Opening += newButtonContextMenu_Opening;
		}

		void newMenuItem_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Communication);
			((ICommunicationController)controller).CreateNewWithParentFormBizObjDefaults = true;

			var newCommunication = (OrgSalesCall)moduleButtonGrid.GetNewBusinessEntity(controller);
			if (newCommunication != null)
			{
				var showOrgSalesCallArgs = new FormShowingForOrgSalesCallArgs(newCommunication);
				OnFormShowingForNewOrgSalesCall(showOrgSalesCallArgs);
				if (!showOrgSalesCallArgs.Cancelled)
				{
					controller.SetFormsModalTo(ParentForm);
					var newForm = controller.ShowFormForNewEntity(newCommunication) as ZForm;
					if (newForm != null)
					{
						newForm.Saved += newForm_Saved;
						newForm.FormClosed += newForm_FormClosed;
					}
				}
			}
		}

		void newForm_Saved(object sender, EventArgs e)
		{
			if (OnNewFormSaved != null)
			{
				OnNewFormSaved(sender, e);
			}
			((ZForm)sender).Saved -= newForm_Saved;
		}

		void newForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			((ZForm)sender).Saved -= newForm_Saved;
			((ZForm)sender).FormClosed -= newForm_FormClosed;
		}

		public event EventHandler OnNewFormSaved;

		void newButtonContextMenu_Opening(object sender, EventArgs e)
		{
			AddUniversalCopyButton();
		}

		void AddUniversalCopyButton()
		{
			if (NewButton != null && NewButton.DropDown != null)
			{
				var form = ParentForm as ZForm;
				if (form != null)
				{
					var relatedCommunicationForm = form as RelatedCommunicationForm;
					if (relatedCommunicationForm != null)
					{
						var relatedCommunicationFormOwner = relatedCommunicationForm.Owner as ZForm;
						if (relatedCommunicationFormOwner != null)
						{
							form = (ZForm)relatedCommunicationForm.Owner;
						}
					}

					universalCopyManager = ObjectFactory.New<IGridUniversalCopyManager>(InnerGrid);
					InnerGrid.ReadOnly = false; // temporarily make not readonly - universal copy disabled for readonly grids
					if (universalCopyManager.AllowsUniversalCopy)
					{
						var universalCopyMenuItem = new ZMenuItem(ResString.GetMultilingualString("9fa84361-23aa-446e-bccc-d208774baf4f", "Universal Copy"));
						universalCopyManager.AddMenuItems(universalCopyMenuItem, includeEditMenuItems: true, includeCopySchedulesItem: true, lazyPopulate: false);
						universalCopyManager.FormShowingForNewElement += universalCopyManager_FormShowingForNewElement;
						NewButton.DropDown.Items.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(universalCopyMenuItem));
					}
					else
					{
						universalCopyManager.Dispose();
						universalCopyManager = null;
					}
					InnerGrid.ReadOnly = true;

					NewButton.DropDown.Opening -= newButtonContextMenu_Opening;
				}
			}
		}

		void universalCopyManager_FormShowingForNewElement(object sender, FormShowingForElementArgs e)
		{
			var newCommunication = e.Element as OrgSalesCall;
			if (newCommunication != null)
			{
				var showOrgSalesCallArgs = new FormShowingForOrgSalesCallArgs(newCommunication);
				OnFormShowingForNewOrgSalesCall(showOrgSalesCallArgs);
				if (showOrgSalesCallArgs.Cancelled)
				{
					e.Cancelled = true;
				}
			}
		}

		#region FormShowingForNewOrgSalesCall

		protected virtual void OnFormShowingForNewOrgSalesCall(FormShowingForOrgSalesCallArgs e)
		{
			if (FormShowingForNewOrgSalesCall != null)
			{
				FormShowingForNewOrgSalesCall(this, e);
			}
		}
		public event EventHandler<FormShowingForOrgSalesCallArgs> FormShowingForNewOrgSalesCall;

		public class FormShowingForOrgSalesCallArgs : EventArgs
		{
			public bool Cancelled;
			public readonly OrgSalesCall OrgSalesCall;

			public FormShowingForOrgSalesCallArgs(OrgSalesCall orgSalesCall)
			{
				OrgSalesCall = orgSalesCall;
			}
		}

		#endregion

		IGridUniversalCopyManager universalCopyManager;

		#endregion

		#region Edit

		void SetupEditButton()
		{
			editButton.Image = editButton.Image = Icons.GetImage(IconTypes.EditButtonRest);
		}

		void editButton_Click(object sender, EventArgs e)
		{
			moduleButtonGrid.PerformEditButtonClick();
		}

		#endregion

		#region NotesTextBox

		void LoadShowNotesSettings()
		{
			ShowNotesCheckBox.Checked = OrganisationGuiState.LoadInteger(ShowNotesCheckBox, "IsChecked") == 1;

			var savedSplitterDistance = OrganisationGuiState.LoadInteger(ShowNotesCheckBox, "SplitterDistance");
			if (savedSplitterDistance > 0)
			{
				MainSplitContainer.SplitterDistance = savedSplitterDistance;
			}
			SetNotesTextBoxVisibility();
		}

		void SaveShowNotesSettings()
		{
			OrganisationGuiState.SaveInteger(ShowNotesCheckBox, "IsChecked", ShowNotesCheckBox.Checked ? 1 : 0);
			OrganisationGuiState.SaveInteger(ShowNotesCheckBox, "SplitterDistance", MainSplitContainer.SplitterDistance);
		}

		void showNotesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SetNotesTextBoxVisibility();
		}

		void SetNotesTextBoxVisibility()
		{
			MainSplitContainer.Panel2Collapsed = !ShowNotesCheckBox.Checked;
		}

		#endregion

		#region Popup

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public bool ShowPopupContextMenu
		{
			get { return showPopupContextMenu; }
			set
			{
				if (popupMenuItem != null)
				{
					popupMenuItem.Visible = value;
				}
				if (popupMenuItemSeparator != null)
				{
					popupMenuItemSeparator.Visible = value;
				}
				showPopupContextMenu = value;
			}
		}
		bool showPopupContextMenu = true;
		ZMenuItem popupMenuItem;
		ZMenuItem popupMenuItemSeparator;

		void AddPopupContextMenu()
		{
			if (popupMenuItem == null)
			{
				var menuItemContainer = InnerGrid.ContextMenu.MenuItems;

				popupMenuItem = new ZMenuItem(ResString.GetMultilingualString("fdc1d7f1-b799-492e-99a5-b4b2423e9d1b", "Popup"), popup_Click);
				popupMenuItem.Visible = ShowPopupContextMenu;
				popupMenuItemSeparator = new ZMenuItem("-");
				popupMenuItemSeparator.Visible = ShowPopupContextMenu;

				menuItemContainer.Add(0, popupMenuItem);
				menuItemContainer.Add(1, popupMenuItemSeparator);
			}
		}

		void popup_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(new RelatedCommunicationForm((OrgSalesCallCollection)BindingSource.Current), ParentForm);
		}

		#endregion

		#region Classes

		public class ModuleGrid : ZModuleButtonGrid
		{
			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				toolStrip.Visible = false;
				mainLayoutPanel.RowCount = 1;
			}

			new internal IBusiness GetNewBusinessEntity(ZController controller)
			{
				return base.GetNewBusinessEntity(controller);
			}

			protected override bool AllowDoubleClick
			{
				get { return true; }
			}

			public void PerformEditButtonClick()
			{
				EditButton_Click(this, null);
			}
		}

		#endregion
	}
}
