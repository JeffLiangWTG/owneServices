using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class USInvoiceLineTaxCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		USInvoiceLineTaxCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, nameof(Xsd.USInvoiceLineTaxCode.Item016));
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.Wines, nameof(Xsd.USInvoiceLineTaxCode.Item017));
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.Tobacco, nameof(Xsd.USInvoiceLineTaxCode.Item018));
			yield return new Mapping(Core.Constants.USCustoms.FeeCodes.OtherExcise, nameof(Xsd.USInvoiceLineTaxCode.Item022));
		}

		public static readonly USInvoiceLineTaxCodeMappings Instance = new USInvoiceLineTaxCodeMappings();

		protected override string Name
		{
			get { return "Tax Code"; }
		}

		public new Xsd.USInvoiceLineTaxCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USInvoiceLineTaxCode.Item016, errorContext, notify);
		}
	}
}
