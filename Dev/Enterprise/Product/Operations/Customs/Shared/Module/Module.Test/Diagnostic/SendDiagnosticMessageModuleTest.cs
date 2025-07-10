using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class SendDiagnosticMessageModuleTest<T, U> : ZPopupModuleBasherTest
		where T : SendDiagnosticMessageModule<U>, new()
		where U : SendDiagnosticMessageController, new()
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.SendTestCustomsMessage, testModule.ID);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.SendTestCustomsMessage, testModule.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, testModule.LicenceCheckPoint);
		}

		public void TestShow()
		{
			using (IZForm form = testModule.ShowNew())
			{
				AssertNotNull(form);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			testModule = new T();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SendTestCustomsMessage;
		}

		protected override Type GetModuleToBashType()
		{
			return typeof(T);
		}

		protected override void TearDown()
		{
			if (testModule != null)
			{
				testModule.Dispose();
			}

			base.TearDown();
		}

		protected T testModule;
	}
}
