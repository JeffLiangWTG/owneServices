using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefZoneHeader))]
	sealed class RefZoneHeaderTest : EnterpriseBusinessObjectTestCase
	{
		[StressTest]
		public void TestLimitOverlappingZonesMessage()
		{
			var allUNLOCOs = Factory.Load<RefUNLOCO>(new ZQuery());
			allUNLOCOs.Length.Should().BeGreaterOrEqualTo(30);

			var zone1 = Factory.New<RefZoneHeader>();
			zone1.FZ_Code = "ZON1";
			zone1.FZ_Description = "Zone 1";
			zone1.FZ_ZoneType = "RAT";
			zone1.FZ_ZoneMode = "AIR";

			var zone2 = Factory.New<RefZoneHeader>();
			zone2.FZ_Code = "ZON2";
			zone2.FZ_Description = "Zone 2";
			zone2.FZ_ZoneType = "RAT";
			zone2.FZ_ZoneMode = "AIR";

			var firstUNLOCOBatch = allUNLOCOs.Take(10);
			foreach (var unloco in firstUNLOCOBatch)
			{
				zone1.UNLOCOs.Add(unloco);
				zone2.UNLOCOs.Add(unloco);
			}

			Factory.Save();

			var message = zone1.GetOverlappingZonesMessage();
			message.Length.Should().BeLessThan(RefZoneHeader.MAGIC_MESSAGE_LENGTH_CUT);
			message.Should().NotEndWith(@"...
It may result in overlapping Rates or Charges to be loaded to the relevant Job when Autorating.");

			// The following setup will add up lines to the message without being cut like:
			// "'UNLOCO' is included in overlapping Rating International Zones: 'ZON1', 'ZON2'." x30 times for 30 different UNLOCOs
			// It would be around 2400 characters.
			var secondUNLOCOBatch = allUNLOCOs.Skip(10).Take(20);
			foreach (var unloco in secondUNLOCOBatch)
			{
				zone1.UNLOCOs.Add(unloco);
				zone2.UNLOCOs.Add(unloco);
			}

			Factory.Save();

			message = zone1.GetOverlappingZonesMessage();
			message.Length.Should().BeGreaterThan(RefZoneHeader.MAGIC_MESSAGE_LENGTH_CUT);
			message.Should().EndWith(@"...
It may result in overlapping Rates or Charges to be loaded to the relevant Job when Autorating.");

			Assert("FluentAssertions is used", condition: true);
		}

		public void TestGetOverlappingZonesMesage_Contracts()
		{
			var zoneZON1 = CreateZoneAndAddLocations(zoneCode: "ZON1", zoneType: "CON", zoneMode: "AIR", relatedParty: ZGuid.Empty, "AUSYD", "AUMEL", "AUBNE");
			var zoneZON2 = CreateZoneAndAddLocations(zoneCode: "ZON2", zoneType: "CON", zoneMode: "AIR", relatedParty: ZGuid.Empty, "NZAKL", "NZROT", "NZWLG");
			var zoneZON3 = CreateZoneAndAddLocations(zoneCode: "ZON3", zoneType: "CON", zoneMode: "AIR", relatedParty: ZGuid.Empty, "USDUL", "USIAD", "USNYC", "USPHL");

			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON1, null);
			AssertOverlappingZonesMessageForContractZones(zoneZON2, null);
			AssertOverlappingZonesMessageForContractZones(zoneZON3, null);

			AddLocationToZone(zoneZON1, "NZAKL");
			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON1, new[] { ("NZAKL", new[] { "ZON1", "ZON2" }) });
			AssertOverlappingZonesMessageForContractZones(zoneZON2, new[] { ("NZAKL", new[] { "ZON1", "ZON2" }) });
			AssertOverlappingZonesMessageForContractZones(zoneZON3, null);

			AddLocationToZone(zoneZON2, "USDUL");
			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON1, new[] { ("NZAKL", new[] { "ZON1", "ZON2" }) });
			AssertOverlappingZonesMessageForContractZones(
				zoneZON2,
				new[]
				{
					("NZAKL", new[] { "ZON1", "ZON2" }),
					("USDUL", new[] { "ZON2", "ZON3" })
				});
			AssertOverlappingZonesMessageForContractZones(zoneZON3, new[] { ("USDUL", new[] { "ZON2", "ZON3" }) });

			AddLocationToZone(zoneZON3, "NZAKL");
			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON1, new[] { ("NZAKL", new[] { "ZON1", "ZON2", "ZON3" }) });
			AssertOverlappingZonesMessageForContractZones(
				zoneZON2,
				new[]
				{
					("NZAKL", new[] { "ZON1", "ZON2", "ZON3" }),
					("USDUL", new[] { "ZON2", "ZON3" })
				});
			AssertOverlappingZonesMessageForContractZones(
				zoneZON3,
				new[]
				{
					("NZAKL", new[] { "ZON1", "ZON2", "ZON3" }),
					("USDUL", new[] { "ZON2", "ZON3" })
				});

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			// different zone mode
			var zoneZON4 = CreateZoneAndAddLocations(zoneCode: "ZON4", zoneType: "CON", zoneMode: "FCL", relatedParty: ZGuid.Empty, "USDUL");
			// different related party
			var zoneZON5 = CreateZoneAndAddLocations(zoneCode: "ZON5", zoneType: "CON", zoneMode: "FCL", relatedParty: orgHeader.PK, "USDUL");
			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON4, null);
			AssertOverlappingZonesMessageForContractZones(zoneZON5, null);

			zoneZON2.FZ_ZoneMode = "FCL";
			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON2, new[] { ("USDUL", new[] { "ZON2", "ZON4" }) });
			AssertOverlappingZonesMessageForContractZones(zoneZON4, new[] { ("USDUL", new[] { "ZON2", "ZON4" }) });
			AssertOverlappingZonesMessageForContractZones(zoneZON5, null);

			zoneZON2.FZ_OH_RelatedParty = orgHeader.PK;
			Factory.Save();
			AssertOverlappingZonesMessageForContractZones(zoneZON2, new[] { ("USDUL", new[] { "ZON2", "ZON5" }) });
			AssertOverlappingZonesMessageForContractZones(zoneZON4, null);
			AssertOverlappingZonesMessageForContractZones(zoneZON5, new[] { ("USDUL", new[] { "ZON2", "ZON5" }) });
		}

		public void TestGetOverlappingZonesMessage()
		{
			var zoneZON1 = CreateZoneAndAddLocations(zoneCode: "ZON1", zoneType: "RAT", zoneMode: "AIR", relatedParty: ZGuid.Empty, "AUSYD", "AUMEL", "AUBNE");
			var zoneZON2 = CreateZoneAndAddLocations(zoneCode: "ZON2", zoneType: "RAT", zoneMode: "AIR", relatedParty: ZGuid.Empty, "NZAKL", "NZROT", "NZWLG");
			var zoneZON3 = CreateZoneAndAddLocations(zoneCode: "ZON3", zoneType: "RAT", zoneMode: "AIR", relatedParty: ZGuid.Empty, "USDUL", "USIAD", "USNYC", "USPHL");

			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON1, null);
			AssertOverlappingZonesMessage(zoneZON2, null);
			AssertOverlappingZonesMessage(zoneZON3, null);

			AddLocationToZone(zoneZON1, "NZAKL");
			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON1, new[] { ("NZAKL", new[] { "ZON1", "ZON2" }) });
			AssertOverlappingZonesMessage(zoneZON2, new[] { ("NZAKL", new[] { "ZON1", "ZON2" }) });
			AssertOverlappingZonesMessage(zoneZON3, null);

			AddLocationToZone(zoneZON2, "USDUL");
			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON1, new[] { ("NZAKL", new[] { "ZON1", "ZON2" }) });
			AssertOverlappingZonesMessage(
				zoneZON2,
				new[]
				{
					("NZAKL", new[] { "ZON1", "ZON2" }),
					("USDUL", new[] { "ZON2", "ZON3" })
				});
			AssertOverlappingZonesMessage(zoneZON3, new[] { ("USDUL", new[] { "ZON2", "ZON3" }) });

			AddLocationToZone(zoneZON3, "NZAKL");
			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON1, new[] { ("NZAKL", new[] { "ZON1", "ZON2", "ZON3" }) });
			AssertOverlappingZonesMessage(
				zoneZON2,
				new[]
				{
					("NZAKL", new[] { "ZON1", "ZON2", "ZON3" }),
					("USDUL", new[] { "ZON2", "ZON3" })
				});
			AssertOverlappingZonesMessage(
				zoneZON3,
				new[]
				{
					("NZAKL", new[] { "ZON1", "ZON2", "ZON3" }),
					("USDUL", new[] { "ZON2", "ZON3" })
				});

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			// different zone mode
			var zoneZON4 = CreateZoneAndAddLocations(zoneCode: "ZON4", zoneType: "RAT", zoneMode: "FCL", relatedParty: ZGuid.Empty, "USDUL");
			// different related party
			var zoneZON5 = CreateZoneAndAddLocations(zoneCode: "ZON5", zoneType: "RAT", zoneMode: "FCL", relatedParty: orgHeader.PK, "USDUL");
			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON4, null);
			AssertOverlappingZonesMessage(zoneZON5, null);

			zoneZON2.FZ_ZoneMode = "FCL";
			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON2, new[] { ("USDUL", new[] { "ZON2", "ZON4" }) });
			AssertOverlappingZonesMessage(zoneZON4, new[] { ("USDUL", new[] { "ZON2", "ZON4" }) });
			AssertOverlappingZonesMessage(zoneZON5, null);

			zoneZON2.FZ_OH_RelatedParty = orgHeader.PK;
			Factory.Save();
			AssertOverlappingZonesMessage(zoneZON2, new[] { ("USDUL", new[] { "ZON2", "ZON5" }) });
			AssertOverlappingZonesMessage(zoneZON4, null);
			AssertOverlappingZonesMessage(zoneZON5, new[] { ("USDUL", new[] { "ZON2", "ZON5" }) });
		}

		RefZoneHeader CreateZoneAndAddLocations(ZString zoneCode, ZString zoneType, ZString zoneMode, ZGuid relatedParty, params string[] unlocos)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneCode + " description";
			zone.FZ_ZoneType = zoneType;
			zone.FZ_ZoneMode = zoneMode;
			zone.FZ_OH_RelatedParty = relatedParty;

			foreach (var unloco in unlocos)
			{
				AddLocationToZone(zone, unloco);
			}

			return zone;
		}

		void AddLocationToZone(RefZoneHeader zone, string unloco)
		{
			var location = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unloco);
			zone.UNLOCOs.Add(location);
		}

		void AssertOverlappingZonesMessage(RefZoneHeader zone, (string, string[])[] expectedLocationZones)
		{
			var message = zone.GetOverlappingZonesMessage();

			if (expectedLocationZones == null)
			{
				message.Should().BeNullOrEmpty(because: $"All locations in `{zone.FZ_Code}` should not be found in other zones having the same zone type, zone mode, and related party.");
				return;
			}

			// Since there is no certain order of the zone codes in the message, let's fetch only the codes for comparisons.
			var lines = message.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			var regex = new Regex(@"'(\w+)'");
			var actualZoneLocations =
			(
				from line in lines
				select regex.Matches(line)
					.OfType<Match>()
					.Select(m => m.Groups[1].Value)
					.ToArray()
				into matches
				where matches.Length > 0
				select (matches[0], matches.Skip(1).ToArray())
			).ToList();

			actualZoneLocations.Should().BeEquivalentTo(expectedLocationZones);
			message.Should().EndWith("It may result in overlapping Rates or Charges to be loaded to the relevant Job when Autorating.");

			Assert("FluentAssertions is used.", condition: true);
		}

		void AssertOverlappingZonesMessageForContractZones(RefZoneHeader zone, (string, string[])[] expectedLocationZones)
		{
			var message = zone.GetOverlappingZonesMessage();

			if (expectedLocationZones == null)
			{
				message.Should().BeNullOrEmpty(because: $"All locations in `{zone.FZ_Code}` should not be found in other zones having the same zone type, zone mode, and related party.");
				return;
			}

			// Since there is no certain order of the zone codes in the message, let's fetch only the codes for comparisons.
			var lines = message.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			var regex = new Regex(@"'(\w+)'");
			var actualZoneLocations =
			(
				from line in lines
				select regex.Matches(line)
					.OfType<Match>()
					.Select(m => m.Groups[1].Value)
					.ToArray()
				into matches
				where matches.Length > 0
				select (matches[0], matches.Skip(1).ToArray())
			).ToList();

			actualZoneLocations.Should().BeEquivalentTo(expectedLocationZones);
			message.Should().EndWith("It may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ when launched from relevant jobs.");

			Assert("FluentAssertions is used.", condition: true);
		}

		[StressTest]
		public void TestGetZonesIncludingCountriesAndUNLOCOsZones_FewDbHits()
		{
			var zoneUSAR = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USAR"));
			var zoneAUSR = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUSR"));

			Factory.ResetDatabaseLoadCount();
			var expectedHits = new Dictionary<string, int>
			{
				{ RefZonePivot.Schema.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories("Make sure it doesnt do 8000 hits!", expectedHits, tablesToCollectQueriesFor: expectedHits.Keys.ToArray(), ignoreUnspecified: true))
			using (RowFactory.RemoveCachedTablesTemporarily(RefZonePivot.Schema.TableName))
			{
				zoneAUSR.CompletelyCovers(zoneUSAR);
			}
		}

		public void TestReportingZoneTypeClearsCarrier()
		{
			RefZoneHeader zone = Factory.New<RefZoneHeader>();
			zone.FZ_OH_RelatedParty = ZGuid.NewZGuid();

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			AssertNotEquals(ZGuid.Empty, zone.FZ_OH_RelatedParty);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Reporting;
			AssertEquals(ZGuid.Empty, zone.FZ_OH_RelatedParty);
		}

		public void TestSetSCHZoneTypeClearsCountries()
		{
			RefZoneHeader zone = Factory.New<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			zone.Countries.AddNew();
			zone.Countries.AddNew();
			AssertEquals("Pre-condition", 2, zone.Countries.Count);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
			AssertEquals(0, zone.Countries.Count);
		}

		public void TestIsRatingAvailableZone()
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse;
			AssertEquals("IsRatingAvailableZone should be false", false, zone.IsRatingAvailableZone);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Reporting;
			AssertEquals("IsRatingAvailableZone should be false", false, zone.IsRatingAvailableZone);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			AssertEquals("IsRatingAvailableZone should be false", false, zone.IsRatingAvailableZone);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			AssertEquals("IsRatingAvailableZone should be false", false, zone.IsRatingAvailableZone);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			AssertEquals("IsRatingAvailableZone should be true", true, zone.IsRatingAvailableZone);
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(RefZoneHeader)));
		}

		public void TestCanDelete()
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			AssertEquals("Can't delete WRS zone", false, zone.CanDelete);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			AssertEquals("Can't delete TAX zone", false, zone.CanDelete);

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			AssertEquals("Can delete RAT zone", true, zone.CanDelete);
		}

		public void TestWRSZoneIsReadOnly()
		{
			var wrsZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean));
			AssertEquals("Can't edit existing WRS Zone code", true, wrsZone.ReadOnly);

			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			AssertEquals("Zone is not readonly", false, zone.ReadOnly);
			Assert("Zone has validation errors", zone.FZ_ZoneTypeInfo.HasErrors());

			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			AssertEquals("Can edit TAX Zone code", false, zone.ReadOnly);
		}

		public void TestILocationReferenceIsLocalInRelationTo()
		{
			var usa = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var rio = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "BRRIO");

			ZoneToTest.Countries.Add(usa);
			ZoneToTest.UNLOCOs.Add(rio);

			ILocationReference locationReference = ZoneToTest;

			AssertEquals("US", true, locationReference.IsLocalInRelationTo("US"));
			AssertEquals("USNYC", true, locationReference.IsLocalInRelationTo("USNYC"));
			AssertEquals("BRRIO", true, locationReference.IsLocalInRelationTo("BRRIO"));
			AssertEquals("BRSAO", false, locationReference.IsLocalInRelationTo("BRSAO"));
			AssertEquals("BRR", false, locationReference.IsLocalInRelationTo("BRR"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Tests for Copy button and Clone functionality

		public void TestTemplateCopy()
		{
			RefZoneHeader copiedZone = (RefZoneHeader)ZoneToTest.TemplateCopy();
			Assert(copiedZone.FZ_Code.IsEmpty);
			AssertEquals(ZoneToTest.FZ_Description, copiedZone.FZ_Description);
		}

		[ExpectNoExceptions]
		public void TestSupportsClone()
		{
			RefZoneHeader clone = (RefZoneHeader)ZoneToTest.Clone();

			Assert("Code in our clone should be empty", clone.FZ_Code.IsEmpty);
			AssertEquals("The description (and other stuff) should be copied though!", ZoneToTest.Description, clone.Description);
		}

		public void TestCloneCopiesCountries()
		{
			RefCountry britain = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			RefCountry america = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			ZoneToTest.Countries.Add(britain);
			ZoneToTest.Countries.Add(america);

			RefZoneHeader clone = (RefZoneHeader)ZoneToTest.Clone();

			AssertEquals("Clone has copied countries", 2, clone.Countries.Count);
			AssertEquals("britain is in our clone", true, clone.Countries.Contains(britain));
			AssertEquals("america is in our clone", true, clone.Countries.Contains(america));

			ZoneToTest.Countries.RemoveAll();
			AssertEquals("Original has NO countries", 0, ZoneToTest.Countries.Count);
			AssertEquals("Clone STILL has copied countries", 2, clone.Countries.Count);
		}

		public void TestCloneCopiesPorts()
		{
			RefUNLOCO brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefUNLOCO melbourne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");

			ZoneToTest.UNLOCOs.Add(brisbane);
			ZoneToTest.UNLOCOs.Add(melbourne);

			RefZoneHeader clone = (RefZoneHeader)ZoneToTest.Clone();

			AssertEquals("Clone has copied ports", 2, clone.UNLOCOs.Count);
			AssertEquals("brisbane is in our clone", true, clone.UNLOCOs.Contains(brisbane));
			AssertEquals("melbourne is in our clone", true, clone.UNLOCOs.Contains(melbourne));

			ZoneToTest.UNLOCOs.RemoveAll();
			AssertEquals("Original has NO ports", 0, ZoneToTest.UNLOCOs.Count);
			AssertEquals("Clone STILL has copied ports", 2, clone.UNLOCOs.Count);
		}

		public void TestTemplateCopySkipsZoneTypeForWRSZones()
		{
			ZoneToTest.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			var clone = (RefZoneHeader)ZoneToTest.Clone();

			Assert("Code in our clone should be empty", clone.FZ_Code.IsEmpty);
			Assert("Zone type in our clone should be empty", clone.FZ_ZoneType.IsEmpty);
			AssertEquals("The description (and other stuff) should be copied though!", ZoneToTest.Description, clone.Description);
		}

		RefZoneHeader ZoneToTest
		{
			get
			{
				if (fZoneToTest == null)
				{
					fZoneToTest = Factory.New<RefZoneHeader>();
					fZoneToTest.FZ_Code = "code";
					fZoneToTest.FZ_Description = "Sumink to copy like";
				}

				return fZoneToTest;
			}
		}
		RefZoneHeader fZoneToTest;

		#endregion

		#region Active Filter

		public void TestActiveFilter()
		{
			var activeZone = Factory.New<RefZoneHeader>();
			activeZone.FZ_Description = "Active Zone";

			var inactiveZone = Factory.New<RefZoneHeader>();
			inactiveZone.FZ_Description = "Inactive Zone";
			inactiveZone.FZ_IsActive = false;

			var filter = new ZQuery(RefZoneHeaderSchema.PK, new ZGuid[] { activeZone.PK, inactiveZone.PK });

			var results = Factory.Load<RefZoneHeader>(filter);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "Active Zone" },
				Array.ConvertAll(results, (v) => v.FZ_Description.ToString()));

			filter.IgnoreActiveFilter = true;
			results = Factory.Load<RefZoneHeader>(filter);
			AssertContainsExactElementsInAnyOrder(
				new string[] { "Active Zone", "Inactive Zone" },
				Array.ConvertAll(results, (v) => v.FZ_Description.ToString()));
		}

		#endregion

		#region ILocation Tests

		public void TestILocationCompletelyCovers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GBBYS";
			var abbotsAdrs = org.MainAddress;
			abbotsAdrs.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			abbotsAdrs.OA_RL_NKRelatedPortCode = "GBBYS";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = org.PK;

			var newYork = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USNYC"));
			var abbots = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBBYS"));
			var gbUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBABB"));

			var britain = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedKingdom);
			var america = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);

			var usZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usZone.Countries.Add(america);

			AssertEquals("One country Zone (with US) covers US.", true, usZone.CompletelyCovers(america));

			usZone.Countries.Add(britain);

			AssertEquals("2 country Zone (with US) covers US.", true, usZone.CompletelyCovers(america));
			AssertEquals("2 country Zone (with BR) covers address with country BR.", true, usZone.CompletelyCovers(abbotsAdrs));
			AssertEquals("2 country Zone (with BR) covers jobdoc with country BR.", true, usZone.CompletelyCovers(docAddress));

			var usAbbotsZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usAbbotsZone.Countries.Add(america);
			usAbbotsZone.UNLOCOs.Add(abbots);

			AssertEquals("One country Zone (with US) and UNLOCO covers US.", true, usAbbotsZone.CompletelyCovers(america));
			AssertEquals("One country Zone (with US) and UNLOCO covers Abbots.", true, usAbbotsZone.CompletelyCovers(abbots));
			AssertEquals("One country Zone (with US) and UNLOCO covers New York.", true, usAbbotsZone.CompletelyCovers(newYork));
			AssertEquals("One country Zone (with US) and UNLOCO doesn't cover Britain.", false, usAbbotsZone.CompletelyCovers(britain));
			AssertEquals("One country Zone (with US) and UNLOCO covers Address in Abbots.", true, usAbbotsZone.CompletelyCovers(abbotsAdrs));

			var gbZone = Factory.NewWithValidTestData<RefZoneHeader>();
			gbZone.UNLOCOs.Add(gbUnloco);
			gbZone.UNLOCOs.Add(abbots);

			AssertEquals("Zone with only British UNLOCO's deson't cover Britain", false, gbZone.CompletelyCovers(britain));
			AssertEquals("Zone with only British UNLOCO's covers it's LOCO", true, gbZone.CompletelyCovers(abbots));
			AssertEquals("Zone with only British UNLOCO's covers it's jobDoc", true, gbZone.CompletelyCovers(docAddress));

			var usGbZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usGbZone.UNLOCOs.Add(newYork);
			usGbZone.UNLOCOs.Add(abbots);

			AssertEquals("Zone with only UNLOCO's covers it's UNLOCO's", true, usGbZone.CompletelyCovers(abbots));
		}

		#endregion
	}
}
