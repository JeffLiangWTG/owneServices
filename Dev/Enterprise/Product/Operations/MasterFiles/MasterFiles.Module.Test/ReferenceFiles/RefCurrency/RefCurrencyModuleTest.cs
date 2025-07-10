using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCurrencyModule))]
	public class RefCurrencyModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCurrency;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (refCurrency = new RefCurrencyModuleForTest())
			{
				using (IDisposable controlForTest = refCurrency.GetNewFilterControlForTest())
				{
					Assert(controlForTest is RefCurrencyFilterControl);
				}
			}
		}

		public void TestGridCollection()
		{
			using (refCurrency = new RefCurrencyModuleForTest())
			{
				Assert(refCurrency.GetNewGridCollectionForTest() is ActiveBusinessObjectCollection<RefCurrency>);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (refCurrency = new RefCurrencyModuleForTest())
			{
				Assert(refCurrency.GetNewFilterBusinessObjectForTest() is FilterBusinessObject);
			}
		}

		#region Implementation

		RefCurrencyModuleForTest refCurrency;

		#endregion
	}
}
