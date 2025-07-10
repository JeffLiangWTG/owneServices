using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	sealed class ProtestLookupsTestCase : BusinessObjectLookupsTestCase
	{
		public void TestTariffActCitations()
		{
			AssertEquals(4, Lookups.TariffActCitations.Count);
		}

		public void TestPeriodBaseDateQualifiers()
		{
			AssertEquals(4, Lookups.ProtestPeriodBaseDateQualifiers.Count);
		}

		public void TestFurtherReviewAnswers()
		{
			AssertEquals(2, Lookups.FurtherReviewAnswers.Count);
		}

		public void TestProtestantTypes()
		{
			AssertEquals(7, Lookups.ProtestantTypes.Count);
		}

		public void TestProtestRefundCOPartyTypes()
		{
			AssertEquals(7, Lookups.ProtestRefundCOPartyTypes.Count);
		}

		public void TestOrganisations()
		{
			AssertNotNull(Lookups.Organisations);
		}

		public void TestSchDPortList()
		{
			AssertNotNull("SchDPortList", Lookups.SchDPortList);
			AssertEquals("SchDPortList type", typeof(ZZRefCusCodeListCombinedCollection), Lookups.SchDPortList.GetType());
		}

		public void TestAssociated514Protests()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			protest.US_P_Assoc514ProtestNo = "342890342";

			var filterDefault = protest.Lookups.Associated514Protests.FilterBusinessObjectDefaults[Protest.Schema.ProtestNumber + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertNotNull(filterDefault);
			AssertEquals("Filter Value", "342890342", filterDefault.Value);
		}

		public void TestAssociated520Protests()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			protest.US_P_Assoc520PetitionNo = "29103830";

			var filterDefault = protest.Lookups.Associated5204Protests.FilterBusinessObjectDefaults[Protest.Schema.ProtestNumber + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertNotNull(filterDefault);
			AssertEquals("Filter Value", "29103830", filterDefault.Value);
		}

		public void TestLeadProtests()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			protest.US_P_LeadProtestNo = "09103830";

			var coll = protest.Lookups.LeadProtests;
			var filterDefault = coll.FilterBusinessObjectDefaults[Protest.Schema.ProtestNumber + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"];
			AssertNotNull(filterDefault);
			AssertEquals("Filter Value", "09103830", filterDefault.Value);
		}

		ProtestLookups lookups;
		ProtestLookups Lookups => lookups ?? (lookups = new ProtestLookups(new Protest(Factory.New<JobDeclaration>())));
	}
}
