using System;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Freight.Agency.ServiceTasks
{
	internal sealed class PortAuthorityInterchangeProvider : InterchangeProviderBase
	{
		public PortAuthorityInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return Res.GetString("5caa776b-b9f5-4e80-b968-169dd80c67cc", "You need to set the Sender ID from the registry item: Liner & Agency -> Port Authority -> Settings"); }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			EDIMessage message = messages[0];
			ISailingEndPoint endPoint = MessagingHelper.GetEndPointFromMessage(message);

			if (endPoint == null)
			{
				message.Delete();
			}
			else
			{
				PortAuthoritySetting setting = MessagingHelper.GetPortAuthoritySettingFromEndPoint(endPoint, message.EM_ApplicationReference);
				SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, setting.RecipientID, setting.SenderID);
				interchange.EI_HeaderText = GetUNB(PreparedTime.ToZDateTime(), setting.SenderID, setting.RecipientID);
			}
		}

		protected override void BuildInterchangeBatchesCore(NonDependentEDIMessageCollection messages)
		{
			foreach (EDIMessage message in messages)
			{
				NonDependentEDIMessageCollection batch = new NonDependentEDIMessageCollection(messages.Factory);
				batch.Add(message);
				AddMessageCollection(batch);
			}
		}

		protected override Type InterchangeType
		{
			get { return typeof(PortAuthorityInterchange); }
		}

		ZString GetUNB(ZDateTime preparedTime, ZString senderID, ZString recipientID)
		{
			return GetUNB(preparedTime,
				recipientID, ZString.Empty, senderID, ZString.Empty,
				ZString.Empty, "UNOA", "4", ZString.Empty, false, false, ZString.Empty
				).ToString(new UNOACharacterSet());
		}
	}
}


