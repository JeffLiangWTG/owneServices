using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class EntryInstructionGridUserControl : EU.GUI.EntryInstructionGridUserControl
{
	public EntryInstructionGridUserControl()
	{
		InitializeComponent();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		var columnStyles = EntryInstructionsGrid.ColumnStyles;
		columnStyles.Clear();

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = "CEI_SubStyle",
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68),
		});

		columnStyles.Add(new ZDateEditColumnStyleInfo()
		{
			ColumnName = "CEI_DateForDuty",
			CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("0F2CF622-60F8-48A9-90BF-5029F8442CE1", "Decl. Date"),
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short,
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = "CEI_Procedure",
			IsCustomColumn = false,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = "CEI_Description",
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290),
		});
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)CurrentDataItem;
}
