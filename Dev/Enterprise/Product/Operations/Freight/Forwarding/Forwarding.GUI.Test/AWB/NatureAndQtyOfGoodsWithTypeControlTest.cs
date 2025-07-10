using System;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	class NatureAndQtyOfGoodsWithTypeControlTest : NatureAndQtyOfGoodsControlTest
	{
		public override void TestSetDetailsControl()
		{
			ExportAWBRateLine line = Factory.New<ExportAWBRateLine>();
			line.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;

			using (FormForTest form = new FormForTest(line, new NatureAndQtyOfGoodsWithTypeControl()))
			{
				form.Show();
				AssertEquals(typeof(NatureAndQtyOfGoodsTextControl), form.DetailControl.UserControlType);

				Action<string, Type> assertCorrectControlFromLineType = (lineType, control) =>
				{
					line.ER_NatureAndQtyOfGoodsType = lineType;
					AssertEquals(control, form.DetailControl.UserControlType);
				};

				assertCorrectControlFromLineType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, typeof(NatureAndQtyOfGoodsVolumeControl));
				assertCorrectControlFromLineType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, typeof(NatureAndQtyOfGoodsDimensionsControl));
				assertCorrectControlFromLineType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, typeof(NatureAndQtyOfGoodsSLACControl));
				assertCorrectControlFromLineType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin, typeof(NatureAndQtyOfGoodsOriginControl));
				assertCorrectControlFromLineType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, typeof(NatureAndQtyOfGoodsTextControl));
				assertCorrectControlFromLineType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery, typeof(NatureAndQtyOfGoodsLithiumBatteryControl));
			}
		}
	}
}
