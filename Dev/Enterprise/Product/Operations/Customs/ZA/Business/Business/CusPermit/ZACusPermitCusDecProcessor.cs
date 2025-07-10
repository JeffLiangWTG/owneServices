using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ZACusPermitCusDecProcessor : CusPermitCusDecProcessorForMessage
	{
		public ZACusPermitCusDecProcessor(CusEntryHeader header, ZString messageType) : base(header, messageType)
		{
		}

		protected CusEntryHeader ZAEntryHeader => (CusEntryHeader)header;

		protected override PermitRecord GetLastSentPermitRecord(IAllowPermitProcessing header, ZString permitNumber)
		{
			PermitRecord lastSentPermitRecord = null;
			if (MessageTypes.Contains(messageType))
			{
				if (((CusEntryHeader)header).LastSentCUSDECMessage is CUSDECEDIMessage outgoingMessage)
				{
					var lastSentPermitRecords = ZAPermitHelper.GetPermitRecords(outgoingMessage);
					lastSentPermitRecord = lastSentPermitRecords?.FirstOrDefault(x =>
					{
						var no = x.PermitHeader?.CPH_Number ?? ZString.Empty;
						return no == permitNumber;
					});
				}
			}
			return lastSentPermitRecord;
		}

		protected override IList<PermitRecord> GetPermitRecords() => ZAPermitHelper.GetPermitRecords(ZAEntryHeader);

		protected override IList<PermitRecord> GetPermitRecords(EDIMessage message) => ZAPermitHelper.GetPermitRecords((CUSDECEDIMessage)message);

		protected override ISet<ZString> MessageTypes => new HashSet<ZString>(new ZString[] { MessageSubTypeCodes.Codes.Change, MessageSubTypeCodes.Codes.Replace });

		protected override bool AllowNegativeAdjustments => true;
	}
}
