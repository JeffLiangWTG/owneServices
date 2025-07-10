using CargoWise.Integration;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class WeightUnitListTest : TestCase
	{
		public void TestConvertFromFreightVolume()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			list.RemoveCode(Core.Constants.Weight.Pounds);
			list.RemoveCode(Core.Constants.Weight.LongTons);
			list.RemoveCode(Core.Constants.Weight.ShortTons);
			list.RemoveCode(Core.Constants.Weight.Tonnes);
			AssertEquals(WeightUnitList.Codes.Pounds, WeightUnitList.ConvertFromFreightWeight(Core.Constants.Weight.Pounds));
			AssertEquals(WeightUnitList.Codes.LongTon, WeightUnitList.ConvertFromFreightWeight(Core.Constants.Weight.LongTons));
			AssertEquals(WeightUnitList.Codes.ShortTon, WeightUnitList.ConvertFromFreightWeight(Core.Constants.Weight.ShortTons));
			AssertEquals(WeightUnitList.Codes.MetricTon, WeightUnitList.ConvertFromFreightWeight(Core.Constants.Weight.Tonnes));
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(WeightUnitList.Codes.Kilograms, WeightUnitList.ConvertFromFreightWeight(pair.Code));
			}

			AssertEquals("Z@", WeightUnitList.ConvertFromFreightWeight("Z@"));
		}
	}
}
