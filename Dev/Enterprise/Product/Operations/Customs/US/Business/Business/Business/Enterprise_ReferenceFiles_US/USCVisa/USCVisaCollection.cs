using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCVisaCollection : BusinessObjectCollection<USCVisa>
	{
		public USCVisaCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
