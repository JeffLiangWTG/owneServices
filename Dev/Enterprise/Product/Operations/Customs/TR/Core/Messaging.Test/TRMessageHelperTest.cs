using System;
using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Messaging.Testing
{
	public class TRMessageHelperTest : TestCase
	{
		public void TestGetMultipleNodeValueListForQueryRemainingBills()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryRemainingBillsForImport.QueryRemainingBillsForImportSuccess.xml");
			var nodeValueList = TRMessageHelper.GetMultipleNodeValueListForQueryRemainingBills(messageText);
			AssertEquals(1, nodeValueList.Count);
			AssertEquals(new Tuple<string, string>("27", "Taşıma Senedi ayırma"), nodeValueList[0]);
		}

		public void TestGetChildNodesFromElement()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionLine.QueryForInspectionLineSuccess.xml");
			var nodeValueList = TRMessageHelper.GetChildNodesFromElement(messageText, "Table1", "TASIMASENEDINO", "HAT", "ONAYDURUMU");
			AssertEquals(3, nodeValueList.Count);
			AssertArrayEqualsByElements(new string[] { "6491183", "SARI", "ONAYLANMIŞ" }, nodeValueList[0]);
		}

		public void TestGetNodeXml()
		{
			const string invalidXmlText1 = "<xml><a></xml>";
			const string invalidXmlText2 = "\u0001\u0003";
			const string validXmlText = @"<CommonCustomsServiceError>
	<ErrorType>Timeout</ErrorType>
	<ErrorDescription>Could not get response within timeout.</ErrorDescription>
	<ErrorDetail>
		<SourceParty>WTLDTRABC</SourceParty>
		<DestinationParty>TROCustomsTest</DestinationParty>
	</ErrorDetail>
	<InboxPK>07F831A9-131C-4BFC-A467-5B26AD8DDFF1</InboxPK>
	<OutboxPK>BB7B9E92-1674-45AA-A074-A5BD6C3D5C0C</OutboxPK>
	<MessageTrackingID>9DB081D9-AB15-4119-848A-BB8C8D733A49</MessageTrackingID>
</CommonCustomsServiceError>";

			var result = TRMessageHelper.GetNodeXml(validXmlText, "CommonCustomsServiceError/ErrorDetail");

			AssertEquals("", TRMessageHelper.GetNodeXml(invalidXmlText1, "xml/a"));
			AssertEquals("", TRMessageHelper.GetNodeXml(invalidXmlText2, ""));
			AssertEquals("<ErrorDetail><SourceParty>WTLDTRABC</SourceParty><DestinationParty>TROCustomsTest</DestinationParty></ErrorDetail>", result);
		}

		public void TestGetNodeValues()
		{
			var text = TRMessageTestHelper.GetFileText("NCTS.GetMessagesListByGuidResponse.xml");
			var listValues = TRMessageHelper.GetNodeValues(text, new List<ZString>() { "Envelope", "Body", "getMessagesListByGuidResponse", "return" }, "list");
			AssertContainsExactElementsInExactOrder(new string[] { "39906267", "39906263", "39916956", "39903172", "39906268", "39916957" }, listValues);
		}

		public void TestGetXmlNode()
		{
			var text = TRMessageTestHelper.GetFileText("CC060A.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.DownloadMessageByIndexResponse.");
			var node = TRMessageHelper.GetXmlNode(text, new ZString[] { "Envelope", "Body", "downloadmessagebyindexResponse", "return", "msgContent" });
			AssertEquals("CC060A", node.FirstChild.Name);
		}

		public void TestGetMultipleNodeValuesList()
		{
			var messageText = @"<?xml version=""1.0"" encoding=""ISO-8859-1"" ?>
<sonuc xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://tempuri.org/"">
	<Tip>Kontrol</Tip>
	<Tescil_tarihi/>
	<Beyanname_no/>
	<Hatalar>
		<Hata>
			<Hata_kodu>1.Kalem</Hata_kodu>
			<Hata_aciklamasi>Kalem 1 Desc</Hata_aciklamasi>
		</Hata>
		<Hata>
			<Hata_kodu>2.Kalem</Hata_kodu>
			<Hata_aciklamasi>Kalem 2 Desc</Hata_aciklamasi>
		</Hata>
		<Hata>
			<Hata_kodu>3.Kalem</Hata_kodu>
			<Hata_aciklamasi>Kalem 3 Desc</Hata_aciklamasi>
		</Hata>
		<Hata>
			<Hata_kodu>4.Kalem</Hata_kodu>
			<Hata_aciklamasi>Kalem 4 Desc</Hata_aciklamasi>
		</Hata>
	</Hatalar>
</sonuc>";

			var nodeValueList = TRMessageHelper.GetMultipleNodeValueList(messageText, new List<ZString>() { "sonuc", "Hatalar", "Hata" }, ("Hata_kodu", "Hata_aciklamasi"));
			CombineAssertions(() =>
			{
				AssertEquals(4, nodeValueList.Count);
				AssertEquals(new Tuple<string, string>("1.Kalem", "Kalem 1 Desc"), nodeValueList[0]);
				AssertEquals(new Tuple<string, string>("2.Kalem", "Kalem 2 Desc"), nodeValueList[1]);
				AssertEquals(new Tuple<string, string>("3.Kalem", "Kalem 3 Desc"), nodeValueList[2]);
				AssertEquals(new Tuple<string, string>("4.Kalem", "Kalem 4 Desc"), nodeValueList[3]);
			});
		}

		public void TestGenerateXml()
		{
			var expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <RefID>ETR0000001</RefID>
  <KullaniciAdi>12345678901</KullaniciAdi>
  <Sifre>25d55ad283aa400af464c76d713c07ad</Sifre>
  <RequestMessage>
    <RequestMessage>
      <q1:tamamlayiciBeyan xmlns:q1=""http://tempuri.org/"">
        <q1:beyanSahibiTemsilci>
          <q1:adiUnvani>WiseTech Global</q1:adiUnvani>
          <q1:vergiTCNo>12453687521</q1:vergiTCNo>
        </q1:beyanSahibiTemsilci>
        <q1:beyannameNo>testRegNoforTest</q1:beyannameNo>
        <q1:tasimaSenetleri>
          <q1:tasimaSenediTamamlayiciBilgi>
            <q1:tasimaSenediNo>1</q1:tasimaSenediNo>
            <q1:aliciVergiTCNo>20201224104</q1:aliciVergiTCNo>
            <q1:teslimTarihi>2020-12-31T00:00:00+11:00</q1:teslimTarihi>
          </q1:tasimaSenediTamamlayiciBilgi>
        </q1:tasimaSenetleri>
      </q1:tamamlayiciBeyan>
    </RequestMessage>
  </RequestMessage>
</Root>";
			var xmlContent = @"<RequestMessage>
    <q1:tamamlayiciBeyan xmlns:q1=""http://tempuri.org/"">
      <q1:beyanSahibiTemsilci>
        <q1:adiUnvani>WiseTech Global</q1:adiUnvani>
        <q1:vergiTCNo>12453687521</q1:vergiTCNo>
      </q1:beyanSahibiTemsilci>
      <q1:beyannameNo>testRegNoforTest</q1:beyannameNo>
      <q1:tasimaSenetleri>
        <q1:tasimaSenediTamamlayiciBilgi>
          <q1:tasimaSenediNo>1</q1:tasimaSenediNo>
          <q1:aliciVergiTCNo>20201224104</q1:aliciVergiTCNo>
          <q1:teslimTarihi>2020-12-31T00:00:00+11:00</q1:teslimTarihi>
        </q1:tasimaSenediTamamlayiciBilgi>
      </q1:tasimaSenetleri>
    </q1:tamamlayiciBeyan>
  </RequestMessage>";

			var generatedXml = TRMessageHelper.GenerateXml("ETR0000001", "12345678901", "25d55ad283aa400af464c76d713c07ad", xmlContent);
			AssertEquals(expectedXml, generatedXml);
		}

		public void TestRemoveCountryCodePrefix()
		{
			CombineAssertions(() =>
			{
				AssertEquals("161600", TRMessageHelper.RemoveCountryCodePrefix("TR161600"));
				AssertEquals("ES161600", TRMessageHelper.RemoveCountryCodePrefix("ES161600"));
				AssertEquals(string.Empty, TRMessageHelper.RemoveCountryCodePrefix(string.Empty));
				AssertEquals("ABC", TRMessageHelper.RemoveCountryCodePrefix("ABC"));
			});
		}

		public void TestGetAlreadyRegisteredDischargeListNo()
		{
			var xmlData = "23066666IM000004 beyanname numarası için daha önceden 23066666BL000003 numarası ile boşaltma listesi tescil edilmiştir!";

			AssertEquals("23066666BL000003", TRMessageHelper.GetAlreadyRegisteredDischargeListNo(xmlData));

			xmlData = "23066666IM000004 bxeyanname numarası için daha önceden 23066666BL000003 numarası ile boşaltma listesi tescil edilmiştir!";

			AssertEquals(ZString.Empty, TRMessageHelper.GetAlreadyRegisteredDischargeListNo(xmlData));
		}

		public void TestDecodeHtmlIfNecessary()
		{
			string input = "<msgContent>&lt;CC015B_RES>&lt;GUID>1A389AD1E7696349E0636903A8C0F5D8&lt;/GUID>&lt;ERR>&lt;PROC>validatecc015b_tr validation failed&lt;/PROC>&lt;VAL>&lt;VAL_MAIN>Çıkış (Sınır) Gümrük İdaresi sınır gümrüğü ya da RORO gümrüğü seçilebilir.(TR066666)&lt;/VAL_MAIN>&lt;VAL_MAIN>NTR_C186_08----Baslik bölümündeki güvenlik kullanilmadiysa (Gümrük alt birimi) kullanilamaz, kullanildiysa veri grubu varsayilan olarak 0 dir&lt;/VAL_MAIN>&lt;/VAL>&lt;/ERR>&lt;/CC015B_RES></msgContent>";
			string expected = "<msgContent><CC015B_RES><GUID>1A389AD1E7696349E0636903A8C0F5D8</GUID><ERR><PROC>validatecc015b_tr validation failed</PROC><VAL><VAL_MAIN>Çıkış (Sınır) Gümrük İdaresi sınır gümrüğü ya da RORO gümrüğü seçilebilir.(TR066666)</VAL_MAIN><VAL_MAIN>NTR_C186_08----Baslik bölümündeki güvenlik kullanilmadiysa (Gümrük alt birimi) kullanilamaz, kullanildiysa veri grubu varsayilan olarak 0 dir</VAL_MAIN></VAL></ERR></CC015B_RES></msgContent>";

			AssertEquals(expected, TRMessageHelper.DecodeHtmlIfNecessary(input));
		}
	}
}
