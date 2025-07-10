using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	sealed class CusInBondHeaderFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImporterList()
		{
			ConsigneeCollection collection = Lookups.ImporterList;
			AssertNotNull(collection);
		}

		public void TestInbondCommonTypeList()
		{
			InbondCommonTypeList typeList = Lookups.InbondCommonTypeList;
			AssertEquals("Number of types in this list", 3, typeList.Count);
		}

		public void TestTransportModeList()
		{
			TransportModeCodes transportModeList = Lookups.TransportModeList;
			AssertEquals("Number of transport modes in this list", 15, transportModeList.Count);
		}

		public void TestBranchList()
		{
			var branchCollection = Lookups.BranchList;
			AssertNotNull(branchCollection);
		}

		CusInBondHeaderFilterStripBusinessObject Filter
		{
			get
			{
				if (filter == null)
				{
					filter = new CusInBondHeaderFilterStripBusinessObject();
				}

				return filter;
			}
		}
		CusInBondHeaderFilterStripBusinessObject filter;

		CusInBondHeaderFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new CusInBondHeaderFilterLookups(Filter);
				}

				return lookups;
			}
		}
		CusInBondHeaderFilterLookups lookups;
	}
}
