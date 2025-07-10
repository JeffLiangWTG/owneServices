using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTariffAdditionalCodeLanguage : AutoRefCusTariffAdditionalCodeLanguage
	{
		public RefCusTariffAdditionalCodeLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
