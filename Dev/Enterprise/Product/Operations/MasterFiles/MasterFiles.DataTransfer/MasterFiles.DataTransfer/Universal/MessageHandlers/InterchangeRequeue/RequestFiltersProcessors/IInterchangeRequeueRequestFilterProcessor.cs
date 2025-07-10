using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	interface IInterchangeRequeueRequestFilterProcessor
	{
		ZQuery Process(IEnumerable<InterchangeRequeueRequestFilter> filters, out string errorMessage);
	}
}
