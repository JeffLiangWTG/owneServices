using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module.Organisation.OrgImport;
using Enterprise.Security;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class TestOrganisationModule : OrganisationModule
	{
		OrgFlattenedDataTransferProcessor orgFlattenedProcessor;
		OrgsToLinkDataTransferProcessor orgsToLinkProcessor;
		DataSaveProcessor orgsSaveProcessor;

		public new OrganisationFilterBusinessObject FilterBusinessObject
		{
			get { return (OrganisationFilterBusinessObject)base.FilterBusinessObject; }
		}

		public ZDisplayGrid Grid_Exposed
		{
			get
			{
				return Grid;
			}
		}

		public void PerformSearch()
		{
			base.PerformSearch();
		}

		public void MergeOrganisation_Exposed()
		{
			base.MergeOrganisation();
		}

		public void MergeIntoOrganisation_Exposed()
		{
			base.MergeIntoOrganisation();
		}

		public void FindAndMergeSimilarOrgs_Exposed()
		{
			base.FindAndMergeSimilarOrgs();
		}

		public void OnActivate_Exposed()
		{
			throw new Exception("this shows GridSelectedElements gets hit");
		}

		public void OnDeActivate_Exposed()
		{
			throw new Exception("this shows GridSelectedElements gets hit");
		}

		protected override void MergeOrganisationCore()
		{
			// Merge Org Form is shown dialog so cannot be opened in testing
		}

		public new IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			return base.ShowEditForm(selectedBusinessObject);
		}

		public void DoExportInvoiseAsJournalToXml()
		{
			const string DataTransferMenuTitle = "D&ata Transfer";
			const string ExportInvoicesAsJornalsMenuTitle = "Export Outstanding Invoices as Journals to XML";

			List<MenuItem> result = new List<MenuItem>(GetNewActionMenuItems());
			MenuItem dataTransferMenuItem = result.FindByText(DataTransferMenuTitle)
				?? throw new ArgumentNullException("Can not find menu item: " + DataTransferMenuTitle);

			MenuItem exportInvoicesAsJornalsMenuItem = dataTransferMenuItem.MenuItems.FindByText(ExportInvoicesAsJornalsMenuTitle)
				?? throw new ArgumentNullException("Can not find menu item: " + ExportInvoicesAsJornalsMenuTitle);

			exportInvoicesAsJornalsMenuItem.PerformClick();
		}

		public bool CheckCopySelectedRowsAllowed_Exposed()
		{
			return CheckCopySelectedRowsAllowed();
		}

		public SecurityCheckpoint ExportSecurityCheckpoint_Exposed
		{
			get
			{
				return ExportSecurityCheckpoint;
			}
		}

		public OrgFlattenedDataTransferProcessor OrgFlattenedProcessor
		{
			get { return orgFlattenedProcessor; }
		}

		public OrgsToLinkDataTransferProcessor OrgsToLinkProcessor
		{
			get { return orgsToLinkProcessor; }
		}

		public void ProcessImport(OrgFlattenedCollection orgCollection)
		{
			this.orgCollection = orgCollection;
			ProcessImport();
		}

		public void ProcessImport()
		{
			var orgsToLinkDict = new Dictionary<OrgFlattened, OrgHeader>();
			orgFlattenedProcessor = new OrgFlattenedDataTransferProcessor(new ImportCollectionInfoImplForOrgFlattened(orgCollection), orgsToLinkDict);
			orgFlattenedProcessor.Import();
			orgsToLinkProcessor = new OrgsToLinkDataTransferProcessor(orgCollection, orgsToLinkDict);
			orgsToLinkProcessor.Import();
			orgsSaveProcessor = new DataSaveProcessor(orgCollection.Factory, "Saving organizations. This may take a long time depending on the amount being imported.", "Organizations saved.");
			orgsSaveProcessor.Import();
		}

		public ExportPatternMatchOverridesUtil ExportPatternMatchOverridesUtil_Exposed { get { return ExportPatternMatchOverridesUtil; } }

		internal override ZString GetRegistryItemErrMessageForSendImporterBondQuery(Enterprise.Integration.Customs.US.IImporterNumberRequester importerBondNumberRequester)
		{
			return ZString.Empty;
		}
	}
}
