using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new InvoiceLineCompleteCollectionFetchStrategy(this);
		}

		protected override bool ShouldAutoAllocatePackageToInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			return false;
		}

		protected override void SetDefaultForCommonInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			base.SetDefaultForCommonInvoiceLine(invoiceLine);
			var declarationType = ((JobComInvoiceLine)invoiceLine).EntryInstruction?.CEI_Style ?? ZString.Empty;
			if (declarationType == Constants.DeclarationTypes.Import.L1)
			{
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._99;
			}
			if (!invoiceLine.HasValidOrder)
			{
				invoiceLine.JI_OrderNumber = invoiceLine.InvoiceHeader?.JZ_OrderNumber ?? ZString.Empty;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (!IsLoading && bizOAdded is JobComInvoiceLine invoiceLine && invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice)
			{
				invoiceLine.EntryInstruction?.ControllingMessageHeaders.Cast<CusTWControllingMessageHeader>().Where(x => x.IsControllingMessageHeaderLinkInvoiceLinesLoaded).ForEach(x => x.ControllingMessageHeaderLinkInvoiceLines.RebuildElements());
				invoice.RebuildInvoiceQuantityAndUnitQtyResultIfNeeded();
			}
		}

		class InvoiceLineCompleteCollectionFetchStrategy : Customs.Business.FetchStrategies.InvoiceLineCompleteCollectionFetchStrategy
		{
			public InvoiceLineCompleteCollectionFetchStrategy(InvoiceLineCompleteCollection collection)
				: base(collection)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				var factory = Collection.Factory;
				foreach (JobComInvoiceLine invoiceLine in Collection)
				{
					factory.AddFetchHint(JobComInvLineRefsSchema.JG_JI, invoiceLine.PK);
				}
			}
		}
	}
}
