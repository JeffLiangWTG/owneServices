using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class OGALineReleaseIndicatorToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OGALineReleaseIndicatorToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(YesNoDefaultList.Codes.No, nameof(Xsd.USDeclarationOGALineReleaseIndicator.N));
			yield return new Mapping(YesNoDefaultList.Codes.Yes, nameof(Xsd.USDeclarationOGALineReleaseIndicator.Y));
		}

		public static readonly OGALineReleaseIndicatorToXmlCodeMappings Instance = new OGALineReleaseIndicatorToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration OGA Line Release Indicator"; }
		}

		public new Xsd.USDeclarationOGALineReleaseIndicator GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationOGALineReleaseIndicator.N, errorContext, notify);
		}
	}
}
