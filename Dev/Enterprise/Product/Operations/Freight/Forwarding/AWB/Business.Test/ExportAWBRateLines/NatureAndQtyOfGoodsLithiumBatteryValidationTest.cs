using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsLithiumBatteryValidation))]
	public class NatureAndQtyOfGoodsLithiumBatteryValidationTest : NatureAndQtyOfGoodsValidationTest
	{
		protected override void AssertText()
		{
			Assert("n/a", true);
		}

		public void TestValidateLithiumBatteryType()
		{
			var line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;

			var natureAndQtyOfGoods = new NatureAndQtyOfGoodsLithiumBattery(line);
			AssertNoErrors(natureAndQtyOfGoods.LithiumBatteryTypeInfo);

			natureAndQtyOfGoods.LithiumBatteryType = "11111";
			AssertHasError(natureAndQtyOfGoods.LithiumBatteryTypeInfo, "Enter a valid selection.");

			natureAndQtyOfGoods.LithiumBatteryType = "";
			AssertHasError(natureAndQtyOfGoods.LithiumBatteryTypeInfo, "Please enter a value.");
		}

		public void TestValidateHasEnoughSpaceToDisplayRemainingDescription()
		{
			var header = Factory.New<ExportAWBHeader>();

			header.AWBRateLine1.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine1.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968;
			header.AWBRateLine2.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;

			header.AWBRateLine1.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateLithiumBatteryType();
			AssertNoErrors("Next line is empty.", header.AWBRateLine1.ER_NatureAndQtyOfGoodsTypeInfo);

			header.AWBRateLine3.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968;
			header.AWBRateLine4.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLine4.NatureAndQtyOfGoods.Text = "Goods description";
			header.AWBRateLine5.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			header.AWBRateLine5.NatureAndQtyOfGoods.Text = "Goods description";

			header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateLithiumBatteryType();
			AssertHasError("Next line is not empty.", header.AWBRateLine3.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, "This is a multi-line entry (3 lines), please leave enough empty lines.");

			header.AWBRateLine12.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			header.AWBRateLine12.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryType = Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968;

			header.AWBRateLine12.NatureAndQtyOfGoodsLithiumBattery.Validation.ValidateLithiumBatteryType();
			AssertHasError("There is no next line.", header.AWBRateLine12.NatureAndQtyOfGoodsLithiumBattery.LithiumBatteryTypeInfo, "This is a multi-line entry (3 lines), please leave enough empty lines.");
		}

		protected override NatureAndQtyOfGoodsValidation GetNewValidation(NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsLithiumBatteryValidation((NatureAndQtyOfGoodsLithiumBattery)parent);
		}

		protected override NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoodsLithiumBattery(Factory.New<ExportAWBRateLine>());
		}
	}
}
