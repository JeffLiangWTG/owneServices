using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.TamamlayiciBeyan;
using CargoWise.Customs.TR.MessageDefinitions.ETrade.TnsTypes;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public class ETradeComplementaryDecMessageBuilder
	{
		public ETradeComplementaryDecMessageBuilder(IETradeComplementaryDec provider)
		{
			this.provider = CargoWise.Common.Argument.NotNull(provider, nameof(provider));
		}
		readonly IETradeComplementaryDec provider;

		public ZString GetMessageText(ZString userId, ZString userPassword, ZString applicationReference)
		{
			return GetGetMessageBodyText(userId, userPassword, applicationReference);
		}

		public ZString GetGetMessageBodyText(ZString userId, ZString userPassword, ZString applicationReference)
		{
			userPassword = TRManifestMessageBuilderHelper.MD5Hash(userPassword);

			ZString xmlContent = AddComplementaryDecleration();
			var xmlBody = TRMessageHelper.GenerateXml(applicationReference, userId, userPassword, xmlContent);
			return xmlBody;
		}

		public string AddComplementaryDecleration()
		{
			var complementaryDecleration = new TamamlayiciBeyan();

			if (!provider.DeclarationOwnerRepresentativeNameAndTitle.IsEmpty || !provider.DeclarationOwnerRepresentativeTaxNo.IsEmpty)
			{
				complementaryDecleration.BeyanSahibiTemsilci = new BeyanSahibiTemsilci()
				{
					AdiUnvani = provider.DeclarationOwnerRepresentativeNameAndTitle.IsEmpty ? null : provider.DeclarationOwnerRepresentativeNameAndTitle,
					VergiTcNo = provider.DeclarationOwnerRepresentativeNameAndTitle.IsEmpty ? null : provider.DeclarationOwnerRepresentativeTaxNo
				};
			}

			complementaryDecleration.BeyannameNo = provider.RegistrationNo.IsEmpty ? null : provider.RegistrationNo;

			complementaryDecleration.TasimaSenetleri = GetBills(provider.Bills);

			var result = XmlObjectSerializer.SerializeWithNamespaces(
				xmlObject: complementaryDecleration,
				encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
				settings: new XmlWriterSettings { OmitXmlDeclaration = true, },
				namespaces: new XmlSerializerNamespaces(new[] { new XmlQualifiedName(string.Empty, "http://tempuri.org/") }
			));

			return result;
		}

		Collection<TasimaSenediTamamlayiciBilgi> GetBills(IEnumerable<IBillComplementary> bills)
		{
			var billComplementaryInfoList = new Collection<TasimaSenediTamamlayiciBilgi>();
			foreach (var bill in bills)
			{
				var billComplementaryInfo = new TasimaSenediTamamlayiciBilgi();
				billComplementaryInfo.TasimaSenediNo = bill.BillNo;
				billComplementaryInfo.AliciVergiTcNo = bill.ConsigneeTaxIDNo;
				if (!bill.DeliveryDate.IsEmpty)
				{
					billComplementaryInfo.TeslimTarihi = Convert.ToDateTime(bill.DeliveryDate.ToSmallDateTime().ToString(), CultureInfo.InvariantCulture);
				}

				billComplementaryInfoList.Add(billComplementaryInfo);
			}

			return billComplementaryInfoList;
		}
	}
}
