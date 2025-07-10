using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	class BaseJobVoyOriginValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateJA_A_ARV

		[TestDate(2011, 06, 26)]
		public void TestValidateJA_A_ARV()
		{
			var today = ZDateTime.Today;
			var dateOrderMessage = "ATA can not be after ATD.";
			var futureDateMessage = "The Actual Time of Arrival cannot be set in the future. The date 27-Jun-11 00:00 is in the future for AUSYD (UTC+0).";

			Origin.JA_A_ARV = ZDateTime.Empty;
			Origin.JA_A_DEP = today.AddDays(-1);
			AssertNoErrors("Pre-condition: ATA should not have errors.", Origin.JA_A_ARVInfo);

			Origin.JA_A_ARV = today.AddDays(-2);
			AssertNoErrors("No errors expected, ATA is before ATD", Origin.JA_A_ARVInfo);

			Origin.JA_A_ARV = today.AddDays(1);
			AssertHasError("Error expected as ATD comes before ATA", Origin.JA_A_ARVInfo, dateOrderMessage);
			AssertHasError("Error expected as ATA has been set to a future date", Origin.JA_A_ARVInfo, futureDateMessage);

			Origin.JA_A_ARV = ZDateTime.Empty;
			AssertNoErrors("No error expected for an empty ATA.", Origin.JA_A_ARVInfo);
		}

		#endregion

		#region TestValidateJA_E_ARV

		public void TestValidateJA_E_ARV()
		{
			var origin = Factory.New<VoyageOrigin>();
			origin.JA_E_ARV = ZDateTime.Today;
			AssertNoErrors("ETD not set, no error expected", origin.JA_E_ARVInfo);
			origin.JA_E_DEP = ZDateTime.Today.AddDays(5);
			AssertNoErrors("ETA is before ETD, no error expected", origin.JA_E_ARVInfo);
			origin.JA_E_ARV = ZDateTime.Today.AddDays(10);
			AssertHasErrors("ETA is after ETD, error expected", origin.JA_E_ARVInfo);
		}

		#endregion

		#region TestValidateJX_FCLCutOff

		public void TestValidateJX_FCLCutOff()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_CutOff = ZDateTime.Empty;
			Origin.Validation.ValidateJA_CutOff();
			Assert("FCL is empty, no warning expected", !Origin.JA_CutOffInfo.HasWarnings());

			Sailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("CTO cut off is after ETD, error expected", Origin.JA_CutOffInfo.HasErrors());

			Sailing.Origin.JA_CutOff = ZDateTime.Today;
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("CTO cut off is same as ETD, no error expected", !Origin.JA_CutOffInfo.HasErrors());

			Sailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("CTO cut off is before ETD, no error expected", !Origin.JA_CutOffInfo.HasErrors());
		}

		#endregion

		#region TestValidateJA_ReceivalCommences

		public void TestValidateJA_ReceivalCommences()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_E_DEP = ZDateTime.Today;
			Sailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_ReceivalCommences = ZDateTime.Empty;
			Origin.Validation.ValidateJA_ReceivalCommences();
			Assert("Receival start date is empty, no warning expected", !Origin.JA_ReceivalCommencesInfo.HasWarnings());

			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Today.AddDays(1);
			Origin.Validation.ValidateJA_ReceivalCommences();
			Assert("Receival start is after ETD, error expected", Origin.JA_ReceivalCommencesInfo.HasErrors());

			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReceivalCommences();
			Assert("Receival start is after cut off, error expected", Origin.JA_ReceivalCommencesInfo.HasErrors());

			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReceivalCommences();
			Assert("Receival start is before ETD, no error expected", !Origin.JA_ReceivalCommencesInfo.HasErrors());
		}

		#endregion

		#region TestValidateJA_CutOff

		public void TestValidateJA_CutOff()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_CutOff = ZDateTime.Empty;
			Origin.Validation.ValidateJA_CutOff();
			Assert("Cut Off is empty, no warning expected", !Origin.JA_CutOffInfo.HasWarnings());

			Sailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("Cut off is after ETD, error expected", Origin.JA_CutOffInfo.HasErrors());

			Sailing.Origin.JA_CutOff = ZDateTime.Today;
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("Cut off is same as ETD, no error expected", !Origin.JA_CutOffInfo.HasErrors());

			Sailing.Origin.JA_CutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("Cut off is before ETD, no error expected", !Origin.JA_CutOffInfo.HasErrors());

			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Today;
			Origin.Validation.ValidateJA_CutOff();
			Assert("Cut off is before receival start, error expected", Origin.JA_CutOffInfo.HasErrors());
		}

		#endregion

		#region TestValidateJA_ReeferReceivalCommences

		public void TestValidateJA_ReeferReceivalCommences()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_E_DEP = ZDateTime.Today;
			Sailing.Origin.JA_ReeferCutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_ReeferReceivalCommences = ZDateTime.Empty;
			Origin.Validation.ValidateJA_ReeferReceivalCommences();
			Assert("Reefer receival start date is empty, no warning expected", !Origin.JA_ReeferReceivalCommencesInfo.HasWarnings());

			Sailing.Origin.JA_ReeferReceivalCommences = ZDateTime.Today.AddDays(1);
			Origin.Validation.ValidateJA_ReeferReceivalCommences();
			Assert("Reefer receival start is after ETD, error expected", Origin.JA_ReeferReceivalCommencesInfo.HasErrors());

			Sailing.Origin.JA_ReeferReceivalCommences = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReeferReceivalCommences();
			Assert("Reefer receival start is after cut off, error expected", Origin.JA_ReeferReceivalCommencesInfo.HasErrors());

			Sailing.Origin.JA_ReeferReceivalCommences = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReeferReceivalCommences();
			Assert("Reefer receival start is before ETD, no error expected", !Origin.JA_ReeferReceivalCommencesInfo.HasErrors());
		}

		#endregion

		#region TestValidateJA_ReeferCutOff

		public void TestValidateJA_ReeferCutOff()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_ReeferCutOff = ZDateTime.Empty;
			Origin.Validation.ValidateJA_ReeferCutOff();
			Assert("Reefer Cut Off is empty, no warning expected", !Origin.JA_ReeferCutOffInfo.HasWarnings());

			Sailing.Origin.JA_ReeferCutOff = ZDateTime.Today.AddDays(1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReeferCutOff();
			Assert("Reefer cut off is after ETD, error expected", Origin.JA_ReeferCutOffInfo.HasErrors());

			Sailing.Origin.JA_ReeferCutOff = ZDateTime.Today;
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReeferCutOff();
			Assert("Reefer cut off is same as ETD, no error expected", !Origin.JA_ReeferCutOffInfo.HasErrors());

			Sailing.Origin.JA_ReeferCutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReeferCutOff();
			Assert("Reefer cut off is before ETD, no error expected", !Origin.JA_ReeferCutOffInfo.HasErrors());

			Sailing.Origin.JA_ReeferReceivalCommences = ZDateTime.Today;
			Origin.Validation.ValidateJA_ReeferCutOff();
			Assert("Reefer cut off is before Reefer receival start, error expected", Origin.JA_ReeferCutOffInfo.HasErrors());
		}

		#endregion

		#region TestValidateJA_EmptyReceivalCommences

		public void TestValidateJA_EmptyReceivalCommences()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_E_DEP = ZDateTime.Today;
			Sailing.Origin.JA_EmptyCutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_EmptyReceivalCommences = ZDateTime.Empty;
			Origin.Validation.ValidateJA_EmptyReceivalCommences();
			Assert("Empty receival start is empty, no warning expected", !Origin.JA_EmptyReceivalCommencesInfo.HasWarnings());

			Sailing.Origin.JA_EmptyReceivalCommences = ZDateTime.Today.AddDays(1);
			Origin.Validation.ValidateJA_EmptyReceivalCommences();
			Assert("Empty receival start is after ETD, error expected", Origin.JA_EmptyReceivalCommencesInfo.HasErrors());

			Sailing.Origin.JA_EmptyReceivalCommences = ZDateTime.Today;
			Origin.Validation.ValidateJA_EmptyReceivalCommences();
			Assert("Empty receival start is after cut off, error expected", Origin.JA_EmptyReceivalCommencesInfo.HasErrors());

			Sailing.Origin.JA_EmptyReceivalCommences = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_EmptyReceivalCommences();
			Assert("Empty receival start is before ETD, no error expected", !Origin.JA_EmptyReceivalCommencesInfo.HasErrors());
		}

		#endregion

		#region TestValidateJA_EmptyCutOff

		public void TestValidateJA_EmptyCutOff()
		{
			Sailing.JX_IsPublished = true;
			Origin.JA_EmptyCutOff = ZDateTime.Empty;
			Origin.Validation.ValidateJA_EmptyCutOff();
			Assert("Empty cut off is empty, no warning expected", !Origin.JA_EmptyCutOffInfo.HasWarnings());

			Sailing.Origin.JA_EmptyCutOff = ZDateTime.Today.AddDays(1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_EmptyCutOff();
			Assert("Empty cut off is after ETD, error expected", Origin.JA_EmptyCutOffInfo.HasErrors());

			Sailing.Origin.JA_EmptyCutOff = ZDateTime.Today;
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_EmptyCutOff();
			Assert("Empty cut off is same as ETD, no error expected", !Origin.JA_EmptyCutOffInfo.HasErrors());

			Sailing.Origin.JA_EmptyCutOff = ZDateTime.Today.AddDays(-1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Origin.Validation.ValidateJA_EmptyCutOff();
			Assert("Empty cut off is before ETD, no error expected", !Origin.JA_EmptyCutOffInfo.HasErrors());

			Sailing.Origin.JA_EmptyReceivalCommences = ZDateTime.Today;
			Origin.Validation.ValidateJA_EmptyCutOff();
			Assert("Empty cut off is before Empty receival start, error expected", Origin.JA_EmptyCutOffInfo.HasErrors());
		}

		#endregion

		#region TestValidateWithoutHomePort

		public void TestValidateWithoutHomePort()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";

			ZString oldHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
			try
			{
				AssertNoExceptionThrown(origin.Validation.ValidateJA_E_DEP);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = oldHomePort;
			}
		}

		#endregion

		#region TestValidateCircularRoute

		public void TestValidateCircularRouteAndDuplicateOrigins()
		{
			ZDateTime today = ZDateTime.Today;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = TransportTypeCode;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = today.AddDays(10);

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = today.AddDays(11);

			AssertNoErrors(origin.JA_E_DEPInfo);

			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.Validation.ValidateJA_E_DEP();
			AssertNoErrors("Different port is ok", origin.JA_E_DEPInfo);

			origin.JA_E_DEP = today.AddDays(9);
			AssertNoErrors("OK to return to the same port", origin.JA_E_DEPInfo);

			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_A_DEP = today.AddDays(13);
			AssertNoErrors(origin2.JA_RL_NKPortOfLoadingInfo);
			AssertNoErrors(origin2.JA_E_DEPInfo);

			origin2.JA_RL_NKPortOfLoading = "NZAKL";
			AssertHasError("Can't depart from the same port twice", origin2.JA_RL_NKPortOfLoadingInfo, "The Load Port has been duplicated and must be unique.");
		}

		#endregion

		#region TestValidateJA_RL_NKPortOfLoading

		public void TestValidateJA_RL_NKPortOfLoading()
		{
			var testDestination = Factory.New<VoyageDestination>();
			var testOrigin2 = Factory.New<VoyageOrigin>();
			var testSailing = Factory.New<JobSailing>();
			var loco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var loco2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var loco3 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			Voyage.Origins.Add(Origin);
			Voyage.Origins.Add(testOrigin2);

			Voyage.Destinations.Add(testDestination);

			Origin.JA_RL_NKPortOfLoading = "AAAAA";
			Assert("Invalid UNLOCO code, error expected", Origin.JA_RL_NKPortOfLoadingInfo.HasErrors());

			Origin.JA_RL_NKPortOfLoading = loco.RL_Code;
			testOrigin2.JA_RL_NKPortOfLoading = loco2.RL_Code;
			testDestination.JB_RL_NKPortOfDischarge = loco3.RL_Code;

			Voyage.GenerateSailings();
			Assert("All UNLOCOs are unique, no error on Test Origin", !Origin.JA_RL_NKPortOfLoadingInfo.HasErrors());

			testOrigin2.JA_RL_NKPortOfLoading = loco.RL_Code;
			Assert("Duplicate UNLOCOs as origin, error expected", testOrigin2.JA_RL_NKPortOfLoadingInfo.HasErrors());
		}

		public void TestValidateJA_RL_NKPortOfLoading_ProhibitedRouting()
		{
			var expectedWarning = "The Australian Government has imposed prohibitions on air cargo that has originated from, or transited through, Turkey. However, this prohibition applies only to electromechanical devices that weigh over 1 kilogram. You are required to meet with government requirements and/or consider a change of transport mode.";

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_IsLinked = true;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "AIR";

			var origin = voyage.Origins.AddNew("TRADA");
			var destination = voyage.Destinations.AddNew("AUBNE");

			voyage.GenerateSailings();

			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("TRADA", "AUBNE");

			origin.Validation.ValidateJA_RL_NKPortOfLoading();
			AssertHasWarning(origin.JA_RL_NKPortOfLoadingInfo, expectedWarning);

			origin.JA_RL_NKPortOfLoading = "USNYC";
			AssertNoWarning(origin.JA_RL_NKPortOfLoadingInfo, expectedWarning);

			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			origin.JA_RL_NKPortOfLoading = "TRADA";
			AssertNoWarning(origin.JA_RL_NKPortOfLoadingInfo, expectedWarning);
		}

		#endregion

		#region TestValidateJA_E_DEP

		public virtual void TestValidateJA_E_DEP()
		{
			Origin.JA_E_DEP = new ZDateTime(2013, 1, 5, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2013, 12, 14, 18, 36, 8);
			Origin.Validation.ValidateJA_E_DEP();
			AssertHasErrors("Origin departure is more than 6 months before arrival, error expected", Origin.JA_E_DEPInfo);

			Origin.JA_E_DEP = ZDateTime.Empty;
			Destination.JB_E_ARV = ZDateTime.Empty;
			Origin.Validation.ValidateJA_E_DEP();
			AssertHasErrors("Date of Departure is empty, error expected", Origin.JA_E_DEPInfo);

			Origin.JA_E_DEP = ZDateTime.Now;
			AssertNoErrors("Date is valid, no error expected", Origin.JA_E_DEPInfo);

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Origin.JA_E_DEP = ZDateTime.Empty;

			AssertNoErrors("For an arrival voyage, departure date should not have errors", Origin.JA_E_DEPInfo);
			AssertHasWarnings("Expecting departure date to have a warning", Origin.JA_E_DEPInfo);

			Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			Origin.Validation.ValidateJA_E_DEP();
			AssertHasErrors("Date of Departure is empty, error expected", Origin.JA_E_DEPInfo);

			var country = Factory.New<RefCountry>();
			country.RN_Code = Constants.CountryCodes.UnitedStates;

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { country.PK.ToGuid() });

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Origin.JA_E_DEP = ZDateTime.Empty;

			AssertHasErrors("Date of Departure is empty, error expected", Origin.JA_E_DEPInfo);
		}

		public void TestValidateETDPortPairs()
		{
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Voyage.JV_VoyageFlight = "QF69";

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			Voyage.Origins.Add(Origin);
			Voyage.Destinations.Add(Destination);
			Voyage.GenerateSailings();

			var sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);
			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);

			AssertHasError(Origin.JA_E_DEPInfo, "ETD is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);
			Origin.JA_E_DEP = new ZDateTime(2020, 1, 13, 15, 36, 8);

			AssertHasError(Origin.JA_E_DEPInfo, "ETD is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);
			Destination.JB_E_ARV = new ZDateTime(2020, 1, 10, 22, 36, 8);
			Factory.Save();

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 13, 15, 36, 8);

			AssertHasError(Origin.JA_E_DEPInfo, "ETD is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");

			Origin.JA_E_DEP = new ZDateTime(2020, 1, 12, 15, 36, 8);

			AssertHasError(Origin.JA_E_DEPInfo, "ETD is invalid for flight leg Port Pairs on this schedule: AUSYD -> USLAX.");
		}

		#endregion

		#region TestValidateJA_A_DEP

		[TestDate(2011, 06, 26)]
		public void TestValidateJA_A_DEP()
		{
			var today = ZDateTime.Today;
			var estimateVerseActualMessage = "ATD must be within 1 month of ETD.";
			var futureDateMessage = "The Actual Time of Departure cannot be set in the future. The date 26-Jul-11 00:00 is in the future for AUSYD (UTC+0).";

			Origin.JA_E_DEP = today.AddMonths(-1).AddDays(-1);
			Origin.JA_A_DEP = ZDateTime.Empty;
			AssertNoErrors("Pre-condition: ATD should have no errors.", Origin.JA_A_DEPInfo);

			Origin.JA_A_DEP = today;
			AssertHasError("Error expected as estimated and actual dates are more than a month apart", Origin.JA_A_DEPInfo, estimateVerseActualMessage);

			Origin.JA_A_DEP = today.AddMonths(-6);
			AssertHasError("Error expected as estimated and actual date are more than a month apart.", Origin.JA_A_DEPInfo, estimateVerseActualMessage);

			Origin.JA_A_DEP = today.AddMonths(-1).AddDays(1);
			AssertNoErrors("No error expected as expected as estimated and actual date are less than a month apart.", Origin.JA_A_DEPInfo);

			Origin.JA_A_DEP = today.AddMonths(1);
			AssertHasError("Error expected as actual date departure date has been set to a future date", Origin.JA_A_DEPInfo, futureDateMessage);
			AssertHasError("Error expected as estimated and actual date are more than a month apart.", Origin.JA_A_DEPInfo, estimateVerseActualMessage);

			Origin.JA_A_DEP = ZDateTime.Empty;
			AssertNoErrors("No errors expected on an empty ATD", Origin.JA_A_DEPInfo);
		}

		#endregion

		#region TestValidateJB_E_ARVUnsafeChange

		public virtual void TestValidateJB_E_ARVUnsafeChange()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobVoyage journey = newFactory.New<JobVoyage>();

			VoyageOrigin o1 = newFactory.New<VoyageOrigin>();
			o1.JA_RL_NKPortOfLoading = "AUSYD";
			o1.JA_E_DEP = now;
			journey.Origins.Add(o1);

			VoyageDestination d1 = newFactory.New<VoyageDestination>();
			d1.JB_RL_NKPortOfDischarge = "HKHKG";
			d1.JB_E_ARV = now.AddDays(8);
			journey.Destinations.Add(d1);

			VoyageDestination d2 = newFactory.New<VoyageDestination>();
			d2.JB_RL_NKPortOfDischarge = "JPTYO";
			d2.JB_E_ARV = now.AddDays(20);
			journey.Destinations.Add(d2);
			newFactory.Save();

			AssertEquals("Precondition: Expecting Journey to have 2 sailings", 2, journey.Sailings.Count);

			JobSailing sailing = journey.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "HKHKG");

			AssertNotNull(sailing);

			CommonShipment shipment = newFactory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_JX = sailing.PK;

			o1.JA_E_DEP = now.AddDays(11);

			Assert("Expecting error:", o1.JA_E_DEPInfo.HasErrors());

			AssertEquals("Expecting error:", "ETD is invalid for sailing Port Pairs on this schedule: AUSYD -> HKHKG.", o1.JA_E_DEPInfo.GetErrors().GetFirstMessage());

			o1.JA_E_DEP = now.AddDays(12);
			o1.JA_E_DEP = now.AddDays(11);

			Assert("Expecting error:", o1.JA_E_DEPInfo.HasErrors());

			AssertEquals("Expecting error:", "ETD is invalid for sailing Port Pairs on this schedule: AUSYD -> HKHKG.", o1.JA_E_DEPInfo.GetErrors().GetFirstMessage());
		}

		#endregion

		#region Test Validate Hazardous Receivals

		public void TestValidateJA_DGReceivalCommences()
		{
			const string errorReceivalAfterCutOff = "HAZ Receival Start date must be before the HAZ Cut Off date.";
			const string errorReceivalAfterETD = "HAZ Receival Start date cannot be after ETD.";
			ZDateTime today = ZDateTime.Today;

			Sailing.JX_IsPublished = true;
			Origin.Validation.ValidateJA_DGReceivalCommences();
			AssertNoErrors("Should be NO errors", Origin.JA_DGReceivalCommencesInfo);

			Origin.JA_E_DEP = today.AddDays(3);
			Origin.JA_DGReceivalCommences = today.AddDays(4);
			AssertHasError("Should be error", Origin.JA_DGReceivalCommencesInfo, errorReceivalAfterETD);

			Origin.JA_DGReceivalCommences = today.AddDays(2);
			AssertNoErrors("Should be NO errors", Origin.JA_DGReceivalCommencesInfo);

			Origin.JA_DGCutOff = today.AddDays(1);
			Origin.Validation.ValidateJA_DGReceivalCommences();
			AssertHasError("Should be error", Origin.JA_DGReceivalCommencesInfo, errorReceivalAfterCutOff);

			Origin.JA_DGReceivalCommences = today;
			AssertNoErrors("Should be NO errors", Origin.JA_DGReceivalCommencesInfo);
		}

		public void TestValidateJA_DGCutOff()
		{
			const string errorReceivalAfterCutOff = "HAZ Receival Start date must be before the HAZ Cut Off date.";
			const string errorCutOffAfterETD = "HAZ Cut off date cannot be after ETD.";
			ZDateTime today = ZDateTime.Today;

			Sailing.JX_IsPublished = true;
			Origin.Validation.ValidateJA_DGCutOff();
			AssertNoErrors("Should be NO errors", Origin.JA_DGCutOffInfo);

			Origin.JA_E_DEP = today.AddDays(3);
			Origin.JA_DGCutOff = today.AddDays(4);
			AssertHasError("Should be error", Origin.JA_DGCutOffInfo, errorCutOffAfterETD);

			Origin.JA_DGCutOff = today.AddDays(2);
			AssertNoErrors("Should be NO errors", Origin.JA_DGCutOffInfo);

			Origin.JA_DGReceivalCommences = today.AddDays(3);
			AssertHasError("Should be error", Origin.JA_DGCutOffInfo, errorReceivalAfterCutOff);

			Origin.JA_DGReceivalCommences = today;
			AssertNoErrors("Should be NO errors", Origin.JA_DGCutOffInfo);
		}

		#endregion

		#region Testing correct Validation Object

		public void TestTestingCorrectValidationObject()
		{
			AssertEquals(ValidationType, Origin.Validation.GetType());
		}

		#endregion

		#region Implementation

		protected VoyageOrigin Origin;
		protected VoyageOrigin Origin2;
		protected JobSailing Sailing;
		protected JobSailing Sailing2;
		protected VoyageDestination Destination;
		protected JobVoyage Voyage;
		protected JobVoyage Voyage2;

		protected virtual ZString TransportTypeCode
		{
			get { return Constants.TransportModes.Other; }
		}

		protected virtual Type ValidationType
		{
			get { return typeof(BaseJobVoyOriginValidation); }
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Voyage2 = Factory.New<JobVoyage>();

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = TransportTypeCode;

			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUMEL";

			Destination = Voyage.Destinations.AddNew();
			Destination.JB_RL_NKPortOfDischarge = "NLAMS";

			Voyage.GenerateSailings();

			Sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "NLAMS");
			Sailing2 = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUMEL", "NLAMS");
		}

		#endregion
	}
}
