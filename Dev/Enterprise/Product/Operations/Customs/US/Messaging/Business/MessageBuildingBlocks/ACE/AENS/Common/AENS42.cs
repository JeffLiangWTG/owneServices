namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("42")]
	[OutputBlock("42")]
	public partial class AENS42 : MessageBlock
	{
		public AENS42()
			: base("42")
		{
		}

		/// <summary>
		/// A code identifying the supplier. Refer to CBP Directive 3500-13 of November 24, 1986, for complete instructions on determining this code.
		/// </summary>
		[MessageBlockString(15, 3, "M")]
		public ZString SupplierIDCode;

		/// <summary>
		/// The invoice number as issued by the Supplier. (Use alphanumeric and dash ['-'] only.)
		/// </summary>
		[MessageBlockString(17, 18, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString InvoiceNumber;

		/// <summary>
		/// The first invoice line number that references the article.
		/// </summary>
		[MessageBlockInt(4, 36, "M")]
		public ZInt InvoiceLineRange1Begin;

		/// <summary>
		/// The ending invoice line number that references the article if a contiguous range of numbers is needed. If only a single line number is needed to specify the reference, repeat the beginning line number.
		/// </summary>
		[MessageBlockInt(4, 41, "M")]
		public ZInt InvoiceLineRange1End;

		/// <summary>
		/// An additional contiguous line number range that references the article.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockInt(4, 46, "C")]
		public ZInt InvoiceLineRange2Begin;

		/// <summary>
		/// An additional contiguous line number range that references the article.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockInt(4, 51, "C")]
		public ZInt InvoiceLineRange2End;

		/// <summary>
		/// An additional contiguous line number range that references the article.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockInt(4, 56, "C")]
		public ZInt InvoiceLineRange3Begin;

		/// <summary>
		/// An additional contiguous line number range that references the article.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockInt(4, 61, "C")]
		public ZInt InvoiceLineRange3End;

		/// <summary>
		/// An additional contiguous line number range that references the article.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockInt(4, 66, "C")]
		public ZInt InvoiceLineRange4Begin;

		/// <summary>
		/// An additional contiguous line number range that references the article.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockInt(4, 71, "C")]
		public ZInt InvoiceLineRange4End;
	}
}
