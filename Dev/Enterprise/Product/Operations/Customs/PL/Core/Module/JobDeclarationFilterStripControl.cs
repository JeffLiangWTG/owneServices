using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.Module;

public partial class JobDeclarationFilterStripControl : EU.Module.JobDeclarationFilterStripControl
{
	public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}

	public static class ResourceStrings
	{
		public static ResourceStringData CustomsSubStyle => Res.GetData("A6F9062A-2CE8-40A4-89EB-1AA0A49A018B", "Entry Sub-style");
	}

	protected override void InitializeAdditionalGridColumns()
	{
		base.InitializeAdditionalGridColumns();
		AddColumns();
	}

	void AddColumns()
	{
		grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			Caption = null,
			CaptionResourceString = ResourceStrings.CustomsSubStyle,
			ColumnName = JobDeclaration.Schema.EntryInstructionSubStyle,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		});
	}
}
