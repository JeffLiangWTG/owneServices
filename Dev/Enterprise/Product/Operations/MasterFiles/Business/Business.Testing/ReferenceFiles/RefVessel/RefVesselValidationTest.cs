using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefVesselValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRV_Code()
		{
			vessel.RV_Code = ZString.Empty;
			vessel.Validation.ValidateAll();
			AssertHasErrors(vessel.RV_CodeInfo);

			vessel.RV_Code = "harbl";
			AssertNoNotifications(vessel.RV_CodeInfo);

			vessel.RV_Code = ZString.Empty;
			AssertHasErrors(vessel.RV_CodeInfo);

			vessel.RV_Code = "harbl";
			Factory.Save();

			var newVessel = Factory.New<RefVessel>();
			newVessel.RV_Code = "harbl";
			AssertHasError(newVessel.RV_CodeInfo, RefVesselValidation.VesselAlreadyExists);

			newVessel.RV_Code = "ZZZZZZ";
			AssertNoError(newVessel.RV_CodeInfo, RefVesselValidation.VesselAlreadyExists);

			Factory.Save();

			var newVessel2 = Factory.New<RefVessel>();
			newVessel2.RV_Code = "ZZZZZZ";
			AssertHasError(newVessel2.RV_CodeInfo, RefVesselValidation.VesselAlreadyExists);

			var newVessel3 = RefVessel.LookupVesselByCode("ADMIRALENGRACHT", Factory);
			newVessel3.RV_Code = "ZZZZZZ";
			AssertHasError(newVessel3.RV_CodeInfo, RefVesselValidation.VesselAlreadyExists);
		}

		public void TestValidateRV_Lloyds()
		{
			vessel.Validation.ValidateAll();
			AssertNoNotifications(vessel.RV_LloydsNumberInfo);

			vessel.RV_LloydsNumber = "483729X";
			AssertHasWarnings(vessel.RV_LloydsNumberInfo);

			vessel.RV_LloydsNumber = "4837293";
			AssertNoNotifications(vessel.RV_LloydsNumberInfo);
		}

		public void TestValidateRV_LloydsDuplicateActive()
		{
			vessel.RV_Code = "vessel1";
			vessel2.RV_Code = "vessel2";

			CheckDuplicateLloydsNotification(true, true, "4837293", "4837293", delegate()
			{ AssertHasError(vessel.RV_LloydsNumberInfo, "All active Vessels must have a unique Lloyds Number. Either mark a Vessel as inactive, or change the Lloyds Number.\r\nThese other Vessels have the same Lloyds Number:\r\n\r\nvessel2\r\n"); });
			AssertionDelegate noNotificationsAssertion = delegate()
			{ AssertNoNotifications(vessel.RV_LloydsNumberInfo); };
			CheckDuplicateLloydsNotification(true, true, "4837293", "483729X", noNotificationsAssertion);
			CheckDuplicateLloydsNotification(true, false, "4837293", "4837293", noNotificationsAssertion);
			CheckDuplicateLloydsNotification(false, true, "4837293", "4837293", noNotificationsAssertion);
			CheckDuplicateLloydsNotification(false, false, "4837293", "4837293", noNotificationsAssertion);
			CheckDuplicateLloydsNotification(true, true, ZString.Empty, ZString.Empty, noNotificationsAssertion);
		}

		void CheckDuplicateLloydsNotification(ZBool vessel1Active, ZBool vessel2Active, ZString vessel1Lloyds, ZString vessel2Lloyds, AssertionDelegate assertion)
		{
			vessel.RV_IsActive = vessel1Active;
			vessel2.RV_IsActive = vessel2Active;
			vessel.RV_LloydsNumber = vessel1Lloyds;
			vessel2.RV_LloydsNumber = vessel2Lloyds;
			vessel.Validation.ValidateAll();
			assertion();
		}

		public void TestValidateRV_ScreeningStatus()
		{
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Block;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.NeedsScreening;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Release;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = string.Empty;
			AssertHasError(vessel.RV_ScreeningStatusInfo, "Please enter a Screening Status.");

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(vessel.RV_ScreeningStatusInfo);

			vessel.RV_ScreeningStatus = "ZZZ";
			AssertHasError(vessel.RV_ScreeningStatusInfo, "Enter a valid Screening Status.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			vessel = Factory.New<RefVessel>();
			vessel2 = Factory.New<RefVessel>();
		}

		RefVessel vessel;
		RefVessel vessel2;

		delegate void AssertionDelegate();

		#endregion
	}
}
