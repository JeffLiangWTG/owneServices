using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(AgencyContainerMovementOwnerTypeMappings))]
	internal class AgencyContainerMovementOwnerTypeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get
			{
				return new[] { typeof(AgencyOwnerTypesForTest) };
			}
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get
			{
				return true;
			}
		}

		class AgencyOwnerTypesForTest
		{
			public const string Carrier = "CAR";
			public const string Shipper = "SHP";
			public const string Leased = "LEA";
		}
	}
}
