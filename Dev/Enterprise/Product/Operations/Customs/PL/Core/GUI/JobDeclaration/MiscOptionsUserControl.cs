using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public partial class MiscOptionsUserControl : EU.GUI.MiscOptionsUserControl
{
	public MiscOptionsUserControl()
		: base()
	{
	}

	protected override ColumnWidth[] GetAdditionalInfosColumnWidths()
	{
		return new[]
		{
			new ColumnWidth(AdditionalInfo.Schema.CSI_Code, 50),
			new ColumnWidth(AdditionalInfo.Schema.CSI_Description, 100)
		};
	}
}
