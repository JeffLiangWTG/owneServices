using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ScheduleTransportLegBusinessObjectFinder : MatchingBusinessObjectFinder<TransportLeg, JobSailing>
	{
		public ScheduleTransportLegBusinessObjectFinder(TransportLeg dataObject, JobVoyage voyage)
			: base(dataObject)
		{
			this.voyage = Argument.NotNull(voyage, nameof(voyage));
		}

		readonly JobVoyage voyage;

		#region Implementation

		protected override JobSailing FindCore(IEnumerable<JobSailing> sailings)
		{
			var result = sailings.FirstOrDefault(s => s.JX_JA_RL_NKPortOfLoading == dataObject.PortOfLoading.GetUNLOCOAsUpperCase(voyage.Factory)
				&& s.JX_JB_RL_NKPortOfDischarge == dataObject.PortOfDischarge.GetUNLOCOAsUpperCase(voyage.Factory));

			return result;
		}

		#endregion
	}
}
