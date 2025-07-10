using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class NumbersUserControlTest : TestCase
	{
		public void TestNumbersUserControl()
		{
			using (NumbersUserControl control = new NumbersUserControl())
			{
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZGrid), "NumbersGrid"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(CustomsReferenceDropEdit), "DropEditType"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZTextBox), "TextBoxNumber"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZTextBox), "TextBoxInfo"));
				Assert(control.ControlHasChildOrIsTypeNamed(typeof(ZDateEdit), "DateEditIssued"));
			}
		}
	}

	static class ControlExtensions
	{
		public static bool ControlHasChildOrIsType(this Control control, Type type)
		{
			bool result = (control.GetType() == type);
			foreach (Control c in control.Controls)
			{
				result = result || ControlHasChildOrIsType(c, type);
			}
			return result;
		}

		public static bool ControlHasChildOrIsTypeNamed(this Control control, Type type, string name)
		{
			bool result = ((control.GetType() == type) && (control.Name == name));
			foreach (Control c in control.Controls)
			{
				result = result || ControlHasChildOrIsTypeNamed(c, type, name);
			}
			return result;
		}
	}
}
