using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.GUI
{
	[CodeAlive("Used in admin panel person deduplication module.")]
	public partial class DeduplicationPersonFilterControl : ZFilterStripControl
	{
		public DeduplicationPersonFilterControl()
		{
			InitializeComponent();
		}

		public DeduplicationPersonFilterControl(DeduplicationPersonCollection collection, FilterStripBusinessObject bizo)
			: base(collection, bizo)
		{
			InitializeComponent();
			AddColumnStyles();
			PerformSearch += DuplicatedPersonFilterControl_PerformSearch;
		}

		void DuplicatedPersonFilterControl_PerformSearch(object sender, EventArgs e)
		{
			DeduplicationFilterControlUtils.PerformSearch(grid, Collection, FilterBusinessObject.Filter, MaxNumberOfRecordsToShowInDisplayGrids, MaximumNumberForMDMDuplicatesPersonGrid, UpdateNumberLoadedMessage);
		}

		void AddColumnStyles()
		{
			this.CreateColumnStyle(Res.GetData("7553fd30-e08b-4d56-acfc-6b35c63f218e", "Full Name"), MDMAdminPanelPersonViewSchema.DPE_FullName.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("a4bd3ff8-65a1-4f9f-9b85-fb0759e140dc", "Total", "Total Results", "Total number of potential duplicates found"), MDMAdminPanelPersonViewSchema.DPE_TotalDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("dfebe7f8-9951-4af4-97dd-22160444d201", "High", "High Results", "High Confidence Results", "Number of potential duplicates with high confidence"), MDMAdminPanelPersonViewSchema.DPE_HighDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), true);
			this.CreateColumnStyle(Res.GetData("9343974b-f107-42a1-a70a-127ac2bfb9f5", "Medium", "Medium Results", "Medium Confidence Results", "Number of potential duplicates with medium confidence"), MDMAdminPanelPersonViewSchema.DPE_MediumDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), true);
			this.CreateColumnStyle(Res.GetData("1f6465ea-1908-4692-bb28-30253c82035c", "Low", "Low Results", "Low Confidence Results", "Number of potential duplicates with low confidence"), MDMAdminPanelPersonViewSchema.DPE_LowDuplicates.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), true);
			this.CreateColumnStyle(Res.GetData("21bbb43a-d2ab-4b02-bb3c-a6e8e73915b4", "Status"), MDMAdminPanelPersonViewSchema.DPE_Status.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), true);
			this.CreateColumnStyle(Res.GetData("706ef244-4d1a-4d01-9cb2-1a51fe62a5ba", "Excluded by"), MDMAdminPanelPersonViewSchema.DPE_ExcludedBy.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
			this.CreateColumnStyle(Res.GetData("cc7376dd-b78f-49ca-9699-f3c9d6c5d86d", "Primary Email"), MDMAdminPanelPersonViewSchema.DPE_EmailAddress.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), false, false);
			this.CreateColumnStyle(Res.GetData("436a7f42-ba1e-4e4a-b7ad-2f69f836fded", "Primary Workplace"), DeduplicationPerson.Schema.Workplace, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), false, false);
			this.CreateColumnStyle(Res.GetData("6029c13a-9d61-4e19-8650-99e7c2399221", "Primary Workplace Code"), DeduplicationPerson.Schema.WorkplaceCode, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), false, false);
			this.CreateColumnStyle(Res.GetData("54d6da64-0673-4c0d-b26e-97f7a251c26e", "Working Location"), DeduplicationPerson.Schema.Location, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), false, false);
			this.CreateColumnStyle(Res.GetData("7639c49c-e04f-4cf3-83f0-0206e184875f", "Job Title"), DeduplicationPerson.Schema.JobTitle, ControlDpiScalingHelper.ScaleToCurrentDpiX(190), false, false);
			this.CreateColumnStyle(Res.GetData("29fb13db-d09e-4678-a4b5-aa627803126b", "Gender"), MDMAdminPanelPersonViewSchema.DPE_Gender.Name, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), false, false);
		}

		BusinessObjectCollection Collection => (BusinessObjectCollection)GridCollection;

		int MaxNumberOfRecordsToShowInDisplayGrids => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

		int MaximumNumberForMDMDuplicatesPersonGrid => SystemDataRegistry.Instance.MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid.Value; // TODO: Add a person own registry
	}
}
