using CargoWise.EntityFramework;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing.RemunerationReview.Workflow
{
	[TestedType(typeof(ReviewProcessNodeProcessTask))]
	class ReviewProcessNodeProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
			=> Factory.New<ReviewProcessNode>().WorkflowItems.AddNew();
	}
}
