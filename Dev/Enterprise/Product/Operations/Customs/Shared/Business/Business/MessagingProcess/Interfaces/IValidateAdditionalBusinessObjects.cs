using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface IValidateAdditionalBusinessObjects
	{
		IReadOnlyCollection<BusinessObject> AdditionalBusniessObjectsToValidate { get; }
	}
}
