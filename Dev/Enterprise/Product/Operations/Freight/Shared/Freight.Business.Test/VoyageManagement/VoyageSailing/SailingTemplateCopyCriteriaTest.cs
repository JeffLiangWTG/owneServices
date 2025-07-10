using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingTemplateCopyCriteriaTest : BaseFreightTest
	{
		public void TestReferencePortFields()
		{
			ZDateTime now = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			origin1.JA_E_DEP = now.AddDays(1);

			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = AlternateHomePort;
			origin2.JA_E_DEP = now.AddDays(2);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			destination.JB_E_ARV = now.AddDays(3);

			SailingTemplateCopyCriteria criteria;

			criteria = new SailingTemplateCopyCriteria(voyage);
			AssertEquals("ReferencePort", HomePort, criteria.ReferencePort);
			AssertEquals("ReferencePortDate", now.AddDays(1), criteria.ReferencePortDate);
			AssertEquals("ReferencePortLabel", "Departs:", criteria.ReferencePortLabel);

			origin1.JA_E_DEP = ZDateTime.Empty;
			criteria = new SailingTemplateCopyCriteria(voyage);
			AssertEquals("ReferencePort", AlternateHomePort, criteria.ReferencePort);
			AssertEquals("ReferencePortDate", now.AddDays(2), criteria.ReferencePortDate);
			AssertEquals("ReferencePortLabel", "Departs:", criteria.ReferencePortLabel);

			origin2.JA_E_DEP = ZDateTime.Empty;
			criteria = new SailingTemplateCopyCriteria(voyage);
			AssertEquals("ReferencePort", OverseasPort, criteria.ReferencePort);
			AssertEquals("ReferencePortDate", now.AddDays(3), criteria.ReferencePortDate);
			AssertEquals("ReferencePortLabel", "Arrives:", criteria.ReferencePortLabel);
		}

		public void TestValidateAdjustedDate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = HomePort;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.GenerateSailings();

			SailingTemplateCopyCriteria criteria = new SailingTemplateCopyCriteria(voyage);

			criteria.AdjustedDate = ZDateTime.Invalid;
			AssertHasErrors(criteria.AdjustedDateInfo);

			criteria.AdjustedDate = ZDateTime.Now;
			AssertNoErrors(criteria.AdjustedDateInfo);

			criteria.AdjustedDate = ZDateTime.Empty;
			AssertHasErrors(criteria.AdjustedDateInfo);
		}

		public void TestNewAddedVoyageDates()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			origin.JA_E_DEP = new ZDateTime(2012, 04, 05, 08, 56, 22);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_E_ARV = new ZDateTime(2012, 04, 06, 06, 33, 22);
			destination.JB_RL_NKPortOfDischarge = OverseasPort;

			SailingTemplateCopyCriteria criteria = new SailingTemplateCopyCriteria(voyage);
			criteria.AdjustedDate = new ZDateTime(2012, 10, 03);

			VoyageOrigin originCopy = criteria.GenerateCopy().Origins.GetOriginFromLoading(origin.JA_RL_NKPortOfLoading);
			VoyageDestination destinationCopy = criteria.GenerateCopy().Destinations.GetDestinationFromDischarge(destination.JB_RL_NKPortOfDischarge);

			AssertEquals("New voyage origin date will be 03/10/2012", new ZDateTime(2012, 10, 03), originCopy.JA_E_DEP);
			AssertEquals("New voyage destination date will be 04/10/2012", new ZDateTime(2012, 10, 04), destinationCopy.JB_E_ARV);

			criteria = new SailingTemplateCopyCriteria(voyage);
			criteria.AdjustedDate = new ZDateTime(2012, 10, 06, 22, 23, 12);

			originCopy = criteria.GenerateCopy().Origins.GetOriginFromLoading(origin.JA_RL_NKPortOfLoading);
			destinationCopy = criteria.GenerateCopy().Destinations.GetDestinationFromDischarge(destination.JB_RL_NKPortOfDischarge);

			AssertEquals("New voyage origin date will be 06/10/2012", new ZDateTime(2012, 10, 06), originCopy.JA_E_DEP);
			AssertEquals("New voyage destination date will be 07/10/2012", new ZDateTime(2012, 10, 07), destinationCopy.JB_E_ARV);
		}

		public void TestDatesEqual()
		{
			var now = ZDateTime.Now;
			var voyage = Factory.New<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = new ZString("CNSGH");
			origin1.JA_E_DEP = now.AddDays(1);

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = new ZString("AUSYD");
			origin2.JA_E_DEP = now.AddDays(2);

			var origin3 = voyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = new ZString("NZAKL");
			origin3.JA_E_DEP = now.AddDays(3);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = new ZString("NZAKL");
			destination1.JB_E_ARV = now.AddDays(4);

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = new ZString("AUSYD");
			destination2.JB_E_ARV = now.AddDays(5);

			var criteria = new SailingTemplateCopyCriteria(voyage) { AdjustedDate = now.AddDays(10) };

			var voyageCopy = criteria.GenerateCopy();

			AssertEquals("VoyageOrigin1PortDate", voyageCopy.Origins[0].JA_E_DEP.Date, criteria.AdjustedDate.Date);

			AssertNotEquals("VoyageOrigin2PortDate", voyageCopy.Origins[1].JA_E_DEP.Date, criteria.AdjustedDate.Date);
			AssertNotEquals("VoyageOrigin3PortDate", voyageCopy.Origins[2].JA_E_DEP.Date, criteria.AdjustedDate.Date);
			AssertNotEquals("VoyageDestination1PortDate", voyageCopy.Destinations[0].JB_E_ARV.Date, criteria.AdjustedDate.Date);
			AssertNotEquals("VoyageDestination2PortDate", voyageCopy.Destinations[1].JB_E_ARV.Date, criteria.AdjustedDate.Date);
		}

		public void TestDatesDifference()
		{
			var now = ZDateTime.Now;
			var voyage = Factory.New<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = new ZString("CNSGH");
			origin1.JA_E_DEP = now.AddDays(1);

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = new ZString("AUSYD");
			origin2.JA_E_DEP = now.AddDays(2);

			var origin3 = voyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = new ZString("NZAKL");
			origin3.JA_E_DEP = now.AddDays(3);

			var destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = new ZString("NZAKL");
			destination1.JB_E_ARV = now.AddDays(4);

			var destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = new ZString("AUSYD");
			destination2.JB_E_ARV = now.AddDays(5);

			var criteria = new SailingTemplateCopyCriteria(voyage) { AdjustedDate = now.AddDays(10) };

			var voyageCopy = criteria.GenerateCopy();

			AssertEquals("VoyageOrigin1PortDate",
				voyageCopy.Origins[0].JA_E_DEP
					.AddDays(-(criteria.AdjustedDate.Date - criteria.ReferencePortDate.Date).Days).Date,
				voyage.Origins[0].JA_E_DEP.Date);

			AssertEquals("VoyageOrigin2PortDate",
				voyageCopy.Origins[1].JA_E_DEP
					.AddDays(-(criteria.AdjustedDate.Date - criteria.ReferencePortDate.Date).Days).Date,
				voyage.Origins[1].JA_E_DEP.Date);

			AssertEquals("VoyageOrigin3PortDate",
				voyageCopy.Origins[2].JA_E_DEP
					.AddDays(-(criteria.AdjustedDate.Date - criteria.ReferencePortDate.Date).Days).Date,
				voyage.Origins[2].JA_E_DEP.Date);

			AssertEquals("VoyageDestination1PortDate",
				voyageCopy.Destinations[0].JB_E_ARV
					.AddDays(-(criteria.AdjustedDate.Date - criteria.ReferencePortDate.Date).Days).Date,
				voyage.Destinations[0].JB_E_ARV.Date);

			AssertEquals("VoyageDestination2PortDate",
				voyageCopy.Destinations[1].JB_E_ARV
					.AddDays(-(criteria.AdjustedDate.Date - criteria.ReferencePortDate.Date).Days).Date,
				voyage.Destinations[1].JB_E_ARV.Date);
		}

		public void TestGenerateCopy_DontRetainVessel()
		{
			JobVoyage voyageOriginal = GeneratePopulatedVoyage();
			SailingTemplateCopyCriteria criteria = new SailingTemplateCopyCriteria(voyageOriginal);
			criteria.AdjustedDate = criteria.ReferencePortDate.AddDays(60);
			criteria.RetainVessel = false;

			AssertCommonVoyageDetails(voyageOriginal, criteria.GenerateCopy(), 60, false);
		}

		public void TestGenerateCopy_RetainVessel()
		{
			JobVoyage voyageOriginal = GeneratePopulatedVoyage();
			SailingTemplateCopyCriteria criteria = new SailingTemplateCopyCriteria(voyageOriginal);
			criteria.AdjustedDate = criteria.ReferencePortDate.AddDays(70);
			criteria.RetainVessel = true;

			AssertCommonVoyageDetails(voyageOriginal, criteria.GenerateCopy(), 70, true);
		}

		#region Implementation

		JobVoyage GeneratePopulatedVoyage()
		{
			int seed = 0;
			ZDateTime today = ZDateTime.Now;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = TestVessel1.RV_FK;
			voyage.JV_VoyageFlight = "blah";
			voyage.JV_OH_Line = ShippingCompany1.PK;

			VoyageCountry country1 = voyage.Countries.GetCountry("AU", true);
			country1.J0_AllocationMethod = AllocationMethodList.Codes.Country;
			SetupAllocations(country1.SlotAllocations, seed++);

			VoyageCountry country2 = voyage.Countries.GetCountry("SG", true);
			country2.J0_AllocationMethod = AllocationMethodList.Codes.Origin;
			SetupAllocations(country2.SlotAllocations, seed++);

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = HomePort;
			origin1.JA_ReceivalCommences = today.AddDays(1);
			origin1.JA_CutOff = today.AddDays(2);
			origin1.JA_DGReceivalCommences = today.AddDays(3);
			origin1.JA_DGCutOff = today.AddDays(4);
			origin1.JA_E_DEP = today.AddDays(5);
			origin1.JA_A_DEP = today.AddDays(6);
			origin1.JA_DocumentaryCutoff = today.AddDays(7);
			origin1.JA_VGMCutOff = today.AddDays(8);
			SetupAllocations(country1.SlotAllocations, seed++);

			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = AlternateHomePort;
			origin2.JA_ReceivalCommences = today.AddDays(8);
			origin2.JA_CutOff = today.AddDays(9);
			origin2.JA_DGReceivalCommences = today.AddDays(10);
			origin2.JA_DGCutOff = today.AddDays(11);
			origin2.JA_E_DEP = today.AddDays(12);
			origin2.JA_A_DEP = today.AddDays(13);
			origin2.JA_DocumentaryCutoff = today.AddDays(14);
			origin2.JA_VGMCutOff = today.AddDays(15);
			SetupAllocations(country2.SlotAllocations, seed++);

			VoyageOrigin origin3 = voyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = AlternateHomePort2;

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = OverseasPort;
			destination1.JB_E_ARV = today.AddDays(15);
			destination1.JB_A_ARV = today.AddDays(16);
			destination1.JB_AvailabilityDate = today.AddDays(17);
			destination1.JB_StorageDate = today.AddDays(18);

			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = OverseasPort2;
			destination2.JB_E_ARV = today.AddDays(19);
			destination2.JB_A_ARV = today.AddDays(20);
			destination2.JB_AvailabilityDate = today.AddDays(21);
			destination2.JB_StorageDate = today.AddDays(22);

			VoyageDestination destination3 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = OverseasPort3;

			voyage.GenerateSailings();
			foreach (JobSailing sailing in voyage.Sailings)
			{
				sailing.JX_ReservedMasterBill = sailing.JX_JA_RL_NKPortOfLoading + sailing.JX_JB_RL_NKPortOfDischarge;
				SetupAllocations(sailing.SlotAllocations, seed++);
			}

			return voyage;
		}

		void SetupAllocations(SlotAllocationDependentCollection allocations, int seed)
		{
			SlotAllocation allocation1 = allocations.GetAllocation(ZGuid.Empty);
			SlotAllocation allocation2 = allocations.GetAllocation(ShippingCompany1.PK);

			for (int i = 0; i < 4; i++)
			{
				allocation1.SetAspect("AS" + i, seed + i + 1);
				allocation2.SetAspect("AS" + i, seed + i + 11);
			}
		}

		void AssertCommonVoyageDetails(JobVoyage voyageOriginal, JobVoyage voyageCopy, int daysToAdd, bool retainVesselDetails)
		{
			AssertNotEquals(voyageOriginal.PK, voyageCopy.PK);

			AssertEquals("Origin count", voyageOriginal.Origins.Count, voyageCopy.Origins.Count);
			AssertEquals("Destination count", voyageOriginal.Destinations.Count, voyageCopy.Destinations.Count);
			AssertEquals("Sailings count", voyageOriginal.Sailings.Count, voyageCopy.Sailings.Count);
			AssertEquals("Voyage", "", voyageCopy.JV_VoyageFlight);

			if (retainVesselDetails)
			{
				AssertEquals("Vessel", voyageOriginal.JV_RV_NKVessel, voyageCopy.JV_RV_NKVessel);
				AssertEquals("Carrier", voyageOriginal.JV_OH_Line, voyageCopy.JV_OH_Line);
				AssertEquals("Country count", voyageOriginal.Countries.Count, voyageCopy.Countries.Count);

				foreach (VoyageCountry countryOriginal in voyageOriginal.Countries)
				{
					VoyageCountry countryCopy = voyageCopy.Countries.GetCountry(countryOriginal.J0_RN_NKCountry, false);
					AssertNotNull("Country not found (" + countryOriginal.J0_RN_NKCountry + ")", countryCopy);
					AssertCommonCountryDetails(countryOriginal, countryCopy);
				}
			}
			else
			{
				AssertEquals("Vessel", "", voyageCopy.JV_RV_NKVessel);
				AssertEquals("Carrier", ZGuid.Empty, voyageCopy.JV_OH_Line);
				AssertEquals("Country count", 0, voyageCopy.Countries.Count);
			}

			foreach (VoyageOrigin originOriginal in voyageOriginal.Origins)
			{
				VoyageOrigin originCopy = voyageCopy.Origins.GetOriginFromLoading(originOriginal.JA_RL_NKPortOfLoading);
				AssertNotNull("Origin not found (" + originOriginal.JA_RL_NKPortOfLoading + ")", originCopy);
				AssertCommonOriginDetails(originOriginal, originCopy, daysToAdd, retainVesselDetails);
			}

			foreach (VoyageDestination destinationOriginal in voyageOriginal.Destinations)
			{
				VoyageDestination destinationCopy = voyageCopy.Destinations.GetDestinationFromDischarge(destinationOriginal.JB_RL_NKPortOfDischarge);
				AssertNotNull("Destination not found (" + destinationOriginal.JB_RL_NKPortOfDischarge + ")", destinationCopy);
				AssertCommonDestinationDetails(destinationOriginal, destinationCopy, daysToAdd, retainVesselDetails);
			}

			foreach (JobSailing sailingOriginal in voyageOriginal.Sailings)
			{
				JobSailing sailingCopy = voyageCopy.Sailings.GetSailingFromLoadAndDischarge(sailingOriginal.JX_JA_RL_NKPortOfLoading, sailingOriginal.JX_JB_RL_NKPortOfDischarge);
				AssertNotNull("Sailing not found (" + sailingOriginal.JX_JA_RL_NKPortOfLoading + "-" + sailingOriginal.JX_JB_RL_NKPortOfDischarge + ")", sailingCopy);
				AssertCommonSailingDetails(sailingOriginal, sailingCopy, retainVesselDetails);
			}
		}

		void AssertCommonCountryDetails(VoyageCountry countryOriginal, VoyageCountry countryCopy)
		{
			AssertNotEquals(countryOriginal.PK, countryCopy.PK);

			AssertEquals("Country code", countryOriginal.J0_RN_NKCountry, countryCopy.J0_RN_NKCountry);
			AssertEquals("Allocation Method", countryOriginal.J0_AllocationMethod, countryCopy.J0_AllocationMethod);

			AssertAllocations(countryOriginal.SlotAllocations, countryCopy.SlotAllocations);
		}

		void AssertCommonOriginDetails(VoyageOrigin originOriginal, VoyageOrigin originCopy, int daysToAdd, bool retainVesselDetails)
		{
			AssertNotEquals(originOriginal.PK, originCopy.PK);

			AssertEquals("Load Port", originOriginal.JA_RL_NKPortOfLoading, originCopy.JA_RL_NKPortOfLoading);
			AssertEquals("ATD", ZDateTime.Empty, originCopy.JA_A_DEP);
			AssertEquals("FCL ReceivalCommences", AddDaysSafe(originOriginal.JA_ReceivalCommences.Date, daysToAdd), originCopy.JA_ReceivalCommences);
			AssertEquals("FCL CutOff", AddDaysSafe(originOriginal.JA_CutOff.Date, daysToAdd), originCopy.JA_CutOff);
			AssertEquals("HAZ ReceivalCommences", AddDaysSafe(originOriginal.JA_DGReceivalCommences.Date, daysToAdd), originCopy.JA_DGReceivalCommences);
			AssertEquals("HAZ CutOff", AddDaysSafe(originOriginal.JA_DGCutOff.Date, daysToAdd), originCopy.JA_DGCutOff);
			AssertEquals("Documentary CutOff", AddDaysSafe(originOriginal.JA_DocumentaryCutoff.Date, daysToAdd), originCopy.JA_DocumentaryCutoff);
			AssertEquals("VGM CutOff", AddDaysSafe(originOriginal.JA_VGMCutOff.Date, daysToAdd), originCopy.JA_VGMCutOff);

			if (retainVesselDetails)
			{
				AssertAllocations(originOriginal.SlotAllocations, originCopy.SlotAllocations);
			}
		}

		void AssertCommonDestinationDetails(VoyageDestination destinationOriginal, VoyageDestination destinationCopy, int daysToAdd, bool retainVesselDetails)
		{
			AssertNotEquals(destinationOriginal.PK, destinationCopy.PK);

			AssertEquals("Discharge Port", destinationOriginal.JB_RL_NKPortOfDischarge, destinationCopy.JB_RL_NKPortOfDischarge);
			AssertEquals("ETA", AddDaysSafe(destinationOriginal.JB_E_ARV.Date, daysToAdd), destinationCopy.JB_E_ARV);
			AssertEquals("ATA", ZDateTime.Empty, destinationCopy.JB_A_ARV);
			AssertEquals("FCL AvailabilityDate", AddDaysSafe(destinationOriginal.JB_AvailabilityDate.Date, daysToAdd), destinationCopy.JB_AvailabilityDate);
			AssertEquals("FCL StorageDate", AddDaysSafe(destinationOriginal.JB_StorageDate.Date, daysToAdd), destinationCopy.JB_StorageDate);
		}

		void AssertCommonSailingDetails(JobSailing sailingOriginal, JobSailing sailingCopy, bool retainVesselDetails)
		{
			AssertNotEquals(sailingOriginal.PK, sailingCopy.PK);

			AssertEquals("Load Port", sailingOriginal.JX_JA_RL_NKPortOfLoading, sailingCopy.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Discharge Port", sailingOriginal.JX_JB_RL_NKPortOfDischarge, sailingCopy.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("Booking Ref", "", sailingCopy.JX_ReservedMasterBill);
			AssertEquals("CFS Receival", ZDateTime.Empty, sailingCopy.JX_DepotReceivalCommences);
			AssertEquals("CFS CutOff", ZDateTime.Empty, sailingCopy.JX_DepotCutOff);
			AssertEquals("CFS AvailabilityDate", ZDateTime.Empty, sailingCopy.JX_DepotAvailabilityDate);
			AssertEquals("CFS Storage", ZDateTime.Empty, sailingCopy.JX_DepotStorageDate);

			if (retainVesselDetails)
			{
				AssertAllocations(sailingOriginal.SlotAllocations, sailingCopy.SlotAllocations);
			}
		}

		void AssertAllocations(SlotAllocationDependentCollection originalAllocations, SlotAllocationDependentCollection copyAllocations)
		{
			AssertEquals(originalAllocations.Count, copyAllocations.Count);

			CombineAssertions(delegate
			{
				foreach (SlotAllocation allocationOriginal in originalAllocations)
				{
					string prefix = allocationOriginal.HumanReadableName + ": ";

					SlotAllocation allocationCopy = copyAllocations.GetAllocation(allocationOriginal.E0_OH_Principal);

					for (int i = 0; i < 4; i++)
					{
						string code = "AS" + i;
						AssertEquals(prefix + code, allocationOriginal.GetAspect(code), allocationCopy.GetAspect(code));
					}
				}
			});
		}

		ZDateTime AddDaysSafe(ZDateTime startDate, int days)
		{
			return startDate.IsEmpty ? ZDateTime.Empty : startDate.AddDays(days);
		}

		#endregion
	}
}
