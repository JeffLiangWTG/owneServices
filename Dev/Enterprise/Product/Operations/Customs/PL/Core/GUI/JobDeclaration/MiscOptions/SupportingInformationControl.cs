using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class SupportingInformationControl : EU.GUI.SupportingInformationControl
{
	public SupportingInformationControl()
	{
		InitializeComponent();
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(PlugIn.LayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PlugIn.LayoutPreviousDocumentsUserControl);

	protected override ColumnWidth[] GetAdditionalInfosColumnWidths() => new[]
	{
		new ColumnWidth(AdditionalInfo.Schema.CSI_Code, 50),
		new ColumnWidth(AdditionalInfo.Schema.CSI_Description, 100)
	};

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		AdditionalInfoTabPage.CaptionResourceString = IsExport
			? Enterprise.Customs.PL.GUI.Res.GetData("FDD09F99-7E2A-4A74-A336-D844A2C8161A", "[44] Additional Documents")
			: Enterprise.Customs.PL.GUI.Res.GetData("B9716134-E54F-4949-828E-5C019294DC68", "[44] Additional Info");
	}

	bool IsExport => CurrentDataItem is JobDeclaration jobDeclaration && jobDeclaration.IsExport;
}
