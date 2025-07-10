using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public sealed class ZZRefCusCodeListWrapperModuleTestBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCanReloadWithFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CUSOF", "Customs Office");
			var cusCodeList = helper.CreateCusCodeList("ZZ", "CUSOF", "1111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var cusCodeListCombined = Factory.Load<ZZRefCusCodeListCombined>(cusCodeList.PK);
			var wrapper = new ZZRefCusCodeListWrapper(cusCodeListCombined);
			using (var testModule = new ZZRefCusCodeListWrapperModuleForTest())
			{
				var query = new ZQuery(ZZRefCusCodeListCombinedSchema.PK, cusCodeList.PK);
				AssertEquals(true, testModule.CanReloadWithFilter_Exposed(Factory, wrapper, query));
			}
		}

		class ZZRefCusCodeListWrapperModuleForTest : ZZRefCusCodeListWrapperModule
		{
			public bool CanReloadWithFilter_Exposed(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter) => CanReloadWithFilter(newFactory, selectedBusinessObject, filter);
			public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;
			protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
			protected override FilterBusinessObject GetNewFilterBusinessObject() => new ZZRefCusCodeListWrapperFilterStripBusinessObject();
			public override ModuleIdentifier ID => throw new System.NotImplementedException();
			protected override IBusinessObjectCollection GetNewGridCollection() => throw new System.NotImplementedException();
		}
	}
}
