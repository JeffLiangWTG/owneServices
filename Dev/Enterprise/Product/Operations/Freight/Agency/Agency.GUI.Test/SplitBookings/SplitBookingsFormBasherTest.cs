using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(SplitBookingsForm))]
	internal class SplitBookingsFormBasherTest : ZFormBasherTest
	{
		public void TestShow()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			Factory.Save();
			SplitBookingsForm.SuppressAutoCloseInTests = true;
			using (SplitBookingsForm form = SplitBookingsForm.Show(booking))
			{
				AssertNotNull(form);
				AssertType(typeof(SplitBookingsHeader), form.CurrentDataItem);
				SplitBookingsHeader header = (SplitBookingsHeader)form.CurrentDataItem;
				AssertNotEquals(booking.Factory, header.Factory);
				AssertEquals(booking.PK, header.OriginalShipmentPK);
				AssertEquals(header.Factory, header.OriginalShipment.Factory);
				AssertEquals(header.Factory, header.NewShipment.Factory);
			}
		}

		[ExpectNoExceptions]
		public void TestShow_GetColumnControl()
		{
			var shipment = Factory.New<AgencyShipment>();
			var container = shipment.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 5;
			container.JC_GrossWeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			container.JC_GrossWeight = 32000;
			container.JC_ReleaseNum = "Test num";
			var header = new SplitBookingsHeader(Factory);
			header.OriginalShipmentPK = shipment.PK;
			header.ShowContainers = true;
			Factory.Save();
			using (SplitBookingsForm form = new SplitBookingsForm(header))
			{
				form.Show();
				Application.DoEvents();
				var controls = form.Controls.Find("SplitGrid", true);
				var grid = (SplitGrid)controls[0];
				AssertNotNull(grid);
				var grid1 = (ZGrid)typeof(SplitGrid).GetField("grid1", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
				AssertNotNull(grid1);
				for (int i = 0; i < grid1.Columns.Count; i++)
				{
					Rectangle rec = grid1.GetCellBounds(0, i);
					var e = new MouseEventArgs(MouseButtons.Left, 1, (rec.Left + rec.Right) >> 1, (rec.Top + rec.Bottom) >> 1, 0);
					typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid1, new object[] { e });
				}
			}
		}

		public void TestOK()
		{
			AgencyBooking originalBooking = Factory.New<AgencyBooking>();
			Factory.Save();
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			AgencyBooking newBooking = testFactory.New<AgencyBooking>();
			SplitBookingsHeader header = new SplitBookingsHeader(testFactory);
			header.OriginalShipmentPK = originalBooking.PK;
			header.NewShipmentPK = newBooking.PK;
			using (SplitBookingsForm form = new SplitBookingsForm(header))
			{
				form.Show();
				Application.DoEvents();
				ZButton button = (ZButton)form.Controls.Find("okButton", true)[0];
				button.PerformClick();
				Application.DoEvents();
				AssertEquals("should show a message", string.Format("Question The booking {0} has been created.", newBooking.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("form should be closed", false, form.Visible);
				AssertEquals("form should be saved", true, newBooking.IsInDatabase);
			}
		}

		public void TestCancel()
		{
			AgencyBooking originalBooking = Factory.New<AgencyBooking>();
			Factory.Save();
			BusinessObjectFactory denySaveFactory = new BusinessObjectFactory();
			denySaveFactory.Saving += delegate
			{
				throw new InvalidOperationException("KA-BOOM!!!");
			};
			AgencyBooking newBooking = denySaveFactory.New<AgencyBooking>();
			SplitBookingsHeader header = new SplitBookingsHeader(denySaveFactory);
			header.OriginalShipmentPK = originalBooking.PK;
			header.NewShipmentPK = newBooking.PK;
			using (SplitBookingsForm form = new SplitBookingsForm(header))
			{
				form.Show();
				Application.DoEvents();
				ZButton button = (ZButton)form.Controls.Find("cancelButton", true)[0];
				button.PerformClick();
				Application.DoEvents();
				AssertEquals("should show a message", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("form should be closed", false, form.Visible);
				AssertEquals("form should not be saved", false, newBooking.IsInDatabase);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			AgencyBooking booking1 = Factory.New<AgencyBooking>();
			booking1.SetReadOnlyIncludingChildren(true);
			AgencyBooking booking2 = Factory.New<AgencyBooking>();
			booking2.SetReadOnlyIncludingChildren(true);
			SplitBookingsHeader header = new SplitBookingsHeader(Factory);
			using (header.SuspendSettingHasChanges())
			{
				header.OriginalShipmentPK = booking1.PK;
				header.NewShipmentPK = booking2.PK;
			}

			return new SplitBookingsForm(header);
		}

		protected override void TearDown()
		{
			base.TearDown();
			SplitBookingsForm.SuppressAutoCloseInTests = false;
		}
		#endregion
	}
}
