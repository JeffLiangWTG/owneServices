using System;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	[TestedType(typeof(FilingOptionToXmlCodeMappings))]
	sealed class FilingOptionToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public override void TestNoNewCodesAdded()
		{
			Assert(true);
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst => new[] { typeof(AESCommodityFilingOptionList) };

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst() => new[]
		{
			AESCommodityFilingOptionList.Codes._4Postdeparture,
			AESCommodityFilingOptionList.Codes._2Predeparture
		};

		protected override bool EnterpriseAndExternalCodeShouldBeSame => true;
	}
}
