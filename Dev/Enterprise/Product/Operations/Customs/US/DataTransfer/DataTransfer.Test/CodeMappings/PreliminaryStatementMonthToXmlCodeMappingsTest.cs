using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(PreliminaryStatementMonthToXmlCodeMappings))]
	sealed class PreliminaryStatementMonthToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(MonthList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			MonthList.Codes._01,
			MonthList.Codes._02,
			MonthList.Codes._03,
			MonthList.Codes._04,
			MonthList.Codes._05,
			MonthList.Codes._06,
			MonthList.Codes._07,
			MonthList.Codes._08,
			MonthList.Codes._09,
			MonthList.Codes._10,
			MonthList.Codes._11,
			MonthList.Codes._12
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
