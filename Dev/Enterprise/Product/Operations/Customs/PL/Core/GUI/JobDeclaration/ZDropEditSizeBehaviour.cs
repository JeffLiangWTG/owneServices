using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class ZDropEditSizeBehaviour : ControlBehaviour<ZDropEdit, BusinessObject>
{
	public ZDropEditSizeBehaviour(int preBoundMaxLength = 5)
	{
		this.preBoundMaxLength = preBoundMaxLength;
	}

	readonly int preBoundMaxLength;

	protected override void UpdateBehaviourCore(ZDropEdit control, BusinessObject dataItem)
	{
		if (control == null || dataItem == null)
		{
			return;
		}

		control.ShouldResizeByMaxLength = false;
		control.PreBoundMaxLength = preBoundMaxLength;
	}
}
