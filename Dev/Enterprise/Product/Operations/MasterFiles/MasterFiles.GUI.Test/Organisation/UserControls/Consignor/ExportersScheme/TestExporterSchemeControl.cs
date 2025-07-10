using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class TestExporterSchemeControl : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestControlIsShown()
		{
			using (ZForm testForm = new ZForm())
			{
				using (ExporterSchemeControl testControl = ControlForTesting)
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
				}
			}
		}

		#region Control For Testing

		protected virtual ExporterSchemeControl GetControlToTestCore()
		{
			return new ExporterSchemeControl();
		}

		Control FindControlByName(Control outerControl, string name)
		{
			Control result = null;
			foreach (Control ctrl in outerControl.Controls)
			{
				if (ctrl.Name == name)
				{
					result = ctrl;
					break;
				}
				result = FindControlByName(ctrl, name);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		ExporterSchemeControl ControlForTesting
		{
			get
			{
				if (controlForTesting == null)
				{
					controlForTesting = GetControlToTestCore();
				}
				return controlForTesting;
			}
		}
		ExporterSchemeControl controlForTesting;

		protected override void TearDown()
		{
			controlForTesting.Dispose();

			base.TearDown();
		}

		#endregion
	}
}
