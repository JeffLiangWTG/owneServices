using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusLiquidationCollection))]
	sealed class CusLiquidationCollectionTest : ActiveBusinessObjectCollectionTestCase<CusLiquidationCollection>
	{
		public void TestGetMostRecentLiquidation()
		{
			var liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_SystemCreateDate = new ZDateTime(2009, 11, 21);
			liquidation.B8_LiquidationDate = new ZDateTime(2009, 11, 20);
			Declaration.Liquidations.Add(liquidation);
			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_SystemCreateDate = new ZDateTime(2009, 11, 24);
			liquidation2.B8_LiquidationDate = new ZDateTime(2009, 11, 23);
			liquidation2.B8_LiquidationType = LiquidationTypeCodeList.Codes.Code04;
			Declaration.Liquidations.Add(liquidation2);
			var liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_SystemCreateDate = new ZDateTime(2009, 11, 22);
			liquidation3.B8_LiquidationDate = new ZDateTime(2009, 11, 21);
			liquidation3.B8_LiquidationType = LiquidationTypeCodeList.Codes.Code08;
			Declaration.Liquidations.Add(liquidation3);
			var declaration2 = Factory.New<JobDeclaration>();
			var liquidation4 = Factory.New<CusLiquidation>();
			liquidation4.B8_SystemCreateDate = new ZDateTime(2009, 11, 25);
			liquidation4.B8_LiquidationDate = new ZDateTime(2009, 11, 25);
			liquidation4.B8_LiquidationType = LiquidationTypeCodeList.Codes.Code01;
			declaration2.Liquidations.Add(liquidation4);
			var collection = GetCollectionToTest();
			AssertEquals(liquidation2, collection.GetMostRecentLiquidation());
			AssertEquals(new ZDateTime(2009, 11, 23), collection.GetMostRecentLiquidationDate());
			AssertEquals(LiquidationTypeCodeList.Codes.Code04, collection.GetMostRecentLiquidationType());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusLiquidation>();

		protected override CusLiquidationCollection GetCollectionToTest() => Declaration.Liquidations;

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
