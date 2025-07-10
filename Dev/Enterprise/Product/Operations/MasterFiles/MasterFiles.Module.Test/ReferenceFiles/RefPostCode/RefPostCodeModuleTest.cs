using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPostCodeModule))]
	class RefPostCodeModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.PostCode, Module.SecurityCheckpoint);
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefPostCode;
		}

		protected virtual RefPostCodeModule Module
		{
			get
			{
				if (module == null)
				{
					module = (RefPostCodeModule)ZModuleFactory.Instance.Create(ModuleIDs.RefPostCode);
				}
				return module;
			}
		}
		RefPostCodeModule module;

		protected override void TearDown()
		{
			base.TearDown();
			if (module != null)
			{
				module.Dispose();
			}
		}

		#endregion

		#region GetIsSystemDefinedDefaultProperty

		protected override string GetIsSystemDefinedDefaultProperty()
		{
			return "All";
		}

		#endregion

	}
}
