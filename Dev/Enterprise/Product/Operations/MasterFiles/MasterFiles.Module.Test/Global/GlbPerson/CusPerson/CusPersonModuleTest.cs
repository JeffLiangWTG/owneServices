using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CusPersonModule))]
	sealed class CusPersonModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CusPerson;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var glbPerson = factory.NewWithValidTestData<GlbPerson>();
			glbPerson.PER_FullName = filterStripHelperTestPersonName;

			return glbPerson;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (ModuleTextFilter)filterBusinessObject["Full Name"];
			filter.IsActive = true;
			filter.Property = filterStripHelperTestPersonName;
		}

		const string filterStripHelperTestPersonName = "MODULE BASHER TEST";
	}
}
