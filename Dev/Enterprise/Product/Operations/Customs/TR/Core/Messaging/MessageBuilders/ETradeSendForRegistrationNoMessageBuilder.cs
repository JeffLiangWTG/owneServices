using CargoWise.Customs.TR.MessageDefinitions.ETrade.ETradeRegistrationQuery;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.TnsTypes;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Messaging
{
	public class ETradeSendForRegistrationNoMessageBuilder
	{
		public ETradeSendForRegistrationNoMessageBuilder()
		{
		}

		public ZString GetMessageText(ZString userId, ZString userPassword, ZString applicationReference, IETradeSendForRegistrationNo provider)
		{
			return GetGetMessageBodyText(userId, userPassword, applicationReference, provider);
		}

		ZString GetGetMessageBodyText(ZString userId, ZString userPassword, ZString applicationReference, IETradeSendForRegistrationNo provider)
		{
			userPassword = TRManifestMessageBuilderHelper.MD5Hash(userPassword);
			ZString xmlContent = AddSendForRegistrationNo(provider);
			//<?xml version="1.0" encoding="utf-8"?> is 0-38
			ZString xmlEncoding = xmlContent.SubstringSafe(0, 38);
			xmlContent = xmlContent.SubstringSafe(40).Replace("<TCGBTescil>", "<TCGBTescil xmlns=\"http://tempuri.org/\">");

			ZString xmlBody = ZString.Format((NoResString)"{0}\r\n" +
											 (NoResString)"<Root>\r\n" +
											 (NoResString)"<RefID>{1}</RefID>\r\n" +
											 (NoResString)"<KullaniciAdi>{2}</KullaniciAdi>\r\n" +
											 (NoResString)"<Sifre>{3}</Sifre>\r\n" +
											 (NoResString)"<RequestMessage>\r\n" +
											 (NoResString)"{4}\r\n" +
											 (NoResString)"</RequestMessage>\r\n" +
											 (NoResString)"</Root>\r\n",
											 xmlEncoding, applicationReference, userId, userPassword, xmlContent);
			return xmlBody;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public string AddSendForRegistrationNo(IETradeSendForRegistrationNo provider)
		{
			var sendForRegistrationNo = new TcgbTescil();

			if (!provider.DeclarantNameAndTitle.IsEmpty || !provider.DeclarantIDTaxNo.IsEmpty)
			{
				sendForRegistrationNo.BeyanSahibiTemsilci = new BeyanSahibiTemsilci()
				{
					AdiUnvani = provider.DeclarantNameAndTitle.IsEmpty ? null : provider.DeclarantNameAndTitle,
					VergiTcNo = provider.DeclarantIDTaxNo.IsEmpty ? null : provider.DeclarantIDTaxNo
				};
			}

			sendForRegistrationNo.GeciciTescilNo = provider.TemporaryRegistrationNo.IsEmpty ? null : provider.TemporaryRegistrationNo;
			sendForRegistrationNo.GumrukIdaresi = provider.CustomsOffice.IsEmpty ? null : provider.CustomsOffice;

			return CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(sendForRegistrationNo);
		}
	}
}
