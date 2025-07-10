using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRSurveyModule))]
	public class MNRSurveyModuleTest : CYDModuleTest<MNRSurveyModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(MNRSurveyFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(MNRSurveyFilterControl);

		protected override Type ExpectedCollectionType => typeof(MNRSurveyCollection);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.MNRSurvey;

		protected override bool ExpectedAllowView => true;

		protected override bool ExpectedAllowEdit => true;
	}
}
