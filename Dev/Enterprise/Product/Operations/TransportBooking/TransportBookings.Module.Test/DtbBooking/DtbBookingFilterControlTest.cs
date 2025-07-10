using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Module;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module.Testing
{
	sealed class DtbBookingFilterControlTest : TestCaseWithFactory
	{
		public void TestHoldReasonColumn()
		{
			using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
			{
				var holdReason = nameof(DtbBooking.Job) + "+" + nameof(DtbBooking.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new DtbBookingCollection(Factory);
			var filter = new DtbBookingFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new DtbBookingFilterControl(collection, filter))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull("The 'CustomString' custom field should exist", filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}

		public void TestGetNewFilterStripControl()
		{
			using (var form = new ZForm())
			{
				var bookings = GetBookingCollection();
				var filterBO = new DtbBookingFilterBusinessObject();
				var filterControl = GetFilterControl(bookings, filterBO);

				form.Controls.Add(filterControl);
				form.Show();

				filterControl.AddNewFilterStrip();
				Assert("Must return WorkflowFilterStrip so that workflow filter strips may be selected", filterControl.LastFilterStripType.IsSubclassOf(typeof(WorkflowFilterStrip)));
			}
		}

		public void TestMasterBookingFilterStripShowsWhenMasterBookingsEnabled()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
				{
					var columnName = nameof(DtbBooking.MasterBooking) + "+" + nameof(DtbBooking.KM_JobID);
					var columnExists = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == columnName);

					AssertEquals("Master bookings column should exist in the filter control.", true, columnExists);
				}
			}
		}

		public void TestMasterBookingsFilterStripDoesNotShowWhenMasterBookingsDisabled()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
				{
					var columnName = nameof(DtbBooking.MasterBooking) + "+" + nameof(DtbBooking.KM_JobID);
					var columnExists = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == columnName);

					AssertEquals("Master bookings column should not exist in the filter control.", false, columnExists);
				}
			}
		}

		public void TestCO2eStatusExistInFilterList_WhenCO2eRegistryEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
			{
				var columnName = nameof(DtbBooking.CO2eStatus);
				var debug = filterControl.FilteredGrid.ColumnStyles;
				var columnExists = filterControl.FilteredGrid.ColumnStyles
									.Cast<ZGridColumnInfo>()
									.Any(col => col.ColumnName == columnName);

				AssertEquals("CO2eStatus column should exist in the filter control.", true, columnExists);
			}
		}

		public void TestCO2eStatusNotExistInFilterList_WhenCO2eRegistryDisabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
			{
				var columnName = nameof(DtbBooking.CO2eStatus);
				var debug = filterControl.FilteredGrid.ColumnStyles;
				var columnExists = filterControl.FilteredGrid.ColumnStyles
									.Cast<ZGridColumnInfo>()
									.Any(col => col.ColumnName == columnName);

				AssertEquals("CO2eStatus column should not exist in the filter control.", false, columnExists);
			}
		}

		public void TestTotalCO2eColumnExistInFilterList_WhenCO2eRegistryEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
			{
				var columnName = nameof(DtbBooking.TotalCO2eForSorting);
				var columnExists = filterControl.FilteredGrid.ColumnStyles
									.Cast<ZGridColumnInfo>()
									.Any(col => col.ColumnName == columnName);

				AssertEquals("CO2e (kg) column should exist in the filter control.", true, columnExists);
			}
		}

		public void TestTotalCO2eColumnNotExistInFilterList_WhenCO2eRegistryDisabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var filterControl = new DtbBookingFilterControl(new DtbBookingCollection(Factory), new DtbBookingFilterBusinessObject()))
			{
				var columnName = nameof(DtbBooking.TotalCO2eForSorting);
				var columnExists = filterControl.FilteredGrid.ColumnStyles
									.Cast<ZGridColumnInfo>()
									.Any(col => col.ColumnName == columnName);

				AssertEquals("CO2e (kg) column should not exist in the filter control.", false, columnExists);
			}
		}

		DtbBookingFilterControl GetFilterControl(DtbBookingCollection gridCollection, DtbBookingFilterBusinessObject filterBizO)
		{
			return new DtbBookingFilterControl(gridCollection, filterBizO);
		}

		DtbBookingCollection GetBookingCollection()
		{
			return new DtbBookingCollection(Factory);
		}
	}
}
