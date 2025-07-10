using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsSLAC))]
	sealed class NatureAndQtyOfGoodsSLACTest : NatureAndQtyOfGoodsTest
	{
		public override void TestText()
		{
			AssertSerialization();
			AssertDeserialization();
		}

		public override void TestTextSize()
		{
			NatureAndQtyOfGoodsSLAC slac = (NatureAndQtyOfGoodsSLAC)GetNewBusinessObject();
			slac.Count = 99999;

			AssertEquals("prerequisite", false, slac.HasErrors);
			Assert("max dimension serialized", slac.Text.Length < 36);
		}

		void AssertSerialization()
		{
			NatureAndQtyOfGoodsSLAC slac = (NatureAndQtyOfGoodsSLAC)GetNewBusinessObject();
			AssertEquals("0 SLAC", slac.Text.ToString());

			slac.Count = 12;
			AssertEquals("12 SLAC", slac.Text.ToString());
		}

		void AssertDeserialization()
		{
			NatureAndQtyOfGoodsSLAC slac = (NatureAndQtyOfGoodsSLAC)GetNewBusinessObject();
			AssertEquals(0, slac.Count);

			slac.Text = "AAAA";
			AssertEquals(0, slac.Count);

			slac.Text = "9 SLAC";
			AssertEquals(9, slac.Count);
		}

		public void TestIsValidSLAC()
		{
			Assert("Should not be a valid SLAC for deserialization", !NatureAndQtyOfGoodsSLAC.IsValidSLAC("AAA"));
			Assert("Should not be a valid SLAC for deserialization", !NatureAndQtyOfGoodsSLAC.IsValidSLAC("SLAC"));
			Assert("Should not be a valid SLAC for deserialization", !NatureAndQtyOfGoodsSLAC.IsValidSLAC("0 SLAC"));
			Assert("Should not be a valid SLAC for deserialization", !NatureAndQtyOfGoodsSLAC.IsValidSLAC("123456 SLAC"));

			Assert("Should be a valid SLAC for deserialization", NatureAndQtyOfGoodsSLAC.IsValidSLAC("8 SLAC"));
			Assert("Should be a valid SLAC for deserialization", NatureAndQtyOfGoodsSLAC.IsValidSLAC("12345 SLAC"));
		}

		public override Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsSLACValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoodsSLAC(Factory.New<ExportAWBRateLine>());
		}
	}
}
