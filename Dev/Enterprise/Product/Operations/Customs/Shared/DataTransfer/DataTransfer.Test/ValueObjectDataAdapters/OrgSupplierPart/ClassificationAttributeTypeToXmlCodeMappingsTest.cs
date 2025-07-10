using System;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(ClassificationAttributeTypeToXmlCodeMappings))]
	sealed class ClassificationAttributeTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(CusAttributeFilter.AttributeFilterName) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[] { nameof(CusAttributeFilter.AttributeFilterName.Unknown) };
	}
}
