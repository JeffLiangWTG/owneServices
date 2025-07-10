using CargoWise.EntityFramework;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public sealed class CompactDisplayModeBehaviour : ControlBehaviour<ZDocAddressControl, BusinessObject>
{
	public CompactDisplayModeBehaviour()
	{
	}

	protected override void UpdateBehaviourCore(ZDocAddressControl control, BusinessObject dataItem)
	{
		if (control == null || dataItem == null)
		{
			return;
		}

		control.DisplayMode = ZDocAddressControlDisplayMode.Compact;
	}
}
