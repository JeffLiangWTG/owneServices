using System.Windows.Forms;
using Enterprise.Packing.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MessageBoxForPackingTest : ZMessageBoxTest
	{
		#region TestSupressKeys

		public void TestSupressKeys()
		{
			var preAmble = Keys.Control | Keys.L;
			var legacyPreAmble = Keys.Control | Keys.OemCloseBrackets;

			using (var messageBoxWhenScanning = new MessageBoxForPacking("Test-Message", "Test-Caption", MessageBoxIcon.Warning))
			{
				var keyDownHandled = false;
				AssertEquals("Caption = \"Test-Caption\", Message = \"Test-Message\"", UserEventDiagnosticReferenceAttribute.Render(messageBoxWhenScanning));
				messageBoxWhenScanning.KeyDown += (s, e) => { keyDownHandled = e.Handled; };
				messageBoxWhenScanning.Show();
				SendKeys(messageBoxWhenScanning, preAmble, Keys.O, Keys.B, preAmble);
				AssertEquals("Key down should be handled.", true, keyDownHandled);

				keyDownHandled = false;
				SendKeys(messageBoxWhenScanning, preAmble, Keys.O, Keys.B, preAmble, Keys.O);
				AssertEquals("Key down should not be handled.", false, keyDownHandled);
			}

			using (var messageBoxWhenScanningLegacy = new MessageBoxForPacking("Test-Message", "Test-Caption", MessageBoxIcon.Warning))
			{
				var keyDownHandled = false;
				messageBoxWhenScanningLegacy.KeyDown += (s, e) => { keyDownHandled = e.Handled; };
				messageBoxWhenScanningLegacy.Show();
				SendKeys(messageBoxWhenScanningLegacy, legacyPreAmble, Keys.O, Keys.B, legacyPreAmble);
				AssertEquals("Key down should be handled.", true, keyDownHandled);
			}

			using (var messageBoxWhenNotScanning = new MessageBoxForPacking("Test-Message", "Test-Caption", MessageBoxIcon.Warning))
			{
				var keyDownHandled = false;
				messageBoxWhenNotScanning.KeyDown += (s, e) => { keyDownHandled = e.Handled; };
				messageBoxWhenNotScanning.Show();
				SendKeys(messageBoxWhenNotScanning, Keys.O);
				AssertEquals("Key down should not be handled.", false, keyDownHandled);
			}
		}

		void SendKeys(Control control, params Keys[] keys)
		{
			foreach (var key in keys)
			{
				KeySender.SendKeyDown(control, control.Handle, key);
			}
		}

		#endregion
	}
}
