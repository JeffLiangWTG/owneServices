using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class MIDGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateMID()
		{
			AssertEquals("MID generated", "FRLAVIE243BRE", new MIDGenerator().GenerateMID("LA VIE DE FRANCE", "243 Rue de la Payees", "Bremond", "FR"));

			AssertEquals("MID generated", "VE20TCEN5880CAR", new MIDGenerator().GenerateMID("20TH CENTURY TECHNOLOGIES", "1234 Ricardo Munoz, Suite 5880", "Caracas", "VE"));

			AssertEquals("MID generated", "GBEKRODLON", new MIDGenerator().GenerateMID("THE E.K. RODGERS COMPANIES", "One World Trade Center", "London", "GB"));

			AssertEquals("MID generated", "USGRE45BIR", new MIDGenerator().GenerateMID("THE GREENHOUSE", "45 Royal Crescent", "Birmingham", "US"));

			AssertEquals("MID generated", "AUCARJON88SID", new MIDGenerator().GenerateMID("CARDUCCIO AND JONES", "88 Canburra Avenue", "Sidney", "AU"));

			AssertEquals("MID generated", "KRABCOM1234PUS", new MIDGenerator().GenerateMID("A B Company", "1234 Main Road", "PUSAN", "KR"));

			AssertEquals("MID generated", "ITBOCSPA61VER", new MIDGenerator().GenerateMID("BOCCHACCIO S.P.A", "Via Mendotti, 61", "Verona", "IT"));

			AssertEquals("MID generated", "GRMURINCATH", new MIDGenerator().GenerateMID("MURLA-PRAXITELES INC.", "", "Athens", "GR"));

			AssertEquals("MID generated", "ITSIGCOY1640SMY", new MIDGenerator().GenerateMID("SIGMA COY E.X.T.", "1640 Delgado", "Smyrna", "IT"));

			AssertEquals("MID generated", "XAALPINT2155VAN", new MIDGenerator().GenerateMID("ALPHANTRANS INTERNATIONAL", "215-5000 MILLER ROAD", "VANCOUVER AIRPORT", "XA"));

			AssertEquals("MID generated", "JPMINCO26KOB", new MIDGenerator().GenerateMID("N. MINAMI & CO. LTD.", "2-6, 8-Chome Isogami-Dori,Fukiai-Ku", "Kobe", "JP"));

			AssertEquals("MID generated", "AUAAAAUT0105GRE", new MIDGenerator().GenerateMID("AAA Automobile", "1-5 CNR RESERVOIR AVIMOBIL : 0105332642", "Greensquare", "AU"));

			AssertEquals("MID generated", "AUAAACUS111BUR", new MIDGenerator().GenerateMID("A A A Cus", "13 ELLIOT 111  COURT", "Burwood", "AU"));

			AssertEquals("MID generated", "PTKAR2527LIS", new MIDGenerator().GenerateMID("COMPANHIA TEXTIL KARSTEN", "Calle Grande, 25-27", "67890 Lisbon", "PT"));

			AssertEquals("MID generated", "MOJUMHIG2527MAC", new MIDGenerator().GenerateMID("FABRICA DE ARTIGOS DE VESTUARIO JUMP HIGH Ltd", "Calle Grande, 25-27", "67890 Lisbon", "MO"));

			AssertEquals("MID generated", "HKJUMHIG2527HON", new MIDGenerator().GenerateMID("JUMP HIGH Ltd", "Calle Grande, 25-27", "67890 KowLoon", "HK"));

			AssertEquals("MID generated", "HKJUMHIG1234HON", new MIDGenerator().GenerateMID("JUMP HIGH Ltd", "12,34,56 Alaska Road", "67890 KowLoon", "HK"));

			AssertEquals("MID generated", "AUOASCOR111BUR", new MIDGenerator().GenerateMID("O.A.S.I.S. Corp.", "13 ELLIOT 111 COURT", "Burwood", "AU"));

			AssertEquals("MID generated", "AUDRSA111BUR", new MIDGenerator().GenerateMID("Dr. S.A. Smith", "13 ELLIOT 111 COURT", "Burwood", "AU"));

			AssertEquals("MID generated", "AUSHABL111BUR", new MIDGenerator().GenerateMID("Shavings B L", "13 ELLIOT 111 COURT", "Burwood", "AU"));

			AssertEquals("MID generated", "AUCON200111BUR", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "Burwood", "AU"));

			AssertEquals("MID generated", "AUCON200111STM", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "St. Michel", "AU"));

			AssertEquals("MID generated", "AUCON200111MIL", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "18-Mile High", "AU"));

			AssertEquals("MID generated", "AUCON200111HAG", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "The Hague", "AU"));

			AssertEquals("MID generated", "AUCON200111HON", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "Hong Kong", "AU"));

			AssertEquals("MID generated", "AUCON200111SIN", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "Singapore", "AU"));

			AssertEquals("MID generated", "AUCON200111MAC", new MIDGenerator().GenerateMID("Concept 2000", "13 ELLIOT 111 COURT", "Macau", "AU"));

			AssertEquals("MID generated", "IDMORIND111SIN", new MIDGenerator().GenerateMID("PT Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "ID"));

			AssertEquals("MID generated", "IDMORIND111SIN", new MIDGenerator().GenerateMID("PT. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "ID"));

			AssertEquals("MID generated", "IDAPTMOR111SIN", new MIDGenerator().GenerateMID("APT Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "ID"));

			AssertEquals("MID generated", "IDAPTMOR111SIN", new MIDGenerator().GenerateMID("APT. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "ID"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("JSC Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("JSC. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("OAO Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("OAO. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("OOO Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("OOO. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("ZAO Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND111SIN", new MIDGenerator().GenerateMID("ZAO. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORZAO111SIN", new MIDGenerator().GenerateMID("Morich ZAO Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "RU"));

			AssertEquals("MID generated", "RUMORIND2727SIN", new MIDGenerator().GenerateMID(" ZAO Morich Indo Fashion.", " Apt. 509 2727 Cleveland St.", "Singapore", "RU"));

			AssertEquals("MID generated", "MOTHODEL2727MAC", new MIDGenerator().GenerateMID("Thomas S. Delvaux Company.", "Apt. 509 2727 Cleveland St.", "Singapore", "MO"));

			AssertEquals("MID generated", "AUCON200111HAG", new MIDGenerator().GenerateMID("Concept, 2000", "13 ELLIOT, 111 COURT", "The Hague", "AU"));

			AssertEquals("MID generated", "MOTHODEL2727MAC", new MIDGenerator().GenerateMID("Thomas S. Delvaux Company.", "Apt. 509 2727 Cleveland St.", "Singapore, One", "MO"));

			AssertEquals("MID generated", "MOTHODEL2727MAC", new MIDGenerator().GenerateMID("Thomas S. Delvaux Company.", "    Apt. 509 2727 Cleveland St.", "Singapore, One", "MO"));

			AssertEquals("MID generated", "XAALPINT2155VAN", new MIDGenerator().GenerateMID("AL'PHANTRANS INTERNATIONAL", "215-5000 MILLER ROAD", "VANCOUVER AIRPORT", "XA"));

			AssertEquals("MID generated", "XAALPINT2155VAN", new MIDGenerator().GenerateMID("ALPHANTRANS & INTERNATIONAL", "215-5000 MILLER ROAD", "VANCOUVER AIRPORT", "XA"));

			AssertEquals("MID generated", "XAALPHA2155VAN", new MIDGenerator().GenerateMID("AL & PHANTRANS INTERNATIONAL", "215-5000 MILLER ROAD", "VANCOUVER AIRPORT", "XA"));

			AssertEquals("MID generated", "USGRE45BIR", new MIDGenerator().GenerateMID("THE GREENHOUSE", "45 Royal. Crescent", "Birmingham", "US"));

			AssertEquals("MID generated", "IDPTMIND111SIN", new MIDGenerator().GenerateMID("PT-Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "ID"));

			AssertEquals("MID generated", "IDPTMOR111SIN", new MIDGenerator().GenerateMID("PT- Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Singapore", "ID"));

			AssertEquals("MID generated", "FRSOLVAN1897GRA", new MIDGenerator().GenerateMID("solune/vanessa bruno", "13 ELLIOT 189/7 COURT", "Grass", "FR"));
		}

		public void TestAdditionalRules()
		{
			AssertEquals("Hong Kong special rule", "HKALFJAY111HON", new MIDGenerator().GenerateMID("Alfa Jaya", "13 ELLIOT 111 COURT", "Kowloon", Core.Constants.CountryCodes.HongKong));

			AssertEquals("Makao special rule", "MOPAKLEI2527MAC", new MIDGenerator().GenerateMID("Fabrica de Artigos de Vestuario Pak Lei", "Calle Grande, 25-27", "67890 Lisbon", Core.Constants.CountryCodes.Macau));
			AssertEquals("Makao special rule", "MOPAKLEI2527MAC", new MIDGenerator().GenerateMID("FABRICA DE ARTIGOS DE VESTUARIO PAK LEI", "Calle Grande, 25-27", "67890 Lisbon", Core.Constants.CountryCodes.Macau));
			AssertEquals("Makao special rule", "MOKINFAI2527MAC", new MIDGenerator().GenerateMID("Fabrica de King Fai", "Calle Grande, 25-27", "67890 Lisbon", Core.Constants.CountryCodes.Macau));
			AssertEquals("Makao special rule", "MOVENON2527MAC", new MIDGenerator().GenerateMID("Artigos de Vestuario Veng On", "Calle Grande, 25-27", "67890 Lisbon", Core.Constants.CountryCodes.Macau));

			AssertEquals("City-States rule: singapore", "SGALFJAY111SIN", new MIDGenerator().GenerateMID("Alfa Jaya", "13 ELLIOT 111 COURT", "Test", Core.Constants.CountryCodes.Singapore));
			AssertEquals("City-States rule: vatican", "VAALFJAY111VAT", new MIDGenerator().GenerateMID("Alfa Jaya", "13 ELLIOT 111 COURT", "Test", Core.Constants.CountryCodes.Vatican));
			AssertEquals("City-States rule: monaco", "MCALFJAY111MON", new MIDGenerator().GenerateMID("Alfa Jaya", "13 ELLIOT 111 COURT", "Test", Core.Constants.CountryCodes.Monaco));
			AssertEquals("City-States rule: san marino", "SMALFJAY111SAN", new MIDGenerator().GenerateMID("Alfa Jaya", "13 ELLIOT 111 COURT", "Test", Core.Constants.CountryCodes.SanMarino));
			AssertEquals("City-States rule: andorra", "ADALFJAY111AND", new MIDGenerator().GenerateMID("Alfa Jaya", "13 ELLIOT 111 COURT", "Test", Core.Constants.CountryCodes.Andorra));

			AssertEquals("Indonesia special rule", "IDALFJAY111JAK", new MIDGenerator().GenerateMID("PT. Alfa Jaya", "13 ELLIOT 111 COURT", "Jakarta", Core.Constants.CountryCodes.Indonesia));
			AssertEquals("Indonesia special rule", "IDALFJAY111JAK", new MIDGenerator().GenerateMID("P.T. Alfa Jaya", "13 ELLIOT 111 COURT", "Jakarta", Core.Constants.CountryCodes.Indonesia));
			AssertEquals("Indonesia special rule", "IDALFJAY111JAK", new MIDGenerator().GenerateMID("pt Alfa Jaya", "13 ELLIOT 111 COURT", "Jakarta", Core.Constants.CountryCodes.Indonesia));
			AssertEquals("Indonesia special rule", "IDALFJAY111JAK", new MIDGenerator().GenerateMID("p.t. Alfa Jaya", "13 ELLIOT 111 COURT", "Jakarta", Core.Constants.CountryCodes.Indonesia));

			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("oao Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));
			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("OAO. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));
			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("oOo Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));
			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("OOO. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));
			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("ZAO Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));
			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("ZaO. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));
			AssertEquals("Russia special rule", "RUMORIND111MOS", new MIDGenerator().GenerateMID("J.S.C. Morich Indo Fashion.", "13 ELLIOT 111 COURT", "Moskow", "RU"));

			AssertEquals("Portugal special rule", "PTKAR2527LIS", new MIDGenerator().GenerateMID("COMPANHIA TEXTIL KARSTEN", "Calle Grande, 25-27", "67890 Lisbon", "PT"));
			AssertEquals("Portugal special rule", "PTKAR2527LIS", new MIDGenerator().GenerateMID("Companhia textil Karsten", "Calle Grande, 25-27", "67890 Lisbon", "PT"));
		}
	}
}
