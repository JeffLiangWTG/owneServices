using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class RefCusProcedureLoader
	{
		public RefCusProcedureLoader(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public RefCusProcedure GetProcedure(ZString procedureCode, ZString previousProcedure, ZString dataGroupingCode)
		{
			RefCusProcedure result = null;
			if (!procedureCode.IsEmpty)
			{
				var ppc = previousProcedure.SubstringSafe(2, 2).Trim();
				if (ppc == "")
				{
					ppc = "00";
				}
				result = new RefCusProcedure.Loader(factory).LoadTop1FromCodeAndCountry(procedureCode, ppc, dataGroupingCode, ZDateTime.Today);
			}

			return result;
		}
	}
}
