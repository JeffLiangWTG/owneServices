using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class STWMessageCollection : Enterprise.Messaging.Business.EDIMessageCollection
	{
		public STWMessageCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{ }

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.StowPlan);
			return result;
		}
	}
}
