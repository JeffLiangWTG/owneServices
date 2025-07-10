using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionDutyRate : AutoNZCConcessionDutyRate
	{
		public NZCConcessionDutyRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
