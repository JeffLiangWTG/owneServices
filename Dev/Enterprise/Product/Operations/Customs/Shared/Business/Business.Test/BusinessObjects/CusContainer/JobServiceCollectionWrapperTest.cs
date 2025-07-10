using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseCusContainer;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobServiceCollectionWrapper))]
	sealed class JobServiceCollectionWrapperTest : BusinessObjectCollectionViewTestCase<JobServiceCollectionWrapper>
	{
		protected override JobServiceCollectionWrapper GetCollectionToTest()
		{
			var container = Factory.New<CommonContainer>();
			return new JobServiceCollectionWrapper(container.Services);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobService>();
		}
	}
}
