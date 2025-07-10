using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCAffirmationOfComplianceCollection : BusinessObjectCollection<USCAffirmationOfCompliance>
	{
		public USCAffirmationOfComplianceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
