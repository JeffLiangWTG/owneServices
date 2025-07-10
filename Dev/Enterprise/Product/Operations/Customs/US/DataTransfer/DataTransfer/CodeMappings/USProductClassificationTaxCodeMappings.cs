using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class USProductClassificationTaxCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		USProductClassificationTaxCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, nameof(Xsd.USProductClassificationTaxCode.Item016));
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.Wines, nameof(Xsd.USProductClassificationTaxCode.Item017));
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.Tobacco, nameof(Xsd.USProductClassificationTaxCode.Item018));
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.OtherExcise, nameof(Xsd.USProductClassificationTaxCode.Item022));
		}

		public static readonly USProductClassificationTaxCodeMappings Instance = new USProductClassificationTaxCodeMappings();

		protected override string Name
		{
			get { return "Tax Code"; }
		}

		public new Xsd.USProductClassificationTaxCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USProductClassificationTaxCode.Item016, errorContext, notify);
		}
	}
}
