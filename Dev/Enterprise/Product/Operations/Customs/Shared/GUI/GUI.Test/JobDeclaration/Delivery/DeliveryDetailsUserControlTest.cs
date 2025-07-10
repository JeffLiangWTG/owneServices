using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class DeliveryDetailsUserControlTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new DeliveryDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("LCLDatesOverrideCheckBox", control.FindSingleOrDefault<ZCheckBox>("LCLDatesOverrideCheckBox"));
					AssertNotNull("DurationLabel", control.FindSingleOrDefault<ZLabel>("DurationLabel"));
					AssertNotNull("TruckWaitTimeEdit", control.FindSingleOrDefault<ZTimeEditEx>("TruckWaitTimeEdit"));
					AssertNotNull("LabourTimeEdit", control.FindSingleOrDefault<ZTimeEditEx>("LabourTimeEdit"));
					AssertNotNull("LabourChargeBoundCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("LabourChargeBoundCalcEdit"));
					AssertNotNull("EquipmentNeededBoundDropEdit", control.FindSingleOrDefault<ZDropEdit>("EquipmentNeededBoundDropEdit"));
					AssertNotNull("FCL_AvailableDateEdit", control.FindSingleOrDefault<ZDateEdit>("FCL_AvailableDateEdit"));
					AssertNotNull("FCL_StorageDateEdit", control.FindSingleOrDefault<ZDateEdit>("FCL_StorageDateEdit"));
					AssertNotNull("RequiredByBoundDateEdit", control.FindSingleOrDefault<ZDateEdit>("RequiredByBoundDateEdit"));
					AssertNotNull("DeliveryRequiredFromDateEdit", control.FindSingleOrDefault<ZDateEdit>("DeliveryRequiredFromDateEdit"));
					AssertNotNull("CartageAdvisedBoundDateEdit", control.FindSingleOrDefault<ZDateEdit>("CartageAdvisedBoundDateEdit"));
					AssertNotNull("CartageCompletedBoundDateEdit", control.FindSingleOrDefault<ZDateEdit>("CartageCompletedBoundDateEdit"));
					AssertNotNull("EstimatedDeliveryDateEdit", control.FindSingleOrDefault<ZDateEdit>("EstimatedDeliveryDateEdit"));
					AssertNotNull("ChargeLabel", control.FindSingleOrDefault<ZLabel>("ChargeLabel"));
					AssertNotNull("WaitChargeBoundCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("WaitChargeBoundCalcEdit"));
					AssertNotNull("LCLAvailableDateEdit", control.FindSingleOrDefault<ZDateEdit>("LCLAvailableDateEdit"));
					AssertNotNull("LCLStorageCommencesDateEdit", control.FindSingleOrDefault<ZDateEdit>("LCLStorageCommencesDateEdit"));
				});
			}
		}

		public void TestCaptions()
		{
			using (var control = new DeliveryDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNull("LCLDatesOverrideCheckBox", control.FindSingle<ZCheckBox>("LCLDatesOverrideCheckBox").CaptionResourceString.Caption);
					AssertEquals("DurationLabel", "Duration", control.FindSingle<ZLabel>("DurationLabel").CaptionResourceString.Caption);
					AssertNull("TruckWaitTimeEdit", control.FindSingle<ZTimeEditEx>("TruckWaitTimeEdit").CaptionResourceString.Caption);
					AssertNull("LabourTimeEdit", control.FindSingle<ZTimeEditEx>("LabourTimeEdit").CaptionResourceString.Caption);
					AssertEquals("LabourChargeBoundCalcEdit", "Delivery Labor", control.FindSingle<ZCalcEdit>("LabourChargeBoundCalcEdit").CaptionResourceString.Caption);
					AssertEquals("EquipmentNeededBoundDropEdit", "Drop Mode", control.FindSingle<ZDropEdit>("EquipmentNeededBoundDropEdit").CaptionResourceString.Caption);
					AssertEquals("FCL_AvailableDateEdit", "Available Date", control.FindSingle<ZDateEdit>("FCL_AvailableDateEdit").CaptionResourceString.Caption);
					AssertEquals("FCL_StorageDateEdit", "Storage Date", control.FindSingle<ZDateEdit>("FCL_StorageDateEdit").CaptionResourceString.Caption);
					AssertEquals("RequiredByBoundDateEdit", "Delivery Required By", control.FindSingle<ZDateEdit>("RequiredByBoundDateEdit").CaptionResourceString.Caption);
					AssertEquals("DeliveryRequiredFromDateEdit", "Delivery Required From", control.FindSingle<ZDateEdit>("DeliveryRequiredFromDateEdit").CaptionResourceString.Caption);
					AssertEquals("CartageAdvisedBoundDateEdit", "Trn. Booking Requested", control.FindSingle<ZDateEdit>("CartageAdvisedBoundDateEdit").CaptionResourceString.Caption);
					AssertEquals("EstimatedDeliveryDateEdit", "Estimated Delivery Date", control.FindSingle<ZDateEdit>("EstimatedDeliveryDateEdit").CaptionResourceString.Caption);
					AssertEquals("ChargeLabel", "Charge", control.FindSingle<ZLabel>("ChargeLabel").CaptionResourceString.Caption);
					AssertEquals("WaitChargeBoundCalcEdit", "Truck Wait Time", control.FindSingle<ZCalcEdit>("WaitChargeBoundCalcEdit").CaptionResourceString.Caption);
					AssertEquals("LCLAvailableDateEdit", "CFS Available", control.FindSingle<ZDateEdit>("LCLAvailableDateEdit").CaptionResourceString.Caption);
					AssertEquals("LCLStorageCommencesDateEdit", "CFS Storage Start", control.FindSingle<ZDateEdit>("LCLStorageCommencesDateEdit").CaptionResourceString.Caption);
				});
			}
		}
	}
}
