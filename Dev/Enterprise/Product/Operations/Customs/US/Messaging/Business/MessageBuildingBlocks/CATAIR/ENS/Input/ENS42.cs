namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("42")]
	public abstract partial class ENS42 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS42()
			: base("42")
		{
		}

		/// <summary>
		/// A code identifying the supplier. Refer to CBP Directive 3500-13 of November 24, 1986, for complete instructions on determining this code
		/// </summary>
		[MessageBlockString(15, 3, "M")]
		public ZString SupplierIDCode;

		/// <summary>
		/// The invoice number assigned by the supplier. This data field is required for electronic invoice entry summaries. Valid characters are alpha, numeric and dash (-) only.
		/// </summary>
		[MessageBlockString(17, 18, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString InvoiceNumber;

		/// <summary>
		/// A number from 0001 to 9999 representing an invoice line number or the first invoice line number in a range of lines associated with the entry summary line. This data field is required for electronic invoice entry summaries.
		/// </summary>
		[MessageBlockInt(4, 35, "M")]
		public ZInt BeginningInvoiceLineNumberA;

		/// <summary>
		/// A number from 0002 to 9999 representing the last number in a range of invoice line numbers associated with the entry summary line. If the Beginning Invoice Line Number (A) is not in a range of numbers, this data field must be space filled.
		/// </summary>
		[MessageBlockInt(4, 39, "C")]
		public ZInt EndingInvoiceLineNumberA;

		/// <summary>
		/// A number from 0002 to 9999 representing an invoice line number or the first invoice line number in a range of lines associated with the entry summary line.
		/// </summary>
		[MessageBlockInt(4, 43, "C")]
		public ZInt BeginningInvoiceLineNumberB;

		/// <summary>
		/// A number from 0003 to 9999 representing the last number in a range of invoice line numbers that is associated with the entry summary line. If the Beginning Invoice Line Number (B) is not the first in a range of numbers, this data field must be space filled.
		/// </summary>
		[MessageBlockInt(4, 47, "C")]
		public ZInt EndingInvoiceLineNumberB;

		/// <summary>
		/// A number from 0003 to 9999 representing an invoice line number or the first invoice line number in a range of lines associated with the entry summary line.
		/// </summary>
		[MessageBlockInt(4, 51, "C")]
		public ZInt BeginningInvoiceLineNumberC;

		/// <summary>
		/// A number from 0004 to 9999 representing the last number in a range of invoice line numbers associated with the entry summary line. If the Beginning Invoice Line Number (C) is not the first in a range of numbers, this data field must be space filled.
		/// </summary>
		[MessageBlockInt(4, 55, "C")]
		public ZInt EndingInvoiceLineNumberC;

		/// <summary>
		/// A number from 0004 to 9999 representing an invoice line number or first invoice line number in a range of lines associated with the entry summary line.
		/// </summary>
		[MessageBlockInt(4, 59, "C")]
		public ZInt BeginningInvoiceLineNumberD;

		/// <summary>
		/// A number from 0005 to 9999 representing the last number in a range of invoice line numbers associated with the entry summary line. If the Beginning Invoice Line Number (D) is not the first in a range of numbers, this data field must be space filled.
		/// </summary>
		[MessageBlockInt(4, 63, "C")]
		public ZInt EndingInvoiceLineNumberD;

		/// <summary>
		/// A number from 0005 to 9999 representing an invoice line number or the first invoice line number in a range of lines associated with the entry summary line.
		/// </summary>
		[MessageBlockInt(4, 67, "")]
		public ZInt BeginningInvoiceLineNumberE;

		/// <summary>
		/// A number from 0006 to 9999 representing the last number in a range of invoice line numbers associated with the entry summary line. If the Beginning Invoice Line Number (E) is not the first in a range of numbers, this data field must be space filled.
		/// </summary>
		[MessageBlockInt(4, 71, "C")]
		public ZInt EndingInvoiceLineNumberE;
	}
}
