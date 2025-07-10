using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer
{
	[Immutable]
	class PaymentTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		PaymentTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(PaymentTypeList.Codes.IndividualBasis, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item1));
			yield return new Mapping(PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item2));
			yield return new Mapping(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item3));
			yield return new Mapping(PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item5));
			yield return new Mapping(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item6));
			yield return new Mapping(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item7));
			yield return new Mapping(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes, nameof(Xsd.USImporterOfRecordDetailsPaymentType.Item8));
		}

		public static readonly PaymentTypeToXmlCodeMappings Instance = new PaymentTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return "Declaration Payment Type"; }
		}

		public new Xsd.USImporterOfRecordDetailsPaymentType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.USImporterOfRecordDetailsPaymentType.Item1, errorContext, notify);
		}
	}
}
