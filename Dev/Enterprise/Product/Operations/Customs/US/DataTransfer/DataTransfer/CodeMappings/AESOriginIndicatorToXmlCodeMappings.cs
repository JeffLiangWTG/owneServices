using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class AESOriginIndicatorToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		AESOriginIndicatorToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(AESOriginIndicatorList.Codes.Domestic, nameof(Xsd.USInvoiceLineOriginIndicator.D));
			yield return new Mapping(AESOriginIndicatorList.Codes.Foreign, nameof(Xsd.USInvoiceLineOriginIndicator.F));
		}

		public static readonly AESOriginIndicatorToXmlCodeMappings Instance = new AESOriginIndicatorToXmlCodeMappings();

		protected override string Name
		{
			get { return "AES Origin Indicator"; }
		}

		public new Xsd.USInvoiceLineOriginIndicator GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USInvoiceLineOriginIndicator.D, errorContext, notify);
		}
	}
}
