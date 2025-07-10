using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTariffTypeLanguage : AutoRefCusTariffTypeLanguage
	{
		public RefCusTariffTypeLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
