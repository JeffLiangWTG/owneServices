using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CustomsNumberViewStmNumsCompanyProviderForTestSetUp : IDisposable, ITestDynamicObjectHandleSupporter
	{
		public CustomsNumberViewStmNumsCompanyProviderForTestSetUp(bool? enableCompanyLevelForTesting = true, bool? enableBranchLevelForTesting = true)
		{
			var providers = new Hashtable
			{
				{ Core.Constants.CountryCodes.Eritrea, new TestDynamicObjectHandle(this) }
			};
			this.enableCompanyLevelForTesting = enableCompanyLevelForTesting;
			this.enableBranchLevelForTesting = enableBranchLevelForTesting;
			providersSubstitution = ObjectFactory.Substitute("CustomsNumberViewStmNumsBusinessProviders", providers);
		}
		protected bool? enableCompanyLevelForTesting;
		protected bool? enableBranchLevelForTesting;
		readonly IDisposable providersSubstitution;

		void IDisposable.Dispose()
		{
			providersSubstitution.Dispose();
		}

		object ITestDynamicObjectHandleSupporter.GetObject()
		{
			throw new NotImplementedException();
		}

		Type ITestDynamicObjectHandleSupporter.GetObjectType()
		{
			return typeof(CustomsNumberViewStmNumsCompanyProviderForTest);
		}

		object ITestDynamicObjectHandleSupporter.GetObject(object[] arguments)
		{
			return new CustomsNumberViewStmNumsCompanyProviderForTest((BusinessObjectFactory)arguments[0], Core.Constants.CountryCodes.Eritrea, (ZGuid)arguments[1], enableCompanyLevelForTesting, enableBranchLevelForTesting);
		}
	}
}
