using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class DummyUltimateDistributeeWithVariableGst : DummyIUltimateDistributee
	{
		public DummyUltimateDistributeeWithVariableGst(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		public override ZDecimal CalculateOwnGstVatRate() => 0.69m;  // 69%

		public override ZBool IsCapableOfCalculatingOwnGstVatRate => true;
	}
}
