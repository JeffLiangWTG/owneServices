using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefCusTradeGroupModule))]
	sealed class RefCusTradeGroupModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.Universal.RefCusTradeGroup;

		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			InitObjectsBeforeModuleShowsAndCanSearch();
			base.TestModuleShowsAndCanSearch();
		}

		public void TestBoundCollection()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertType<RefCusTradeGroupCollection>(module.GetNewBusinessObjectCollection());
			}
		}

		public void TestAttributes()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				CombineAssertions(() =>
				{
					AssertEquals("AllowNew", false, module.AllowNew);
					AssertEquals("AllowDelete", false, module.AllowDelete);
					AssertEquals("AllowEdit", false, module.AllowEdit);
					AssertEquals("AllowEdit", false, module.AllowView);
				});
			}
		}

		void InitObjectsBeforeModuleShowsAndCanSearch()
		{
			var eun = Factory.NewWithValidTestData<RefDataGrouping>();
			eun.ZZZ_DataGrouping = "TGR";
			eun.ZZZ_Description = "Test group";
			var refCusTradeGroup = Factory.NewWithValidTestData<RefCusTradeGroup>();
			refCusTradeGroup.ZZA_ZZZ_NKDataGrouping = "TGR";
			Factory.Save();
		}
	}
}
