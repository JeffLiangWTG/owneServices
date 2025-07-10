using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTaxOrFeeLanguage : AutoRefCusTaxOrFeeLanguage
	{
		public RefCusTaxOrFeeLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
