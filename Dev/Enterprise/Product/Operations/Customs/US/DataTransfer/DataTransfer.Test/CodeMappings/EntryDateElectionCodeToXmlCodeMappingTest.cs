using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(EntryDateElectionCodeToXmlCodeMapping))]
	sealed class EntryDateElectionCodeToXmlCodeMappingTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(EntryDateElectionCodeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			EntryDateElectionCodeList.Codes.ArrivalDate,
			EntryDateElectionCodeList.Codes.PresentationDate,
			EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate,
			EntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
