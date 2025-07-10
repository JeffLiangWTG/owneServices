using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ProcessQueuePlugInTest : TestCaseWithFactory
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, PlugIn.LicenceCheckPoint);
		}

		public void TestPlugInName()
		{
			AssertEquals("Process Queue", PlugIn.Name);
		}

		public void TestHasUserControls()
		{
			Assert(PlugIn.HasUserControl);
		}

		public void TestGetNewUserControl()
		{
			using (Control control = PlugIn.GetNewUserControl())
			{
				AssertEquals("Wrong type", typeof(ProcessQueueUserControl), control.GetType());
				AssertEquals("Wrong DockStyle", DockStyle.Fill, control.Dock);
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			PlugIn.Dispose();
			base.TearDown();
		}

		ProcessQueuePlugInForTest PlugIn
		{
			get
			{
				if (fPlugIn == null)
				{
					fPlugIn = new ProcessQueuePlugInForTest(ProcessQueueParent);
				}
				return fPlugIn;
			}
		}

		ProcessQueueParentForTest ProcessQueueParent
		{
			get
			{
				if (fProcessQueueParent == null)
				{
					fProcessQueueParent = new ProcessQueueParentForTest(Factory);
				}
				return fProcessQueueParent;
			}
		}

		ProcessQueuePlugInForTest fPlugIn;
		ProcessQueueParentForTest fProcessQueueParent;

		#region ProcessQueuePlugInForTest

		class ProcessQueuePlugInForTest : ProcessQueuePlugIn
		{
			public ProcessQueuePlugInForTest(IProcessQueueParent hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			public new LicenceCheckpoint LicenceCheckPoint
			{
				get { return base.LicenceCheckPoint; }
			}

			public new bool HasUserControl
			{
				get { return base.HasUserControl; }
			}

			public new Control GetNewUserControl()
			{
				return base.GetNewUserControl();
			}
		}

		#endregion

		#endregion
	}
}
