using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Units = CargoWise.Definitions.RefPackTypeStandardUnits;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefPackTypeCollection))]
	sealed class RefPackTypeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefPackTypeCollection>
	{
		public void TestConstructor()
		{
			var refPackTypes = new RefPackTypeCollection(Factory);
			AssertEquals(false, refPackTypes.ContainsCode(RefPackTypeCollection.ReservedContainerType));

			var refPackTypesIncludeCNT = new RefPackTypeCollection(Factory, false);
			AssertEquals(true, refPackTypesIncludeCNT.ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}

		public void TestGetStandardUnitsAsCodeDescriptionPairs()
		{
			var standardUnitDescriptions = typeof(Units.Descriptions)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && fi.FieldType == typeof(string))
				.ToDictionary(x => x.Name, x => (string)x.GetRawConstantValue());

			var standardUnitCodeAndDescriptions = typeof(Units.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && fi.FieldType == typeof(string))
				.Select(x => new { Code = (string)x.GetRawConstantValue(), Description = standardUnitDescriptions[x.Name] })
				.ToList();

			var refPackTypeStandardUnitCodeAndDescriptionPairs = RefPackTypeCollection.GetStandardUnitsAsCodeDescriptionPairs();
			AssertContainsExactElementsInAnyOrder(
				"All CW1 standard units should be in shared and vice versa.",
				standardUnitCodeAndDescriptions,
				refPackTypeStandardUnitCodeAndDescriptionPairs.ToArray().Select(cdp => new { Code = cdp.Code, Description = cdp.Description }));
		}

		public void TestStandardUnitsAreValidatedInRefPackTypeCollection()
		{
			CombineAssertions("Listed units are not validated in RefPackTypeValidation:",
								() =>
								{
									var units = RefPackTypeValidation.StandardUnitListIncludingRatingUnits;
									foreach (ICodeDescription pair in RefPackTypeCollection.GetStandardUnitsAsCodeDescriptionPairs())
									{
										Assert(pair.Code, units.Contains(pair.Code));
									}
								});
		}

		public void TestContainsCode()
		{
			var collection = new RefPackTypeCollection(Factory);
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "ROB";
			packType.F3_Description = "ROBERT";
			AssertEquals(true, collection.ContainsCode("ROB"));
			AssertEquals(false, collection.ContainsCode("XXX"));
			AssertEquals(false, collection.ContainsCode(null));
			AssertEquals(false, collection.ContainsCode(string.Empty));
		}

		public void TestGetDescriptionFromCode()
		{
			var collection = new RefPackTypeCollection(Factory);
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "ROB";
			packType.F3_Description = "ROBERT";

			AssertEquals("This method should return a description of BAG based on the code entered", "ROBERT", collection.GetDescriptionFromCode("ROB"));
			AssertEquals("Should return empty string for a non existing code", string.Empty, collection.GetDescriptionFromCode("NON"));
			AssertEquals("Should return empty string for a non existing code", string.Empty, collection.GetDescriptionFromCode(null));
		}

		public void TestGetAsCodeDescriptionPair()
		{
			var collection = new RefPackTypeCollection(Factory);
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "ROB";
			packType.F3_Description = "ROBERT";

			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("ROB", "ROBERT");

			AssertEquals(pairList.GetDescriptionFromCode("ROB"), collection.GetAsCodeDescriptionPair().GetDescriptionFromCode("ROB"));
		}

		public void TestGetAsCodeDescriptionPairFull()
		{
			var collection = new RefPackTypeCollection(Factory);
			AssertEquals(false, collection.GetAsCodeDescriptionPair().ContainsCode(RefPackTypeCollection.ReservedContainerType));
			AssertEquals(true, collection.GetAsCodeDescriptionPairFull().ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}

		public void TestGetAsCodeDescriptionPairWithStandardUnits()
		{
			var collection = new RefPackTypeCollection(Factory);
			var pairList = new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair();

			string dummyStockUnit = "&ZZ";
			string dummyStockUnitDesc = "Dummy code added for test";
			AssertEquals("false - pre-condition test for dummy code", false, collection.ContainsCode(dummyStockUnit));

			pairList.AddPairIfNotExist(dummyStockUnit, dummyStockUnitDesc);
			AssertEquals(dummyStockUnitDesc, pairList.GetDescriptionFromCode(dummyStockUnit));
		}

		public void TestStockKeepingUnitListDoesNotDuplicateCodeAndKeepsRefPackCodeValueIfExist()
		{
			var collection = new RefPackTypeCollection(Factory);
			var pairList = new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair();
			AssertEquals("true - pre-condition test for KG", true, pairList.ContainsCode("BAG"));

			string duplicateStockUnit = "BAG";
			string duplicateStockUnitDesc = "Baggage";

			int listCount = pairList.Count;
			pairList.AddPairIfNotExist(duplicateStockUnit, duplicateStockUnitDesc);
			AssertEquals("pairList count should not have incremented", listCount, pairList.Count);
			AssertEquals("Description for 'BAG' should still be the description set up on RefPackCollection", pairList.GetDescriptionFromCode(duplicateStockUnitDesc), collection.GetAsCodeDescriptionPairWithStandardUnits().GetDescriptionFromCode(duplicateStockUnitDesc));
		}

		public void TestRefPackTypeCollectionAlphaOrder()
		{
			var collection = new RefPackTypeCollection(Factory);
			collection.AdditionalFilter = new ZQuery();

			var codesForLookup = collection.ToList<ICodeDescription>();

			Assert("Expected to have at least 1 RefPackTypeCode", codesForLookup.Count > 0);

			var codesAlphaOrdered = new List<ICodeDescription>(codesForLookup);
			codesAlphaOrdered.Sort((x, y) => x.Code.CompareTo(y.Code));

			var actualListForAssert = from x in codesForLookup
									  select x.Code;

			var expectedListForAssert = from x in codesAlphaOrdered
										select x.Code;

			AssertArrayEqualsByElements(expectedListForAssert.ToArray(), actualListForAssert.ToArray());
		}

		public void TestCodeDescriptionPairAlphaOrder()
		{
			var collection = new RefPackTypeCollection(Factory);

			var codesFromMethod = collection.GetAsCodeDescriptionPair().ToList<CodeDescriptionPair>();

			var codesAlphaOrdered = new List<CodeDescriptionPair>(codesFromMethod);
			codesAlphaOrdered.Sort((x, y) => x.Code.CompareTo(y.Code));

			AssertArrayEqualsByElements(codesAlphaOrdered.ToArray(), codesFromMethod.ToArray());
		}

		public void TestCodeDescriptionPairWithStandardUnitsAlphaOrder()
		{
			var collection = new RefPackTypeCollection(Factory);

			var codesFromMethod = collection.GetAsCodeDescriptionPairWithStandardUnits().ToList<CodeDescriptionPair>();

			var codesAlphaOrdered = new List<CodeDescriptionPair>(codesFromMethod);
			codesAlphaOrdered.Sort((x, y) => x.Code.CompareTo(y.Code));

			AssertArrayEqualsByElements(codesAlphaOrdered.ToArray(), codesFromMethod.ToArray());
		}

		protected override RefPackTypeCollection GetCollectionToTest()
		{
			return new RefPackTypeCollection(Factory);
		}
	}
}
