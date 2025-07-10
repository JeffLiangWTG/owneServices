
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	sealed public class QuoteFieldStateChange : IBusinessObjectFieldChangeState
	{
		public bool IsObjectInitialized(BusinessObject bizo)
		{
			return true;
		}

		public bool IsRootObject(BusinessObject bizo)
		{
			return true;
		}

		public void NotifyObjectHooked(BusinessObject bizo)
		{
			// No need to do anything with this
		}
	}

	sealed class QuoteFieldStateChangeFactory : IBusinessObjectFieldChangeStateFactory
	{
		public IBusinessObjectFieldChangeState BusinessObjectFieldChangeState => new QuoteFieldStateChange();

		public string ChildTableCodePrefix => RatingHeaderSchema.Constants.Prefix;
	}
}
