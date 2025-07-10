using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Printing
{
	public class WhsPackingSlipDocumentPrinter : WhsDocumentPrinter
	{
		public WhsPackingSlipDocumentPrinter(WhsPickableDocket parent, INotifications notify)
			: base(parent, notify)
		{
		}

		#region Overrides

		public override ZString DocumentMenuName
		{
			get { return (NoResString)"Packing Slip"; } // Hard-coded menu name
		}

		protected override ZString DocumentMenuBusinessContext
		{
			get { return nameof(BusinessContext.WhsOrder); }
		}

		protected override ZBool UseLegacyVersion
		{
			get { return !DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value; }
		}

		#endregion
	}
}
