using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRBaseMessage))]
	class TRBaseMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<TRBaseMessage>();

			CombineAssertions("Default value of TR Base Message", () =>
			{
				AssertEquals(ZString.Empty, message.EM_ApplicationCode);
				Assert(!message.NeedToSignMessage);
			});
		}

		public void TestEM_FormattedMessageTextReturnEM_MessageInterpretation()
		{
			var message = Factory.New<TRBaseMessage>();
			var messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Gelen xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://LoginKontrol.YeniOzetBeyanGelen"">
<RefID xmlns="""">ULU-MAN0000199|1</RefID>
<KullaniciAdi xmlns="""">1</KullaniciAdi>
<Sifre xmlns="""">e10adc3949ba59abbe56e057f20f883e</Sifre>
<OzetBeyanBilgisi xmlns=""http://www.gumruk.gov.tr/"">
  <BeyanTuru>HAVİTH</BeyanTuru>
  <EkBelgeSayisi>0</EkBelgeSayisi>
  <GumrukIdaresi>067777</GumrukIdaresi>
  <KullaniciKodu>1</KullaniciKodu>
  <Kurye>HAYIR</Kurye>
  <LimanYerAdiBos>TRIST</LimanYerAdiBos>
  <LimanYerAdiYuk>DEHAM</LimanYerAdiYuk>
  <Rejim>I</Rejim>
  <TasimaSekli>40</TasimaSekli>
  <TasimaSenetleri />
  <OzbyAcmalar />
  <TasitinUgradigiUlkeler />
  <TasiyiciFirma />
  <TasiyiciVergiNo />
  <UlkeKoduBos>052</UlkeKoduBos>
  <UlkeKoduYuk>004</UlkeKoduYuk>
  <VarisTarihSaati>0001-01-01T00:00:00</VarisTarihSaati>
  <XmlRefId>ULU-MAN0000199</XmlRefId>
</OzetBeyanBilgisi>
</Gelen>";
			message.EM_MessageInterpretation = messageText;
			AssertEquals(messageText, message.EM_FormattedMessageText);
		}

		public void TestEM_MessageInterpretationDoNotReturnEM_MessageText()
		{
			var message = Factory.New<TRBaseMessage>();
			message.EM_MessageText = "Poop";
			AssertEquals("", message.EM_MessageInterpretation);

			message.EM_MessageText = "<?xml version='1.0'?><UniversalShipment>I am any XML but not a fragment<ChildNode>Baby</ChildNode></UniversalShipment>";
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			AssertEquals("", message.EM_MessageInterpretation);
		}

		public void TestMessageSubTypeList()
		{
			var message = Factory.New<TRBaseMessageForTest>();
			var list = message.MessageSubTypeListExpose;

			Assert("Should contains the Normal code.", list.ContainsCode(MessageSubTypeList.Codes.Normal));
			Assert("Should contains the normal Error.", list.ContainsCode(MessageSubTypeList.Codes.Error));

			AssertSame("Should cache the list.", list, message.MessageSubTypeListExpose);
		}

		sealed class TRBaseMessageForTest : TRBaseMessage
		{
			public TRBaseMessageForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public CodeDescriptionPairList MessageSubTypeListExpose => base.MessageSubTypeList;
		}
	}
}
