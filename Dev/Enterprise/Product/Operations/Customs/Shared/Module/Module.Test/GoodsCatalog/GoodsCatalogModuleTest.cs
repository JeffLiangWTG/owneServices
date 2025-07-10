using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GoodsCatalogModule))]
	public class GoodsCatalogModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestElementType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				var elementType = module.GetElementType();
				AssertEquals("Job must be of type BaseCusGoodsCatalog.", true, typeof(BaseCusGoodsCatalog).IsAssignableFrom(elementType));
			}
		}
		public void TestAllowEdit()
		{
			using (var module = new GoodsCatalogModule())
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new GoodsCatalogModule())
			{
				AssertEquals(true, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new GoodsCatalogModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new GoodsCatalogModule())
			{
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new GoodsCatalogModule())
			{
				AssertEquals(Env.Security.GoodsCatalog, module.SecurityCheckpoint);
			}
		}

		public void TestCheckpoints()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(Env.Licence.Broker, module.LicenceCheckPoint);
			}
		}

		public void TestWorkflowType()
		{
			using (var module = (ZFilterGridModule)GetModule())
			{
				AssertEquals(WorkflowDescriptors.CusGoodsCatalogWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		protected sealed override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.GoodsCatalog;
	}
}
