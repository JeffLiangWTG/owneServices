using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CBP7512DocumentLine))]
	sealed class CBP7512DocumentLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckBI_Weight()
		{
			CusInBondMoveLineItem moveLineItem = MoveDetail.CBP7512Lines.AddNew();
			moveLineItem.BI_Weight = 9999999999m;
			moveLineItem.BI_WeightUnit = WeightUnitList.Codes.Kilograms;
			Assert(((CargoWise.ComponentModel.INotificationProvider)moveLineItem.BI_WeightInfo).HasNotifications(CargoWise.ComponentModel.NotificationType.Error));
			var message = ((CargoWise.ComponentModel.NotificationCollection)moveLineItem.BI_WeightInfo.Notifications)[0].Message;
			AssertEquals("The number 9,999,999,999 is too large, the maximum value allowed for Weight is 999,999,999.", message);
			CBP7512DocumentLine line = new CBP7512DocumentLine();
			line.UpdateFrom(moveLineItem);
			AssertEquals("9999999999 K", line.WeightFormatted);
			moveLineItem.BI_Weight = 9.99m;
			line.UpdateFrom(moveLineItem);
			AssertEquals("10 K", line.WeightFormatted);
		}

		public void TestUpdateFrom()
		{
			CusInBondMoveLineItem moveLineItem = MoveDetail.CBP7512Lines.AddNew();
			moveLineItem.BI_MarksAndNumbers = "MARKS AND NUMBERS";
			moveLineItem.BI_Description = "DESCRIPTION";
			moveLineItem.BI_Weight = 10.00m;
			moveLineItem.BI_WeightUnit = WeightUnitList.Codes.Kilograms;
			moveLineItem.BI_MonetaryValue = 145.00m;
			moveLineItem.BI_IsMonetaryValueEstimated = ZBool.True;
			moveLineItem.BI_RateComment = "RATE 12";
			moveLineItem.BI_DutyComment = "DUTY 324";
			CBP7512DocumentLine line = new CBP7512DocumentLine();
			AssertEquals("", line.MarksAndNumbers);
			AssertEquals("", line.DescriptionAndQtyOfMerchandise);
			AssertEquals("", line.WeightFormatted);
			AssertEquals("", line.GoodsValueInLocalCurrency);
			AssertEquals(false, line.GoodsValueEstimated);
			AssertEquals("", line.Rate);
			AssertEquals("", line.Duty);
			line.UpdateFrom(moveLineItem);
			AssertEquals("MARKS AND NUMBERS", line.MarksAndNumbers);
			AssertEquals("DESCRIPTION", line.DescriptionAndQtyOfMerchandise);
			AssertEquals("10 K", line.WeightFormatted);
			AssertEquals("145", line.GoodsValueInLocalCurrency);
			AssertEquals(true, line.GoodsValueEstimated);
			AssertEquals("RATE 12", line.Rate);
			AssertEquals("DUTY 324", line.Duty);
			moveLineItem.BI_WeightUnit = WeightUnitList.Codes.Pounds;
			moveLineItem.BI_IsMonetaryValueEstimated = ZBool.False;
			line.UpdateFrom(moveLineItem);
			AssertEquals("MARKS AND NUMBERS", line.MarksAndNumbers);
			AssertEquals("DESCRIPTION", line.DescriptionAndQtyOfMerchandise);
			AssertEquals("10 L", line.WeightFormatted);
			AssertEquals("145", line.GoodsValueInLocalCurrency);
			AssertEquals(false, line.GoodsValueEstimated);
			AssertEquals("RATE 12", line.Rate);
			AssertEquals("DUTY 324", line.Duty);
			moveLineItem.BI_Weight = 11m;
			line.UpdateFrom(moveLineItem);
			AssertEquals("11 L", line.WeightFormatted);
			moveLineItem.BI_WeightUnit = Core.Constants.Weight.PoundsTroy;
			line.UpdateFrom(moveLineItem);
			AssertEquals("9 L", line.WeightFormatted);
			moveLineItem.BI_WeightUnit = WeightUnitList.Codes.Kilograms;
			line.UpdateFrom(moveLineItem);
			AssertEquals("11 K", line.WeightFormatted);
			moveLineItem.BI_WeightUnit = Core.Constants.Weight.Tonnes;
			line.UpdateFrom(moveLineItem);
			AssertEquals("11000 K", line.WeightFormatted);
		}

		protected override BusinessObject GetNewBusinessObject() => new CBP7512DocumentLine();

		CusInBondMoveDetail MoveDetail
		{
			get
			{
				if (moveDetail == null)
				{
					CusInBondHeader header = Factory.New<CusInBondHeader>();
					CusInBondBill bill = header.Bills.AddNew();
					CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
					moveDetail = moveHeader.MovementDetails.AddNew();
					moveDetail.B9_B0 = bill.PK;
				}

				return moveDetail;
			}
		}

		CusInBondMoveDetail moveDetail;
	}
}
