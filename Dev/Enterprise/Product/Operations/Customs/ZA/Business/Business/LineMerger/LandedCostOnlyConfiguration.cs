using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	class LandedCostOnlyConfiguration : ILandedCostOnlyConfiguration
	{
		public Func<Customs.Business.CusEntryLine, string, bool> GetIsLandedCostingOnlyFuncForEntryLine()
		{
			return delegate(Customs.Business.CusEntryLine entryLine, string feeType)
			{
				var result = false;

				var procedure = (entryLine as CusEntryLine)?.CusProcedure;
				if (procedure != null)
				{
					result = !procedure.ZZ6_CalculateDuty && procedure.ZZ6_LandedCost;
				}

				return result;
			};
		}

		public Func<Customs.Business.CusEntryHeader, string, bool> GetIsLandedCostingOnlyFuncForEntryHeader() => (x, y) => false;
	}
}
