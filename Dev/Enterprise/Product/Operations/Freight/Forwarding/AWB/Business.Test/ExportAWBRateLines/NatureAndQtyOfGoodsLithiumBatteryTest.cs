using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsLithiumBattery))]
	public class NatureAndQtyOfGoodsLithiumBatteryTest : NatureAndQtyOfGoodsTest
	{
		public override void TestText()
		{
			AssertSerialization();
			AssertDeserialization();
		}

		public override void TestTextSize()
		{
			var lithiumBattery = (NatureAndQtyOfGoodsLithiumBattery)GetNewBusinessObject();
			lithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI965;

			AssertEquals("prerequisite", false, lithiumBattery.HasErrors);
			Assert("max lithiumBattery serialized", lithiumBattery.Text.Length < 36);
		}

		void AssertSerialization()
		{
			var lithiumBattery = (NatureAndQtyOfGoodsLithiumBattery)GetNewBusinessObject();
			AssertEquals(string.Empty, lithiumBattery.Text.ToString());

			lithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI965;
			AssertEquals("Lithium Battery: PI965", lithiumBattery.Text.ToString());
		}

		void AssertDeserialization()
		{
			var lithiumBattery = (NatureAndQtyOfGoodsLithiumBattery)GetNewBusinessObject();
			AssertEquals(string.Empty, lithiumBattery.LithiumBatteryType);

			lithiumBattery.Text = "Lithium Battery: PI965";
			AssertEquals(Constants.AWB.LithiumBatteryTypes.Codes.PI965, lithiumBattery.LithiumBatteryType);
		}

		public void TestTextAndDescription()
		{
			var line = Factory.New<ExportAWBRateLine>();

			line.NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery;
			var lithiumBattery = line.NatureAndQtyOfGoodsLithiumBattery;
			lithiumBattery.LithiumBatteryType = Constants.AWB.LithiumBatteryTypes.Codes.PI968;

			AssertEquals("Lithium Battery: PI968", lithiumBattery.Text);
			AssertEquals("Lithium metal batteries in ", lithiumBattery.WrappedDescriptions[0]);
			AssertEquals("compliance with Section II of PI968", lithiumBattery.WrappedDescriptions[1]);
			AssertEquals(" CAO", lithiumBattery.WrappedDescriptions[2]);
		}

		public override Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsLithiumBatteryValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoodsLithiumBattery(Factory.New<ExportAWBRateLine>());
		}
	}
}
