using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class CustomFieldsBookingHostUserControlTest : TestCase
	{
		public void Test_NothingSetupMessageIsSet()
		{
			using (var ctrl = new CustomFieldsBookingHostUserControl())
			{
				var nothingSetupLabel = GetAllNestedControls(ctrl).OfType<CustomPropertiesControl>().First();
				Assertion.AssertEquals(nothingSetupLabel.Controls[0].Text, "To make use of this tab, please setup Transport Booking custom fields in Workflow Manager.");
			}
		}

		static IEnumerable<Control> GetAllNestedControls(Control root)
		{
			var stack = new Stack<Control>();
			stack.Push(root);

			do
			{
				var control = stack.Pop();

				foreach (Control child in control.Controls)
				{
					yield return child;
					stack.Push(child);
				}
			}
			while (stack.Count > 0);
		}
	}
}
