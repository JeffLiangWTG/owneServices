using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public interface IScheduleValidationProvider
	{
		JobVoyageValidation GetExtraVoyageValidation(JobVoyage voyage);
	}
}
