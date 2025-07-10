using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

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
			ColumnName = AutoCusEntryInstruction.Schema.CEI_Style,
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = AutoCusEntryInstruction.Schema.CEI_SubStyle,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68),
		});

		columnStyles.Add(new ZTextBoxColumnStyleInfo()
		{
			ColumnName = AutoCusEntryInstruction.Schema.CEI_Description,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = AutoCusEntryInstruction.Schema.CEI_Procedure,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
		});

		columnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = Business.Declaration.CusEntryInstruction.Schema.ZG_TransNature,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
			IsCustomColumn = false,
			CaptionResourceString = Res.GetData("59854658-6E2E-498B-9C6C-191FD120A683", "Tran. Nature", "[UCC 8/5] Transaction Nature"),
		});
	}

	protected override bool ShowRequestedProcedure => true;
}
