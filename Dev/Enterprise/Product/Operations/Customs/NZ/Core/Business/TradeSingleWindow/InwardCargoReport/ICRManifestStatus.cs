using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class ICRManifestStatus : CustomsManifestStatus
	{
		public ICRManifestStatus(ForwardingConsol consol)
			: base(consol)
		{
			this.consol = consol;
			this.consol.Factory.Saved += Factory_Saved;
		}

		public ICRManifestStatus(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
			this.declaration.Factory.Saved += Factory_Saved;
		}

		public ICRManifestStatus(ForwardingConsol consol, IAdditionalInformation additionalInformation)
			: base(consol)
		{
			this.consol = consol;
			this.consol.Factory.Saved += Factory_Saved;
			AdditionalInformation = additionalInformation;
		}

		#region Overrides

		[BusinessObjectTestExclude]
		public new ZString E2_MessageStatus
		{
			get
			{
				CusEntryNumber iCRNumberCached = ICRNumber;
				return iCRNumberCached == null || iCRNumberCached.CE_EntryStatus.IsEmpty ? LowValueManifestStatusList.Descriptions.NotSentToCustoms : LowValueManifestStatusList.GetDescriptionFromCode(iCRNumberCached.CE_EntryStatus);
			}
			set
			{
				CusEntryNumber iCRNumberCached = ICRNumber
					?? CreateCusEntryNum();
				iCRNumberCached.CE_EntryStatus = value.Left(CusEntryNumber.Schema.CE_EntryStatusMaxLength);
				E2_MessageStatusInfo.RefreshBinding();
			}
		}

		public bool E2_MessageStatus_ReadOnly => true;

		public new ZPropertyInfo E2_MessageStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.E2_MessageStatus); }
		}

		public override ZString E2_CustomsEntryNumber
		{
			get
			{
				CusEntryNumber iCRNumberCached = ICRNumber;
				return iCRNumberCached != null ? iCRNumberCached.CE_EntryNum : ZString.Empty;
			}
		}

		public void SetCustomsEntryNumber(ZString newValue)
		{
			var icrNumber = ICRNumber;
			if (icrNumber != null)
			{
				icrNumber.CE_EntryNum = newValue.Left(CusEntryNumber.Schema.CE_EntryNumMaxLength);
			}
		}

		public override ZString E2_CustomsEntryNumberHumanReadableName
		{
			get { return ResString.GetMultilingualString("3F7E351C-ACD8-41C7-B77D-4457EF10503E", CusEntryNumberTypeList.Codes.ICRNumber); }
		}

		protected ICRMessageCollection ICRCollection
		{
			get
			{
				if (fICRCollection == null)
				{
					if (Consol != null)
					{
						fICRCollection = new ICRMessageCollection(Consol);
					}
					else
					{
						fICRCollection = new ICRMessageCollection(Declaration);
					}

					fICRCollection.Load();
				}
				return fICRCollection;
			}
		}
		ICRMessageCollection fICRCollection;

		public new EDIMessageCollection MessagesIncludingInterchangeRejections
		{
			get
			{
				if (fMessageCollection == null)
				{
					fMessageCollection = ICRCollection;
				}

				return fMessageCollection;
			}
		}
		EDIMessageCollection fMessageCollection;

		#endregion

		public ForwardingConsol Consol
		{
			get { return consol; }
		}
		protected readonly ForwardingConsol consol;

		public JobDeclaration Declaration
		{
			get { return declaration; }
		}
		protected readonly JobDeclaration declaration;

		#region Implementation

		protected readonly IAdditionalInformation AdditionalInformation;

		protected CusEntryNumber CreateCusEntryNum()
		{
			CusEntryNumber result = null;
			if (Consol != null)
			{
				result = (CusEntryNumber)Consol.Factory.New(typeof(CusEntryNumber));
				result.CE_ParentID = Consol.PK;
				result.CE_ParentTable = ForwardingConsol.Schema.TableName;
			}
			else
			{
				result = (CusEntryNumber)Declaration.Factory.New(typeof(CusEntryNumber));
				result.CE_ParentID = Declaration.PK;
				result.CE_ParentTable = JobDeclaration.Schema.TableName;
			}

			result.CE_EntryType = CusEntryNumberTypeList.Codes.ICRNumber;
			return result;
		}

		LowValueManifestStatusList LowValueManifestStatusList
		{
			get { return Factory.GetCachedValue<LowValueManifestStatusList>(); }
		}

		protected CusEntryNumber ICRNumber
		{
			get
			{
				CusEntryNumber[] result = null;
				if (Consol != null)
				{
					result = (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), CusEntryNumber.GetEntryNumberFilter(Consol, CusEntryNumberTypeList.Codes.ICRNumber));
				}
				else
				{
					result = (CusEntryNumber[])Factory.Load(typeof(CusEntryNumber), CusEntryNumber.GetEntryNumberFilter(Declaration, CusEntryNumberTypeList.Codes.ICRNumber));
				}

				if (result.Length == 0)
				{
					return null;
				}
				else if (result.Length > 1)
				{
					ErrorReporter.ReportOnce("EntryNumber for ICR has more than one record", "EntryNumber for ICR has more than one record");
				}

				return result[0];
			}
		}

		ZString SubmitterCode
		{
			get { return NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant().PadLeft(9, '0'); }
		}

		protected override string MessageApplicationCode
		{
			get { return EDIInterchange.ApplicationCodes.NewZealandCustoms; }
		}

		public override IManifestMessageBuilder NewCreateOrReplaceMessageBuilder()
		{
			MessageBuilder.MessageTypes messageType = E2_CustomsEntryNumber.IsEmpty ? MessageBuilder.MessageTypes.Original : MessageBuilder.MessageTypes.Replacement;
			return new MessageBuilder(Consol, AdditionalInformation, messageType, this, SubmitterCode);
		}

		protected override IManifestMessageBuilder[] NewMessageBuilders(Common.MessageBuilders.MessageSubTypes messageSubType)
		{
			if (messageSubType == Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				return new IManifestMessageBuilder[] { new MessageBuilder(Consol, AdditionalInformation, MessageBuilder.MessageTypes.Cancellation, this, SubmitterCode) };
			}
			return System.Array.Empty<IManifestMessageBuilder>();
		}

		protected override void DoResetToOriginal()
		{
			CusEntryNumber iCRNumberCached = ICRNumber;
			if (iCRNumberCached != null)
			{
				iCRNumberCached.Delete();
			}
		}

		protected override bool IsWaitingForResponse
		{
			get { return E2_MessageStatus == LowValueManifestStatusList.Descriptions.SentToCustoms; }
		}

		protected override bool IsStatusNotSent
		{
			get { return E2_MessageStatus == LowValueManifestStatusList.Descriptions.NotSentToCustoms; }
		}

		protected override bool IsClearCore
		{
			get { return E2_MessageStatus == LowValueManifestStatusList.Descriptions.ManifestAccepted; }
		}

		protected bool IsWithdrawn
		{
			get { return E2_MessageStatus == LowValueManifestStatusList.Descriptions.ManifestCancelled; }
		}

		protected override string ValidateEnvironmentForSendingManifests(ISendsMessagesToCustoms sender)
		{
			return IsWithdrawn ? "This manifest has been withdrawn. You cannot send further messages on this consol." : "";
		}

		protected override bool ContinueWithAction(ISendsMessagesToCustoms sender, Action action)
		{
			bool result = true;
			if (action == Action.Withdraw)
			{
				result = sender.ContinueWithAction("You have chosen to send a withdrawal message. Once you send and are acknowledged, you cannot send further messages on this consol.\r\nAre you sure you want to continue?", "Withrawal Message?");
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
				var cusEntryNum = ICRNumber;
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
