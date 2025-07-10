using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests
{
	[TestFixture]
	class RITADataParserTest
	{
		[Test]
		public void GetFRConditionTypesFromHTML()
		{
			var testPageHtmlContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "FRConditionTypesWebPage.html"));
			var expectedConditionTypes = RITADataParser.GetFRConditionTypesFromHTML(testPageHtmlContent);
			Assert.AreEqual(51, expectedConditionTypes.Count);
			Assert.AreEqual("AAN", expectedConditionTypes[0].ZX2_ConditionType);
		}

		[Test]
		public void GetErrorFromWebServiceResponse()
		{
			var errorResponse = @"<soap:Envelope xmlns:soap=http://schemas.xmlsoap.org/soap/envelope/><soap:Body><ns1:nomenclatureRenvoiXmlResponse xmlns:ns1=http://ejb.service.rita.douane.finances.gouv.fr/><result> 
<L_NOMENC dateInterrogation=""22/02/2022"" dateTraitement=""22/02/2022 00:52:42"" chapitre=""50""> 
<Messages> 
<Message> 
<CodeErreur>RITA910</CodeErreur> 
<TypeMessage>ERT</TypeMessage> 
<Description>LT - Impossible d+apos;obtenir une connexion pour l+apos;accès aux données de la base, contactez l+apos;administrateur.</Description> 
</Message> 
</Messages> 
</L_NOMENC> 
</result></ns1:nomenclatureRenvoiXmlResponse></soap:Body></soap:Envelope>";
			var errorResponseDescription = RITADataParser.GetErrorFromWebServiceResponse(errorResponse);
			Assert.That(errorResponseDescription == "LT - Impossible d'obtenir une connexion pour l'accès aux données de la base, contactez l'administrateur.");

			errorResponse = @"<soap:Envelope xmlns:soap=http://schemas.xmlsoap.org/soap/envelope/><soap:Body> 
<ns1:listeReglementationConditionXmlResponse xmlns:ns1=http://ejb.service.rita.douane.finances.gouv.fr/> 
<result> 
<ltDateTraitement>22/02/2022 01:06:10</ltDateTraitement> 
<ltLegende> Association : 1 = Code Additionnel, 2 = Nomenclature, 3 = Mesure.</ltLegende> 
<ltMessages> 
<ltCodeErreur>RITA910</ltCodeErreur> 
<ltDescription>LT - Impossible d'obtenir une connexion pour l'accès aux données de la base, contactez l'administrateur.</ltDescription> 
<ltTypeMessage>ERT</ltTypeMessage> 
</ltMessages> 
<ltSousTitre>(Données issues du traitement du 21/01/2011 01:16)</ltSousTitre> 
<ltTitre>Codes Renvois valides au 21/01/2011.</ltTitre> 
</result> 
</ns1:listeReglementationConditionXmlResponse> 
</soap:Body> 
</soap:Envelope>";
			errorResponseDescription = RITADataParser.GetErrorFromWebServiceResponse(errorResponse);
			Assert.That(errorResponseDescription == "LT - Impossible d'obtenir une connexion pour l'accès aux données de la base, contactez l'administrateur.");
		}

		[Test]
		public void GetTariffListFromHTML()
		{
			var testPageHtmlContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "UpdateWebPage.htm"));
			var expectedTariffs = RITADataParser.GetFRTariffsFromHTML(testPageHtmlContent);
			Assert.AreEqual(11, expectedTariffs.Count);
			Assert.AreEqual("3824999266", expectedTariffs[0]);
		}

		[Test]
		public void GetDescription()
		{
			var analyser = new RITADataParser();
			var dictionaryEntry = new RITADictionaryEntry("Code", "Description", "Type");
			analyser.RitaDictionary.Add(dictionaryEntry);
			Assert.AreEqual(analyser.GetDescription("Code", "Type"), "Description");
		}

		[Test]
		public void GetMeasuresAndConditions()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var analyser = new RITADataParser();
			var measures = analyser.GetMeasuresAndConditions("7228302010", "I");
			Assert.AreEqual(measures.Count, 94);
			Assert.AreEqual(measures[89].ApplicationTerritory, "MGPRE");
			Assert.AreEqual(measures[89].EndDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), "06-06-2079");
			Assert.AreEqual(measures[89].ExcludedTradeGroups.Count, 0);
			Assert.AreEqual(measures[89].MeasureType, "TVB");
			Assert.AreEqual(measures[89].MeasureClass, MeasureHelper.MeasureClass.Vat);
			Assert.AreEqual(measures[89].Nomenclature, "7200000000");
			Assert.AreEqual(measures[89].Preferences.Count, 0);
			Assert.AreEqual(measures[89].Direction, "I");
			Assert.AreEqual(measures[89].QuotaNumber, "");
			Assert.AreEqual(measures[89].Regulation, "N1200002");
			Assert.AreEqual(measures[89].Renvois.Count, 1);
			Assert.AreEqual(measures[89].Sid, "-244214");
			Assert.AreEqual(measures[89].StartDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), "01-01-2012");
			Assert.AreEqual(measures[89].SupplementaryCode, "V900");
			Assert.IsTrue(measures[89].SupplementaryCodeDescription.Contains("seringue"));
			Assert.AreEqual(measures[89].TaxCode, "A505");
			Assert.AreEqual(measures[89].TaxCodeDescription, "Taux de TVA réduit");
			Assert.AreEqual(measures[89].TradeGroup, "FR01");
			Assert.AreEqual(measures[89].Conditions.Count, 2);
			Assert.AreEqual(measures[89].Conditions[0].Description, "Présentation d'un certificat/licence/document");
			Assert.AreEqual(measures[89].Conditions[0].TaxCode, "");
			Assert.AreEqual(measures[89].Conditions[0].SequenceNumber, 1);
			Assert.AreEqual(measures[89].Conditions[0].DocumentCode, "6018");
			Assert.AreEqual(measures[89].Conditions[0].DocumentType, "SUP");
			Assert.AreEqual(measures[89].Conditions[0].MeasurementCode, "");
			Assert.AreEqual(measures[89].Conditions[0].Amount, null);
			Assert.AreEqual(measures[89].Conditions[0].IsRateFormula, false);
			Assert.AreEqual(measures[89].Conditions[0].Components.Count, 0);

			var otherMeasureWithUnparseableDates = measures.Where(m => m.Sid == "-267020").FirstOrDefault();
			Assert.AreEqual(otherMeasureWithUnparseableDates.StartDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), "01-01-1900");
			Assert.AreEqual(otherMeasureWithUnparseableDates.EndDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), "06-06-2079");

			Assert.IsTrue(measures.Any(m => m.Conditions.Any(c => c.IsRateFormula == true)));
		}

		[Test]
		public void GetConditionsTaxCode()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var analyser = new RITADataParser();
			var measures = analyser.GetMeasuresAndConditions("2208409900", "I");
			Assert.AreEqual(measures[59].Conditions[1].TaxCode, "L437");
		}
	}
}
