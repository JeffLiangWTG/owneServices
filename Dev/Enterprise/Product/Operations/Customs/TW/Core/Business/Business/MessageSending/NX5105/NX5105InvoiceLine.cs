using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Commodity_InvoiceLine : IInvoiceLine
	{
		public NX5105Commodity_InvoiceLine(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, "entryLine");
			entryHeader = Argument.NotNull(entryLine.Header, "header");
		}

		protected readonly CusEntryLine entryLine;
		readonly CusEntryHeader entryHeader;

		public ZString ChargesTypeCode => entryHeader.CH_DeclarationIncoterm;

		public virtual ZString CurrencyTypeCode => entryHeader.FirstInvoiceCurrencyCode;

		public virtual ZDecimal UnitPriceAmount => entryLine.CL_EntryLineUnitPrice;

		#region Not Applicable

		public ZDecimal ItemChargeAmount => ZDecimal.Zero;

		public ZDecimal SubTotalAmount => ZDecimal.Zero;

		#endregion
	}
}
