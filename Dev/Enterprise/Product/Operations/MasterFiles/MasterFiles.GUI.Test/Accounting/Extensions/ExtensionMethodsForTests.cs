using System.Globalization;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public static class ExtensionMethodsForTests
	{
		public static ControlType GetControl<ControlType>(this Control controlContainer, string controlName, bool searchAllChildren = true)
			where ControlType : Control
		{
			var controls = controlContainer.Controls.Find(controlName, searchAllChildren);
			Assertion.AssertEquals(string.Format(CultureInfo.CurrentCulture, "Amount of {0} found.", controlName), 1, controls.Length);

			var control = controls[0];
			Assertion.AssertType<ControlType>(control);

			return (ControlType)control;
		}
	}
}
