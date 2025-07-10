using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsOrigin))]
	sealed class NatureAndQtyOfGoodsOriginTest : NatureAndQtyOfGoodsTest
	{
		public override void TestText()
		{
			AssertSerialization();
			AssertDeserialization();
		}

		public override void TestTextSize()
		{
			NatureAndQtyOfGoodsOrigin origin = (NatureAndQtyOfGoodsOrigin)GetNewBusinessObject();
			origin.Country = Core.Constants.CountryCodes.Australia;

			AssertEquals("prerequisite", false, origin.HasErrors);
			Assert("max dimension serialized", origin.Text.Length < 36);
		}

		void AssertSerialization()
		{
			NatureAndQtyOfGoodsOrigin origin = (NatureAndQtyOfGoodsOrigin)GetNewBusinessObject();
			AssertEquals(ZString.Empty, origin.Text.ToString());

			origin.Country = Core.Constants.CountryCodes.Australia;
			AssertEquals("Goods Origin: AU", origin.Text.ToString());
		}

		void AssertDeserialization()
		{
			NatureAndQtyOfGoodsOrigin origin = (NatureAndQtyOfGoodsOrigin)GetNewBusinessObject();
			AssertEquals(ZString.Empty, origin.Country);

			origin.Text = "AAAA";
			AssertEquals(ZString.Empty, origin.Country);

			origin.Text = "Goods Origin: AU";
			AssertEquals("AU", origin.Country);
		}

		public void TestIsValidOrigin()
		{
			Assert("Should not be a valid origin for deserialization", !NatureAndQtyOfGoodsOrigin.IsValidOrigin("AAA", Factory));
			Assert("Should not be a valid origin for deserialization", !NatureAndQtyOfGoodsOrigin.IsValidOrigin("Goods Origin: ", Factory));
			Assert("Should not be a valid origin for deserialization", !NatureAndQtyOfGoodsOrigin.IsValidOrigin("Goods Origin: XX", Factory));

			Assert("Should be a valid SLAC for deserialization", NatureAndQtyOfGoodsOrigin.IsValidOrigin("Goods Origin: AU", Factory));
		}

		public override Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsOriginValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoodsOrigin(Factory.New<ExportAWBRateLine>());
		}
	}
}
