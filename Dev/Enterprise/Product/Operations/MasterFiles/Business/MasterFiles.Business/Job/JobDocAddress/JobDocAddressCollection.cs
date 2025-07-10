using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressCollection : BusinessObjectCollection<JobDocAddress>
	{
		public JobDocAddressCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
