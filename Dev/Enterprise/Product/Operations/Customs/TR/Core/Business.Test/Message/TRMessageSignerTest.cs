using System;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRMessageSignerTest : TestCaseWithFactory
	{
		public void TestEncodeMessageWithUTF8()
		{
			var message = "<BeyanTuru>HAVİTH</BeyanTuru>";
			var expected = Encoding.UTF8.GetBytes(message);
			var actual = TRMessageSigner.Encode(message);
			AssertEquals(expected, actual);
		}

		public void TestSignMessageWithoutChipset()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var glbPassword = CreateExternalPassword(staff, "", "02b9572b9cad7250a906b3");
				AssertExceptionThrown<CryptographicException>("Exception with Chipset not specified", "Chipset not specified", () => TRMessageSigner.New().SignXMLMessage(message, glbPassword, "123456"));

				glbPassword.TR_Chipset = "EKART";
				try
				{
					TRMessageSigner.New().SignXMLMessage(message, glbPassword, "123456");
				}
				catch (Exception ex)
				{
					Assert(!ex.Message.Contains("Chipset not specified"));
				}
			}
		}

		public void TestSignMessageWithWrongCertificateSerialNumber()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var glbPassword = CreateExternalPassword(staff, "EKART", "");
				AssertExceptionThrown<InvalidOperationException>("Certificate serial number is empty", () => TRMessageSigner.New().SignXMLMessage(message, glbPassword, "123456"));

				glbPassword.GP_CertificateSerialNumber = "abc";
				AssertExceptionThrown<InvalidOperationException>("Certificate serial number is invalid", "Invalid certificate serial number: 'abc'.", () => TRMessageSigner.New().SignXMLMessage(message, glbPassword, "123456"));

				glbPassword.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
				try
				{
					TRMessageSigner.New().SignXMLMessage(message, glbPassword, "123456");
				}
				catch (Exception ex)
				{
					Assert(!ex.Message.Contains("Invalid certificate serial number"));
				}
			}
		}

		public void TestSignXmlMessagePkcs11Crypto()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var glbPassword = CreateExternalPassword(staff, "EKART", "02b9572b9cad7250a906b3");
				var zBlob = TRMessageSignerForTest.New().SignXMLMessage(message, glbPassword, "123456");

				AssertEquals("Signed message", "Signed with: Pkcs11CryptoApiSignatureAlgorithm\nMessage: <Body><Test>MessageToSign</Test></Body>", zBlob.ToUTF8());
			}
		}

		public void TestSignXmlMessageX509Certificate()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var glbPassword = CreateExternalPassword(staff, "WINDOWS", "02b9572b9cad7250a906b3");
				var zBlob = TRMessageSignerForTest.New().SignXMLMessage(message, glbPassword, "123456");

				AssertEquals("Signed message", "Signed with: X509Certificate2CryptoApiSignatureAlgorithm\nMessage: <Body><Test>MessageToSign</Test></Body>", zBlob.ToUTF8());
			}
		}

		public void TestSignEdiMessage()
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var glbPassword = CreateExternalPassword(staff, "EKART", "02b9572b9cad7250a906b3");
				var ediMessage = Factory.New<EDIMessage>();
				ediMessage.EM_MessageText = message;

				TRMessageSignerForTest.New().SignEdiMessage(ediMessage, glbPassword, "123456");
				AssertEquals("TSN", ediMessage.EM_MessageOwner);
				AssertEquals("EM_MessageData", "Signed with: Pkcs11CryptoApiSignatureAlgorithm\nMessage: <Body><Test>MessageToSign</Test></Body>", ediMessage.EM_MessageData.ToUTF8());
			}
		}

		public void TestProcessSignedXMLMessageByDefault()
		{
			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("Common.SampleSignedMessage.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(ZString.Empty, signedMessageBytes);
			var expectedMessageString = Convert.ToBase64String(signedMessageBytes);

			AssertEquals(expectedMessageString, Convert.ToBase64String(processedMessageBytes));
		}

		public void TestProcessSignedXMLMessageForSPTS()
		{
			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("SPTS.SampleSignedMessageforTSP.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.TSP, signedMessageBytes);
			var expectedMessageString = $@"<soapenv:Envelope xmlns:any=""http://schemas.microsoft.com/BizTalk/2003/Any"" xmlns:gum=""http://Gumruk.BizTalk.Integration"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <gum:Aktarma>
      <any:Root>
        <mesaj>{Convert.ToBase64String(signedMessageBytes)}</mesaj>
      </any:Root>
    </gum:Aktarma>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestProcessSignedXMLMessageForManifest()
		{
			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("Common.SampleSignedMessage.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.TRO, signedMessageBytes);
			var expectedMessageString = $@"<soapenv:Envelope xmlns:any=""http://schemas.microsoft.com/BizTalk/2003/Any"" xmlns:gum=""http://Gumruk.BizTalk.Integration"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <gum:OzetBeyan>
      <any:Root>
        <mesaj>{Convert.ToBase64String(signedMessageBytes)}</mesaj>
      </any:Root>
    </gum:OzetBeyan>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestProcessSignedXMLMessageForETradeTRE()
		{
			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("ETrade.SampleMessageforTRE.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.TRE, signedMessageBytes);
			var expectedMessageString = $@"<soapenv:Envelope xmlns:GumETrade=""http://GumrukETApp.RequestSignedMessage"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <GumETrade:Root>
      <GumETrade:RequestMessage>{Convert.ToBase64String(signedMessageBytes)}</GumETrade:RequestMessage>
    </GumETrade:Root>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestProcessSignedXMLMessageForETradeTRD()
		{
			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("ETrade.ImportDischargeList.RequestImportDischargeList.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.TRD, signedMessageBytes);
			var expectedMessageString = $@"<soapenv:Envelope xmlns:GumETrade=""http://GumrukETApp.RequestSignedMessage"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <GumETrade:Root>
      <GumETrade:RequestMessage>{Convert.ToBase64String(signedMessageBytes)}</GumETrade:RequestMessage>
    </GumETrade:Root>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestProcessSignedXMLMessageForNCTS()
		{
			var glbPassword = CreateExternalPassword(staff, "", "02b9572b9cad7250a906b3");

			string otherParametersForNcts = "ulu1987appRef" + "," + glbPassword.GP_UserID + "," + TRManifestMessageBuilderHelper.MD5Hash(glbPassword.CurrentDecryptedPassword);

			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("Common.SampleSignedMessage.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.TRN, signedMessageBytes, otherParametersForNcts);
			var expectedMessageString = $@"<soapenv:Envelope xmlns:ws=""http://ws/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <ws:submitdeclaration>
      <FIRM_ID>ULUKOM</FIRM_ID>
      <USER_ID>{otherParametersForNcts}</USER_ID>
      <MSG_TYPE>CC015B</MSG_TYPE>
      <SIGN_FLAG>1</SIGN_FLAG>
      <MSG_CONTENT>{Convert.ToBase64String(signedMessageBytes)}</MSG_CONTENT>
    </ws:submitdeclaration>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestProcessWithoutSignedXMLMessageForNCTS()
		{
			TRCustomsDataRegistry.Instance.SendTRNCTSMessageWithoutSign.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var messageText = TRMessageTestHelper.GetFileText("NCTS.202300000242-15.xml");
			var glbPassword = CreateExternalPassword(staff, "", "02b9572b9cad7250a906b3");

			string otherParametersForNcts = "ulu1987appRef" + "," + glbPassword.GP_UserID + "," + TRManifestMessageBuilderHelper.MD5Hash(glbPassword.CurrentDecryptedPassword) + "," + messageText;

			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("Common.SampleSignedMessage.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.TRN, signedMessageBytes, otherParametersForNcts);
			var expectedMessageString = "<soapenv:Envelope xmlns:ws=\"http://ws/\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\">\r\n  <soapenv:Header />\r\n  <soapenv:Body>\r\n    <ws:submitdeclaration>\r\n      <FIRM_ID>ULUKOM</FIRM_ID>\r\n      <USER_ID>ulu1987appRef,1234,f561aaf6ef0bf14d4208bb46a4ccb3ad</USER_ID>\r\n      <MSG_TYPE>CC015B</MSG_TYPE>\r\n      <SIGN_FLAG>0</SIGN_FLAG>\r\n      <MSG_CONTENT><![CDATA[<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CC015B>\r\n  <SynIdeMES1>UNOC</SynIdeMES1>\r\n  <SynVerNumMES2>3</SynVerNumMES2>\r\n  <MesSenMES3>NTA.TR</MesSenMES3>\r\n  <MesRecMES6>NTA.TR</MesRecMES6>\r\n  <DatOfPreMES9>240222</DatOfPreMES9>\r\n  <TimOfPreMES10>2115</TimOfPreMES10>\r\n  <IntConRefMES11>NCT00050123</IntConRefMES11>\r\n  <AckReqMES16>0</AckReqMES16>\r\n  <TesIndMES18>0</TesIndMES18>\r\n  <MesIdeMES19>NCT00050123KR</MesIdeMES19>\r\n  <MesTypMES20>CC015B</MesTypMES20>\r\n  <ComAgrIdMES17>0</ComAgrIdMES17>\r\n  <HEAHEA>\r\n    <RefNumHEA4>12</RefNumHEA4>\r\n    <TypOfDecHEA24>TR</TypOfDecHEA24>\r\n    <CouOfDesCodHEA30/>\r\n    <AgrLocOfGooCodHEA38> S</AgrLocOfGooCodHEA38>\r\n    <AgrLocOfGooHEA39LNG>TR</AgrLocOfGooHEA39LNG>\r\n    <CouOfDisCodHEA55/>\r\n    <IdeOfMeaOfTraAtDHEA78/>\r\n    <IdeOfMeaOfTraAtDHEA78LNG/>\r\n    <NatOfMeaOfTraAtDHEA80/>\r\n    <ConIndHEA96>0</ConIndHEA96>\r\n    <DiaLanIndAtDepHEA254>TR</DiaLanIndAtDepHEA254>\r\n    <NCTSAccDocHEA601LNG>TR</NCTSAccDocHEA601LNG>\r\n    <CHKGIK117>HAYIR</CHKGIK117>\r\n    <LOCGIK117/>\r\n    <TotNumOfIteHEA305>0</TotNumOfIteHEA305>\r\n    <TotNumOfPacHEA306>0</TotNumOfPacHEA306>\r\n    <TotGroMasHEA307>0</TotGroMasHEA307>\r\n    <DecDatHEA383>20240222</DecDatHEA383>\r\n    <DecPlaHEA394/>\r\n    <DecPlaHEA394LNG/>\r\n    <DamgaVergi>0.0000</DamgaVergi>\r\n    <RefNumEBT1/>\r\n    <Tanker358>0</Tanker358>\r\n    <DMGV_STATUS>1</DMGV_STATUS>\r\n    <DMGV_DEFTER_TARIHI>22-Feb-24</DMGV_DEFTER_TARIHI>\r\n  </HEAHEA>\r\n  <TRAPRIPC1>\r\n    <NamPC17/>\r\n    <StrAndNumPC122> </StrAndNumPC122>\r\n    <PosCodPC123/>\r\n    <CitPC124/>\r\n    <CouPC125/>\r\n    <NADLNGPC/>\r\n  </TRAPRIPC1>\r\n  <TRACONCO1>\r\n    <NamCO17/>\r\n    <StrAndNumCO122> </StrAndNumCO122>\r\n    <PosCodCO123/>\r\n    <CitCO124/>\r\n    <CouCO125/>\r\n    <NADLNGCO/>\r\n  </TRACONCO1>\r\n  <TRACONCE1>\r\n    <NamCE17/>\r\n    <StrAndNumCE122> </StrAndNumCE122>\r\n    <PosCodCE123/>\r\n    <CitCE124/>\r\n    <CouCE125/>\r\n    <NADLNGCE/>\r\n  </TRACONCE1>\r\n  <CUSOFFDEPEPT/>\r\n  <CUSOFFDESEST>\r\n    <RefNumEST1/>\r\n  </CUSOFFDESEST>\r\n  <REPREP>\r\n    <NamREP5>Mehmet Çakmak</NamREP5>\r\n  </REPREP>\r\n  <GUAGUA>\r\n    <GuaTypGUA1/>\r\n    <GUAREFREF>\r\n      <CurREF8>TRY</CurREF8>\r\n      <AmoConREF7>0</AmoConREF7>\r\n    </GUAREFREF>\r\n  </GUAGUA>\r\n  <CARTRA100>\r\n    <NamCARTRA121/>\r\n    <StrAndNumCARTRA254> </StrAndNumCARTRA254>\r\n    <PosCodCARTRA121/>\r\n    <CitCARTRA789/>\r\n    <CouCodCARTRA587/>\r\n    <NADCARTRA121/>\r\n  </CARTRA100>\r\n  <BS>\r\n    <NamBeyanSahibi>EDI Demonstration System TR</NamBeyanSahibi>\r\n    <StBeyanSahibi>Resit Pasa Mah. Katar Cad.ARI Teknokent 1 No:2/5 </StBeyanSahibi>\r\n    <CitBeyanSahibi>Istanbul</CitBeyanSahibi>\r\n    <TinBeyanSahibi>VATREGNO1111198</TinBeyanSahibi>\r\n    <MusavirVergino>VATREGNO1111198</MusavirVergino>\r\n    <MusavirUnvan>EDI Demonstration System TR</MusavirUnvan>\r\n  </BS>\r\n</CC015B>]]></MSG_CONTENT>\r\n    </ws:submitdeclaration>\r\n  </soapenv:Body>\r\n</soapenv:Envelope>";
			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestTestProcessSignedXMLMessageForDTE()
		{
			var signedMessageBytes = TRMessageTestHelper.GetFileBinary("Common.SampleSignedMessage.xml");
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.DTE, signedMessageBytes);
			var expectedMessageString = $@"<soapenv:Envelope xmlns:any=""http://schemas.microsoft.com/BizTalk/2003/Any"" xmlns:gum=""http://Gumruk.BizTalk.Integration"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <gum:Tescil>
      <any:Root>
        <mesaj>{Convert.ToBase64String(signedMessageBytes)}</mesaj>
      </any:Root>
    </gum:Tescil>
  </soapenv:Body>
</soapenv:Envelope>";

			AssertEquals(expectedMessageString, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestProcessWithoutSignedXMLMessageForExportUnion()
		{
			var messageText = (NoResString)@"<?xml version=""1.0"" encoding=""UTF-8""?><ExportUnion><MessageText><Content> dummy XML message.</Content></MessageText></ExportUnion>";
			var otherParameters = "ulu1987appRef,RandomUserID,RandomPassHash," + messageText;
			var processedMessageBytes = TRMessageSigner.New().ProcessSignedXMLMessage(TRMessageTypes.Codes.EUT, ZBlob.Empty, otherParameters);
			AssertEquals(messageText, Encoding.UTF8.GetString(processedMessageBytes));
		}

		public void TestShouldSendMessageWithoutSigning()
		{
			var item = TRCustomsDataRegistry.Instance.SendTRNCTSMessageWithoutSign;

			using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var glbPassword = CreateExternalPassword(staff, "EKART", "02b9572b9cad7250a906b3");
				var ediMessage = Factory.New<EDIMessage>();
				ediMessage.EM_MessageText = "<Body><Test>MessageWithoutSigning</Test></Body>";

				ediMessage.EM_MessageType = TRMessageTypes.Codes.DTE;
				TRMessageSignerForTest.New().SignEdiMessage(ediMessage, glbPassword, "123456");
				AssertNotContains("DTE Message should be signed", "<Body><Test>MessageWithoutSigning</Test></Body>", ediMessage.EM_MessageData.ToUTF8());

				var messageTypes = new[] { TRMessageTypes.Codes.TRN, TRMessageTypes.Codes.EUT, TRMessageTypes.Codes.TR5, TRMessageTypes.Codes.T15 };

				foreach (var type in messageTypes)
				{
					ediMessage.EM_MessageType = type;

					TRMessageSignerForTest.New().SignEdiMessage(ediMessage, glbPassword, "123456");

					AssertContains("Message should be without signed", "<Body><Test>MessageWithoutSigning</Test></Body>", ediMessage.EM_MessageData.ToUTF8());
				}
			}
		}

		GlbExternalPassword_TR CreateExternalPassword(GlbStaff staff, string chipset, string certificateSerialNumber)
		{
			var result = TRGlbStaffWrapper.Get(staff).TRBPassword;
			result.GP_UserID = "1234";
			result.CurrentDecryptedPassword = "xxx";
			result.GP_CertificateAuthority = "TÜBİTAK";
			result.TR_Chipset = chipset;
			result.GP_CertificateSerialNumber = certificateSerialNumber;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TSN";
			staff.GS_FullName = "TR Testing User";
			Factory.Save();
		}
		GlbStaff staff;
		readonly ZString message = "<Body><Test>MessageToSign</Test></Body>";
	}

	public class SignatureBuilderForTest : ISignatureBuilder
	{
		byte[] ISignatureBuilder.Sign(byte[] messageText, ISignatureAlgorithm signatureAlgorithm)
		{
			return ((ISignatureBuilder)this).Sign(messageText, signatureAlgorithm, DateTime.UtcNow);
		}

		byte[] ISignatureBuilder.Sign(byte[] messageText, ISignatureAlgorithm signatureAlgorithm, DateTime utcNow)
		{
			var msg = Encoding.UTF8.GetString(messageText);
			var algName = signatureAlgorithm.GetType().Name;

			var data = $"Signed with: {algName}\nMessage: {msg}";

			return Encoding.UTF8.GetBytes(data);
		}
	}

	public class TRMessageSignerForTest : TRMessageSigner
	{
		public static new TRMessageSigner New() => new TRMessageSignerForTest(new SignatureBuilderForTest());
		public static TRMessageSigner New(ISignatureBuilder signatureBuilder) => new TRMessageSignerForTest(signatureBuilder ?? new SignatureBuilderForTest());
		protected TRMessageSignerForTest(ISignatureBuilder signatureBuilder) : base(signatureBuilder) { }
	}
}
