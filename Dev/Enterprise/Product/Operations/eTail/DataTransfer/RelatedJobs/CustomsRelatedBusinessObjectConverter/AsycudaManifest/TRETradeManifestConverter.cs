using System.Collections.Generic;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.eTail.DataTransfer
{
	public class TRETradeManifestConverter : AsycudaManifestConverter
	{
		public TRETradeManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override CodeDescriptionPair ManifestType => new CodeDescriptionPair { Code = TRETradeManifestTypes.Codes.TRETrade, Description = TRETradeManifestTypes.Descriptions.TRETrade };

		public override CodeDescriptionPair ManifestApplicationTypeCode => new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.TRETrade, Description = ApplicationCodeTypeList.Descriptions.TRETrade };

		protected override void CreateManifestStyleEntryInstruction(Shipment dataObject, ForwardingShipment forwardingShipment)
		{
			var headerEntry = dataObject.EntryHeaderCollection?.Count == 1 ? dataObject.EntryHeaderCollection[0] : default;
			if (headerEntry != null)
			{
				var entryInstruction = new EntryInstruction(DefaultDataObjectWriterStrategy.Instance) { Link = 1, Style = TRETradeManifestTypes.Codes.TRETrade };
				headerEntry.EntryInstructionLink = 1;
				dataObject.SetEntryInstructionCollection(() => new List<EntryInstruction> { entryInstruction });
			}
		}
	}
}
