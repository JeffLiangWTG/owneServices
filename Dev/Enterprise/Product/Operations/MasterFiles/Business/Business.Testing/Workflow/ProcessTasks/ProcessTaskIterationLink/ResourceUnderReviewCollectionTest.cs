using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ResourceUnderReviewCollection))]
	class ResourceUnderReviewCollectionTest : ActiveBusinessObjectCollectionTestCase<ResourceUnderReviewCollection>
	{
		protected override ResourceUnderReviewCollection GetCollectionToTest()
		{
			var task = Factory.New<ProcessTask>();
			return new ResourceUnderReviewCollection(task, Factory);
		}
	}
}
