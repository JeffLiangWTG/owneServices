using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesRepModule))]
	sealed class TestSalesRepModule : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SalesRep;
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var salesRep = (GlbStaff)factory.NewWithValidTestData(businessObjectType);
			salesRep.GS_IsSalesRep = true;

			return salesRep;
		}

		#endregion
	}
}
