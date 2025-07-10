using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class ACASHouseChecklistMessagingExtensions : BaseMessagingExtensions
	{
		public ACASHouseChecklistMessagingExtensions(IDocument document)
		{
			this.document = document ?? throw new ArgumentNullException(nameof(document));
		}

		readonly IDocument document;

		#region Message Status

		public override string GetMessageStatus()
		{
			if (document.Data is IDynamicData dynamicData
				&& dynamicData.Value is ACASHouseChecklist acas)
			{
				return acas.DisplayInformation;
			}

			return null;
		}

		#endregion
	}
}
