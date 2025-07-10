using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class PickupDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (ZForm form = new ZForm(declaration))
			using (var control = new PickupDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertNotNull("LCLDatesOverrideCheckBox", control.FindSingleOrDefault<ZCheckBox>("LCLDatesOverrideCheckBox"));
					AssertNotNull("DurationLabel", control.FindSingleOrDefault<ZLabel>("DurationLabel"));
					AssertNotNull("JE_PickupTruckWaitTimeEdit", control.FindSingleOrDefault<ZTimeEditEx>("JE_PickupTruckWaitTimeEdit"));
					AssertNotNull("JE_PickupLabourTimeEdit", control.FindSingleOrDefault<ZTimeEditEx>("JE_PickupLabourTimeEdit"));
					AssertNotNull("JE_PickupLabourChargeBoundCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("JE_PickupLabourChargeBoundCalcEdit"));
					AssertNotNull("JE_FCLPickupEquipmentNeededBoundDropEdit", control.FindSingleOrDefault<ZDropEdit>("JE_FCLPickupEquipmentNeededBoundDropEdit"));
					AssertNotNull("FCL_AvailableDateEdit", control.FindSingleOrDefault<ZDateEdit>("FCL_AvailableDateEdit"));
					AssertNotNull("FCL_StorageDateEdit", control.FindSingleOrDefault<ZDateEdit>("FCL_StorageDateEdit"));
					AssertNotNull("PickupRequiredFromDateEdit", control.FindSingleOrDefault<ZDateEdit>("PickupRequiredFromDateEdit"));
					AssertNotNull("JE_PickupRequiredByBoundDateEdit", control.FindSingleOrDefault<ZDateEdit>("JE_PickupRequiredByBoundDateEdit"));
					AssertNotNull("JE_PickupCartageAdvisedBoundDateEdit", control.FindSingleOrDefault<ZDateEdit>("JE_PickupCartageAdvisedBoundDateEdit"));
					AssertNotNull("JE_PickupCartageCompletedBoundDateEdit", control.FindSingleOrDefault<ZDateEdit>("JE_PickupCartageCompletedBoundDateEdit"));
					AssertNotNull("JE_EstimatedDeliveryOrPickupDateEdit", control.FindSingleOrDefault<ZDateEdit>("JE_EstimatedDeliveryOrPickupDateEdit"));
					AssertNotNull("ChargeLabel", control.FindSingleOrDefault<ZLabel>("ChargeLabel"));
					AssertNotNull("JE_DeliveryTruckWaitChargeBoundCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("JE_DeliveryTruckWaitChargeBoundCalcEdit"));
					AssertNotNull("JP_LCLAirStorageChargeBoundCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("JP_LCLAirStorageChargeBoundCalcEdit"));
					AssertNotNull("JP_LCLAirStorageDaysOrHoursBoundCalcEdit", control.FindSingleOrDefault<ZCalcEdit>("JP_LCLAirStorageDaysOrHoursBoundCalcEdit"));
					AssertNotNull("JP_LCLAvailableDateEdit", control.FindSingleOrDefault<ZDateEdit>("JP_LCLAvailableDateEdit"));
					AssertNotNull("JP_LCLStorageCommencesDateEdit", control.FindSingleOrDefault<ZDateEdit>("JP_LCLStorageCommencesDateEdit"));
				});
			}
		}

		public void TestJP_LCLAirStorageDaysOrHoursBoundCalcEdit_InvisibleIfIsNonTransportDeclarationType()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsExport).Returns(false);
			mockDeclaration.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			using (ZForm form = new ZForm(mockDeclaration.Object))
			using (var control = new PickupDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.FindSingleOrDefault<ZCalcEdit>("JP_LCLAirStorageDaysOrHoursBoundCalcEdit").Visible);
			}
		}

		public void TestJP_LCLAirStorageChargeBoundCalcEdit_InvisibleIfIsNonTransportDeclarationType()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.IsExport).Returns(false);
			mockDeclaration.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			using (ZForm form = new ZForm(mockDeclaration.Object))
			using (var control = new PickupDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.FindSingleOrDefault<ZCalcEdit>("JP_LCLAirStorageChargeBoundCalcEdit").Visible);
			}
		}

		public void TestJP_LCLAirStorageChargeBoundCalcEdit_Caption()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (ZForm form = new ZForm(declaration))
			using (var control = new PickupDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Default", "Storage Days", control.FindSingleOrDefault<ZCalcEdit>("JP_LCLAirStorageChargeBoundCalcEdit").GetExtension<ILabelCaptionRenderer>().Caption);
					declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("Air", "Storage Hours", control.FindSingleOrDefault<ZCalcEdit>("JP_LCLAirStorageChargeBoundCalcEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				});
			}
		}

		public void TestJE_PickupCartageAdvisedBoundDateEdit_Caption()
		{
			using (var control = new PickupDetailsUserControl())
			{
				AssertEquals("Trn. Booking Requested", control.FindSingleOrDefault<ZDateEdit>("JE_PickupCartageAdvisedBoundDateEdit").CaptionResourceString.Caption);
			}
		}

		public void TestJE_PickupCartageCompletedBoundDateEdit_CaptionResourceString()
		{
			using (var control = new PickupDetailsUserControl())
			{
				var captionResourceString = control.FindSingleOrDefault<ZDateEdit>("JE_PickupCartageCompletedBoundDateEdit").CaptionResourceString;
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Actual Pickup", captionResourceString.Caption);
					AssertEquals("FullDescription", "The actual date/time the goods have been picked up from the origin by the carrier.", captionResourceString.FullDescription);
				});
			}
		}

		public void TestJP_LCLAvailableDateEdit_CaptionResourceString()
		{
			using (var control = new PickupDetailsUserControl())
			{
				AssertEquals("CFS Available", control.FindSingleOrDefault<ZDateEdit>("JP_LCLAvailableDateEdit").CaptionResourceString.Caption);
			}
		}

		public void TestJP_LCLStorageCommencesDateEdit_CaptionResourceString()
		{
			using (var control = new PickupDetailsUserControl())
			{
				AssertEquals("CFS Storage Start", control.FindSingleOrDefault<ZDateEdit>("JP_LCLStorageCommencesDateEdit").CaptionResourceString.Caption);
			}
		}
	}
}
