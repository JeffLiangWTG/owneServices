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
	class VolumeUnitListTest : TestCase
	{
		public void TestConvertFromFreightVolume()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.Volume);
			list.RemoveCode(Core.Constants.Volume.CubicFeet);
			AssertEquals(VolumeUnitList.Codes.CubicFeet, VolumeUnitList.ConvertFromFreightVolume(Core.Constants.Volume.CubicFeet));
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(VolumeUnitList.Codes.CubicMeters, VolumeUnitList.ConvertFromFreightVolume(pair.Code));
			}

			AssertEquals("Z@", VolumeUnitList.ConvertFromFreightVolume("Z@"));
		}
	}
}
