using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NameBasedProviderTest : TestCaseWithFactory
	{
		public void TestProviderPresent()
		{
			AssertNotNull(NameBasedProvider.Get<OrgCusAccountProvider>(Core.Constants.CountryCodes.Eritrea, new ZString(Core.Constants.CountryCodes.Eritrea)));
		}

		public void TestAbsentProviderThrowsException()
		{
			try
			{
				AssertNull(NameBasedProvider.Get<NameBasedProviderTest>(Core.Constants.CountryCodes.Eritrea, new ZString(Core.Constants.CountryCodes.Eritrea)));
			}
			catch (CargoWise.Application.Exceptions.NoSuchObjectDefinitionException)
			{
				Assert(true);
			}
		}
	}
}
