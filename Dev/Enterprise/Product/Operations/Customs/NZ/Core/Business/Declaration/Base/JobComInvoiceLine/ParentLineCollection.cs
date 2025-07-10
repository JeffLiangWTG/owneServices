
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ParentLineCollection : BusinessObjectCollection<JobComInvoiceLine>
	{
		public ParentLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
