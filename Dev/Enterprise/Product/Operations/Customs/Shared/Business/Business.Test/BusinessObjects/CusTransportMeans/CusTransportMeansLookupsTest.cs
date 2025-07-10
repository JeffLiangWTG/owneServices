using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusTransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportStateList_MatchesDBConstraint()
		{
			AssertEquals("Codes as string", "DEC, DIF, MIS, NEW", lookups.TransportStateList.CodesAsString);
		}

		public void TestTransportStateList_ValuesValidForDBConstraint()
		{
			cusTransportMeans.TPM_ParentTableCode = "BM";
			foreach (var code in lookups.TransportStateList.GetAllCodes())
			{
				cusTransportMeans.TPM_TransportState = code;
				AssertNoExceptionThrown($"The database constraint for 'TPM_TransportState' does not allow the value '{code}'. Consider updating the database constraint or revising the valid values in the lookup list.", Factory.Save);
			}
		}

		public void TestTransportStateList_Cached()
		{
			var list = lookups.TransportStateList;
			AssertSame(list, lookups.TransportStateList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusTransportMeans = Factory.New<CusTransportMeans>();
			lookups = new CusTransportMeansLookups(cusTransportMeans);
		}
		CusTransportMeans cusTransportMeans;
		CusTransportMeansLookups lookups;
	}
}
