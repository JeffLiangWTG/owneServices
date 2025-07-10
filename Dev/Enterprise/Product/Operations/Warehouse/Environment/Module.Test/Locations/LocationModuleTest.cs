using System;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(LocationModule))]
	class LocationModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (LocationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsConfigLocation, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (LocationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (LocationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigLocation, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			using (var module = (LocationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Type typeOfTopLevelBusinessObject = null;
				AssertNoExceptionThrown("LocationModule.TypeOfTopLevelBusinessObject should not throw an exception.", () => typeOfTopLevelBusinessObject = module.TypeOfTopLevelBusinessObject);
				AssertNotNull("Returned type is not null.", typeOfTopLevelBusinessObject);
				AssertEquals("Returned type is WhsLocation.", typeOfTopLevelBusinessObject, typeof(WhsLocation));
			}
		}

		#endregion

		#region Unsupported

		[RequiresSTA]
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert("LocationModule.GetNewGridCollection() is not supported. This module is only available from a findbox.", true);
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("LocationModule.GetNewGridCollection() is not supported. This module is only available from a findbox.", true);
		}

		[RequiresSTA]
		public override void TestModuleShowsAndCanSearch()
		{
			Assert("LocationModule.GetNewGridCollection() is not supported. This module is only available from a findbox.", true);
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("LocationModule.GetNewController() is not supported. This module is only available from a findbox.", true);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigLocation;
		}

		#endregion
	}
}
