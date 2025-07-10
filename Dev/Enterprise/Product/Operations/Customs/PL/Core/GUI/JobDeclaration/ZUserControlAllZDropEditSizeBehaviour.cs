using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ZUserControlAllZDropEditSizeBehaviour : ControlBehaviour<ZUserControl, BusinessObject>
{
	public ZUserControlAllZDropEditSizeBehaviour(int preBoundMaxLength = 5)
	{
		this.preBoundMaxLength = preBoundMaxLength;
	}

	readonly int preBoundMaxLength;

	protected override void UpdateBehaviourCore(ZUserControl control, BusinessObject dataItem)
	{
		if (control == null || dataItem == null)
		{
			return;
		}

		var totalSizeDiff = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
		foreach (Control tempControl in control.Controls)
		{
			tempControl.Location += totalSizeDiff;
			if (tempControl is ZDropEdit dropEdit)
			{
				var previousDropEditSize = dropEdit.Size;
				dropEdit.ShouldResizeByMaxLength = false;
				dropEdit.PreBoundMaxLength = preBoundMaxLength;
				totalSizeDiff += dropEdit.Size - previousDropEditSize;
			}
		}
		control.Size += totalSizeDiff;
	}
}
