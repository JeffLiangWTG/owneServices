using System.Windows.Forms;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PrintFinalMasterActionMethod))]
	public class PrintFinalMasterActionMethodTest : OperationalActionMethodTest<PrintFinalMasterActionMethod>
	{
		public void TestOverrides()
		{
			AssertEquals("Print Final Master", Method.Name);
			AssertEquals("Allows for the printing and/or electronic sending of Air Waybills", Method.Description);
			AssertEquals(typeof(PrintFinalMasterMethodApplicator), Method.NewApplicator(Factory, new AWBPrintSettings()).GetType());
			Assert(Method.HasSettings);
			AssertEquals(typeof(AWBPrintSettings), Method.NewSetting(Factory).GetType());
			using (Control control = (Control)Method.NewSettingsControl())
			{
				AssertEquals(typeof(AWBPrintSettingsControl), control.GetType());
			}
		}

		#region Implementation

		protected override PrintFinalMasterActionMethod NewMethod()
		{
			return new PrintFinalMasterActionMethod();
		}

		#endregion
	}
}
