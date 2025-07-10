using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(TurkeyComplianceInfo))]
	public class TurkeyComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Turkey;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "EAR", "EIN", "EIC", "ICN", "XCL", "PAR", "PIN", "PIC", "PCL", "DAR", "DIN", "DCN", "DCL", "CAR", "CIN", "CCN", "CCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "EAR", "EIN", "EIC", "ICN", "XCL", "CAR", "CIN", "CCN", "CCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "PAR", "PIN", "PIC", "PCL", "DAR", "DIN", "DCN", "DCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "EAR", "EIN", "EIC", "XCL", "CCN", "CCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "PAR", "PIN", "PIC", "PCL", "DCN", "DCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "ICN", "XCL", "CAR", "CIN", "CCL" };

		protected override string[] ExpectedReceivablesAmendingCreditNoteComplianceSubTypes => new string[] { "XCL", "CAR", "CIN", "CCL" };

		protected override string[] ExpectedReceivablesReversalCreditNoteComplianceSubTypes => new string[] { "ICN", "XCL", "CCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "PCL", "DAR", "DIN", "DCL" };

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "EAR", "EIN", "EIC", "DAR", "DIN" };

		protected override string[] ExpectedRejectReQueueForReversedTransactionExclusionSubTypes => new string[] { "DAR", "DIN", "CCN" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "XCL";

		protected override string ExpectedComplianceSubTypeDescription => "Receivables Reimbursement / Disbursement / Excluded Supply";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Satış Faturası Geri Ödeme Belgesi";

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
			{ "EAR", LedgerOfUse.AR }, { "EIN", LedgerOfUse.AR }, { "EIC", LedgerOfUse.AR }, { "ICN", LedgerOfUse.AR }, { "XCL", LedgerOfUse.AR },
			{ "PAR", LedgerOfUse.AP }, { "PIN", LedgerOfUse.AP }, { "PIC", LedgerOfUse.AP }, { "PCL", LedgerOfUse.AP },
			{ "DAR", LedgerOfUse.AP }, { "DIN", LedgerOfUse.AP }, { "DCN", LedgerOfUse.AP }, { "DCL", LedgerOfUse.AP },
			{ "CAR", LedgerOfUse.AR }, { "CIN", LedgerOfUse.AR }, { "CCN", LedgerOfUse.AR }, { "CCL", LedgerOfUse.AR },
		};

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "EAR", TransactionTypeOfUse.INV }, { "EIN", TransactionTypeOfUse.INV }, { "EIC", TransactionTypeOfUse.INV }, { "ICN", TransactionTypeOfUse.CRD }, { "XCL", TransactionTypeOfUse.ALL },
			{ "PAR", TransactionTypeOfUse.INV }, { "PIN", TransactionTypeOfUse.INV }, { "PIC", TransactionTypeOfUse.INV }, { "PCL", TransactionTypeOfUse.ALL },
			{ "DAR", TransactionTypeOfUse.CRD }, { "DIN", TransactionTypeOfUse.CRD }, { "DCN", TransactionTypeOfUse.INV }, { "DCL", TransactionTypeOfUse.ALL },
			{ "CAR", TransactionTypeOfUse.CRD }, { "CIN", TransactionTypeOfUse.CRD }, { "CCN", TransactionTypeOfUse.INV }, { "CCL", TransactionTypeOfUse.ALL },
		};

		protected override string ExpectedComplianceRules =>
