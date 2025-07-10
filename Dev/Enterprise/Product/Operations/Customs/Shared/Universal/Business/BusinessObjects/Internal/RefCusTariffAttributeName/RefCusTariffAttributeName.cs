using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAttributeName : AutoRefCusTariffAttributeName
	{
		public RefCusTariffAttributeName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
