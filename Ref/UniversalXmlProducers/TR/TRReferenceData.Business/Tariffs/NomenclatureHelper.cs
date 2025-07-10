using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public class NomenclatureHelper
	{
		public NomenclatureHelper()
		{
			var sections = new List<Section>()
			{
				new Section { Code = "01", Description = "CANLI HAYVANLAR VE HAYVANSAL ÜRÜNLER", Chapters = new string[] { "01", "02", "03", "04", "05" } },
				new Section { Code = "02", Description = "BİTKİSEL ÜRÜNLER", Chapters = new string[] { "06", "07", "08", "09", "10", "11", "12", "13", "14" } },
				new Section { Code = "03", Description = "HAYVANSAL VE BİTKİSEL KATI VE SIVI YAĞLAR VE BUNLARIN PARÇALANMA ÜRÜNLERİ; HAZIR YEMEKLİK KATI YAĞLAR; HAYVANSAL VE BİTKİSEL MUMLAR", Chapters = new string[] { "15" } },
				new Section { Code = "04", Description = "GIDA SANAYİİ MÜSTAHZARLARI; MEŞRUBAT, ALKOLLÜ İÇKİLER VE SİRKE; TÜTÜN VEYA TÜTÜN YERİNE GEÇEN İŞLENMİŞ MADDELER", Chapters = new string[] { "16", "17", "18", "19", "20", "21", "22", "23", "24" } },
				new Section { Code = "05", Description = "MİNERAL MADDELER", Chapters = new string[] { "25", "26", "27" } },
				new Section { Code = "06", Description = "KİMYA SANAYİİ VE BUNA BAĞLI SANAYİİ ÜRÜNLERİ", Chapters = new string[] { "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38" } },
				new Section { Code = "07", Description = "PLASTİKLER VE MAMULLERİ; KAUÇUK VE MAMULLERİ", Chapters = new string[] { "39", "40" } },
				new Section { Code = "08", Description = "HAM POSTLAR VE DERİLER, KÖSELELER, POSTLAR, KÜRKLER VE BU MADDELERDEN MAMUL EŞYA; SARACİYE EŞYASI VE EYER VE KOŞUM TAKIMLARI; SEYAHAT EŞYASI, EL ÇANTALARI VE BENZERİ MAHFAZALAR; HAYVAN BAĞIRSAĞINDAN MAMUL EŞYA (İPEK BÖCEĞİ BAĞIRSAĞI HARİÇ)", Chapters = new string[] { "41", "42", "43" } },
				new Section { Code = "09", Description = "AĞAÇ VE AHŞAP EŞYA; ODUN KÖMÜRÜ; MANTAR VE MANTARDAN MAMUL EŞYA; HASIRDAN, SAZDAN VEYA ÖRÜLMEYE ELVERİŞLİ DİĞER MADDELERDEN MAMULLER; SEPETÇİ VE HASIRCI EŞYASI", Chapters = new string[] { "44", "45", "46" } },
				new Section { Code = "10", Description = "ODUN VEYA DİĞER LİFLİ SELÜLOZİK MADDELERİN HAMURLARI VE GERİ KAZANILMIŞ KAĞIT VEYA KARTON (DÖKÜNTÜ, KIRPINTI VE HURDALAR); KAĞIT, KARTON VE MAMULLERİ", Chapters = new string[] { "47", "48", "49" } },
				new Section { Code = "11", Description = "DOKUMAYA ELVERİŞLİ MADDELER VE BUNLARDAN MAMUL EŞYA", Chapters = new string[] { "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63" } },
				new Section { Code = "12", Description = "AYAKKABILAR, BAŞLIKLAR, ŞEMSİYELER, GÜNEŞ ŞEMSİYELERİ, BASTONLAR, İSKEMLE BASTONLAR, KEMERLER, KIRBAÇLAR VE BUNLARIN AKSAMI; HAZIRLANMIŞ TÜYLER VE BUNLARDAN MAMUL EŞYA; YAPMA ÇİÇEKLER; İNSAN SAÇINDAN MAMUL EŞYA", Chapters = new string[] { "64", "65", "66", "67" } },
				new Section { Code = "13", Description = "TAŞ, ALÇI, ÇİMENTO, AMYANT, MİKA VEYA BENZERİ MADDELERDEN EŞYA; SERAMİK MAMULLERİ; CAM VE CAM EŞYA", Chapters = new string[] { "68", "69", "70" } },
				new Section { Code = "14", Description = "TABİİ VEYA KÜLTÜR İNCİLER, KIYMETLİ VEYA YARI KIYMETLİ TAŞLAR, KIYMETLİ METALLER, KIYMETLİ METALLERLE KAPLAMA METALLER VE BUNLARDAN MAMUL EŞYA; TAKLİT MÜCEVHERCİ EŞYASI; METAL PARALAR", Chapters = new string[] { "71" } },
				new Section { Code = "15", Description = "ADİ METALLER VE ADİ METALLERDEN EŞYA", Chapters = new string[] { "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83" } },
				new Section { Code = "16", Description = "MAKİNALAR VE MEKANİK CİHAZLAR; ELEKTRİK MALZEMELERİ; BUNLARIN AKSAM VE PARÇALARI; SES KAYDEDİCİLER VE KAYDEDİLEN SESİ TEKRAR VERMEYE MAHSUS CİHAZLAR, TELEVİZYON GÖRÜNTÜ VE SES KAYDEDİCİLERİ VE KAYDEDİLEN GÖRÜNTÜ VE SESİ TEKRAR VERMEYE MAHSUS CİHAZLAR; BUNLARIN AKSAM, PARÇA VE TEFERRUATI", Chapters = new string[] { "84", "85" } },
				new Section { Code = "17", Description = "NAKİL VASITALARI", Chapters = new string[] { "86", "87", "88", "89" } },
				new Section { Code = "18", Description = "OPTİK ALET VE CİHAZLAR, FOTOĞRAF, SİNEMA, ÖLÇÜ, KONTROL, AYAR ALET VE CİHAZLARI, TIBBİ VEYA CERRAHİ ALET VE CİHAZLAR; SAATÇİ EŞYASI; MÜZİK ALETLERİ; BUNLARIN AKSAM, PARÇA VE AKSESUARI", Chapters = new string[] { "90", "91", "92" } },
				new Section { Code = "19", Description = "SİLAHLAR VE MÜHİMMAT; BUNLARIN AKSAM, PARÇA VE AKSESUARI", Chapters = new string[] { "93" } },
				new Section { Code = "20", Description = "MUHTELİF MAMUL EŞYA", Chapters = new string[] { "94", "95", "96" } },
				new Section { Code = "21", Description = "SANAT ESERLERİ, KOLLEKSİYON EŞYASI VE ANTİKALAR", Chapters = new string[] { "97", "98", "99" } },
			};
			Sections = sections;

			var chapterToSection = new Dictionary<string, string>();
			foreach (var section in sections)
			{
				foreach (var chapter in section.Chapters)
				{
					chapterToSection.Add(chapter, section.Code);
				}
			}
			ChapterToSection = chapterToSection;

			var chapterInfo = new Dictionary<string, string>();
			chapterInfo.Add("01", "Canlı hayvanlar");
			chapterInfo.Add("02", "Etler ve yenilen sakatat");
			chapterInfo.Add("03", "Balıklar, kabuklu hayvanlar, yumuşakçalar ve suda yaşayan diğer omurgasız hayvanlar");
			chapterInfo.Add("04", "Süt ürünleri; kuş ve kümes hayvanlarının yumurtaları; tabii bal; tarifenin başka yerinde belirtilmeyen veya yer almayan yenilebilir hayvansal menşeli ürünler");
			chapterInfo.Add("05", "Tarifenin başka yerinde belirtilmeyen veya yer almayan hayvansal menşeli ürünler");
			chapterInfo.Add("06", "Canlı ağaçlar ve diğer bitkiler; yumrular, kökler ve benzerleri; kesme çiçekler ve süs yaprakları");
			chapterInfo.Add("07", "Yenilen sebzeler ve bazı kök ve yumrular");
			chapterInfo.Add("08", "Yenilen meyvalar ve yenilen sert kabuklu meyvalar; turunçgillerin ve kavunların ve karpuzların kabukları");
			chapterInfo.Add("09", "Kahve, çay, paraguay çayı ve baharat.");
			chapterInfo.Add("10", "Hububat");
			chapterInfo.Add("11", "Değirmencilik ürünleri; malt; nişasta; inülin; buğday gluteni");
			chapterInfo.Add("12", "Yağlı tohum ve meyvalar; muhtelif tane, tohum ve meyvalar; sanayiide ve tıpta kullanılan bitkiler; saman ve kaba yem");
			chapterInfo.Add("13", "Lak; sakız, reçine ve diğer bitkisel özsu ve hülasalar");
			chapterInfo.Add("14", "Örülmeye elverişli bitkisel maddeler; tarifenin başka yerinde belirtilmeyen veya yer almayan bitkisel ürünler");
			chapterInfo.Add("15", "Hayvansal ve bitkisel katı ve sıvı yağlar ve bunların parçalanma ürünleri; hazır yemeklik katı yağlar; hayvansal ve bitkisel mumlar");
			chapterInfo.Add("16", "Et, balık, kabuklu hayvanlar, yumuşakçalar veya diğer su omurgasızlarının müstahzarları");
			chapterInfo.Add("17", "Şeker ve şeker mamulleri");
			chapterInfo.Add("18", "Kakao ve kakao müstahzarları");
			chapterInfo.Add("19", "Hububat, un, nişasta veya süt müstahzarları; pastacılık ürünleri");
			chapterInfo.Add("20", "Sebzeler, meyvalar, sert kabuklu meyvalar ve bitkilerin diğer kısımlarından elde edilen müstahzarlar");
			chapterInfo.Add("21", "Yenilen çeşitli gıda müstahzarları");
			chapterInfo.Add("22", "Meşrubat, alkollü içkiler ve sirke");
			chapterInfo.Add("23", "Gıda sanayiinin kalıntı ve döküntüleri; hayvanlar için hazırlanmış kaba yemler");
			chapterInfo.Add("24", "Tütün ve tütün yerine geçen işlenmiş maddeler");
			chapterInfo.Add("25", "Tuz; kükürt; topraklar ve taşlar; alçılar, kireçler ve çimento");
			chapterInfo.Add("26", "Metal cevherleri, cüruf ve kül");
			chapterInfo.Add("27", "Mineral yakıtlar, mineral yağlar ve bunların damıtılmasından elde edilen ürünler; bitümenli maddeler; mineral mumlar");
			chapterInfo.Add("28", "Anorganik kimyasallar; kıymetli metallerin, radyoaktif elementlerin; nadir toprak metallerinin ve izotoplarının organik veya anorganik bileşikleri");
			chapterInfo.Add("29", "Organik kimyasal ürünler");
			chapterInfo.Add("30", "Eczacılık ürünleri");
			chapterInfo.Add("31", "Gübreler");
			chapterInfo.Add("32", "Debagatte ve boyacılıkta kullanılan hülasalar; tanenler ve türevleri; boyalar, pigmentler ve diğer boyayıcı maddeler; müstahzar boyalar ve vernikler; macunlar; mürekkepler");
			chapterInfo.Add("33", "Uçucu yağlar ve rezinoitler; parfümeri, kozmetik veya tuvalet müstahzarları");
			chapterInfo.Add("34", "Sabunlar, yüzey-aktif organik maddeler, yıkama müstahzarları, yağlama müstahzarları, suni mumlar, müstahzar mumlar, temizleme veya bakım müstahzarları, ışık temini için kullanılan her türlü mumlar ve benzerleri, model yapmaya mahsus her türlü patlar, \"dişçi mumları\" ve alçı esaslı dişçilik müstahzarları");
			chapterInfo.Add("35", "Albüminoid maddeler; değişikliğe uğramış nişasta esaslı ürünler; tutkallar; enzimler");
			chapterInfo.Add("36", "Barut ve patlayıcı maddeler; pirotekni mamulleri; kibritler; piroforik alaşımlar; ateş alıcı maddeler");
			chapterInfo.Add("37", "Fotoğrafçılıkta veya sinemacılıkta kullanılan eşya");
			chapterInfo.Add("38", "Muhtelif kimyasal maddeler");
			chapterInfo.Add("39", "Plastikler ve mamulleri");
			chapterInfo.Add("40", "Kauçuk ve kauçuktan eşya");
			chapterInfo.Add("41", "Ham postlar, deriler (kürkler hariç) ve köseleler");
			chapterInfo.Add("42", "Deri eşya; saraciye eşyası ve eyer ve koşum takımları; seyahat eşyası, el çantaları ve benzeri mahfazalar; hayvan bağırsağından mamul eşya (ipek böceği bağırsağı hariç)");
			chapterInfo.Add("43", "Kürkler ve taklit kürkler; bunların mamulleri");
			chapterInfo.Add("44", "Ağaç ve ahşap eşya; odun kömürü");
			chapterInfo.Add("45", "Mantar ve mantardan eşya");
			chapterInfo.Add("46", "Hasırdan, sazdan veya örülmeye elverişli diğer maddelerden mamuller; sepetçi ve hasırcı eşyası");
			chapterInfo.Add("47", "Odun veya diğer lifli selülozik maddelerin hamurları; geri kazanılmış kağıt veya karton (döküntü, kırpıntı ve hurdalar)");
			chapterInfo.Add("48", "Kağıt ve karton; kağıt hamurundan, kağıttan veya kartondan eşya");
			chapterInfo.Add("49", "Basılı kitaplar, gazeteler, resimler ve baskı sanayiinin diğer mamulleri; el ve makina yazısı metinler ve planlar");
			chapterInfo.Add("50", "İpek");
			chapterInfo.Add("51", "Yapağı ve yün, ince veya kaba hayvan kılı; at kılından iplik ve dokunmuş mensucat");
			chapterInfo.Add("52", "Pamuk");
			chapterInfo.Add("53", "Dokumaya elverişli diğer bitkisel lifler; kağıt ipliği ve kağıt ipliğinden");
			chapterInfo.Add("54", "Sentetik ve suni filamentler, şeritler ve benzeri sentetik ve suni dokumaya elverişli maddeler.......");
			chapterInfo.Add("55", "Sentetik ve suni devamsız lifler");
			chapterInfo.Add("56", "Vatka, keçe ve dokunmamış mensucat; özel iplikler; sicim, kordon, ip, halat ve bunlardan mamul eşya");
			chapterInfo.Add("57", "Halılar ve diğer dokumaya elverişli maddelerden yer kaplamaları");
			chapterInfo.Add("58", "Özel dokunmuş mensucat; tufte edilmiş dokumaya elverişli mensucat; dantela; duvar halıları; şeritçi ve kaytancı eşyası; işlemeler");
			chapterInfo.Add("59", "Emdirilmiş, sıvanmış, kaplanmış veya lamine edilmiş dokumaya elverişli mensucat; dokumaya elverişli maddelerden teknik eşya");
			chapterInfo.Add("60", "Örme veya kroşe eşya");
			chapterInfo.Add("61", "Örme veya kroşe giyim eşyası ve aksesuarı");
			chapterInfo.Add("62", "Örülmemiş veya kroşe olmayan giyim eşyası ve aksesuarı");
			chapterInfo.Add("63", "Dokumaya elverişli maddelerden diğer hazır eşya; takımlar; kullanılmış giyim eşyası ve dokumaya elverişli maddelerden kullanılmış eşya; paçavralar");
			chapterInfo.Add("64", "Ayakkabılar, getrler, tozluklar ve benzeri eşya; bunların aksamı");
			chapterInfo.Add("65", "Başlıklar ve aksamı");
			chapterInfo.Add("66", "Şemsiyeler, güneş şemsiyeleri, bastonlar, iskemle bastonlar, kamçılar, kırbaçlar ve bunların aksamı");
			chapterInfo.Add("67", "Hazırlanmış ince ve kalın kuş tüyleri ve bunlardan eşya; yapma çiçekler; insan saçından eşya");
			chapterInfo.Add("68", "Taş, alçı, çimento, amyant, mika veya benzeri maddelerden eşya");
			chapterInfo.Add("69", "Seramik mamulleri");
			chapterInfo.Add("70", "Cam ve cam eşya");
			chapterInfo.Add("71", "Tabii veya kültür inciler, kıymetli veya yarı kıymetli taşlar, kıymetli metaller, kıymetli metallerle kaplama metaller ve bunlardan mamul eşya; taklit mücevherci eşyası; metal paralar");
			chapterInfo.Add("72", "Demir ve çelik");
			chapterInfo.Add("73", "Demir veya çelikten eşya");
			chapterInfo.Add("74", "Bakır ve bakırdan eşya");
			chapterInfo.Add("75", "Nikel ve nikelden eşya");
			chapterInfo.Add("76", "Aluminyum ve aluminyumdan eşya");
			chapterInfo.Add("77", "(Armonize Sistem Nomanklatürü'nde ileride kullanılmak amacıyla saklı tutulmuştur.)");
			chapterInfo.Add("78", "Kurşun ve kurşundan eşya");
			chapterInfo.Add("79", "Çinko ve çinkodan eşya");
			chapterInfo.Add("80", "Kalay ve kalaydan eşya");
			chapterInfo.Add("81", "Diğer adi metaller; sermetler; bunlardan eşya");
			chapterInfo.Add("82", "Adi metallerden aletler, bıçakcı eşyası ve sofra takımları; adi metallerden bunların aksam ve parçaları");
			chapterInfo.Add("83", "Adi metallerden çeşitli eşya");
			chapterInfo.Add("84", "Nükleer reaktörler, kazanlar, makinalar, mekanik cihazlar ve aletler; bunların aksam ve parçaları");
			chapterInfo.Add("85", "Elektrikli makina ve cihazlar ve bunların aksam ve parçaları; ses kaydetmeye ve kaydedilen sesi tekrar vermeye mahsus cihazlar; televizyon görüntü ve seslerinin kaydedilmesine ve kaydedilen görüntü ve sesin tekrar verilmesine mahsus cihazlar ve bunların aksam, parça ve aksesuarı");
			chapterInfo.Add("86", "Demiryolu ve benzeri hatlara ait taşıtlar ve malzemeler ve bunların aksam ve parçaları; her türlü mekanik (elektro mekanik olanlar dahil) trafik sinyalizasyon cihazları");
			chapterInfo.Add("87", "Motorlu kara taşıtları, traktörler, bisikletler, motosikletler ve diğer kara taşıtları; bunların aksam, parça ve aksesuarı");
			chapterInfo.Add("88", "Hava taşıtları, uzay taşıtları ve bunların aksam ve parçalar");
			chapterInfo.Add("89", "Gemiler ve suda yüzen taşıt ve araçlar");
			chapterInfo.Add("90", "Optik alet ve cihazlar, fotoğraf, sinema, ölçü, kontrol, ayar alet ve cihazları, tıbbi veya cerrahi alet ve cihazlar; bunların aksam, parça ve aksesuarı");
			chapterInfo.Add("91", "Saatler ve bunların aksam ve parçaları");
			chapterInfo.Add("92", "Müzik aletleri; bunların aksam, parça ve aksesuarı");
			chapterInfo.Add("93", "Silahlar ve mühimmat; bunların aksam, parça ve aksesuarı");
			chapterInfo.Add("94", "Mobilyalar, tıpta veya cerrahide kullanılan mobilyalar, yatak takımları ve benzeri doldurulmuş eşya; tarifenin başka yerinde belirtilmeyen veya yer almayan aydınlatma cihazları; reklam lambaları, ışıklı tabelalar, ışıklı isim plakaları ve benzerleri; prefabrik yapılar");
			chapterInfo.Add("95", "Oyuncaklar, oyun ve spor malzemeleri; bunların aksam, parça ve aksesuarı");
			chapterInfo.Add("96", "Çeşitli mamul eşya");
			chapterInfo.Add("97", "Sanat eserleri, kolleksiyon eşyası ve antikalar");
			chapterInfo.Add("98", "Akit taraflarca, özel amaçlarda kullanılmak üzere saklı tutulmuştur.");
			chapterInfo.Add("99", "Özel amaçlı gümrük tarife istatistik pozisyonları");
			ChapterInfo = chapterInfo;

			ChapterCodes = chapterInfo.Keys;
		}

		public IEnumerable<Section> Sections { get; }

		IDictionary<string, string> ChapterToSection { get; }

		public string GetSection(string chapter)
		{
			ChapterToSection.TryGetValue(chapter, out var section);
			return section;
		}

		IDictionary<string, string> ChapterInfo { get; }

		public IEnumerable<string> ChapterCodes { get; }

		public string GetChapterTitle(string code)
		{
			ChapterInfo.TryGetValue(code, out var title);
			return title;
		}
	}
}
