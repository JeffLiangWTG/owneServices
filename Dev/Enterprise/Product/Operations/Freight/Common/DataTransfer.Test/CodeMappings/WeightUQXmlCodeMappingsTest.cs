using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.DataTransfer.Testing
{
	[TestedType(typeof(WeightUQXmlCodeMappings))]
	public class WeightUQXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Core.Constants.Weight) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
