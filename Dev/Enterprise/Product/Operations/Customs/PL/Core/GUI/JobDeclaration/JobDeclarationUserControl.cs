using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class JobDeclarationUserControl : EUJobDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
		CustomsOfficesUserControl.Size = ControlDpiScalingHelper.NewScaledSize(469, 166, true);
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override Type GetCustomsOfficesUserControlType() => typeof(PLCustomsOfficesUserControl);

	protected override bool GetTransportIDVisible() => false;
}
