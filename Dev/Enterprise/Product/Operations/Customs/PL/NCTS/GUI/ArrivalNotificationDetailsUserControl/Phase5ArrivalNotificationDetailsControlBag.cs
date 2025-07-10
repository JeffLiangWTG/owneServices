using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.NCTS.GUI;

public sealed class Phase5ArrivalNotificationDetailsControlBag : ControlBag
{
	Phase5ArrivalNotificationDetailsControlBag()
	{
		RepresentativeTraderGuidFindBox = RegisterControl(nameof(Phase5ArrivalNotificationDetailsUserControl.RepresentativeTraderGuidFindBox));
	}

	public static Phase5ArrivalNotificationDetailsControlBag Instance => instance ??= new Phase5ArrivalNotificationDetailsControlBag();

	[ThreadStatic]
	static Phase5ArrivalNotificationDetailsControlBag instance;

	protected override Control CreateTemplate() => new Phase5ArrivalNotificationDetailsUserControl();

	public ControlReference RepresentativeTraderGuidFindBox {  get; }
}
