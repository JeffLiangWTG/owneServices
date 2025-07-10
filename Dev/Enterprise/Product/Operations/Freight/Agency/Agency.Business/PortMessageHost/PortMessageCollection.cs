using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageCollection : EDIMessageCollection
	{
		public PortMessageCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{ }

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.PortAuthority);
			return result;
		}
	}
}
