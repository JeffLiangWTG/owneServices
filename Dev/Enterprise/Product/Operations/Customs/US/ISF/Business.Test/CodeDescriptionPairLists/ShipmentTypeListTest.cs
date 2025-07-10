using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ShipmentTypeListTest : TestCase
	{
		public void TestIsBondDataRequired()
		{
			var list = new ShipmentTypeList();
			list.RemoveCode(ShipmentTypeList.Codes.StandardOrRegularFilings);
			list.RemoveCode(ShipmentTypeList.Codes.ToOrderShipments);
			list.RemoveCode(ShipmentTypeList.Codes.USReturnGoods);
			list.RemoveCode(ShipmentTypeList.Codes.FTZShipments);
			list.RemoveCode(ShipmentTypeList.Codes.OuterContinentalShelfShipments);
			AssertEquals(true, ShipmentTypeList.IsBondDataRequired(ShipmentTypeList.Codes.StandardOrRegularFilings));
			AssertEquals(true, ShipmentTypeList.IsBondDataRequired(ShipmentTypeList.Codes.ToOrderShipments));
			AssertEquals(true, ShipmentTypeList.IsBondDataRequired(ShipmentTypeList.Codes.USReturnGoods));
			AssertEquals(true, ShipmentTypeList.IsBondDataRequired(ShipmentTypeList.Codes.FTZShipments));
			AssertEquals(true, ShipmentTypeList.IsBondDataRequired(ShipmentTypeList.Codes.OuterContinentalShelfShipments));
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(false, ShipmentTypeList.IsBondDataRequired(pair.Code));
			}
		}

		public void TestIsPassportAllowed()
		{
			var list = new ShipmentTypeList();
			list.RemoveCode(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects);
			list.RemoveCode(ShipmentTypeList.Codes.DiplomaticShipment);
			list.RemoveCode(ShipmentTypeList.Codes.Carnet);
			AssertEquals(true, ShipmentTypeList.IsPassportAllowed(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects));
			AssertEquals(true, ShipmentTypeList.IsPassportAllowed(ShipmentTypeList.Codes.DiplomaticShipment));
			AssertEquals(true, ShipmentTypeList.IsPassportAllowed(ShipmentTypeList.Codes.Carnet));
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(false, ShipmentTypeList.IsPassportAllowed(pair.Code));
			}
		}

		public void TestICodeDescriptionPairListProviderMembers()
		{
			var list = new ShipmentTypeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}

		public void TestIsDOBRequired()
		{
			AssertEquals(true, ShipmentTypeList.IsDOBRequired(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects));
			AssertEquals(true, ShipmentTypeList.IsDOBRequired(ShipmentTypeList.Codes.DiplomaticShipment));
			AssertEquals(true, ShipmentTypeList.IsDOBRequired(ShipmentTypeList.Codes.Carnet));
			AssertEquals(true, ShipmentTypeList.IsDOBRequired(ShipmentTypeList.Codes.Informal));
			AssertEquals(false, ShipmentTypeList.IsDOBRequired(ShipmentTypeList.Codes.InternationalMailShipments));
		}
	}
}
