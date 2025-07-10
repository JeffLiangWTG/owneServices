using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(FDAOwnerFirmTypeToXmlCodeMappings))]
	sealed class FDAOwnerFirmTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(OwnerFirmTypeList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			OwnerFirmTypeList.Codes.Carrier,
			OwnerFirmTypeList.Codes.I,
			OwnerFirmTypeList.Codes.M,
			OwnerFirmTypeList.Codes.U
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
