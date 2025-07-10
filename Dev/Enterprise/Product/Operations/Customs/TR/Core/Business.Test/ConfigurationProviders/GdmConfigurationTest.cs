using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(GdmConfiguration))]
	sealed class GdmConfigurationBaseTest : GdmConfigurationAbstractTest<GdmConfiguration>
	{
		public override void TestWhatIsAWaiver()
		{
			AssertEquals(ZString.Empty, configuration.WhatIsAWaiver);
		}
	}
}
