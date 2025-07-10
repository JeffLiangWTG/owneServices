using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderCollection : ActiveBusinessObjectCollection<CusISFHeader>
	{
		public CusISFHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
