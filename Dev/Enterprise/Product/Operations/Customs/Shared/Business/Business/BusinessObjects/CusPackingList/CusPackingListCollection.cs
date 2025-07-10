using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	[ModuleID(ModuleId.CusPackingList)]
	public class CusPackingListCollection : ActiveBusinessObjectCollection<CusPackingList>
	{
		public CusPackingListCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
