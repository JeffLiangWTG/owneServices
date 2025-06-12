using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class Constants
  {
    public static class CountryCodes
    {
      //Copy from Enterprise/Product/Core/Resources/Resources/Constants/CountryCodes.cs
      public const string _TemplateCountryName_ = "AI";       // Anguilla
      public const string _EUTemplateCountryName_ = "MT";     // Malta

      public const string Afghanistan = "AF";
      public const string AlandIslands = "AX";  // Åland Islands, see https://en.wikipedia.org/wiki/ISO_3166-2:AX
      public const string Albania = "AL";
      public const string Algeria = "DZ";
      public const string AmericanSamoa = "AS";
      public const string Andorra = "AD";
      public const string Angola = "AO";
      public const string Anguilla = "AI";
      public const string Antarctica = "AQ";
      public const string AntiguaAndBarbuda = "AG";
      public const string Argentina = "AR";
      public const string Armenia = "AM";
      public const string Aruba = "AW";
      public const string Australia = "AU";
      public const string Austria = "AT";
      public const string Azerbaijan = "AZ";
      public const string Bahamas = "BS";
      public const string Bahrain = "BH";
      public const string Bangladesh = "BD";
      public const string Barbados = "BB";
      public const string Belarus = "BY";
      public const string Belgium = "BE";
      public const string Belize = "BZ";
      public const string Benin = "BJ";
      public const string Bermuda = "BM";
      public const string Bhutan = "BT";
      public const string Bolivia = "BO";
      public const string BonaireSintEustatiusAndSaba = "BQ";
      public const string BosniaAndHerzegovina = "BA";
      public const string Botswana = "BW";
      public const string Brazil = "BR";
      public const string BritishIndianOceanTerritory = "IO";
      public const string BritishVirginIslands = "VG";
      public const string Brunei = "BN";
      public const string Bulgaria = "BG";
      public const string BurkinaFaso = "BF";
      public const string Burundi = "BI";
      public const string Cambodia = "KH";
      public const string Cameroon = "CM";
      public const string Canada = "CA";
      public const string CapeVerde = "CV";
      public const string CaymanIslands = "KY";
      public const string CentralAfricanRepublic = "CF";
      public const string Chad = "TD";
      public const string Chile = "CL";
      public const string China = "CN";
      public const string ChristmasIsland = "CX";
      public const string CocosKeelingIslands = "CC";
      public const string Colombia = "CO";
      public const string Comoros = "KM";
      public const string Congo = "CG";
      public const string CookIslands = "CK";
      public const string CostaRica = "CR";
      public const string CoteDivoire = "CI";
      public const string Croatia = "HR";
      public const string Cuba = "CU";
      public const string Curacao = "CW";
      public const string Cyprus = "CY";
      public const string CzechRepublic = "CZ";
      public const string DemocraticRepublicOfCongo = "CD";
      public const string Denmark = "DK";
      public const string Djibouti = "DJ";
      public const string Dominica = "DM";
      public const string DominicanRepublic = "DO";
      public const string EastTimor = "TP";
      public const string Ecuador = "EC";
      public const string Egypt = "EG";
      public const string ElSalvador = "SV";
      public const string EquatorialGuinea = "GQ";
      public const string Eritrea = "ER";
      public const string Estonia = "EE";
      public const string Ethiopia = "ET";
      public const string EuropeanUnion = "EU";
      public const string FaeroeIslands = "FO";
      public const string FalklandIslands = "FK";
      public const string Fiji = "FJ";
      public const string Finland = "FI";
      public const string FishFromForeignBoats = "ZF";
      public const string France = "FR";
      public const string FrenchGuyana = "GF";
      public const string FrenchPolynesia = "PF";
      public const string FrenchSouthernTerritories = "TF";
      public const string Gabon = "GA";
      public const string Gambia = "GM";
      public const string Georgia = "GE";
      public const string Germany = "DE";
      public const string Ghana = "GH";
      public const string Gibraltar = "GI";
      public const string Greece = "GR";
      public const string Greenland = "GL";
      public const string Grenada = "GD";
      public const string Guadeloupe = "GP";
      public const string Guam = "GU";
      public const string Guatemala = "GT";
      public const string Guernsey = "GG";
      public const string Guinea = "GN";
      public const string GuineaBissau = "GW";
      public const string Guyana = "GY";
      public const string Haiti = "HT";
      public const string Honduras = "HN";
      public const string HongKong = "HK";
      public const string Hungary = "HU";
      public const string Iceland = "IS";
      public const string India = "IN";
      public const string Indonesia = "ID";
      public const string Iran = "IR";
      public const string Iraq = "IQ";
      public const string Ireland = "IE";
      public const string IsleOfMan = "IM";
      public const string Israel = "IL";
      public const string Italy = "IT";
      public const string Jamaica = "JM";
      public const string Japan = "JP";
      public const string Jersey = "JE";
      public const string Jordan = "JO";
      public const string Kazakhstan = "KZ";
      public const string Kenya = "KE";
      public const string Kiribati = "KI";
      public const string KoreaNorth = "KP";
      public const string KoreaSouth = "KR";
      public const string Kosovo = "XK";
      public const string Kuwait = "KW";
      public const string Kyrgyzstan = "KG";
      public const string LaoPeoplesDemocraticRepublic = "LA";
      public const string Latvia = "LV";
      public const string Lebanon = "LB";
      public const string Lesotho = "LS";
      public const string Liberia = "LR";
      public const string LibyanArabJamahiriya = "LY";
      public const string Liechtenstein = "LI";
      public const string Lithuania = "LT";
      public const string Luxembourg = "LU";
      public const string Macau = "MO";
      public const string Macedonia = "MK";
      public const string Madagascar = "MG";
      public const string Malawi = "MW";
      public const string Malaysia = "MY";
      public const string Maldives = "MV";
      public const string Mali = "ML";
      public const string Malta = "MT";
      public const string MarshallIslands = "MH";
      public const string Martinique = "MQ";
      public const string Mauritania = "MR";
      public const string Mauritius = "MU";
      public const string Mayotte = "YT";
      public const string Mexico = "MX";
      public const string Micronesia = "FM";
      public const string Moldova = "MD";
      public const string Monaco = "MC";
      public const string Mongolia = "MN";
      public const string Montenegro = "ME";
      public const string Montserrat = "MS";
      public const string Morocco = "MA";
      public const string Mozambique = "MZ";
      public const string Myanmar = "MM";
      public const string Namibia = "NA";
      public const string Nauru = "NR";
      public const string Nepal = "NP";
      public const string Netherlands = "NL";
      public const string NetherlandsAntilles = "AN";
      public const string NeutralZone = "NT";
      public const string NewCaledonia = "NC";
      public const string NewZealand = "NZ";
      public const string Nicaragua = "NI";
      public const string Niger = "NE";
      public const string Nigeria = "NG";
      public const string Niue = "NU";
      public const string NorfolkIsland = "NF";
      public const string NorthernIreland_ForUseOnlyByEuInCertainScopes = "XI";
      public const string NorthernMarianaIslands = "MP";
      public const string Norway = "NO";
      public const string Oman = "OM";
      public const string Pakistan = "PK";
      public const string Palau = "PW";
      public const string PalestinianTerritory = "PS";
      public const string Panama = "PA";
      public const string PapuaNewGuinea = "PG";
      public const string Paraguay = "PY";
      public const string Peru = "PE";
      public const string Philippines = "PH";
      public const string Pitcairn = "PN";
      public const string Poland = "PL";
      public const string Portugal = "PT";
      public const string PuertoRico = "PR";
      public const string Qatar = "QA";
      public const string Reunion = "RE";
      public const string Romania = "RO";
      public const string Russia = "RU";
      public const string Rwanda = "RW";
      public const string SaintKittsAndNevis = "KN";
      public const string SaintLucia = "LC";
      public const string SaintVincentAndTheGrenadin = "VC";
      public const string SanMarino = "SM";
      public const string SaoTomeAndPrincipe = "ST";
      public const string SaudiArabia = "SA";
      public const string Senegal = "SN";
      public const string Serbia = "RS";
      public const string Serbia_ForEUTrading = "XS";
      public const string Seychelles = "SC";
      public const string SierraLeone = "SL";
      public const string Singapore = "SG";
      public const string SintMaarten = "SX";
      public const string Slovakia = "SK";
      public const string Slovenia = "SI";
      public const string SolomonIslands = "SB";
      public const string Somalia = "SO";
      public const string SouthAfrica = "ZA";
      public const string SouthSudan = "SS";
      public const string Spain = "ES";
      public const string SriLanka = "LK";
      public const string StHelena = "SH";
      public const string StPierreEtMiquelon = "PM";
      public const string Sudan = "SD";
      public const string Suriname = "SR";
      public const string SvalbardAndJanMayen = "SJ";
      public const string Swaziland = "SZ";
      public const string Sweden = "SE";
      public const string Switzerland = "CH";
      public const string SyrianArabRepublic = "SY";
      public const string Taiwan = "TW";
      public const string Tajikistan = "TJ";
      public const string Tanzania = "TZ";
      public const string Thailand = "TH";
      public const string TimorLeste = "TL";
      public const string Togo = "TG";
      public const string Tokelau = "TK";
      public const string Tonga = "TO";
      public const string TrinidadAndTobago = "TT";
      public const string Tunisia = "TN";
      public const string Turkey = "TR";
      public const string Turkmenistan = "TM";
      public const string TurksAndCaicosIslands = "TC";
      public const string Tuvalu = "TV";
      public const string Uganda = "UG";
      public const string Ukraine = "UA";
      public const string UnitedArabEmirates = "AE";
      public const string UnitedKingdom = "GB";
      public const string UnitedStates = "US";
      public const string UnitedStatesMinorIslands = "UM";
      public const string Uruguay = "UY";
      public const string Uzbekistan = "UZ";
      public const string Vanuatu = "VU";
      public const string Vatican = "VA";
      public const string Venezuela = "VE";
      public const string VietNam = "VN";
      public const string VirginIslands = "VI";
      public const string WallisAndFutunaIslands = "WF";
      public const string WesternSahara = "EH";
      public const string WesternSamoa = "WS";
      public const string Yemen = "YE";
      public const string Yugoslavia = "YU";
      public const string Zaire = "ZR";
      public const string Zambia = "ZM";
      public const string Zimbabwe = "ZW";
      public const string HeardAndMcdonaldIslands = "HM";
      public const string SaintBarthelemy = "BL";
      public const string SaintMartin = "MF";
    }

    public static readonly List<string> CountriesInEuropeanUnion = new List<string>
    {
      CountryCodes.Austria,
      CountryCodes.Belgium,
      CountryCodes.Bulgaria,
      CountryCodes.Croatia,
      CountryCodes.Cyprus,
      CountryCodes.CzechRepublic,
      CountryCodes.Denmark,
      CountryCodes.Estonia,
      CountryCodes.Finland,
      CountryCodes.France,
      CountryCodes.Germany,
      CountryCodes.Greece,
      CountryCodes.Hungary,
      CountryCodes.Ireland,
      CountryCodes.Italy,
      CountryCodes.Latvia,
      CountryCodes.Lithuania,
      CountryCodes.Luxembourg,
      CountryCodes.Malta,
      CountryCodes.Netherlands,
      CountryCodes.Poland,
      CountryCodes.Portugal,
      CountryCodes.Romania,
      CountryCodes.Slovakia,
      CountryCodes.Slovenia,
      CountryCodes.Spain,
      CountryCodes.Sweden
    };
  }
}