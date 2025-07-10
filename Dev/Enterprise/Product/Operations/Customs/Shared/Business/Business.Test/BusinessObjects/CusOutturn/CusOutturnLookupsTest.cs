using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class CusOutturnLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPurgePackageTypes()
		{
			AssertEquals("precondition", null, Lookups.fPackageTypes);
			AssertNotNull("precondition: cache PackageTypes list", Lookups.PackageTypes);
			AssertNotNull(Lookups.fPackageTypes);
			Lookups.PurgePackageTypes();
			AssertNull(Lookups.fPackageTypes);
		}

		public void TestCargoTypes()
		{
			AssertNotNull(Lookups.CargoTypes);
			AssertEquals(typeof(CodeDescriptionPairList), Lookups.CargoTypes.GetType());
		}

		public void TestPackageTypes()
		{
			AssertNotNull(Lookups.PackageTypes);
			AssertEquals(typeof(CodeDescriptionPairList), Lookups.PackageTypes.GetType());
		}

		public void TestOutturnResultTypeList()
		{
			AssertEquals("OutturnResultTypeList.Count", ExpectedOutturnResultTypeListCount, Lookups.OutturnResultTypeList.Count);
		}

		public void TestOutturnLineList()
		{
			DummyBizoWithUnderbondCollection dummy = Factory.New<DummyBizoWithUnderbondCollection>();
			CusUnderbondThatLinksToDummyBizo underbond = (CusUnderbondThatLinksToDummyBizo)dummy.Underbonds.AddNew(typeof(CusUnderbondThatLinksToDummyBizo));
			OutturnableDummy outturnableDummy = Factory.New<OutturnableDummy>();
			dummy.OutturnableLines = new IOutturnableLine[] { outturnableDummy };
			AssertEquals(1, new CusOutturnLookups(underbond.Outturns.AddNew(typeof(TestHelperCusOutturn))).OutturnLineList.Count);
		}

		#region Implementation

		CusOutturnLookups lookups;
		CusOutturnLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}

		protected virtual CusOutturnLookups GetNewLookups()
		{
			return new CusOutturnLookups(null);
		}

		protected virtual int ExpectedOutturnResultTypeListCount
		{
			get
			{
				return 0;
			}
		}

		#endregion
	}
}
