using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRWorkOrderModule))]
	public class MNRWorkOrderModuleTest : CYDModuleTest<MNRWorkOrderModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(MNRWorkOrderFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(MNRWorkOrderFilterControl);

		protected override Type ExpectedCollectionType => typeof(MNRWorkOrderHeaderCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.MNRWorkOrder;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (MNRWorkOrderHeader)factory.NewWithValidTestData(businessObjectType);
			businessObject.MWO_ParentID = factory.NewWithValidTestData(typeof(CYDYardUnitState)).PK;
			businessObject.YardUnitState.YUS_YRL_ReceiveLine = factory.NewWithValidTestData(typeof(CYDReceiveAdviceLine)).PK;
			businessObject.YardUnitState.ReceiveAdviceLine.YRL_YRA_ReceiveAdvice = factory.NewWithValidTestData(typeof(CYDReceiveAdvice)).PK;
			businessObject.MWO_WW_Facility = YardInCurrentBranch.PK;
			return businessObject;
		}
	}
}
