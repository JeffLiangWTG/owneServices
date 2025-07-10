using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(PaymentTypeToXmlCodeMappings))]
	sealed class PaymentTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(PaymentTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			PaymentTypeList.Codes.IndividualBasis,
			PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode,
			PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter,
			PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes,
			PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate,
			PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter,
			PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
