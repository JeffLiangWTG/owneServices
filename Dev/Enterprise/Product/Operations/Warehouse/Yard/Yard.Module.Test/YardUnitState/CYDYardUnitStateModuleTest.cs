using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDYardUnitStateModule))]
	public class CYDYardUnitStateModuleTest : CYDModuleTest<CYDYardUnitStateModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDYardUnitStateFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDYardUnitStateFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDYardUnitStateCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDYardUnitState;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDYardUnitState)factory.NewWithValidTestData(businessObjectType);
			businessObject.YUS_WW_CurrentYard = YardInCurrentBranch.PK;
			return businessObject;
		}
	}
}
