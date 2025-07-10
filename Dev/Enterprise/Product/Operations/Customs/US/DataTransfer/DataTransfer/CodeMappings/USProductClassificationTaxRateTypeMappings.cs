using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class USProductClassificationTaxRateTypeMappings : EnterpriseCodeExternalCodeMappings
	{
		USProductClassificationTaxRateTypeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(RateTypeList.Codes.Primary, nameof(Xsd.USProductClassificationTaxRateType.P));
			yield return new Mapping(RateTypeList.Codes.Secondary, nameof(Xsd.USProductClassificationTaxRateType.S));
		}

		public static readonly USProductClassificationTaxRateTypeMappings Instance = new USProductClassificationTaxRateTypeMappings();

		protected override string Name
		{
			get { return "Tax Rate Type"; }
		}

		public new Xsd.USProductClassificationTaxRateType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USProductClassificationTaxRateType.P, errorContext, notify);
		}
	}
}
