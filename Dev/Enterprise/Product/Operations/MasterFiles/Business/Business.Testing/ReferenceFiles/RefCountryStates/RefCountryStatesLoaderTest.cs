using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryStates.Loader))]
	sealed class RefCountryStatesLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCountryStates.Loader(Factory);
		}

		#region Loader

		public void TestGetRefCountryStatesFromCodeOrDesc()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			AssertNotNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCodeOrDesc("KNZ", "MX"));
			AssertNotNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCodeOrDesc("KNZTEST", "MX"));
			AssertNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCodeOrDesc("KNZ", "CA"));
			AssertNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCodeOrDesc("KNZTEST", "CA"));
		}

		public void TestGetRefCountryStatesFromCode()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			AssertNotNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode("KNZ", "MX"));
			AssertNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode("KNZTEST", "MX"));
			AssertNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode("KNZ", "CA"));
			AssertNull(new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode("KNZTEST", "CA"));
		}

		#endregion
	}
}
