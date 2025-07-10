using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.DataTransfer
{
	public class NCATKUniversalMessagingHelper : JobDeclarationUniversalMessagingHelper
	{
		public NCATKUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent messageSendingObject)
			: base(messageSendingObject)
		{
		}

		protected override ZString UniversalCustomsMessagingRecipientID => MessageConstants.TWCustoms;

		protected override ZString InstructionForRecipientIDSetup => ZString.Empty;

		protected override ZBool UseMessageNumberAsRerenceNumber => true;

		protected override ZString GetMessageTypeForEventReference(BusinessObject header)
		{
			var result = ZString.Empty;
			if (header is CusTWControllingMessageHeader messageHeader)
			{
				result = messageHeader.TW1_ControllingMessageType;
			}
			return result;
		}

		protected override IEnumerable<BusinessObject> GetSelectedMessageSendingObjects() => MessageSendingObjectParent.SendingObjectsCollection.Cast<NCATKMessageSendingObject>().Where(obj => obj.ShouldSend).Select(obj => obj.Header);

		protected override void UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(BusinessObject businessObject, ZString status)
		{
			LogDeclarationEvent(businessObject);
		}

		void LogDeclarationEvent(BusinessObject businessObject)
		{
			if (MessageSendingObjectParent is NCATKMessageSendingObjectParent messageHeader)
			{
				var menuCaption = messageHeader.MenuCaption;
				if (!menuCaption.IsEmpty && businessObject is CusTWControllingMessageHeader)
				{
					messageHeader.Declaration.Logs.AddNew(Events.MessageSent, menuCaption);
				}
			}
		}

		protected override NonPersistentEDICommunicationMode GetEDICommunicationMode(BusinessObject header, ZString destination)
		{
			var mode = base.GetEDICommunicationMode(header, destination);
			if (header is CusTWControllingMessageHeader messageHeader)
			{
				mode.EK_Filename = GetFileName(messageHeader);
			}
			return mode;
		}

		ZString GetFileName(CusTWControllingMessageHeader messageHeader)
		{
			var currentTime = ZDateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
			var messageType = messageHeader.TW1_ControllingMessageType;
			var namePart3 = messageHeader.TW1_CertificateType;
			switch (messageType)
			{
				case ControllingMessageTypeList.Codes.NX401:
				case ControllingMessageTypeList.Codes.NX601:
					namePart3 = messageHeader.TW1_BusinessType;
					break;
			}
			return FormattableString.Invariant($"{messageType}.{namePart3}.{messageHeader.TW1_FunctionalReferenceId}.{currentTime}.xml");
		}

		protected override DeclarationDataObjectWriterConfiguration GetDeclarationDataObjectWriterConfiguraion(BusinessObject header)
		{
			return new NCATKDataObjectWriterConfiguration(header, MessageSendingObjectParent);
		}
	}
}
