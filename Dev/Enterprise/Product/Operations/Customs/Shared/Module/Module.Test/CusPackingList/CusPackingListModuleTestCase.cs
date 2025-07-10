using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusPackingListModule))]
	sealed class CusPackingListModuleTestCase : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestCusPackingListModuleButtons()
		{
			using (var module = new CusPackingListModule())
			{
				Assert("Delete button should be hidden.", !module.AllowDelete);
				Assert("New button should be hidden.", !module.AllowNew);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CusPackingList;
	}
}
