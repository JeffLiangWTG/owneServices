using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	class RimDataRecordNumberStrategy : IDataRecordNumberStrategy
	{
		public string GetNextFormatted(BusinessObjectFactory factory)
		{
			return $"WTG-{Env.CurrentCompany.Code}-{Env.NumberFountains.TelematicsRimDataBatchNumber.GetNext(factory):D10}";
		}
	}
}
