using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.LVS.Business
{
	class CusUSLVConsignmentAdhocEdocsSupportCollection : BusinessObjectCollection<CusUSLVConsignment>
	{
		public CusUSLVConsignmentAdhocEdocsSupportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
