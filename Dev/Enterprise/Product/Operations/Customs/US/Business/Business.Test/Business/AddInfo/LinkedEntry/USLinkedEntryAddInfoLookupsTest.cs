using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USLinkedEntryAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSchDPortList()
		{
			AssertNotNull("SchDPortList", Lookups.SchDPortList);
			AssertEquals("SchDPortList type", typeof(ZZRefCusCodeListCombinedCollection), Lookups.SchDPortList.GetType());
		}

		public void TestEntries()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "Z1Z2Z3Z4";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "Z4Z3Z2Z1";
			var protest = new Protest.Protest(Factory.New<JobDeclaration>());
			protest.Protestant.OrganisationPK = importer1.PK;
			var linkedEntry = protest.LinkedEntries.AddNew();
			var entries = linkedEntry.AddInfoLookups.Entries;
			AssertEquals(importer1.PK, entries.FilterBusinessObjectDefaults["Importer of Record:Property"].Value);
			AssertEquals("", entries.FilterBusinessObjectDefaults["Import Date:Property1"].Value.ToString());
			var linkedEntry2 = protest.LinkedEntries.AddNew();
			linkedEntry2.US_LE_EntryNumber = "XJ512";
			entries = linkedEntry2.AddInfoLookups.Entries;
			AssertEquals("12", entries.FilterBusinessObjectDefaults["Entry Number (ENS):Property"].Value);
		}

		USLinkedEntryAddInfoLookups lookups;
		USLinkedEntryAddInfoLookups Lookups => lookups ?? (lookups = new USLinkedEntryAddInfoLookups(LinkedEntryAddInfo));

		USLinkedEntryAddInfo linkedEntryAddInfo;
		USLinkedEntryAddInfo LinkedEntryAddInfo => linkedEntryAddInfo ?? (linkedEntryAddInfo = new Protest.Protest(Factory.New<JobDeclaration>()).LinkedEntries.AddNew().Data);
	}
}
