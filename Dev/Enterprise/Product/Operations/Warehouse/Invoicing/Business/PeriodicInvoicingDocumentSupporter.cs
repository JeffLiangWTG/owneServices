using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public abstract class PeriodicInvoicingDocumentSupporter(PeriodicInvoicing invoice) : DocumentSupporter(invoice)
	{
		protected PeriodicInvoicing Invoice
		{
			get { return (PeriodicInvoicing)BusinessObject; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return [Core.Constants.DataContext.GenericFreightJob];
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Invoice);
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}
}
