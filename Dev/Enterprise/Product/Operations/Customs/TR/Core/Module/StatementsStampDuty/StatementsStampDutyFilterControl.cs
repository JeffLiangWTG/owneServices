using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Module
{
	public partial class StatementsStampDutyFilterControl : ZFilterStripControl
	{
		public StatementsStampDutyFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			AddCustomColumns();
		}

		void AddCustomColumns()
		{
			grid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_StatementType), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZDateEditColumnStyleInfo(nameof(CusStatementHeader.B2_DueDate), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_PaymentParty), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_StatementNumber), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_PaymentType), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZDateEditColumnStyleInfo(nameof(CusStatementHeader.B2_PeriodEndDate), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)),
				new ZDateEditColumnStyleInfo(nameof(CusStatementHeader.B2_PeriodStartDate), CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)),
				});
		}
	}
}
