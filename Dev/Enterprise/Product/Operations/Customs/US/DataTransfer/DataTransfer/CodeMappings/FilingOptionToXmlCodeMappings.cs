using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	public class FilingOptionToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		FilingOptionToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(AESCommodityFilingOptionList.Codes._4Postdeparture, nameof(Xsd.USDeclarationFilingOption.Item4));
			yield return new Mapping(AESCommodityFilingOptionList.Codes._2Predeparture, nameof(Xsd.USDeclarationFilingOption.Item2));
		}

		public static readonly FilingOptionToXmlCodeMappings Instance = new FilingOptionToXmlCodeMappings();

		protected override string Name
		{
			get { return "Filing Options"; }
		}

		public new Xsd.USDeclarationFilingOption GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USDeclarationFilingOption.Item2, errorContext, notify);
		}
	}
}
