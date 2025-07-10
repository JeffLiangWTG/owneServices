using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSAAdditonalNumAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNumberTypes()
		{
			AssertNotNull(Lookups.NumberTypes);
		}

		#region Implementation

		USNHTSAAdditionalNumAddInfoLookups Lookups
		{
			get { return AdditionalNum.AddInfoLookups; }
		}

		NHTSAAdditionalNum AdditionalNum
		{
			get { return fAdditionalNum ?? (fAdditionalNum = Factory.New<NHTSAAdditionalNum>()); }
		}
		NHTSAAdditionalNum fAdditionalNum;

		#endregion
	}
}
