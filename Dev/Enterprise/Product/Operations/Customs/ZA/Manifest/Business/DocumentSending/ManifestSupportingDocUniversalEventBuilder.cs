using System.Collections.Generic;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.DocumentSending;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ManifestSupportingDocUniversalEventBuilder : ZASupportingDocUniversalEventBuilder
	{
		public ManifestSupportingDocUniversalEventBuilder(ISupportingDocumentMessageDataProvider dataWrapper) : base(dataWrapper)
		{
		}

		protected override void PopulateContextCollection(IeDoc eDoc, UniversalEvent universalEvent)
		{
			universalEvent.ContextCollection = new List<Context>
				{
					new Context { Type = FileName, Value = eDoc.FileName }
				};
		}

		const string FileName = "FileName";
	}
}
