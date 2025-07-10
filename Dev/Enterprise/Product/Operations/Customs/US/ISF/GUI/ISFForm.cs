using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.GUI
{
	public partial class ISFForm : ZTemplateForm
	{
		public ISFForm()
		{
			InitializeComponent();
			InitializeComponentAfterSizeSet();
		}

		public ISFForm(CusISFHeader header)
			: base(header)
		{
			InitializeComponent();
			InitializeComponentAfterSizeSet();
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 3);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.AddJobInvoicing(header.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			WorkflowTabPage.Initialize(header);
			AddMessagingMenu();
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("US|ISF|Menu|ImportCommercialInvoice", "Import Commercial Invoice"), ImportCommercialInvoice));
			header.BF_EntryTypeInfo.ValueChanged += BF_EntryTypeInfo_ValueChanged;
			header.BF_OH_ImporterInfo.ValueChanged += BF_OH_ImporterInfo_ValueChanged;
			SetUpDataTransferMenu();
			ManufacturersUserControl.ManufacturerDocAddressControl.BindToOrganisations = "Lookups+Consignors";
			ManufacturersUserControl.ManufacturerDocAddressControl.BindToContacts = "ManufacturerAddresses.Organisation+Contacts";
			ExtraShipToPartiesUserControl.ExtraShipToPartyDocAddressControl.BindToOrganisations = "Lookups+Organisations";
			ExtraShipToPartiesUserControl.ExtraShipToPartyDocAddressControl.BindToContacts = "ExtraShipToPartyAddresses.Organisation+Contacts";
			SetPartAttributeCaptionsByOrg(BusinessEntity.Importer);
			BillsGrid.FindBoxColumnModuleShowing += BillsGrid_FindBoxColumnModuleShowing;
		}

		void BillsGrid_FindBoxColumnModuleShowing(object sender, FindBoxColumnModuleShowingEventArgs e)
		{
			if (e.ColumnStyle.MappingName == "DeclarationJobNumber")
			{
				var bill = (CusISFBill)e.CurrentBusinessObject;
				if (bill.Header != null && bill.Header.Branch != null && bill.Header.Branch.Company.PK != GlbCompany.CurrentCompany.PK)
				{
					e.ModuleID = ModuleIDs.NotAssigned;
					Globals.Message.ShowError(Res.GetString("6970BAD1-1015-450B-8436-2B83EE6BCF41", "You cannot view a declaration in a different company. Please log into the '{0}'.", bill.Header.Branch.Company.GC_Name));
				}
			}
		}

		public override bool IsResizableByTabPageAllowed
		{
			get { return true; }
		}

		void InitializeComponentAfterSizeSet()
		{
			// this need to be set after the Size of the controls have been set... it's a bug in VS Designer serialisation
			ExtraShipToPartiesSplitContainer.Panel2MinSize = 280;
			EquipmentReferenceDataSplitContainer.Panel2MinSize = 250;
			ManufacturerLinesSplitContainer.Panel2MinSize = 150;
		}

		public new CusISFHeader BusinessEntity
		{
			get { return (CusISFHeader)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get
			{
				var aCaption = Res.GetString("ISFForm|FormCaption", "Importer Security Filing");
				if (!this.IsDesignMode())
				{
					aCaption += " - " + BusinessEntity.HumanReadableName;
				}
				return aCaption;
			}
		}

		void AddMessagingMenu()
		{
			var messagingMenutItem = new EDIMenu(BusinessEntity);
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenutItem);
		}

		#region Export To Xml

		void SetUpDataTransferMenu()
		{
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItems);
		}

		List<MenuItem> ExportToXmlMenuItems
		{
			get
			{
				return new ExportToXmlMenuItemSet<CusISFHeader>(() => Exporter, BusinessEntity);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				var director = new XmlDataTransferExporter(new ImporterSecurityFilingDataAdapter(), true);
				director.DefaultFileName = BusinessEntity.BF_JobReference + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss");
				return director;
			}
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var customFieldsControl1 = CusISFDetailsUserControl.FindSingle<ProcessTemplateCustomFieldsControl>("shipmentCustomFieldsControl1");
			customFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup ISF custom fields in Workflow Manager";
			SetControlVisibility();
		}

		void BF_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetControlVisibility();
		}

		void SetControlVisibility()
		{
			var tab = CusISFDetailsUserControl.FindSingle<ZTabPage>("zTabISFType");
			tab.Controls.Clear();

			if (BusinessEntity.IsISF5Entry)
			{
				tab.Text = "ISF 5";
				tab.Controls.Add(ISF5UserControl);
			}
			else
			{
				tab.Text = "ISF 10";
				tab.Controls.Add(ISF10UserControl);
			}

			tab.ResumeLayout(false);
		}

		void BF_OH_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			SetPartAttributeCaptionsByOrg(BusinessEntity.Importer);
		}

		#region ISF5UserControl

		ISF5UserControl ISF5UserControl
		{
			get
			{
				if (isf5UserControl == null)
				{
					isf5UserControl = new ISF5UserControl();
					isf5UserControl.Dock = DockStyle.Fill;
				}

				return isf5UserControl;
			}
		}
		ISF5UserControl isf5UserControl;

		#endregion

		#region ISF10UserControl

		ISF10UserControl ISF10UserControl
		{
			get
			{
				if (isf10UserControl == null)
				{
					isf10UserControl = new ISF10UserControl();
					isf10UserControl.Dock = DockStyle.Fill;
				}

				return isf10UserControl;
			}
		}
		ISF10UserControl isf10UserControl;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (BusinessEntity != null)
				{
					BusinessEntity.BF_EntryTypeInfo.ValueChanged -= BF_EntryTypeInfo_ValueChanged;
					BusinessEntity.BF_OH_ImporterInfo.ValueChanged -= BF_OH_ImporterInfo_ValueChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}

				if (isf5UserControl != null)
				{
					isf5UserControl.Dispose();
				}

				if (isf10UserControl != null)
				{
					isf10UserControl.Dispose();
				}

				BillsGrid.FindBoxColumnModuleShowing -= BillsGrid_FindBoxColumnModuleShowing;
			}
			base.Dispose(disposing);
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			if (BusinessEntity.IsCancelled && MessageStatusList.HasActiveMessageInCustoms(BusinessEntity.BF_CustomsStatus))
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("ISFForm|DeactiveNotAllow", DeactiveIsNotAllowed, BusinessEntity.BF_JobReference));
			}
			else
			{
				base.Save(factories);
			}
		}

		internal const string DeactiveIsNotAllowed = "Importer Security Filing {0} cannot be deactivated because it has been accepted by or waiting responses from US Customs. It would need to be DELETED from US Customs first";

		void ImportCommercialInvoice(object sender, EventArgs e)
		{
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.CommercialInvoice, Core.Constants.CountryCodes.UnitedStates);
			var popup = new EmbeddedModulePopup(module);

			var decisionProvider = new CommercialInvoicePopupDecisionProvider(popup, module.ModuleDecisionProvider, ImportSelectedCommercialInvoice);
			module.OverrideModuleDecisionProvider(decisionProvider);
			popup.EmbeddedModulePopupOKButtonStrategy = decisionProvider;
			popup.Load += (o, args) => SetupCommercialInvoiceFilters(module);

			ZFormModaliser.Show(popup, this);
		}

		public const string ImporterSupplierFilterName = "Importer / Supplier";
		public const string ReferencesFilterName = "References";
		public const string ReferenceTypeFilterPropertyName = "ReferenceType";

		void SetupCommercialInvoiceFilters(ZFilterGridModule module)
		{
			var filterStripControl = (CommercialInvoiceFilterControl)module.DisplayGrid.GetContainerControl();

			if (filterStripControl == null)
			{
				return;
			}

			if (BusinessEntity.BF_OH_Importer.IsValid)
			{
				var emptyFilterStrip = filterStripControl.GetEmptyFilterStrip();
				var filterStrip = emptyFilterStrip ?? filterStripControl.AddNewFilterStrip();
				filterStrip.CurrentDataItem.FilterDescription = ImporterSupplierFilterName;
				((ModuleGuidsFilter)filterStrip.CurrentDataItem.CurrentModuleFilter).Property1 = BusinessEntity.BF_OH_Importer;
			}

			HashSet<ZString> houseBills = new HashSet<ZString>();
			HashSet<ZString> masterBills = new HashSet<ZString>();
			foreach (var data in BusinessEntity.ReferenceDatas)
			{
				if (data.IsHouseBillOfLading)
				{
					houseBills.Add(data.BB_BillNum);
				}
				if (data.IsMasterBillOfLading || data.IsOceanBillOfLading)
				{
					masterBills.Add(data.BB_BillNum);
				}
			}

			AddReferenceFilters(filterStripControl, FilterOrCategory.Red, InvoiceHeaderRefsTypeList.Codes.HB, houseBills);
			AddReferenceFilters(filterStripControl, FilterOrCategory.Green, InvoiceHeaderRefsTypeList.Codes.MB, masterBills);
		}

		void AddReferenceFilters(CommercialInvoiceFilterControl filterStripControl, FilterOrCategory orCategory, string refType, ICollection<ZString> numbers)
		{
			if (!(numbers.Count > 0))
			{
				return;
			}
			foreach (var num in numbers.OrderBy(n => n))
			{
				AddReferencesFilter(filterStripControl, orCategory, refType, ModuleTextFilter.ComparisonConstants.StartsWith, num);
			}
			AddReferencesFilter(filterStripControl, orCategory, refType, ModuleTextFilter.ComparisonConstants.NotFound, ZString.Empty);
		}

		void AddReferencesFilter(CommercialInvoiceFilterControl filterStripControl, FilterOrCategory orCategory, string refType, string comparisonOperator, string text)
		{
			var emptyFilterStrip = filterStripControl.GetEmptyFilterStrip();

			var filterStrip = emptyFilterStrip ?? filterStripControl.AddNewFilterStrip();
			filterStrip.CurrentDataItem.FilterDescription = ReferencesFilterName;
			filterStrip.CurrentDataItem.OrCategory = orCategory;

			var filter = filterStrip.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter;
			if (filter != null)
			{
				filter.ComparisonOperator = comparisonOperator;
				filter.Property = text;
				filter[ReferenceTypeFilterPropertyName] = refType;
			}
		}

		void ImportSelectedCommercialInvoice(BusinessObject selectedBusinessObject)
		{
			var invoice = selectedBusinessObject as JobComInvoiceHeader;
			if (invoice != null)
			{
				new ISFInvoiceImporter().Import(BusinessEntity, invoice);
			}
		}

		void SetPartAttributeCaptionsByOrg(OrgHeader orgHeader)
		{
			if (orgHeader != null)
			{
				LinesGrid.SetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib1, orgHeader.PartAttributeManager.PartAttributeName1);
				LinesGrid.SetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib2, orgHeader.PartAttributeManager.PartAttributeName2);
				LinesGrid.SetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib3, orgHeader.PartAttributeManager.PartAttributeName3);
			}
			else
			{
				LinesGrid.SetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib1, Res.GetString("1892be5c-3284-4982-a627-58c479b65a6d", "Part Attrib. 1"));
				LinesGrid.SetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib2, Res.GetString("078608fd-38fb-4b8f-950e-5d0c1d6e381c", "Part Attrib. 2"));
				LinesGrid.SetColumnCaption(CusISFLineSchema.Constants.BL_PartAttrib3, Res.GetString("6c856af6-aaea-4edb-b37b-cf376163d51a", "Part Attrib. 3"));
			}
		}
	}

	internal class CommercialInvoicePopupDecisionProvider : IModuleDecisionProvider
	{
		readonly EmbeddedModulePopup popup;
		readonly IModuleDecisionProvider originalDecisionProvider;
		readonly Action<BusinessObject> handleSelect;

		public CommercialInvoicePopupDecisionProvider(EmbeddedModulePopup popup, IModuleDecisionProvider originalDecisionProvider, Action<BusinessObject> handleSelect)
		{
			this.popup = popup;
			this.originalDecisionProvider = originalDecisionProvider;
			this.handleSelect = handleSelect;
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			HandleSelect(selectedBusinessObjects);
		}

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
		{
			// we do not want to import invoices on double-click, to avoid accidental import
			originalDecisionProvider.HandleDefaultAction(selectedBusinessObjects);
		}

		void HandleSelect(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length == 1)
			{
				handleSelect(selectedBusinessObjects[0]);
				popup.Close();
			}
			else
			{
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowError(Res.GetString("E329CC20-65B5-4E95-8ACB-510FF1CB041C", "Please select one item."));
			}
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
		}

		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
		}

		public bool ShouldDisplayNotifications => false;
		public bool ShouldLoadFilterBizObj => false;
		public bool ShouldSaveFilterBizObj => false;
		public bool ShouldIgnoreAdditionalFilter => false;
		public bool AllowExcelExport => true;
		public bool EnablePreviousNextSupport => true;
		public IBusinessObjectCollection List => null;
	}
}
