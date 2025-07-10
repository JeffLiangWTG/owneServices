using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	interface IDataRecordNumberStrategy
	{
		string GetNextFormatted(BusinessObjectFactory factory);
	}
}
