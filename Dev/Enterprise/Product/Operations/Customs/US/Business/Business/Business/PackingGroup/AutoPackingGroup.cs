using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public abstract class AutoPackingGroup : Customs.Business.BasePackingGroup
	{
		protected AutoPackingGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
