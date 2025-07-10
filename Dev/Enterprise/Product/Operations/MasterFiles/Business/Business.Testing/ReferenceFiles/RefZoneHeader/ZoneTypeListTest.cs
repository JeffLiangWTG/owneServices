using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ZoneTypeListTest : TestCase
	{
		public void TestGetCodes()
		{
			ZoneTypeList zones = new ZoneTypeList();
			zones.Add(ZoneTypeCodeDescriptionPair.All);
			zones.Add(ZoneTypeCodeDescriptionPair.Reporting);
			zones.Add(ZoneTypeCodeDescriptionPair.TransitWarehouse);

			List<string> zoneCodes = zones.GetCodes();

			AssertEquals("Should contain the code for ALL zone type", true, zoneCodes.Contains(ZoneTypeCodeDescriptionPair.All.Code));
			AssertEquals("Should contain the code for RPT zone type", true, zoneCodes.Contains(ZoneTypeCodeDescriptionPair.Reporting.Code));
			AssertEquals("Should not contain the code for Rating zone type", false, zoneCodes.Contains(ZoneTypeCodeDescriptionPair.RatingExport.Code));
			AssertEquals("Should contain the code for TransitWarehouse zone type", true, zoneCodes.Contains(ZoneTypeCodeDescriptionPair.TransitWarehouse.Code));
		}
	}
}
