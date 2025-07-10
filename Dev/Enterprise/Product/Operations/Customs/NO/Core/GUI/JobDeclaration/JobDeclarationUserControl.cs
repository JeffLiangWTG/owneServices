using System;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI;

public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		UpdateCustomsOfficesUserControlTypeIfRequired();
	}

	protected override void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		base.JE_MessageTypeInfo_ValueChanged(sender, e);
		UpdateCustomsOfficesUserControlTypeIfRequired();
	}

	void UpdateCustomsOfficesUserControlTypeIfRequired()
	{
		CustomsOfficesUserControl.UserControlType = (string)JobDeclaration.JE_MessageType switch
		{
			SharedJobMessageTypeList.Codes.Import => typeof(ImportCustomsOfficesUserControl),
			SharedJobMessageTypeList.Codes.Export => typeof(ExportCustomsOfficesUserControl),
			_ => null,
		};
	}

	protected override void SetRightTabControlSelectTab()
	{
		RightTabControl.SelectedTab = OrganisationsTabPage;
	}

	protected override void AdjustHeightAndMakeRoomForDeclarationDetailsGroupBox() => AdjustHeightAndMakeRoomForControl(DeclarationDetailsGroupBox, 71, RightTabControl, TransportDetailsGroupBox, ShipmentDetailsGroupBox, CustomsOfficesUserControl);

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
}
