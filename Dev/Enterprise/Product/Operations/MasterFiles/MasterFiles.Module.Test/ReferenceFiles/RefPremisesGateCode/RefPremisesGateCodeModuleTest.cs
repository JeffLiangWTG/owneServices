using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPremisesGateCodeModule))]
	sealed class RefPremisesGateCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefPremisesGateCode;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			refPremisesGateCode = new RefPremisesGateCodeModuleForTest();
			IFilterControl controlForTest = refPremisesGateCode.GetNewFilterControlForTest();
			Assert(controlForTest is RefPremisesGateCodeFilterControl);
			controlForTest.Dispose();
			refPremisesGateCode.Dispose();
		}

		public void TestGridCollection()
		{
			refPremisesGateCode = new RefPremisesGateCodeModuleForTest();
			IBusinessObjectCollection collectionForTest = refPremisesGateCode.GetNewGridCollectionForTest();
			Assert(collectionForTest is ActiveBusinessObjectCollection<RefPremisesGateCode>);
			refPremisesGateCode.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			refPremisesGateCode = new RefPremisesGateCodeModuleForTest();
			FilterBusinessObject businessForTest = refPremisesGateCode.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			refPremisesGateCode.Dispose();
		}

		#region Implementation

		RefPremisesGateCodeModuleForTest refPremisesGateCode;

		#endregion

		#region Additional Tests

		public void TestGridSecurityCheckpoint()
		{
			AssertEquals("Same objects expected", Env.Security.PremisesGateCode, Module.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals("Same objects expected", Env.Licence.Core, Module.LicenceCheckPoint);
		}

		#region Implementation

		RefPremisesGateCodeModule Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new RefPremisesGateCodeModule();
				}
				return fModule;
			}
		}

		RefPremisesGateCodeModule fModule;

		protected override void TearDown()
		{
			base.TearDown();
			if (fModule != null)
			{
				fModule.Dispose();
			}
		}

		#endregion

		#endregion
	}
}
