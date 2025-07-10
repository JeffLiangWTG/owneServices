using System.Windows.Forms;
using Enterprise.Customs.TR.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class Phase5ManifestsToOpenUserControlTest : BaseManifestsToOpenUserControlTestCase
	{
		protected override Control CreateControl() => new Phase5ManifestsToOpenUserControl();

		protected override string ControlName => "Phase5ManifestsToOpenUserControl";

		protected override ExpectedGridColumnInfo[] ExpectedGridColumnInfos => new ExpectedGridColumnInfo[]
		{
			new(NctsManifestsToOpen.Schema.IsPartial,"IsPartialCheckBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_ReferenceNumber,"BillNoTextBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_ReferenceNumber2,"DeclarationNoTextBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_LineNo,"BillLineNoCalcEdit", true ),
			new (NctsManifestsToOpen.Schema.CSI_Quantity3,"QuantityCalcEdit", true ),
			new (NctsManifestsToOpen.Schema.AtWarehouse,"AtWarehouseCheckBox", true ),
			new (NctsManifestsToOpen.Schema.CSI_CustomsOffice,"WarehouseCodeDropEdit", true ),
			new (NctsManifestsToOpen.Schema.IsOtherProcedure,"IsOtherProcedureCheckBox", false ),
		};
	}
}
