using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public sealed class MeasureMappingProvider : IMeasureMappingProvider
	{
		IEnumerable<string> IMeasureMappingProvider.ConvertPreferences(IEnumerable<string> defaultPreferences, string geographicalArea, IEnumerable<string> footnotes) => defaultPreferences.ToList();

		string IMeasureMappingProvider.ConvertRateCode(string defaultRateCode, Measure measure) => defaultRateCode;

		Dictionary<string, MeasureTypeMapping> IMeasureMappingProvider.GetMeasureTypeMappings() => new Dictionary<string, MeasureTypeMapping>
		{
			{ "BAO", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "100" } }, // Accijns
			{ "BAF", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "199" } }, // Specifieke accijns 
			{ "BAS", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "200" } }, // Bijzondere accijns
			{ "BAP", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Excises, RateCode = "299" } }, // Specifieke bijzondere accijns
			{ "BCE", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Retribution, RateCode = "300" } }, // Bijdrage op de energie
			{ "BCR", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Retribution, RateCode = "400" } }, // Controle retributie België
			{ "BRC", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Retribution, RateCode = "400" } }, // Controleretributie
			{ "BVH", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Levies, RateCode = "500" } }, // Verpakkingsheffing 
			{ "BIR", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Interest, RateCode = "801" } }, // Nalatigheidsintrest op de gemeenschappelijke accijns
			{ "BNI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Interest, RateCode = "802" } }, // nalatigheidsintrest : niet-gemeenschappelijke accijns, milieutaksen, verpakkings- en milieuheffing
			{ "MSI", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Rate, RateType = RateTypes.Duty, RateCode = "A00" } }, // invoerheffing
		};

		/*
		MeasureTypeSeries
		0: Landbouwtoezicht (Agriculture supervision)
		1: Borgstellingen (Guarantee)
		2: Landbouwregelingen (Agriculture measures)
		3: Vergunningen / aanvullende rechten (Licences / additional duties)
		4: BTW  (VAT)
		5: Accijnzen  (Excise)
		6: free (free)
		7: Free (Free)
		8: Andere nationale maatregelen (Other national measures)
		9: Free (Free)
		A: Invoer en/of uitvoer niet toegestaan (Importation and/or exportation prohibited)
		B: In vrije verkeer brengen of uitvoer aan voorwaarden onderworpen (Entry into free circulation or exportation subject to conditions)
		C: Toepasbaar recht (Applicable duty)
		D: Antidumpingrecht of compenserend recht (Anti-dumping or countervailing duties)
		E: Heffingen, uitvoerrestituties en andere landbouwbedragen (Levies, export refunds and other agricultural amounts)
		F: Bedrag van aanvullend invoerrecht voor suiker, meel (Additional duty on sugar, flour)
		G: Monetair compenserend bedrag (Monetary compensatory amount)
		H: Compenserend bedrag toetreding (Accession compensatory amount)
		J: Compenserende heffing (Countervailing charge)
		K: Referentieprijs (Reference price)
		L: Aanvullende regeling voor het handelsverkeer (Complementary trade mechanism)
		M: Eenheidsprijs, forfaitaire invoerwaarden, representatieve prijzen (pluimvee, suiker) (Unit price, standard import value, representative price (poultry, sugar))
		N: Toezicht a posteriori (Posterior surveillance)
		O: Bijzondere maatstaf (Supplementary unit)
		P: BTW (VAT)
		Q: Accijnzen (Excises)
		R: Voorlopig uitgesloten (Provisional exclusion)
		S: Aanvullende bedrag (Supplementary amount)
		Z: Gearchiveerd maatregeltype (Archived measure type)
		*/

		/*
		MeasureType
		BCP	//1: borgstelling met prijsvork (Guarantee with price range)
		BOR	//1: Borgstelling bij invoer (Guarantee for imports)
		HTB	//1: Harde tarwe borg (Durum wheat: caution)
		MAB	//1: Glazige maïs - borg (Flint maize (caution))
		TCT	//1: Zekerheid tariefcontingent (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		ZTB	//1: Zachte tarwe - borg (Common wheat (caution))
		FIA	//2: Certificaat achteraf (Certificate a posteriori)
		FIO	//2: voorfixatiecertificaat verplicht (Advance-fixing certificate mandatory)
		FIV	//2: Voorfixatiecertificaat  verplicht (Advance-fixing certificate mandatory)
		INC	//2: Invoercertificaat AGRIM (Import certificate AGRIM)
		INP	//2: Invoercertificaat AGRIM-preferentiële regeling (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		LZP	//2: Landbouwregelingen zonder preferentiele recht (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		NBI	//2: Restitutiecertificaat Niet Bijlage I (Refund certificate non-Annex I)
		TRV	//2: Terugvordering restituties (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		UFC	//2: Uitvoer- en voorfixatiecertificaat verplicht (Export licence or advance-fixing certificate mandatory)
		UNC	//2: Uitvoercertificaat AGREX (Export certificate AGREX)
		UNI	//2: Uitvoercertificaat isoglucose buiten quotum (Export licence out-of-quota)
		DAD	//3: Aanvullende rechten (overgangsmaatregelen) (Additional duties (transitional measures))
		IBD	//3: Bijzondere bestemmingen vergunningen - andere bepalingen (End use licenses – other provisions)
		ILC	//3: Nationale vergunningen bij invoer D.I. 591.00 (Import: National licence C.D.591.00)
		IST	//3: Vergunningen (strategische goederen) (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		NOM	//3: Nomenclatuurbepalingen, inleidende bepalingen (schorsing producten bestemd voor bepaalde soorten schepen en voor boor-en werkeilanden) (Nomenclature provisions (goods for certain categories of ships, boats and other vessels and for drilling or production platforms))
		ULI	//3: Nationale vergunningen bij uitvoer D.I. 591.00 (Export: National authorisation)
		BBH	//4: BTW H7 (VAT H7)
		BBT	//4: BTW (VAT rate)
		RBT	//4: Verminderd BTW-tarief (Reduced VAT rate)
		VAT	//4: BTW (VAT rate)
		BAF	//5: Specifieke accijns (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BAO	//5: Accijns (Excise duty )
		BAP	//5: Specifieke bijzondere accijns (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BAS	//5: Bijzondere accijns  (Special excise duty )
		BCE	//5: Bijdrage op de energie  (Energy contribution)
		BCR	//5: Controle retributie België (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BIR	//5: Nalatigheidsintrest op de gemeenschappelijke accijns (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BMH	//5: Milieuheffing - Belgïe (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BMT	//5: Milieutaks - België (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BNI	//5: nalatigheidsintrest : niet-gemeenschappelijke accijns, milieutaksen, verpakkings- en milieuheffing (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		BRC	//5: Controleretributie  (Monitoring change)
		BUM	//5: Accijnzen - maatstaf van heffing gekoppeld aan een vrijstelling (Excise: assessment base linked to an exemption)
		BVA	//5: BTW-tarief ACC4 (VAT rate – ACC4 )
		BVH	//5: Verpakkingsheffing  (Packaging levy)
		MIL	//5: Milieutaksen (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		TST	//5: Test (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		CTG	//8: Contingent bevroren rundvlees (Quota frozen beef)
		EXA	//8: Maximumhoeveelheden accijnsproducten H7 (Maximum quantities of excise products)
		INF	//8: informatie bij invoer (The content of this item is available in Dutch and French (via the links NL and FR in the header).)
		MAD	//8: Antidumping- en/of compenserende rechten van toepassing in de exclusieve economische zone of het continentaal plat   (Anti - dumping and/or countervailing duties are applicable in the exclusive economic zone or on the continental shelf)
		MDB	//8: Indelingsmaatregel /Controlemaatregel bij invoer  (Control measure)
		MDC	//8: Indelingsmaatregel /Controlemaatregel bij invoer  (Control measure)
		MDH	//8: Indelingsmaatregel / Controlemaatregel bij invoer en uitvoer (Control measure)
		MSI	//8: Schorsing douanerechten op bepaalde militaire uitrusting (Suspension of import duties on certain military equipment)
		MTI	//8: Controle- en beperkingsmaatregelen bij invoer (Import: Control- and restrictive measures)
		NFA	//8: Controle gedifferentieerd geprijsde geneesmiddelen (Import tiered priced medicines)
		NFB	//8: Invoercontrole minicups (Import mini-cups)
		NFC	//8: Invoercontrole plantaardige producten (Import control plant products)
		NFD	//8: Invoercontrole springstoffen (Import explosives)
		NFG	//8: Fytosanitaire controles (Import: plant-health checks)
		NFH	//8: Ordening van de wijnmarkt bij uitvoer (Export: common organization of the market in wine)
		NFJ	//8: Uitvoercontrole plantaardige producten (Export control plant products)
		NFP	//8: Invoercontrole productveiligheid (Import control product safety)
		NFR	//8: Invoercontrole radioactiviteit bepaalde landbouwproducten (Control of radioactivity agricultural products)
		277	//A: Invoerverbod (Import prohibition)
		278	//A: Uitvoerverbod (Export prohibition)
		481	//A: Aangifte onder deze postonderverdeling is onderworpen aan bepaalde beperkingen (invoer) (Declaration of subheading submitted to restrictions (import))
		485	//A: Aangifte onder deze postonderverdeling is onderworpen aan bepaalde beperkingen (uitvoer) (Declaration of subheading submitted to restrictions (export))
		072	//B: Invoercertificaat (Import licence)
		089	//B: Conventie van Washington (The Washington Convention)
		092	//B: De Washington conventie (export) (The Washington Convention (export))
		410	//B: Veterinaire controle (Veterinary control)
		420	//B: Beperking van het vrije verkeer (Entry into free circulation (prior surveillance))
		464	//B: Aangifte onder deze postonderverdeling is onderworpen aan bepalingen betr. ";bijzondere bestemmingen"; (Declaration of subheading submitted to end-use provisions)
		465	//B: Beperking van het vrije verkeer (Restriction on entry into free circulation)
		467	//B: Uitvoerbeperking (Restriction on export)
		473	//B: Uitvoervergunning (Export authorization)
		474	//B: In het vrije verkeer brengen (kwantitatieve maxima) (Entry into free circulation (quantitative limitation))
		475	//B: Beperking van het vrije verkeer (Restriction on entry into free circulation)
		476	//B: Uitvoerbeperking (Restriction on export)
		477	//B: In vrij verkeer brengen (passief veredelingsverkeer) (Entry into free circulation (outward processing traffic))
		478	//B: Uitvoervergunning (Dual use) (Export authorization (Dual use))
		479	//B: Uitvoerbewaking van gevaarlijke chemische stoffen (Export control on dangerous chemicals)
		482	//B: Aangifte van onderverdeling waarvoor wettelijke beperkingen gelden (nettogewicht/bijzondere maatstaf) (Declaration of subheading submitted to legal restrictions (net weight/supplementary unit))
		483	//B: Aangifte van onderverdeling onderworpen aan wettelijke beperkingen (prijs per eenheid) (Declaration of subheading submitted to legal restrictions (unit price))
		484	//B: Aangifte van onderverdeling waarvoor fysieke beperkingen gelden (nettogewicht/bijzondere maatstaf) (Declaration of subheading submitted to physical restrictions (net weight/supplementary unit))
		491	//B: Aangifte van onderverdeling waarvoor fysieke beperkingen gelden (opgegeven statistische waarde) (Declaration of subheading submitted to physical restrictions (declared statistical value))
		492	//B: Aangifte van onderverdeling waarvoor fysieke beperkingen gelden (opgegeven nettomassa) (Declaration of subheading submitted to physical restrictions (declared net mass))
		493	//B: Aangifte van onderverdeling onderworpen aan fysieke beperkingen (aangegeven bijzondere eenheid) (Declaration of subheading submitted to physical restrictions (declared supplementary unit))
		494	//B: Aangifte van onderverdeling onderworpen aan wettelijke beperkingen (aangegeven statistische waarde) (Declaration of subheading submitted to legal restrictions (declared statistical value))
		495	//B: Aangifte van onderverdeling onderworpen aan wettelijke beperkingen (opgegeven nettomassa) (Declaration of subheading submitted to legal restrictions (declared net mass))
		496	//B: Aangifte van onderverdeling onderworpen aan wettelijke beperkingen (aangegeven bijzondere maatstaf) (Declaration of subheading submitted to legal restrictions (declared supplementary unit))
		705	//B: Goederen voor foltering en repressie, invoerverbod (Import prohibition on goods for torture and repression)
		706	//B: Goederen voor foltering en repressie, uitvoerverbod (Goods for torture and repression, export prohibition)
		707	//B: Invoercontrole (Import control)
		708	//B: Goederen voor foltering en repressie, uitvoerbeperking (Goods for torture and repression, export restriction)
		709	//B: Uitvoercontrole (Export control)
		710	//B: Invoercontrole – CITES (Import control - CITES)
		711	//B: Controle bij de invoer voor aan beperkingen onderhevige goederen en technologie (Import control on restricted goods and technologies)
		712	//B: Invoercontrole – IAS (Import control - IAS)
		713	//B: Invoercontrole op genetisch gemanipuleerde organismen (GOM) en producten die GOM’s bevatten (Import control on genetically modified organisms (GMO) and products containing GMOs)
		714	//B: Invoercontrole (Import control)
		715	//B: Uitvoercontrole – CITES (Export control - CITES)
		716	//B: Controlemaatregel bij uitvoer van vis (Export control - Fish)
		717	//B: Controle bij de uitvoer voor aan beperkingen onderhevige goederen en technologie (Export control on restricted goods and technologies)
		718	//B: Controle bij uitvoer van luxegoederen (Export control on luxury goods)
		719	//B: Controle op illegale, ongemelde en ongereglementeerde visserij (Control on illegal, unreported and unregulated fishing)
		722	//B: In het vrije verkeer brengen (beperkingen - levensmiddelen en diervoeders) (Entry into free circulation (restriction - feed and food))
		724	//B: Controle op de invoer van gefluoreerde broeikasgassen (Import control of fluorinated greenhouse gases)
		725	//B: Uitvoercontrole betreffende ozonlaag afbrekende stoffen (Export control on ozone-depleting substances)
		726	//B: Invoercontrole van ozonafbrekende stoffen (Import control on ozone-depleting substances)
		728	//B: Controle bij invoer van luxegoederen (Import control on luxury goods)
		730	//B: Naleving van de eisen betreffende aan uitvoer voorafgaande controles (Compliance with the pre-export checks requirements)
		735	//B: Controle op de uitvoer van cultuurgoederen (Export control on cultural goods)
		740	//B: Uitvoercontrole van katten- en hondenbont (Export control on cat and dog fur)
		745	//B: Invoercontrole van katten- en hondenbont (Import control on cat and dog fur)
		746	//B: Controle bij invoer van zeehondenproducten&lt;br (Import control on seal products)
		747	//B: Invoercontrole van hout en houtproducten behoudens de vergunningenregeling van FLEGT&lt;br (Import control of timber and timber products subject to the FLEGT licensing scheme)
		748	//B: Controle bij invoer van kwik (Import control of mercury)
		749	//B: Controle op de uitvoer van kwik (Export control of mercury)
		750	//B: Invoercontrole van biologische producten (Import control of organic products)
		751	//B: Uitvoercontrole afval (Export control - Waste)
		755	//B: Invoercontrole – afval (Import control - Waste)
		760	//B: Invoercontrole&lt;br (Import control)
		761	//B: Invoercontrole van REACH (Import control on REACH)
		762	//B: Invoercontrole (Import control)
		763	//B: Invoercontrole (Import control)
		765	//B: Uitvoercontrole van gefluoreerde broeikasgassen (Export control of fluorinated greenhouse gases)
		766	//B: Uitvoercontrole (Export control)
		767	//B: Uitvoercontrole (Export control)
		768	//B: Uitvoercontrole (Export control)
		769	//B: Invoercontrole van persistente organische verontreinigende stoffen (POP) (Import control on persistent organic pollutants (POP))
		770	//B: Invoercontrole van hout en houtproducten behoudens de vergunningenregeling van FLEGT-Ghana&lt;br (Import control of timber and timber products subject to the FLEGT licensing scheme-Ghana)
		771	//B: Invoercontrole van hout en houtproducten behoudens de vergunningenregeling van FLEGT-Kameroen&lt;br (Import control of timber and timber products subject to the FLEGT licensing scheme-Cameroon)
		772	//B: Invoercontrole van hout en houtproducten behoudens de vergunningenregeling van FLEGT-Republiek Congo&lt;br (Import control of timber and timber products subject to the FLEGT licensing scheme-Republic of Congo)
		773	//B: Invoercontrole van hout en houtproducten behoudens de vergunningenregeling van FLEGT-Centraal Afrikaanse Republiek&lt;br (Import control of timber and timber products subject to the FLEGT licensing scheme-Central Africa Republic)
		774	//B: Invoercontrole van hout en houtproducten behoudens de vergunningenregeling van FLEGT-Vietnam&lt;br (Import control of timber and timber products subject to the FLEGT licensing scheme-Vietnam)
		775	//B: Mechanisme voor Koolstofgrenscorrectie (Carbon Border Adjustment Mechanism)
		MTE	//B: Indelingsmaatregel /Controlemaatregel bij uitvoer (Control measure)
		103	//C: Douanerecht derde landen (Third country duty)
		105	//C: Niet-preferentieel recht dat onderworpen is aan een bijzondere bestemming (Non preferential duty under end-use)
		106	//C: Door de Douane-Unie vastgesteld recht (Customs Union Duty)
		112	//C: Autonome tariefschorsing (Autonomous tariff suspension)
		115	//C: Autonome schorsing van het recht dat onderworpen is aan een bijzondere bestemming (Autonomous suspension under end-use)
		117	//C: Schorsing - producten bestemd voor bepaalde soorten schepen en voor boor- en werkeilanden (Suspension - goods for certain categories of ships, boats and other vessels and for drilling or production platforms)
		119	//C: Luchtwaardigheids-tariefschorsing (Airworthiness tariff suspension)
		122	//C: Niet-preferentieel tariefcontingent (Non preferential tariff quota)
		123	//C: Niet-preferentieel tariefcontingent dat onderworpen is aan een bijzondere bestemming (Non preferential tariff quota under end-use)
		140	//C: Tariefpreferentie bij passieve veredeling (Outward processing tariff preference)
		141	//C: Preferentiële schorsing (Preferential suspension)
		142	//C: Tariefpreferenties (Tariff preference)
		143	//C: Preferentieel tariefcontingent (Preferential tariff quota)
		144	//C: Preferentieel plafond (Preferential ceiling)
		145	//C: Tariefpreferentie die onderworpen is aan een bijzondere bestemming (Preference under end-use)
		146	//C: Preferentieel tariefcontingent dat onderworpen is aan een bijzondere bestemming (Preferential tariff quota under end-use)
		147	//C: Contingent in het kader van Douane-Unie (Customs Union Quota)
		551	//D: Voorlopige antidumpingrechten (Provisional anti-dumping duty)
		552	//D: Definitieve antidumpingrechten (Definitive anti-dumping duty)
		553	//D: Voorlopige compenserende rechten (Provisional countervailing duty)
		554	//D: Definitieve compenserende rechten (Definitive countervailing duty)
		555	//D: Antidumping/compenserend recht - inning in afwachting (Anti-dumping/countervailing duty - Pending collection)
		561	//D: Bericht van inleiding van een onderzoek in verband met antidumping-/compenserende maatregelen (Notice of initiation of an anti-dumping or countervailing proceeding)
		562	//D: Geschorste antidumping-/compenserende maatregelen (Suspended anti-dumping or countervailing duty)
		563	//D: Dump-Verbintenis (Dump Undertakings)
		564	//D: Registratie voor antidumping-/compenserende maatregelen (Anti-dumping or countervailing registration)
		565	//D: Herziening van antidumping-/compenserende maatregelen (Anti-dumping/countervailing review)
		566	//D: Antidumping-/compenserende maatregelen statistieken (Anti-dumping/countervailing statistic)
		570	//D: Antidumping/Compenserende rechten - controle (Anti-dumping/countervailing duty - Control)
		670	//E: Bedrag heffing (Amount of levy)
		674	//E: Agrarisch element (Agricultural component)
		680	//E: Uitvoerrestituties (basisproducten) (Export refund (basic products))
		681	//E: Uitvoerrestituties (ingrediëten - informatie) (Export refund (ingredients - information))
		682	//E: Uitvoerbelasting (Export tax)
		683	//E: Uitvoerrestituties (ingrediënten - bedragen) (Export refund (ingredients - amounts))
		684	//E: Uitvoerrestituties voor het gehalte aan graan (Export refunds for cereal contents)
		685	//E: Uitvoerrestituties voor het gehalte aan rijst (Export refunds for rice contents)
		686	//E: Uitvoerrestituties voor het gehalte aan eieren (Export refund for eggs contents)
		687	//E: Uitvoerrestituties voor het gehalte aan suiker (Export refunds for sugar contents)
		688	//E: Uitvoerrestituties voor het gehalte aan melkproducten (Export refunds for milk products contents)
		672	//F: Bedrag van aanvullend invoerrecht voor suiker (Amount of additional duty on sugar)
		673	//F: Bedrag van aanvullend invoerrecht voor meel (Amount of additional duty on flour)
		086	//G: MCM + ACA (MCM + ACA)
		676	//G: Monetair compenserend bedrag invoer (Monetary compen. amount - import)
		678	//G: Monetair compenserend bedrag uitvoer (Monetary compel. amount for export)
		677	//H: Compenserend bedrag toetreding invoer (ACA) (Accesion compensatory amount (ACA) import)
		679	//H: Compenserend bedrag toetreding uitvoer (ACA) (Accession comp. amount (ACA) export)
		690	//J: Compenserende heffing (Countervailing charge)
		695	//J: Aanvullende rechten (Additional duties)
		696	//J: Aanvullende rechten (vrijwaringsmaatregelen) (Additional duties (safeguard))
		624	//K: Referentieprijs (Reference price)
		625	//K: Statistiek - referentieprijs vis (Statistic - reference price fish)
		630	//K: Prijsverschillen voor basisprodukten (Differences in prices for basic products)
		692	//L: Aanvullende regeling voor het handelsverkeer (Supplementary trade mechanism)
		487	//M: Representatieve prijzen (pluimvee) (Representative price (poultry))
		488	//M: Eenheidsprijs (Unit price)
		489	//M: Representatieve prijzen (Representative price)
		490	//M: Forfaitaire invoerwaarden (Standard import value)
		430	//N: Controle van de gegevens van de aangifte (verdachte eenheidsprijs) (Control of particulars of the declaration (suspicious unit price))
		431	//N: Controle van de aangiftegegevens (verdacht mbt nettogewicht/aanvullende eenheid) (Control of particulars of the declaration (suspicious net weight/supplementary unit))
		440	//N: Openbare invoerbewaking (Public Import Monitoring)
		442	//N: Vertrouwelijke invoerbewaking (Confidential Import Monitoring)
		445	//N: Openbare uitvoerbewaking (Public Export Monitoring)
		447	//N: Vertrouwelijke uitvoerbewaking (Confidential Export Monitoring)
		450	//N: Statistisch toezicht - Alle invoer - Behalve Verordening 1555/96 (Statistical surveillance - all imports, except Regulation 1555/96)
		455	//N: Vertrouwelijke bewaking (Tariefindeling) (Confidential Surveillance (Tariff classification))
		456	//N: vertrouwelijke bewaking (andere) (Confidential Surveillance (Other))
		457	//N: Vertrouwelijke bewaking (tariefcontingent met certificaten) (Confidential Surveillance (Licence TQs))
		460	//N: Geautomatiseerd toezicht (Computer surveillance)
		461	//N: Communautair toezicht - Referentiehoeveelheden (Community surveillance - reference quantities)
		462	//N: Toezicht a posteriori op de invoer (Posterior import surveillance)
		463	//N: Toezicht a posteriori op de uitvoer (Posterior export surveillance)
		466	//N: Toezicht TPP post. (Outward processing post. surveillance)
		468	//N: Vertrouwelijk toezicht, SAP (GSP Confidential Surveillance)
		469	//N: andere vertrouwelijk toezicht dan voor SAP (Confidential surveillance other than GSP)
		470	//N: Toezicht op de uitvoer (Export surveillance)
		471	//N: Toezicht op de uitvoer (TQS) (Export surveillance (TQS))
		472	//N: Toezicht bij uitvoer (Export surveillance)
		109	//O: Aanvullende eenheid (Supplementary unit)
		110	//O: Bijzondere maatstaf invoer (Supplementary unit import)
		111	//O: Aanvullende eenheden uitvoer (Supplementary unit export)
		305	//P: Belasting op de toegevoegde waarde (Value added tax)
		306	//Q: Accijnzen (Excises)
		166	//R: Van de preferentie uitgesloten (Provisional exclusion)
		651	//S: Zekerheid op basis van de representatieve prijst (Security based on representative price)
		652	//S: Aanvullend recht op basis van de CIF-invoerprijs (Additional duty based on CIF price)
		653	//S: Zekerheid op basis van representatieve prijs, verminderd onder toepassing van een tariefcontingent (Security based on representative price, reduced under the benefit of a tariff quota)
		654	//S: Aanvullend recht op basis van cif-invoerprijs, verminderd onder toepassing van een tariefcontingent (Additional duty based on CIF price, reduced under the benefit of a tariff quota)
		655	//S: Zekerheid (pluimvee) op basis van de representatieve prijs (Security (poultry) based on representative price)
		656	//S: Aanvullend recht (pluimvee) op basis van de CIF-invoerprijs (Additional duty (poultry) based on CIF price)
		657	//S: Verminderde zekerheid op basis van de representatieve prijs (Reduced security based on representative price)
		658	//S: Verminderd aanvullend recht op basis van de cif-invoerprijs (Reduced additional duty based on CIF price)
		691	//S: Extra heffing (Suplementary amount)
		046	//Z: Tariefcontigent/plafond (Tariff quota/ceiling)
		081	//Z: MOB (MOB)
		082	//Z: MOB+MCM (MOB+MCM)
		083	//Z: MOB+MCM+ACA (MOB+MCM+ACA)
		084	//Z: MOB+ACA (MOB+ACA)
		*/

		string IMeasureMappingProvider.GetVatCode(Measure measure)
		{
			var result = string.Empty;
			var dutyAmount = measure.Components?.FirstOrDefault()?.DutyAmount;

			if (measure.ConditionClass == MeasureHelper.ConditionClass.Vat && dutyAmount.HasValue)
			{
				result = GetVatCodeFromDutyAmount(dutyAmount.Value);
			}

			return result;
		}

		static string GetVatCodeFromDutyAmount(decimal dutyAmount)
		{
			switch (dutyAmount)
			{
				case VatCodesAndValues.StandardValue:
					return VatCodesAndValues.StandardCode;
				case VatCodesAndValues.ReducedValue:
					return VatCodesAndValues.ReducedCode;
				case VatCodesAndValues.ReducedParkingValue:
					return VatCodesAndValues.ReducedParkingCode;
				case VatCodesAndValues.ZeroRatedValue:
					return VatCodesAndValues.ZeroRatedCode;
				default:
					return string.Empty;
			}
		}

		public static class VatCodesAndValues
		{
			public const decimal StandardValue = 21;
			public const string StandardCode = "BSR";

			public const decimal ReducedValue = 6;
			public const string ReducedCode = "BRR";

			public const decimal ReducedParkingValue = 12;
			public const string ReducedParkingCode = "BRP";

			public const decimal ZeroRatedValue = 0;
			public const string ZeroRatedCode = "BEZ";
		}
	}
}
