using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NO.Module;

public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
{
	[Obsolete("Do not call. Only for designer use.")]
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
				CaptionResourceString = Res.GetData("B7A4BB88-738A-4CBA-A225-6DB69DFAAED4", "Phase Status"),
				ColumnName = JobDeclaration.Schema.PhaseStatus,
				IsReadOnly = true,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6B93CF70-4421-4B79-8F17-C453D5EC3F06", "Phase Status Description"),
				ColumnName = JobDeclaration.Schema.PhaseStatusDescription,
				IsReadOnly = true,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			},
		});
	}
}
