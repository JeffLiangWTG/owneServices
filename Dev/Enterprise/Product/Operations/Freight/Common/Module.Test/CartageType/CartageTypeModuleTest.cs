using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Module.Testing
{
	[TestedType(typeof(CartageTypeModule))]
	sealed class CartageTypeModuleTest : ZModuleBasherTest
	{
		#region TestAllowNewIsEnabled

		public void TestAllowNewIsEnabled()
		{
			using (CartageTypeModule module = (CartageTypeModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("AllowNew", true, module.AllowNew);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CartageType;
		}

		#endregion

		#region TestFilterControl

		public void TestFilterControl()
		{
			using (cartageType = new CartageTypeModule())
			{
				using (IDisposable controlForTest = cartageType.GetNewFilterControlInternal())
				{
					Assert(controlForTest is LocalCartageJobTypeFilterControl);
				}
			}
		}

		#endregion

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (cartageType = new CartageTypeModule())
			{
				Assert(cartageType.GetNewGridCollectionInternal() is ActiveBusinessObjectCollection<CommonCartageType>);
			}
		}

		#endregion

		#region TestFilterBusinessObject

		public void TestFilterBusinessObject()
		{
			using (cartageType = new CartageTypeModule())
			{
				Assert(cartageType.GetNewFilterBusinessObjectInternal() is FilterBusinessObject);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = new CartageTypeModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region Implementation

		CartageTypeModule cartageType;

		#endregion
	}
}
