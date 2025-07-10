using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class DocPackingLineBuilderTest : TestCaseWithFactory
	{
		public void TestValidateAmountQuantity_OverMaximumValue() => TestValidateAmountQuantity(100000, "Amount Quantity can not be greater than 99999.");
		public void TestValidateAmountQuantity_EqualMaximumValue() => TestValidateAmountQuantity(99999, null);
		public void TestValidateAmountQuantity_UnderMinimumValue() => TestValidateAmountQuantity(-100000, "Amount Quantity can not be less than -99999.");
		public void TestValidateAmountQuantity_EqualMinimumValue() => TestValidateAmountQuantity(-99999, null);

		void TestValidateAmountQuantity(int amountWeight, string expectedMessage)
		{
			var docPackingLine = BuildDocPackingLine(amountWeight, 1m);
			if (expectedMessage != null)
			{
				AssertHasMessageError(docPackingLine.AmountQuantityInfo, expectedMessage);
			}
			else
			{
				AssertNoErrors(docPackingLine.AmountQuantityInfo);
			}
		}

		public void TestValidateAmountWeight_OverMaximumValue() => TestValidateAmountWeight(100000m, "Amount Weight can not be greater than 99999.999.");
		public void TestValidateAmountWeight_EqualMaximumValue() => TestValidateAmountWeight(99999m, null);
		public void TestValidateAmountWeight_UnderMinimumValue() => TestValidateAmountWeight(-100000m, "Amount Weight can not be less than -99999.999.");
		public void TestValidateAmountWeight_EqualMinimumValue() => TestValidateAmountWeight(-99999m, null);
		public void TestValidateAmountWeight_DecimalDigitsOverLimit_AutoRound()
		{
			var docPackingLine = BuildDocPackingLine(1, 123.456789m);

			AssertNoErrors(docPackingLine.AmountWeightInfo);
			AssertEquals(docPackingLine.AmountWeight, 123.457m);
		}

		void TestValidateAmountWeight(decimal amountWeight, string expectedMessage)
		{
			var docPackingLine = BuildDocPackingLine(1, amountWeight);
			if (expectedMessage != null)
			{
				AssertHasMessageError(docPackingLine.AmountWeightInfo, expectedMessage);
			}
			else
			{
				AssertNoErrors(docPackingLine.AmountWeightInfo);
			}
		}

		#region Implemention

		protected void AssertGoodsDetails(DocPackingLine goods, ZInt qty, ZDecimal weight, string description = "", string refTypeCode = "", string refCode = "")
		{
			AssertNotNull(goods);
			AssertEquals(qty, goods.AmountQuantity);
			AssertEquals(weight, goods.AmountWeight);
			if (!string.IsNullOrEmpty(description))
			{
				AssertEquals(description, goods.Description);
			}
			if (!string.IsNullOrEmpty(refTypeCode))
			{
				AssertEquals(refTypeCode, goods.RefType.Code);
			}
			if (!string.IsNullOrEmpty(refCode))
			{
				AssertEquals(refCode, goods.RefCode);
			}
		}

		protected void SetPackageStateDetails(WhsItemPackageState packageState, ZDecimal weight, ZString description, string receiveAs = "SCN")
		{
			packageState.WPS_ReceivedAs = receiveAs;
			packageState.Package.KP_Weight = weight;
			packageState.Package.KP_GoodsDescription = description;
		}

		DocPackingLine BuildDocPackingLine(ZInt amountQuantity, ZDecimal amountWeight)
		{
			var docPackingLine = new DocPackingLineBuilder().Build();
			docPackingLine.AmountQuantity = amountQuantity;
			docPackingLine.AmountWeight = amountWeight;
			return docPackingLine;
		}

		#endregion

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
