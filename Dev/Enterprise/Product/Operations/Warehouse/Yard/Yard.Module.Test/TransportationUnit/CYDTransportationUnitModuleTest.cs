using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDTransportationUnitModule))]
	public class CYDTransportationUnitModuleTest : CYDModuleTest<CYDTransportationUnitModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDTransportationUnitFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDTransportationUnitFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDTransportationUnitCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDTransportationUnit;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDTransportationUnit)factory.NewWithValidTestData(businessObjectType);
			businessObject.YTU_WW_Yard = YardInCurrentBranch.PK;
			return businessObject;
		}
	}
}
