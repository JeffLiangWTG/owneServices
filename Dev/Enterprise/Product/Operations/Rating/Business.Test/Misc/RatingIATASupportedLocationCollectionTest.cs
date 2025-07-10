using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatingIATASupportedLocationCollection))]
	public class RatingIATASupportedLocationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RatingIATASupportedLocationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(RefUNLOCO));
		}

		public void TestContainsCode()
		{
			var collection = new RatingIATASupportedLocationCollection(Factory);
			var codeDescriptionPairList = (ICodeDescriptionPairList)collection;

			AssertEquals("LocationCollection should NOT have contained null string", false, codeDescriptionPairList.ContainsCode(null));
			AssertEquals("LocationCollection should NOT have contained empty code", false, codeDescriptionPairList.ContainsCode(""));
			AssertEquals("LocationCollection should NOT have contained 'Z'", false, codeDescriptionPairList.ContainsCode("Z"));
			AssertEquals("LocationCollection should NOT have contained 'ZZ'", false, codeDescriptionPairList.ContainsCode("ZZ"));
			AssertEquals("LocationCollection should NOT have contained 'ZZZ'", false, codeDescriptionPairList.ContainsCode("ZZZ"));
			AssertEquals("LocationCollection should NOT have contained 'ZZZZ'", false, codeDescriptionPairList.ContainsCode("ZZZZ"));
			AssertEquals("LocationCollection should NOT have contained 'ZZZZZ'", false, codeDescriptionPairList.ContainsCode("ZZZZZ"));
			AssertEquals("LocationCollection should have contained 'AU'", true, codeDescriptionPairList.ContainsCode("AU"));
			AssertEquals("LocationCollection should have contained 'SYD'", true, codeDescriptionPairList.ContainsCode("SYD"));
			AssertEquals("LocationCollection should have contained 'GBLON'", true, codeDescriptionPairList.ContainsCode("GBLON"));
			AssertEquals("LocationCollection should have contained 'MEAR'", true, codeDescriptionPairList.ContainsCode("MEAR"));
		}

		public void TestGetDescriptionFromCode()
		{
			var collection = new RatingIATASupportedLocationCollection(Factory);
			var codeDescriptionPairList = (ICodeDescriptionPairList)collection;

			AssertEquals("Description of 'null' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode(null));
			AssertEquals("Description of '' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode(""));
			AssertEquals("Description of 'X' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode("Z"));
			AssertEquals("Description of 'XX' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode("ZZ"));
			AssertEquals("Description of 'XXX' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode("ZZZ"));
			AssertEquals("Description of 'XXXX' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode("ZZZZ"));
			AssertEquals("Description of 'XXXXX' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode("ZZZZZ"));
			AssertEquals("Description of 'AU' should have been 'Australia'", "Australia", codeDescriptionPairList.GetDescriptionFromCode("AU"));
			AssertEquals("Description of 'SYD' should have been empty string", "", codeDescriptionPairList.GetDescriptionFromCode("SYD"));
			AssertEquals("Description of 'BNE' should have been 'Brisbane'", "Brisbane", codeDescriptionPairList.GetDescriptionFromCode("BNE"));
			AssertEquals("Description of 'GBLON' should have been 'London'", "London", codeDescriptionPairList.GetDescriptionFromCode("GBLON"));
			AssertEquals("Description of 'OCEG' should have been 'Middle East'", "Middle East", codeDescriptionPairList.GetDescriptionFromCode("MEAR"));
		}

		public void TestDescriptionFromCode()
		{
			var locationCollection = new RatingIATASupportedLocationCollection(Factory);
			var findBoxListProvider = (IFindBoxListProvider)locationCollection;

			AssertNull("Description of null code (invalid code) should have been null", findBoxListProvider.DescriptionFromCode(null));
			AssertNull("Description of 'X' (invalid code) should have been null", findBoxListProvider.DescriptionFromCode("X"));

			AssertEquals("Description of 'AUSYD' should have been 'Sydney'", "Sydney", findBoxListProvider.DescriptionFromCode("AUSYD"));
			AssertEquals("Description of 'AU' should have been 'Australia'", "Australia", findBoxListProvider.DescriptionFromCode("AU"));
			AssertEquals("Description of 'SYD' should have been empty string", "", findBoxListProvider.DescriptionFromCode("SYD"));
			AssertEquals("Description of 'BNE' should have been 'Brisbane'", "Brisbane", findBoxListProvider.DescriptionFromCode("BNE"));
			AssertEquals("Description of 'MEAR' should have been 'Middle East'", "Middle East", findBoxListProvider.DescriptionFromCode("MEAR"));

			locationCollection = new RatingIATASupportedLocationCollection(Factory, false);
			findBoxListProvider = locationCollection;
			AssertNull("Description Code - zones not allowed so no description", findBoxListProvider.DescriptionFromCode("MEAR"));

			var zoneList = new ZoneTypeList();
			zoneList.Add(ZoneTypeCodeDescriptionPair.Rating);
			locationCollection = new RatingIATASupportedLocationCollection(Factory, zoneList);
			findBoxListProvider = locationCollection;
			AssertNull("No description as XXXX zone is not allowed zone type", findBoxListProvider.DescriptionFromCode("MEAR"));
		}

		public void TestAutoCompleteOnCommit()
		{
			var locationCollection = new LocationCollection(Factory);
			AssertEquals(true, ((IFindBoxListProvider)locationCollection).AutoCompleteOnCommit);
		}

		public void TestNearestMatch()
		{
			var locationCollection = new RatingIATASupportedLocationCollection(Factory);
			var findBoxListProvider = (IFindBoxListProvider)locationCollection;

			AssertEquals("NearestMatch method should have returned 'XX'", "XX", findBoxListProvider.NearestMatch("XX", false, -1).Item1);
			AssertEquals("NearestMatch method should have returned 'AUSYD'", "AUSYD", findBoxListProvider.NearestMatch("AUSYD", false, -1).Item1);
			AssertEquals("NearestMatch method should have returned 'AU'", "AU", findBoxListProvider.NearestMatch("AU", false, -1).Item1);
			AssertEquals("NearestMatch method should have returned 'SYD'", "SYD", findBoxListProvider.NearestMatch("SYD", false, -1).Item1);
			AssertEquals("NearestMatch method should have returned 'IQBSR'", "IQBSR", findBoxListProvider.NearestMatch("BSR", false, -1).Item1);
			AssertEquals("NearestMatch method should have returned 'USSWF'", "USSWF", findBoxListProvider.NearestMatch("SWF", false, -1).Item1);
			AssertEquals("NearestMatch method should have returned 'MEAR'", "MEAR", findBoxListProvider.NearestMatch("MEAR", false, -1).Item1);
		}
	}
}
