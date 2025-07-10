using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoods))]
	sealed class NatureAndQtyOfGoodsTest : Forwarding.AWB.Business.Testing.NatureAndQtyOfGoodsTest
	{
		public override Type ExpectedValidationType
		{
			get { return typeof(NatureAndQtyOfGoodsValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NatureAndQtyOfGoods(Factory.New<ExportAWBRateLine>());
		}
	}
}
