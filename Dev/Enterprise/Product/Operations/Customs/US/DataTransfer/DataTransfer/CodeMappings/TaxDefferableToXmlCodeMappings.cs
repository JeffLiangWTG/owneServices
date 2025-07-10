using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class TaxDefferableToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		TaxDefferableToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(TaxDeferIndicatorList.Codes.NotApplicableOrNoDeferredTax, nameof(Xsd.USImporterOfRecordDetailsTaxDeferredInd.Item0));
			yield return new Mapping(TaxDeferIndicatorList.Codes.DeferredTax, nameof(Xsd.USImporterOfRecordDetailsTaxDeferredInd.Item1));
			yield return new Mapping(TaxDeferIndicatorList.Codes.DeferredTaxWithEFT, nameof(Xsd.USImporterOfRecordDetailsTaxDeferredInd.Item2));
			yield return new Mapping(TaxDeferIndicatorList.Codes.BulkLiquorDeferred, nameof(Xsd.USImporterOfRecordDetailsTaxDeferredInd.B));
		}

		public static readonly TaxDefferableToXmlCodeMappings Instance = new TaxDefferableToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration Tax Defferable"; }
		}

		public new Xsd.USImporterOfRecordDetailsTaxDeferredInd GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USImporterOfRecordDetailsTaxDeferredInd.Item0, errorContext, notify);
		}
	}
}
