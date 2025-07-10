using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCommunicationPreviewPaneControl : ZUserControl
	{
		public OrgCommunicationPreviewPaneControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				CommunicationDetailsControl.ClientGuidFindBox.BindToForDescription = "Header.OH_FullName";
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!filterGridAdded)
			{
				var org = dataSource as OrgHeader;
				if (org != null)
				{
					AddFilterGrid(org);
				}
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		void AddFilterGrid(OrgHeader org)
		{
			filterGridAdded = true;

			CommunicationModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Communication);
			((ICommunicationModule)CommunicationModule).Header = org;

			FilterStripControl = (ZFilterStripCommonControl)CommunicationModule.EmbeddedControl;
			FilterStripControl.Dock = DockStyle.Fill;
			FilterPanel.Controls.Add(FilterStripControl);

			var activeGridCollection = FilterStripControl.GridCollection as IActiveBusinessObjectCollection;
			if (activeGridCollection != null)
			{
				activeGridCollection.AdditionalFilter = ZQuery.NoResultQuery;
			}

			CommunicationDetailsControl.SetDataBinding(FilterStripControl.GridCollection, "");
			CallNoteRichTextBox.SetDataBinding(FilterStripControl.GridCollection, OrgSalesCallSchema.OQ_SalesCallNotes.Name);
			FollowupNoteRichTextBox.SetDataBinding(FilterStripControl.GridCollection, OrgSalesCallSchema.OQ_FollowupNotes.Name);
			TradeProfileLookupCheckedListBox.DataBindings.Add(new KBinding("BindingItems", FilterStripControl.GridCollection, "TradeProfileDescriptionList"));
			CustomFieldsControl.SetDataBinding(FilterStripControl.GridCollection, "");
			CommunicationRelatedActivityControl.SetDataBinding(FilterStripControl.GridCollection, "");
		}
		bool filterGridAdded;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				NotesTabPage.SetupSecurity(Env.Security.CommunicationManagerViewNotes);
				FollowUpNotesTabPage.SetupSecurity(Env.Security.CommunicationManagerViewFollowUpNotes);

				if (CommunicationModule?.FormActionMenu != null)
				{
					foreach (var menuItem in CommunicationModule.FormActionMenu)
					{
						var newMenuItem = menuItem != null
							? MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, CommunicationModule)
							: null;

						if (newMenuItem != null)
						{
							ToolStrip.Items.Add(newMenuItem);
						}
					}
				}

				CommunicationReportMenuItem = new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.CommunicationReport", "Communication Report"), new EventHandler(CommunicationReport_Click));

				if (FilterStripControl?.Grid?.ContextMenu != null)
				{
					FilterStripControl.Grid.ContextMenu.MenuItems.Add(CommunicationReportMenuItem);
					FilterStripControl.Grid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);

					CommunicationDetailsControl.SetReadOnlyIncludingChildren();

					var form = FindForm() as ZForm;
					if (form != null && form.DisplayMode == ODisplayMode.ReadOnly)
					{
						CommunicationDetailsControl.SetButtonsReadOnly();
					}
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (CommunicationModule != null)
			{
				CommunicationModule.Dispose();
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		internal ZFilterGridModule CommunicationModule;
		internal ZFilterStripCommonControl FilterStripControl;
		internal MenuItem CommunicationReportMenuItem;

		protected void ContextMenu_Popup(object sender, EventArgs e)
		{
			CommunicationReportMenuItem.Enabled = (FilterStripControl.Grid.SelectedElements.Length > 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void CommunicationReport_Click(object sender, EventArgs e)
		{
			const string communicationReportMenuName = "Communication Report";
			var org = (OrgHeader)CurrentDataItem;

			org.SalesCalls.ShouldCheckIncludeOnSalesCallDocument = true;

			foreach (OrgSalesCall salesCall in FilterStripControl.Grid.SelectedElements)
			{
				var salesCallOnOrg = org.SalesCalls.FindByPK(salesCall.PK) as OrgSalesCall;
				if (salesCallOnOrg != null)
				{
					salesCallOnOrg.ShouldIncludeOnSalesCallDocument = true;
				}
			}

			try
			{
				var documentRunner = new DocumentRunner();
				var documentSupportable = org as DocumentEngineCore.DocumentSupport.IDocumentSupportable;
				var documentCommand = DocumentCommand.GetDocumentCommand(org.Factory, documentSupportable, communicationReportMenuName, true, DocumentsDataRegistry.Instance.UseNewDocBuilderOrganizationDocumentsOnly.Value);
				documentCommand.Parent = documentSupportable;
				documentRunner.Run(documentCommand);
			}
			finally
			{
				org.SalesCalls.ShouldCheckIncludeOnSalesCallDocument = false;

				foreach (OrgSalesCall salesCall in org.SalesCalls)
				{
					salesCall.ShouldIncludeOnSalesCallDocument = false;
				}
			}
		}
	}
}
