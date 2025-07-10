using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Transhipment.Module.Test
{
	internal sealed class CusInBondHeaderFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImporterList()
		{
			ConsigneeCollection collection = Lookups.ImporterList;
			AssertNotNull(collection);
		}

		#region Implementation
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
		#endregion
	}
}
