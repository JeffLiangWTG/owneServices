using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Interop;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(AgencyBookingForm))]
	internal class AgencyBookingBOFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			using (AgencyBookingForm form = new AgencyBookingForm(GetBooking()))
			{
				form.ControllerID = ControllerIDs.AgencyBooking;
				form.Show();
				Application.DoEvents();
				ExposeTabPages(form);
			}
		}

		[ExpectNoExceptions]
		public void TestClickOnDisappearingTabControl()
		{
			var clickPoint = new Point(200, 5);
			using (var form = (AgencyBookingForm)this.GetFormToBash())
			{
				form.Show();
				var bookingDetailsControl = (BookingDetailsControl)form.Controls.Find("BookingDetails", true)[0];
				ZTabControl tabControl = (ZTabControl)bookingDetailsControl.Controls.Find("ShipmentContentTabControl", true)[0];
#pragma warning disable IDE0004 // Remove unnecessary cast. The cast is necessary, however visual studio incorrectly marks it as redundant
				MouseSender.PostMessage(tabControl, tabControl.Handle, WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, (IntPtr)MakeLParam(clickPoint.X, clickPoint.Y));
#pragma warning restore IDE0004 // restore warning
				Application.DoEvents();
				AssertEquals("Precondition: clicking on the third tab", 3, tabControl.SelectedIndex);
			}

			using (var form = (AgencyBookingForm)this.GetFormToBash())
			{
				form.Show();
				var bookingDetailsControl = (BookingDetailsControl)form.Controls.Find("BookingDetails", true)[0];
				var dropEdit = (ZDropEdit)bookingDetailsControl.Controls.Find("JS_PackingModeBoundDropEdit", true)[0];
				ZTabControl tabControl = (ZTabControl)bookingDetailsControl.Controls.Find("ShipmentContentTabControl", true)[0];
				dropEdit.Focus();
				KeySender.PostKeyDown(dropEdit.CodeBox, Keys.R);
				Application.DoEvents();
#pragma warning disable IDE0004 // Remove unnecessary cast. The cast is necessary, however visual studio incorrectly marks it as redundant
				MouseSender.PostMessage(tabControl, tabControl.Handle, WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, (IntPtr)MakeLParam(clickPoint.X, clickPoint.Y));
#pragma warning restore IDE0004 // restore warning
				Application.DoEvents();
				AssertEquals(3, tabControl.TabPages.Count);
			}
		}

		int MakeLParam(int loWord, int hiWord)
		{
			return ((hiWord << 16) | (loWord & 0xffff));
		}

		#region Implementation

		AgencyBooking GetBooking()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = "FCL";
			shipment.HasChanges = false;
			return shipment;
		}

		void ExposeTabPages(Control ctrl)
		{
			foreach (Control nextCtrl in ctrl.Controls)
			{
				if (nextCtrl is ZTabControl)
				{
					ZTabControl tabControl = nextCtrl as ZTabControl;
					foreach (ZTabPage page in tabControl.TabPages)
					{
						tabControl.SelectedTab = page;
						Application.DoEvents();
						ExposeTabPages(page);
					}
				}
				else
				{
					ExposeTabPages(nextCtrl);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			AgencyBookingForm result = new AgencyBookingForm(GetBooking());
			result.ControllerID = ControllerIDs.AgencyBooking;
			return result;
		}

		protected override void BashControl(Control controlToBash)
		{
			base.BashControl(controlToBash);
			if (controlToBash.Name == "JS_PackingModeBoundDropEdit")
			{
				ZDropEdit modeDropEdit = (ZDropEdit)controlToBash;
				modeDropEdit.Text = Enterprise.Core.Constants.ContainerModes.FCL;
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		#endregion
	}
}
