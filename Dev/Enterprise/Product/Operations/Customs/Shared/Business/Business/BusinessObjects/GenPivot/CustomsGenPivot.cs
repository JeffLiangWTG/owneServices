using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CustomsGenPivot : GenPivot
	{
		protected CustomsGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }
	}
}
