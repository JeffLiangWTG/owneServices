using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CompletionTriggerActionModule))]
	sealed class CompletionTriggerActionModuleTest : ZModuleBasherTest
	{
		public void TestAllowNew_ShouldBeFalse()
		{
			using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowView_ShouldBeFalse()
		{
			using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
			{
				AssertEquals(false, module.AllowView);
			}
		}

		public void TestAllowEdit_ShouldBeFalse()
		{
			using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
			{
				AssertEquals(false, module.AllowEdit);
			}
		}

		public void TestAllowDelete_ShouldBeFalse()
		{
			using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
			{
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestShowRecentItems()
		{
			using (var module = (ZFilterGridModule)ZFilterModule.GetZFilterModule(GetModuleID()))
			{
				AssertEquals(false, module.ShowRecentItems);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CompletionTriggerAction;
		}
	}
}
