using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusUnderbondLookups))]
	public abstract class CusUnderbondLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModeOfTransportList()
		{
			AssertNotNull(Lookups.ModeOfTransportList);
			AssertEquals("ExpectedModeOfTransportListCount", ExpectedModeOfTransportListCount, Lookups.ModeOfTransportList.Count);
		}

		public void TestUnderbondForList()
		{
			DummyBizoWithUnderbondCollection dummy = Factory.New<DummyBizoWithUnderbondCollection>();
			CusUnderbondThatLinksToDummyBizo underbond = Factory.New<CusUnderbondThatLinksToDummyBizo>();
			CusUnderbondLookups lookups = new CusUnderbondLookups(underbond);
			underbond.AddCollectionProvider(dummy);
			AssertEquals("UnderbondForList.Count", 1, lookups.UnderbondForList.Count);
			AssertEquals("UnderbondForList[0]", "Dummy Underbond Biz Obj", lookups.UnderbondForList[0].Code);
			AssertEquals("UnderbondForList[0]", "Dummy Underbond Biz Obj", lookups.UnderbondForList[0].Description);
		}

		public void TestRequestReasonList()
		{
			AssertNotNull(Lookups.RequestReasonList);
			AssertEquals("ExpectedRequestReasonListCount", ExpectedRequestReasonListCount, Lookups.RequestReasonList.Count);
		}

		public void TestUnderbondStatusList()
		{
			AssertEquals("ExpectedUnderbondStatusListCount", ExpectedUnderbondStatusListCount, Lookups.UnderbondStatusList.Count);
		}

		public void TestOutturnStatusList()
		{
			AssertEquals("ExpectedUnderbondStatusListCount", ExpectedOutturnStatusListCount, Lookups.OutturnStatusList.Count);
		}

		public void TestOrgHeaderCollection()
		{
			AssertEquals("CollectionType", typeof(AirCTOCollection), Lookups.AirCTOs.GetType());
		}

		public void TestCustomsControlledPremisesList()
		{
			AssertEquals("CollectionType", typeof(CTOOrDepotOrWarehouseCollection), Lookups.CustomsControlledPremisesList.GetType());
		}

		#region Implementation

		protected virtual int ExpectedModeOfTransportListCount
		{
			get
			{
				return 0;
			}
		}

		protected virtual int ExpectedRequestReasonListCount
		{
			get
			{
				return 0;
			}
		}

		protected virtual int ExpectedUnderbondStatusListCount
		{
			get { return 0; }
		}

		protected virtual int ExpectedOutturnStatusListCount
		{
			get { return 0; }
		}

		protected CusUnderbondLookups Lookups => Underbond.Lookups;

		protected abstract CusUnderbond CreateNewUnderbond();

		protected CusUnderbond Underbond => underbond ?? (underbond = CreateNewUnderbond());
		CusUnderbond underbond;

		#endregion
	}
}
