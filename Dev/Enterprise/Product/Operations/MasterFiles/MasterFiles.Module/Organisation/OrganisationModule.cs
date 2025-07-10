using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module.Organisation.OrgImport;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationModule : ZFilterGridModule, IOperationalActionSupportable, IImportCollectionInfoProvider
	{
		#region Construction

		public OrganisationModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void AddExtraImportExportMenuItems()
		{
			AddImportFromLegacyCSVMenuItem(ResString.GetMultilingualString("20aa8fed-0ed5-4ca0-8a43-7c2209bf1abd", "Import From Legacy CSV"), OnOrgCsvImport);
			AddInterfaceConnectorImportMenuItem(Res.GetString("5d1e8df9-f114-4a1a-8fe9-642899e28a4c", "From XML"), OnOrgXmlImport);
			base.AddExtraImportExportMenuItems();
		}

		void AddImportFromLegacyCSVMenuItem(MultilingualString caption, EventHandler handler)
		{
			handler = new ImportSecurityChecker(handler, Env.Security.OrganisationImportFromLegacyCSV).OnClick;
			ImportMenuItems.Add(caption, handler);
		}

		#endregion

		#region Data Import / Export

		void OnOrgCsvImport(object sender, EventArgs e)
		{
			new ImportOrganisationsFromCSVForm().Show();
		}

		void OnOrgXmlImport(object sender, EventArgs e)
		{
			GetNewOrgDataTransferImporter().PromptUserAndImport(BillingInterfaceName.OrganisationXMLImport);
		}

		IXmlDataTransferImporter GetNewOrgDataTransferImporter()
		{
			return ObjectFactory.Get<IXmlDataTransferImporter>("IXmlDataTransferDirector");
		}

		#region System Merge

		#region Importer
#if DEBUG
		internal
#endif
 IXmlDataTransferImporter GetNewSysMergeImporter(string key)
		{
			var directorTypes = ObjectFactory.Get<Hashtable>("SystemMergeXmlDirectorDictionary");
			var objectHandle = (ObjectHandle)directorTypes[key];
			if (objectHandle != null)
			{
				var result = (IXmlDataTransferImporter)objectHandle.GetObject();
				return result;
			}
			else
			{
				throw new Exception(ZString.Format("The System Merge Director with key {0} doesn't exist", key));
			}
		}

		void OnSystemMergeOrgImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter((NoResString)"Organization").PromptUserAndImport(BillingInterfaceName.SystemMergeOrganisationImport);
		}

		void OnSystemMergeRatingImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter((NoResString)"Rating").PromptUserAndImport(BillingInterfaceName.SystemMergeRatingImport);
		}

		void OnSystemMergeProductImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter((NoResString)"Product").PromptUserAndImport(BillingInterfaceName.SystemMergeProductImport);
		}

		void OnSystemMergeOrgEdocsImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter("EDocs").PromptUserAndImport(BillingInterfaceName.SystemMergeEDocsImport);
		}

		void OnSystemMergeClassificationsImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter((NoResString)"Classifications").PromptUserAndImport(BillingInterfaceName.SystemMergeClassificationsImport);
		}

		void OnSystemMergeWarehousesImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter((NoResString)"Warehouses").PromptUserAndImport(BillingInterfaceName.SystemMergeWarehousesImport);
		}

		void OnSystemMergeWarehouseInventoryImport(object sender, EventArgs e)
		{
			GetNewSysMergeImporter("WarehouseInventory").PromptUserAndImport(BillingInterfaceName.SystemMergeWarehouseInventoryImport);
		}

		#endregion

		#region Exporter
#if DEBUG
		internal
