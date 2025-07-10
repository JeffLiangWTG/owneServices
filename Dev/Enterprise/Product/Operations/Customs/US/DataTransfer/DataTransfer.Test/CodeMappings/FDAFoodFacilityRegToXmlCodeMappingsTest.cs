using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(FDAFoodFacilityRegToXmlCodeMappings))]
	sealed class FDAFoodFacilityRegToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(FDAPriorNoticeExemptCodeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			FDAPriorNoticeExemptCodeList.Codes.A,
			FDAPriorNoticeExemptCodeList.Codes.B,
			FDAPriorNoticeExemptCodeList.Codes.C,
			FDAPriorNoticeExemptCodeList.Codes.D,
			FDAPriorNoticeExemptCodeList.Codes.E,
			FDAPriorNoticeExemptCodeList.Codes.F,
			FDAPriorNoticeExemptCodeList.Codes.G,
			FDAPriorNoticeExemptCodeList.Codes.H,
			FDAPriorNoticeExemptCodeList.Codes.I,
			FDAPriorNoticeExemptCodeList.Codes.J,
			FDAPriorNoticeExemptCodeList.Codes.K,
			FDAPriorNoticeExemptCodeList.Codes.L,
			FDAPriorNoticeExemptCodeList.Codes.M,
			FDAPriorNoticeExemptCodeList.Codes.O,
			FDAPriorNoticeExemptCodeList.Codes.Y
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
