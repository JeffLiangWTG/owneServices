using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI
{
	public partial class DeduplicationOrganisationFilterControl : DeduplicationFilterControl
	{
		public DeduplicationOrganisationFilterControl()
			: base()
		{
			InitializeComponent();
		}

		public DeduplicationOrganisationFilterControl(DeduplicationOrganisationCollection collection, FilterStripBusinessObject bizo)
			: base(collection, bizo)
		{
			InitializeComponent();
			AddColumnStyles();
			PerformSearch += DuplicatedOrganisationFilterControl_PerformSearch;
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return ObjectFactory.Get("OrganisationFilterStrip") as ZFilterStrip;
		}

		protected override void RecalculateConfidenceScore_Click(object sender, EventArgs e)
		{
			var selectedDedupOrgs = Grid.GetSelectedElements<DeduplicationOrganisation>();

			foreach (var selectedDedupOrg in selectedDedupOrgs)
			{
				DeduplicationHelper.DeleteDeduplicationResults(selectedDedupOrg);
				selectedDedupOrg.PropagateForcedReloadRequired();
			}
		}

		void DuplicatedOrganisationFilterControl_PerformSearch(object sender, EventArgs e)
		{
			DeduplicationFilterControlUtils.PerformSearch(Grid, Collection, FilterBusinessObject.Filter, MaxNumberOfRecordsToShowInDisplayGrids, MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid, UpdateNumberLoadedMessage);
		}

		BusinessObjectCollection Collection => (BusinessObjectCollection)GridCollection;

		int MaxNumberOfRecordsToShowInDisplayGrids => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

		int MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid => SystemDataRegistry.Instance.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.Value;

		void AddColumnStyles()
		{
			this.CreateColumnStyle(Res.GetData("cfe4b6d5-ddb1-4deb-bb75-3e5f84203d32", "Code"), MDMAdminPanelOrganisationViewSchema.DOH_Code.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("a748086e-0c2a-4dab-85a1-6bd545911312", "Organization Name"), MDMAdminPanelOrganisationViewSchema.DOH_FullName.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("a4bd3ff8-65a1-4f9f-9b85-fb0759e140dc", "Total", "Total Results", "Total number of potential duplicates found"), MDMAdminPanelOrganisationViewSchema.DOH_TotalDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("dfebe7f8-9951-4af4-97dd-22160444d201", "High", "High Results", "High Confidence Results", "Number of potential duplicates with high confidence"), MDMAdminPanelOrganisationViewSchema.DOH_HighDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), true);
			this.CreateColumnStyle(Res.GetData("9343974b-f107-42a1-a70a-127ac2bfb9f5", "Medium", "Medium Results", "Medium Confidence Results", "Number of potential duplicates with medium confidence"), MDMAdminPanelOrganisationViewSchema.DOH_MediumDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), true);
			this.CreateColumnStyle(Res.GetData("1f6465ea-1908-4692-bb28-30253c82035c", "Low", "Low Results", "Low Confidence Results", "Number of potential duplicates with low confidence"), MDMAdminPanelOrganisationViewSchema.DOH_LowDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), true);
			this.CreateColumnStyle(Res.GetData("21bbb43a-d2ab-4b02-bb3c-a6e8e73915b4", "Status"), MDMAdminPanelOrganisationViewSchema.DOH_Status.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("706ef244-4d1a-4d01-9cb2-1a51fe62a5ba", "Excluded by"), MDMAdminPanelOrganisationViewSchema.DOH_ExcludedBy.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
			this.CreateColumnStyle(Res.GetData("56f52257-9aac-4a2e-bb47-49ad0bd35d16", "Branch", "By Branch", "Created Under Branch", "Branch that the organization was created under"), DeduplicationOrganisation.Schema.DOH_CreatedUnderBranch, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), visible: false);
			this.CreateColumnStyle(Res.GetData("b9c5e7bf-48f5-4903-afc6-41f2ef88d5ac", "Company", "By Company", "Created Under Company", "Company that the organization was created under"), DeduplicationOrganisation.Schema.DOH_CreatedUnderCompany, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), visible: false);
			this.CreateColumnStyle(Res.GetData("0D7986FD-381B-4C69-B9FF-59335171FB41", "Consolidations", "Associated Consolidations", "Associated Consolidations Count", "Organization Associated Consolidations Count"), DeduplicationOrganisation.Schema.DOH_ConsolidationsCount, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), visible: false);
			this.CreateColumnStyle(Res.GetData("B1003AC2-D19F-44F4-B92D-B9B66258A349", "Declarations", "Associated Declarations", "Associated Declarations Count", "Organization Associated Declarations Count"), DeduplicationOrganisation.Schema.DOH_DeclarationsCount, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), visible: false);
			this.CreateColumnStyle(Res.GetData("80FB66EF-82AA-409B-BDF6-9F15EC1325B0", "Shipments", "Associated Shipments", "Associated Shipments Count", "Organization Associated Shipments Count"), DeduplicationOrganisation.Schema.DOH_ShipmentsCount, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), visible: false);
			if (ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI)
			{
				this.CreateColumnStyle(Res.GetData("54B2A901-8456-4B3C-8C13-FC2A3C58F6F7", "Enterprise ID"), DeduplicationOrganisation.Schema.DOH_EnterpriseId, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
				this.CreateColumnStyle(Res.GetData("C404554C-2C5F-474D-B663-F81B21F87A54", "Enterprise Code"), DeduplicationOrganisation.Schema.DOH_EnterpriseCode, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
				this.CreateColumnStyle(Res.GetData("D2363ACE-BD83-42BD-AF88-4D7E4A2F1DD9", "Company Code"), DeduplicationOrganisation.Schema.DOH_CompanyCode, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
				this.CreateColumnStyle(Res.GetData("D7DDECD9-4AC0-4E59-B663-ABEA1260C61B", "Product"), DeduplicationOrganisation.Schema.DOH_ProductId, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			}
		}
	}
}
