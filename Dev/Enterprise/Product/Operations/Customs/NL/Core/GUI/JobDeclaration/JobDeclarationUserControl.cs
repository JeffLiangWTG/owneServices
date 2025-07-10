using System;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI;

public partial class JobDeclarationUserControl : EUJobDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
	}

	public new JobDeclaration JobDeclaration
	{
		get
		{
			return (JobDeclaration)base.JobDeclaration;
		}
		set
		{
			base.JobDeclaration = value;
		}
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		ControllingCustomerGuidFindBox.CaptionResourceString = null;
		PresentationGroupBox.Visible = JobDeclaration.IsExport;
		SpecificCircumstanceDropEdit.Visible = JobDeclaration.AreTransportChargedAndSpecificCircumstanceVisibleForDeclarationType();

		this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 253, true);
		this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 310, true);
		this.ShipmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 344, true);
		this.CustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 658);
		this.PresentationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 806);
	}

	protected override Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);

	protected override Type GetOrganizationImportUserControlType() => typeof(ImportOrganizationUserControl);

	protected override void SetRightTabControlSelectTab()
	{
		if (RightTabControl != null)
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}
	}
}

