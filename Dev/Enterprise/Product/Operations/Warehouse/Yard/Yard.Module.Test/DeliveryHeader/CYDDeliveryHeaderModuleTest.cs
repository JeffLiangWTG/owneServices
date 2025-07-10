using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDDeliveryHeaderModule))]
	public class CYDDeliveryHeaderModuleTest : CYDModuleTest<CYDDeliveryHeaderModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDDeliveryHeaderFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDDeliveryHeaderFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDDeliveryHeaderCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDDeliveryHeader;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDDeliveryHeader)factory.NewWithValidTestData(businessObjectType);
			businessObject.YDH_WW_Yard = YardInCurrentBranch.PK;
			businessObject.YDH_IsBulkRun = true;
			businessObject.YDH_FromDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(-1);
			businessObject.YDH_ToDate = (CargoWise.Types.ZDate)DateTime.Now.AddDays(1);
			return businessObject;
		}
	}
}
