using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class ConsolidatedInformalToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ConsolidatedInformalToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ConsolidatedInformalList.Codes.Consolidated, nameof(Xsd.USDeclarationConsolidatedInformal.C));
			yield return new Mapping(ConsolidatedInformalList.Codes.Personal, nameof(Xsd.USDeclarationConsolidatedInformal.P));
			yield return new Mapping(ConsolidatedInformalList.Codes.Samples, nameof(Xsd.USDeclarationConsolidatedInformal.X));
		}

		public static readonly ConsolidatedInformalToXmlCodeMappings Instance = new ConsolidatedInformalToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration Consolidated Informal"; }
		}

		public new Xsd.USDeclarationConsolidatedInformal GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationConsolidatedInformal.C, errorContext, notify);
		}
	}
}
