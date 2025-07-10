using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsGlowServicesModuleService
	{
		string CreateAdHocServiceJob(ZGuid jobPK, ZGuid clientPK, ZGuid warehousePK, ZDate billingDate, ZString? customerReference);

		string CreateWhsJobService(ZGuid servicePK, ZString serviceType, ZDecimal serviceCount, ZGuid jobPK, ZString jobType, ZDateTimeOffset? bookedDateTimeOffset, ZGuid contractor, ZDateTime? duration, ZGuid locationPK, ZString subLocation, ZString reference, ZString? note);

		string CompleteWhsJobService(ZGuid servicePK, ZString serviceType, ZDecimal serviceCount, ZDateTimeOffset? bookedDateTimeOffset, ZGuid contractor, ZDateTime? duration, ZGuid locationPK, ZString subLocation, ZString reference, ZString? note, bool finaliseServiceJob);

		string CompleteAllWhsJobServices(ZGuid jobPK, ZString jobType, bool finaliseServiceJob);
	}
}
