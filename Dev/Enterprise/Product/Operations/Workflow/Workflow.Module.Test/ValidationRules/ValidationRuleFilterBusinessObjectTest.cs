using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(ValidationRuleFilterBusinessObject))]
	class ValidationRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Impl

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ValidationRuleFilterBusinessObject();

		#endregion
	}
}
