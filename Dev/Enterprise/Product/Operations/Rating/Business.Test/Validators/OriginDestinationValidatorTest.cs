using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;

namespace Enterprise.Rating.Business.Testing
{
	public class OriginDestinationValidatorTest : RatingTestCase
	{
		public void TestZoneValidation_NonRatingZones()
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Reporting;
			zone.FZ_Code = "HELO";
			zone.FZ_Description = "HELO";

			var rate = Factory.New<ClientRate>();
			var airEntry = rate.AddRateEntry("AIR");

			airEntry.TI_OriginLRC = "HELO";
			AssertHasErrors("Reporting zones are not allowed in rating", airEntry.TI_OriginLRCInfo);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			airEntry.TI_OriginLRC = "AUSYD";
			airEntry.TI_OriginLRC = "HELO";
			AssertNoErrors("Reporting zones are not allowed in rating", airEntry.TI_OriginLRCInfo);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Reporting;
			airEntry.TI_OriginLRC = "";
			airEntry.TI_DestinationLRC = "HELO";
			AssertHasErrors("Reporting zones are not allowed in rating", airEntry.TI_DestinationLRCInfo);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			airEntry.TI_DestinationLRC = "AUSYD";
			airEntry.TI_DestinationLRC = "HELO";
			AssertNoErrors("Reporting zones are not allowed in rating", airEntry.TI_DestinationLRCInfo);
		}

		public void TestZoneValidation_ZoneTypeCompatibilityWithRateMode()
		{
			var zoneA = Helper.NewInternationalZone("AACA", null, "AUSYD");
			zoneA.FZ_ZoneMode = Core.Constants.RateMode.ULD;

			var zoneB = Helper.NewInternationalZone("BBCA", null, "AUSYD");
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.LRA;

			var rate = Factory.New<ClientRate>();
			var originLSEEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, zoneA.Code, zoneB.Code);

			AssertHasErrors("This Origin cannot be chosen as the International Zone Type's Mode is incompatible with this Rate Mode.", originLSEEntry.TI_OriginLRCInfo);
			AssertHasErrors("This Destination cannot be chosen as the International Zone Type's Mode is incompatible with this Rate Mode.", originLSEEntry.TI_DestinationLRCInfo);

