using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class OGAIndicatorToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OGAIndicatorToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(OGAIndicatorList.Codes.Declared, nameof(Xsd.OGAIndicator.Declared));
			yield return new Mapping(OGAIndicatorList.Codes.Disclaimed, nameof(Xsd.OGAIndicator.Disclaimed));
		}

		public static readonly OGAIndicatorToXmlCodeMappings Instance = new OGAIndicatorToXmlCodeMappings();

		protected override string Name
		{
			get { return "Invoice Line OGA Indicator"; }
		}

		public new Xsd.OGAIndicator GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OGAIndicator.Declared, errorContext, notify);
		}
	}
}
