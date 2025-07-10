using CargoWise.EntityFramework;

using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PRAMessageCollection : EDIMessageCollection<PRAMessage>
	{
		public PRAMessageCollection(BusinessObject master, BusinessObjectFactory factory) : base(master, factory) { }

		public PRAMessageCollection(BusinessObject master) : base(master) { }

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.OneStop);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, PRAMessage.MessageType);
			return filter;
		}
	}
}
