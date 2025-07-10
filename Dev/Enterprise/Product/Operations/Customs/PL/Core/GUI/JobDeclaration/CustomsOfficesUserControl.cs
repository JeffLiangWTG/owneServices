using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class PLCustomsOfficesUserControl : EU.GUI.CustomsOfficesUserControl
{
	public PLCustomsOfficesUserControl() : base()
	{
		InitializeComponent();
		CustomsOfficesGrid.RemoveFromAvailableColumns(EU.Business.EuOfficeCode.Schema.CY_Date);
	}

	public new JobDeclaration JobDeclaration => (JobDeclaration)CurrentDataItem;

	JobDeclarationCustomsOfficeRequirementHelper Helper => JobDeclaration?.CustomsOfficeRequirementHelper;

	public override void HandleDeclarationControlVisibilityChanged()
	{
		base.HandleDeclarationControlVisibilityChanged();

		AdditionalRequiredOfficeFindBox.Visible = Helper?.AdditionalOffice != null;
		AdditionalRequiredOfficeFindBox.Extensions.Get<ILabelCaptionRenderer>().Caption = Helper?.AdditionalOffice?.FriendlyName ?? Res.GetString("a9bbf070-55bd-4ee4-8b6a-a48913e226f9", "Additional Customs Office");

		PresentationStartDateEdit.Visible = Helper?.IsPresentationStartDateEditVisible ?? true;
	}
}
