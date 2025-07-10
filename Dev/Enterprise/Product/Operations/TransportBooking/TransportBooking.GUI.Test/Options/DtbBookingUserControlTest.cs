using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Testing
{
	class DtbBookingUserControlTest : DtbBookingTestCaseWithFactory
	{
		public void TestGridBookingCreatedHooked()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM123";
			Factory.Save();

			var wrapper = new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC);

			using (var form = new ZForm(wrapper))
			{
				var userControl = new DtbBookingUserControlForTesting();
				form.Controls.Add(userControl);
				form.Show();

				DtbDeliveryManager manager = null;
				userControl.TransportBookingsGridExposed.SetActionForDeliveryManager(m => manager = m);
				userControl.DataSources.Clear();
				userControl.TransportBookingsGridExposed.CreateTransportBookingSplitButton.PerformButtonClick();
				manager.LastControllerForTest.LastShownForm.Dispose();
				AssertEquals("Should have called SetDataBinding twice.", 2, userControl.DataSources.Count);
				AssertNull("First call should have passed null.", userControl.DataSources[0]);

				var newWrapper = (DtbBookingParentWrapper)userControl.DataSources[1];
				AssertEquals("New bound object should have correct object.", dummy, newWrapper.Parent);
				AssertEquals("New bound object should have correct direction.", DtbBookingDirection.PIC, newWrapper.Direction);
				AssertNotEquals("Should have bound a new object.", wrapper, newWrapper);
			}
		}

		public void TestDisposeDoesNotThrowErrorIfGridAlreadyNull()
		{
			AssertNoExceptionThrown(() =>
			{
				var userControl = new DtbBookingUserControlForTesting();
				userControl.DereferenceTransportBookingsGrid();
				userControl.Dispose();
			});
		}

		public void TestTransportBookingsGridMasterBookingNumberColumn_WhenMasterBookingsIsEnabled()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var userControl = new DtbBookingUserControlForTesting())
				{
					var masterBookingNumberColumnName = nameof(DtbBooking.MasterBooking) + "+" + nameof(DtbBooking.KM_JobID);
					var columnExists = userControl.TransportBookingsGridExposed.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == masterBookingNumberColumnName);

					AssertEquals("Master Booking Number column should exist in the transport bookings grid.", true, columnExists);
				}
			}
		}

		public void TestTransportBookingsGridMasterBookingNumberColumn_WhenMasterBookingsIsDisabled()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var userControl = new DtbBookingUserControlForTesting())
				{
					var masterBookingNumberColumnName = nameof(DtbBooking.MasterBooking) + "+" + nameof(DtbBooking.KM_JobID);
					var columnExists = userControl.TransportBookingsGridExposed.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == masterBookingNumberColumnName);

					AssertEquals("Master Booking Number column should not exist in the transport bookings grid.", false, columnExists);
				}
			}
		}

		public void TestTransportBookingsGridCO2eColumn()
		{
			var cO2eColumnName = nameof(DtbBooking.TotalCO2eForSorting);
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var userControl = new DtbBookingUserControlForTesting())
			{
				var columnExists = userControl.TransportBookingsGridExposed.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == cO2eColumnName);

				AssertEquals("CO2e column should not exist in the transport bookings grid when registry is disabled", false, columnExists);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var userControl = new DtbBookingUserControlForTesting())
			{
				var columnExists = userControl.TransportBookingsGridExposed.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(col => col.ColumnName == cO2eColumnName);

				AssertEquals("CO2e column should exist in the transport bookings grid when registry is enabled", true, columnExists);
			}
		}

		class DtbBookingUserControlForTesting : DtbBookingUserControl
		{
			public override void SetDataBinding(object dataSource, string dataMember)
			{
				DataSources.Add(dataSource);

				base.SetDataBinding(dataSource, dataMember);
			}

			public List<object> DataSources { get; } = new List<object>();

			public ParentTransportBookingModuleGrid TransportBookingsGridExposed => TransportBookingsGrid;
			public void DereferenceTransportBookingsGrid()
			{
				TransportBookingsGrid = null;
			}
		}
	}
}
