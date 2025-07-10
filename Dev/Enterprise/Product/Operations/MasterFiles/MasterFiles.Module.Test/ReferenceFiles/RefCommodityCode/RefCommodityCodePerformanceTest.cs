using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefCommodityCodePerformanceTest : ZModulePerformanceTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCommodityCode;
		}

		protected override int MaximumDBHitsForPerformSearch
		{
			get { return 2; }
		}

		protected override void PrepareModuleForPerformanceTest(ZFilterGridModule module)
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.Ukrainian;
			BusinessObjectFactory factory = new BusinessObjectFactory();
			RefCommodityCode[] codes = factory.Load<RefCommodityCode>(new ZQuery());

			for (int i = 0; i < 100; i++)
			{
				RefCommodityCode code = factory.NewWithValidTestData<RefCommodityCode>();
				code.RH_Description = "Some description " + i;
			}

			factory.Save();
		}
	}
}
