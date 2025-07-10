using System;
using System.Xml;
using CargoWise.Types;
namespace Enterprise.Customs.TR.Business
{
	public class InnerXmlGroupageAnswerObject : InnerMessageObjectBase
	{
		public new static class Constants
		{
			public const string GroupageAnswer = "GrupajCevap";

			public const string CountryCode = "UlkeKodu";
			public const string LicensePlateNo = "PlakaSeferNo";
			public const string Arrivaldate = "VarisTarihi";
			public const string GdStartDate = "GdBaslangicTarihi";
			public const string GdTime = "GdSuresi";
			public const string NameOfVehicle = "TasitinAdi";
			public const string ReferenceNumber = "ReferanNumarasi";
			public const string InternalNoDso = "DahiliNoDso";
			public const string TransportType = "TasimaSekli";
			public const string PortLocationNameYuk = "LimanYerAdiYuk";
			public const string PortLocationNameBos = "LimanYerAdiBos";
			public const string CountryCodeYuk = "UlkeKoduYuk";
			public const string CountryCodeBos = "UlkeKoduBos";

			public const string GroupageCompany = "GrupajFirma";
			public const string IdentificationNumber = "KimlikNo";
			public const string IdentityTour = "KimlikTuru";
			public const string NameTitle = "AdiUnvani";
		}

		public static (string ElementName, string NamespaceUrl)[] MatchPath => matchPath ?? (matchPath = new (string, string)[] {
			(InnerMessageObjectBase.Constants.ResultInfo, TRMessageConstants.Xml.CustomsNamespace),
			(Constants.GroupageAnswer, TRMessageConstants.Xml.CustomsNamespace)
		});

		[ThreadStatic]
		static (string, string)[] matchPath;

		public InnerXmlGroupageAnswerObject(XmlElement groupageAnswerElement) : base(groupageAnswerElement)
		{
			if (groupageAnswerElement != null)
			{
				CountryCode = TryGetInnerText(groupageAnswerElement, Constants.CountryCode);
				LicensePlateNo = TryGetInnerText(groupageAnswerElement, Constants.LicensePlateNo);
				ArrivalDate = TryGetDateTime(TryGetInnerText(groupageAnswerElement, Constants.Arrivaldate));
				GdStartDate = TryGetDateTime(TryGetInnerText(groupageAnswerElement, Constants.GdStartDate));
				GdTime = TryGetDateTime(TryGetInnerText(groupageAnswerElement, Constants.GdTime));

				NameOfVehicle = TryGetInnerText(groupageAnswerElement, Constants.NameOfVehicle);
				ReferenceNumber = TryGetInnerText(groupageAnswerElement, Constants.ReferenceNumber);
				InternalNoDso = TryGetInnerText(groupageAnswerElement, Constants.InternalNoDso);
				TransportType = TryGetInnerText(groupageAnswerElement, Constants.TransportType);
				PortLocationNameYuk = TryGetInnerText(groupageAnswerElement, Constants.PortLocationNameYuk);
				PortLocationNameBos = TryGetInnerText(groupageAnswerElement, Constants.PortLocationNameBos);

				CountryCodeYuk = TryGetInnerText(groupageAnswerElement, Constants.CountryCodeYuk);
				CountryCodeBos = TryGetInnerText(groupageAnswerElement, Constants.CountryCodeBos);

				var groupageCompanyNode = groupageAnswerElement[Constants.GroupageCompany];
				if (groupageCompanyNode != null)
				{
					IdentificationNumber = TryGetInnerText(groupageCompanyNode, Constants.IdentificationNumber);
					IdentityTour = TryGetInnerText(groupageCompanyNode, Constants.IdentityTour);
					NameTitle = TryGetInnerText(groupageCompanyNode, Constants.NameTitle);
				}
			}
		}

		public ZString CountryCode { get; }
		public ZString LicensePlateNo { get; }
		public ZDateTime ArrivalDate { get; }
		public ZDateTime GdStartDate { get; }
		public ZDateTime GdTime { get; }
		public ZString NameOfVehicle { get; }
		public ZString ReferenceNumber { get; }
		public ZString InternalNoDso { get; }
		public ZString TransportType { get; }

		public ZString IdentificationNumber { get; }
		public ZString IdentityTour { get; }
		public ZString NameTitle { get; }
		public ZString PortLocationNameYuk { get; }
		public ZString PortLocationNameBos { get; }
		public ZString CountryCodeYuk { get; }
		public ZString CountryCodeBos { get; }
	}
}
