using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionLine : AutoCusWHSOperatorTransactionLine
	{
		public CusWHSOperatorTransactionLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Order))]
		public override ZGuid WOL_WOT_WHSOperatorTransactionOrder
		{
			get => base.WOL_WOT_WHSOperatorTransactionOrder;
			set => base.WOL_WOT_WHSOperatorTransactionOrder = value;
		}

		[RelatedBusinessObject(nameof(Receipt))]
		public override ZGuid WOL_WOT_WHSOperatorTransactionReceipt
		{
			get => base.WOL_WOT_WHSOperatorTransactionReceipt;
			set => base.WOL_WOT_WHSOperatorTransactionReceipt = value;
		}

		public virtual CusWHSOperatorTransaction Order
		{
			get { return Factory.Load<CusWHSOperatorTransaction>(WOL_WOT_WHSOperatorTransactionOrder); }
		}

		public virtual CusWHSOperatorTransaction Receipt
		{
			get { return Factory.Load<CusWHSOperatorTransaction>(WOL_WOT_WHSOperatorTransactionReceipt); }
		}

		[ResourceStringData("BC018E93-4CEA-4E1F-8218-AACBF1608A3E", Caption = "Receipt Reference")]
		public ZString ReceiptReference => Receipt?.WOT_OwnerReference ?? ZString.Empty;

		[ResourceStringData("D1B29F18-D04A-44D1-9BF6-096AA557B5ED", Caption = "Receipt Quantity")]
		public ZDecimal ReceiptQuantity => Receipt?.WOT_Quantity ?? ZDecimal.Zero;

		[ResourceStringData("0DA0D75D-20AF-4517-9015-18B4C006C3DF", Caption = "Order Reference")]
		public ZString OrderReference => Order?.WOT_OwnerReference ?? ZString.Empty;

		[ResourceStringData("78A0A1D3-7CB1-41C9-9DB9-E889C651ED1B", Caption = "Order Quantity")]
		public ZDecimal OrderQuantity => Order?.WOT_Quantity ?? ZDecimal.Zero;

		[ResourceStringData("A9B8C31F-B419-4D24-BB49-6CD5B76EDE02", Caption = "Line Quantity")]
		public override ZDecimal WOL_Quantity { get => base.WOL_Quantity; set => base.WOL_Quantity = value; }
	}
}
