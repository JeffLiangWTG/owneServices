using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCommodityLine : AutoDISCommodityLine
	{
		public DISCommodityLine(DISDocument disDocument)
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

		[ReadOnlyMember(nameof(IsRangeOfVNELineNumbersSpecified))]
		public override ZInt InvoiceLineTo
		{
			get { return base.InvoiceLineTo; }
			set { base.InvoiceLineTo = value; }
		}

		bool IsRangeOfVNELineNumbersSpecified
		{
			get { return VNELineNumber > 0; }
		}

		[ReadOnlyMember(nameof(IsRangeOfInvoiceLinesSpecified))]
		public override ZInt VNELineNumber
		{
			get { return base.VNELineNumber; }
			set { base.VNELineNumber = value; }
		}

		[ReadOnlyMember(nameof(IsRangeOfInvoiceLinesSpecified))]
		public override ZInt VNELineNumberTo
		{
			get { return base.VNELineNumberTo; }
			set { base.VNELineNumberTo = value; }
		}

		bool IsRangeOfInvoiceLinesSpecified
		{
			get { return InvoiceLineTo > 0; }
		}

		public IEnumerable<IDISCommodityLine> CommodityLines
		{
			get
			{
				if (VNELineNumberTo > 0)
				{
					return disDocument.HostWrapper.GetCommodityLinesWithRangesOfVNELines(InvoiceNumber, InvoiceLineNumber, VNELineNumber, VNELineNumberTo);
				}
				else if (InvoiceLineTo > 0)
				{
					return disDocument.HostWrapper.GetCommodityLinesWithRangesOfInvoiceLines(InvoiceNumber, InvoiceLineNumber, InvoiceLineTo);
				}
				return disDocument.HostWrapper.GetCommodityLines(InvoiceNumber, InvoiceLineNumber, VNELineNumber);
			}
		}
	}
}
