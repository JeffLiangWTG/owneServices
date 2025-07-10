using System;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CustomsNumberViewStmNumsGuiProviderForTestingSetUp : CustomsNumberViewStmNumsCompanyProviderForTestSetUp, ITestDynamicObjectHandleSupporter
	{
		public CustomsNumberViewStmNumsGuiProviderForTestingSetUp(bool? enableCompanyLevelForTesting = true, bool? enableBranchLevelForTesting = true)
			: base(enableCompanyLevelForTesting, enableBranchLevelForTesting)
		{
		}

		object ITestDynamicObjectHandleSupporter.GetObject()
		{
			throw new NotImplementedException();
		}

		Type ITestDynamicObjectHandleSupporter.GetObjectType()
		{
			return typeof(CustomsNumberViewStmNumsGuiProviderForTesting);
		}

		object ITestDynamicObjectHandleSupporter.GetObject(object[] arguments)
		{
			return new CustomsNumberViewStmNumsGuiProviderForTesting((BusinessObjectFactory)arguments[0], Core.Constants.CountryCodes.Eritrea, (ZGuid)arguments[1], enableCompanyLevelForTesting, enableBranchLevelForTesting);
		}
	}
}
