using System;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(ProductStyleModule))]
	class ProductStyleModuleTest : ZModuleBasherTest
	{
		#region TestFlags

		public void TestFlags()
		{
			using (var module = new ProductStyleModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
				AssertEquals(true, module.AllowNew);
				AssertEquals(true, module.AllowDelete);
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestExpectedFilterBusinessObjectType()
		{
			using (var module = new ProductStyleModule())
			{
				AssertEquals(true, module.FilterBusinessObject.GetType().IsAssignableFrom(typeof(ProductStyleFilterBusinessObject)));
			}
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControlType()
		{
			using (var module = new ProductStyleModule())
			{
				using (var controlForTest = (IDisposable)module.GetNewFilterControlForGrid())
				{
					AssertEquals(true, controlForTest.GetType().IsAssignableFrom(typeof(ProductStyleFilterControl)));
				}
			}
		}

		#endregion

		#region TestGridCollection

		public void TestCollectionType()
		{
			using (var module = new ProductStyleModule())
			{
				AssertEquals(true, module.GridCollection.GetType().IsAssignableFrom(typeof(WhsProductStyleCollection)));
			}
		}

		#endregion

		#region TestLicenceCheckpoint

		public void TestLicenceCheckpoint()
		{
			using (var module = new ProductStyleModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckpoint

		public void TestSecurityCheckpoint()
		{
			using (var module = new ProductStyleModule())
			{
				AssertEquals(Env.Security.WhsConfigProductStyle, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestSupportsWorkflow

		public void TestSupportsWorkflow()
		{
			using (var module = new ProductStyleModule())
			{
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigProductStyle;
		}

		#endregion
	}
}
