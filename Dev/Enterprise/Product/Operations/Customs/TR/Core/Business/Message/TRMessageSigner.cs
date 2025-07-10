using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.Customs.TR.MessageDefinitions.BizTalk.Any;
using CargoWise.Customs.TR.MessageDefinitions.Gum;
using CargoWise.Customs.TR.MessageDefinitions.NCTSSubmitDeclaration;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class TRMessageSigner
	{
		public static TRMessageSigner New() => new TRMessageSigner(new SignatureBuilder());

		protected TRMessageSigner(ISignatureBuilder signatureBuilder)
		{
			sigBuilder = signatureBuilder;
		}
		readonly ISignatureBuilder sigBuilder;

		public static byte[] Encode(string message)
		{
			return Encoding.UTF8.GetBytes(message);
		}

		public void SignEdiMessage(EDIMessage message, GlbExternalPassword_TR externalPassword, ZString pinCode)
		{
			SignEdiMessage(message, externalPassword, pinCode, message.EM_MessageText);
		}

		public void SignEdiMessage(EDIMessage message, GlbExternalPassword_TR externalPassword, ZString pinCode, ZString messageText)
		{
			string otherParameters = message.EM_ApplicationReference + "," + externalPassword.GP_UserID + "," + TRManifestMessageBuilderHelper.MD5Hash(externalPassword.CurrentDecryptedPassword) + "," + messageText;
			var sendMessageWithOutSigning = ShouldSendMessageWithoutSigning(message.EM_MessageType) && TRCustomsDataRegistry.Instance.IsSendTRNCTSMessageWithoutSign; //TODO, this code just for testing, in the development stage, TR Customs cannot fix the issue of NCTS signature, this code will removed after 6 month.			
			var signedXMLMessage = sendMessageWithOutSigning ? new ZBlob() : SignXMLMessage(messageText, externalPassword, pinCode);
			message.EM_MessageData = ProcessSignedXMLMessage(message.EM_MessageType, signedXMLMessage, otherParameters);
			message.EM_MessageOwner = GlbStaff.CurrentUser.GS_Code;
		}

		static bool ShouldSendMessageWithoutSigning(ZString messageType)
		{
			return messageType == TRMessageTypes.Codes.EUT ||
				  (messageType == TRMessageTypes.Codes.TRN ||
				   messageType == TRMessageTypes.Codes.TR5 ||
				   messageType == TRMessageTypes.Codes.T15);
		}

		public ZBlob SignXMLMessage(ZString messageText, GlbExternalPassword_TR externalPassword, ZString pinCode)
		{
			byte[] signature = SignMessage(messageText, externalPassword, pinCode);
			ZBlob messageData = new ZBlob(signature);
			return messageData;
		}

		byte[] SignMessage(string message, GlbExternalPassword_TR externalPassword, ZString pinCode)
		{
			var algorithm = GetSignatureAlgorithm(externalPassword, pinCode);
			return sigBuilder.Sign(Encode(message), algorithm);
		}

		ISignatureAlgorithm GetSignatureAlgorithm(GlbExternalPassword_TR externalPassword, ZString pinCode)
		{
			if (string.IsNullOrEmpty(externalPassword.TR_Chipset))
			{
				throw new CryptographicException("Chipset not specified");
			}

			if (!CertificateHelper.TryGetHexValue(externalPassword.GP_CertificateSerialNumber, out var certificateSerialNumberBytes))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Invalid certificate serial number: '{externalPassword.GP_CertificateSerialNumber}'."));
			}

			ISignatureAlgorithm algorithm;
			var cryptoApi = ObjectFactory.Get<ICryptoApi>();
			switch (externalPassword.TR_Chipset)
			{
				case ChipsetList.Codes.WINDOWS:
					algorithm = X509Certificate2CryptoApiSignatureAlgorithm.Create(cryptoApi, certificateSerialNumberBytes);
					break;
				default:
					algorithm = Pkcs11CryptoApiSignatureAlgorithm.Create
					(
						cryptoApi,
						(Chipset)Enum.Parse(typeof(Chipset), externalPassword.TR_Chipset),
						pinCode,
						certificateSerialNumberBytes
					);
					break;
			}

			return algorithm;
		}

		public static bool IsValidCertificateSerialNumber(ZString certificateSerialNumber)
		{
			return CertificateHelper.TryGetHexValue(certificateSerialNumber, out var certificateSerialNumberBytes);
		}

		public ZBlob ProcessSignedXMLMessage(ZString messageType, ZBlob signedXMLMessage, params ZString[] otherParameters)
		{
			Envelope messageData;
			var processedMessage = signedXMLMessage;
			var settings = new XmlWriterSettings
			{
				Indent = true,
				Encoding = Encoding.UTF8,
				OmitXmlDeclaration = true
			};

			var prefixsMappingNamespaces = new Dictionary<string, string>();
			prefixsMappingNamespaces["soapenv"] = "http://schemas.xmlsoap.org/soap/envelope/";
			prefixsMappingNamespaces["any"] = "http://schemas.microsoft.com/BizTalk/2003/Any";

			switch (messageType)
			{
				case TRMessageTypes.Codes.TRO:
					messageData = new Envelope()
					{
						Header = new EnvelopeHeader(),
						Body = new EnvelopeBody()
						{
							OzetBeyan = new OzetBeyan() { Root = new Root() { Mesaj = Convert.ToBase64String(signedXMLMessage) } }
						}
					};

					prefixsMappingNamespaces["gum"] = "http://Gumruk.BizTalk.Integration";
					processedMessage = SerializeSOAPMessage(settings, messageData, prefixsMappingNamespaces);
					break;
				case TRMessageTypes.Codes.TRE:
				case TRMessageTypes.Codes.TRD:
				case TRMessageTypes.Codes.TCD:
				case TRMessageTypes.Codes.TRS:
					messageData = new Envelope()
					{
						Header = new EnvelopeHeader(),
						Body = new EnvelopeBody()
						{
							Root = new CargoWise.Customs.TR.MessageDefinitions.GumETrade.Root() { RequestMessage = Convert.ToBase64String(signedXMLMessage) }
						}
					};

					prefixsMappingNamespaces.Remove((NoResString)"any");
					prefixsMappingNamespaces["GumETrade"] = "http://GumrukETApp.RequestSignedMessage";
					processedMessage = SerializeSOAPMessage(settings, messageData, prefixsMappingNamespaces);
					break;
				case TRMessageTypes.Codes.TSP:
					messageData = new Envelope()
					{
						Header = new EnvelopeHeader(),
						Body = new EnvelopeBody()
						{
							Aktarma = new Aktarma() { Root = new Root() { Mesaj = Convert.ToBase64String(signedXMLMessage) } }
						}
					};
					settings = new XmlWriterSettings
					{
						Indent = true,
						Encoding = Encoding.UTF8,
						OmitXmlDeclaration = true
					};
					prefixsMappingNamespaces["gum"] = "http://Gumruk.BizTalk.Integration";
					processedMessage = SerializeSOAPMessage(settings, messageData, prefixsMappingNamespaces);
					break;
				case TRMessageTypes.Codes.TRN:
					if (otherParameters?.Length > 0)
					{
						var spiltValues = otherParameters[0].Split(',');
						var isSendTRNCTSMessageWithoutSign = TRCustomsDataRegistry.Instance.IsSendTRNCTSMessageWithoutSign;
						var userID = spiltValues[0] + "," + spiltValues[1] + "," + spiltValues[2];

						messageData = new Envelope()
						{
							Header = new EnvelopeHeader(),
							Body = new EnvelopeBody()
							{
								Submitdeclaration = new Submitdeclaration
								{
									FirmId = TRMessageConstants.FirmID,
									UserId = userID,
									MsgType = "CC015B",
									SignFlag = isSendTRNCTSMessageWithoutSign ? "0" : "1",
									MsgContent = isSendTRNCTSMessageWithoutSign ? (NoResString)"##CDATAPLACEHOLDER##" : Convert.ToBase64String(signedXMLMessage)
								}
							}
						};

						prefixsMappingNamespaces.Remove((NoResString)"any");
						prefixsMappingNamespaces["ws"] = "http://ws/";
						processedMessage = SerializeSOAPMessage(settings, messageData, prefixsMappingNamespaces);

						if (isSendTRNCTSMessageWithoutSign)
						{
							var soapContent = Encoding.UTF8.GetString(processedMessage);
							soapContent = soapContent.Replace("##CDATAPLACEHOLDER##", "<![CDATA[" + spiltValues[3] + "]]>");
							processedMessage = Encoding.UTF8.GetBytes(soapContent);
						}
					}
					break;
				case TRMessageTypes.Codes.DTE:
					messageData = new Envelope()
					{
						Header = new EnvelopeHeader(),
						Body = new EnvelopeBody()
						{
							Tescil = new Tescil() { Root = new Root() { Mesaj = Convert.ToBase64String(signedXMLMessage) } }
						}
					};

					prefixsMappingNamespaces["gum"] = "http://Gumruk.BizTalk.Integration";
					processedMessage = SerializeSOAPMessage(settings, messageData, prefixsMappingNamespaces);
					break;
				case TRMessageTypes.Codes.EUT:
					if (otherParameters?.Length > 0)
					{
						var spiltValues = otherParameters[0].Split(',');
						var messageText = spiltValues.Length > 3 ? spiltValues[3] : ZString.Empty;
						processedMessage = Encoding.UTF8.GetBytes(messageText);
					}
					break;
				case TRMessageTypes.Codes.TR5:
				case TRMessageTypes.Codes.T15:
					if (otherParameters?.Length > 0)
					{
						var spiltValues = otherParameters[0].Split(',');
						var messageText = spiltValues.Length > 3 ? spiltValues[3] : ZString.Empty;
						processedMessage = Encoding.UTF8.GetBytes(messageText);
					}
					break;
				default:
					break;
			}

			return processedMessage;
		}

		static ZBlob SerializeSOAPMessage(XmlWriterSettings settings, Envelope messageData, Dictionary<string, string> prefixsMappingNamespaces)
		{
			var namespaces = new XmlSerializerNamespaces();
			foreach (var prefix in prefixsMappingNamespaces.Keys)
			{
				namespaces.Add(prefix, prefixsMappingNamespaces[prefix]);
			}

			var xmlMessage = TRMessageHelper.SerializeSOAPMessage(messageData, settings, namespaces);
			return new ZBlob(Encoding.UTF8.GetBytes(xmlMessage));
		}
	}
}
