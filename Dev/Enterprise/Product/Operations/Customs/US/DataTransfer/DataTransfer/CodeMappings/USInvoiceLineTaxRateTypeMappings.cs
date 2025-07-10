using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class USInvoiceLineTaxRateTypeMappings : EnterpriseCodeExternalCodeMappings
	{
		USInvoiceLineTaxRateTypeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(RateTypeList.Codes.Primary, nameof(Xsd.USInvoiceLineTaxRateType.P));
			yield return new Mapping(RateTypeList.Codes.Secondary, nameof(Xsd.USInvoiceLineTaxRateType.S));
		}

		public static readonly USInvoiceLineTaxRateTypeMappings Instance = new USInvoiceLineTaxRateTypeMappings();

		protected override string Name
		{
			get { return "Tax Rate Type"; }
		}

		public new Xsd.USInvoiceLineTaxRateType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USInvoiceLineTaxRateType.P, errorContext, notify);
		}
	}
}
