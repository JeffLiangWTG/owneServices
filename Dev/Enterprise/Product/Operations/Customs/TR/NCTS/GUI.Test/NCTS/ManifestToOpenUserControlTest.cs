using System.Windows.Forms;
using Enterprise.Customs.TR.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class ManifestsToOpenUserControlTest : BaseManifestsToOpenUserControlTestCase
	{
		protected override Control CreateControl() => new ManifestsToOpenUserControl();

		protected override string ControlName => "ManifestsToOpenUserControl";

		protected override ExpectedGridColumnInfo[] ExpectedGridColumnInfos => new ExpectedGridColumnInfo[]
		{
			new(NctsManifestsToOpen.Schema.IsPartial,"IsPartialCheckBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_ReferenceNumber,"BillNoTextBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_ReferenceNumber2,"DeclarationNoTextBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_LineNo,"BillLineNoCalcEdit", true ),
			new (NctsManifestsToOpen.Schema.CSI_Quantity3,"QuantityCalcEdit", true ),
			new (NctsManifestsToOpen.Schema.AtWarehouse,"AtWarehouseCheckBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_CustomsOffice,"WarehouseCodeDropEdit", true ),
		};
	}
}
