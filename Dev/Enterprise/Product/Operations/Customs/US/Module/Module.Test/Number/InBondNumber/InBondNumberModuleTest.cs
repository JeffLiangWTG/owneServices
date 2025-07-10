using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(InBondNumberModule))]
	sealed class InBondNumberModuleTest : NumberModuleTest
	{
		public void TestSecurityCheckPoint()
		{
			using (var module = new InBondNumberModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.InBondNumber, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.US.InBondNumber;

		protected override Type ExpectedControllerType => typeof(InBondNumberController);
	}
}
