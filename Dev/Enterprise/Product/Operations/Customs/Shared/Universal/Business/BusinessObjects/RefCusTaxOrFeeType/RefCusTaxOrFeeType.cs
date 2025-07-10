using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTaxOrFeeType : AutoRefCusTaxOrFeeType
	{
		public RefCusTaxOrFeeType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
