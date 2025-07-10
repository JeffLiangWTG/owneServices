using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class BaseInvoiceArrayBoundGridTest : TestCase
	{
		[ExpectNoExceptions()]
		public void TestAddColumnsToSkip()
		{
			using (BaseInvoiceArrayBoundGrid testGrid = new BaseInvoiceArrayBoundGrid())
			{
				testGrid.AddColumnToSkip("FakeColumnToSkip");
			}
		}

#if !WINZOR

		[ExpectNoExceptions()]
		public void TestWndProc_ShouldCatchObjectDisposedException()
		{
			using (TestGrid control = new TestGrid())
			{
				control.CreateControl();
				var handle = new HandleRef(control, control.Handle);
				UnsafeNativeMethods.PostMessage(handle, WM_LBUTTONDOWN, new IntPtr(0), new IntPtr(0));
				Application.DoEvents();
			}
		}

		sealed class TestGrid : BaseInvoiceArrayBoundGrid
		{
			protected override void WndProc(ref Message m)
			{
				switch (m.Msg)
				{
					case WM_LBUTTONDOWN:
						Dispose();
						base.WndProc(ref m);
						return;
				}

				base.WndProc(ref m);
			}
		}

		const int WM_LBUTTONDOWN = 0x201;

#endif
	}
}
