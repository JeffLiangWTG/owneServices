using Enterprise.HRM.Common;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing.RemunerationReview.Workflow
{
	[TestedType(typeof(ReviewProcessNodeProcessTaskCollection))]
	class ReviewProcessNodeProcessTaskCollectionTest : ProcessTaskCollectionTest<ReviewProcessNodeProcessTaskCollection>
	{
		protected override ReviewProcessNodeProcessTaskCollection GetCollectionToTestCore()
			=> new ReviewProcessNodeProcessTaskCollection(Factory.NewWithValidTestData<ReviewProcessNode>());
	}
}