#endif
 IXmlDataTransferExporter GetNewSysMergeExporter(string key)
		{
			var directorTypes = ObjectFactory.Get<Hashtable>("SystemMergeXmlExporterDictionary");
			var objectHandle = (ObjectHandle)directorTypes[key];
			if (objectHandle != null)
			{
				var result = (IXmlDataTransferExporter)objectHandle.GetObject();
				return result;
			}
			else
			{
				throw new Exception(ZString.Format("The System Merge Exporter with key {0} doesn't exist", key));
			}
		}

		void OnSystemMergeOrgExport(object sender, EventArgs e)
		{
			IList orgs = GetOrganisationsToExport();
			GetNewSysMergeExporter((NoResString)"Organization").PromptUserAndExport(orgs);
		}

		void OnSystemMergeRatingExport(object sender, EventArgs e)
		{
			IList orgs = GetOrganisationsToExport();
			GetNewSysMergeExporter((NoResString)"Rating").PromptUserAndExport(orgs);
		}

		void OnSystemMergeProductExport(object sender, EventArgs e)
		{
			if (this.GridCollection.Count > 0)
			{
				IList orgs = GetOrganisationsToExport();
				GetNewSysMergeExporter((NoResString)"Product").PromptUserAndExport(orgs);
			}
			else
			{
				var query = ExportQuery;
				query.MaximumRows = null;
				GetNewSysMergeExporter((NoResString)"Product").PromptUserAndExport(query);
			}
		}

		void OnSystemMergeOrgEdocsExport(object sender, EventArgs e)
		{
			IList orgs = GetOrganisationsToExport();
			GetNewSysMergeExporter("EDocs").PromptUserAndExport(orgs);
		}

		#region OnSystemMergeClassificationsExport

		void OnSystemMergeClassificationsExport(object sender, EventArgs e)
		{
			IList classifications = GetClassificationsToExport();
			if (classifications.Count > 0)
			{
				GetNewSysMergeExporter((NoResString)"Classifications").PromptUserAndExport(classifications);
			}
		}

		IList GetClassificationsToExport()
		{
			ZQuery query = new ZQuery(CusClassificationSchema.CC_IsActive, true);
			Type classificationType = ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseCusClassification));
			return new BusinessObjectListReader(query, classificationType);
		}

		#endregion

		#region OnSystemMergeWarehousesExport

		void OnSystemMergeWarehousesExport(object sender, EventArgs e)
		{
			var warehouses = GetSelectedWarehouses();
			GetNewSysMergeExporter((NoResString)"Warehouses").PromptUserAndExport(warehouses);
		}

		IList GetSelectedWarehouses()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(WhsWarehouseSchema.WW_IsActive, true);
			var warehouseType = ObjectFactory.GetType<IWhsWarehouse>();
			var result = factory.Load(warehouseType, query);

			return result;
		}

		#endregion

		#region OnSystemMergeWarehouseInventoryExport

		void OnSystemMergeWarehouseInventoryExport(object sender, EventArgs e)
		{
			IList orgs = GetOrganisationsToExport();
			GetNewSysMergeExporter("WarehouseInventory").PromptUserAndExport(orgs);
		}

		#endregion

		IList GetOrganisationsToExport()
		{
			BusinessObjectReader organizationsReader = CollectionForExport;
			IList result = organizationsReader as IList;

			if (result == null)
			{
				result = new List<BusinessObject>();
				foreach (BusinessObject bizObj in organizationsReader)
				{
					result.Add(bizObj);
				}
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Organisation; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.Organisation }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Organisation);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var result = new OrganisationFilterControl(GridCollection, (OrganisationFilterBusinessObject)FilterBusinessObject);
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Canada)
			{
				var newStyleInfo = new ZTextBoxColumnStyleInfo { CaptionResourceString = AccountCodeCaption, ColumnName = OrgHeader.Schema.CAAccountSecurityNumber, IsVisible = false };
				ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);
				result.FilteredGrid.ColumnStyles.Add(newStyleInfo);
			}
			return result;
		}

		static ResourceStringData AccountCodeCaption
		{
			get { return Res.GetData("F9C14BC2-B5EC-4C54-834B-F941045A3C32", "Acc. Sec. No.", "Acc. Security No.", "Account Security Number", ""); }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrganisationFilterBusinessObject();
		}

		protected override void AdditionalSetupForCollectionDefaults(Form parentModalForm)
		{
			if (parentModalForm != null)
			{
				EmbeddedModulePopup popup = parentModalForm as EmbeddedModulePopup;
				if (popup != null && GridCollection is OrganisationsFindBoxCollection)
				{
					((IOrganisationDefaultProvider)GridCollection).ShouldSetValuesFromConditionalDefaults = (popup.FindBoxCode == OrgHeader.UnmatchedOrganisationCode);
				}
			}
		}

		protected override ZString DefaultMessageWhenCreatingANewBizObjFromFindBoxCore(IFindBox findbox)
		{
			ZString result = ZString.Empty;

			if (findbox.Code == OrgHeader.UnmatchedOrganisationCode && findbox.ListProvider != null)
			{
				var orgDefaultProvider = findbox.ListProvider.List as IOrganisationDefaultProvider;
				if (orgDefaultProvider != null && orgDefaultProvider.ConditionalDefaults.Count > 0)
				{
					return Res.GetString("b5d13ec9-c966-4e4f-8263-44515137f86b",
						"Would you like to create a new organization with details defaulted from '{0}' Note?", PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				}
			}

			return base.DefaultMessageWhenCreatingANewBizObjFromFindBoxCore(findbox);
		}

		protected override bool CheckCopySelectedRowsAllowed()
		{
			return ExportSecurityCheckpoint.IsAllowed;
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Menu / Toolbar

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem("-"));
			if (SystemDataRegistry.Instance.ActivateSystemMergeDataInterface.Value)
			{
				result.Add(GetSystemDataMergeMenuItem());
			}
			result.Add(DeleteTemporaryOrgsMenuItem);
			result.Add(MergeOrgMenuItem);
			result.Add(MergeIntoOrgMenuItem);
			result.Add(MergeSelectedOrgsMenuItem);
			result.Add(new ZMenuItem("-"));
			result.Add(ActivateOrgMenuItem);
			result.Add(DeActivateOrgMenuItem);
			result.Add(ReDefaultARAPTaxSettingsMenuItem);
			result.Add(SetOrgSecurityAccessGroupMenuItem);
			result.Add(SetOrgSecurityMenuItem);
			result.Add(GenerateClientNumberMenuItem);
			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				result.Add(EPaymentConfigurationsMenuItem);
			}
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Singapore)
			{
				result.Add(UpdateUENMenuItem);
			}

			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				result.Add(SendImporterBondQueryMenuItem);
			}
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForScreeningEntity(this, result);

			AddExportPatternOverrideMenuItem(result);

			AddTaxConfigurationOrganizationRateFileImportMenuItem(result);

			return result.ToArray();
		}

		MenuItem DeleteTemporaryOrgsMenuItem
		{
			get
			{
				var result = new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.DeleteTemporaryOrgs", "Delete Unused Temp. Organizations"));
				result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.DeleteTemporaryOrgs.ReviewAndDelete", "Review and Delete..."), delegate
				{ ZControllerFactory.Create(ControllerIDs.TemporaryOrgRemover).ShowNewForm(); }));
				result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.DeleteTemporaryOrgs.DeleteAllInBulk", "Delete All In Bulk"), delegate
				{ BulkDeleteTemporaryOrgs(); }));
				return result;
			}
		}

		MenuItem MergeOrgMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.MergeOrg", "&Merge This Organization With..."), delegate { MergeOrganisation(); }); }
		}

		MenuItem MergeIntoOrgMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.MergeInto", "&Merge Into This Organization..."), delegate { MergeIntoOrganisation(); }); }
		}

		MenuItem MergeSelectedOrgsMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.FindAndMergeSelected", "&Find and Merge Similar Organizations..."), delegate { FindAndMergeSimilarOrgs(); }); }
		}

		MenuItem ActivateOrgMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ActivateOrg", "&Activate Organizations..."), new EventHandler(OnActivate)); }
		}

		MenuItem DeActivateOrgMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.DeActivateOrg", "&Deactivate Organizations..."), new EventHandler(OnDeActivate)); }
		}

		MenuItem ReDefaultARAPTaxSettingsMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("6D9B8AC3-11C9-405E-9B6C-67D47B258C3E", "&Re-Default AR/AP Tax Settings"), delegate { ReDefaultARAPTaxSettings(); }); }
		}

		MenuItem SendImporterBondQueryMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("0D52B624-A533-4246-918D-4CDB1E8F56C4", "&Send Importer Bond Query"), delegate { SendImporterBondQuery(); }); }
		}

		MenuItem UpdateUENMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("9806d7cc-3cef-407e-8ee1-5e11049ed63e", "&UEN Reference Update"), new EventHandler(updateUENReferences)); }
		}

		MenuItem SetOrgSecurityAccessGroupMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("047871a6-69d1-43b1-b5b4-49c93cbff0bb", "&Set Organization Security Access Group"), delegate { SetOrgSecurityAccessGroup(); }); }
		}

		MenuItem SetOrgSecurityMenuItem => new ZMenuItem(ResString.GetMultilingualString("59726eab-4d3d-425e-b7ae-b398b9ab8f42", "&Apply Web Security Profile"), delegate { SetOrgSecurity(); });

		MenuItem GenerateClientNumberMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("97D1EBAB-C59E-4B01-AA97-F077F3062BD0", "&Generate Client Number"), delegate { GenerateClientNumber(); }); }
		}

		MenuItem EPaymentConfigurationsMenuItem
		{
			get
			{
				var result = new ZMenuItem(ResString.GetMultilingualString("B4969FE0-2B34-4F06-BD03-17186454D4F3", "E-Payment Configurations"));
				result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("4AABB576-C45C-481C-ABCE-B062FBA26A9A", "&Match Recipients for E-Payment"), delegate
				{ MatchEPaymentRecipientsBulk(); }));
				return result;
			}
		}

		#region System Merge Menu

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		MenuItem GetSystemDataMergeMenuItem()
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.SystemDataMerge", "System Data Merge ({0} to {1})", "CargoWise", "CargoWise"));

			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportOrganisations", "Import Organizations"), new EventHandler(OnSystemMergeOrgImport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportWarehouses", "Import Warehouses"), new EventHandler(OnSystemMergeWarehousesImport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportClientRatings", "Import Client Ratings"), new EventHandler(OnSystemMergeRatingImport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportProducts", "Import Products"), new EventHandler(OnSystemMergeProductImport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportEdocs", "Import Organization eDocs"), new EventHandler(OnSystemMergeOrgEdocsImport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportActClassifications", "Import Active Classifications"), new EventHandler(OnSystemMergeClassificationsImport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ImportInventory", "Import Warehouse Inventory"), new EventHandler(OnSystemMergeWarehouseInventoryImport)));

			result.MenuItems.Add(new ZMenuItem("-"));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportOrganisations", "Export Organizations"), new EventHandler(OnSystemMergeOrgExport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportWarehouses", "Export Warehouses"), new EventHandler(OnSystemMergeWarehousesExport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportClientRatings", "Export Client Ratings"), new EventHandler(OnSystemMergeRatingExport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportProducts", "Export Products"), new EventHandler(OnSystemMergeProductExport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportEdocs", "Export Organization eDocs"), new EventHandler(OnSystemMergeOrgEdocsExport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportActClassifications", "Export Active Classifications"), new EventHandler(OnSystemMergeClassificationsExport)));
			result.MenuItems.Add(GetSystemMergeSubMenuItem(ResString.GetMultilingualString("MasterFiles.Organisation.ExportInventory", "Export Warehouse Inventory"), new EventHandler(OnSystemMergeWarehouseInventoryExport)));

			return result;
		}

		MenuItem GetSystemMergeSubMenuItem(MultilingualString caption, EventHandler handler)
		{
			var handlerWithSecurityCheck = new EventHandler(SecurityCheckerForImportingData(handler).OnClick);
			var result = new ZMenuItem(caption, handlerWithSecurityCheck);
			return result;
		}

		#endregion

		#region ExportPatternMatchOverride

		protected ExportPatternMatchOverridesUtil ExportPatternMatchOverridesUtil { get { return new ExportPatternMatchOverridesUtil(() => { return SelectedBusinessObjects.Cast<OrgHeader>().ToList(); }); } }

		void AddExportPatternOverrideMenuItem(List<MenuItem> result)
		{
			var dataTransferMenuItem = result.ToArray().Find(ExportXmlMenuItemHelper.DataTransferMenuItemName);
			if (dataTransferMenuItem != null)
			{
				ExportPatternMatchOverridesUtil.AddExportMenuItem(dataTransferMenuItem, ExportXmlMenuItemHelper.NativeExportMenuItemName);
			}
		}

		#endregion ExportPatternMatchOverride

		#region Tax Configuration Organization Rate File Import

		void AddTaxConfigurationOrganizationRateFileImportMenuItem(List<MenuItem> result)
		{
			var orgTaxRateImportFileFormatProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IOrgTaxRateImportFileFormatProvider>)?.Get();

			if (orgTaxRateImportFileFormatProvider != null)
			{
				var menuItem = new ZMenuItem(ResString.GetMultilingualString("4fadffc4-4661-471b-9bea-5473e5ae38be", "Tax Configuration Organization Rate Update File Import"), HandleTaxRateFileImportForm);
				menuItem.Enabled = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().HasAnyTaxConfigurationThatSupportsOrganisationRates(Factory, GlbCompany.CurrentCompany);

				result.Add(menuItem);
			}
		}

		void HandleTaxRateFileImportForm(object sender, EventArgs e)
		{
			if (!Env.Security.OrganisationTaxRateFileImport.IsAllowed)
			{
				Env.Security.OrganisationTaxRateFileImport.ShowError();
				return;
			}

			var form = new OrganisationTaxRateFileImportForm();
			form.Show();
		}

		#endregion

		#endregion

		void ReDefaultARAPTaxSettings()
		{
			if (Env.Security.OrganisationAllowReDefaultARAPTaxSettings.IsAllowed)
			{
				var dialogResult = Globals.Message.Show(Res.GetString("d1ac3ce8-759c-463e-ba90-33b173f0820e", @"This action will re-default the Tax (GST/VAT) settings on ALL Organizations flagged as either a Receivables or Payables Organization Type.
Are you sure you want to re-default your Tax settings?"),
					Res.GetString("930B42E8-B536-483E-A553-750A55E85C6A", "Re-Default AR/AP Tax Settings"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (dialogResult == DialogResult.Yes)
				{
					Globals.Message.Show(Res.GetString("D3B9B357-2819-4299-AC84-31FD6A20E346", "NOTE: Changing the Tax settings will only affect NEW transactions. Transactions ALREADY POSTED for each Creditor / Debtor will NOT change."));

					var newFactory = new BusinessObjectFactory();
					if (OrgHeader.ReDefaultARAPTaxConfigurations(newFactory))
					{
						newFactory.Save();
					}

					Globals.Message.Show(Res.GetString("5334CA8E-2C45-4E05-998A-DF5CF19A9B83", "Tax Settings Re-Defaulted"));
				}
			}
			else
			{
				Globals.Message.Show(Env.Security.OrganisationAllowReDefaultARAPTaxSettings.ErrorMessageForNotAllowed);
			}
		}

		void SetOrgSecurityAccessGroup()
		{
			if (!Env.Security.GroupsManageOSMG.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.GroupsManageOSMG.ErrorMessageForNotAllowed);
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var orgMiscs = newFactory.Load<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, GridSelectedElements));
			if (orgMiscs != null && orgMiscs.Any())
			{
				ZFormModaliser.ShowDialogAndDispose(new OrgSecurityGroupUpdateForm(orgMiscs));
			}
			else
			{
				Globals.Message.ShowError(NonSelectedMessage);
			}
		}

		void SetOrgSecurity()
		{
			if (!Env.Security.OrgDetailsModifyWebSecurity.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.OrgDetailsModifyWebSecurity.ErrorMessageForNotAllowed);
				return;
			}

			var pks = GridSelectedElements;
			if (pks != null && pks.Any())
			{
				ZFormModaliser.ShowDialogAndDispose(new OrgSecurityProfileUpdateForm(new OrgSecurityProfileUpdater(pks), true));
			}
			else
			{
				Globals.Message.ShowError(NonSelectedMessage);
			}
		}

		void GenerateClientNumber()
		{
			if (!Env.Security.OrganisationGenerateClientNumber.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.OrganisationGenerateClientNumber.ErrorMessageForNotAllowed);
				return;
			}

			if (GridSelectedElements.Length > 0)
			{
				var selectedOrg = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GridSelectedElements));
				if (selectedOrg.Any(x => !x.OH_IsDebtor))
				{
					Globals.Message.ShowError(Res.GetString("b9c9810e-7426-4807-912f-9176fc5c0d8f", "You cannot generate a Client Number for non-Receivables organizations."));
					return;
				}
				else
				{
					var orgWithClientNumber = selectedOrg.Select(x => x.CompanyData).Where(x => !string.IsNullOrEmpty(x.OB_ARClientNumber));
					if (orgWithClientNumber.Any())
					{
						Globals.Message.ShowError(Res.GetString("526a15cb-8010-447b-8453-8142b7915295", "Client Number has already been generated for the following organizations: {0}", string.Join(",", orgWithClientNumber.Select(x => x.Organisation.OH_Code))));
						return;
					}

					using (var manager = ((IDbConnected)Factory).Connection.BeginTransactionWithManager())
					{
						selectedOrg.ForEach(x => x.CompanyData.GenerateARClientNumber());
						Factory.Save();
						manager.CommitTransaction();
						Globals.Message.ShowInformation(Res.GetString("710d343b-9edf-40ee-bd77-22bf36684f43", "Client Numbers are generated."));
						return;
					}
				}
			}
			else
			{
				Globals.Message.ShowError(NonSelectedMessage);
			}
		}

		void MatchEPaymentRecipientsBulk()
		{
			if (Env.Security.OrganisationMatchRecipientsForEPayment.IsAllowed)
			{
				ZController matchRecipientsBulkController = ZControllerFactory.Create(ControllerIDs.MatchEPaymentRecipientsBulk);
				matchRecipientsBulkController.ShowChildrenAsDialog = true;
				matchRecipientsBulkController.ShowNewForm();
			}
			else
			{
				Globals.Message.ShowError(Env.Security.OrganisationMatchRecipientsForEPayment.ErrorMessageForNotAllowed);
			}
		}

		string NonSelectedMessage => Res.GetString("04b3b8c2-8453-4201-a99e-8459f6ea3e02", "You have not selected any Organizations.");

		#region Activate/Deactivate Organisation

		protected void OnActivate(object sender, EventArgs e)
		{
			ActivateDeactivate(true);
		}

		protected void OnDeActivate(object sender, EventArgs e)
		{
			var orgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GridSelectedElements));
			var notUNKOrgFound = Array.Find(orgHeaders, x => x.OH_ScreeningStatus != ScreeningStatusesList.Codes.Unknown) != null;

			if (!notUNKOrgFound)
			{
				ActivateDeactivate(false);
			}
			else
			{
				var parentForm = LocateMainForm();

				var screenStatusChangedWarningHeaderAction = new OrganisationActivator.ScreenStatusChangedWarningHeaderAction(() =>
				{
					return Globals.Message.Show(Res.GetString("4a866fa1-6d2b-43a1-b4c9-51a550317f0f", @"Setting organizations to inactive will remove these entities from the Denied Party re-screening process and reset the screening status to UNK, unless their current screening status is CLP.
Would you like to continue?"), Res.GetString("15a098e8-f1bf-4cbb-8995-10bb55bde2cb", "Continue?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
				});

				var organisationsToProceed = ActivateDeactivate(false, screenStatusChangedWarningHeaderAction);
				if (organisationsToProceed.Any() && !OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.Value)
				{
					PromptProgressBarAndUpdateRelatedJobs(parentForm, organisationsToProceed.ToArray());
				}
			}
		}

		List<OrgHeader> ActivateDeactivate(bool activate, OrganisationActivator.ScreenStatusChangedWarningHeaderAction screenStatusChangedWarningHeaderAction = null)
		{
			OrganisationActivator.ActivatingHeaderAction activatingHeaderAction = new OrganisationActivator.ActivatingHeaderAction((org) =>
			{
				var newForm = new ZOrganisationsForm(org);
				newForm.ActivateOrganisationForForm();
				var validateAllMethod = typeof(ZForm).GetMethod("ValidateAll", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				validateAllMethod.Invoke(newForm, new object[] { ValidationType.Full });
				return newForm;
			});

			OrganisationActivator.FailedToActivateHeaderAction failedToActivateHeaderAction = new OrganisationActivator.FailedToActivateHeaderAction((form) =>
			{
				((ZOrganisationsForm)form).ActivateOrganisation();
			});

			OrganisationActivator.ActivateOrDeactivateNotificationHeaderAction notificationHeaderAction = new OrganisationActivator.ActivateOrDeactivateNotificationHeaderAction(Globals.Message.Show);

			return OrganisationActivator.ActivateOrDeactivate(GridSelectedElements, activate, activatingHeaderAction, failedToActivateHeaderAction, notificationHeaderAction, screenStatusChangedWarningHeaderAction);
		}

		void PromptProgressBarAndUpdateRelatedJobs(Form parentForm, OrgHeader[] orgHeaders)
		{
			var progressForm = new ProgressForm();
			progressForm.ShowCancelButton = false;
			progressForm.ShowModalTo(parentForm);
			string msg;
			int itemNumber = 0;
			int itemsCount = orgHeaders.Length;

			ScreeningStatusUpdater.ProgressUpdaterDelegate progressUpdater = (string partyCode) =>
			{
				msg = Res.GetString("e3b9769a-0cc0-486f-8a07-8212b577bf6d", "Processing jobs related to party: {0}", partyCode);
				progressForm.SetStatusAndPercentComplete(msg, (int)(itemNumber / (double)itemsCount * 100));
				itemNumber++;
			};

			ScreeningStatusUpdater.UpdateRelatedJobsForChangedParties(orgHeaders, progressUpdater);
			progressForm.SetStatusAndPercentComplete(Res.GetString("b2ad637b-52cf-45ef-ad40-baa32ad336e8", "Processed all jobs related to all parties."), 100);
			progressForm.Close();
			progressForm = null;
		}

		protected virtual ZGuid[] GridSelectedElements
		{
			get { return Array.ConvertAll(Grid.SelectedElements, x => x.PK); }
		}

		#endregion

		#region Merge Organisation

#if DEBUG
		protected
#endif
 void MergeOrganisation()
		{
			if (Env.Security.OrgDuplicateDetectionMerge.IsAllowed)
			{
				MergeOrganisationCore();
			}
			else
			{
				Env.Security.OrgDuplicateDetectionMerge.ShowError();
			}
		}

#if DEBUG
		protected
#endif
 void MergeIntoOrganisation()
		{
			if (Env.Security.OrgDuplicateDetectionMerge.IsAllowed)
			{
				MergeIntoOrganisationCore();
			}
			else
			{
				Env.Security.OrgDuplicateDetectionMerge.ShowError();
			}
		}

#if DEBUG
		protected virtual
#endif
 void MergeOrganisationCore()
		{
			if (Grid.ListManager.Position >= 0)
			{
				OrgHeader orgToBeMerged = (OrgHeader)Grid.ListManager.GetCurrent();
				if (orgToBeMerged != null && orgToBeMerged.IsInDatabase)
				{
					MergeOrgHeader mergeOrganisation = new MergeOrgHeader(new BusinessObjectFactory(), orgToBeMerged);
					ZFormModaliser.ShowDialogAndDispose(new MergeOrgHeaderForm(mergeOrganisation));
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("da754a78-29c4-4447-b099-5d80ce62db91", "Organization must be saved to the database before merging can occur."));
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 void MergeIntoOrganisationCore()
		{
			if (Grid.ListManager.Position >= 0)
			{
				OrgHeader orgToBeMerged = (OrgHeader)Grid.ListManager.GetCurrent();
				if (orgToBeMerged != null && orgToBeMerged.IsInDatabase)
				{
					if (orgToBeMerged.OH_IsActive)
					{
						MergeOrgHeader mergeOrganisation = new MergeOrgHeader(new BusinessObjectFactory(), null);
						mergeOrganisation.HasToLoadSimilarOrgs = true;
						mergeOrganisation.NewOrganisationPk = orgToBeMerged.PK;
						if (mergeOrganisation.NewOrganisation == null)
						{
							Globals.Message.ShowError(Res.GetString("edc20b63-376c-449e-b860-32d5d3a222fc", "The organization has been deleted, please reload the grid. If the error persists, please contact your system administrator."));
						}
						else
						{
							MergeIntoOrgHeaderForm mergeForm = new MergeIntoOrgHeaderForm(mergeOrganisation);
							mergeForm.Show();
						}
					}
					else
					{
						Globals.Message.Show(Res.GetString("323219cd-9e11-42a2-ab9e-89b83fc6bdec", "This organization is Inactive. You cannot merge into Inactive organizations. Please select an Active organization to merge into."));
					}
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("da754a78-29c4-4447-b099-5d80ce62db91", "Organization must be saved to the database before merging can occur."));
				}
			}
		}

		protected void BulkDeleteTemporaryOrgs()
		{
			BulkTemporaryOrgRemoverGUI remover = new BulkTemporaryOrgRemoverGUI();
			remover.Remove();
		}

		protected void FindAndMergeSimilarOrgs()
		{
			if (Env.Security.OrgDuplicateDetectionMerge.IsAllowed)
			{
				var orgHeaderQuery = new ZQuery();
				if (Grid.SelectedRowCount == 0)
				{
					if (Grid.ListManager != null && Grid.ListManager.Position >= 0)
					{
						var currentOrgHeader = (OrgHeader)Grid.ListManager.GetCurrent();
						orgHeaderQuery.AddToFilter(OrgHeaderSchema.PK, currentOrgHeader.PK);
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("a0c59e73-9cc0-4e9a-8b7e-b3768dcc8188", "Please select a record in the grid."));
						return;
					}
				}
				else
				{
					var selectedOrgs = Grid.SelectedElements.Cast<OrgHeader>().Select(x => x.PK);
					orgHeaderQuery.AddToFilter(OrgHeaderSchema.PK, selectedOrgs);
				}

				ManyToManyOrgMergerGUI merger = new ManyToManyOrgMergerGUI(orgHeaderQuery);
				merger.Merge();
			}
			else
			{
				Env.Security.OrgDuplicateDetectionMerge.ShowError();
			}
		}

		#endregion

		#region Country Specific

		#region SG Import UEN References

		public void updateUENReferences(object sender, EventArgs e)
		{
			new UpdateUENReferencesForm().Show();
		}

		#endregion

		#region US Send Importer Bond Query
		void SendImporterBondQuery()
		{
			var orgHeaders = (BusinessObjectCollection)Grid.DataSource;
			if (!orgHeaders.Any())
			{
				Globals.Message.ShowInformation(ImporterBondQuery_NoOrganizationSelected);
				return;
			}
			var messageCount = 0;
			var importerBondNumberRequester = ObjectFactory.Get<Enterprise.Integration.Customs.US.IImporterNumberRequester>();
			var registryItemErrMessage = GetRegistryItemErrMessageForSendImporterBondQuery(importerBondNumberRequester);
			if (!registryItemErrMessage.IsEmpty)
			{
				Globals.Message.ShowInformation(registryItemErrMessage);
				return;
			}
			else
			{
				var orgList_IsNotConsignee = new ZStringBuilder();
				var orgList_IsNotValidNumber = new ZStringBuilder();
				var orgList_NoPermission = new ZStringBuilder();
				foreach (OrgHeader orgHeader in orgHeaders)
				{
					if (orgHeader.OH_IsConsignee)
					{
						var cusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber });
						if (cusCode != null)
						{
							var importerBondNumber = cusCode.OK_CustomsRegNo;
							if (!importerBondNumberRequester.IsValidImporterBondNumber(importerBondNumber))
							{
								orgList_IsNotValidNumber.Append(orgHeader.OH_Code);
							}
							else if (!importerBondNumberRequester.HasPermissionToSendImporterBondNumber(importerBondNumber))
							{
								orgList_NoPermission.Append(orgHeader.OH_Code);
							}
							else
							{
								importerBondNumberRequester.RequestImporterBond(orgHeader, importerBondNumber);
								messageCount++;
							}
						}
					}
					else
					{
						orgList_IsNotConsignee.Append(orgHeader.OH_Code);
					}
				}

				var information = Res.GetString("923D90A2-1876-4DC7-A4A8-D7ED3D1734EE", "{0} message(s) sent.", messageCount);
				if (!orgList_IsNotConsignee.IsEmpty || !orgList_IsNotValidNumber.IsEmpty || !orgList_NoPermission.IsEmpty)
				{
					if (!orgList_IsNotConsignee.IsEmpty)
					{
						information += "\r\n" + Res.GetString("3EBD8D68-BA72-475D-8FBA-1646CEAE90B2", "There is no Importer Bond Query message sent for following organizations because they are not marked as Consignee.") + " \r\n  " + orgList_IsNotConsignee.ToStringWithDelimiterBetweenAppends(", ") + ".";
					}
					if (!orgList_IsNotValidNumber.IsEmpty)
					{
						information += "\r\n" + Res.GetString("A636D085-8620-4A98-B6C1-BE80ED6A9234", "There is no Importer Bond Query message sent for following organizations because they don't have a valid EIN or CBP Assigned Number or Social Security number.") + " \r\n  " + orgList_IsNotValidNumber.ToStringWithDelimiterBetweenAppends(", ") + ".";
					}
					if (!orgList_NoPermission.IsEmpty)
					{
						information += "\r\n" + Res.GetString("A1F6AFA2-B5B0-48D9-BBFA-A1F34027F8E5", "There is no Importer Bond Query message sent for following organizations because you do not have the security right to view personal information.") + " \r\n  " + orgList_NoPermission.ToStringWithDelimiterBetweenAppends(", ") + ".";
					}
				}

				Globals.Message.ShowInformation(information);
			}
		}

		internal virtual ZString GetRegistryItemErrMessageForSendImporterBondQuery(Enterprise.Integration.Customs.US.IImporterNumberRequester importerBondNumberRequester)
		{
			return importerBondNumberRequester.CurrentCompanyRegistryItemValidationForRequestImporterBond();
		}

		public static string ImporterBondQuery_NoOrganizationSelected => Res.GetString("E7F59C01-E88A-4EFD-A72E-216C20619A9D", "There are no organizations in the module grid system can send a query message for.");
		#endregion

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new OrganisationActionSupporter(); }
		}

		#endregion

		#region Implementation of IImportCollectionInfoProvider

		ImportCollectionInfoImpl orgCollectionInfo;
		protected OrgFlattenedCollection orgCollection;

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				if (orgCollectionInfo == null)
				{
					orgCollection = orgCollection ?? new OrgFlattenedCollection(Factory);
					orgCollectionInfo = new ImportCollectionInfoImplForOrgFlattened(orgCollection);
				}
				return orgCollectionInfo;
			}
		}

		public string ContextKey
		{
			get { return "{BE7FAA9E-7120-49CA-9910-DFAE61A50BD8}"; }
		}

		#endregion

		#region Implementation of ZFilterGridModule

		protected override void RunImportDataWizard()
		{
			orgCollection = new OrgFlattenedCollection(Factory);
			var orgsToLinkDict = new Dictionary<OrgFlattened, OrgHeader>();
			var impl = new ImportCollectionInfoImplForOrgFlattened(orgCollection);
			var orgFlattenedProcessor = new OrgFlattenedDataTransferProcessor(impl, orgsToLinkDict);
			var orgsToLinkProcessor = new OrgsToLinkDataTransferProcessor(orgCollection, orgsToLinkDict);
			var orgsSaveProcessor = new DataSaveProcessor(Factory,
				inProgressMessage: Res.GetString("7010a38f-c040-490f-b927-8acf7b274dd1", "Saving organizations. This may take a long time depending on the amount being imported."),
				completedMessage: Res.GetString("9db9e487-3ca8-4ec8-bcff-54953604a830", "Organizations saved."));

			RunImportWizardForm(impl, ContextKey, orgFlattenedProcessor, orgsToLinkProcessor, orgsSaveProcessor);
		}

		void RunImportWizardForm(ImportCollectionInfoImpl impl, string contextKey, OrgFlattenedDataTransferProcessor orgFlattenedProcessor, OrgsToLinkDataTransferProcessor orgsToLinkProcessor, DataSaveProcessor orgsSaveProcessor)
		{
			bool isCancelled = false;
			DataImportWizardForm form = new MultistepDataImportWizardForm(impl, contextKey, new DataTransferProcessor[] { orgFlattenedProcessor, orgsToLinkProcessor, orgsSaveProcessor }, this.ID.Description.ToString());
			form.Cancelled += (s, e) => { orgFlattenedProcessor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(orgFlattenedProcessor, orgsToLinkProcessor, isCancelled); };
			form.Show();
		}

		void DisplayResult(OrgFlattenedDataTransferProcessor orgFlattenedProcessor, OrgsToLinkDataTransferProcessor orgsToLinkProcessor, bool isCancelled)
		{
			string mesgs;
			string caption;

			if (isCancelled)
			{
				mesgs = Res.GetString("9606ad35-2470-42db-ac91-e5317a71b245", "No organization was created.");
				caption = Res.GetString("edb2bc96-906e-4706-977a-fe3ab04ea6c1", "Import canceled");
			}
			else
			{
				mesgs = Res.GetString("ff6883ba-3fc9-465f-ad29-3baed1471975", "Organizations to Import = {0}", orgFlattenedProcessor.HeadersToCreate) + "\r\n";
				mesgs += orgFlattenedProcessor.Log;
				mesgs += "\r\n" + Res.GetString("ac7600bf-1e8f-4194-a4c3-bb40ea04cb5e", "TOTAL: Organizations created = {0}, Organizations excluded = {1}", orgFlattenedProcessor.HeadersCreated, orgFlattenedProcessor.HeadersExcluded) + "\r\n";
				foreach (var childMerger in orgFlattenedProcessor.UniqueChildrenMergers)
				{
					if (childMerger.ChildRecordsCreated + childMerger.ChildRecordsExcluded > 0)
					{
						mesgs += Res.GetString("ea51afc4-ef23-42ec-8e77-68a54ce85b26", "TOTAL: {0} created = {1}, {0} excluded = {2}", childMerger.ChildHumanReadableNameForPlural, childMerger.ChildRecordsCreated, childMerger.ChildRecordsExcluded) + "\r\n";
					}
				}
				mesgs += "\r\n" + orgsToLinkProcessor.Log;
				mesgs += "\r\n" + Res.GetString("3ed0911f-77f5-4042-b209-202864d3606e", "TOTAL: Organizations updated with linked records = {0}", orgsToLinkProcessor.OrgsLinked);

				caption = Res.GetString("d4d459e2-eff8-4d86-ae6f-a81896c7c196", "Import completed");
			}

			using (ZMessageBox notification = new ZMessageBox(mesgs, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				notification.ShowDialog();
			}
		}

		#endregion
	}
}
