
using CargoWise.EntityFramework;

using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ComTracMessageCollection : EDIMessageCollection<ComTracMessage>
	{
		public ComTracMessageCollection(BusinessObject master, BusinessObjectFactory factory) : base(master, factory) { }

		public ComTracMessageCollection(BusinessObject master) : base(master) { }

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery filter = base.CreateAdditionalFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ComTrac);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationCodeList.Codes.ComTrac);
			return filter;
		}
	}
}
