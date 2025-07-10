using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class RunSheetSecurityProviderTest : TestCaseWithFactory
	{
		public void TestGetProvider()
		{
			var provider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertNotNull(provider);
			AssertEquals(typeof(RunSheetSecurityProvider), provider.GetType());
			var otherFactory = new BusinessObjectFactory();
			var providerInOtherFactory = RunSheetSecurityProvider.GetProvider(otherFactory);
			AssertNotEquals(provider, providerInOtherFactory);
			var testProvider = new RunSheetSecurityGUIProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => testProvider);
			provider = RunSheetSecurityProvider.GetProvider(Factory);
			AssertEquals(typeof(RunSheetSecurityGUIProviderForTest), provider.GetType());
			AssertEquals(testProvider, provider);
		}

		class RunSheetSecurityGUIProviderForTest : IRunSheetSecurityQueryProvider
		{
			void IRunSheetSecurityQueryProvider.TryAuthorise(CommonCartageLeg leg)
			{
				throw new System.NotImplementedException();
			}
		}

		[ExpectNoExceptions()]
		public void TestIRunSheetSecurityQueryProvider_IsOKToAttachLeg()
		{
			var provider = RunSheetSecurityProvider.GetProvider(Factory);
			provider.TryAuthorise(null);
			var leg = Factory.New<CommonCartageLeg>();
			AssertEquals(false, leg.IsRunSheetAuthorised);
			provider.TryAuthorise(leg);
			AssertEquals(true, leg.IsRunSheetAuthorised);
		}
	}
}
