using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffAdditionalCode : AutoRefCusTariffAdditionalCode, ITariffDataGroupingRelatedBusinessObject
	{
		public RefCusTariffAdditionalCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => ZY2_ZZZ_NKDataGrouping;
	}
}
