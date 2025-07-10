using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	static class OrgCusCodeCountryFactory
	{
		internal static IOrgCusCodeProvider GetIOrgCusCodeProvider(ZString? countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeProvider;
		}

		internal static IOrgCusCodeProvider GetFallbackIOrgCusCodeProvider()
		{
			return new FallbackOrgCusCodeInfo(null);
		}

		internal static IOrgCusCodeCustomsRegNoValidationProvider GetIOrgCusCodeCustomsRegNoValidationProvider(ZString countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeCustomsRegNoValidationProvider;
		}

		internal static IOrgCusCodeUniqueValidation GetIOrgCusCodeUniqueValidation(ZString countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeUniqueValidation;
		}

		internal static IOrgCusCodeNonUniqueProvider GetIOrgCusCodeNonUniqueProvider(ZString countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeNonUniqueProvider;
		}

		internal static IOrgCusCodeStandardIndustrialClassificationNoValidationProvider GetIOrgCusCodeStandardIndustrialClassificationNoValidationProvider(ZString countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeStandardIndustrialClassificationNoValidationProvider;
		}

		internal static IOrgCusCodeLookupsProvider GetIOrgCusCodeLookupsProvider(ZString countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeLookupsProvider;
		}

		internal static IOrgCusCodeDependencyProvider GetIOrgCusCodeDependencyProvider(ZString countryCode)
		{
			return GetOrgCusCodeInfo(countryCode) as IOrgCusCodeDependencyProvider;
		}

		public class FallbackOrgCusCodeInfo : OrgCusCodeInfo, IOrgCusCodeProvider
		{
			string CountryCode { get; }

			public FallbackOrgCusCodeInfo(string countryCode)
			{
				CountryCode = countryCode;
			}

			public FallbackOrgCusCodeInfo()
			{
				CountryCode = null;
			}

			CodeDescriptionPairList IOrgCusCodeProvider.AddOrgCusCodes(CodeDescriptionPairList list)
			{
				list.AddPair(OrgCusCode.CodeTypes.GSTCode, Country.GetDefaultTaxCodeDescription(OrgCusCode.CodeTypes.GSTCode)); // Accounting consumption code

				return list;
			}

			HashSet<string> IOrgCusCodeProvider.GetPrimaryCusCodes()
			{
				if (CountryCode != null)
				{
					var result = GetPrimaryCusCodes(CountryCode);
					return result;
				}
				else
				{
					var result = GetPrimaryCusCodes("__"); // Fallback country code
					return result;
				}
			}

			HashSet<string> IOrgCusCodeProvider.GetMainOrganizationNumberTypes()
			{
				if (CountryCode != null)
				{
					var result = GetMainOrganizationNumberTypes(CountryCode);
					return result;
				}
				else
				{
					var result = GetMainOrganizationNumberTypes("__"); // Fallback country code
					return result;
				}
			}
		}

		static OrgCusCodeInfo GetOrgCusCodeInfo(ZString? countryCode)
		{
			//Order country cases alphabetically
			switch (countryCode)
			{
				//Creates a country specific OrgCusCodeInfo child class which only contains OrgCusCode related methods.
				case CountryCodes.Andorra:
					return new AndorraOrgCusCodeInfo();
				case CountryCodes.UnitedArabEmirates:
					return new UnitedArabEmiratesOrgCusCodeInfo();
				case CountryCodes.Afghanistan:
					return new AfghanistanOrgCusCodeInfo();
				case CountryCodes.AntiguaAndBarbuda:
					return new AntiguaAndBarbudaOrgCusCodeInfo();
				case CountryCodes.Anguilla:
					return new AnguillaOrgCusCodeInfo();
				case CountryCodes.Albania:
					return new AlbaniaOrgCusCodeInfo();
				case CountryCodes.Armenia:
					return new ArmeniaOrgCusCodeInfo();
				case CountryCodes.NetherlandsAntilles:
					return new NetherlandsAntillesOrgCusCodeInfo();
				case CountryCodes.Angola:
					return new AngolaOrgCusCodeInfo();
				case CountryCodes.Argentina:
					return new ArgentinaOrgCusCodeInfo();
				case CountryCodes.AmericanSamoa:
					return new AmericanSamoaOrgCusCodeInfo();
				case CountryCodes.Austria:
					return new AustriaOrgCusCodeInfo();
				case CountryCodes.Australia:
					return new AustraliaOrgCusCodeInfo();
				case CountryCodes.Aruba:
					return new ArubaOrgCusCodeInfo();
				case CountryCodes.AlandIslands:
					return new AlandIslandsOrgCusCodeInfo();
				case CountryCodes.Azerbaijan:
					return new AzerbaijanOrgCusCodeInfo();
				case CountryCodes.BosniaAndHerzegovina:
					return new BosniaAndHerzegovinaOrgCusCodeInfo();
				case CountryCodes.Barbados:
					return new BarbadosOrgCusCodeInfo();
				case CountryCodes.Bangladesh:
					return new BangladeshOrgCusCodeInfo();
				case CountryCodes.Belgium:
					return new BelgiumOrgCusCodeInfo();
				case CountryCodes.BurkinaFaso:
					return new BurkinaFasoOrgCusCodeInfo();
				case CountryCodes.Bulgaria:
					return new BulgariaOrgCusCodeInfo();
				case CountryCodes.Bahrain:
					return new BahrainOrgCusCodeInfo();
				case CountryCodes.Burundi:
					return new BurundiOrgCusCodeInfo();
				case CountryCodes.Benin:
					return new BeninOrgCusCodeInfo();
				case CountryCodes.SaintBarthelemy:
					return new SaintBarthelemyOrgCusCodeInfo();
				case CountryCodes.Bermuda:
					return new BermudaOrgCusCodeInfo();
				case CountryCodes.Brunei:
					return new BruneiOrgCusCodeInfo();
				case CountryCodes.Bolivia:
					return new BoliviaOrgCusCodeInfo();
				case CountryCodes.BonaireSintEustatiusAndSaba:
					return new BonaireSintEustatiusAndSabaOrgCusCodeInfo();
				case CountryCodes.Brazil:
					return new BrazilOrgCusCodeInfo();
				case CountryCodes.Bahamas:
					return new BahamasOrgCusCodeInfo();
				case CountryCodes.Bhutan:
					return new BhutanOrgCusCodeInfo();
				case CountryCodes.Botswana:
					return new BotswanaOrgCusCodeInfo();
				case CountryCodes.Belarus:
					return new BelarusOrgCusCodeInfo();
				case CountryCodes.Belize:
					return new BelizeOrgCusCodeInfo();
				case CountryCodes.Canada:
					return new CanadaOrgCusCodeInfo();
				case CountryCodes.CocosKeelingIslands:
					return new CocosKeelingIslandsOrgCusCodeInfo();
				case CountryCodes.DemocraticRepublicOfCongo:
					return new DemocraticRepublicOfCongoOrgCusCodeInfo();
				case CountryCodes.CentralAfricanRepublic:
					return new CentralAfricanRepublicOrgCusCodeInfo();
				case CountryCodes.Congo:
					return new CongoOrgCusCodeInfo();
				case CountryCodes.Switzerland:
					return new SwitzerlandOrgCusCodeInfo();
				case CountryCodes.CoteDivoire:
					return new CoteDivoireOrgCusCodeInfo();
				case CountryCodes.CookIslands:
					return new CookIslandsOrgCusCodeInfo();
				case CountryCodes.Chile:
					return new ChileOrgCusCodeInfo();
				case CountryCodes.Cameroon:
					return new CameroonOrgCusCodeInfo();
				case CountryCodes.China:
					return new ChinaOrgCusCodeInfo();
				case CountryCodes.Colombia:
					return new ColombiaOrgCusCodeInfo();
				case CountryCodes.CostaRica:
					return new CostaRicaOrgCusCodeInfo();
				case CountryCodes.Cuba:
					return new CubaOrgCusCodeInfo();
				case CountryCodes.CapeVerde:
					return new CapeVerdeOrgCusCodeInfo();
				case CountryCodes.Curacao:
					return new CuracaoOrgCusCodeInfo();
				case CountryCodes.ChristmasIsland:
					return new ChristmasIslandOrgCusCodeInfo();
				case CountryCodes.Cyprus:
					return new CyprusOrgCusCodeInfo();
				case CountryCodes.CzechRepublic:
					return new CzechRepublicOrgCusCodeInfo();
				case CountryCodes.Germany:
					return new GermanyOrgCusCodeInfo();
				case CountryCodes.Djibouti:
					return new DjiboutiOrgCusCodeInfo();
				case CountryCodes.Denmark:
					return new DenmarkOrgCusCodeInfo();
				case CountryCodes.Dominica:
					return new DominicaOrgCusCodeInfo();
				case CountryCodes.DominicanRepublic:
					return new DominicanRepublicOrgCusCodeInfo();
				case CountryCodes.Algeria:
					return new AlgeriaOrgCusCodeInfo();
				case CountryCodes.Ecuador:
					return new EcuadorOrgCusCodeInfo();
				case CountryCodes.Estonia:
					return new EstoniaOrgCusCodeInfo();
				case CountryCodes.Egypt:
					return new EgyptOrgCusCodeInfo();
				case CountryCodes.WesternSahara:
					return new WesternSaharaOrgCusCodeInfo();
				case CountryCodes.Eritrea:
					return new EritreaOrgCusCodeInfo();
				case CountryCodes.Spain:
					return new SpainOrgCusCodeInfo();
				case CountryCodes.Ethiopia:
					return new EthiopiaOrgCusCodeInfo();
				case CountryCodes.Finland:
					return new FinlandOrgCusCodeInfo();
				case CountryCodes.Fiji:
					return new FijiOrgCusCodeInfo();
				case CountryCodes.FalklandIslands:
					return new FalklandIslandsOrgCusCodeInfo();
				case CountryCodes.Micronesia:
					return new MicronesiaOrgCusCodeInfo();
				case CountryCodes.FaeroeIslands:
					return new FaeroeIslandsOrgCusCodeInfo();
				case CountryCodes.France:
					return new FranceOrgCusCodeInfo();
				case CountryCodes.Gabon:
					return new GabonOrgCusCodeInfo();
				case CountryCodes.UnitedKingdom:
					return new UnitedKingdomOrgCusCodeInfo();
				case CountryCodes.Grenada:
					return new GrenadaOrgCusCodeInfo();
				case CountryCodes.Georgia:
					return new GeorgiaOrgCusCodeInfo();
				case CountryCodes.FrenchGuyana:
					return new FrenchGuianaOrgCusCodeInfo();
				case CountryCodes.Guernsey:
					return new GuernseyOrgCusCodeInfo();
				case CountryCodes.Ghana:
					return new GhanaOrgCusCodeInfo();
				case CountryCodes.Gibraltar:
					return new GibraltarOrgCusCodeInfo();
				case CountryCodes.Greenland:
					return new GreenlandOrgCusCodeInfo();
				case CountryCodes.Gambia:
					return new GambiaOrgCusCodeInfo();
				case CountryCodes.Guinea:
					return new GuineaOrgCusCodeInfo();
				case CountryCodes.Guadeloupe:
					return new GuadeloupeOrgCusCodeInfo();
				case CountryCodes.EquatorialGuinea:
					return new EquatorialGuineaOrgCusCodeInfo();
				case CountryCodes.Greece:
					return new GreeceOrgCusCodeInfo();
				case CountryCodes.Guatemala:
					return new GuatemalaOrgCusCodeInfo();
				case CountryCodes.Guam:
					return new GuamOrgCusCodeInfo();
				case CountryCodes.GuineaBissau:
					return new GuineaBissauOrgCusCodeInfo();
				case CountryCodes.Guyana:
					return new GuyanaOrgCusCodeInfo();
				case CountryCodes.HongKong:
					return new HongKongOrgCusCodeInfo();
				case CountryCodes.HeardAndMcdonaldIslands:
					return new HeardAndMcdonaldIslandsOrgCusCodeInfo();
				case CountryCodes.Honduras:
					return new HondurasOrgCusCodeInfo();
				case CountryCodes.Croatia:
					return new CroatiaOrgCusCodeInfo();
				case CountryCodes.Haiti:
					return new HaitiOrgCusCodeInfo();
				case CountryCodes.Hungary:
					return new HungaryOrgCusCodeInfo();
				case CountryCodes.Indonesia:
					return new IndonesiaOrgCusCodeInfo();
				case CountryCodes.Ireland:
					return new IrelandOrgCusCodeInfo();
				case CountryCodes.Israel:
					return new IsraelOrgCusCodeInfo();
				case CountryCodes.IsleOfMan:
					return new IsleOfManOrgCusCodeInfo();
				case CountryCodes.India:
					return new IndiaOrgCusCodeInfo();
				case CountryCodes.BritishIndianOceanTerritory:
					return new BritishIndianOceanTerritoryOrgCusCodeInfo();
				case CountryCodes.Iraq:
					return new IraqOrgCusCodeInfo();
				case CountryCodes.Iran:
					return new IranOrgCusCodeInfo();
				case CountryCodes.Iceland:
					return new IcelandOrgCusCodeInfo();
				case CountryCodes.Italy:
					return new ItalyOrgCusCodeInfo();
				case CountryCodes.Jersey:
					return new JerseyOrgCusCodeInfo();
				case CountryCodes.Jamaica:
					return new JamaicaOrgCusCodeInfo();
				case CountryCodes.Jordan:
					return new JordanOrgCusCodeInfo();
				case CountryCodes.Japan:
					return new JapanOrgCusCodeInfo();
				case CountryCodes.Kenya:
					return new KenyaOrgCusCodeInfo();
				case CountryCodes.Kyrgyzstan:
					return new KyrgyzstanOrgCusCodeInfo();
				case CountryCodes.Cambodia:
					return new CambodiaOrgCusCodeInfo();
				case CountryCodes.Kiribati:
					return new KiribatiOrgCusCodeInfo();
				case CountryCodes.Comoros:
					return new ComorosOrgCusCodeInfo();
				case CountryCodes.SaintKittsAndNevis:
					return new SaintKittsAndNevisOrgCusCodeInfo();
				case CountryCodes.KoreaNorth:
					return new KoreaNorthOrgCusCodeInfo();
				case CountryCodes.KoreaSouth:
					return new KoreaSouthOrgCusCodeInfo();
				case CountryCodes.Kuwait:
					return new KuwaitOrgCusCodeInfo();
				case CountryCodes.CaymanIslands:
					return new CaymanIslandsOrgCusCodeInfo();
				case CountryCodes.Kazakhstan:
					return new KazakhstanOrgCusCodeInfo();
				case CountryCodes.LaoPeoplesDemocraticRepublic:
					return new LaoPeoplesDemocraticRepublicOrgCusCodeInfo();
				case CountryCodes.Lebanon:
					return new LebanonOrgCusCodeInfo();
				case CountryCodes.SaintLucia:
					return new SaintLuciaOrgCusCodeInfo();
				case CountryCodes.Liechtenstein:
					return new LiechtensteinOrgCusCodeInfo();
				case CountryCodes.SriLanka:
					return new SriLankaOrgCusCodeInfo();
				case CountryCodes.Liberia:
					return new LiberiaOrgCusCodeInfo();
				case CountryCodes.Lesotho:
					return new LesothoOrgCusCodeInfo();
				case CountryCodes.Lithuania:
					return new LithuaniaOrgCusCodeInfo();
				case CountryCodes.Luxembourg:
					return new LuxembourgOrgCusCodeInfo();
				case CountryCodes.Latvia:
					return new LatviaOrgCusCodeInfo();
				case CountryCodes.LibyanArabJamahiriya:
					return new LibyanArabJamahiriyaOrgCusCodeInfo();
				case CountryCodes.Morocco:
					return new MoroccoOrgCusCodeInfo();
				case CountryCodes.Monaco:
					return new MonacoOrgCusCodeInfo();
				case CountryCodes.Moldova:
					return new MoldovaOrgCusCodeInfo();
				case CountryCodes.Montenegro:
					return new MontenegroOrgCusCodeInfo();
				case CountryCodes.SaintMartin:
					return new SaintMartinOrgCusCodeInfo();
				case CountryCodes.Madagascar:
					return new MadagascarOrgCusCodeInfo();
				case CountryCodes.MarshallIslands:
					return new MarshallIslandsOrgCusCodeInfo();
				case CountryCodes.Macedonia:
					return new MacedoniaOrgCusCodeInfo();
				case CountryCodes.Mali:
					return new MaliOrgCusCodeInfo();
				case CountryCodes.Myanmar:
					return new MyanmarOrgCusCodeInfo();
				case CountryCodes.Mongolia:
					return new MongoliaOrgCusCodeInfo();
				case CountryCodes.Macau:
					return new MacauOrgCusCodeInfo();
				case CountryCodes.NorthernMarianaIslands:
					return new NorthernMarianaIslandsOrgCusCodeInfo();
				case CountryCodes.Martinique:
					return new MartiniqueOrgCusCodeInfo();
				case CountryCodes.Mauritania:
					return new MauritaniaOrgCusCodeInfo();
				case CountryCodes.Montserrat:
					return new MontserratOrgCusCodeInfo();
				case CountryCodes.Malta:
					return new MaltaOrgCusCodeInfo();
				case CountryCodes.Mauritius:
					return new MauritiusOrgCusCodeInfo();
				case CountryCodes.Maldives:
					return new MaldivesOrgCusCodeInfo();
				case CountryCodes.Malawi:
					return new MalawiOrgCusCodeInfo();
				case CountryCodes.Mexico:
					return new MexicoOrgCusCodeInfo();
				case CountryCodes.Malaysia:
					return new MalaysiaOrgCusCodeInfo();
				case CountryCodes.Mozambique:
					return new MozambiqueOrgCusCodeInfo();
				case CountryCodes.Namibia:
					return new NamibiaOrgCusCodeInfo();
				case CountryCodes.NewCaledonia:
					return new NewCaledoniaOrgCusCodeInfo();
				case CountryCodes.Niger:
					return new NigerOrgCusCodeInfo();
				case CountryCodes.NorfolkIsland:
					return new NorfolkIslandOrgCusCodeInfo();
				case CountryCodes.Nigeria:
					return new NigeriaOrgCusCodeInfo();
				case CountryCodes.Nicaragua:
					return new NicaraguaOrgCusCodeInfo();
				case CountryCodes.Netherlands:
					return new NetherlandsOrgCusCodeInfo();
				case CountryCodes.Norway:
					return new NorwayOrgCusCodeInfo();
				case CountryCodes.Nepal:
					return new NepalOrgCusCodeInfo();
				case CountryCodes.Nauru:
					return new NauruOrgCusCodeInfo();
				case CountryCodes.Niue:
					return new NiueOrgCusCodeInfo();
				case CountryCodes.NewZealand:
					return new NewZealandOrgCusCodeInfo();
				case CountryCodes.Oman:
					return new OmanOrgCusCodeInfo();
				case CountryCodes.Panama:
					return new PanamaOrgCusCodeInfo();
				case CountryCodes.Peru:
					return new PeruOrgCusCodeInfo();
				case CountryCodes.FrenchPolynesia:
					return new FrenchPolynesiaOrgCusCodeInfo();
				case CountryCodes.PapuaNewGuinea:
					return new PapuaNewGuineaOrgCusCodeInfo();
				case CountryCodes.Philippines:
					return new PhilippinesOrgCusCodeInfo();
				case CountryCodes.Pakistan:
					return new PakistanOrgCusCodeInfo();
				case CountryCodes.Poland:
					return new PolandOrgCusCodeInfo();
				case CountryCodes.StPierreEtMiquelon:
					return new StPierreEtMiquelonOrgCusCodeInfo();
				case CountryCodes.Pitcairn:
					return new PitcairnOrgCusCodeInfo();
				case CountryCodes.PuertoRico:
					return new PuertoRicoOrgCusCodeInfo();
				case CountryCodes.PalestinianTerritory:
					return new PalestineOrgCusCodeInfo();
				case CountryCodes.Portugal:
					return new PortugalOrgCusCodeInfo();
				case CountryCodes.Palau:
					return new PalauOrgCusCodeInfo();
				case CountryCodes.Paraguay:
					return new ParaguayOrgCusCodeInfo();
				case CountryCodes.Qatar:
					return new QatarOrgCusCodeInfo();
				case CountryCodes.Reunion:
					return new ReunionOrgCusCodeInfo();
				case CountryCodes.Romania:
					return new RomaniaOrgCusCodeInfo();
				case CountryCodes.Serbia:
					return new SerbiaOrgCusCodeInfo();
				case CountryCodes.Russia:
					return new RussiaOrgCusCodeInfo();
				case CountryCodes.Rwanda:
					return new RwandaOrgCusCodeInfo();
				case CountryCodes.SaudiArabia:
					return new SaudiArabiaOrgCusCodeInfo();
				case CountryCodes.SolomonIslands:
					return new SolomonIslandsOrgCusCodeInfo();
				case CountryCodes.Seychelles:
					return new SeychellesOrgCusCodeInfo();
				case CountryCodes.Sudan:
					return new SudanOrgCusCodeInfo();
				case CountryCodes.Sweden:
					return new SwedenOrgCusCodeInfo();
				case CountryCodes.Singapore:
					return new SingaporeOrgCusCodeInfo();
				case CountryCodes.StHelena:
					return new StHelenaOrgCusCodeInfo();
				case CountryCodes.Slovenia:
					return new SloveniaOrgCusCodeInfo();
				case CountryCodes.SvalbardAndJanMayen:
					return new SvalbardAndJanMayenOrgCusCodeInfo();
				case CountryCodes.Slovakia:
					return new SlovakiaOrgCusCodeInfo();
				case CountryCodes.SierraLeone:
					return new SierraLeoneOrgCusCodeInfo();
				case CountryCodes.SanMarino:
					return new SanMarinoOrgCusCodeInfo();
				case CountryCodes.Senegal:
					return new SenegalOrgCusCodeInfo();
				case CountryCodes.Somalia:
					return new SomaliaOrgCusCodeInfo();
				case CountryCodes.Suriname:
					return new SurinameOrgCusCodeInfo();
				case CountryCodes.SouthSudan:
					return new SouthSudanOrgCusCodeInfo();
				case CountryCodes.SaoTomeAndPrincipe:
					return new SaoTomeAndPrincipeOrgCusCodeInfo();
				case CountryCodes.ElSalvador:
					return new ElSalvadorOrgCusCodeInfo();
				case CountryCodes.SintMaarten:
					return new SintMaartenOrgCusCodeInfo();
				case CountryCodes.SyrianArabRepublic:
					return new SyrianArabRepublicOrgCusCodeInfo();
				case CountryCodes.Swaziland:
					return new SwazilandOrgCusCodeInfo();
				case CountryCodes.TurksAndCaicosIslands:
					return new TurksAndCaicosIslandsOrgCusCodeInfo();
				case CountryCodes.Chad:
					return new ChadOrgCusCodeInfo();
				case CountryCodes.FrenchSouthernTerritories:
					return new FrenchSouthernTerritoriesOrgCusCodeInfo();
				case CountryCodes.Togo:
					return new TogoOrgCusCodeInfo();
				case CountryCodes.Thailand:
					return new ThailandOrgCusCodeInfo();
				case CountryCodes.Tajikistan:
					return new TajikistanOrgCusCodeInfo();
				case CountryCodes.Tokelau:
					return new TokelauOrgCusCodeInfo();
				case CountryCodes.TimorLeste:
					return new TimorLesteOrgCusCodeInfo();
				case CountryCodes.Turkmenistan:
					return new TurkmenistanOrgCusCodeInfo();
				case CountryCodes.Tunisia:
					return new TunisiaOrgCusCodeInfo();
				case CountryCodes.Tonga:
					return new TongaOrgCusCodeInfo();
				case CountryCodes.Turkey:
					return new TurkeyOrgCusCodeInfo();
				case CountryCodes.TrinidadAndTobago:
					return new TrinidadAndTobagoOrgCusCodeInfo();
				case CountryCodes.Tuvalu:
					return new TuvaluOrgCusCodeInfo();
				case CountryCodes.Taiwan:
					return new TaiwanOrgCusCodeInfo();
				case CountryCodes.Tanzania:
					return new TanzaniaOrgCusCodeInfo();
				case CountryCodes.Ukraine:
					return new UkraineOrgCusCodeInfo();
				case CountryCodes.Uganda:
					return new UgandaOrgCusCodeInfo();
				case CountryCodes.UnitedStatesMinorIslands:
					return new UnitedStatesMinorIslandsOrgCusCodeInfo();
				case CountryCodes.UnitedStates:
					return new UnitedStatesOrgCusCodeInfo();
				case CountryCodes.Uruguay:
					return new UruguayOrgCusCodeInfo();
				case CountryCodes.Uzbekistan:
					return new UzbekistanOrgCusCodeInfo();
				case CountryCodes.Vatican:
					return new VaticanOrgCusCodeInfo();
				case CountryCodes.SaintVincentAndTheGrenadin:
					return new SaintVincentAndTheGrenadinOrgCusCodeInfo();
				case CountryCodes.Venezuela:
					return new VenezuelaOrgCusCodeInfo();
				case CountryCodes.BritishVirginIslands:
					return new BritishVirginIslandsOrgCusCodeInfo();
				case CountryCodes.VirginIslands:
					return new VirginIslandsOrgCusCodeInfo();
				case CountryCodes.VietNam:
					return new VietnamOrgCusCodeInfo();
				case CountryCodes.Vanuatu:
					return new VanuatuOrgCusCodeInfo();
				case CountryCodes.WallisAndFutunaIslands:
					return new WallisAndFutunaIslandsOrgCusCodeInfo();
				case CountryCodes.WesternSamoa:
					return new WesternSamoaOrgCusCodeInfo();
				case CountryCodes.Kosovo:
					return new KosovoOrgCusCodeInfo();
				case CountryCodes.Yemen:
					return new YemenOrgCusCodeInfo();
				case CountryCodes.Mayotte:
					return new MayotteOrgCusCodeInfo();
				case CountryCodes.SouthAfrica:
					return new SouthAfricaOrgCusCodeInfo();
				case CountryCodes.Zambia:
					return new ZambiaOrgCusCodeInfo();
				case CountryCodes.Zimbabwe:
					return new ZimbabweOrgCusCodeInfo();

				default:
					return new FallbackOrgCusCodeInfo(countryCode);
			}
		}
	}
}
