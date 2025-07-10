using System;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	/// <summary>
	///-------------------------------------------------------------------
	/// 🚩🚩🚩 IMPORTANT: THIS FACTORY MUST NOT BE USED FOR NEW FEATURES.
	///-------------------------------------------------------------------
	///
	/// It just keeps existing features that we did not move to a new Accounting.CountryCompliance solution.
	/// New features must be implemented in new Accounting.CountryCompliance solution.
	/// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/11765/Accounting.CountryCompliance-solution
	/// </summary>
	public class CountryComplianceFactory : ICountryComplianceFactory
	{
		ICountryComplianceInfoBase ICountryComplianceFactory.GetICountryComplianceInfoBase(ZString countryCode) => GetCountryComplianceInfo(countryCode);

		#region Obsolete: this code design is obsolete, don't add any more methods here, use ICountryComplianceInfoBase and cast it for the interface you need where needed.

		IEquivalentComplianceSubTypeProvider ICountryComplianceFactory.GetIEquivalentComplianceSubTypeProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IEquivalentComplianceSubTypeProvider;

		IQRCodeDataProvider ICountryComplianceFactory.GetIQRCodeDataProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IQRCodeDataProvider;

		IComplianceSubTypeAndNumberUpdateRules ICountryComplianceFactory.GetIComplianceSubTypeAndNumberUpdateRules(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeAndNumberUpdateRules;

		IComplianceInfoElectronicInvoicing ICountryComplianceFactory.GetIComplianceInfoElectronicInvoicing(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceInfoElectronicInvoicing;

		IComplianceInfoElectronicInvoicingEligibleSubType ICountryComplianceFactory.GetIComplianceInfoElectronicInvoicingEligibleSubType(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceInfoElectronicInvoicingEligibleSubType;

		IComplianceNumberSequenceConfigurationProvider ICountryComplianceFactory.GetIComplianceNumberSequenceConfigurationProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceNumberSequenceConfigurationProvider;

		ITransactionAuthorisationRecordProvider ICountryComplianceFactory.GetTransactionAuthorisationRecordProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as ITransactionAuthorisationRecordProvider;

		IComplianceRegistryDefaultProvider ICountryComplianceFactory.GetIComplianceRegistryDefaultProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceRegistryDefaultProvider;

		IFiscalTaxCodeProvider ICountryComplianceFactory.GetIFiscalTaxCodeProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IFiscalTaxCodeProvider;

		IComplianceSubTypeRulesWithMultipleRuleSetProvider ICountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(ZString countryCode) => GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(countryCode);

		IOriginalInvoiceReference ICountryComplianceFactory.GetIOriginalInvoiceReference(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IOriginalInvoiceReference;

		ITransactionAuthorizationNumber ICountryComplianceFactory.GetITransactionAuthorizationNumber(ZString countryCode) => GetCountryComplianceInfo(countryCode) as ITransactionAuthorizationNumber;

		IOrgCusCodePredicateProvider ICountryComplianceFactory.GetIOrgCusCodePredicateProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IOrgCusCodePredicateProvider;

		IComplianceSubTypeValidation ICountryComplianceFactory.GetIComplianceSubTypeValidation(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeValidation;

		IComplianceSubTypeGUIProvider ICountryComplianceFactory.GetIComplianceSubTypeGUIProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeGUIProvider;

		IComplianceDocumentStatusProvider ICountryComplianceFactory.GetIComplianceDocumentStatusProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceDocumentStatusProvider;

		IProtectComplianceSubTypeForEInvoicingTransactions ICountryComplianceFactory.GetIProtectComplianceSubTypeForEInvoicingTransactions(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IProtectComplianceSubTypeForEInvoicingTransactions;

		IEInvoicingRegistryProvider ICountryComplianceFactory.GetIEInvoicingRegistryProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IEInvoicingRegistryProvider;

		#region ComplianceSubType

		public static IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider GetIComplianceSubTypeAdditionalTaxRegistrationTypeListProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeAdditionalTaxRegistrationTypeListProvider;

		public static IComplianceSubTypeCodeProvider GetIComplianceSubTypeCodeProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeCodeProvider;

		public static ITaxMessagesGroupProvider GetITaxMessageGroupProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as ITaxMessagesGroupProvider;

		public static IComplianceSubTypeRuleProvider GetIComplianceSubTypeRuleProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeRuleProvider;

		public static IComplianceSubTypeTaxRegistrationTypeRuleProvider GetIComplianceSubTypeTaxRegistrationTypeRuleProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeTaxRegistrationTypeRuleProvider;

		public static IComplianceSubTypeTaxInvoiceRulePrecedenceProvider GetIComplianceSubTypeTaxInvoiceRulePrecedenceProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeTaxInvoiceRulePrecedenceProvider;

		public static IComplianceSubTypeTaxRegistrationTypePrecedenceProvider GetIComplianceSubTypeTaxRegistrationTypePrecedenceProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeTaxRegistrationTypePrecedenceProvider;

		internal static IComplianceSubTypeRulesWithMultipleRuleSetProvider GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeRulesWithMultipleRuleSetProvider;

		internal static IComplianceSubTypeDependencyConfigurationProvider GetIComplianceSubTypeDependencyConfigurationProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceSubTypeDependencyConfigurationProvider;

		#endregion

		internal static ICountryComplianceInfo GetICountryComplianceInfo(ZString countryCode) => GetCountryComplianceInfo(countryCode);

		internal static IComplianceRegistryDefaultProvider GetIComplianceRegistryDefaultProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceRegistryDefaultProvider;

		#region Electronic Invoicing

		public static IComplianceInfoElectronicInvoicing GetICountryComplianceEInvoice(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceInfoElectronicInvoicing;

		public static IComplianceInfoElectronicInvoicingEligibleSubType GetICountryEligibleComplianceSubTypeForEInvoice(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceInfoElectronicInvoicingEligibleSubType;
		public static IBankAccountValidation GetIBankAccountValidator(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IBankAccountValidation;

		public static IComplianceInfoEInvoicingGUIActionProvider GetIComplianceInfoEInvoicingGUIActionProvider(ZString countryCode) => GetCountryComplianceInfo(countryCode) as IComplianceInfoEInvoicingGUIActionProvider;

		#endregion

		#endregion

		public class FallbackCountryComplianceInfo : CountryComplianceInfo
		{
			readonly string countryCode;
			public FallbackCountryComplianceInfo(string countryCode)
			{
				this.countryCode = countryCode;
			}

			#region CountryComplianceInfo

			public override ZString CountryCode => countryCode;
			protected override string GetConsumptionTaxRegistrationCode() => OrgCusCode.CodeTypes.GSTCode;
			protected override string GetConsumptionTaxCode() => OrgCusCode.CodeTypes.GSTCode;
			protected override string GetLocalBusinessRegNoCodeType() => OrgCusCode.CodeTypes.GSTCode;
			protected override bool? GetIsReciprocal() => throw new NotSupportedException("Must decide whether the country is reciprocal or not");

			#endregion
		}

		public static CountryComplianceInfo GetCountryComplianceInfo(ZString countryCode)
		{
			// Please keep the switch expression below in an alphabetical order
			return (string)countryCode switch
			{
				CountryCodes.Afghanistan => new AfghanistanComplianceInfo(),
				CountryCodes.AlandIslands => new AlandIslandsComplianceInfo(),
				CountryCodes.Albania => new AlbaniaComplianceInfo(),
				CountryCodes.Algeria => new AlgeriaComplianceInfo(),
				CountryCodes.AmericanSamoa => new AmericanSamoaComplianceInfo(),
				CountryCodes.Andorra => new AndorraComplianceInfo(),
				CountryCodes.Angola => new AngolaComplianceInfo(),
				CountryCodes.Anguilla => new AnguillaComplianceInfo(),
				CountryCodes.AntiguaAndBarbuda => new AntiguaAndBarbudaComplianceInfo(),
				CountryCodes.Argentina => new ArgentinaComplianceInfo(),
				CountryCodes.Armenia => new ArmeniaComplianceInfo(),
				CountryCodes.Aruba => new ArubaComplianceInfo(),
				CountryCodes.Australia => new AustraliaComplianceInfo(),
				CountryCodes.Austria => new AustriaComplianceInfo(),
				CountryCodes.Azerbaijan => new AzerbaijanComplianceInfo(),
				CountryCodes.Bahamas => new BahamasComplianceInfo(),
				CountryCodes.Bahrain => new BahrainComplianceInfo(),
				CountryCodes.Bangladesh => new BangladeshComplianceInfo(),
				CountryCodes.Barbados => new BarbadosComplianceInfo(),
				CountryCodes.Belarus => new BelarusComplianceInfo(),
				CountryCodes.Belgium => new BelgiumComplianceInfo(),
				CountryCodes.Belize => new BelizeComplianceInfo(),
				CountryCodes.Benin => new BeninComplianceInfo(),
				CountryCodes.Bermuda => new BermudaComplianceInfo(),
				CountryCodes.Bhutan => new BhutanComplianceInfo(),
				CountryCodes.Bolivia => new BoliviaComplianceInfo(),
				CountryCodes.BonaireSintEustatiusAndSaba => new BonaireSintEustatiusAndSabaComplianceInfo(),
				CountryCodes.BosniaAndHerzegovina => new BosniaAndHerzegovinaComplianceInfo(),
				CountryCodes.Botswana => new BotswanaComplianceInfo(),
				CountryCodes.Brazil => new BrazilComplianceInfo(),
				CountryCodes.BritishIndianOceanTerritory => new BritishIndianOceanTerritoryComplianceInfo(),
				CountryCodes.BritishVirginIslands => new BritishVirginIslandsComplianceInfo(),
				CountryCodes.Brunei => new BruneiComplianceInfo(),
				CountryCodes.Bulgaria => new BulgariaComplianceInfo(),
				CountryCodes.BurkinaFaso => new BurkinaFasoComplianceInfo(),
				CountryCodes.Burundi => new BurundiComplianceInfo(),
				CountryCodes.Cambodia => new CambodiaComplianceInfo(),
				CountryCodes.Cameroon => new CameroonComplianceInfo(),
				CountryCodes.Canada => new CanadaComplianceInfo(),
				CountryCodes.CapeVerde => new CapeVerdeComplianceInfo(),
				CountryCodes.CaymanIslands => new CaymanIslandsComplianceInfo(),
				CountryCodes.CentralAfricanRepublic => new CentralAfricanRepublicComplianceInfo(),
				CountryCodes.Chad => new ChadComplianceInfo(),
				CountryCodes.Chile => new ChileComplianceInfo(),
				CountryCodes.China => new ChinaComplianceInfo(),
				CountryCodes.ChristmasIsland => new ChristmasIslandComplianceInfo(),
				CountryCodes.CocosKeelingIslands => new CocosKeelingIslandsComplianceInfo(),
				CountryCodes.Colombia => new ColombiaComplianceInfo(),
				CountryCodes.Comoros => new ComorosComplianceInfo(),
				CountryCodes.Congo => new CongoComplianceInfo(),
				CountryCodes.CookIslands => new CookIslandsComplianceInfo(),
				CountryCodes.CostaRica => new CostaRicaComplianceInfo(),
				CountryCodes.CoteDivoire => new CoteDivoireComplianceInfo(),
				CountryCodes.Croatia => new CroatiaComplianceInfo(),
				CountryCodes.Cuba => new CubaComplianceInfo(),
				CountryCodes.Curacao => new CuracaoComplianceInfo(),
				CountryCodes.Cyprus => new CyprusComplianceInfo(),
				CountryCodes.CzechRepublic => new CzechRepublicComplianceInfo(),
				CountryCodes.DemocraticRepublicOfCongo => new DemocraticRepublicOfCongoComplianceInfo(),
				CountryCodes.Denmark => new DenmarkComplianceInfo(),
				CountryCodes.Djibouti => new DjiboutiComplianceInfo(),
				CountryCodes.Dominica => new DominicaComplianceInfo(),
				CountryCodes.DominicanRepublic => new DominicanRepublicComplianceInfo(),
				CountryCodes.Ecuador => new EcuadorComplianceInfo(),
				CountryCodes.Egypt => new EgyptComplianceInfo(),
				CountryCodes.ElSalvador => new ElSalvadorComplianceInfo(),
				CountryCodes.EquatorialGuinea => new EquatorialGuineaComplianceInfo(),
				CountryCodes.Eritrea => new EritreaComplianceInfo(),
				CountryCodes.Estonia => new EstoniaComplianceInfo(),
				CountryCodes.Ethiopia => new EthiopiaComplianceInfo(),
				CountryCodes.FaeroeIslands => new FaeroeIslandsComplianceInfo(),
				CountryCodes.FalklandIslands => new FalklandIslandsComplianceInfo(),
				CountryCodes.Fiji => new FijiComplianceInfo(),
				CountryCodes.Finland => new FinlandComplianceInfo(),
				CountryCodes.France => new FranceComplianceInfo(),
				CountryCodes.FrenchGuyana => new FrenchGuianaComplianceInfo(),
				CountryCodes.FrenchPolynesia => new FrenchPolynesiaComplianceInfo(),
				CountryCodes.FrenchSouthernTerritories => new FrenchSouthernTerritoriesComplianceInfo(),
				CountryCodes.Gabon => new GabonComplianceInfo(),
				CountryCodes.Gambia => new GambiaComplianceInfo(),
				CountryCodes.Georgia => new GeorgiaComplianceInfo(),
				CountryCodes.Germany => new GermanyComplianceInfo(),
				CountryCodes.Ghana => new GhanaComplianceInfo(),
				CountryCodes.Gibraltar => new GibraltarComplianceInfo(),
				CountryCodes.Greece => new GreeceComplianceInfo(),
				CountryCodes.Greenland => new GreenlandComplianceInfo(),
				CountryCodes.Grenada => new GrenadaComplianceInfo(),
				CountryCodes.Guadeloupe => new GuadeloupeComplianceInfo(),
				CountryCodes.Guam => new GuamComplianceInfo(),
				CountryCodes.Guatemala => new GuatemalaComplianceInfo(),
				CountryCodes.Guernsey => new GuernseyComplianceInfo(),
				CountryCodes.Guinea => new GuineaComplianceInfo(),
				CountryCodes.GuineaBissau => new GuineaBissauComplianceInfo(),
				CountryCodes.Guyana => new GuyanaComplianceInfo(),
				CountryCodes.Haiti => new HaitiComplianceInfo(),
				CountryCodes.HeardAndMcdonaldIslands => new HeardAndMcdonaldIslandsComplianceInfo(),
				CountryCodes.Honduras => new HondurasComplianceInfo(),
				CountryCodes.HongKong => new HongKongComplianceInfo(),
				CountryCodes.Hungary => new HungaryComplianceInfo(),
				CountryCodes.Iceland => new IcelandComplianceInfo(),
				CountryCodes.India => new IndiaComplianceInfo(),
				CountryCodes.Indonesia => new IndonesiaComplianceInfo(),
				CountryCodes.Iran => new IranComplianceInfo(),
				CountryCodes.Iraq => new IraqComplianceInfo(),
				CountryCodes.Ireland => new IrelandComplianceInfo(),
				CountryCodes.IsleOfMan => new IsleOfManComplianceInfo(),
				CountryCodes.Israel => new IsraelComplianceInfo(),
				CountryCodes.Italy => new ItalyComplianceInfo(),
				CountryCodes.Jamaica => new JamaicaComplianceInfo(),
				CountryCodes.Japan => new JapanComplianceInfo(),
				CountryCodes.Jersey => new JerseyComplianceInfo(),
				CountryCodes.Jordan => new JordanComplianceInfo(),
				CountryCodes.Kazakhstan => new KazakhstanComplianceInfo(),
				CountryCodes.Kenya => new KenyaComplianceInfo(),
				CountryCodes.Kiribati => new KiribatiComplianceInfo(),
				CountryCodes.KoreaNorth => new KoreaNorthComplianceInfo(),
				CountryCodes.KoreaSouth => new KoreaSouthComplianceInfo(),
				CountryCodes.Kosovo => new KosovoComplianceInfo(),
				CountryCodes.Kuwait => new KuwaitComplianceInfo(),
				CountryCodes.Kyrgyzstan => new KyrgyzstanComplianceInfo(),
				CountryCodes.LaoPeoplesDemocraticRepublic => new LaoPeoplesDemocraticRepublicComplianceInfo(),
				CountryCodes.Latvia => new LatviaComplianceInfo(),
				CountryCodes.Lebanon => new LebanonComplianceInfo(),
				CountryCodes.Lesotho => new LesothoComplianceInfo(),
				CountryCodes.Liberia => new LiberiaComplianceInfo(),
				CountryCodes.LibyanArabJamahiriya => new LibyanArabJamahiriyaComplianceInfo(),
				CountryCodes.Liechtenstein => new LiechtensteinComplianceInfo(),
				CountryCodes.Lithuania => new LithuaniaComplianceInfo(),
				CountryCodes.Luxembourg => new LuxembourgComplianceInfo(),
				CountryCodes.Macau => new MacauComplianceInfo(),
				CountryCodes.Macedonia => new MacedoniaComplianceInfo(),
				CountryCodes.Madagascar => new MadagascarComplianceInfo(),
				CountryCodes.Malawi => new MalawiComplianceInfo(),
				CountryCodes.Malaysia => new MalaysiaComplianceInfo(),
				CountryCodes.Maldives => new MaldivesComplianceInfo(),
				CountryCodes.Mali => new MaliComplianceInfo(),
				CountryCodes.Malta => new MaltaComplianceInfo(),
				CountryCodes.MarshallIslands => new MarshallIslandsComplianceInfo(),
				CountryCodes.Martinique => new MartiniqueComplianceInfo(),
				CountryCodes.Mauritania => new MauritaniaComplianceInfo(),
				CountryCodes.Mauritius => new MauritiusComplianceInfo(),
				CountryCodes.Mayotte => new MayotteComplianceInfo(),
				CountryCodes.Mexico => new MexicoComplianceInfo(),
				CountryCodes.Micronesia => new MicronesiaComplianceInfo(),
				CountryCodes.Moldova => new MoldovaComplianceInfo(),
				CountryCodes.Monaco => new MonacoComplianceInfo(),
				CountryCodes.Mongolia => new MongoliaComplianceInfo(),
				CountryCodes.Montenegro => new MontenegroComplianceInfo(),
				CountryCodes.Montserrat => new MontserratComplianceInfo(),
				CountryCodes.Morocco => new MoroccoComplianceInfo(),
				CountryCodes.Mozambique => new MozambiqueComplianceInfo(),
				CountryCodes.Myanmar => new MyanmarComplianceInfo(),
				CountryCodes.Namibia => new NamibiaComplianceInfo(),
				CountryCodes.Nauru => new NauruComplianceInfo(),
				CountryCodes.Nepal => new NepalComplianceInfo(),
				CountryCodes.Netherlands => new NetherlandsComplianceInfo(),
				CountryCodes.NetherlandsAntilles => new NetherlandsAntillesComplianceInfo(),
				CountryCodes.NewCaledonia => new NewCaledoniaComplianceInfo(),
				CountryCodes.NewZealand => new NewZealandComplianceInfo(),
				CountryCodes.Nicaragua => new NicaraguaComplianceInfo(),
				CountryCodes.Niger => new NigerComplianceInfo(),
				CountryCodes.Nigeria => new NigeriaComplianceInfo(),
				CountryCodes.Niue => new NiueComplianceInfo(),
				CountryCodes.NorfolkIsland => new NorfolkIslandComplianceInfo(),
				CountryCodes.NorthernMarianaIslands => new NorthernMarianaIslandsComplianceInfo(),
				CountryCodes.Norway => new NorwayComplianceInfo(),
				CountryCodes.Oman => new OmanComplianceInfo(),
				CountryCodes.Pakistan => new PakistanComplianceInfo(),
				CountryCodes.Palau => new PalauComplianceInfo(),
				CountryCodes.PalestinianTerritory => new PalestineComplianceInfo(),
				CountryCodes.Panama => new PanamaComplianceInfo(),
				CountryCodes.PapuaNewGuinea => new PapuaNewGuineaComplianceInfo(),
				CountryCodes.Paraguay => new ParaguayComplianceInfo(),
				CountryCodes.Peru => new PeruComplianceInfo(),
				CountryCodes.Philippines => new PhilippinesComplianceInfo(),
				CountryCodes.Pitcairn => new PitcairnComplianceInfo(),
				CountryCodes.Poland => new PolandComplianceInfo(),
				CountryCodes.Portugal => new PortugalComplianceInfo(),
				CountryCodes.PuertoRico => new PuertoRicoComplianceInfo(),
				CountryCodes.Qatar => new QatarComplianceInfo(),
				CountryCodes.Reunion => new ReunionComplianceInfo(),
				CountryCodes.Romania => new RomaniaComplianceInfo(),
				CountryCodes.Russia => new RussiaComplianceInfo(),
				CountryCodes.Rwanda => new RwandaComplianceInfo(),
				CountryCodes.SaintBarthelemy => new SaintBarthelemyComplianceInfo(),
				CountryCodes.SaintKittsAndNevis => new SaintKittsAndNevisComplianceInfo(),
				CountryCodes.SaintLucia => new SaintLuciaComplianceInfo(),
				CountryCodes.SaintMartin => new SaintMartinComplianceInfo(),
				CountryCodes.SaintVincentAndTheGrenadin => new SaintVincentAndTheGrenadinComplianceInfo(),
				CountryCodes.SanMarino => new SanMarinoComplianceInfo(),
				CountryCodes.SaoTomeAndPrincipe => new SaoTomeAndPrincipeComplianceInfo(),
				CountryCodes.SaudiArabia => new SaudiArabiaComplianceInfo(),
				CountryCodes.Senegal => new SenegalComplianceInfo(),
				CountryCodes.Serbia => new SerbiaComplianceInfo(),
				CountryCodes.Seychelles => new SeychellesComplianceInfo(),
				CountryCodes.SierraLeone => new SierraLeoneComplianceInfo(),
				CountryCodes.Singapore => new SingaporeComplianceInfo(),
				CountryCodes.SintMaarten => new SintMaartenComplianceInfo(),
				CountryCodes.Slovakia => new SlovakiaComplianceInfo(),
				CountryCodes.Slovenia => new SloveniaComplianceInfo(),
				CountryCodes.SolomonIslands => new SolomonIslandsComplianceInfo(),
				CountryCodes.Somalia => new SomaliaComplianceInfo(),
				CountryCodes.SouthAfrica => new SouthAfricaComplianceInfo(),
				CountryCodes.SouthSudan => new SouthSudanComplianceInfo(),
				CountryCodes.Spain => new SpainComplianceInfo(),
				CountryCodes.SriLanka => new SriLankaComplianceInfo(),
				CountryCodes.StHelena => new StHelenaComplianceInfo(),
				CountryCodes.StPierreEtMiquelon => new StPierreEtMiquelonComplianceInfo(),
				CountryCodes.Sudan => new SudanComplianceInfo(),
				CountryCodes.Suriname => new SurinameComplianceInfo(),
				CountryCodes.SvalbardAndJanMayen => new SvalbardAndJanMayenComplianceInfo(),
				CountryCodes.Swaziland => new SwazilandComplianceInfo(),
				CountryCodes.Sweden => new SwedenComplianceInfo(),
				CountryCodes.Switzerland => new SwitzerlandComplianceInfo(),
				CountryCodes.SyrianArabRepublic => new SyrianArabRepublicComplianceInfo(),
				CountryCodes.Taiwan => new TaiwanComplianceInfo(),
				CountryCodes.Tajikistan => new TajikistanComplianceInfo(),
				CountryCodes.Tanzania => new TanzaniaComplianceInfo(),
				CountryCodes.Thailand => new ThailandComplianceInfo(),
				CountryCodes.TimorLeste => new TimorLesteComplianceInfo(),
				CountryCodes.Togo => new TogoComplianceInfo(),
				CountryCodes.Tokelau => new TokelauComplianceInfo(),
				CountryCodes.Tonga => new TongaComplianceInfo(),
				CountryCodes.TrinidadAndTobago => new TrinidadAndTobagoComplianceInfo(),
				CountryCodes.Tunisia => new TunisiaComplianceInfo(),
				CountryCodes.Turkey => new TurkeyComplianceInfo(),
				CountryCodes.Turkmenistan => new TurkmenistanComplianceInfo(),
				CountryCodes.TurksAndCaicosIslands => new TurksAndCaicosIslandsComplianceInfo(),
				CountryCodes.Tuvalu => new TuvaluComplianceInfo(),
				CountryCodes.Uganda => new UgandaComplianceInfo(),
				CountryCodes.Ukraine => new UkraineComplianceInfo(),
				CountryCodes.UnitedArabEmirates => new UnitedArabEmiratesComplianceInfo(),
				CountryCodes.UnitedKingdom => new UnitedKingdomComplianceInfo(),
				CountryCodes.UnitedStates => new UnitedStatesComplianceInfo(),
				CountryCodes.UnitedStatesMinorIslands => new UnitedStatesMinorIslandsComplianceInfo(),
				CountryCodes.Uruguay => new UruguayComplianceInfo(),
				CountryCodes.Uzbekistan => new UzbekistanComplianceInfo(),
				CountryCodes.Vanuatu => new VanuatuComplianceInfo(),
				CountryCodes.Vatican => new VaticanComplianceInfo(),
				CountryCodes.Venezuela => new VenezuelaComplianceInfo(),
				CountryCodes.VietNam => new VietnamComplianceInfo(),
				CountryCodes.VirginIslands => new VirginIslandsComplianceInfo(),
				CountryCodes.WallisAndFutunaIslands => new WallisAndFutunaIslandsComplianceInfo(),
				CountryCodes.WesternSahara => new WesternSaharaComplianceInfo(),
				CountryCodes.WesternSamoa => new SamoaComplianceInfo(),
				CountryCodes.Yemen => new YemenComplianceInfo(),
				CountryCodes.Zambia => new ZambiaComplianceInfo(),
				CountryCodes.Zimbabwe => new ZimbabweComplianceInfo(),
				_ => new FallbackCountryComplianceInfo(countryCode),
			};
			// Please keep the switch expression above in an alphabetical order
		}

		[CodeAlive("Used through interface declared in Spring.Net xml")]
		public class CrossAssemblyAccess : ICountryComplianceFactoryIntegration
		{
			ICountryComplianceInfo ICountryComplianceFactoryIntegration.GetICountryComplianceInfo(ZString countryCode) => GetICountryComplianceInfo(countryCode);

			IComplianceSubTypeCodeProvider ICountryComplianceFactoryIntegration.GetIComplianceSubTypeCodeProvider(ZString countryCode) => GetIComplianceSubTypeCodeProvider(countryCode);
		}
	}
}
