using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVCancelableMinimisableProgressForm))]
	public class HVLVCancelableMinimisableProgressFormTest : ZFormBasherTest
	{
		public void TestMinimiseParentFormWhenMinimised()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				using (var form = new HVLVCancelableMinimisableProgressForm(parentForm, null))
				{
					form.Show();
					AssertEquals("pre condition", FormWindowState.Normal, parentForm.WindowState);

					form.WindowState = FormWindowState.Minimized;

					AssertEquals("Parent form should be minimized", FormWindowState.Minimized, parentForm.WindowState);
				}
			}
		}

		public void TestRestoreParentFormWhenRestored()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				var originalWindowState = parentForm.WindowState;

				using (var form = new HVLVCancelableMinimisableProgressForm(parentForm, null))
				{
					form.Show();
					form.WindowState = FormWindowState.Minimized;
					AssertEquals("pre condition", FormWindowState.Minimized, parentForm.WindowState);

					form.WindowState = FormWindowState.Normal;

					AssertEquals("Parent form should be restored to its original window state", originalWindowState, parentForm.WindowState);
				}
			}
		}

		public void TestRestoreParentFormWhenDisposed()
		{
			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				var originalWindowState = parentForm.WindowState;

				using (var form = new HVLVCancelableMinimisableProgressForm(parentForm, null))
				{
					form.Show();
					form.WindowState = FormWindowState.Minimized;
					AssertEquals("pre condition", FormWindowState.Minimized, parentForm.WindowState);
				}

				AssertEquals("Parent form should be restored to its original window state", originalWindowState, parentForm.WindowState);
			}
		}

		public void TestCancelActionIsInvokedAfterClickCancel()
		{
			var cancelActionInvoked = false;

			using (var form = new HVLVCancelableMinimisableProgressForm(null, CancelAction))
			{
				form.Show();
				AssertEquals("pre condition", false, cancelActionInvoked);

				form.CancelButton.PerformClick();

				AssertEquals("Should requested cancellation on token source", true, cancelActionInvoked);
			}

			void CancelAction()
			{
				cancelActionInvoked = true;
			}
		}

#if !WINZOR	// The form.ClassStyle and related CreateParams properties are only available in WinForms, so we ignore this test in Winzor
		public void TestCloseButtonIsNotAvailable()
		{
			using (var form = new HVLVCancelableMinimisableProgressForm(null, null))
			{
				form.Show();
				AssertEquals("Progress from should have class style flag set at 0x200", 0x200, form.ClassStyle & 0x200);
			}
		}
#endif

		#region Implementations

		protected override Form GetFormToBashCore()
		{
			return new HVLVCancelableMinimisableProgressForm(null, null);
		}

		protected override bool AllowFormSizeFixed => true;

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		#endregion
	}
}
