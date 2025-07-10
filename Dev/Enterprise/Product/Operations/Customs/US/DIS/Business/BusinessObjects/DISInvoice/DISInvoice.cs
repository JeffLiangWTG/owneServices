using CargoWise.ComponentModel;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISInvoice : AutoDISInvoice
	{
		public DISInvoice(DISDocument disDocument)
			: base(disDocument.Factory)
		{
			this.disDocument = disDocument;
		}

		internal readonly DISDocument disDocument;

		[List(nameof(DefaultInvoiceList))]
		public override ZString InvoiceNumber
		{
			get { return base.InvoiceNumber; }
			set { base.InvoiceNumber = value; }
		}

		public CodeDescriptionPairList DefaultInvoiceList
		{
			get { return disDocument.DefaultInvoiceList; }
		}

		public DISInvoiceLineRangeCollection InvoiceLineRanges
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new DISInvoiceLineRangeCollection(this);
					RegisterEditableChildObject(invoiceLines);
				}
				return invoiceLines;
			}
		}
		DISInvoiceLineRangeCollection invoiceLines;
	}
}
