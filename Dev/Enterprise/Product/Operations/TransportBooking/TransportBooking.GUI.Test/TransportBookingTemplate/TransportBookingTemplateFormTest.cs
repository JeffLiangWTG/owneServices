using System.Windows.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Testing
{
	[TestedType(typeof(TransportBookingTemplateForm))]
	public class TransportBookingTemplateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var template = Factory.New<DtbBookingTmpl>();
			return new TransportBookingTemplateForm(template);
		}

		public void TestFileDeleteMenuItem_HasMeaningfulText()
		{
			using (var form = GetFormToBashCore())
			{
				var menuItem = form.Menu.MenuItems.FindByText(ZFormMenuStrategy.GetFileDeleteMenuItemText(form), true);
				AssertEquals("FileDeleteMenuItem has meaningful text", "Deactivate", menuItem.Text);
			}
		}
	}
}
