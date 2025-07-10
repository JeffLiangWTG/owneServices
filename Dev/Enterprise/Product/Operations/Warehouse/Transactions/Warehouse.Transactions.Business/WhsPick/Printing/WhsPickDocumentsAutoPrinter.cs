using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Printing
{
	public class WhsPickDocumentsAutoPrinter : WhsDocumentPrinter
	{
		public WhsPickDocumentsAutoPrinter(WhsPick parent, INotifications notify, bool ignoreAutoPrint = false)
			: base(parent, notify)
		{
			this.ignoreAutoPrint = ignoreAutoPrint;
		}

		readonly bool ignoreAutoPrint;

		#region Overrides

		public override ZString DocumentMenuName
		{
			get { return new ZString((NoResString)"Pick Documents Pack"); } // Hard-coded menu name
		}

		protected override ZString DocumentMenuBusinessContext
		{
			get { return nameof(BusinessContext.WhsDespatch); }
		}

		protected override ZBool UseLegacyVersion
		{
			get { return !DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value; }
		}

		protected override ZBool ShouldPrint
		{
			get
			{
				var pick = (WhsPick)Parent;
				var warehouse = pick.Warehouse;

				return ignoreAutoPrint ||
					(warehouse != null &&
					(warehouse.WW_AutoPrintPickingSlip ||
					warehouse.WW_AutoPrintOrderSummaryOnPick ||
					warehouse.WW_AutoPrintOrderCopyForMOPOnPick ||
					warehouse.WW_AutoPrintPickingNonPickedItems ||
					warehouse.WW_AutoPrintPickingShortfallItems));
			}
		}

		#endregion
	}
}
