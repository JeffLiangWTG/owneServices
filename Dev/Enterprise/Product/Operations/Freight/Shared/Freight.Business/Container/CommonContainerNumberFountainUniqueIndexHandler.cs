using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonContainerNumberFountainUniqueIndexHandler : NumberFountainUniqueIndexFailureHandler
	{
		public CommonContainerNumberFountainUniqueIndexHandler(CommonContainer container)
			: base(JobContainerSchema.Constants.Indexes.NR_UX__JC_ContainerJobID, container)
		{
		}

		protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.JobContainerJobID;
	}
}
