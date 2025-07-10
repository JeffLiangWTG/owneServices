using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceLineCOValidationTest : JobComInvoiceLineValidationTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.COO;
			}
		}

		public void TestItemQuantity()
		{
			Validation.ValidateJI_CustomsQuantity();
			AssertEquals(true, InvoiceLine.JI_CustomsQuantityInfo.HasMessageErrors());
			InvoiceLine.JI_CustomsQuantity = 100;
			Validation.ValidateJI_CustomsQuantity();
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.HasMessageErrors());
		}

		public void TestItemUnitofQuantity()
		{
			InvoiceLine.JI_CustomsUnitQty = null;
			Validation.ValidateJI_CustomsUnitQty();
			AssertEquals(true, InvoiceLine.JI_CustomsUnitQtyInfo.HasMessageErrors());
			InvoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.KGM;
			Validation.ValidateJI_CustomsUnitQty();
			AssertEquals(false, InvoiceLine.JI_CustomsUnitQtyInfo.HasMessageErrors());
		}

		public void TestFOBValue()
		{
			Validation.ValidateJI_LinePrice();
			AssertEquals(true, InvoiceLine.JI_LinePriceInfo.HasMessageErrors());
			InvoiceLine.JI_LinePrice = 110;
			Validation.ValidateJI_LinePrice();
			AssertEquals(false, InvoiceLine.JI_LinePriceInfo.HasMessageErrors());
		}
	}
}
