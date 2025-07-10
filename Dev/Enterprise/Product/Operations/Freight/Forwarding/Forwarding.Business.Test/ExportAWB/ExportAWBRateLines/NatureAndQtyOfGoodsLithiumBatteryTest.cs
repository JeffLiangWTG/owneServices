using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolNatureAndQtyOfGoodsLithiumBattery))]
	sealed class NatureAndQtyOfGoodsLithiumBatteryTest : Forwarding.AWB.Business.Testing.NatureAndQtyOfGoodsLithiumBatteryTest
	{
		public override Type ExpectedValidationType
		{
			get { return typeof(ConsolNatureAndQtyOfGoodsLithiumBatteryValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolNatureAndQtyOfGoodsLithiumBattery(Factory.New<ConsolExportAWBRateLine>());
		}
	}
}
