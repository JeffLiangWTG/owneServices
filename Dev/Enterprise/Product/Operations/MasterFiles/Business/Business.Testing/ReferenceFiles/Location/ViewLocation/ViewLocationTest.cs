using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ViewLocation))]
	sealed class ViewLocationTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot insert or delete view", true);
		}

		public void TestContains_Org()
		{
			var auZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUSR"));
			var usZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USAR"));

			var australia = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var unitedStates = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var nswQuery = new ZQuery(RefCountryStatesSchema.RW_Code, "NSW");
			nswQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, australia.RN_Code);
			var nsw = Factory.LoadTop1<RefCountryStates>(nswQuery);

			var qldQuery = new ZQuery(RefCountryStatesSchema.RW_Code, "QLD");
			qldQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, australia.RN_Code);
			var qld = Factory.LoadTop1<RefCountryStates>(qldQuery);

			var sydneyPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var brisbanePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var bankstownQuery = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			bankstownQuery.AddToFilter(RefCityTownSchema.R9_RW_NKState, "NSW");
			bankstownQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, "BANKSTOWN");
			var bankstown = Factory.LoadTop1<RefCityTown>(bankstownQuery);

			var alexandriaQuery = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			alexandriaQuery.AddToFilter(RefCityTownSchema.R9_RW_NKState, "NSW");
			alexandriaQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, "ALEXANDRIA");
			var alexandria = Factory.LoadTop1<RefCityTown>(alexandriaQuery);

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_City = "Alexandria";

			var auZoneAsLocation = Factory.Load<ViewLocation>(auZone.PK);
			AssertEquals(true, auZoneAsLocation.Contains(org));

			var usZoneAsLocation = Factory.Load<ViewLocation>(usZone.PK);
			AssertEquals(false, usZoneAsLocation.Contains(org));

			var australiaAsLocation = Factory.Load<ViewLocation>(australia.PK);
			AssertEquals(true, australiaAsLocation.Contains(org));

			var unitedStatesAsLocation = Factory.Load<ViewLocation>(unitedStates.PK);
			AssertEquals(false, unitedStatesAsLocation.Contains(org));

			var nswAsLocation = Factory.Load<ViewLocation>(nsw.PK);
			AssertEquals(true, nswAsLocation.Contains(org));

			var qldAsLocation = Factory.Load<ViewLocation>(qld.PK);
			AssertEquals(false, qldAsLocation.Contains(org));

			var sydneyPortAsLocation = Factory.Load<ViewLocation>(sydneyPort.PK);
			AssertEquals(true, sydneyPortAsLocation.Contains(org));

			var brisbanePortAsLocation = Factory.Load<ViewLocation>(brisbanePort.PK);
			AssertEquals(false, brisbanePortAsLocation.Contains(org));

			var bankstownAsLocation = Factory.Load<ViewLocation>(bankstown.PK);
			AssertEquals(false, bankstownAsLocation.Contains(org));

			var alexandriaAsLocation = Factory.Load<ViewLocation>(alexandria.PK);
			AssertEquals(true, alexandriaAsLocation.Contains(org));
		}

		public void TestContains_ViewLocation()
		{
			var australia = ViewLocationHelper.GetLocationFromString(Factory, "AU", RefCountrySchema.Constants.Prefix);
			var china = ViewLocationHelper.GetLocationFromString(Factory, "CN", RefCountrySchema.Constants.Prefix);

			var nsw = ViewLocationHelper.GetLocationFromString(Factory, "NSW", RefCountryStatesSchema.Constants.Prefix);
			var vic = ViewLocationHelper.GetLocationFromString(Factory, "VIC", RefCountryStatesSchema.Constants.Prefix);

			var ausyd = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix);
			var aumel = ViewLocationHelper.GetLocationFromString(Factory, "AUMEL", RefUNLOCOSchema.Constants.Prefix);

			var alexandriaQuery = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			alexandriaQuery.AddToFilter(RefCityTownSchema.R9_RW_NKState, "NSW");
			alexandriaQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, "ALEXANDRIA");
			var alexandria = Factory.LoadTop1<RefCityTown>(alexandriaQuery);
			var alexandriaAsLocation = Factory.Load<ViewLocation>(alexandria.PK);

			var stkildaQuery = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, "AU");
			stkildaQuery.AddToFilter(RefCityTownSchema.R9_RW_NKState, "VIC");
			stkildaQuery.AddToFilter(RefCityTownSchema.R9_InternationalName, "ST KILDA");
			var stkilda = Factory.LoadTop1<RefCityTown>(stkildaQuery);
			var stkildaAsLocation = Factory.Load<ViewLocation>(stkilda.PK);

			var intZone = Factory.NewWithValidTestData<RefZoneHeader>();
			intZone.FZ_Code = "AUAU";
			intZone.Countries.Add(australia);
			intZone.UNLOCOs.Add(ausyd);

			var zoneProviderType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateTransportProviderSchema.Constants.Prefix);
			var zoneType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateTransportZonesSchema.Constants.Prefix);
			var zoneItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(RateTransportZoneItemSchema.Constants.Prefix);

			var domZoneProvider = Factory.New(zoneProviderType);
			domZoneProvider[RateTransportProviderSchema.Constants.TP_RN_NKCountry] = "AU";

			var domZone = Factory.New(zoneType);
			domZone[RateTransportZonesSchema.Constants.TZ_TP] = domZoneProvider.PK;
			domZone[RateTransportZonesSchema.Constants.TZ_ZoneName] = "Sydney Zone";

			var domZoneItem = Factory.New(zoneItemType);
			domZoneItem[RateTransportZoneItemSchema.Constants.TQ_TZ_DomesticZone] = domZone.PK;
			domZoneItem[RateTransportZoneItemSchema.Constants.TQ_R9_CityTown] = alexandria.PK;

			Factory.Save();

			var intZoneAsLocation = Factory.Load<ViewLocation>(intZone.PK);
			var domZoneAsLocation = Factory.Load<ViewLocation>(domZone.PK);

			AssertLocationContainsLocation(australia,
										new ViewLocation[] { nsw, vic, ausyd, aumel, alexandriaAsLocation, stkildaAsLocation, domZoneAsLocation },
										new ViewLocation[] { australia, china, intZoneAsLocation });

			AssertLocationContainsLocation(china,
										System.Array.Empty<ViewLocation>(),
										new ViewLocation[] { australia, china, nsw, vic, ausyd, aumel, alexandriaAsLocation, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });

			AssertLocationContainsLocation(nsw,
										new ViewLocation[] { ausyd, alexandriaAsLocation },
										new ViewLocation[] { australia, china, nsw, vic, aumel, stkildaAsLocation, domZoneAsLocation, intZoneAsLocation });

			AssertLocationContainsLocation(vic,
										new ViewLocation[] { aumel, stkildaAsLocation },
										new ViewLocation[] { australia, china, nsw, vic, ausyd, alexandriaAsLocation, domZoneAsLocation, intZoneAsLocation });

			AssertLocationContainsLocation(ausyd,
										System.Array.Empty<ViewLocation>(),
										new ViewLocation[] { australia, china, nsw, vic, ausyd, aumel, alexandriaAsLocation, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });

			AssertLocationContainsLocation(aumel,
										System.Array.Empty<ViewLocation>(),
										new ViewLocation[] { australia, china, nsw, vic, ausyd, aumel, alexandriaAsLocation, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });

			AssertLocationContainsLocation(alexandriaAsLocation,
										System.Array.Empty<ViewLocation>(),
										new ViewLocation[] { australia, china, nsw, vic, ausyd, aumel, alexandriaAsLocation, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });

			AssertLocationContainsLocation(stkildaAsLocation,
										System.Array.Empty<ViewLocation>(),
										new ViewLocation[] { australia, china, nsw, vic, ausyd, aumel, alexandriaAsLocation, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });

			AssertLocationContainsLocation(intZoneAsLocation,
										new ViewLocation[] { australia, ausyd },
										new ViewLocation[] { china, nsw, vic, aumel, alexandriaAsLocation, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });

			AssertLocationContainsLocation(domZoneAsLocation,
										new ViewLocation[] { alexandriaAsLocation },
										new ViewLocation[] { australia, china, nsw, vic, ausyd, aumel, stkildaAsLocation, intZoneAsLocation, domZoneAsLocation });
		}

		void AssertLocationContainsLocation(ViewLocation mainLocation, ViewLocation[] locationsContain, ViewLocation[] locationsNotContain)
		{
			CombineAssertions(() =>
			{
				foreach (var location in locationsContain)
				{
					Assert(mainLocation.VLO_Code + " contains " + location.VLO_Code, mainLocation.Contains(location));
				}

				foreach (var location in locationsNotContain)
				{
					Assert(mainLocation.VLO_Code + " doesn't contain " + location.VLO_Code, !mainLocation.Contains(location));
				}
			});
		}
	}
}
