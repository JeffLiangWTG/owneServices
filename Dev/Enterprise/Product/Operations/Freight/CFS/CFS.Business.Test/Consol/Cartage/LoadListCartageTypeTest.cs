using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public abstract class LoadListCartageTypeTest : TestCaseWithFactory
	{
		#region TestOriginScheduleDates

		public void TestOriginScheduleDates()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = sailingHelper.SydLaxSailing.PK;

			var ct = new LoadListPickupCartageType(loadList);
			Assert(!ct.FCLReceivalCommences.IsEmpty);
			AssertEquals(loadList.JK_CTOReceivalCommences, ct.FCLReceivalCommences);

			Assert(!ct.FCLCutOff.IsEmpty);
			AssertEquals(loadList.JK_CTOCutOff, ct.FCLCutOff);

			Assert(!ct.LCLReceivalCommences.IsEmpty);
			AssertEquals(loadList.JK_DepotReceivalCommences, ct.LCLReceivalCommences);

			Assert(!ct.LCLCutOff.IsEmpty);
			AssertEquals(loadList.JK_DepotCutOff, ct.LCLCutOff);
		}

		#endregion

		#region TestDestinationScheduleDates

		public void TestDestinationScheduleDates()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = sailingHelper.SydLaxSailing.PK;

			var ct = GetLoadListCartageType(loadList);
			Assert(!ct.FCLAvailabilityDate.IsEmpty);
			AssertEquals(loadList.JK_CTOAvailabilityDate, ct.FCLAvailabilityDate);

			Assert(!ct.FCLStorageDate.IsEmpty);
			AssertEquals(loadList.JK_CTOStorageDate, ct.FCLStorageDate);

			Assert(!ct.LCLAvailabilityDate.IsEmpty);
			AssertEquals(loadList.JK_DepotAvailabilityDate, ct.LCLAvailabilityDate);

			Assert(!ct.LCLStorageDate.IsEmpty);
			AssertEquals(loadList.JK_DepotStorageDate, ct.LCLStorageDate);
		}

		#endregion

		protected abstract LoadListCartageType GetLoadListCartageType(CFSLoadListConsol loadList);
	}
}
