using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAttribute : AutoRefCusTariffAttribute
	{
		public RefCusTariffAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
