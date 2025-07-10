using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffNationalCode : AutoRefCusTariffNationalCode
	{
		public RefCusTariffNationalCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
