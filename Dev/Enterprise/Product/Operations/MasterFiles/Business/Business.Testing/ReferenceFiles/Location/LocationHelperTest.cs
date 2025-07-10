using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LocationHelperTest : TestCaseWithFactory
	{
		#region Test CompletelyCovers

		public void TestCompletelyCovers()
		{
			RefUNLOCO aUSYD = (RefUNLOCO)LocationHelper.GetLocationFromString("AUSYD", Factory);
			RefUNLOCO aUBWU = (RefUNLOCO)LocationHelper.GetLocationFromString("AUBWU", Factory);
			RefUNLOCO aUMEL = (RefUNLOCO)LocationHelper.GetLocationFromString("AUMEL", Factory);
			RefUNLOCO gBLON = (RefUNLOCO)LocationHelper.GetLocationFromString("GBLON", Factory);

			IATACityCode sYD = (IATACityCode)LocationHelper.GetLocationFromString("SYD", Factory);
			IATACityCode lON = (IATACityCode)LocationHelper.GetLocationFromString("LON", Factory);

			RefCountry aU = (RefCountry)LocationHelper.GetLocationFromString("AU", Factory);
			RefCountry gB = (RefCountry)LocationHelper.GetLocationFromString("GB", Factory);

			RefZoneHeader z_Empty = NewZone("ZZ00");
			RefZoneHeader z_AU = NewZone("ZZ01", aU);
			RefZoneHeader z_GB = NewZone("ZZ02", gB);
			RefZoneHeader z_AU_GB = NewZone("ZZ03", aU, gB);
			RefZoneHeader z_AUSYD = NewZone("ZZ04", aUSYD);
			RefZoneHeader z_AUSYD_AUBWU = NewZone("ZZ05", aUSYD, aUBWU);
			RefZoneHeader z_AUSYD_AUMEL = NewZone("ZZ06", aUSYD, aUMEL);
			RefZoneHeader z_AUSYD_AUBWU_GBLON = NewZone("ZZ07", aUSYD, aUBWU, gBLON);
			RefZoneHeader z_AU_AUSYD = NewZone("ZZ08", aU, aUSYD);
			RefZoneHeader z_GB_AUSYD = NewZone("ZZ09", gB, aUSYD);
			RefZoneHeader z_AU_GBLON = NewZone("ZZ10", aU, gBLON);
			RefZoneHeader z_AU_AUSYD_GBLON = NewZone("ZZ11", aU, aUSYD, gBLON);
			RefZoneHeader z_AU_GB_AUSYD = NewZone("ZZ12", aU, gB, aUSYD);

			Factory.Save();

			AssertCompletelyCovers(aUSYD, aUSYD, true, true);
			AssertCompletelyCovers(aUSYD, gBLON, false, false);
			AssertCompletelyCovers(aUSYD, sYD, false, true);
			AssertCompletelyCovers(aUBWU, sYD, false, true);
			AssertCompletelyCovers(aUSYD, lON, false, false);
			AssertCompletelyCovers(aUSYD, aU, false, true);
			AssertCompletelyCovers(aUSYD, gB, false, false);
			AssertCompletelyCovers(aUSYD, z_Empty, true, false);
			AssertCompletelyCovers(aUSYD, z_AU, false, true);
			AssertCompletelyCovers(aUSYD, z_GB, false, false);
			AssertCompletelyCovers(aUSYD, z_AU_GB, false, true);
			AssertCompletelyCovers(aUSYD, z_AUSYD, true, true);
			AssertCompletelyCovers(aUSYD, z_AUSYD_AUBWU, false, true);
			AssertCompletelyCovers(aUSYD, z_AUSYD_AUMEL, false, true);
			AssertCompletelyCovers(aUSYD, z_AUSYD_AUBWU_GBLON, false, true);
			AssertCompletelyCovers(aUSYD, z_AU_AUSYD, false, true);
			AssertCompletelyCovers(aUSYD, z_GB_AUSYD, false, true);
			AssertCompletelyCovers(aUSYD, z_AU_GBLON, false, true);
			AssertCompletelyCovers(aUSYD, z_AU_AUSYD_GBLON, false, true);
			AssertCompletelyCovers(aUSYD, z_AU_GB_AUSYD, false, true);

			AssertCompletelyCovers(sYD, sYD, true, true);
			AssertCompletelyCovers(sYD, lON, false, false);
			AssertCompletelyCovers(sYD, aU, false, true);
			AssertCompletelyCovers(sYD, gB, false, false);
			AssertCompletelyCovers(sYD, z_Empty, true, false);
			AssertCompletelyCovers(sYD, z_AU, false, true);
			AssertCompletelyCovers(sYD, z_GB, false, false);
			AssertCompletelyCovers(sYD, z_AU_GB, false, true);
			AssertCompletelyCovers(sYD, z_AUSYD, true, false);
			AssertCompletelyCovers(sYD, z_AUSYD_AUBWU, true, true);
			AssertCompletelyCovers(sYD, z_AUSYD_AUMEL, false, false);
			AssertCompletelyCovers(sYD, z_AUSYD_AUBWU_GBLON, false, true);
			AssertCompletelyCovers(sYD, z_AU_AUSYD, false, true);
			AssertCompletelyCovers(sYD, z_GB_AUSYD, false, false);
			AssertCompletelyCovers(sYD, z_AU_GBLON, false, true);
			AssertCompletelyCovers(sYD, z_AU_AUSYD_GBLON, false, true);
			AssertCompletelyCovers(sYD, z_AU_GB_AUSYD, false, true);

			AssertCompletelyCovers(aU, aU, true, true);
			AssertCompletelyCovers(aU, gB, false, false);
			AssertCompletelyCovers(aU, z_Empty, true, false);
			AssertCompletelyCovers(aU, z_AU, true, true);
			AssertCompletelyCovers(aU, z_GB, false, false);
			AssertCompletelyCovers(aU, z_AU_GB, false, true);
			AssertCompletelyCovers(aU, z_AUSYD, true, false);
			AssertCompletelyCovers(aU, z_AUSYD_AUBWU, true, false);
			AssertCompletelyCovers(aU, z_AUSYD_AUMEL, true, false);
			AssertCompletelyCovers(aU, z_AUSYD_AUBWU_GBLON, false, false);
			AssertCompletelyCovers(aU, z_AU_AUSYD, true, true);
			AssertCompletelyCovers(aU, z_GB_AUSYD, false, false);
			AssertCompletelyCovers(aU, z_AU_GBLON, false, true);
			AssertCompletelyCovers(aU, z_AU_AUSYD_GBLON, false, true);
			AssertCompletelyCovers(aU, z_AU_GB_AUSYD, false, true);

			AssertCompletelyCovers(z_Empty, z_Empty, true, true);

			AssertCompletelyCovers(z_AU, z_Empty, true, false);
			AssertCompletelyCovers(z_AU, z_AU, true, true);
			AssertCompletelyCovers(z_AU, z_GB, false, false);
			AssertCompletelyCovers(z_AU, z_AU_GB, false, true);
			AssertCompletelyCovers(z_AU, z_AUSYD, true, false);
			AssertCompletelyCovers(z_AU, z_AUSYD_AUBWU, true, false);
			AssertCompletelyCovers(z_AU, z_AUSYD_AUMEL, true, false);
			AssertCompletelyCovers(z_AU, z_AUSYD_AUBWU_GBLON, false, false);
			AssertCompletelyCovers(z_AU, z_AU_AUSYD, true, true);
			AssertCompletelyCovers(z_AU, z_GB_AUSYD, false, false);
			AssertCompletelyCovers(z_AU, z_AU_GBLON, false, true);
			AssertCompletelyCovers(z_AU, z_AU_AUSYD_GBLON, false, true);
			AssertCompletelyCovers(z_AU, z_AU_GB_AUSYD, false, true);

			AssertCompletelyCovers(z_AUSYD, z_Empty, true, false);
			AssertCompletelyCovers(z_AUSYD, z_AU, false, true);
			AssertCompletelyCovers(z_AUSYD, z_GB, false, false);
			AssertCompletelyCovers(z_AUSYD, z_AU_GB, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AUSYD, true, true);
			AssertCompletelyCovers(z_AUSYD, z_AUSYD_AUBWU, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AUSYD_AUMEL, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AUSYD_AUBWU_GBLON, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AU_AUSYD, false, true);
			AssertCompletelyCovers(z_AUSYD, z_GB_AUSYD, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AU_GBLON, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AU_AUSYD_GBLON, false, true);
			AssertCompletelyCovers(z_AUSYD, z_AU_GB_AUSYD, false, true);

			AssertCompletelyCovers(z_AU_AUSYD, z_Empty, true, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AU, true, true);
			AssertCompletelyCovers(z_AU_AUSYD, z_GB, false, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AU_GB, false, true);
			AssertCompletelyCovers(z_AU_AUSYD, z_AUSYD, true, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AUSYD_AUBWU, true, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AUSYD_AUMEL, true, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AUSYD_AUBWU_GBLON, false, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AU_AUSYD, true, true);
			AssertCompletelyCovers(z_AU_AUSYD, z_GB_AUSYD, false, false);
			AssertCompletelyCovers(z_AU_AUSYD, z_AU_GBLON, false, true);
			AssertCompletelyCovers(z_AU_AUSYD, z_AU_AUSYD_GBLON, false, true);
			AssertCompletelyCovers(z_AU_AUSYD, z_AU_GB_AUSYD, false, true);
		}

		string CompletelyCoversErrorMessage(ILocation mainLocation, ILocation otherLocation, bool expectedResult)
		{
			return $"{mainLocation.Code} should{(!expectedResult ? " NOT " : " ")}have covered {otherLocation.Code}.";
		}

		void AssertCompletelyCovers(ILocation firstLocation, ILocation secondLocation, bool firstCoversSecond, bool secondCoversFirst)
		{
			AssertEquals(CompletelyCoversErrorMessage(firstLocation, secondLocation, firstCoversSecond), firstCoversSecond, firstLocation.CompletelyCovers(secondLocation));
			AssertEquals(CompletelyCoversErrorMessage(secondLocation, firstLocation, secondCoversFirst), secondCoversFirst, secondLocation.CompletelyCovers(firstLocation));
		}

		RefZoneHeader NewZone(string zoneCode, params ILocation[] zoneMembers)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneCode + " Desc";
			zone.FZ_ZoneType = "ALL";

			foreach (var location in zoneMembers)
			{
				if (location is RefCountry country)
				{
					zone.Countries.Add(country);
				}
				else if (location is RefUNLOCO port)
				{
					zone.UNLOCOs.Add(port);
				}
			}

			return zone;
		}

		#endregion

		#region TestGetLocationFromIDocAddress

		public void TestGetLocationFromIDocAddress()
		{
			var expectedPortLocation = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var expectedCountryLocation = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var organisation = Factory.New<OrgHeader>();
			var address = organisation.Addresses.AddNew();
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = address.PK;
			AssertEquals(expectedPortLocation, LocationHelper.GetLocationFromIDocAddress(jobDocAddress, Factory));

			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_RN_NKCountryCode = "US";
			AssertEquals(expectedCountryLocation, LocationHelper.GetLocationFromIDocAddress(jobDocAddress, Factory));
		}

		#endregion

		public void TestBuildLocationFilter()
		{
			AssertEquals(@"(TI_OriginLRC IN ('', 'AU', 'OCEG', 'US', 'AUMEL', 'AUSR', 'AUEC', 'USCA', 'AUBNE', 'USLAX', 'USWG', 'USAR', 'AUSYD') OR ((TI_OriginLRC like 'AU%' OR TI_OriginLRC like 'US%') AND LEN(TI_OriginLRC) = 5) OR (
	LEN(TI_OriginLRC) IN (2, 5) AND
	TI_OriginLRC IN
	(
		SELECT DISTINCT RN_Code
		FROM dbo.vw_ZoneCountry WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC', 'USCA')
		UNION ALL
		SELECT DISTINCT RL_Code
		FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC', 'USCA')
		UNION ALL
		SELECT DISTINCT RL_Code
		FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC', 'USCA')
	)
))",
				LocationHelper.GetLocationFilter(Factory, RateEntrySchema.TI_OriginLRC, true, ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType, "AU", "US", "AUMEL", "AUEC", "USCA", "AUBNE", "USLAX", "AUSYD").LiteralTextADO);
		}

		public void TestGetLocationFilter()
		{
			AssertEquals(@"(TI_OriginLRC IN ('', 'AU', 'OCEG', 'US', 'AUMEL', 'AUSR', 'AUEC', 'USCA', 'AUBNE', 'USLAX', 'USWG', 'USAR', 'AUSYD') OR ((TI_OriginLRC like 'AU%' OR TI_OriginLRC like 'US%') AND LEN(TI_OriginLRC) = 5) OR (
	LEN(TI_OriginLRC) IN (2, 5) AND
	TI_OriginLRC IN
	(
		SELECT DISTINCT RN_Code
		FROM dbo.vw_ZoneCountry WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC', 'USCA')
		UNION ALL
		SELECT DISTINCT RL_Code
		FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC', 'USCA')
		UNION ALL
		SELECT DISTINCT RL_Code
		FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC', 'USCA')
	)
))",
				LocationHelper.GetLocationFilter(Factory, RateEntrySchema.TI_OriginLRC, true, ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType, "AU", "US", "AUMEL", "AUEC", "USCA", "AUBNE", "USLAX", "AUSYD").LiteralTextADO);

			AssertEquals("(TI_OriginLRC IN ('AU', 'OCEG') OR ((TI_OriginLRC like 'AU%') AND LEN(TI_OriginLRC) = 5))", LocationHelper.GetLocationFilter(Factory, "AU", RateEntrySchema.TI_OriginLRC, ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType).LiteralTextADO);
			AssertEquals("(TI_OriginLRC IN ('AUSYD', 'AU', 'AUSR', 'AUEC', 'OCEG'))", LocationHelper.GetLocationFilter(Factory, "AUSYD", RateEntrySchema.TI_OriginLRC, ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType).LiteralTextADO);
			AssertEquals("(TI_OriginLRC IN ('IN', 'INSG', 'INDR') OR ((TI_OriginLRC like 'IN%') AND LEN(TI_OriginLRC) = 5))", LocationHelper.GetLocationFilter(Factory, "IN", RateEntrySchema.TI_OriginLRC, ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType).LiteralTextADO);
			AssertEquals("(TI_OriginLRC IN ('SOME JUNK'))", LocationHelper.GetLocationFilter(Factory, "SOME JUNK", RateEntrySchema.TI_OriginLRC, ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType).LiteralTextADO);

			AssertEquals("(JS_RL_NKOrigin like 'IN%')", LocationHelper.GetLocationFilter(Factory, "IN", JobShipmentSchema.JS_RL_NKOrigin, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>()).LiteralTextADO);
			AssertEquals("(JS_RL_NKOrigin IN ('AUSYD'))", LocationHelper.GetLocationFilter(Factory, "AUSYD", JobShipmentSchema.JS_RL_NKOrigin, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>()).LiteralTextADO);
			AssertMultilineASCIIEquals("Filter for UNLOCO only field", @"(JS_RL_NKOrigin IN
(
	SELECT DISTINCT RL_Code
	FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
	WHERE FZ_Code IN ('AUEC')
	UNION ALL
	SELECT DISTINCT RL_Code
	FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
	WHERE FZ_Code IN ('AUEC')
))", LocationHelper.GetLocationFilter(Factory, "AUEC", JobShipmentSchema.JS_RL_NKOrigin, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>()).LiteralTextADO);
			AssertEquals("(JS_RL_NKOrigin IN ('SOME JUNK'))", LocationHelper.GetLocationFilter(Factory, "SOME JUNK", JobShipmentSchema.JS_RL_NKOrigin, ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>()).LiteralTextADO);

			AssertEquals("(GC_RN_NKCountryCode IN ('IN'))", LocationHelper.GetLocationFilter(Factory, "IN", GlbCompanySchema.GC_RN_NKCountryCode, typeof(GlbCompany)).LiteralTextADO);
			AssertEquals(@"((
	GC_RN_NKCountryCode IN
	(
		SELECT DISTINCT RN_Code
		FROM dbo.vw_ZoneCountry WITH (NOEXPAND)
		WHERE FZ_Code IN ('AUEC')
	)
))", LocationHelper.GetLocationFilter(Factory, "AUEC", GlbCompanySchema.GC_RN_NKCountryCode, typeof(GlbCompany)).LiteralTextADO);
			AssertEquals("(GC_RN_NKCountryCode IN ('AU'))", LocationHelper.GetLocationFilter(Factory, "AUSYD", GlbCompanySchema.GC_RN_NKCountryCode, typeof(GlbCompany)).LiteralTextADO);
			AssertEquals("(GC_RN_NKCountryCode IN ('SOME JUNK'))", LocationHelper.GetLocationFilter(Factory, "SOME JUNK", GlbCompanySchema.GC_RN_NKCountryCode, typeof(GlbCompany)).LiteralTextADO);
		}

		public void TestGetLocationFilter_ExpectEmptyFilter()
		{
			var portFieldType = JobShipmentSchema.JS_RL_NKDestination;
			var portFieldTypeObject = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var allFieldType = RateEntrySchema.TI_DestinationLRC;
			var allFieldTypeObject = ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().RateEntryType;

			var message = "Port Field Type columns should not try to generate a filter for three letter IATA codes.";
			var expected = "";
			var actual = LocationHelper.GetLocationFilter(Factory, portFieldType, false, portFieldTypeObject, "XXX").LiteralTextADO;
			AssertEquals(message, expected, actual);

			message = "Filter should only include the empty option";
			expected = "(JS_RL_NKDestination IN (''))";
			actual = LocationHelper.GetLocationFilter(Factory, portFieldType, true, portFieldTypeObject, "XXX").LiteralTextADO;
			AssertEquals(message, expected, actual);

			message = "When location is invalid, we should still filter by it. The invalid data might refer to a deleted UNLOCO/International Zone.";
			expected = "(TI_DestinationLRC IN ('', 'XXX'))";
			actual = LocationHelper.GetLocationFilter(Factory, allFieldType, true, allFieldTypeObject, "XXX").LiteralTextADO;
			AssertEquals(message, expected, actual);

			expected = "(TI_DestinationLRC IN ('!'))";
			actual = LocationHelper.GetLocationFilter(Factory, allFieldType, false, allFieldTypeObject, "!").LiteralTextADO;
			AssertEquals(message, expected, actual);
		}

		public void TestGetFullNameWhenTownNameInMultipleStates()
		{
			RefUNLOCO thomasvillePenn = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USTMV"));
			RefUNLOCO thomasvilleNC = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USTVE"));
			RefUNLOCO thomasvilleGeorgia = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USTVI"));

			AssertEquals("Thomasville (PA), US", LocationHelper.GetFullName(thomasvillePenn, true));
			AssertEquals("Thomasville (NC), US", LocationHelper.GetFullName(thomasvilleNC, true));
			AssertEquals("Thomasville (GA), US", LocationHelper.GetFullName(thomasvilleGeorgia, true));

			RefUNLOCO ashfordKent = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBASD"));
			RefUNLOCO ashfordSurry = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBASF"));
			AssertEquals("Ashford (KEN), GB", LocationHelper.GetFullName(ashfordKent, true));
			AssertEquals("Ashford (SRY), GB", LocationHelper.GetFullName(ashfordSurry, true));

			RefUNLOCO londonUK = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));
			AssertEquals("London, GB", LocationHelper.GetFullName(londonUK, true));

			RefUNLOCO sydneyAU = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			AssertEquals("Sydney", LocationHelper.GetFullName(sydneyAU, true));
		}

		public void TestIsLocationRule()
		{
			var sydneyAU = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			AssertEquals(false, sydneyAU.IsLocationRule());

			var rule = new LocationRule(GlbCompany.CurrentCompany, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
			AssertEquals(true, rule.IsLocationRule());

			rule = new LocationRule(GlbCompany.CurrentCompany, PlaceOfSupplyListProvider.Codes.OtherTerritories);
			AssertEquals(true, rule.IsLocationRule());
		}
	}
}
