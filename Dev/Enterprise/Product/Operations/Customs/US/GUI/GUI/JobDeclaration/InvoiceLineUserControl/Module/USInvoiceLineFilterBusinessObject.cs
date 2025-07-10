using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using IInvoicesProvider = Enterprise.Customs.Business.IInvoicesProvider;

namespace Enterprise.Customs.US.GUI
{
	public class USInvoiceLineFilterBusinessObject : InvoiceLineFilterBusinessObject
	{
		public USInvoiceLineFilterBusinessObject(Func<IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable) : base(getInvoicesProvider, isColumnAvailable)
		{
			this.getInvoicesProvider = getInvoicesProvider;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new USInvoiceLineFilterBusinessObject(getInvoicesProvider, IsColumnAvailable);

		readonly Func<IInvoicesProvider> getInvoicesProvider;

		protected override void AddOrUpdateFunc(ModuleFilterCollection filters)
		{
			base.AddOrUpdateFunc(filters);
			UpdateTextFunc(InvoiceLineFilterConstants.Origin, x => ((JobComInvoiceLine)x).US_UC_NKCountryOfOrigin);
		}
	}
}
