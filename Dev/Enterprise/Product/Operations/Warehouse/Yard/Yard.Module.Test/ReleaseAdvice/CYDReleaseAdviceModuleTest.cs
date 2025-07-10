using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDReleaseAdviceModule))]
	public class CYDReleaseAdviceModuleTest : CYDModuleTest<CYDReleaseAdviceModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(CYDReleaseAdviceFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CYDReleaseAdviceFilterControl);

		protected override Type ExpectedCollectionType => typeof(CYDReleaseAdviceCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CYDReleaseAdvice;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var businessObject = (CYDReleaseAdvice)factory.NewWithValidTestData(businessObjectType);
			businessObject.YRE_WW_Yard = YardInCurrentBranch.PK;
			return businessObject;
		}
	}
}