			zoneA.FZ_ZoneMode = Core.Constants.RateMode.FCL;
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.LCL;
			var originLCLEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, zoneA.Code, zoneB.Code);

			AssertHasErrors("This Origin cannot be chosen as the International Zone Type's Mode is incompatible with this Rate Mode.", originLCLEntry.TI_OriginLRCInfo);
			AssertNoErrors("Zone Type for destination location is Sea Uncontainerized and similar to this Rate Mode", originLCLEntry.TI_DestinationLRCInfo);

			zoneA.FZ_ZoneMode = Core.Constants.RateMode.ULD;
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			var dstEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ULD, zoneA.Code, zoneB.Code);

			AssertNoErrors("Zone Mode for origin location is Air and similar to this rate mode", dstEntry.TI_OriginLRCInfo);
			AssertNoErrors("Zone Mode for destination location is Air and similar to this rate mode", dstEntry.TI_DestinationLRCInfo);

			zoneA.FZ_ZoneMode = Core.Constants.RateMode.FCL;
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.SEA;
			var fclSeaEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, zoneA.Code, zoneB.Code);
			AssertNoErrors("Zone Mode for origin location is FCL and similar to this rate mode", fclSeaEntry.TI_OriginLRCInfo);
			AssertNoErrors("Zone Mode for destination location is SEA and similar to this rate mode", fclSeaEntry.TI_DestinationLRCInfo);

			zoneA.FZ_ZoneMode = Core.Constants.RateMode.FRO;
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.ROA;
			var fclRoadEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.ROA, zoneA.Code, zoneB.Code);
			AssertNoErrors("Zone Mode for origin location is FRO and similar to this rate mode", fclRoadEntry.TI_OriginLRCInfo);
			AssertNoErrors("Zone Mode for destination location is ROA and similar to this rate mode", fclRoadEntry.TI_DestinationLRCInfo);

			zoneA.FZ_ZoneMode = Core.Constants.RateMode.FRA;
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.LCL;
			var fclRailEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.RAI, zoneA.Code, zoneB.Code);
			AssertNoErrors("Zone Mode for origin location is FRA and similar to this rate mode", fclRailEntry.TI_OriginLRCInfo);
			AssertHasErrors("This Origin cannot be chosen as the International Zone Type's Mode is incompatible with this Rate Mode.", fclRailEntry.TI_DestinationLRCInfo);

			zoneA.FZ_ZoneMode = Core.Constants.RateMode.RAI;
			zoneB.FZ_ZoneMode = Core.Constants.RateMode.FRA;
			var originRailEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.RAI, zoneA.Code, zoneB.Code);
			AssertNoErrors("Zone Mode for origin location is RAI and similar to this rate mode", originRailEntry.TI_OriginLRCInfo);
			AssertHasErrors("This Origin cannot be chosen as the International Zone Type's Mode is incompatible with this Rate Mode.", originRailEntry.TI_DestinationLRCInfo);
		}

		public void TestZoneValidation_CarrierZones()
		{
			var carrier = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();

			var zone = Helper.NewInternationalZone("HELO", carrier, "AUSYD");
			var rate = Helper.NewClientRate(NewClient);
			var airEntry = rate.AddRateEntry("AIR");

			airEntry.TI_OriginLRC = "HELO";
			airEntry.TI_OH_TransportProvider = carrier.PK;
			AssertNoErrors("Matching carrier", airEntry.TI_OriginLRCInfo);

			airEntry.TI_OH_TransportProvider = carrier2.PK;
			AssertHasErrors("Non matching carrer", airEntry.TI_OriginLRCInfo);

			airEntry.TI_OH_TransportProvider = ZGuid.Empty;
			AssertHasErrors("Blank carrier on rate", airEntry.TI_OriginLRCInfo);

			zone.FZ_OH_RelatedParty = ZGuid.Empty;
			airEntry.TI_OriginLRC = "";
			airEntry.TI_OriginLRC = "HELO";
			airEntry.TI_OH_TransportProvider = carrier2.PK;
			AssertNoErrors("Blank carrier on zone but not on rate", airEntry.TI_OriginLRCInfo);
		}

		public void TestZoneValidationForCompanyTariffAndStandardCosts()
		{
			var errorMessage = "You have chosen a zone that is specific for an Organization QANTAS.";

			var carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_Code = "QANTAS";

			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_Code = "ABCDE";

			var zone = Helper.NewInternationalZone("EURO", carrier1, "AUSYD");

			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("AIR", "LSE", "EURO", "EURO");
			entry.TI_OH_TransportProvider = carrier1.PK;
			AssertNoErrors("Carrier on the zone matches the carrier on the rate entry.", entry.TI_OriginLRCInfo);
			AssertNoErrors("Carrier on the zone matches the carrier on the rate entry.", entry.TI_DestinationLRCInfo);

			entry.TI_OH_TransportProvider = ZGuid.Empty;
			entry.TI_OH_Supplier = carrier1.PK;
			AssertNoErrors("Carrier on the zone matches the supplier/transport provider on the rate entry.", entry.TI_OriginLRCInfo);
			AssertNoErrors("Carrier on the zone matches the supplier/transport provider on the rate entry.", entry.TI_DestinationLRCInfo);

			entry.TI_OH_Supplier = ZGuid.Empty;
			AssertHasError("Blank carrier and supplier on rate entry.", entry.TI_OriginLRCInfo, errorMessage);
			AssertHasError("Blank carrier and supplier on rate entry.", entry.TI_DestinationLRCInfo, errorMessage);

			entry.TI_OH_Supplier = carrier2.PK;
			AssertHasError("Carrier on the zone does not match the supplier/transport provider on the rate entry.", entry.TI_OriginLRCInfo, errorMessage);
			AssertHasError("Carrier on the zone does not match the supplier/transport provider on the rate entry.", entry.TI_DestinationLRCInfo, errorMessage);

			var standardCost = Factory.New<Costing>();
			entry = standardCost.AddRateEntry("AIR");

			entry.TI_OriginLRC = "EURO";
			entry.TI_DestinationLRC = "EURO";
			entry.TI_OH_Supplier = carrier2.PK;
			AssertHasError("Carrier on the zone does not match the supplier/transport provider on the rate entry.", entry.TI_OriginLRCInfo, errorMessage);
			AssertHasError("Carrier on the zone does not match the supplier/transport provider on the rate entry.", entry.TI_DestinationLRCInfo, errorMessage);

			entry.TI_OH_Supplier = ZGuid.Empty;
			AssertHasError("Blank carrier and supplier on rate entry.", entry.TI_OriginLRCInfo, errorMessage);
			AssertHasError("Blank carrier and supplier on rate entry.", entry.TI_DestinationLRCInfo, errorMessage);

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			entry = clientRate.AddRateEntry("AIR");
			entry.TI_OriginLRC = "EURO";
			entry.TI_DestinationLRC = "EURO";

			entry.TI_OH_Supplier = carrier2.PK;
			AssertHasError(entry.TI_OriginLRCInfo, errorMessage);
			AssertHasError(entry.TI_DestinationLRCInfo, errorMessage);

			entry.TI_OH_Supplier = carrier1.PK;
			AssertNoErrors(entry.TI_OriginLRCInfo);
			AssertNoErrors(entry.TI_DestinationLRCInfo);
		}

		public void TestZoneValidationForOrgProxy()
		{
			var errorMessage = "You have chosen a zone that is specific for an Organisation EDICUS.";

			var anotherOrgProxy = Helper.NewOrgHeader();
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_Code = "ANC";
			anotherCompany.GC_OH_OrgProxy = anotherOrgProxy.PK;
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompany.Branches.Add(anotherBranch);
			Factory.Save();

			var edicusZone = Helper.NewInternationalZone("EDIZ", GlbCompany.CurrentCompany.OrgProxy, "AUSYD");

			var anotherCompanyZone = Helper.NewInternationalZone("ANOT", anotherOrgProxy, "AUSYD");

			var clientRate = Helper.NewGlobalClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "EDIZ", "EDIZ");

			var standardCosting = Helper.NewGlobalCosting(null);
			var costEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "EDIZ", "EDIZ");

			AssertNoErrors(rateEntry.TI_OriginLRCInfo);
			AssertNoErrors(rateEntry.TI_DestinationLRCInfo);
			AssertNoErrors(costEntry.TI_OriginLRCInfo);
			AssertNoErrors(costEntry.TI_DestinationLRCInfo);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "EDIZ", "EDIZ");
				var costEntry2 = standardCosting.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "EDIZ", "EDIZ");

				AssertNoErrors("Should have no error as EDICUS is the publisher of this RateEntry", rateEntry.TI_OriginLRCInfo);
				AssertNoErrors("Should have no error as EDICUS is the publisher of this RateEntry", rateEntry.TI_DestinationLRCInfo);
				AssertNoErrors("Should have no error as EDICUS is the publisher of this RateEntry", costEntry.TI_OriginLRCInfo);
				AssertNoErrors("Should have no error as EDICUS is the publisher of this RateEntry", costEntry.TI_DestinationLRCInfo);

				AssertEquals("Pre-condition: should be published in another company", anotherCompany.PK, rateEntry2.TI_GC_Publisher);
				AssertEquals("Pre-condition: should be published in another company", anotherCompany.PK, costEntry2.TI_GC_Publisher);

				AssertHasError("EDICUS is neither the current OrgProxy nor the OrgProxy of the Publisher", rateEntry2.TI_OriginLRCInfo, errorMessage);
				AssertHasError("EDICUS is neither the current OrgProxy nor the OrgProxy of the Publisher", rateEntry2.TI_DestinationLRCInfo, errorMessage);
				AssertHasError("EDICUS is neither the current OrgProxy nor the OrgProxy of the Publisher", costEntry2.TI_OriginLRCInfo, errorMessage);
				AssertHasError("EDICUS is neither the current OrgProxy nor the OrgProxy of the Publisher", costEntry2.TI_DestinationLRCInfo, errorMessage);

				rateEntry2.TI_OriginLRC = "ANOT";
				rateEntry2.TI_DestinationLRC = "ANOT";
				costEntry2.TI_OriginLRC = "ANOT";
				costEntry2.TI_DestinationLRC = "ANOT";

				AssertNoErrors(rateEntry2.TI_OriginLRCInfo);
				AssertNoErrors(rateEntry2.TI_DestinationLRCInfo);
				AssertNoErrors(costEntry2.TI_OriginLRCInfo);
				AssertNoErrors(costEntry2.TI_DestinationLRCInfo);
			}
		}

		public void TestPortsForTransportModes()
		{
			#region Setup

			var seaPortOnly = Factory.New<RefUNLOCO>();
			seaPortOnly.RL_Code = "ZUBSE";
			seaPortOnly.RL_HasSeaport = true;
			seaPortOnly.RL_HasAirport = false;

			var airPortOnly = Factory.New<RefUNLOCO>();
			airPortOnly.RL_Code = "ZUBAI";
			airPortOnly.RL_HasSeaport = false;
			airPortOnly.RL_HasAirport = true;

			var nonePortOnly = Factory.New<RefUNLOCO>();
			nonePortOnly.RL_Code = "ZUBNO";
			nonePortOnly.RL_HasSeaport = false;
			nonePortOnly.RL_HasAirport = false;

			var bothPortOnly = Factory.New<RefUNLOCO>();
			bothPortOnly.RL_Code = "ZUBBO";
			bothPortOnly.RL_HasSeaport = true;
			bothPortOnly.RL_HasAirport = true;

			var rate = Factory.New<ClientRate>();
			var airEntry = rate.AddRateEntry("AIR");
			var fCLEntry = rate.AddRateEntry("FCL");
			var lCLEntry = rate.AddRateEntry("LCL");
			var airOriginEntry = rate.AddRateEntry("ORG", "AIR", "", "");
			var seaDestEntry = rate.AddRateEntry("DST", "FCL", "", "");
			var allDestEntry = rate.AddRateEntry("DST", "ALL", "", "");

			#endregion

			AssertPortWarningForTransportMode(airEntry, seaPortOnly, "The port ZUBSE is not an Airport so should not be used here.");
			AssertPortWarningForTransportMode(airEntry, airPortOnly, "");
			AssertPortWarningForTransportMode(airEntry, nonePortOnly, "The port ZUBNO is not an Airport so should not be used here.");
			AssertPortWarningForTransportMode(airEntry, bothPortOnly, "");

			AssertPortWarningForTransportMode(airOriginEntry, seaPortOnly, "The port ZUBSE is not an Airport so should not be used here.");
			AssertPortWarningForTransportMode(airOriginEntry, airPortOnly, "");
			AssertPortWarningForTransportMode(airOriginEntry, nonePortOnly, "The port ZUBNO is not an Airport so should not be used here.");
			AssertPortWarningForTransportMode(airOriginEntry, bothPortOnly, "");

			AssertPortWarningForTransportMode(fCLEntry, seaPortOnly, "");
			AssertPortWarningForTransportMode(fCLEntry, airPortOnly, "The port ZUBAI is not a Seaport so should not be used here.");
			AssertPortWarningForTransportMode(fCLEntry, nonePortOnly, "The port ZUBNO is not a Seaport so should not be used here.");
			AssertPortWarningForTransportMode(fCLEntry, bothPortOnly, "");

			AssertPortWarningForTransportMode(lCLEntry, seaPortOnly, "");
			AssertPortWarningForTransportMode(lCLEntry, airPortOnly, "The port ZUBAI is not a Seaport so should not be used here.");
			AssertPortWarningForTransportMode(lCLEntry, nonePortOnly, "The port ZUBNO is not a Seaport so should not be used here.");
			AssertPortWarningForTransportMode(lCLEntry, bothPortOnly, "");

			AssertPortWarningForTransportMode(seaDestEntry, seaPortOnly, "");
			AssertPortWarningForTransportMode(seaDestEntry, airPortOnly, "The port ZUBAI is not a Seaport so should not be used here.");
			AssertPortWarningForTransportMode(seaDestEntry, nonePortOnly, "The port ZUBNO is not a Seaport so should not be used here.");
			AssertPortWarningForTransportMode(seaDestEntry, bothPortOnly, "");

			AssertPortWarningForTransportMode(allDestEntry, seaPortOnly, "");
			AssertPortWarningForTransportMode(allDestEntry, airPortOnly, "");
			AssertPortWarningForTransportMode(allDestEntry, nonePortOnly, "");
			AssertPortWarningForTransportMode(allDestEntry, bothPortOnly, "");
		}

		void AssertPortWarningForTransportMode(RateEntry entry, RefUNLOCO loco, ZString expectedErrorMessage)
		{
			entry.TI_OriginLRC = loco.RL_Code;
			if (expectedErrorMessage.IsEmpty)
			{
				AssertNoWarnings(entry.TI_OriginLRCInfo);
			}
			else
			{
				AssertHasWarning(entry.TI_OriginLRCInfo, expectedErrorMessage);
			}

			entry.TI_DestinationLRC = loco.RL_Code;
			if (expectedErrorMessage.IsEmpty)
			{
				AssertNoWarnings(entry.TI_DestinationLRCInfo);
			}
			else
			{
				AssertHasWarning(entry.TI_DestinationLRCInfo, expectedErrorMessage);
			}
		}

		public void TestOriginPortToDestinationPort()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "AIR", "LSE");
			testEntry.TI_OriginLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "AUMEL";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = "";
			testEntry.TI_DestinationLRC = "";
			testEntry.TI_RateCategory = RatingConstants.RateCategory.TBC;

			Assert(!testEntry.TI_OriginLRCInfo.HasErrors());
			Assert(!testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_TZ_OriginZone = ZGuid.NewZGuid();

			Assert(!testEntry.TI_OriginLRCInfo.HasErrors());
			Assert(!testEntry.TI_DestinationLRCInfo.HasErrors());
		}

		public void TestInactiveOriginAndDestinationPorts()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "AIR", "LSE");
			testEntry.TI_OriginLRC = "ANBON";
			AssertHasErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_DestinationLRC = "ANWIL";
			AssertHasErrors(testEntry.TI_OriginLRCInfo);
			AssertHasErrors(testEntry.TI_DestinationLRCInfo);
		}

		public void TestOriginDestinationBlankOnOriginEntry()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "ORG", "");

			testEntry.TI_OriginLRC = "DEHAM";
			testEntry.TI_DestinationLRC = "DEHAM";
			testEntry.TI_OriginLRC = "";
			testEntry.TI_DestinationLRC = "";
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_DestinationLRC = "AUSYD";
			testEntry.TI_OriginLRC = "";
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_DestinationLRC = "";
			testEntry.TI_OriginLRC = "AUSYD";
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_OriginLRC = "";
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_ViaLRC = "SGSIN";
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_OriginLRC = "AUSYD";
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_ViaLRC = "";
			testEntry.TI_IsCrossTrade = true;
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);

			testEntry.TI_ViaLRC = "SGSIN";
			testEntry.TI_IsCrossTrade = true;
			AssertNoErrors(testEntry.TI_OriginLRCInfo);
			AssertNoErrors(testEntry.TI_DestinationLRCInfo);
		}

		public void TestOriginRegionToDestinationRegion()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "AIR", "LSE");
			testEntry.TI_OriginLRC = "AUEC";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "AUEC";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "USCA";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = "AUEC";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());
		}

		public void TestOriginCountryToDestinationCountry()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "AIR", "LSE");
			testEntry.TI_OriginLRC = "AU";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "AU";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "US";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = "AU";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());
		}

		public void TestOriginNotNull()
		{
			var testEntry = SetupBusinessObject(typeof(Quote), "AIR", "LSE");
			testEntry.TI_OriginLRC = "DEHAM";
			testEntry.TI_OriginLRC = ZString.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
		}

		public void TestDestinationNotNull()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "AIR", "LSE");
			testEntry.TI_DestinationLRC = "DEHAM";
			testEntry.TI_DestinationLRC = ZString.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());
		}

		public void TestPortsNotNullOriginCharges()
		{
			var testEntry = SetupBusinessObject(typeof(Quote), "ORG", "");
			testEntry.TI_OriginLRC = "DEHAM";
			testEntry.TI_OriginLRC = ZString.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = ZString.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());
		}

		public void TestPortsNotNullDestinationCharges()
		{
			var testEntry = SetupBusinessObject(typeof(ClientRate), "DST", "");
			testEntry.TI_DestinationLRC = "DEHAM";
			testEntry.TI_DestinationLRC = ZString.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_DestinationLRC = "AUSYD";
			AssertEquals("Has Errors", false, testEntry.TI_DestinationLRCInfo.HasErrors());

			testEntry.TI_OriginLRC = ZString.Empty;
			AssertEquals("Has Errors", false, testEntry.TI_OriginLRCInfo.HasErrors());
		}

		#region IATA City Code Validation

		public void TestIATACityCodeValidation()
		{
			foreach (var testCase in IATACityCodeTestCases)
			{
				var orgHeader = testCase.HasOrgHeader
					? Factory.NewWithValidTestData<OrgHeader>()
					: null;

				var rateHeader = Helper.NewRatingHeader(testCase.RateType, orgHeader);
				var rateEntry = rateHeader.AddRateEntry(testCase.Category, origin: testCase.Origin, destination: testCase.Destination);
				rateEntry.TI_ViaLRC = testCase.Via;

				if (!string.IsNullOrEmpty(testCase.ExpectedError))
				{
					AssertHasError($"{testCase}: Origin", rateEntry.TI_OriginLRCInfo, testCase.ExpectedError);
					AssertHasError($"{testCase}: Destination", rateEntry.TI_DestinationLRCInfo, testCase.ExpectedError);
					AssertHasError($"{testCase}: Via", rateEntry.TI_ViaLRCInfo, testCase.ExpectedError);
				}
				else
				{
					AssertNoErrors($"{testCase}: Origin", rateEntry.TI_OriginLRCInfo);
					AssertNoErrors($"{testCase}: Destination", rateEntry.TI_DestinationLRCInfo);
					AssertNoErrors($"{testCase}: Via", rateEntry.TI_ViaLRCInfo);
				}
			}
		}

		static readonly IATACityCodeTestCase[] IATACityCodeTestCases =
		{
			// ClientRate
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.AIR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.FCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.LCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.ORG, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.DST, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.AIR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.FCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.LCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.ORG, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.DST, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),

			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.CAI, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.CFC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.CLC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.COR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: false, Category.CDS, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.CAI, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.CFC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.CLC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.COR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.ClientRate, hasOrgHeader: true, Category.CDS, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),

			// Quote
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.AIR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.FCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.LCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.ORG, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.DST, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.AIR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.FCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.LCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.ORG, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.DST, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),

			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.CAI, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.CFC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.CLC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.COR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: false, Category.CDS, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.CAI, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.CFC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.CLC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.COR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Quote, hasOrgHeader: true, Category.CDS, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),

			// Costing
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.AIR, origin: "SYD", destination: "BNE", via: "PER", expectedError: null),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.FCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.LCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.ORG, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.DST, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.AIR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.FCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.LCL, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.ORG, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.DST, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),

			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.CAI, origin: "SYD", destination: "BNE", via: "PER", expectedError: null),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.CFC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.CLC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.COR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: false, Category.CDS, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.CAI, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.CFC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.CLC, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.COR, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
			new IATACityCodeTestCase(RatingConstants.RatingHeaderTypes.Costing, hasOrgHeader: true, Category.CDS, origin: "SYD", destination: "BNE", via: "PER", expectedError: "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates."),
		};

		class IATACityCodeTestCase
		{
			public string RateType { get; }
			public bool HasOrgHeader { get; }
			public string Category { get; }
			public string Origin { get; }
			public string Destination { get; }
			public string Via { get; }

			public string ExpectedError { get; }

			public string Message { get; }

			public override string ToString() => $"Type: {RateType}, hasOrgHeader: {HasOrgHeader}, category: {Category}, {Origin} > {Destination} > {Via}";

			public IATACityCodeTestCase(string rateType, bool hasOrgHeader, string category, string origin, string destination, string via, string expectedError, string message = default)
			{
				RateType = rateType;
				HasOrgHeader = hasOrgHeader;
				Category = category;
				Origin = origin;
				Destination = destination;
				Via = via;
				ExpectedError = expectedError;
				Message = message;
			}
		}

		public void TestIATACityCodeValidation_CostingWhenParentOrganizationChanged_AIR()
		{
			var costing = Factory.New<Costing>();
			var airEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, origin: "SYD");
			AssertNoErrors("Init", airEntry.TI_OriginLRCInfo);

			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertHasError("Set OrgHeader", airEntry.TI_OriginLRCInfo, "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates.");

			costing.TH_OH = ZGuid.Empty;
			AssertNoErrors("Empty OrgHeader", airEntry.TI_OriginLRCInfo);

			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(true, airEntry.TI_OriginLRCInfo.HasErrors());
			AssertHasError("Re-set OrgHeader", airEntry.TI_OriginLRCInfo, "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates.");

			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(true, airEntry.TI_OriginLRCInfo.HasErrors());
			AssertHasError("Re-set OrgHeader again", airEntry.TI_OriginLRCInfo, "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates.");
		}

		public void TestIATACityCodeValidation_CostingWhenParentOrganizationChanged_CAI()
		{
			var costing = Factory.New<Costing>();
			var airEntry = costing.AddRateEntry(RatingConstants.RateCategory.CAI, origin: "SYD");
			AssertNoErrors("Init", airEntry.TI_OriginLRCInfo);

			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertHasError("Set OrgHeader", airEntry.TI_OriginLRCInfo, "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates.");

			costing.TH_OH = ZGuid.Empty;
			AssertNoErrors("Empty OrgHeader", airEntry.TI_OriginLRCInfo);

			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(true, airEntry.TI_OriginLRCInfo.HasErrors());
			AssertHasError("Re-set OrgHeader", airEntry.TI_OriginLRCInfo, "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates.");

			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(true, airEntry.TI_OriginLRCInfo.HasErrors());
			AssertHasError("Re-set OrgHeader again", airEntry.TI_OriginLRCInfo, "IATA City Codes may only be used on the Air Freight tab of Standard Costs/TACT Rates.");
		}

		#endregion

		#region Implementation

		RateEntry SetupBusinessObject(Type typeofRatingHeader, ZString fCL_LCL, ZString mode)
		{
			var header = (RatingHeader)Factory.New(typeofRatingHeader);
			return header.AddRateEntry(fCL_LCL, mode, "", "");
		}

		#endregion
	}
}
