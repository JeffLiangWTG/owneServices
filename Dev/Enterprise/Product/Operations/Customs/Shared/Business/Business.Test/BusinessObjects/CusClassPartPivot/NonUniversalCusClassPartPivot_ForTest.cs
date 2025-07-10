using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class NonUniversalCusClassPartPivot_ForTest : BaseCusClassPartPivot
	{
		public NonUniversalCusClassPartPivot_ForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		protected internal override bool UseUniversalTariff => false;
	}
}
