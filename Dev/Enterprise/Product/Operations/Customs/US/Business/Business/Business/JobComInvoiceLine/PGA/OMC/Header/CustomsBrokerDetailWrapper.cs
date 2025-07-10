using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public class CustomsBrokerDetailWrapper : ICustomsBrokerDetails
	{
		public CustomsBrokerDetailWrapper(IAddressDetails address, ZString contactName, ZString contactPhone, ZString contactEmail)
		{
			this.Address = address;
			this.ContactName = contactName;
			this.ContactPhone = contactPhone;
			this.ContactEmail = contactEmail;
		}

		public IAddressDetails Address { get; set; }
		public ZString ContactName { get; set; }
		public ZString ContactPhone { get; set; }
		public ZString ContactEmail { get; set; }
	}
}
