using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Messaging.Business;
using Sixteen = Enterprise.Edifact.D16A.Messages.CUSCAR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR
{
	public class CUSCARMessageBuilder : EDIFACTMessageBuilder<ICusCarHeader, Sixteen.CUSCARMessage, CUSCAREDIMessage>, ICustomsMessageGenerator
	{
		public CUSCARMessageBuilder(ICusCarHeader source, string messageSubType, ZString billIssuerCode)
			: base(source, GetMessageSubTypeEnumFromString(messageSubType), new ZACharacterSet())
		{
			this.dataSource = source;
			this.billIssuerCode = billIssuerCode;
			this.messageSubTypeString = messageSubType;
		}

		protected override void PopulateEdifactMessage()
		{
			GetNewBuilder(dataSource.ManifestDocumentType)?.Create();
		}

		CUSCARMessageTextBuilder GetNewBuilder(ManifestDocumentType manifestType)
		{
			switch (manifestType) // COM, COH, etc
			{
				case ManifestDocumentType.BBB:
					return new CUSCARMessageTextBuilder_BBB(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.COM:
					return new CUSCARMessageTextBuilder_COM(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.COH:
					return new CUSCARMessageTextBuilder_COH(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.ECL:
					return new CUSCARMessageTextBuilder_ECL(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.HAB:
					return new CUSCARMessageTextBuilder_HAB(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.RFM:
					return new CUSCARMessageTextBuilder_RFM(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.FWB:
					return new CUSCARMessageTextBuilder_FWB(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.ALH:
					return new CUSCARMessageTextBuilder_ALH(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.RMA:
					return new CUSCARMessageTextBuilder_RMA(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.FFM:
					return new CUSCARMessageTextBuilder_FFM(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.ALM:
					return new CUSCARMessageTextBuilder_ALM(edifactMessage, dataSource, messageSubType, billIssuerCode);
				case ManifestDocumentType.AQM:
					return new CUSCARMessageTextBuilder_AQM(edifactMessage, dataSource, messageSubType, billIssuerCode);
				default:
					return null;
			}
		}

		protected override ZString GetMessageSubType() => messageSubTypeString;

		public static MessageSubTypes GetMessageSubTypeEnumFromString(string messageSubType)
		{
			switch (messageSubType)
			{
				case MessageSubTypeCodes.Codes.Change:
					return MessageSubTypes.Change;
				case MessageSubTypeCodes.Codes.Original:
					return MessageSubTypes.Create;
				case MessageSubTypeCodes.Codes.Cancellation:
					return MessageSubTypes.Withdraw;
				default:
					return MessageSubTypes.Undefined;
			}
		}

		readonly ICusCarHeader dataSource;
		readonly ZString billIssuerCode;
		readonly string messageSubTypeString;

		public static ZBool IsManifestTypeSupported(ManifestDocumentType manifestType) => new[] {
			ManifestDocumentType.BBB,
			ManifestDocumentType.COM,
			ManifestDocumentType.COH,
			ManifestDocumentType.ECL,
			ManifestDocumentType.HAB,
			ManifestDocumentType.RFM,
			ManifestDocumentType.FWB,
			ManifestDocumentType.ALH,
			ManifestDocumentType.RMA,
			ManifestDocumentType.FFM,
			ManifestDocumentType.ALM,
			ManifestDocumentType.AQM }.Contains(manifestType);

		EDIMessage ICustomsMessageGenerator.GenerateMessage()
		{
			var msg = PopulateMessagesReturningResult();

			return msg;
		}
	}
}
