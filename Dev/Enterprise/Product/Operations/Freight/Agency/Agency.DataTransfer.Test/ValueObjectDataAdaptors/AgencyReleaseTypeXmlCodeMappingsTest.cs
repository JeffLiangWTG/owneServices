using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyReleaseTypeXmlCodeMappings))]
	internal class AgencyReleaseTypeXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get
			{
				return new[] { typeof(AgencyReleaseTypesForTest) };
			}
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get
			{
				return true;
			}
		}

		class AgencyReleaseTypesForTest
		{
			public const string ExpressBofL = "EBL";
			public const string OriginalReq = "OBR";
			public const string OriginalReqSurrender = "OBO";
			public const string SeaWaybill = "SWB";
		}
	}
}
