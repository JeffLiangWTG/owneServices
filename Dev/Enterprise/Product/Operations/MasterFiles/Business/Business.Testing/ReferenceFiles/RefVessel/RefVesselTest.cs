using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefVessel))]
	class RefVesselTest : EnterpriseBusinessObjectTestCase
	{
		public void TestInvalidateScreeningStatusesAddLogs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			vessel.InvalidateScreeningStatuses();
			AssertEquals(0, vessel.GetLogs().GetAllLogs().Count);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			vessel.InvalidateScreeningStatuses();

			var latestLog = vessel.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertContains("|NEW=UNK|OLD=MAT|TYP=MAN", latestLog.SL_Reference);
		}

		public void TestInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus()
		{
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.Matched, false, true);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.Matched, true, false);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.PermanentClear, false, false);
			AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(ScreeningStatusesList.Codes.Unknown, false, false);
		}

		void AssertInvalidateScreeningStatusesSetShouldUpdateRelatedJobsAsPerRegistryAndOldScreeningStatus(string screeningStatus, bool registryValue, bool expectedValue)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_ScreeningStatus = screeningStatus;

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals("Precondition: ", false, vessel.ShouldUpdateRelatedJobs);

				vessel.InvalidateScreeningStatuses();
				AssertEquals(expectedValue, vessel.ShouldUpdateRelatedJobs);
			}
		}

		public void TestSystemVessel()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals(OverriddenVessel.PK, OverrideVessel.SystemVessel.PK);
			}
		}

		public void TestRelatedSystemVessel()
		{
			var secondOverriddenVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			secondOverriddenVessel.ZZO_Code = OverriddenVessel.ZZO_Code;
			secondOverriddenVessel.ZZO_LloydsNumber = "2";
			secondOverriddenVessel.ZZO_RadioCallSign = "2";
			secondOverriddenVessel.ZZO_VesselType = "CV";
			secondOverriddenVessel.ZZO_RN_NKCountryOfReg = "ZA";
			secondOverriddenVessel.ZZO_ZZZ_NKDataGrouping = "ZA";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("Related System Vessels found with: " + OverriddenVessel.ZZO_Code, 2, OverrideVessel.RelatedSystemVessels.Length);
				AssertCollectionContains("Finds Overridden Vessel", OverriddenVessel, OverrideVessel.RelatedSystemVessels);
				AssertCollectionContains("Finds Overridden Vessel", secondOverriddenVessel, OverrideVessel.RelatedSystemVessels);
			}
		}

		public void TestHasSystemVessel()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				Assert("HasSystemVessel is false for custom vessel", !CustomVessel.HasSystemVessel);
				Assert("HasSystemVessel is true for custom vessel overriding system vessel", OverrideVessel.HasSystemVessel);
			}
		}

		public void TestRefVesselIsNoLongerACombinedViewWithRefVesselZZ()
		{
			var systemVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			systemVessel.ZZO_LloydsNumber = "LLLL";
			systemVessel.ZZO_RadioCallSign = "RRRR";
			systemVessel.ZZO_RN_NKCountryOfReg = "ZA";
			systemVessel.ZZO_VesselType = "CV";
			systemVessel.ZZO_ZZZ_NKDataGrouping = "ZA";

			var userVessel = Factory.NewWithValidTestData<RefVessel>();
			userVessel.RV_Code = systemVessel.ZZO_Code;
			userVessel.RV_LloydsNumber = ZString.Empty;
			userVessel.RV_RadioCallSign = ZString.Empty;
			userVessel.RV_RN_NKCountryOfReg = ZString.Empty;
			userVessel.RV_VesselType = ZString.Empty;

			Factory.Save();

			var loadedVessel = RefVessel.LookupVesselByCode(systemVessel.ZZO_Code, new BusinessObjectFactory());
			AssertEquals("RV_LloydsNumber not populated from system data", ZString.Empty, loadedVessel.RV_LloydsNumber);
			AssertEquals("RV_RadioCallSign not populated from system data", ZString.Empty, loadedVessel.RV_RadioCallSign);
			AssertEquals("RV_RN_NKCountryOfReg not populated from system data", ZString.Empty, loadedVessel.RV_RN_NKCountryOfReg);
			AssertEquals("RV_VesselType not populated from system data", ZString.Empty, loadedVessel.RV_VesselType);
		}

		#region Active Filter

		public void TestActiveFilter()
		{
			RefVessel activeVessel = Factory.New<RefVessel>();
			activeVessel.RV_Code = "Active Vessel";

			RefVessel inactiveVessel = Factory.New<RefVessel>();
			inactiveVessel.RV_Code = "Inactive Vessel";
			inactiveVessel.RV_IsActive = false;

			RefVessel[] results;
			ZQuery filter = new ZQuery(RefVesselSchema.PK, new ZGuid[] { activeVessel.PK, inactiveVessel.PK });

			results = Factory.Load<RefVessel>(filter);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "Active Vessel" },
				Array.ConvertAll(results, (v) => v.RV_Code.ToString()));

			filter.IgnoreActiveFilter = true;
			results = Factory.Load<RefVessel>(filter);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "Active Vessel", "Inactive Vessel" },
				Array.ConvertAll(results, (v) => v.RV_Code.ToString()));
		}

		#endregion

		#region Lookups

		public void TestLookupVesselByNameIfVesselNameIsExactMatch()
		{
			RefVessel expectedVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, SQLComparisonOperator.Equal, "COLUMBUS OLIVOS"));

			RefVessel unExpectedVessel = Factory.New<RefVessel>();
			unExpectedVessel.RV_Code = "COLUMBUS OLIVOS";
			unExpectedVessel.RV_LloydsNumber = expectedVessel.RV_LloydsNumber;
			unExpectedVessel.RV_IsActive = false;

			AssertEquals("Should have a valid RefVessel obj", expectedVessel.PK, RefVessel.LookupVesselByCode("COLUMBUS OLIVOS", Factory).PK);

			expectedVessel.RV_IsActive = false;
			AssertEquals(null, RefVessel.LookupVesselByCode("COLUMBUS OLIVOS", Factory));

			unExpectedVessel.RV_IsActive = true;
			AssertEquals(unExpectedVessel, RefVessel.LookupVesselByCode("COLUMBUS OLIVOS", Factory));
		}

		public void TestLookupVesselByNameIfVesselNameHasNoMatch()
		{
			AssertEquals("Should be null", null, RefVessel.LookupVesselByCode("SS Yamum", Factory));
		}

		public void TestLookupVesselByNameIfLloydsNoNameIsExactMatch()
		{
			RefVessel expectedVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.Equal, "8811924"));

			RefVessel unExpectedVessel = Factory.New<RefVessel>();
			unExpectedVessel.RV_Code = "HARBLUS MAXIMUS";
			unExpectedVessel.RV_LloydsNumber = expectedVessel.RV_LloydsNumber;
			unExpectedVessel.RV_IsActive = false;

			AssertEquals("Should have a valid RefVessel obj", expectedVessel.PK, RefVessel.LookupVesselByLloyds("8811924", Factory).PK);

			expectedVessel.RV_IsActive = false;
			AssertEquals(null, RefVessel.LookupVesselByLloyds(expectedVessel.RV_LloydsNumber, Factory));

			unExpectedVessel.RV_IsActive = true;
			AssertEquals(unExpectedVessel, RefVessel.LookupVesselByLloyds(expectedVessel.RV_LloydsNumber, Factory));
		}

		public void TestLookupVesselByNameIfLloydsNoNameHasNoMatch()
		{
			AssertEquals("Should be null", null, RefVessel.LookupVesselByLloyds("31337", Factory));
		}

		public void TestLookupVesselByNameAndLloydsNumber()
		{
			const string vesselName = "Vessel001";
			AssertEquals("Finds no Vessels named " + vesselName, 0, RefVessel.LookupVesselsByNameAndLloyds(vesselName, "", Factory).Length);

			var vesselNoLloyds1 = Factory.New<RefVessel>();
			vesselNoLloyds1.RV_Code = vesselName;
			vesselNoLloyds1.RV_LloydsNumber = "";
			var vesselNoLloyds2 = Factory.New<RefVessel>();
			vesselNoLloyds2.RV_Code = vesselName;
			vesselNoLloyds2.RV_LloydsNumber = "";
			var vessel112233 = Factory.New<RefVessel>();
			vessel112233.RV_Code = vesselName;
			vessel112233.RV_LloydsNumber = "112233";

			var vessels = RefVessel.LookupVesselsByNameAndLloyds(vesselName, "", Factory);
			AssertCollectionContains("Finds vesselNoLloyds1", vesselNoLloyds1, vessels);
			AssertCollectionContains("Finds vesselNoLloyds2", vesselNoLloyds2, vessels);
			AssertEquals("Finds Two Vessels named " + vesselName, 2, vessels.Length);

			var vesselWithLloyds112233 = RefVessel.LookupVesselsByNameAndLloyds(vesselName, "112233", Factory);
			AssertCollectionContains("Finds vesselA", vessel112233, vesselWithLloyds112233);
			AssertEquals("Finds Vessel with Lloyds 112233 from all named " + vesselName, 1, vesselWithLloyds112233.Length);

			AssertEquals("Finds No unnamed Vessel with Lloyds 112233", 0, RefVessel.LookupVesselsByNameAndLloyds("", "112233", Factory).Length);
		}

		#endregion

		#region Validation Tests

		public void TestValidateRV_Code()
		{
			Vessel.RV_Code = "";
			Assert("Code is empty, error expected", Vessel.RV_CodeInfo.HasErrors());

			Vessel.RV_Code = "UU";
			Assert("Code is not empty, no error expected", !Vessel.RV_CodeInfo.HasErrors());
		}

		public void TestValidateRV_LloydsNumber()
		{
			Vessel.RV_LloydsNumber = "";
			AssertEquals("Lloyds Number is empty, no error expected", false, Vessel.RV_LloydsNumberInfo.HasErrors());

			Vessel.RV_LloydsNumber = "4837292";
			AssertEquals("Lloyds Number is invalid, message error expected", false, Vessel.RV_LloydsNumberInfo.HasMessageErrors());

			Vessel.RV_LloydsNumber = "4837293";
			AssertEquals("Lloyds Number is valid, no error expected", false, Vessel.RV_LloydsNumberInfo.HasErrors());
		}

		public void TestCustomAttribute1UsedProperties()
		{
			AssertEquals("CustomAttribute1Used", false, Vessel.CustomAttribute1Used);
			FreightDataRegistry.Instance.VesselCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cap1");
			AssertEquals("CustomAttribute1Used", true, Vessel.CustomAttribute1Used);
			AssertEquals("CustomAttribute1Caption", "Cap1", Vessel.CustomAttribute1Caption);
		}

		public void TestCustomAttribute2UsedProperties()
		{
			AssertEquals("CustomAttribute2Used", false, Vessel.CustomAttribute2Used);
			FreightDataRegistry.Instance.VesselCustomAttribute2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cap2");
			AssertEquals("CustomAttribute2Used", true, Vessel.CustomAttribute2Used);
			AssertEquals("CustomAttribute2Caption", "Cap2", Vessel.CustomAttribute2Caption);
		}

		public void TestCustomAttribute3UsedProperties()
		{
			AssertEquals("CustomAttribute3Used", false, Vessel.CustomAttribute3Used);
			FreightDataRegistry.Instance.VesselCustomAttribute3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cap3");
			AssertEquals("CustomAttribute3Used", true, Vessel.CustomAttribute3Used);
			AssertEquals("CustomAttribute3Caption", "Cap3", Vessel.CustomAttribute3Caption);
		}

		public void TestCustomFlag1UsedProperties()
		{
			AssertEquals("CustomFlag1Used", false, Vessel.CustomFlag1Used);
			FreightDataRegistry.Instance.VesselCustomFlag1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cap3");
			AssertEquals("CustomFlag1Used", true, Vessel.CustomFlag1Used);
			AssertEquals("CustomFlag1Caption", "Cap3", Vessel.CustomFlag1Caption);
		}

		public void TestCustomDecimal1UsedProperties()
		{
			AssertEquals("CustomDecimal1Used", false, Vessel.CustomDecimal1Used);
			FreightDataRegistry.Instance.VesselCustomDecimal1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cap3");
			AssertEquals("CustomDecimal1Used", true, Vessel.CustomDecimal1Used);
			AssertEquals("CustomDecimal1Caption", "Cap3", Vessel.CustomDecimal1Caption);
		}

		#endregion

		#region Consortium
		public void TestConsortiumReference()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			AssertEquals("Vessel should not belong to a consortium by default", null, vessel.CarrierConsortium);

			RefCarrierConsortium consortium = Factory.New<RefCarrierConsortium>();
			consortium.RG_Code = "test";
			vessel.RV_RG = consortium.PK;
			Assert("Vessel should now belong to a consortium.", vessel.CarrierConsortium != null);
			AssertEquals("Vessel has incorrect consortium.", "test", vessel.CarrierConsortium.RG_Code);

			vessel.RV_RG = ZGuid.Empty;
			AssertEquals("Vessel's consortium should have been reset by changing the RV_RG property.", null, vessel.CarrierConsortium);
		}
		#endregion

		#region PreventDelete

		public void TestPreventDelete()
		{
			RefVessel vessel = Factory.New<RefVessel>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(RefVessel)));
		}

		#endregion

		#region IScreeningPartyProvider

		public void TestIScreeningPartyProvider()
		{
			OrgHeader shippingCompany = Factory.NewWithValidTestData<OrgHeader>();
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "hello";
			vessel.RV_OH = shippingCompany.PK;

			AssertNotNull(vessel);
			ScreeningParty[] deniedCandidates = ((IScreeningPartyProvider)vessel).ScreeningParties;
			AssertEquals("Shipping Provider", shippingCompany, Array.Find(deniedCandidates, x => x.Description == "Shipping Provider").Header);
			AssertEquals("Vessel itself", vessel, Array.Find(deniedCandidates, x => x.Description == "Vessel").Vessel);
		}

		#endregion

		#region TestRV_ScreeningStatus

		public void TestRV_ScreeningStatus()
		{
			AssertEquals("RV_ScreeningStatus must be readonly", true, Vessel.RV_ScreeningStatusInfo.ReadOnly);
		}

		public void TestShouldUpdateRelatedJobs()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("vessel.RV_ScreeningStatus:", ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);

			vessel.ShouldUpdateRelatedJobs = false;
			vessel.RV_RadioCallSign = "TEST";
			Factory.Save();
			AssertEquals("org.ShouldUpdateRelatedJobs:", true, vessel.ShouldUpdateRelatedJobs);
		}

		public void TestShouldUpdateRelatedJobsIsFalseWhenCurrentStatusIsCLP()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			AssertEquals("vessel.RV_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);

			vessel.ShouldUpdateRelatedJobs = false;
			vessel.RV_RadioCallSign = "TEST";
			Factory.Save();
			AssertEquals("org.ShouldUpdateRelatedJobs:", false, vessel.ShouldUpdateRelatedJobs);
		}

		public void TestDeactivateVesselChangeScreeningStatusToUNK()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("vessel.RV_ScreeningStatus:", ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);

			vessel.RV_IsActive = false;
			Factory.Save();
			AssertEquals("vessel.RV_ScreeningStatus:", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
		}

		public void TestDeactivateVesselNotChangeScreeningStatusToUNKWhenCurrentStatusIsCLP()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();
			AssertEquals("vessel.RV_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);

			vessel.RV_IsActive = false;
			Factory.Save();
			AssertEquals("vessel.RV_ScreeningStatus:", ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);
		}

		public void TestInvalidateScreeningStatusesNotChangeStatusWhenCurrentStatusIsCLP()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			vessel.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, vessel.RV_ScreeningStatus);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			vessel.InvalidateScreeningStatuses();
			AssertEquals(ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
		}

		public void TestRelatedShippingLineScreeningStatusCollection()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_OH = orgHeader.PK;
			var vesselScreenStatus = CreateScreenStatus(vessel.PK, RefVesselSchema.Constants.Prefix);
			(vesselScreenStatus as IBusinessObjectInternals).Row["PJ_SourceTableCode"] = RefVesselSchema.Constants.Prefix;
			(vesselScreenStatus as IBusinessObjectInternals).Row["PJ_SourceID"] = vessel.PK.ToGuid();
			var orgScreenStatus = CreateScreenStatus(orgHeader.PK, OrgHeaderSchema.Constants.Prefix);
			(orgScreenStatus as IBusinessObjectInternals).Row["PJ_SourceTableCode"] = OrgHeaderSchema.Constants.Prefix;
			(orgScreenStatus as IBusinessObjectInternals).Row["PJ_SourceID"] = orgHeader.PK.ToGuid();
			var orgScreenStatusFromJob = CreateScreenStatus(orgHeader.PK, OrgHeaderSchema.Constants.Prefix);
			(orgScreenStatusFromJob as IBusinessObjectInternals).Row["PJ_SourceTableCode"] = JobShipmentSchema.Constants.Prefix;
			(orgScreenStatusFromJob as IBusinessObjectInternals).Row["PJ_SourceID"] = shipment.PK.ToGuid();
			Factory.Save();

			var collection = vessel.RelatedShippingLineScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
			CombineAssertions(() =>
			{
				AssertEquals(3, collection.Count);
				AssertEquals($"{vessel.RV_Code}(Vessel)", collection.Single(u => u.PJ_ParentID == vessel.PK).RelatedOrganization);
				AssertEquals("Shipping provider's logs exists", 2, collection.Count(u => u.PJ_ParentID == orgHeader.PK && u.RelatedOrganization == $"{orgHeader.OH_Code}(Shipping Provider)"));
			});
		}

		IStmEntityScreeningLog CreateScreenStatus(ZGuid parentID, string tablePrefix)
		{
			var status = Factory.New<IStmEntityScreeningLog>();
			var statusRow = (status as IBusinessObjectInternals).Row;
			statusRow["PJ_ParentID"] = parentID.ToGuid();
			statusRow["PJ_ParentTableCode"] = tablePrefix;

			return status;
		}

		public void TestVesselShippingProviderChange_ChangeScreenStatusToUNK()
		{
			var shippingProvider = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			Factory.Save();
			shippingProvider.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals("Precondition", ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);

			vessel.RV_OH = shippingProvider.PK;
			Factory.Save();
			AssertEquals("When shipping provider changed and status not equal", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);

			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			vessel.RV_OH = Guid.Empty;
			Factory.Save();
			AssertEquals("When shipping provider changed to empty", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);

			shippingProvider.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			vessel.RV_OH = shippingProvider.PK;
			Factory.Save();
			AssertEquals("When shipping provider changed and status are equal", ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);

			vessel.RV_OH = ZGuid.NewZGuid();
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals("When shipping provider changed to not exist org", ScreeningStatusesList.Codes.Unknown, vessel.RV_ScreeningStatus);
		}

		#endregion

		#region Implementation

		protected RefVessel Vessel;
		protected RefVesselZZ OverriddenVessel;
		protected RefVessel OverrideVessel;
		protected RefVessel CustomVessel;

		protected override void SetUp()
		{
			base.SetUp();
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Vessel = Factory.New<RefVessel>();
			Vessel.RV_Code = "TESTVESSEL";

			var customZZVessel = Factory.NewWithValidTestData<RefVessel>();
			customZZVessel.RV_LloydsNumber = "1";
			customZZVessel.RV_RadioCallSign = "1";
			customZZVessel.RV_VesselType = "CV";
			customZZVessel.RV_RN_NKCountryOfReg = "ZA";
			OverriddenVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			OverriddenVessel.ZZO_LloydsNumber = "1";
			OverriddenVessel.ZZO_RadioCallSign = "1";
			OverriddenVessel.ZZO_VesselType = "CV";
			OverriddenVessel.ZZO_RN_NKCountryOfReg = "ZA";
			OverriddenVessel.ZZO_ZZZ_NKDataGrouping = "ZA";
			Factory.Save();

			CustomVessel = RefVessel.LookupVesselByCode(customZZVessel.RV_Code, Factory);
			OverrideVessel = Factory.NewWithValidTestData<RefVessel>();
			OverrideVessel.RV_Code = OverriddenVessel.ZZO_Code;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var vessel = factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "DELETE";
			return vessel;
		}

		#endregion
	}
}
