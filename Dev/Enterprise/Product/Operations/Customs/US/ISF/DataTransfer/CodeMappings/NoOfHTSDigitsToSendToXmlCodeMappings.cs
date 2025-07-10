using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class NoOfHTSDigitsToSendToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		NoOfHTSDigitsToSendToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(NumberOfHarmonizedDigitsList.Codes.Six, nameof(Xsd.ISFActionMessagingNoOfHTSDigitsToSend.Item6));
			yield return new Mapping(NumberOfHarmonizedDigitsList.Codes.Eight, nameof(Xsd.ISFActionMessagingNoOfHTSDigitsToSend.Item8));
			yield return new Mapping(NumberOfHarmonizedDigitsList.Codes.Ten, nameof(Xsd.ISFActionMessagingNoOfHTSDigitsToSend.Item10));
		}

		public static readonly NoOfHTSDigitsToSendToXmlCodeMappings Instance = new NoOfHTSDigitsToSendToXmlCodeMappings();

		protected override string Name
		{
			get { return "ISF No. Of HTS Digits To Send Type"; }
		}

		public new Xsd.ISFActionMessagingNoOfHTSDigitsToSend GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFActionMessagingNoOfHTSDigitsToSend.Item10, errorContext, notify);
		}
	}
}
