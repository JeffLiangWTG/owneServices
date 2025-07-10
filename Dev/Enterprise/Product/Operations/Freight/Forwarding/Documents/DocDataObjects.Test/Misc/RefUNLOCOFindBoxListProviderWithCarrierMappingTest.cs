using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class RefUNLOCOFindBoxListProviderWithCarrierMappingTest : TestCaseWithFactory
	{
		public void TestAutoCompleteOnCommit()
		{
			Assert(refUNLOCOFindBoxListProviderMapping.AutoCompleteOnCommit);
		}

		public void TestNearestMatch()
		{
			AssertEquals("Match the whole local code and return foreign code if founded when not explicit autocomplete", "(CNX1X, False)", refUNLOCOFindBoxListProviderMapping.NearestMatch("CNX1X", false, -1).ToString());
			AssertEquals("Match the whole local code and return foreign code if founded when explicit autocomplete", "(CNX1X, True)", refUNLOCOFindBoxListProviderMapping.NearestMatch("CNX1X", true, -1).ToString());

			AssertEquals("Not autoCompleted to the UNLOCO when not explicit autocomplete", "(CNX1, False)", refUNLOCOFindBoxListProviderMapping.NearestMatch("CNX1", false, -1).ToString());
			AssertEquals("AutoCompleted to the UNLOCO when explicit autocomplete", "(CNX1X, True)", refUNLOCOFindBoxListProviderMapping.NearestMatch("CNX1", true, -1).ToString());
		}

		public void TestNearestMatchCore()
		{
			AssertEquals("Match the whole local code and return foreign code if founded when not explicit autocomplete", "(CNX1X, False)", refUNLOCOFindBoxListProviderMapping.NearestMatchCore("CNX1X", false).ToString());
			AssertEquals("Match the whole local code and return foreign code if founded when explicit autocomplete", "(CNX1X, True)", refUNLOCOFindBoxListProviderMapping.NearestMatchCore("CNX1X", true).ToString());

			AssertEquals("Not autoCompleted to the UNLOCO when not explicit autocomplete", "(CNX1, False)", refUNLOCOFindBoxListProviderMapping.NearestMatchCore("CNX1", false).ToString());
			AssertEquals("AutoCompleted to the UNLOCO when explicit autocomplete", "(CNX1X, True)", refUNLOCOFindBoxListProviderMapping.NearestMatchCore("CNX1", true).ToString());
		}

		public void TestDescriptionFromCode()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertEquals("Return description from local UNLOCO", sydneyUNLOCO.Description, refUNLOCOFindBoxListProviderMapping.DescriptionFromCode("AUSYD"));
			AssertEquals("Return description from foreign UNLOCO", sydneyUNLOCO.Description, refUNLOCOFindBoxListProviderMapping.DescriptionFromCode("AUY1Y"));
		}

		public void TestDescriptionFromPrimaryKey()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertEquals("Return description from UNLOCO PK", sydneyUNLOCO.Description, refUNLOCOFindBoxListProviderMapping.DescriptionFromPrimaryKey(sydneyUNLOCO.PK));
		}

		public void TestPrimaryKeyFromCode()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertEquals("Return PK from local UNLOCO", sydneyUNLOCO.PK, refUNLOCOFindBoxListProviderMapping.PrimaryKeyFromCode("AUSYD"));
			AssertEquals("Return PK from foreign UNLOCO", sydneyUNLOCO.PK, refUNLOCOFindBoxListProviderMapping.PrimaryKeyFromCode("AUY1Y"));
		}

		public void TestGetBusinessObjectFromCode()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertEquals("Return UNLOCO from local UNLOCO", sydneyUNLOCO.PK, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectFromCode("AUSYD").PK);
			AssertEquals("Return UNLOCO from foreign UNLOCO", sydneyUNLOCO.PK, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectFromCode("AUY1Y").PK);
		}

		public void TestGetBusinessObjectsFromCode()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertContainsExactElementsInAnyOrder("Return UNLOCO from local UNLOCO", new[] { sydneyUNLOCO.PK }, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectsFromCode("AUSYD").ToList().Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder("Return UNLOCO from foreign UNLOCO", new[] { sydneyUNLOCO.PK }, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectsFromCode("AUY1Y").ToList().Select(x => x.PK));
		}

		public void TestGetBusinessObjectFromCodeWithoutFilter()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertEquals("Return UNLOCO from local UNLOCO", sydneyUNLOCO.PK, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectFromCodeWithoutFilter("AUSYD").PK);
			AssertEquals("Return UNLOCO from foreign UNLOCO", sydneyUNLOCO.PK, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectFromCodeWithoutFilter("AUY1Y").PK);
		}

		public void TestGetBusinessObjectsFromCodeWithoutFilter()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertContainsExactElementsInAnyOrder("Return UNLOCO from local UNLOCO", new[] { sydneyUNLOCO.PK }, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectsFromCodeWithoutFilter("AUSYD").ToList().Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder("Return UNLOCO from foreign UNLOCO", new[] { sydneyUNLOCO.PK }, refUNLOCOFindBoxListProviderMapping.GetBusinessObjectsFromCodeWithoutFilter("AUY1Y").ToList().Select(x => x.PK));
		}

		public void TestGetCustomCodeDescription()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "AUSYD";
			dummy.Z0_Description = sydneyUNLOCO.Description;

			AssertEquals("Return foreign UNLOCO", "AUY1Y", refUNLOCOFindBoxListProviderMapping.GetCustomCodeDescription(dummy).Code);
			AssertEquals("Return foreign UNLOCO", sydneyUNLOCO.Description, refUNLOCOFindBoxListProviderMapping.GetCustomCodeDescription(dummy).Description);

			dummy.Z0_Code = "AUY1Y";

			AssertEquals("Return foreign UNLOCO", "AUY1Y", refUNLOCOFindBoxListProviderMapping.GetCustomCodeDescription(dummy).Code);
			AssertEquals("Return foreign UNLOCO", sydneyUNLOCO.Description, refUNLOCOFindBoxListProviderMapping.GetCustomCodeDescription(dummy).Description);
		}

		public void TestCodeFromPrimaryKey()
		{
			var sydneyUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			AssertEquals("Return foreign code from UNLOCO PK", "AUY1Y", refUNLOCOFindBoxListProviderMapping.CodeFromPrimaryKey(sydneyUNLOCO.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var carrier = CreateOrgHeaderWithUNLOCOMappings();
			refUNLOCOCollection = new RefUNLOCOCollectionForTesting(Factory);

			var carrierUNLOCOMapping = new CarrierUNLOCOMapping(Factory, carrier.PK);
			refUNLOCOFindBoxListProviderMapping = new RefUNLOCOFindBoxListProviderWithCarrierMapping(refUNLOCOCollection, carrierUNLOCOMapping, refUNLOCOCollection.FindBoxListProvider);
		}

		OrgHeader CreateOrgHeaderWithUNLOCOMappings()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mapping1 = orgHeader.CreatePatternMatchOverrideForTest();
			SetupOrgPatternMatchOverride(mapping1, "CNX1X", "CNSHA");

			var mapping2 = orgHeader.CreatePatternMatchOverrideForTest();
			SetupOrgPatternMatchOverride(mapping2, "AUY1Y", "AUSYD");

			var mapping3 = orgHeader.CreatePatternMatchOverrideForTest();
			SetupOrgPatternMatchOverride(mapping3, "DEZ1Z", "DEHAM");

			return orgHeader;
		}

		void SetupOrgPatternMatchOverride(OrgPatternMatchOverride mapping, string foreignUNLOCO, string localUNLOCO)
		{
			mapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping.OO_ForeignCode = foreignUNLOCO;
			mapping.OO_LocalCode = localUNLOCO;
		}

		RefUNLOCOFindBoxListProviderWithCarrierMapping refUNLOCOFindBoxListProviderMapping;
		RefUNLOCOCollectionForTesting refUNLOCOCollection;
	}

	sealed class RefUNLOCOCollectionForTesting : RefUNLOCOCollection
	{
		public RefUNLOCOCollectionForTesting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IFindBoxListProvider FindBoxListProvider => base.FindBoxListProvider;
	}
}
