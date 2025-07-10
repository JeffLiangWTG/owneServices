using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class VolumeUnitListTest : TestCase
	{
		public void TestConvertFromStandardCode()
		{
			AssertEquals("CubicDecimeters", VolumeUnitList.Codes.CubicDecimeters, VolumeUnitList.ConvertFromStandardCode(Core.Constants.Volume.CubicDecimetres));
			AssertEquals("CubicFeet", VolumeUnitList.Codes.CubicFeet, VolumeUnitList.ConvertFromStandardCode(Core.Constants.Volume.CubicFeet));
			AssertEquals("CubicInches", VolumeUnitList.Codes.CubicInches, VolumeUnitList.ConvertFromStandardCode(Core.Constants.Volume.CubicInches));
			AssertEquals("CubicMetres", VolumeUnitList.Codes.CubicMeters, VolumeUnitList.ConvertFromStandardCode(Core.Constants.Volume.CubicMetres));
			AssertEquals("Litre", VolumeUnitList.Codes.Liter, VolumeUnitList.ConvertFromStandardCode(Core.Constants.Volume.Litre));
		}
	}
}
