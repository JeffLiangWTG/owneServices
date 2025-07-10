using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyCountryTestCase : BaseAgencyTest
	{
		public void TestCountryName()
		{
			SetSailing();
			AssertEquals("Should show the current country name", GlbCompany.CurrentCompany.Country.RN_Desc, voyageWrapper.CountryName);
		}

		public void TestLoadPrincipalsWithBrokenBranch()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			FindOrCreateSailing(voyage, HomePort, OverseasPort);
			Factory.Save();
			ZString homePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
			try
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				JobVoyage voyageInNewFactory = newFactory.Load<JobVoyage>(voyage.PK);
				AgencyCountry schedule = new AgencyCountry(voyageInNewFactory, new RefCountry.Loader(Factory).LoadForCountry("AU"));
				AssertNotNull("should not throw exception", schedule.Principals);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = homePort;
			}
		}

		public void TestLoadPrincipals()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			OrgHeader principal3 = NewPrincipal();
			OrgHeader principal4 = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "HKHKG";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN");
			origin.VoyageCountry.SlotAllocations.GetAllocation(principal1.PK).SetAspect(AllocationAspectTypes.Tonnes, 50);
			origin.SlotAllocations.GetAllocation(principal2.PK).SetAspect(AllocationAspectTypes.Tonnes, 50);
			sailing.SlotAllocations.GetAllocation(principal3.PK).SetAspect(AllocationAspectTypes.Tonnes, 50);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobVoyage voyageInNewFactory = newFactory.Load<JobVoyage>(voyage.PK);
			AgencyCountry schedule = new AgencyCountry(voyageInNewFactory, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			AssertEquals("Principal1 should be in the collection", true, schedule.Principals.Contains(principal1));
			AssertEquals("Principal2 should be in the collection", true, schedule.Principals.Contains(principal2));
			AssertEquals("Principal3 should be in the collection", true, schedule.Principals.Contains(principal3));
			AssertEquals("Principal4 should not be in the collection", false, schedule.Principals.Contains(principal4));
			AssertEquals("WrappedPrincipals should have the same number of element as Principals", schedule.Principals.Count, schedule.WrappedPrincipals.Count);
			schedule = new AgencyCountry(voyageInNewFactory, new RefCountry.Loader(Factory).LoadForCountry("HK"));
			AssertEquals("Principal1 should not be in the collection", false, schedule.Principals.Contains(principal1));
			AssertEquals("Principal2 should not be in the collection", false, schedule.Principals.Contains(principal2));
			AssertEquals("Principal3 should not be in the collection", false, schedule.Principals.Contains(principal3));
			AssertEquals("Principal4 should not be in the collection", false, schedule.Principals.Contains(principal4));
			AssertEquals("WrappedPrincipals should have the same number of element as Principals", schedule.Principals.Count, schedule.WrappedPrincipals.Count);
		}

		public void TestRemovePrincipals()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			SlotAllocation countryAllocation1 = origin.VoyageCountry.SlotAllocations.GetAllocation(principal1.PK);
			SlotAllocation countryAllocation2 = origin.VoyageCountry.SlotAllocations.GetAllocation(principal2.PK);
			SlotAllocation originAllocation1 = origin.SlotAllocations.GetAllocation(principal1.PK);
			SlotAllocation originAllocation2 = origin.SlotAllocations.GetAllocation(principal2.PK);
			SlotAllocation sailingAllocation1 = sailing.SlotAllocations.GetAllocation(principal1.PK);
			SlotAllocation sailingAllocation2 = sailing.SlotAllocations.GetAllocation(principal2.PK);
			foreach (SlotAllocation allocation in new SlotAllocation[] { countryAllocation1, countryAllocation2, originAllocation1, originAllocation2, sailingAllocation1, sailingAllocation2 })
			{
				allocation.SetAspect(AllocationAspectTypes.Tonnes, 50);
			}

			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.Principals.Remove(principal1);
			AssertEquals("Expecting CountryAllocation1 to be deleted", true, countryAllocation1.IsDeleted);
			AssertEquals("Expecting OriginAllocation1 to be deleted", true, originAllocation1.IsDeleted);
			AssertEquals("Expecting SailingAllocation1 to be deleted", true, sailingAllocation1.IsDeleted);
			AssertEquals("Not expecting CountryAllocation2 to be deleted", false, countryAllocation2.IsDeleted);
			AssertEquals("Not expecting OriginAllocation2 to be deleted", false, originAllocation2.IsDeleted);
			AssertEquals("Not expecting SailingAllocation2 to be deleted", false, sailingAllocation2.IsDeleted);
			AssertEquals("The WrappedPrincipals collection should have the same number of elements as the Principals collection", schedule.Principals.Count, schedule.WrappedPrincipals.Count);
		}

		public void TestAddPrincipals()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			SlotAllocation[] allocations = new SlotAllocation[] { origin.VoyageCountry.SlotAllocations.GetAllocation(ZGuid.Empty), origin.SlotAllocations.GetAllocation(ZGuid.Empty), sailing.SlotAllocations.GetAllocation(ZGuid.Empty) };
			foreach (SlotAllocation allocation in allocations)
			{
				allocation.SetAspect(AllocationAspectTypes.Tonnes, 50);
				allocation.SetAspect(AllocationAspectTypes.PowerPoints, 1);
				allocation.SetAspect(AllocationAspectTypes.TEU, 10);
				allocation.SetAspect(AllocationAspectTypes.Volume, 20);
			}

			origin.SlotAllocations.GetAllocation(principal1.PK).HasChanges = true;
			AgencyCountry schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			schedule.Principals.Add(principal2);
			SlotAllocation countryAllocation = origin.VoyageCountry.SlotAllocations.GetAllocation(principal2.PK);
			SlotAllocation originAllocation = origin.SlotAllocations.GetAllocation(principal2.PK);
			SlotAllocation sailingAllocation = sailing.SlotAllocations.GetAllocation(principal2.PK);
			foreach (SlotAllocation allocation in new SlotAllocation[] { countryAllocation, originAllocation, sailingAllocation })
			{
				AssertEquals("Principal2 just added to the collection Tonnes (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.Tonnes));
				AssertEquals("Principal2 just added to the collection TEU (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.TEU));
				AssertEquals("Principal2 just added to the collection PowerPoints (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.PowerPoints));
				AssertEquals("Principal2 just added to the collection Volume (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.Volume));
			}

			SlotAllocation countryAllocation2 = origin.VoyageCountry.SlotAllocations.GetAllocation(principal1.PK);
			SlotAllocation originAllocation2 = origin.SlotAllocations.GetAllocation(principal1.PK);
			SlotAllocation sailingAllocation2 = sailing.SlotAllocations.GetAllocation(principal1.PK);
			foreach (SlotAllocation allocation in new SlotAllocation[] { countryAllocation2, originAllocation2, sailingAllocation2 })
			{
				AssertEquals("Principal1 already in the collection Tonnes (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.Tonnes));
				AssertEquals("Principal1 already in the collection TEU (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.TEU));
				AssertEquals("Principal1 already in the collection PowerPoints (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.PowerPoints));
				AssertEquals("Principal1 already in the collection Volume (" + allocation.E0_ParentTableCode + ")", 0m, allocation.GetAspect(AllocationAspectTypes.Volume));
			}
		}

		public void TestReadonlynessFromSecuritySettings_True()
		{
			ReadonlynessFromSecuritySettingsTest(true);
		}

		public void TestReadonlynessFromSecuritySettings_False()
		{
			ReadonlynessFromSecuritySettingsTest(false);
		}

		void ReadonlynessFromSecuritySettingsTest(bool allowed)
		{
			Env.Security.SailingScheduleAllocationEdit.IsAllowed = allowed;
			SetSailing();
			CombineAssertions(delegate
			{
				AssertEquals("IsAllowed", allowed, Env.Security.SailingScheduleAllocationEdit.IsAllowed);
				AssertEquals("IsScheduleAllocationEditDenied", !allowed, voyageWrapper.IsScheduleAllocationEditDenied);
				AssertEquals("J0_AllocationMethod", !allowed, voyageWrapper.J0_AllocationMethodInfo.ReadOnly);
				AssertEquals("J0_AllocationsByPrincipal", !allowed, voyageWrapper.J0_AllocationsByPrincipalInfo.ReadOnly);
			});
		}

		public void TestLoadPrincipals_SlotAllocationAspectDBHit()
		{
			var principal1 = NewPrincipal();
			var principal2 = NewPrincipal();
			var principal3 = NewPrincipal();
			var principal4 = NewPrincipal();
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "HKHKG";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "SGSIN");
			origin.VoyageCountry.SlotAllocations.GetAllocation(principal1.PK).SetAspect(AllocationAspectTypes.Tonnes, 50);
			origin.VoyageCountry.SlotAllocations.GetAllocation(principal1.PK).SetAspect(AllocationAspectTypes.TEU, 20);
			origin.SlotAllocations.GetAllocation(principal2.PK).SetAspect(AllocationAspectTypes.Tonnes, 50);
			origin.SlotAllocations.GetAllocation(principal2.PK).SetAspect(AllocationAspectTypes.TEU, 20);
			sailing.SlotAllocations.GetAllocation(principal3.PK).SetAspect(AllocationAspectTypes.Tonnes, 50);
			sailing.SlotAllocations.GetAllocation(principal3.PK).SetAspect(AllocationAspectTypes.TEU, 20);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.ResetDatabaseLoadCount();
			var voyageInNewFactory = newFactory.Load<JobVoyage>(voyage.PK);
			AgencyCountry schedule = new AgencyCountry(voyageInNewFactory, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			AssertEquals("Principal1 should be in the collection", true, schedule.Principals.Contains(principal1));
			AssertEquals("Principal2 should be in the collection", true, schedule.Principals.Contains(principal2));
			AssertEquals("Principal3 should be in the collection", true, schedule.Principals.Contains(principal3));
			AssertEquals("Principal4 should not be in the collection", false, schedule.Principals.Contains(principal4));
			AssertEquals("WrappedPrincipals should have the same number of element as Principals", schedule.Principals.Count, schedule.WrappedPrincipals.Count);
			AssertEquals(2, voyageInNewFactory.Origins.Count);
			AssertEquals(2, voyageInNewFactory.Sailings.Count);
			var newOrigin = voyageInNewFactory.Origins[0];
			var newSailing = voyageInNewFactory.Sailings[0];
			foreach (var aspectType in new string[] { AllocationAspectTypes.Tonnes, AllocationAspectTypes.TEU, AllocationAspectTypes.PowerPoints, AllocationAspectTypes.Volume })
			{
				newOrigin.VoyageCountry.SlotAllocations.GetAllocation(principal1.PK).GetAspect(aspectType);
				newOrigin.SlotAllocations.GetAllocation(principal2.PK).GetAspect(aspectType);
				newSailing.SlotAllocations.GetAllocation(principal3.PK).GetAspect(aspectType);
			}

			AssertMaxDbHits(15, newFactory);
		}

		#region Implementation
		void SetupAgencyShipment()
		{
			voyage = Factory.New<JobVoyage>();
			voyageWrapper = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
			principalWrapper = voyageWrapper.GenericPrincipal;
			voyageWrapper.WrappedPrincipals.Add(principalWrapper);
		}

		void SetSailing()
		{
			SetupAgencyShipment();
			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();
		}

		JobVoyage voyage;
		AgencyPrincipal principalWrapper;
		AgencyCountry voyageWrapper;
		VoyageOrigin origin;
		VoyageDestination destination;
		#endregion
	}
}
