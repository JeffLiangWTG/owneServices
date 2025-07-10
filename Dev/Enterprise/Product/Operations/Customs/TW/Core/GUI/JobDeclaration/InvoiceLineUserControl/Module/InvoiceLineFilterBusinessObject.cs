using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.GUI
{
	public class InvoiceLineFilterBusinessObject : Customs.GUI.InvoiceLineFilterBusinessObject
	{
		public InvoiceLineFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
			: base(getInvoicesProvider, isColumnAvailable)
		{
			this.getInvoicesProvider = getInvoicesProvider;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new InvoiceLineFilterBusinessObject(getInvoicesProvider, IsColumnAvailable);

		readonly Func<Customs.Business.IInvoicesProvider> getInvoicesProvider;

		protected new IInvoicesProvider InvoicesProvider => (IInvoicesProvider)base.InvoicesProvider;

		protected override void AddOrUpdateFunc(ModuleFilterCollection filters)
		{
			base.AddOrUpdateFunc(filters);

			if (InvoicesProvider.IsImport)
			{
				AddTranslatableTextFunc(
					filters,
					InvoiceLineFilterConstants.DutyTreatment,
					x => x.JI_Procedure,
					ResString.GetMultilingualString("878604D1-547B-4BCC-B332-63226B5BF4A0", InvoiceLineFilterConstants.DutyTreatment)
				);
			}
			else
			{
				AddTranslatableTextFunc(
					filters,
					InvoiceLineFilterConstants.ModeOfStatistics,
					x => x.JI_Procedure,
					ResString.GetMultilingualString("D981472F-937A-4CBB-9D91-2A3C1F5D2B0E", InvoiceLineFilterConstants.ModeOfStatistics)
				);
			}

			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ChineseDescription, x => x.JI_NDescription, ResString.GetMultilingualString("E1543C18-4FBE-475E-A716-95FA54A64D67", InvoiceLineFilterConstants.ChineseDescription));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.Grouping, x => ((JobComInvoiceLine)x).JI_Group, ResString.GetMultilingualString("AD7B5F66-5914-4D1C-8F67-279858F06BBD", InvoiceLineFilterConstants.Grouping));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.DeclarationGoodsDescption, x => ((JobComInvoiceLine)x).JI_DeclarationGoodsDescription, ResString.GetMultilingualString("3A6725DA-A390-44DE-884D-F8A5CE15DF32", InvoiceLineFilterConstants.DeclarationGoodsDescption));
		}
	}
}
