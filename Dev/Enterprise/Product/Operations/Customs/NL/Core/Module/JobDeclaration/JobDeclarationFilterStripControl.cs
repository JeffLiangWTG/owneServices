using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl()
		{
			InitializeComponent();
		}

		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclaration.Schema.PhaseStatus,
					IsReadOnly = true,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclaration.Schema.PhaseStatusDescription,
					IsReadOnly = true,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
			});
		}
	}
}
