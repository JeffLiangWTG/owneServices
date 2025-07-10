using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDReceiveAdviceModule))]
	public class CYDReceiveAdviceModuleTest : CYDModuleTest<CYDReceiveAdviceModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDReceiveAdviceFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDReceiveAdviceFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDReceiveAdviceCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDReceiveAdvice;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDReceiveAdvice)factory.NewWithValidTestData(businessObjectType);
			businessObject.YRA_WW_Yard = YardInCurrentBranch.PK;
			return businessObject;
		}
	}
}
