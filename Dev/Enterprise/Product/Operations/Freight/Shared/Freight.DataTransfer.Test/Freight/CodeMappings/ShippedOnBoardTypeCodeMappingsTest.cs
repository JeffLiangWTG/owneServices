using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ShippedOnBoardTypeCodeMappings))]
	sealed class ShippedOnBoardTypeCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(FreightConstants.ShippedOnBoardType) }; }
		}
	}
}