@"TR,ICN,AR,CRD,TID,ALL,RTO,,EAR,ALL,1,e-Fatura,,,,,,
TR,ICN,AR,CRD,TID,ALL,RTO,,EIN,ALL,1,e-Fatura,,,,,,
TR,EIN,AR,INV,TID,ALL,OTO,,,ERO,1,e-Fatura,,,,,,
TR,EIN,AR,INV,TID,ALL,ARO,,EIN,ERO,1,e-Fatura,,,,,,
TR,EAR,AR,INV,TID,ALL,OTO,,,NER,1,e-Fatura,,,,,,
TR,EAR,AR,INV,TID,ALL,ARO,,EAR,NER,1,e-Fatura,,,,,,
TR,CIN,AR,CRD,TID,ALL,ATO,,,ERG,1,e-Fatura,,,,,,
TR,CAR,AR,CRD,TID,ALL,ATO,,,NER,1,e-Fatura,,,,,,
TR,XCL,AR,INV,EXL,ALL,OTO,,,ALL,1,e-Fatura,,,,,,
TR,XCL,AR,CRD,EXL,ALL,OTO,,,ALL,1,e-Fatura,,,,,,
TR,XCL,AR,INV,EXL,ALL,ARO,,XCL,ALL,1,e-Fatura,,,,,,
TR,XCL,AR,CRD,EXL,ALL,ARO,,XCL,ALL,1,e-Fatura,,,,,,
TR,DCL,AP,INV,EXL,ALL,ARO,,DCL,ALL,1,e-Fatura,,,,,,
TR,DCL,AP,CRD,EXL,ALL,ARO,,DCL,ALL,1,e-Fatura,,,,,,
TR,DAR,AP,CRD,TID,ALL,ALL,,,NER,1,e-Fatura,,,,,,
TR,DIN,AP,CRD,TID,ALL,ALL,,,ERO,1,e-Fatura,,,,,,
TR,DIN,AP,CRD,TID,ALL,ALL,,,ERC,1,e-Fatura,,,,,,
TR,PIN,AP,INV,TID,ALL,OTO,,,ERO,1,e-Fatura,,,,,,
TR,PIC,AP,INV,TID,ALL,OTO,,,ERC,1,e-Fatura,,,,,,
TR,PAR,AP,INV,TID,ALL,OTO,,,NER,1,e-Fatura,,,,,,
TR,PCL,AP,INV,EXL,ALL,OTO,,,ALL,1,e-Fatura,,,,,,
TR,PCL,AP,CRD,EXL,ALL,OTO,,,ALL,1,e-Fatura,,,,,,
TR,PCL,AP,CRD,EXL,ALL,ARO,,PCL,ALL,1,e-Fatura,,,,,,
TR,CCN,AR,INV,TID,ALL,ARO,,CAR,ALL,1,e-Fatura,,,,,,
TR,CCN,AR,INV,TID,ALL,ARO,,CIN,ALL,1,e-Fatura,,,,,,
TR,CCL,AR,INV,EXL,ALL,ARO,,CCL,ALL,1,e-Fatura,,,,,,
TR,CCL,AR,CRD,EXL,ALL,ARO,,CCL,ALL,1,e-Fatura,,,,,,
TR,ICN,AR,CRD,TID,ALL,RTO,,EAR,ALL,2,e-Archive Only,,,,,,
TR,CAR,AR,CRD,TID,ALL,ATO,,EAR,NER,2,e-Archive Only,,,,,,
TR,EAR,AR,INV,TID,ALL,OTO,,,NER,2,e-Archive Only,,,,,,
TR,EAR,AR,INV,TID,ALL,ARO,,EAR,NER,2,e-Archive Only,,,,,,
TR,XCL,AR,INV,EXL,ALL,OTO,,,ALL,2,e-Archive Only,,,,,,
TR,XCL,AR,CRD,EXL,ALL,OTO,,,ALL,2,e-Archive Only,,,,,,
TR,XCL,AR,INV,EXL,ALL,ARO,,XCL,ALL,2,e-Archive Only,,,,,,
TR,XCL,AR,CRD,EXL,ALL,ARO,,XCL,ALL,2,e-Archive Only,,,,,,
TR,DCL,AP,INV,EXL,ALL,ARO,,DCL,ALL,2,e-Archive Only,,,,,,
TR,DCL,AP,CRD,EXL,ALL,ARO,,DCL,ALL,2,e-Archive Only,,,,,,
TR,DAR,AP,CRD,TID,ALL,ALL,,,NER,2,e-Archive Only,,,,,,
TR,PAR,AP,INV,TID,ALL,OTO,,,NER,2,e-Archive Only,,,,,,
TR,PCL,AP,INV,EXL,ALL,OTO,,,ALL,2,e-Archive Only,,,,,,
TR,PCL,AP,CRD,EXL,ALL,OTO,,,ALL,2,e-Archive Only,,,,,,
TR,PCL,AP,CRD,EXL,ALL,ARO,,PCL,ALL,2,e-Archive Only,,,,,,
TR,CCN,AR,INV,TID,ALL,ARO,,CAR,ALL,2,e-Archive Only,,,,,,
TR,CCL,AR,INV,EXL,ALL,ARO,,CCL,ALL,2,e-Archive Only,,,,,,
TR,CCL,AR,CRD,EXL,ALL,ARO,,CCL,ALL,2,e-Archive Only,,,,,,";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "201", Description = (NoResString)"17/1 Kültür ve Eğitim Amacı Taşıyan İşlemler", Bool = true, RelatedItemCode = "201" },
				new CodeDescriptionBoolRelatedItem() { Code = "202", Description = (NoResString)"17/2-a Sağlık, Çevre Ve Sosyal Yardım Amaçlı İşlemler", Bool = true, RelatedItemCode = "202" },
				new CodeDescriptionBoolRelatedItem() { Code = "204", Description = (NoResString)"17/2-c Yabancı Diplomatik Organ Ve Hayır Kurumlarının Yapacakları Bağışlarla İlgili Mal Ve Hizmet Alışları", Bool = true, RelatedItemCode = "204" },
				new CodeDescriptionBoolRelatedItem() { Code = "205", Description = (NoResString)"17/2-d Taşınmaz Kültür Varlıklarına İlişkin Teslimler ve Mimarlık Hizmetleri", Bool = true, RelatedItemCode = "205" },
				new CodeDescriptionBoolRelatedItem() { Code = "206", Description = (NoResString)"17/2-e Mesleki Kuruluşların İşlemleri", Bool = true, RelatedItemCode = "206" },
				new CodeDescriptionBoolRelatedItem() { Code = "207", Description = (NoResString)"17/3 Askeri Fabrika, Tersane ve Atölyelerin İşlemleri", Bool = true, RelatedItemCode = "207" },
				new CodeDescriptionBoolRelatedItem() { Code = "208", Description = (NoResString)"17/4-c Birleşme, Devir, Dönüşüm ve Bölünme İşlemleri", Bool = true, RelatedItemCode = "208" },
				new CodeDescriptionBoolRelatedItem() { Code = "209", Description = (NoResString)"17/1 Kültür ve Eğitim Amacı Taşıyan İşlemler", Bool = true, RelatedItemCode = "209" },
				new CodeDescriptionBoolRelatedItem() { Code = "211", Description = (NoResString)"17/4-h Zirai Amaçlı Su Teslimleri İle Köy Tüzel Kişiliklerince Yapılan İçme Suyu teslimleri", Bool = true, RelatedItemCode = "211" },
				new CodeDescriptionBoolRelatedItem() { Code = "212", Description = (NoResString)"17/4-ı Serbest Bölgelerde Verilen Hizmetler", Bool = true, RelatedItemCode = "212" },
				new CodeDescriptionBoolRelatedItem() { Code = "213", Description = (NoResString)"17/4-j Boru Hattı İle Yapılan Petrol Ve Gaz Taşımacılığı", Bool = true, RelatedItemCode = "213" },
				new CodeDescriptionBoolRelatedItem() { Code = "214", Description = (NoResString)"17/4-k Organize Sanayi Bölgelerindeki Arsa ve İşyeri Teslimleri İle Konut Yapı Kooperatiflerinin Üyelerine Konut Teslimleri", Bool = true, RelatedItemCode = "214" },
				new CodeDescriptionBoolRelatedItem() { Code = "215", Description = (NoResString)"17/4-l Varlık Yönetim Şirketlerinin İşlemleri", Bool = true, RelatedItemCode = "215" },
				new CodeDescriptionBoolRelatedItem() { Code = "216", Description = (NoResString)"17/4-m Tasarruf Mevduatı Sigorta Fonunun İşlemleri", Bool = true, RelatedItemCode = "216" },
				new CodeDescriptionBoolRelatedItem() { Code = "217", Description = (NoResString)"17/4-n Basın-Yayın ve Enformasyon Genel Müdürlüğüne Verilen Haber Hizmetleri", Bool = true, RelatedItemCode = "217" },
				new CodeDescriptionBoolRelatedItem() { Code = "218", Description = (NoResString)"KDV 17/4-o md. Gümrük Antrepoları, Geçici Depolama Yerleri ile Gümrüklü Sahalarda Vergisiz Satış Yapılan İşyeri, Depo ve Ardiye Gibi Bağımsız Birimlerin Kiralanması", Bool = true, RelatedItemCode = "218" },
				new CodeDescriptionBoolRelatedItem() { Code = "219", Description = (NoResString)"17/4-p Hazine ve Arsa Ofisi Genel Müdürlüğünün işlemleri", Bool = true, RelatedItemCode = "219" },
				new CodeDescriptionBoolRelatedItem() { Code = "220", Description = (NoResString)"17/4-r İki Tam Yıl Süreyle Sahip Olunan Taşınmaz ve İştirak Hisseleri ile 15/7/2023 tarihinden önce kurumların aktifinde kayıtlı Taşınmaz satışı", Bool = true, RelatedItemCode = "220" },
				new CodeDescriptionBoolRelatedItem() { Code = "221", Description = (NoResString)"Geçici 15 Konut Yapı Kooperatifleri, Belediyeler ve Sosyal Güvenlik Kuruluşlarına Verilen İnşaat Taahhüt Hizmeti", Bool = true, RelatedItemCode = "221" },
				new CodeDescriptionBoolRelatedItem() { Code = "223", Description = (NoResString)"Geçici 20/1 Teknoloji Geliştirme Bölgelerinde Yapılan İşlemler", Bool = true, RelatedItemCode = "223" },
				new CodeDescriptionBoolRelatedItem() { Code = "225", Description = (NoResString)"Geçici 23 Milli Eğitim Bakanlığına Yapılan Bilgisayar Bağışları İle İlgili Teslimler", Bool = true, RelatedItemCode = "225" },
				new CodeDescriptionBoolRelatedItem() { Code = "226", Description = (NoResString)"17/2-b Özel Okulları, Üniversite ve Yüksekokullar Tarafından Verilen Bedelsiz Eğitim Ve Öğretim Hizmetleri", Bool = true, RelatedItemCode = "226" },
				new CodeDescriptionBoolRelatedItem() { Code = "227", Description = (NoResString)"17/2-b Kanunların Gösterdiği Gerek Üzerine Bedelsiz Olarak Yapılan Teslim ve Hizmetler", Bool = true, RelatedItemCode = "227" },
				new CodeDescriptionBoolRelatedItem() { Code = "228", Description = (NoResString)"17/2-b Kanunun (17/1) Maddesinde Sayılan Kurum ve Kuruluşlara Bedelsiz Olarak Yapılan Teslimler", Bool = true, RelatedItemCode = "228" },
				new CodeDescriptionBoolRelatedItem() { Code = "229", Description = (NoResString)"17/2-b Gıda Bankacılığı Faaliyetinde Bulunan Dernek ve Vakıflara Bağışlanan Gıda, Temizlik, Giyecek ve Yakacak Maddeleri", Bool = true, RelatedItemCode = "229" },
				new CodeDescriptionBoolRelatedItem() { Code = "230", Description = (NoResString)"17/4-g Külçe Altın, Külçe Gümüş Ve Kiymetli Taşlarin Teslimi", Bool = true, RelatedItemCode = "230" },
				new CodeDescriptionBoolRelatedItem() { Code = "231", Description = (NoResString)"17/4-g Metal Plastik, Lastik, Kauçuk, Kağit, Cam Hurda Ve Atıkların Teslimi", Bool = true, RelatedItemCode = "231" },
				new CodeDescriptionBoolRelatedItem() { Code = "232", Description = (NoResString)"17/4-g Döviz, Para, Damga Pulu, Değerli Kağıtlar, Hisse Senedi ve Tahvil Teslimleri", Bool = true, RelatedItemCode = "232" },
				new CodeDescriptionBoolRelatedItem() { Code = "234", Description = (NoResString)"17/4-ş Konut Finansmanı Amacıyla Teminat Gösterilen ve İpotek Konulan Konutların Teslimi", Bool = true, RelatedItemCode = "234" },
				new CodeDescriptionBoolRelatedItem() { Code = "235", Description = (NoResString)"16/1-c Transit ve Gümrük Antrepo Rejimleri İle Geçici Depolama ve Serbest Bölge Hükümlerinin Uygulandığiı Malların Teslimi", Bool = true, RelatedItemCode = "235" },
				new CodeDescriptionBoolRelatedItem() { Code = "236", Description = (NoResString)"19/2 Usulüne Göre Yürürlüğe Girmiş Uluslararası Anlaşmalar Kapsamındaki İstisnalar (İade Hakkı Tanınmayan)", Bool = true, RelatedItemCode = "236" },
				new CodeDescriptionBoolRelatedItem() { Code = "237", Description = (NoResString)"17/4-t 5300 Sayılı Kanuna Göre Düzenlenen Ürün Senetlerinin İhtisas/Ticaret Borsaları Aracılığıyla İlk Teslimlerinden Sonraki Teslim", Bool = true, RelatedItemCode = "237" },
				new CodeDescriptionBoolRelatedItem() { Code = "238", Description = (NoResString)"17/4-u Varlıkların Varlık Kiralama Şirketlerine Devri İle Bu Varlıkların Varlık Kiralama Şirketlerince Kiralanması ve Devralınan Kuruma Devri", Bool = true , RelatedItemCode = "238" },
				new CodeDescriptionBoolRelatedItem() { Code = "239", Description = (NoResString)"17/4-y Taşınmazların Finansal Kiralama Şirketlerine Devri, Finansal Kiralama Şirketi Tarafından Devredene Kiralanması ve Devri", Bool = true, RelatedItemCode = "239" },
				new CodeDescriptionBoolRelatedItem() { Code = "240", Description = (NoResString)"17/4-z Patentli Veya Faydalı Model Belgeli Buluşa İlişkin Gayri Maddi Hakların Kiralanması, Devri ve Satışı", Bool = true , RelatedItemCode = "240" },
				new CodeDescriptionBoolRelatedItem() { Code = "241", Description = (NoResString)"TürkAkım Gaz Boru Hattı Projesine İlişkin Anlaşmanın (9/b) Maddesinde Yer Alan Hizmetler", Bool = true, RelatedItemCode = "241" },
				new CodeDescriptionBoolRelatedItem() { Code = "242", Description = (NoResString)"KDV 17/4-ö md. Gümrük Antrepoları, Geçici Depolama Yerleri ile Gümrüklü Sahalarda, İthalat ve İhracat İşlemlerine konu mallar ile transit rejim kapsamında işlem gören mallar için verilen ardiye, depolama ve terminal hizmetleri", Bool = true , RelatedItemCode = "242" },
				new CodeDescriptionBoolRelatedItem() { Code = "250", Description = (NoResString)"Diğerleri (Kısmi İstisna)", Bool = true, RelatedItemCode = "250" },
				new CodeDescriptionBoolRelatedItem() { Code = "301", Description = (NoResString)"11/1-a Mal İhracatı", Bool = true , RelatedItemCode = "301" },
				new CodeDescriptionBoolRelatedItem() { Code = "302", Description = (NoResString)"11/1-a Hizmet İhracatı", Bool = true , RelatedItemCode = "302" },
				new CodeDescriptionBoolRelatedItem() { Code = "303", Description = (NoResString)"11/1-a Roaming Hizmetleri", Bool = true , RelatedItemCode = "303" },
				new CodeDescriptionBoolRelatedItem() { Code = "304", Description = (NoResString)"13/a Deniz Hava ve Demiryolu Taşıma Araçlarının Teslimi İle İnşa, Tadil, Bakım ve Onarımları", Bool = true , RelatedItemCode = "304" },
				new CodeDescriptionBoolRelatedItem() { Code = "305", Description = (NoResString)"13/b Deniz ve Hava Taşıma Araçları İçin Liman Ve Hava Meydanlarında Yapılan Hizmetler", Bool = true , RelatedItemCode = "305" },
				new CodeDescriptionBoolRelatedItem() { Code = "306", Description = (NoResString)"13/c Petrol Aramaları ve Petrol Boru Hatlarının İnşa ve Modernizasyonuna İlişkin Yapılan Teslim ve Hizmetler", Bool = true , RelatedItemCode = "306" },
				new CodeDescriptionBoolRelatedItem() { Code = "307", Description = (NoResString)"13/c Maden Arama, Altın, Gümüş ve Platin Madenleri İçin İşletme, Zenginleştirme Ve Rafinaj Faaliyetlerine İlişkin Teslim Ve Hizmetler[KDVGUT-(II/8-4)]", Bool = true , RelatedItemCode = "307" },
				new CodeDescriptionBoolRelatedItem() { Code = "308", Description = (NoResString)"13/d Teşvikli Yatırım Mallarının Teslimi", Bool = true , RelatedItemCode = "308" },
				new CodeDescriptionBoolRelatedItem() { Code = "309", Description = (NoResString)"13/e Liman Ve Hava Meydanlarının İnşası, Yenilenmesi Ve Genişletilmesi", Bool = true , RelatedItemCode = "309" },
				new CodeDescriptionBoolRelatedItem() { Code = "310", Description = (NoResString)"13/f Ulusal Güvenlik Amaçlı Teslim ve Hizmetler", Bool = true , RelatedItemCode = "310" },
				new CodeDescriptionBoolRelatedItem() { Code = "311", Description = (NoResString)"14/1 Uluslararası Taşımacılık", Bool = true , RelatedItemCode = "311" },
				new CodeDescriptionBoolRelatedItem() { Code = "312", Description = (NoResString)" 15/a Diplomatik Organ Ve Misyonlara Yapılan Teslim ve Hizmetler", Bool = true , RelatedItemCode = "312" },
				new CodeDescriptionBoolRelatedItem() { Code = "313", Description = (NoResString)" 15/b Uluslararası Kuruluşlara Yapılan Teslim ve Hizmetler", Bool = true , RelatedItemCode = "313" },
				new CodeDescriptionBoolRelatedItem() { Code = "314", Description = (NoResString)"19/2 Usulüne Göre Yürürlüğe Girmiş Uluslar Arası Anlaşmalar Kapsamındaki İstisnalar", Bool = true , RelatedItemCode = "314" },
				new CodeDescriptionBoolRelatedItem() { Code = "315", Description = (NoResString)"14/3 İhraç Konusu Eşyayı Taşıyan Kamyon, Çekici ve Yarı Romorklara Yapılan Motorin Teslimleri", Bool = true , RelatedItemCode = "315" },
				new CodeDescriptionBoolRelatedItem() { Code = "316", Description = (NoResString)"11/1-a Serbest Bölgelerdeki Müşteriler İçin Yapılan Fason Hizmetler", Bool = true , RelatedItemCode = "316" },
				new CodeDescriptionBoolRelatedItem() { Code = "317", Description = (NoResString)"17/4-s Engellilerin Eğitimleri, Meslekleri ve Günlük Yaşamlarına İlişkin Araç-Gereç ve Bilgisayar Programları", Bool = true , RelatedItemCode = "317" },
				new CodeDescriptionBoolRelatedItem() { Code = "318", Description = (NoResString)"Geçici 29 3996 Sayılı Kanuna Göre Gerçekleştirilecek Projeler ve 652 Sayılı Kanun Hükmünde Kararnameye Göre Kiralama Karşılığı Yaptırılan Eğitim Öğretim Tesislerine İlişkin Projelere İlişkin Teslim ve Hizmetler", Bool = true , RelatedItemCode = "318" },
				new CodeDescriptionBoolRelatedItem() { Code = "319", Description = (NoResString)"13/g Başbakanlık Merkez Teşkilatına Yapılan Araç Teslimleri", Bool = true , RelatedItemCode = "319" },
				new CodeDescriptionBoolRelatedItem() { Code = "320", Description = (NoResString)"Geçici 16 (6111 sayılı K.) İSMEP Kapsamında İstanbul İl Özel İdaresi'ne Bağlı Olarak Faaliyet Gösteren \"İstanbul Proje Koordinasyon Birim\"ine Yapılacak Teslim ve Hizmetler", Bool = true , RelatedItemCode = "320" },
				new CodeDescriptionBoolRelatedItem() { Code = "321", Description = (NoResString)"Geçici 26 Birleşmiş Milletler(BM) ile Kuzey Atlantik Antlaşması Teşkilatı(NATO) Temsilcilikleri ve Bu Teşkilatlara Bağlı Program, Fon ve Özel İhtisas Kuruluşları ile İktisadi İşbirliği ve Kalkınma Teşkilatına(OECD) Yapılacak Mal Teslimi ve Hizmet İfaları", Bool = true , RelatedItemCode = "321" },
				new CodeDescriptionBoolRelatedItem() { Code = "322", Description = (NoResString)"11/1-a Türkiye'de İkamet Etmeyenlere Özel Fatura ile Yapılan Teslimler (Bavul Ticareti)", Bool = true , RelatedItemCode = "322" },
				new CodeDescriptionBoolRelatedItem() { Code = "323", Description = (NoResString)"13/ğ 5300 Sayılı Kanuna Göre Düzenlenen Ürün Senetlerinin İhtisas/Ticaret Borsaları Aracılığıyla İlk Teslimi", Bool = true , RelatedItemCode = "323" },
				new CodeDescriptionBoolRelatedItem() { Code = "324", Description = (NoResString)"13/h Türkiye Kızılay Derneğine Yapılan Teslim ve Hizmetler ile Türkiye Kızılay Derneğinin Teslim ve Hizmetleri", Bool = true , RelatedItemCode = "324" },
				new CodeDescriptionBoolRelatedItem() { Code = "325", Description = (NoResString)"13/ı Yem Teslimleri", Bool = true , RelatedItemCode = "325" },
				new CodeDescriptionBoolRelatedItem() { Code = "326", Description = (NoResString)"13/ı Gıda, Tarım ve Hayvancılık Bakanlığı Tarafından Tescil Edilmiş Gübrelerin Teslimi", Bool = true , RelatedItemCode = "326" },
				new CodeDescriptionBoolRelatedItem() { Code = "327", Description = (NoResString)"13/ı Gıda, Tarım ve Hayvancılık Bakanlığı Tarafından Tescil Edilmiş Gübrelerin İçeriğinde Bulunan Hammaddelerin Gübre Üreticilerine Teslimi", Bool = true , RelatedItemCode = "327" },
				new CodeDescriptionBoolRelatedItem() { Code = "328", Description = (NoResString)"13/i Konut veya İşyeri Teslimleri", Bool = true , RelatedItemCode = "328" },
				new CodeDescriptionBoolRelatedItem() { Code = "330", Description = (NoResString)"KDV 13/j md. Organize Sanayi Bölgeleri ile Küçük Sanayi Sitelerinin İnşasına İlişkin Teslim ve Hizmetler", Bool = true , RelatedItemCode = "330" },
				new CodeDescriptionBoolRelatedItem() { Code = "331", Description = (NoResString)"KDV 13/m md. Ar-Ge, Yenilik ve Tasarım Faaliyetlerinde Kullanılmak Üzere Yapılan Yeni Makina ve Teçhizat Teslimlerinde İstisna", Bool = true , RelatedItemCode = "331" },
				new CodeDescriptionBoolRelatedItem() { Code = "332", Description = (NoResString)"KDV Geçici 39. Md. İmalat Sanayiinde Kullanılmak Üzere Yapılan Yeni Makina ve Teçhizat Teslimlerinde İstisna", Bool = true , RelatedItemCode = "332" },
				new CodeDescriptionBoolRelatedItem() { Code = "333", Description = (NoResString)"KDV 13/k md. Kapsamında Genel ve Özel Bütçeli Kamu İdarelerine, İl Özel İdarelerine, Belediyelere ve Köylere bağışlanan Tesislerin İnşasına İlişkin İstisna", Bool = true , RelatedItemCode = "333" },
				new CodeDescriptionBoolRelatedItem() { Code = "334", Description = (NoResString)"KDV 13/l md. Kapsamında Yabancılara Verilen Sağlık Hizmetlerinde İstisna", Bool = true , RelatedItemCode = "334" },
				new CodeDescriptionBoolRelatedItem() { Code = "335", Description = (NoResString)"KDV 13/n Basılı Kitap ve Süreli Yayınların Teslimleri", Bool = true , RelatedItemCode = "335" },
				new CodeDescriptionBoolRelatedItem() { Code = "336", Description = (NoResString)"Geçici 40 UEFA Müsabakaları Kapsamında Yapılacak Teslim ve Hizmetler", Bool = true , RelatedItemCode = "336" },
				new CodeDescriptionBoolRelatedItem() { Code = "337", Description = (NoResString)"Türk Akım Gaz Boru Hattı Projesine İlişkin Anlaşmanın (9/h) Maddesi Kapsamındaki Gaz Taşıma Hizmetleri", Bool = true , RelatedItemCode = "337" },
				new CodeDescriptionBoolRelatedItem() { Code = "338", Description = (NoResString)"İmalatçıların Mal İhracatları", Bool = true , RelatedItemCode = "338" },
				new CodeDescriptionBoolRelatedItem() { Code = "339", Description = (NoResString)"İmalat Sanayii ile Turizme Yönelik Yatırım Teşvik Belgesi Kapsamındaki İnşaat İşlerine İlişkin Teslim ve Hizmetler", Bool = true , RelatedItemCode = "339" },
				new CodeDescriptionBoolRelatedItem() { Code = "340", Description = (NoResString)"Elektrik Motorlu Taşıt Araçlarının Geliştirilmesine Yönelik Mühendislik Hizmetleri", Bool = true , RelatedItemCode = "340" },
				new CodeDescriptionBoolRelatedItem() { Code = "341", Description = (NoResString)"Afetzedelere Bağışlanacak Konutların İnşasına İlişkin İstisna", Bool = true , RelatedItemCode = "341" },
				new CodeDescriptionBoolRelatedItem() { Code = "350", Description = (NoResString)"Diğerleri", Bool = true , RelatedItemCode = "350" },
				new CodeDescriptionBoolRelatedItem() { Code = "351", Description = (NoResString)"KDV - İstisna Olmayan Diğer", Bool = true , RelatedItemCode = "351" },
				new CodeDescriptionBoolRelatedItem() { Code = "601", Description = (NoResString)"Yapim İşleri İle Bu İşlerle Birlikte İfa Edilen Mühendislik-Mimarlik Ve Etüt-Proje Hizmetleri", Bool = true , RelatedItemCode = "601" },
				new CodeDescriptionBoolRelatedItem() { Code = "602", Description = (NoResString)"Etüt, Plan-Proje, Danişmanlik, Denetim Ve Benzeri Hizmetler", Bool = true , RelatedItemCode = "602" },
				new CodeDescriptionBoolRelatedItem() { Code = "603", Description = (NoResString)"Makine, Teçhizat, Demirbaş Ve Taşitlara Ait Tadil, Bakim Ve Onarim Hizmetleri", Bool = true , RelatedItemCode = "603" },
				new CodeDescriptionBoolRelatedItem() { Code = "604", Description = (NoResString)"Yemek Servis Hizmeti", Bool = true , RelatedItemCode = "604" },
				new CodeDescriptionBoolRelatedItem() { Code = "605", Description = (NoResString)"Organizasyon Hizmeti", Bool = true , RelatedItemCode = "605" },
				new CodeDescriptionBoolRelatedItem() { Code = "606", Description = (NoResString)"İşgücü Temin Hizmetleri", Bool = true , RelatedItemCode = "606" },
				new CodeDescriptionBoolRelatedItem() { Code = "607", Description = (NoResString)"Özel Güvenlik Hizmeti", Bool = true , RelatedItemCode = "607" },
				new CodeDescriptionBoolRelatedItem() { Code = "608", Description = (NoResString)"Yapi Denetim Hizmetleri", Bool = true , RelatedItemCode = "608" },
				new CodeDescriptionBoolRelatedItem() { Code = "609", Description = (NoResString)"Fason Olarak Yaptirilan Tekstil Ve Konfeksiyon İşleri, Çanta Ve Ayakkabi Dikim İşleri Ve Bu İşlere Aracilik Hizmetleri", Bool = true , RelatedItemCode = "609" },
				new CodeDescriptionBoolRelatedItem() { Code = "610", Description = (NoResString)"Turistik Mağazalara Verilen Müşteri Bulma / Götürme Hizmetleri", Bool = true , RelatedItemCode = "610" },
				new CodeDescriptionBoolRelatedItem() { Code = "611", Description = (NoResString)"Spor Kulüplerinin Yayin, Reklâm Ve İsim Hakki Gelirlerine Konu İşlemleri", Bool = true , RelatedItemCode = "611" },
				new CodeDescriptionBoolRelatedItem() { Code = "612", Description = (NoResString)"Temizlik Hizmeti", Bool = true , RelatedItemCode = "612" },
				new CodeDescriptionBoolRelatedItem() { Code = "613", Description = (NoResString)"Çevre Ve Bahçe Bakim Hizmetleri", Bool = true , RelatedItemCode = "613" },
				new CodeDescriptionBoolRelatedItem() { Code = "614", Description = (NoResString)"Servis Taşimaciliği Hizmeti", Bool = true , RelatedItemCode = "614" },
				new CodeDescriptionBoolRelatedItem() { Code = "615", Description = (NoResString)"Her Türlü Baski Ve Basim Hizmetleri", Bool = true , RelatedItemCode = "615" },
				new CodeDescriptionBoolRelatedItem() { Code = "616", Description = (NoResString)"Diğer Hizmetler [Kdvgut-(I/C-2.1.3.2.13)]", Bool = true , RelatedItemCode = "616" },
				new CodeDescriptionBoolRelatedItem() { Code = "617", Description = (NoResString)"Hurda Metalden Elde Edilen Külçe Teslimleri", Bool = true , RelatedItemCode = "617" },
				new CodeDescriptionBoolRelatedItem() { Code = "618", Description = (NoResString)"Hurda Metalden Elde Edilenler Dişindaki Bakir, Çinko Demir; Çelik Alüminyum Ve Kurşun Külçe Teslimleri [Kdvgut-(I/C-2.1.3.3.1)]", Bool = true , RelatedItemCode = "618" },
				new CodeDescriptionBoolRelatedItem() { Code = "619", Description = (NoResString)"Bakir, Çinko Ve Alüminyum Ürünlerinin Teslimi", Bool = true , RelatedItemCode = "619" },
				new CodeDescriptionBoolRelatedItem() { Code = "620", Description = (NoResString)"İstisnadan Vazgeçenlerin Hurda Ve Atik Teslimi", Bool = true , RelatedItemCode = "620" },
				new CodeDescriptionBoolRelatedItem() { Code = "621", Description = (NoResString)"Metal, Plastik, Lastik, Kauçuk, Kâğit Ve Cam Hurda Ve Atiklardan Elde Edilen Hammadde Teslimi", Bool = true , RelatedItemCode = "621" },
				new CodeDescriptionBoolRelatedItem() { Code = "622", Description = (NoResString)"Pamuk, Tiftik, Yün Ve Yapaği İle Ham Post Ve Deri Teslimleri", Bool = true , RelatedItemCode = "622" },
				new CodeDescriptionBoolRelatedItem() { Code = "623", Description = (NoResString)"Ağaç Ve Orman Ürünleri Teslimi", Bool = true , RelatedItemCode = "623" },
				new CodeDescriptionBoolRelatedItem() { Code = "624", Description = (NoResString)"Yük Taşimaciliği Hizmeti [Kdvgut-(I/C-2.1.3.2.11)]", Bool = true , RelatedItemCode = "624" },
				new CodeDescriptionBoolRelatedItem() { Code = "625", Description = (NoResString)"Ticari Reklam Hizmetleri [Kdvgut-(I/C-2.1.3.2.15)]", Bool = true , RelatedItemCode = "625" },
				new CodeDescriptionBoolRelatedItem() { Code = "626", Description = (NoResString)"Diğer Teslimler [Kdvgut-(I/C-2.1.3.3.7.)]", Bool = true , RelatedItemCode = "626" },
				new CodeDescriptionBoolRelatedItem() { Code = "627", Description = (NoResString)"Demir-Çelik Ürünlerinin Teslimi [Kdvgut-(I/C-2.1.3.3.8)]", Bool = true , RelatedItemCode = "627" },
				new CodeDescriptionBoolRelatedItem() { Code = "801", Description = (NoResString)"Yapım İşleri ile Bu İşlerle Birlikte İfa Edilen Mühendislik-Mimarlık ve Etüt-Proje Hizmetleri[KDVGUT-(I/C-2.1.3.2.1)]", Bool = true , RelatedItemCode = "801" },
				new CodeDescriptionBoolRelatedItem() { Code = "802", Description = (NoResString)"Etüt, Plan-Proje, Danışmanlık, Denetim ve Benzeri Hizmetler[KDVGUT-(I/C-2.1.3.2.2)]", Bool = true , RelatedItemCode = "802" },
				new CodeDescriptionBoolRelatedItem() { Code = "803", Description = (NoResString)"Makine, Teçhizat, Demirbaş ve Taşıtlara Ait Tadil, Bakım ve Onarım Hizmetleri[KDVGUT- (I/C-2.1.3.2.3)]", Bool = true , RelatedItemCode = "803" },
				new CodeDescriptionBoolRelatedItem() { Code = "804", Description = (NoResString)"Yemek Servis Hizmeti[KDVGUT-(I/C-2.1.3.2.4)]", Bool = true , RelatedItemCode = "804" },
				new CodeDescriptionBoolRelatedItem() { Code = "805", Description = (NoResString)"Organizasyon Hizmeti[KDVGUT-(I/C-2.1.3.2.4)]", Bool = true , RelatedItemCode = "805" },
				new CodeDescriptionBoolRelatedItem() { Code = "806", Description = (NoResString)"İşgücü Temin Hizmetleri[KDVGUT-(I/C-2.1.3.2.5)]", Bool = true , RelatedItemCode = "806" },
				new CodeDescriptionBoolRelatedItem() { Code = "807", Description = (NoResString)"Özel Güvenlik Hizmeti[KDVGUT-(I/C-2.1.3.2.5)]", Bool = true , RelatedItemCode = "807" },
				new CodeDescriptionBoolRelatedItem() { Code = "808", Description = (NoResString)"Yapı Denetim Hizmetleri[KDVGUT-(I/C-2.1.3.2.6)]", Bool = true , RelatedItemCode = "808" },
				new CodeDescriptionBoolRelatedItem() { Code = "809", Description = (NoResString)"Fason Olarak Yaptırılan Tekstil ve Konfeksiyon İşleri, Çanta ve Ayakkabı Dikim İşleri ve Bu İşlere Aracılık Hizmetleri[KDVGUT-(I/C-2.1.3.2.7)]", Bool = true , RelatedItemCode = "809" },
				new CodeDescriptionBoolRelatedItem() { Code = "810", Description = (NoResString)"Turistik Mağazalara Verilen Müşteri Bulma/ Götürme Hizmetleri[KDVGUT-(I/C-2.1.3.2.8)]", Bool = true , RelatedItemCode = "810" },
				new CodeDescriptionBoolRelatedItem() { Code = "811", Description = (NoResString)"Spor Kulüplerinin Yayın, Reklâm ve İsim Hakkı Gelirlerine Konu İşlemleri[KDVGUT-(I/C-2.1.3.2.9)]", Bool = true , RelatedItemCode = "811" },
				new CodeDescriptionBoolRelatedItem() { Code = "812", Description = (NoResString)"Temizlik Hizmeti[KDVGUT-(I/C-2.1.3.2.10)]", Bool = true , RelatedItemCode = "812" },
				new CodeDescriptionBoolRelatedItem() { Code = "813", Description = (NoResString)"Çevre ve Bahçe Bakım Hizmetleri[KDVGUT-(I/C-2.1.3.2.10)]", Bool = true , RelatedItemCode = "813" },
				new CodeDescriptionBoolRelatedItem() { Code = "814", Description = (NoResString)"Servis Taşımacılığı Hizmeti[KDVGUT-(I/C-2.1.3.2.11)]", Bool = true , RelatedItemCode = "814" },
				new CodeDescriptionBoolRelatedItem() { Code = "815", Description = (NoResString)"Her Türlü Baskı ve Basım Hizmetleri[KDVGUT-(I/C-2.1.3.2.12)]", Bool = true , RelatedItemCode = "815" },
				new CodeDescriptionBoolRelatedItem() { Code = "816", Description = (NoResString)"Hurda Metalden Elde Edilen Külçe Teslimleri[KDVGUT-(I/C-2.1.3.3.1)]", Bool = true , RelatedItemCode = "816" },
				new CodeDescriptionBoolRelatedItem() { Code = "817", Description = (NoResString)"Hurda Metalden Elde Edilenler Dışındaki Bakır, Çinko, Demir Çelik, Alüminyum ve Kurşun Külçe Teslimi [KDVGUT-(I/C-", Bool = true , RelatedItemCode = "817" },
				new CodeDescriptionBoolRelatedItem() { Code = "818", Description = (NoResString)"Bakır, Çinko, Alüminyum ve Kurşun Ürünlerinin Teslimi[KDVGUT-(I/C-2.1.3.3.2)]", Bool = true , RelatedItemCode = "818" },
				new CodeDescriptionBoolRelatedItem() { Code = "819", Description = (NoResString)"İstisnadan Vazgeçenlerin Hurda ve Atık Teslimi[KDVGUT-(I/C-2.1.3.3.3)]", Bool = true , RelatedItemCode = "819" },
				new CodeDescriptionBoolRelatedItem() { Code = "820", Description = (NoResString)"Metal, Plastik, Lastik, Kauçuk, Kâğıt ve Cam Hurda ve Atıklardan Elde Edilen Hammadde Teslimi[KDVGUT-(I/C-2.1.3.3.4)]", Bool = true , RelatedItemCode = "820" },
				new CodeDescriptionBoolRelatedItem() { Code = "821", Description = (NoResString)"Pamuk, Tiftik, Yün ve Yapağı İle Ham Post ve Deri Teslimleri[KDVGUT-(I/C-2.1.3.3.5)]", Bool = true , RelatedItemCode = "821" },
				new CodeDescriptionBoolRelatedItem() { Code = "822", Description = (NoResString)"Ağaç ve Orman Ürünleri Teslimi[KDVGUT-(I/C-2.1.3.3.6)]", Bool = true , RelatedItemCode = "822" },
				new CodeDescriptionBoolRelatedItem() { Code = "823", Description = (NoResString)"Yük Taşımacılığı Hizmeti [KDVGUT-(I/C-2.1.3.2.11)]", Bool = true , RelatedItemCode = "823" },
				new CodeDescriptionBoolRelatedItem() { Code = "824", Description = (NoResString)"Ticari Reklam Hizmetleri [KDVGUT-(I/C-2.1.3.2.15)]", Bool = true , RelatedItemCode = "824" },
				new CodeDescriptionBoolRelatedItem() { Code = "825", Description = (NoResString)"Demir-Çelik Ürünlerinin Teslimi [KDVGUT-(I/C-2.1.3.3.8)]", Bool = true , RelatedItemCode = "825" },
			};

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "VAT";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "VAT";

			AssertEquals(expected, result);
		}

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Turkey()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(TurkeyComplianceInfo.RuleSetCodes.eFactura, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(TurkeyComplianceInfo.RuleSetCodes.eFactura));
			Assert(ruleSet.ContainsCode(TurkeyComplianceInfo.RuleSetCodes.eArchive));
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_EnableNewTurkeyARComplianceFeaturesFrom_eFactura_Default()
		{
			var expected = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "ICN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "RTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "EAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "ICN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "RTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "EIN");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EIN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ERO", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EIN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ERO", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "EIN");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "ICN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "RTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "EIC");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EIC", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ERC", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EIC", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ERC", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "EIC");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EAR", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EAR", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "EAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CIN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ATO", disbursementRule: "ALL", taxRegistrationType: "ERG", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CAR", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ATO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "XCL");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "XCL");

			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DCL", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "DCL");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DCL", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "DCL");

			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DAR", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ALL", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DIN", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ALL", disbursementRule: "ALL", taxRegistrationType: "ERO", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DIN", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ALL", disbursementRule: "ALL", taxRegistrationType: "ERC", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PIN", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ERO", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PIC", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ERC", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PAR", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PCL", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PCL", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PCL", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "PCL");

			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "CAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "CIN");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "CCL");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "1", ruleSetDescription: "e-Fatura", parentTransactionSubType: "CCL");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime()))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expected);
			}
		}

		public void TestComplianceSubTypeAttributionRuleConfiguration_EnableNewTurkeyARComplianceFeaturesFrom_eArchive()
		{
			var expected = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "ICN", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "RTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "EAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CAR", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ATO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "EAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EAR", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "EAR", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "EAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "XCL");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "XCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "XCL");

			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DCL", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "DCL");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DCL", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "DCL");

			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "DAR", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "TID", originalRule: "ALL", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PAR", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "NER", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PCL", ledger: "AP", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PCL", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "OTO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "PCL", ledger: "AP", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "PCL");

			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCN", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "TID", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "CAR");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCL", ledger: "AR", invoiceType: "INV", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "CCL");
			AddComplianceSubTypeAttributionRule(expected, country: "TR", subType: "CCL", ledger: "AR", invoiceType: "CRD", taxInvoiceRule: "EXL", originalRule: "ARO", disbursementRule: "ALL", taxRegistrationType: "ALL", ruleSetCode: "2", ruleSetDescription: "e-Archive Only", parentTransactionSubType: "CCL");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime()))
			{
				var fallback = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var ruleSet = new ComplianceSubTypeAttributionRuleSet(fallback, Factory, TurkeyComplianceInfo.RuleSetCodes.eArchive);
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleSet.SetValue(fallback.CompanyPK(false), Guid.Empty, Guid.Empty, ruleSet);

				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				AssertTestComplianceSubTypeAttributionRuleConfiguration(collection, expected);
			}
		}

		#region IOriginalInvoiceNumberAndDateValidationDecider

		protected override string[] OriginalInvoiceNumberAndDateValidationSubTypes =>
			[
				TurkeyComplianceInfo.ComplianceSubTypeCodes.DAR,
				TurkeyComplianceInfo.ComplianceSubTypeCodes.DIN
			];

		protected override bool ExpectedShouldValidateOriginalTransactionNumberAndDate => expectedShouldValidateOriginalTransactionNumberAndDate;

		bool expectedShouldValidateOriginalTransactionNumberAndDate;

		public override void TestIOriginalInvoiceNumberAndDateValidationDecider()
		{
			expectedShouldValidateOriginalTransactionNumberAndDate = true;
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				base.TestIOriginalInvoiceNumberAndDateValidationDecider();
			}

			expectedShouldValidateOriginalTransactionNumberAndDate = false;
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				base.TestIOriginalInvoiceNumberAndDateValidationDecider();
			}
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProviderBase

		[TestDate(2020, 11, 15)]
		[TestDateIncremental]
		public override void TestIsCountryEnableComplianceEInvoicing()
		{
			var ruleSetProvider = TestObject as IComplianceInfoEInvoicingGUIActionProvider;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime()))
			{
				Assert(ruleSetProvider.IsCountryEnableComplianceEInvoicing());
				Assert(ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));

				using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime().AddSeconds(1)))
				using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime().AddSeconds(1)))
				{
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
				Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
				Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing());
					Assert(!ruleSetProvider.IsCountryEnableComplianceEInvoicing(isAPTransaction: true));
				}
			}
		}

		public void TestGetEligibleInvoices()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionProvider;

			var transactions = new List<AccTransactionHeader>();

			var invoiceWithEAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithEAR.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithEAR.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
			transactions.Add(invoiceWithEAR);

			var invoiceWithEIN = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithEIN.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithEIN.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIN;
			transactions.Add(invoiceWithEIN);

			var invoiceWithEIC = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithEIC.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithEIC.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EIC;
			transactions.Add(invoiceWithEIC);

			var invoiceReversedWithEAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceReversedWithEAR.AH_TransactionType = TransactionTypes.Invoice;
			invoiceReversedWithEAR.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
			invoiceReversedWithEAR.AH_IsCancelled = true;
			transactions.Add(invoiceReversedWithEAR);

			var invoiceWithICN = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceWithICN.AH_TransactionType = TransactionTypes.Invoice;
			invoiceWithICN.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
			transactions.Add(invoiceWithICN);

			var creditNoteWithEAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			creditNoteWithEAR.AH_TransactionType = TransactionTypes.CreditNote;
			creditNoteWithEAR.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR;
			transactions.Add(creditNoteWithEAR);

			var eligibleInvoices = complianceInfo.GetEligibleInvoices(transactions);

			AssertEquals(3, eligibleInvoices.Count());
			Assert(eligibleInvoices.Contains(invoiceWithEAR));
			Assert(eligibleInvoices.Contains(invoiceWithEIN));
			Assert(eligibleInvoices.Contains(invoiceWithEIC));
		}

		public void TestExistActiveDocumentRequestPivot()
		{
			AccountingMasterFilesRegistry.Instance.TaxInvoiceStatusUpdateAutomatedRequestSchedule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);

			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionProvider;
			var lastSentDate = ZDateTime.UtcNow;
			AssertEquals("Last sent date is not past 1 hour so we cannot re send document request.", true, complianceInfo.ExistActiveDocumentRequestPivot(lastSentDate));

			lastSentDate = ZDateTime.UtcNow.AddHours(-2);
			AssertEquals("Last sent date has past 1 hour so we cannot re send document request.", false, complianceInfo.ExistActiveDocumentRequestPivot(lastSentDate));
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public void TestComplianceInfoEInvoicingGUIActionDocumentRequestAR()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionDocumentRequest;

			AssertEquals("Request e-Invoice PDF Copy", complianceInfo.DocumentRequestMenuName);
			AssertEquals(@"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction does not have a Compliance Sub Type of EIN, EIC, or EAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.", complianceInfo.DocumentRequestActionInformation);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionStatusRequest

		public void TestComplianceInfoEInvoicingGUIActionStatusRequestAR()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionStatusRequest;

			AssertEquals("Request e-Invoice Transaction Status Update", complianceInfo.StatusRequestMenuName);
			AssertEquals(@"Your request for an update to the status of the electronic invoice is now being processed.
Please Note:
- No status request will be made for electronic invoices under the following conditions:
    When transaction already has a status of Success.
    When transaction has been Canceled.
    When transaction does not have a compliance sub type of EIN, EIC or EAR attached.
    When transaction does not have an ETTN attached.
    When transaction's status has already been requested.", complianceInfo.StatusRequestActionInformation);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionDocumentRequestAP

		public void TestComplianceInfoEInvoicingGUIActionDocumentRequestAP()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionDocumentRequest;
			var complianceInfoAP = complianceInfo as IComplianceInfoEInvoicingGUIActionDocumentRequestAP;

			AssertEquals("Request e-Invoice PDF Copy", complianceInfo.DocumentRequestMenuName);
			AssertEquals(@"Your request for a PDF copy of the tax invoice is being processed.
Please Note:
- No request for a copy of the tax invoice will be made for electronic invoices under the following conditions:
    When transaction is not Payable Credit Note
    When transaction does not have a Compliance Sub Type of DIN or DAR attached.
    When transaction has been reversed.
    When transaction does not have an ETTN attached.
    When transaction has an existing request for a copy where it is still being processed.", complianceInfoAP.DocumentRequestActionInformationAP);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionStatusRequestAP

		public void TestComplianceInfoEInvoicingGUIActionStatusRequestAP()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceInfoEInvoicingGUIActionStatusRequest;
			var complianceInfoAP = complianceInfo as IComplianceInfoEInvoicingGUIActionStatusRequestAP;

			AssertEquals("Request e-Invoice Transaction Status Update", complianceInfo.StatusRequestMenuName);
			AssertEquals(@"Your request for an update to the status of the electronic invoice is now being processed.
Please Note:
- No status request will be made for electronic invoices under the following conditions:
    When transaction is not Payable Credit Note.
    When transaction already has a status of Success.
    When transaction has been Canceled.
    When transaction does not have a compliance sub type of DIN or DAR attached.
    When transaction does not have an ETTN attached.
    When transaction's status has already been requested.", complianceInfoAP.StatusRequestActionInformationAP);
		}

		#endregion

		#region IProtectComplianceSubTypeForEInvoicingTransactions

		public void TestIProtectComplianceSubTypeForEInvoicingTransactions()
		{
			var expectedErrorMessageAP = "You cannot change the current 'PIN' value of Compliance Sub Type for e-Reporting transactions.";
			var expectedErrorMessageAR = "You cannot change the current 'CIN' value of Compliance Sub Type for e-Reporting transactions.";
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IProtectComplianceSubTypeForEInvoicingTransactions;

			var mockEInvoicingHelper = new Mock<IEInvoicingHelper>();
			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX")).Returns(true);
			ObjectFactory.Substitute(mockEInvoicingHelper.Object);

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();

			AssertEquals("Transaction is not saved", string.Empty, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Never);
			Factory.Save();

			AssertEquals("Compliance sub type is empty", string.Empty, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Never);

			transaction.AH_ComplianceSubType = "PIN";
			AssertEquals("Original value of compliance sub type is empty", string.Empty, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Never);
			Factory.Save();

			transaction.AH_ComplianceSubType = "PIC";
			AssertEquals("Original value of compliance sub type is changed", string.Empty, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Never);

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("Transaction is not AP Invoice or AR Credit Note", string.Empty, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Never);

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("Transaction is AP Invoice", expectedErrorMessageAP, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Once);

			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("Transaction is AR Credit Note", expectedErrorMessageAR, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Exactly(2));

			transaction.AH_ComplianceSubType = "CIN";
			AssertEquals("Transaction is AR Credit Note with Expected Compliance Sub Type", string.Empty, complianceInfo.ErrorMessageIfComplianceSubTypeIsProtected(transaction));
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.Exactly(2));

			complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCodes.Australia) as IProtectComplianceSubTypeForEInvoicingTransactions;
			AssertNull(complianceInfo);
			mockEInvoicingHelper.Verify(x => x.HasActiveEInvoicingTransactionPivot(It.IsAny<AccTransactionHeader>(), "CRX"), Times.AtLeast(2));
		}

		#endregion

		#region IEnableTransactionsPendingAllocationAllocateAsReceivable

		public void TestGetEligibleComplianceSubTypeForAllocateAsReceivable()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode);
			var complianceSubTypes = (complianceInfo as IComplianceSubTypeCodeProvider).GetComplianceSubTypes();

			foreach (var complianceSubType in complianceSubTypes)
			{
				var returnValue = (complianceInfo as IEnableTransactionsPendingAllocationAllocateAsReceivable).GetEligibleComplianceSubTypeForAllocateAsReceivable(complianceSubType.Code);
				var shouldBeConverted = AllocateAsReceivableConversionConfiguration.TryGetValue(complianceSubType.Code, out var convertedValue);
				Assert("Tested Compliance Sub Type: " + complianceSubType.Code, shouldBeConverted ? returnValue == convertedValue : returnValue == complianceSubType.Code);
			}
		}

		public void TestIsComplianceSubTypeNotEligibleForAllocateAsReceivable()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode);
			var complianceSubTypes = (complianceInfo as IComplianceSubTypeCodeProvider).GetComplianceSubTypes();

			foreach (var complianceSubType in complianceSubTypes)
			{
				var returnValue = (complianceInfo as IEnableTransactionsPendingAllocationAllocateAsReceivable).IsComplianceSubTypeNotEligibleForAllocateAsReceivable(complianceSubType.Code);
				var expectedValue = !AllocateAsReceivableConversionConfiguration.ContainsValue(complianceSubType.Code);
				AssertEquals("Tested Compliance Sub Type: " + complianceSubType.Code, expectedValue, returnValue);
			}
		}

		readonly Dictionary<string, string> AllocateAsReceivableConversionConfiguration = new Dictionary<string, string>()
		{
			{ TurkeyComplianceInfo.ComplianceSubTypeCodes.PAR, TurkeyComplianceInfo.ComplianceSubTypeCodes.CAR },
			{ TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN, TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN },
			{ TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC, TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN },
		};

		#endregion

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTax()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("QST", complianceInfo.GetExtraTaxDescription("QCT"));
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			var caption = complianceInfo.GetExtraTaxOSAmountCaption();
			AssertEquals("VAT Withholding Amt", caption.ShortCaption);
			AssertEquals("VAT Withholding Amount", caption.Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			var caption = complianceInfo.GetExtraTaxLocalAmountCaption();
			AssertEquals("VAT Withholding Local", caption.Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("VAT Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("VAT Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("VAT Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}

		public void TestComplianceSubTypeDependencyConfigurationForTurkey()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				AssertEquals(0, collection.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime()))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

				AssertEquals(6, collection.Count);
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[0], "TR", "EIC", "EIN");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[1], "TR", "DIN", "EIN");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[2], "TR", "DCL", "XCL");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[3], "TR", "DAR", "EAR");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[4], "TR", "DCN", "ICN");
				AssertComplianceSubTypeDependencyConfigurationCollection(collection[5], "TR", "CCL", "PCL");
			}

			void AssertComplianceSubTypeDependencyConfigurationCollection(ComplianceSubTypeDependencyConfiguration item, string expectedCountry, string expectedChildSubType, string expectedParentSubType)
			{
				AssertEquals("Country", expectedCountry, item.Country);
				AssertEquals("Child Sub Type", expectedChildSubType, item.ChildSubType);
				AssertEquals("Parent Sub Type", expectedParentSubType, item.ParentSubType);
			}
		}

		[TestDate(2023, 4, 23)]
		public void TestCustomComplianceSequenceValidation()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as IComplianceSequenceValidationProvider;
			AssertNotNull(complianceInfo);

			AssertComplianceSequenceDateRange(null, ZDate.Empty, ZDateTime.Empty);

			AssertComplianceSequenceDateRange(null, new ZDate(2023, 01, 01), new ZDate(2023, 04, 30));
			AssertComplianceSequenceDateRange(null, new ZDate(2023, 01, 01), new ZDate(2023, 12, 31));
			AssertComplianceSequenceDateRange(null, new ZDate(2023, 04, 01), new ZDate(2023, 12, 31));

			AssertComplianceSequenceDateRange("Valid date should be smaller than expire date", ZDate.Today, ZDateTime.Today.AddDays(-1));
			AssertComplianceSequenceDateRange("Valid From and Expiry Date must be in the same calendar year", ZDate.Today, ZDateTime.Today.AddMonths(13));
			AssertComplianceSequenceDateRange("Valid From and Expiry Date must be in the same calendar year", new ZDate(2023, 01, 01), new ZDate(2024, 01, 01));

			void AssertComplianceSequenceDateRange(string error, ZDate start, ZDateTime expiry)
			{
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_StartDate = start;
				sequence.XD_ExpiryDate = expiry;

				var validation = complianceInfo.GetAccComplianceSequenceValidation(sequence);
				validation.ValidateAll();

				if (error == null)
				{
					AssertNoErrors(sequence.XD_ExpiryDateInfo);
					AssertNoErrors(sequence.XD_StartDateInfo);
				}
				else
				{
					AssertHasErrorContaining(sequence.XD_ExpiryDateInfo, error);
					AssertHasErrorContaining(sequence.XD_StartDateInfo,error);
				}
			}
		}

		#region IComplianceSubTypeValidation

		public void TestErrorMessageForComplianceSubTypeValidation()
		{
			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(CountryCode);

			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.CreditNote;
			arInvoice.AH_TransactionNum = "ARCRD001";
			arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN;
			((INeedRow)arInvoice).Row.AcceptChanges();

			Assert("We expect empty error message if compliance sub type is not ICN", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice).IsEmpty);

			arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
			AssertEquals("Expected error message if ICN compliance sub type is selected", "You cannot select ICN compliance sub type for Amending Credit Notes.", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice));

			((INeedRow)arInvoice).Row.AcceptChanges();
			Assert("We expect empty error message if compliance sub type is not changed", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice).IsEmpty);

			arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.CIN;
			((INeedRow)arInvoice).Row.AcceptChanges();

			arInvoice.AH_IsCancelled = true;
			arInvoice.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
			Assert("We expect empty error message when transaction is reversal and even if compliance sub type being changed", complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(arInvoice).IsEmpty);
		}

		public void TestWarningMessageForComplianceSubTypeValidation()
		{
			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(CountryCode);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.OH_Category = OrgConstants.Category.Business;

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_OH = org.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_TransactionNum = "ARINV001";

			AssertEquals(ZString.Empty, complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(transaction));
		}

		#endregion

		public void TestIsOrganizationIsTaxRegistrationTypeRuleApplicable()
		{
			var ruleProvider = new TurkeyComplianceInfo() as IComplianceSubTypeTaxRegistrationTypeRuleProvider;
			var rule = new ComplianceSubTypeAttributionRuleConfiguration();
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.AllOrganizations;

			Assert("AllOrganizations must be true", ruleProvider.IsTaxRegistrationTypeRuleApplicable(rule, null));

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.RemoveAll();
			orgHeader.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, "1111111", CountryCode);
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.BasicEInvoiceRegisteredOrganizationTurkey;

			Assert("BasicEInvoiceRegisteredOrganizationTurkey with VTE OrgCusCode must be true", ruleProvider.IsTaxRegistrationTypeRuleApplicable(rule, null));

			orgHeader.CustomsCodes.RemoveAll();
			orgHeader.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTC, "1111111", CountryCode);
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.CommercialEInvoiceRegisteredOrganizationTurkey;

			Assert("CommercialEInvoiceRegisteredOrganizationTurkey with VTC OrgCusCode must be true", ruleProvider.IsTaxRegistrationTypeRuleApplicable(rule, null));

			rule.TaxRegistrationType = TaxRegistrationTypeCodes.EInvoiceRegisteredOrganizationTurkey;

			Assert("EInvoiceRegisteredOrganizationTurkey with VTC OrgCusCode must be true", ruleProvider.IsTaxRegistrationTypeRuleApplicable(rule, null));

			orgHeader.CustomsCodes.RemoveAll();
			orgHeader.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, "1111111", CountryCode);

			Assert("EInvoiceRegisteredOrganizationTurkey with VTE OrgCusCode must be true", ruleProvider.IsTaxRegistrationTypeRuleApplicable(rule, null));

			orgHeader.CustomsCodes.RemoveAll();
			rule.TaxRegistrationType = TaxRegistrationTypeCodes.eInvoiceNotRegisteredOrganizationTurkey;

			Assert("eInvoiceNotRegisteredOrganizationTurkey without VTE and VTC OrgCusCode must be true", ruleProvider.IsTaxRegistrationTypeRuleApplicable(rule, null));
		}

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
			=> new Dictionary<(string, bool), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), "'GVT' is not valid for country/region 'TR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true),  "'GVT' is not valid for country/region 'TR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), "'PRN' is not valid for country/region 'TR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true),  "'PRN' is not valid for country/region 'TR'." },
			};

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry()
			=> new Dictionary<(string, bool), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), "'GVT' is not valid for country/region 'TR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true),  "'GVT' is not valid for country/region 'TR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true),  null },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), "'PRN' is not valid for country/region 'TR'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true),  "'PRN' is not valid for country/region 'TR'." },
			};
	}
}
