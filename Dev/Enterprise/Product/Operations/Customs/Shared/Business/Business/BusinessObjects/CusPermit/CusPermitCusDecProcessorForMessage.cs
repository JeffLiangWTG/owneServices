using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.BusinessObjects.CusPermit
{
	public abstract class CusPermitCusDecProcessorForMessage : CusPermitCusDecProcessor<EDIMessage>
	{
		protected CusPermitCusDecProcessorForMessage(IAllowPermitProcessing header, ZString messageType) : base(header)
		{
			this.messageType = messageType;
		}

		protected readonly ZString messageType;

		protected override bool ShouldAddPermitRecordsAndLockMutexIfNeeded() => messageType != MessageSubTypeCodes.Codes.Cancellation;

		protected override PermitRecord GetLastSentPermitRecord(IAllowPermitProcessing header, ZString permitNumber)
		{
			PermitRecord lastSentPermitRecord = null;
			if (MessageTypes.Contains(messageType))
			{
				if (header.Messages.GetLastMessage(ApplicationCode) is EDIMessage outgoingMessage)
				{
					var lastSentPermitRecords = GetPermitRecords(outgoingMessage);
					lastSentPermitRecord = lastSentPermitRecords?.FirstOrDefault(x =>
					{
						var no = x.PermitHeader?.CPH_Number ?? ZString.Empty;
						return no == permitNumber;
					});
				}
			}
			return lastSentPermitRecord;
		}

		protected abstract IList<PermitRecord> GetPermitRecords(EDIMessage message);

		protected virtual ZString ApplicationCode => ZString.Empty;

		protected virtual ISet<ZString> MessageTypes => new HashSet<ZString>(new ZString[] { MessageSubTypeCodes.Codes.Change });
	}
}
