using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.Business
{
	public sealed class EBondEDIMessage : BaseEDIMessage
	{
		public EBondEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = ApplicationCodeList.Codes.USeBond;
			EM_MessageType = EDIInterchangeTypeList.Codes.XDC;
			EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override ZString MessageTypeDescriptionCore => EM_MessageType == EDIInterchangeTypeList.Codes.XDC
			? IsTransmitMessage ? (ZString)"eBond Request" : (ZString)"eBond Response"
			: EM_MessageSubTypeDescription;

		public override bool IsInterpretationInHtmlFormat => !IsTransmitMessage;

		public override bool HasStatusOrErrors => false;

		protected override void PopulateMessageNumber()
		{
			PopulateNumberPropertyIfRequired(EM_MessageNumInfo, x => GetNewMessageNumber(x));
		}

		ZString GetNewMessageNumber(BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			if (MessageNumberStrategy != null)
			{
				result = MessageNumberStrategy.GetMessageReferenceNumber();
			}
			else
			{
				result = Env.NumberFountains.XmlEDIMessageNumber.GetNextFormatted(factory);
			}
			return result;
		}

		public override void OnSaving()
		{
			if (EM_MessageNum.IsEmpty && !IsInDatabase)
			{
				PopulateMessageNumber();
			}

			base.OnSaving();
		}
	}
}
