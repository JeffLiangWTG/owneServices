using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingInvoiceItem : SterlingRecord
	{
		public SterlingInvoiceItem()
		{
		}

		#region Source

		public Xsd.TxnLine Source
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
		Xsd.TxnLine fSource;

		#endregion

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "IIT";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(LineType);
			AddField(Sequence);
			AddField(ChargeCode);
			AddField(ChargeGroup);
			AddField(ChargeCodeSalesGroup);
			AddField(ChargeCodeExpenseGroup);
			AddField(Description);
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
			AddField(DepartmentActivity);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region LineType

		public ZString LineType
		{
			get
			{
				return Source.LineType.ToString();
			}
		}

		#endregion

		#region Sequence

		public ZString Sequence
		{
			get
			{
				return Source.Sequence;
			}
		}

		#endregion

		#region ChargeCode

		public ZString ChargeCode
		{
			get
			{
				return Source.ChargeCode;
			}
		}

		#endregion

		#region ChargeGroup

		public ZString ChargeGroup
		{
			get
			{
				return Source.ChargeGroup;
			}
		}

		#endregion

		#region ChargeCodeSalesGroup

		public ZString ChargeCodeSalesGroup
		{
			get
			{
				return Source.ChargeCodeSalesGroup;
			}
		}

		#endregion

		#region ChargeCodeExpenseGroup

		public ZString ChargeCodeExpenseGroup
		{
			get
			{
				return Source.ChargeCodeExpenseGroup;
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

		#region DepartmentActivity

		public ZString DepartmentActivity
		{
			get
			{
				return Source.DepartmentActivity.ToString();
			}
		}

		#endregion

		#endregion
	}
}
