using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.NCTS.GUI;

sealed class ArrivalNotificationDetailsControlBag : ControlBag
{
	ArrivalNotificationDetailsControlBag()
	{
		GoodsRegistrationNumberTextBox = RegisterControl(nameof(ArrivalNotificationDetailsUserControl.GoodsRegistrationNumberTextBox));
	}

	public static ArrivalNotificationDetailsControlBag Instance => instance ??= new ();

	[ThreadStatic]
	static ArrivalNotificationDetailsControlBag instance;

	public ControlReference GoodsRegistrationNumberTextBox { get; }

	protected override Control CreateTemplate() => new ArrivalNotificationDetailsUserControl();
}
