using CargoWise.EntityFramework;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	sealed class ConsolidatedDeclarationModuleBaseOnlyTest : TestCase
	{
		public void TestAllowEdit()
		{
			using (var module = new ConsolidateDeclarationTestedModule())
			{
				AssertEquals(true, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new ConsolidateDeclarationTestedModule())
			{
				AssertEquals(true, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new ConsolidateDeclarationTestedModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new ConsolidateDeclarationTestedModule())
			{
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new ConsolidateDeclarationTestedModule())
			{
				AssertEquals(Env.Security.CustomsConsolidatedDeclaration, module.SecurityCheckpoint);
			}
		}
	}

	class ConsolidateDeclarationTestedModule : ConsolidatedDeclarationModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection() => null;
	}
}
