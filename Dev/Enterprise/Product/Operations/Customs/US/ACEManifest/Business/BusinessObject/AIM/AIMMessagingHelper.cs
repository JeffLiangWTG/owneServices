using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.AIM.Messaging;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMMessagingHelper
	{
		public (bool IsSuccess, string Message, Action RollbackAction) SendBillMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, IList<IMessageParent> messageParents)
		{
			var messageChooser = new MessageChooser(header, messageParents.Select(b => b.SelectionItem), false);
			messageChooser.SelectAll();
			return SendBillMessage(header, messageSubType, messageParents, messageChooser);
		}

		public (bool IsSuccess, string Message, Action RollbackAction) SendBillMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, IList<IMessageParent> messageParents, MessageChooser messageChooser)
		{
			var selectedItems = messageChooser.GetSelectedItems();
			var bills = messageParents.Where(c => selectedItems.Any(d => d.Equals(c.SelectionItem))).Cast<AsycudaBill>();
			Action rollbackAction = null;

			var reason = ZString.Empty;
			if (messageChooser is AIMMessageChooser chooser && chooser.IsChangeOrCancellation)
			{
				reason = chooser.Reason;
			}

			foreach (var bill in bills)
			{
				var messageHeader = new BillMessageHeader(bill, messageSubType);

				if (!reason.IsEmpty)
				{
					messageHeader.SetAmendmentReason(reason, string.Empty);
				}

				var message = new AIMMessageBuilder(messageHeader, bill.Factory, true).Build();
				bill.Messages.Add(message);
				message.EM_LinkedObject = bill;

				bill.ABL_MessageStatus = messageSubType == AIMMessageSubTypes.FRX || messageSubType == AIMMessageSubTypes.FXX ? MessageStatusCodeList.Codes.Cancel : MessageStatusCodeList.Codes.Sent;
				rollbackAction += () =>
				{
					message.Delete();
					bill.Logs.LogsNotInDB.ForEach(log => log.Delete());
					bill.Reload();
					header.Reload();
				};
			}

			return (true, Res.GetString("AE9CB645-8D72-4DC6-8ACA-C111C7D3D406", "{0} Air Import Message(s) Created.", bills.Count()), rollbackAction);
		}

		public (bool IsSuccess, string Message) SendManifestMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, MessageChooser messageChooser, AdditionalMessageInformation additionalMessageInformation)
		{
			if (header?.Bills.AsEnumerable().FirstOrDefault() is AsycudaBill bill)
			{
				var messageHeader = new ManifestMessageHeader((AsycudaManifestHeader)header, bill, messageSubType, additionalMessageInformation);
				if (messageChooser is AIMMessageChooser chooser && chooser.IsChangeOrCancellation)
				{
					messageHeader.SetAmendmentReason(chooser.Reason, string.Empty);
				}

				var message = new AIMMessageBuilder(messageHeader, header.Factory).Build();
				header.Messages.Add(message);
				message.EM_LinkedObject = header;

				return (true, Res.GetString("64842824-41C8-4364-8763-3896151A9401", "Air Import Manifest Message Created."));
			}

			return (false, Res.GetString("E4C64037-6A5F-40A4-B0A3-1F4FFEBFE51E", "The Manifest has no Bills"));
		}
	}
}
