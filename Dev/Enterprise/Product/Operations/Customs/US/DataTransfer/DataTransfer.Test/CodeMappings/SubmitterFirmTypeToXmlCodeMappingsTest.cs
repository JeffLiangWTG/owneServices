using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(SubmitterFirmTypeToXmlCodeMappings))]
	sealed class SubmitterFirmTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(SubmitterFirmTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			SubmitterFirmTypeList.Codes.Carrier,
			SubmitterFirmTypeList.Codes.F,
			SubmitterFirmTypeList.Codes.I,
			SubmitterFirmTypeList.Codes.M,
			SubmitterFirmTypeList.Codes.S,
			SubmitterFirmTypeList.Codes.U,
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
