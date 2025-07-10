using System.Collections.Generic;
using CargoWise.Common;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.Business.MessageBuilders
{
	public class SupportingDocUniversalEventBuilder
	{
		public SupportingDocUniversalEventBuilder(ISupportingDocumentMessageDataProvider dataWrapper)
		{
			DataWrapper = Argument.NotNull(dataWrapper, "dataWrapper");
		}

		protected readonly ISupportingDocumentMessageDataProvider DataWrapper;

		public virtual IEnumerable<UniversalEvent> BuildUniversalEvent(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			yield return new UniversalEvent();
		}
	}
}
