using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingInvoice : SterlingRecord
	{
		public SterlingInvoice()
		{
		}

		#region Source

		public Xsd.TxnHeader Source
		{
			get
			{
				return fSource;
			}
			set
			{
				fSource = value;
			}
		}
		Xsd.TxnHeader fSource;

		#endregion

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "INV";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(DebtorOrCreditor);
			AddField(TxnType);
			AddField(TxnCount);
			AddField(TxnNumber);
			AddField(JobInvoiceNo);
			AddField(Description);
			AddField(InvoiceDate);
			AddField(InvTerm);
			AddField(InvTermDays);
			AddField(DueDate);
			AddField(PostDate);
			AddField(Branch);
			AddField(Department);
			AddField(LocalInvoiceAmtExclTax);
			AddField(LocalInvoiceAmtExclTaxCurrencyCode);
			AddField(LocalInvoiceAmtInclTax);
			AddField(LocalInvoiceAmtInclTaxCurrencyCode);
			AddField(LocalTaxAmount);
			AddField(LocalTaxAmountCurrencyCode);
			AddField(LocalWHTAmount);
			AddField(LocalWHTAmountCurrencyCode);
			AddField(OsInvoiceAmtExclTax);
			AddField(OsInvoiceAmtExclTaxCurrencyCode);
			AddField(OsInvoiceAmtInclTax);
			AddField(OsInvoiceAmtInclTaxCurrencyCode);
			AddField(OsTaxAmount);
			AddField(OsTaxAmountCurrencyCode);
			AddField(OsWHTAmount);
			AddField(OsWHTAmountCurrencyCode);
			AddField(CashBasisTaxIndicator);
			AddField(CreatedUserId);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region DebtorOrCreditor

		public ZString DebtorOrCreditor
		{
			get
			{
				return Source.DebtorOrCreditor.EDICode;
			}
		}

		#endregion

		#region TxnType

		public ZString TxnType
		{
			get
			{
				return Source.TxnType.ToString();
			}
		}

		#endregion

		#region TxnCount

		public ZString TxnCount
		{
			get
			{
				return Source.TxnCount;
			}
		}

		#endregion

		#region TxnNumber

		public ZString TxnNumber
		{
			get
			{
				return Source.TxnNumber;
			}
		}

		#endregion

		#region JobInvoiceNo

		public ZString JobInvoiceNo
		{
			get
			{
				return Source.JobInvoiceNo;
			}
		}

		#endregion

		#region Description

		public ZString Description
		{
			get
			{
				return Source.Description;
			}
		}

		#endregion

		#region InvoiceDate

		public ZString InvoiceDate
		{
			get
			{
				return ToTimeFormat(Source.InvoiceDate);
			}
		}

		#endregion

		#region InvTerm

		public ZString InvTerm
		{
			get
			{
				return Source.InvTerm;
			}
		}

		#endregion

		#region InvTermDays

		public ZString InvTermDays
		{
			get
			{
				return Source.InvTermDays;
			}
		}

		#endregion

		#region DueDate

		public ZString DueDate
		{
			get
			{
				return ToTimeFormat(Source.DueDate);
			}
		}

		#endregion

		#region PostDate

		public ZString PostDate
		{
			get
			{
				return ToTimeFormat(Source.PostDate);
			}
		}

		#endregion

		#region Branch

		public ZString Branch
		{
			get
			{
				return Source.Branch;
			}
		}

		#endregion

		#region Department

		public ZString Department
		{
			get
			{
				return Source.Department;
			}
		}

		#endregion

		#region LocalInvoiceAmtExclTax

		public ZString LocalInvoiceAmtExclTax
		{
			get
			{
				return Source.LocalInvoiceAmtExclTax.Value.ToString();
			}
		}

		#endregion

		#region LocalInvoiceAmtExclTaxCurrencyCode

		public ZString LocalInvoiceAmtExclTaxCurrencyCode
		{
			get
			{
				return Source.LocalInvoiceAmtExclTax.CurrencyCode;
			}
		}

		#endregion

		#region LocalInvoiceAmtInclTax

		public ZString LocalInvoiceAmtInclTax
		{
			get
			{
				return Source.LocalInvoiceAmtInclTax.Value.ToString();
			}
		}

		#endregion

		#region LocalInvoiceAmtInclTaxCurrencyCode

		public ZString LocalInvoiceAmtInclTaxCurrencyCode
		{
			get
			{
				return Source.LocalInvoiceAmtInclTax.CurrencyCode;
			}
		}

		#endregion

		#region LocalTaxAmount

		public ZString LocalTaxAmount
		{
			get
			{
				return Source.LocalTaxAmount.Value.ToString();
			}
		}

		#endregion

		#region LocalTaxAmountCurrencyCode

		public ZString LocalTaxAmountCurrencyCode
		{
			get
			{
				return Source.LocalTaxAmount.CurrencyCode;
			}
		}

		#endregion

		#region LocalWHTAmount

		public ZString LocalWHTAmount
		{
			get
			{
				return Source.LocalWHTAmount.Value.ToString();
			}
		}

		#endregion

		#region LocalWHTAmountCurrencyCode

		public ZString LocalWHTAmountCurrencyCode
		{
			get
			{
				return Source.LocalWHTAmount.CurrencyCode;
			}
		}

		#endregion

		#region OsInvoiceAmtExclTax

		public ZString OsInvoiceAmtExclTax
		{
			get
			{
				return Source.OsInvoiceAmtExclTax.Value.ToString();
			}
		}

		#endregion

		#region OsInvoiceAmtExclTaxCurrencyCode

		public ZString OsInvoiceAmtExclTaxCurrencyCode
		{
			get
			{
				return Source.OsInvoiceAmtExclTax.CurrencyCode;
			}
		}

		#endregion

		#region OsInvoiceAmtInclTax

		public ZString OsInvoiceAmtInclTax
		{
			get
			{
				return Source.OsInvoiceAmtInclTax.Value.ToString();
			}
		}

		#endregion

		#region OsInvoiceAmtInclTaxCurrencyCode

		public ZString OsInvoiceAmtInclTaxCurrencyCode
		{
			get
			{
				return Source.OsInvoiceAmtInclTax.CurrencyCode;
			}
		}

		#endregion

		#region OsTaxAmount

		public ZString OsTaxAmount
		{
			get
			{
				return Source.OsTaxAmount.Value.ToString();
			}
		}

		#endregion

		#region OsTaxAmountCurrencyCode

		public ZString OsTaxAmountCurrencyCode
		{
			get
			{
				return Source.OsTaxAmount.CurrencyCode;
			}
		}

		#endregion

		#region OsWHTAmount

		public ZString OsWHTAmount
		{
			get
			{
				return Source.OsWHTAmount.Value.ToString();
			}
		}

		#endregion

		#region OsWHTAmountCurrencyCode

		public ZString OsWHTAmountCurrencyCode
		{
			get
			{
				return Source.OsWHTAmount.CurrencyCode;
			}
		}

		#endregion

		#region CashBasisTaxIndicator

		public ZString CashBasisTaxIndicator
		{
			get
			{
				return Source.CashBasisTaxIndicator.ToString();
			}
		}

		#endregion

		#region CreatedUserId

		public ZString CreatedUserId
		{
			get
			{
				return Source.CreatedUserId;
			}
		}

		#endregion

		#region Invoice Item Info

		public SterlingInvoiceItemCollection InvoiceItemInfo
		{
			get
			{
				if (fInvoiceItemInfo == null)
				{
					fInvoiceItemInfo = new SterlingInvoiceItemCollection(this);
				}
				return fInvoiceItemInfo;
			}
		}
		SterlingInvoiceItemCollection fInvoiceItemInfo;

		#endregion

		#endregion

	}
}
