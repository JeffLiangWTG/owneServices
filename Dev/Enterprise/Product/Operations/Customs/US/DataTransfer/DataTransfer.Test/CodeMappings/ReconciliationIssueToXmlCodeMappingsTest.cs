using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(ReconciliationIssueToXmlCodeMappings))]
	sealed class ReconciliationIssueToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(ReconIssueCodeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			ReconIssueCodeList.Codes._9802Recon,
			ReconIssueCodeList.Codes.Class9802Recon,
			ReconIssueCodeList.Codes.ClassRecon,
			ReconIssueCodeList.Codes.FTA,
			ReconIssueCodeList.Codes.Value9802Recon,
			ReconIssueCodeList.Codes.ValueClass9802Recon,
			ReconIssueCodeList.Codes.ValueClassRecon,
			ReconIssueCodeList.Codes.ValueRecon
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
