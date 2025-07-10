using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPickupHeaderModule))]
	public class CYDPickupHeaderModuleTest : CYDModuleTest<CYDPickupHeaderModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDPickupHeaderFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDPickupHeaderFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDPickupHeaderCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDPickupHeader;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDPickupHeader)factory.NewWithValidTestData(businessObjectType);
			businessObject.YPH_WW_Yard = YardInCurrentBranch.PK;
			businessObject.YPH_IsBulkRun = true;
			businessObject.YPH_FromDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(-1);
			businessObject.YPH_ToDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(1);
			return businessObject;
		}
	}
}
