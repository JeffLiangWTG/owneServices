using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCFXConfigurationExtensionsTest : TestCase
	{
		public void TestCFXLevel()
		{
			AssertEquals(AccCFXConfigurationLevelEnum.Company, ((ZString)"").ToCFXLevel());
			AssertEquals(AccCFXConfigurationLevelEnum.Branch, ((ZString)"GB").ToCFXLevel());
			AssertEquals(AccCFXConfigurationLevelEnum.Organisation, ((ZString)"OH").ToCFXLevel());

			AssertExceptionThrown<InvalidOperationException>("Unknown table prefix value XXX", () => ((ZString)"XXX").ToCFXLevel());
		}

		public void TestTablePrefix()
		{
			AssertEquals(string.Empty, AccCFXConfigurationLevelEnum.Company.ToTablePrefix());
			AssertEquals("GB", AccCFXConfigurationLevelEnum.Branch.ToTablePrefix());
			AssertEquals("OH", AccCFXConfigurationLevelEnum.Organisation.ToTablePrefix());
		}
	}
}
