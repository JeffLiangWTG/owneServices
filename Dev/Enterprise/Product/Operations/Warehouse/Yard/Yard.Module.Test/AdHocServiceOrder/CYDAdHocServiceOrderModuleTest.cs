using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderModule))]
	public class CYDAdHocServiceOrderModuleTest : CYDModuleTest<CYDAdHocServiceOrderModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDAdHocServiceOrderFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDAdHocServiceOrderFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDAdHocServiceOrderCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDAdHocServiceOrder;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDAdHocServiceOrder)factory.NewWithValidTestData(businessObjectType);
			businessObject.YAO_WW_Facility = YardInCurrentBranch.PK;
			return businessObject;
		}
	}
}
