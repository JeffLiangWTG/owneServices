using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class StorageAndShippingConditionJobComInvLineRefsCollection : JobComInvLineRefsCollection<StorageAndShippingConditionJobComInvLineRefs>
	{
		public StorageAndShippingConditionJobComInvLineRefsCollection(BusinessObject parent)
			: base(parent, JobComInvLineRefsType.Codes.StorageAndShippingCondition)
		{
		}
	}
}
