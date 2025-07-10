using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public class GoodsLocationColumnStyle : ZCodeFindBoxColumnStyle
{
	public GoodsLocationColumnStyle(GoodsLocationColumnStyleInfo columnInfo)
		: base(() => new GoodsLocationGridFindBox(), columnInfo)
	{
	}

	protected override void PrepareControlData(CurrencyManager source, int rowNum)
	{
		var businessObject = source.Position == -1 ? null : source.GetCurrent();
		if (businessObject != null)
		{
			var findBox = (GoodsLocationGridFindBox)EditControl;
			findBox.GoodsLocationProvider = (ICusGoodsLocationProvider)businessObject;
		}
	}

	protected override bool EditControlShownForReadOnlyCore => true;

	internal void CommitEditingRow()
	{
		if (parentZGrid.ListManager is CurrencyManager currencyManager)
		{
			currencyManager.EndCurrentEdit();
			parentZGrid.BeginEdit(this, LastFocusedCell.RowNumber);
			currencyManager.Refresh();
		}
	}
}
