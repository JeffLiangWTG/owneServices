using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	[SystemDefinedValues]
	public class OutwardReportManifestStatus : CustomsManifestStatus
	{
		public const string InvalidReportNumber = "00000000";

		#region Schema

		public new class Schema : CustomsManifestStatus.Schema
		{
			public const string E2_OA_DeliveryNotificationParty = "E2_OA_DeliveryNotificationParty";
			public const string DeliveryNotificationPartyName = "DeliveryNotificationPartyName";
			public const string DeliveryNotificationPartyEmail = "DeliveryNotificationPartyEmail";
			public const string DeliveryNotificationPartyPort = "DeliveryNotificationPartyPort";
			public const string E2_MessagingMode = "E2_MessagingMode";
		}

		#endregion

		public OutwardReportManifestStatus(IManifestProvider manifestProvider)
			: base(manifestProvider)
		{
			ManifestProvider.Factory.Saved += Factory_Saved;
		}

		public OutwardReportManifestStatusValidation Validation
		{
			get
			{
				return fOutwardReportValidation ??= new OutwardReportManifestStatusValidation(this);
			}
		}
		OutwardReportManifestStatusValidation fOutwardReportValidation;

		[ChildEditable]
		public DeliveryNotificationParty DeliveryNotificationParty
		{
			get
			{
				if (deliveryNotificationParty is null)
				{
					deliveryNotificationParty = new DeliveryNotificationParty(this);
					RegisterEditableChildObject(deliveryNotificationParty);
				}
				return deliveryNotificationParty;
			}
		}
		DeliveryNotificationParty deliveryNotificationParty;

		#region Overrides

		protected override ZGuid GetPK()
		{
			return ManifestProvider != null ? ((BusinessObject)ManifestProvider).PK : ZGuid.Empty;
		}

		protected override void SetPKAndDefaults()
		{
		}

		public override string TablePrefix => ((BusinessObject)ManifestProvider).TablePrefix;

		public override bool IsInDatabase
		{
			get { return ((BusinessObject)ManifestProvider).IsInDatabase; }
		}

		[BusinessObjectTestExclude]
		public new ZString E2_MessageStatus
		{
			get
			{
				ZString result = OutwardReportStatusList.Descriptions.NotSent;
				var outwardReportNumberCached = OutwardReportNumber;
				if (outwardReportNumberCached != null)
				{
					result = outwardReportNumberCached.CE_EntryStatus.IsEmpty ? OutwardReportStatusList.Descriptions.Acknowledgement : ORNStatus.GetDescriptionFromCode(outwardReportNumberCached.CE_EntryStatus);
				}
				return result;
			}
			set
			{
				CusEntryNumber outwardReportNumberCached = OutwardReportNumber ?? FindOrCreateCusEntryNum();
				outwardReportNumberCached.CE_EntryStatus = value.Left(CusEntryNumber.Schema.CE_EntryStatusMaxLength);
				E2_MessageStatusInfo.RefreshBinding();
			}
		}

		public bool E2_MessageStatus_ReadOnly => true;

		public new ZPropertyInfo E2_MessageStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.E2_MessageStatus); }
		}

		[List(nameof(Lookups) + "." + nameof(OutwardReportLookups.MsgTransportList))]
		public ZString E2_MessagingMode
		{
			get { return this.GetSystemDefinedValue<ZString>(NZCMessage.MsgTransModeConstant); }
			set { this.SetSystemDefinedValue(NZCMessage.MsgTransModeConstant, value); }
		}

		public ZString E2_MessagingModeDesc => MsgTransportList.Descriptions.TSW;

		public override ZString E2_CustomsEntryNumber
		{
			get
			{
				CusEntryNumber outwardReportNumberCached = OutwardReportNumber;
				return outwardReportNumberCached != null ? outwardReportNumberCached.CE_EntryNum : ZString.Empty;
			}
		}

		public override ZString E2_CustomsEntryNumberHumanReadableName
		{
			get { return CusEntryNumberTypeList.Descriptions.OutwardReportNumber + ":"; }
		}

		public new EDIMessageCollection MessagesIncludingInterchangeRejections
		{
			get
			{
				if (fConsolMessages == null && Consol != null)
				{
					fConsolMessages = new NZCMessageCollection(Consol);
					fConsolMessages.Load();
				}

				return fConsolMessages;
			}
		}
		EDIMessageCollection fConsolMessages;

		#endregion

		public ForwardingConsol Consol => ManifestProvider as ForwardingConsol;

		public TSWTransactionTypes CreateOrReplaceTransaction
		{
			get { return (E2_CustomsEntryNumber.IsEmpty) ? TSWTransactionTypes.Original : TSWTransactionTypes.Replace; }
		}

		#region Lookups

		public OutwardReportLookups Lookups
		{
			get { return new OutwardReportLookups(this); }
		}

		#endregion

		#region Implementation
		protected CusEntryNumber FindOrCreateCusEntryNum()
		{
			CusEntryNumber existingEntryNumber = Factory.LoadTop1<CusEntryNumber>(new ZDBOnlyQuery(typeof(CusEntryNumber)).AddToFilter(CusEntryNumber.GetEntryNumberFilter(Consol)));

			if (existingEntryNumber == null)
			{
				CusEntryNumber result = (CusEntryNumber)ManifestProvider.Factory.New(typeof(CusEntryNumber));
				result.CE_ParentID = ((BusinessObject)ManifestProvider).PK;
				result.CE_ParentTable = ForwardingConsol.Schema.TableName;
				result.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
				result.CE_Category = string.Empty;
				return result;
			}
			else
			{
				return existingEntryNumber;
			}
		}

		OutwardReportStatusList fORNStatus;
		public OutwardReportStatusList ORNStatus
		{
			get
			{
				return fORNStatus ??= new OutwardReportStatusList();
			}
		}

		protected CusEntryNumber OutwardReportNumber
		{
			get
			{
				if (Consol != null)
				{
					CusEntryNumber[] result = (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), CusEntryNumber.GetEntryNumberFilter(Consol));
					if (result.Length == 0)
					{
						return null;
					}
					else if (result.Length > 1)
					{
						ErrorReporter.ReportOnce("EntryNumber for outward report has more than one record", "EntryNumber for outward cargo report has more than one record");
					}
					return result[0];
				}
				else
				{
					return null;
				}
			}
		}

		protected override string MessageApplicationCode
		{
			get { return EDIInterchange.ApplicationCodes.NewZealandCustoms; }
		}

		public override IManifestMessageBuilder NewCreateOrReplaceMessageBuilder()
		{
			if (Consol != null)
			{
				var messageType = EntryNeedsOriginalMessage ? MessageBuilder.MessageTypes.Original : MessageBuilder.MessageTypes.Replacement;
				return new MessageBuilder(Consol, messageType, this);
			}
			else
			{
				return null;
			}
		}

		protected bool EntryNeedsOriginalMessage
		{
			get { return E2_CustomsEntryNumber.IsEmpty || E2_CustomsEntryNumber == InvalidReportNumber; }
		}

		protected override IManifestMessageBuilder[] NewMessageBuilders(Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			if (messageSubType == Customs.Common.MessageBuilders.MessageSubTypes.Withdraw && Consol != null)
			{
				return new IManifestMessageBuilder[] { new MessageBuilder(Consol, MessageBuilder.MessageTypes.Cancellation, this) };
			}
			return Array.Empty<IManifestMessageBuilder>();
		}

		protected override void DoResetToOriginal()
		{
			OutwardReportNumber?.Delete();
		}

		protected override bool IsWaitingForResponse
		{
			get { return E2_MessageStatus == OutwardReportStatusList.Descriptions.AwaitingResponse.ToString(); }
		}

		protected override bool IsStatusNotSent
		{
			get { return E2_MessageStatus == OutwardReportStatusList.Descriptions.NotSent.ToString(); }
		}

		protected override bool IsClearCore
		{
			get { return E2_MessageStatus == OutwardReportStatusList.Descriptions.Cleared.ToString(); }
		}

		protected bool IsCancelled
		{
			get { return E2_MessageStatus == OutwardReportStatusList.Descriptions.Cancelled.ToString(); }
		}

		protected override string ValidateEnvironmentForSendingManifests(ISendsMessagesToCustoms sender)
		{
			return IsCancelled ? "This manifest has been cancelled. You cannot send further messages on this consol." : "";
		}

		protected override bool ContinueWithAction(ISendsMessagesToCustoms sender, Action action)
		{
			bool result = true;
			if (action == Action.Withdraw)
			{
				result = sender.ContinueWithAction("You have chosen to send a cancellation message. Once you send and are acknowledged, you cannot send further messages on this consol.\r\nAre you sure you want to continue?", "Cancellation Message?");
			}
			return result;
		}

		public ZDateTime EDITransmitDate
		{
			get
			{
				EDIMessage firstOutgoingMessage = MessagesIncludingInterchangeRejections.FirstOutgoingMessage;
				return firstOutgoingMessage == null ? ZDateTime.Now : firstOutgoingMessage.EM_SystemCreateTimeUtc;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				var cusEntryNum = OutwardReportNumber;
				if (cusEntryNum != null)
				{
					if (cusEntryNum.IsInDatabase)
					{
						cusEntryNum.CE_EntryStatus = (ZString)cusEntryNum.CE_EntryStatusInfo.OriginalValue;
					}
					else
					{
						cusEntryNum.Delete();
					}
				}
			}
		}

		#endregion
	}
}
