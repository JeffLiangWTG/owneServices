using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module
{
	public class USCAffirmationOfComplianceCollection : BusinessObjectCollection<USCAffirmationOfCompliance>
	{
		public USCAffirmationOfComplianceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
