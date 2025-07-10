using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefUNLOCOForm))]
	sealed class RefUNLOCOFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefUNLOCOForm(Factory.New<RefUNLOCO>());
		}

		#region RL_IsUpdatable Handling

		[RequiresSTA]
		public void TestRL_IsUpdatableIsSetToFalseForNewUnloco()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			Assert("Precondition", unloco.RL_IsUpdatable);

			using (new RefUNLOCOForm(unloco))
			{
				Assert("RL_IsUpdatable should be set to false", !unloco.RL_IsUpdatable);
			}
		}

		public void TestRL_IsUpdatableHandling()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			Factory.Save();
			Assert("Precondition", unloco.RL_IsUpdatable);

			using (new RefUNLOCOForm(unloco))
			{
				Assert("RL_IsUpdatable remains true", unloco.RL_IsUpdatable);

				unloco.RL_PortName = "XXX";
				Assert("RL_IsUpdatable should be reset to false", !unloco.RL_IsUpdatable);

				unloco.RL_IsUpdatable = true;
				unloco.RL_PortName = "YYY";
				Assert("No more handling after first reset", unloco.RL_IsUpdatable);
			}
		}

		public void TestRL_IsUpdatableIsNotHandledIfItWasFalse()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_IsUpdatable = false;
			Factory.Save();
			Assert("Precondition", !unloco.RL_IsUpdatable);

			using (new RefUNLOCOForm(unloco))
			{
				Assert("RL_IsUpdatable remains false", !unloco.RL_IsUpdatable);

				unloco.RL_IsUpdatable = true;
				Assert("Precondition", unloco.RL_IsUpdatable);

				unloco.RL_PortName = "XXX";
				Assert("No handling should be done", unloco.RL_IsUpdatable);
			}
		}

		#endregion

		public void TestAuditPluginIsAdded()
		{
			using (var form = (RefUNLOCOForm)GetFormToBashCore())
			{
				AssertNotNull("RefUNLOCOForm should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
