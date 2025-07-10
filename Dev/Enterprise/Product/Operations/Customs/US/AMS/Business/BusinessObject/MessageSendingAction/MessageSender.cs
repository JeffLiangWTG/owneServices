using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging;
using Enterprise.Customs.US.AMS.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSender : Integration.Customs.US.USAMS.IUSAMSMessageSender
	{
		public static Customs.Business.MessageSendingNotificationCollection GetNotifications(MessageSendingAction sendingAction)
		{
			Customs.Business.MessageSendingNotificationCollection result = null;
			var header = sendingAction.Header;
			var headerIsTopLevel = header.IsTopLevel;
			try
			{
				header.IsTopLevel = false;
				sendingAction.RegisterEditableChildObject(header);
				result = AMSMessageSendingValidation.New(header, sendingAction.CanSendMessageErrorsAsWarnings, null).CheckBusinessObjectLevelValidation();
			}
			finally
			{
				header.IsTopLevel = headerIsTopLevel;
				sendingAction.UnRegisterEditableChildObject(header);
			}
			return result;
		}

		public Tuple<int, string> SendManifest(ZGuid headerPK, IEnumerable<USAMSManifestBillAmendment> amendments = null, bool canSendMessageErrorsAsWarnings = true)
		{
			var header = new BusinessObjectFactory().Load<CusInBondHeader>(headerPK);
			if (header == null)
			{
				return Tuple.Create(0, Constants.Message.NoHeaderNotification);
			}
			else if (header.SCACInCarrier.IsEmpty)
			{
				return Tuple.Create(0, Constants.Message.NoOrgProxySCACNotification(header));
			}
			else
			{
				var actionCode = amendments?.Count() > 0 ? ActionCode.AmendingAdd : ActionCode.Creating;
				var sendingAction = new MessageSendingAction(header, actionCode, canSendMessageErrorsAsWarnings);
				if (sendingAction.MessageSendingObjects.Count == 0)
				{
					return Tuple.Create(0, Constants.Message.NoBillsNotification);
				}

				if (amendments != null)
				{
					var messages = sendingAction.MessageSendingObjects.OfType<MessageSendingObject>();
					foreach (var message in messages)
					{
						var amendment = amendments.FirstOrDefault(a => message.MoveDetail.B9_B0 == a.BillPK);
						var shouldSend = amendment != null;
						message.MB_Send = shouldSend;

						if (shouldSend)
						{
							message.MB_BillActionCode = amendment.ActionCode;
							message.MB_AmendmentCode = amendment.AmendmentCode;
						}
					}
				}

				var notifications = GetNotifications(sendingAction);
				if (notifications.ContainsError())
				{
					return Tuple.Create(0, notifications.ErrorNotificationsAsString().ToString());
				}
				try
				{
					var totalMessages = sendingAction.CreateAMSMessages();
					header.Factory.Save();
					return Tuple.Create(totalMessages, Constants.Message.MessageSentNotification(totalMessages));
				}
				catch (ZSaveException ex)
				{
					return Tuple.Create(0, string.Format(CultureInfo.InvariantCulture, "An exception has occurred while saving : {0}", ex));
				}
			}
		}

		#region Constants

		public static string AMSReportingCaption
		{
			get { return ResString.GetMultilingualString("USAMSMessageSendingActionForm|AMSReportingCaption", "AMS Reporting"); }
		}

		public static class Constants
		{
			public static class Caption
			{
				public static string NoBillsCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|98455BC3-9A95-44E4-8E40-D723F382E3E1", "No Bills"); }
				}

				public static string NoHeaderCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|{88A61931-CE4C-490A-8E1C-AC113B00A92D}", "No AMS Header"); }
				}

				public static string NoOrgProxySCACCaption
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|30F33345-3CE9-4333-A2CB-A60D3645D7D6", "No Org. Proxy SCAC"); }
				}
			}

			public static class Message
			{
				public static string MessageSentNotification(int totalMessages)
				{
					return totalMessages == 1
						? ResString.GetMultilingualString("a23b566e-35ac-43ba-b916-2ba78b7e4150", "1 message sent")
						: ResString.GetMultilingualString("3b54d923-2c75-4fad-9283-f4122cfce249", "{0} messages sent", totalMessages);
				}

				public static string NoBillsNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|8A8C5E39-FE9B-4A82-B0DD-A6B9AA5FF4D2", "No Bill of Lading has been entered."); }
				}

				public static string NoHeaderNotification
				{
					get { return ResString.GetMultilingualString("USAMSPlugIn|1211453E-D43E-4475-9C7D-C436DCA3FE4C", "No AMS Header has been created."); }
				}

				public static string NoOrgProxySCACNotification(CusInBondHeader header)
				{
					return ResString.GetMultilingualString("USAMSPlugIn|6FF87466-FC16-47B9-AF13-53F8545C90AB", "Both {0}'s Organization Proxy and {1} Branch's Organization Proxy do not have a SCAC (Standard Carrier Alpha Code) for US country of issue.\r\nPlease either set it up under AMS Branch > Organization Proxy > Details > Config > Registration Numbers / Codes or \r\nMaintain > User Admin > Companies > Current Company > Company Info. > Organization Proxy > Details > Config > Registration Numbers / Codes.", header.Company.GC_Name, header.Branch.GB_BranchName);
				}
			}
		}

		#endregion
	}
}
