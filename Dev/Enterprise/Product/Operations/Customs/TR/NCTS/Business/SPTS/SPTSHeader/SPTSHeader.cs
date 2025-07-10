using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TR;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Business
{
	[SystemDefinedValues]
	public class SPTSHeader : CusInBondHeader, Integration.Customs.TR.ICusInBondSPTSHeader, IMessageAttachee, IRegistrationNoEntryProvider, IEDIMessageCollectionOwner, ICusInBondContainerTypeSupporter
	{
		public SPTSHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : AutoCusInBondHeader.Schema
		{
			public const string RegistrationNumber = "RegistrationNumber";
			public const string RegistrationDate = "RegistrationDate";
		}

		#region Override

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.TRSPTS;
			BH_HeaderType = NctsMovementType.Codes.Departure;
		}

		public new SPTSHeaderLookups Lookups
		{
			get { return (SPTSHeaderLookups)base.Lookups; }
		}

		[ChildEditable]
		public new SPTSBillCollection Bills => (SPTSBillCollection)base.Bills;
		protected override ICusInBondBillCollection GetNewBillsCollection() => new SPTSBillCollection(this);
		protected override Type BillTypeCore => typeof(SPTSBill);

		protected override Type MovementHeaderTypeCore => typeof(SPTSDepartureMovementHeader);
		public new SPTSDepartureMovementHeader MovementHeader => (SPTSDepartureMovementHeader)base.MovementHeader;
		protected override CusInBondMoveHeader GetNewMovementHeader()
		{
			var departureMovementHeader = SPTSDepartureMovementHeader.LoadOrCreate(this, Common.EU.NctsMoveHeaderType.Codes.Departure);
			RegisterEditableChildObject(departureMovementHeader);
			return departureMovementHeader;
		}

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (ediMessages == null)
				{
					ediMessages = new EDIMessageCollection(this, Factory);
					ediMessages.Load();
					ediMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessages);
				}
				return ediMessages;
			}
		}
		EDIMessageCollection ediMessages;

		[ChildEditable]
		public SPTSContainerCollection HeaderContainers
		{
			get
			{
				if (headerContainers == null)
				{
					headerContainers = new SPTSContainerCollection(this);
					RegisterEditableChildObject(headerContainers);
				}
				return headerContainers;
			}
		}
		SPTSContainerCollection headerContainers;

		protected override CusInBondHeaderLookups GetNewLookups() => new SPTSHeaderLookups(this);
		public new SPTSHeaderValidation Validation => (SPTSHeaderValidation)GetNewValidation();
		protected override CusInBondHeaderValidation GetNewValidation() => new SPTSHeaderValidation(this);

		[ChildEditable]
		public new CusInBondMoveHeaderCollection MovementHeaders => (SPTSCusInBondMoveHeaderCollection)base.MovementHeaders;
		protected override CusInBondMoveHeaderCollection GetMovementHeaders() => new SPTSCusInBondMoveHeaderCollection(this);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("CEE0C7FD-6EA6-42B7-9A4A-A667FC387685", "SPTS Simplified Procedure Transit System {0}", BH_JobReference);

		#endregion

		#region Properties

		public ZString CountryCode => Company.GC_RN_NKCountryCode;

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ReadOnly(true)]
		[ResourceStringData("SPTSHeader.RegistrationNumber", Caption = "Registration Number", ShortCaption = "Reg. Number")]
		public ZString RegistrationNumber
		{
			get => RegistrationEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, Common.CusInBondApplicationCodeList.Codes.TRSPTS, CountryCode);
						if (!registrationEntryNumber.IsInDatabase)
						{
							registrationEntryNumber.CE_EntryStatus = AsycudaRegistrationStatuses.Codes.Registered;
						}
						RegisterEditableChildObject(registrationEntryNumber);
					}
					registrationEntryNumber.CE_EntryNum = value;
					if (registrationEntryNumber.CE_IssueDate.IsEmpty)
					{
						registrationEntryNumber.CE_IssueDate = ZDateTime.Now;
						RegistrationDateInfo.RefreshBinding(ZDateTime.Empty);
					}
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryNum = value;
					}
				}
				RegistrationNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo RegistrationNumberInfo => GetZPropertyInfo(Schema.RegistrationNumber);

		[ReadOnly(true)]
		[ResourceStringData("SPTSHeader.RegistrationDate", Caption = "Registration Date", ShortCaption = "Date")]
		public ZDateTime RegistrationDate
		{
			get => RegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, Common.CusInBondApplicationCodeList.Codes.TRSPTS, CountryCode);
						if (!registrationEntryNumber.IsInDatabase)
						{
							registrationEntryNumber.CE_EntryStatus = AsycudaRegistrationStatuses.Codes.Registered;
						}
						RegisterEditableChildObject(registrationEntryNumber);
					}

					registrationEntryNumber.CE_IssueDate = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_IssueDate = value;
					}
				}

				RegistrationDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRegistrationDate();
				}
			}
		}
		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		internal CusEntryNumber RegistrationEntryNumber
		{
			get
			{
				if (registrationEntryNumber == null || registrationEntryNumber.IsDeleted || registrationEntryNumber.CE_EntryType != Common.CusInBondApplicationCodeList.Codes.TRSPTS)
				{
					registrationEntryNumber = CusEntryNumber.Load(this, Common.CusInBondApplicationCodeList.Codes.TRSPTS, CountryCode);
					if (registrationEntryNumber != null)
					{
						RegisterEditableChildObject(registrationEntryNumber);
					}
				}
				return registrationEntryNumber;
			}
		}
		CusEntryNumber registrationEntryNumber;

		public override void OnSaving()
		{
			if (BH_JobReference.IsEmpty)
			{
				BH_JobReference = Env.NumberFountains.GetTRSPTSJobReferenceNumber().GetNextFormatted(Factory);
			}

			if (registrationEntryNumber != null && ((registrationEntryNumber.CE_EntryNum.IsEmpty && registrationEntryNumber.CE_IssueDate.IsEmpty) && (registrationEntryNumber.CE_EntryLineReference.IsEmpty && registrationEntryNumber.CE_ExpiryDate.IsEmpty)))
			{
				registrationEntryNumber.Delete();
			}

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				BH_JobReference = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		[MaxLength(10)]
		[ResourceStringData("SPTSHeader|BH_VoyageNumber", Caption = "Voyage No")]
		public override ZString BH_VoyageNumber { get => base.BH_VoyageNumber; set => base.BH_VoyageNumber = value; }

		[ResourceStringData("SPTSHeader|BH_SailingDate", Caption = "Voyage Date")]
		public override ZDateTime BH_SailingDate { get => base.BH_SailingDate; set => base.BH_SailingDate = value; }

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(SPTSHeaderLookups.MessageStatusList))]
		[ReadOnly(true)]
		[ResourceStringData("SPTSHeader|BH_MessageStatus", Caption = "Messaging Status", ShortCaption = "Msg. St.")]
		public override ZString BH_MessageStatus
		{
			get => base.BH_MessageStatus;
			set
			{
				var oldValue = BH_MessageStatus;
				base.BH_MessageStatus = value;
				BH_MessageStatusInfo.RefreshBinding(oldValue);
			}
		}

		[ResourceStringData("SPTSHeader|InBondCarrier", Caption = "Carrier")]
		public ZString InBondCarrier
		{
			get
			{
				var result = MovementHeaders.Cast<SPTSDepartureMovementHeader>().Where(x => x.InBondCarrierOrg != null).Select(x => x.InBondCarrierOrg.OH_Code).Distinct();
				return result.Count() > 1 ? (ZString)"MUL" : result.FirstOrDefault();
			}
		}

		public void ChangeToAmmendSPTS()
		{
			var cusEntryNumber = RegistrationEntryNumber;
			if (cusEntryNumber != null && !cusEntryNumber.CE_EntryNum.IsEmpty)
			{
				BH_MessageStatus = ZString.Empty;

				cusEntryNumber.CE_EntryLineReference = cusEntryNumber.CE_EntryNum;
				cusEntryNumber.CE_ExpiryDate = ZDateTime.Now;
				cusEntryNumber.CE_EntryNum = ZString.Empty;
				cusEntryNumber.CE_IssueDate = ZDateTime.Empty;
				cusEntryNumber.CE_EntryStatus = SPTSMessageStatusList.Codes.Amendment;
				this.Logs.AddNew(AutoEvents.CustomsManifestStatus, SPTSMessageStatusList.Codes.Amendment);
			}
		}

		public override ZString BH_JobReference
		{
			get
			{
				var result = base.BH_JobReference;
				if (JobSequenceNumber > 0)
				{
					result = string.Format("{0}-{1}", result, JobSequenceNumber);
				}
				return result;
			}
			set => base.BH_JobReference = value;
		}

		int JobSequenceNumber => Factory.GetValue(ref jobSequenceNumberCached, delegate
					{
						var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsManifestStatusCode);
						logQuery.AddToFilter(StmALogSchema.SL_Reference, SPTSMessageStatusList.Codes.Amendment);
						return Logs.Find(logQuery).Length;
					});

		CachedProperty<int> jobSequenceNumberCached;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new SPTSHeaderFetchStrategy(this);
		}

		#region ICusInBondSPTSHeader

		IBusinessObjectCollection Integration.Customs.TR.ICusInBondSPTSHeader.Messages => Messages;

		ZString Integration.Customs.TR.ICusInBondSPTSHeader.MessageStatus { get => BH_MessageStatus; set => BH_MessageStatus = value; }

		ZDateTime Integration.Customs.TR.ICusInBondSPTSHeader.RegistrationDate { get => RegistrationDate; set => RegistrationDate = value; }

		ZString Integration.Customs.TR.ICusInBondSPTSHeader.RegistrationNumber { get => RegistrationNumber; set => RegistrationNumber = value; }

		#endregion

		#region IMessageAttachee

		ZString IMessageAttachee.MessageStatus { get => BH_MessageStatus; set => BH_MessageStatus = value; }

		ZGuid IMessageAttachee.GlobalBranchPK => BH_GB;

		IBusinessObjectCollection IMessageAttachee.Messages => Messages;

		ZString IMessageAttachee.JobReference => BH_JobReference;
		ZString IMessageAttachee.CustomsStatus
		{
			get => MovementHeader?.BM_CustomsStatus ?? ZString.Empty;
			set
			{
				if (MovementHeader != null)
				{
					MovementHeader.BM_CustomsStatus = value;
				}
			}
		}

		#endregion

		#region IRegistrationNoEntryProvider

		ZString IRegistrationNoEntryProvider.RegistrationNumber { get => RegistrationNumber; set => RegistrationNumber = value; }
		ZDateTime IRegistrationNoEntryProvider.RegistrationDate { get => RegistrationDate; set => RegistrationDate = value; }
		SecurityCheckpoint IRegistrationNoEntryProvider.CanModifyRegistrationNumbers => Env.Security.SPTSModifyRegistrationNumbers;
		BusinessObject IRegistrationNoEntryProvider.ParentBusinessObject => this;
		#endregion

		#region IEDIMessageCollectionOwner
		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;

		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => this.Messages;

		#endregion

		#region ICusInBondContainerTypeSupporter

		Type ICusInBondContainerTypeSupporter.ContainerType
		{
			get
			{
				return typeof(SPTSContainer);
			}
		}

		#endregion

		#region ICusInBondHeader

		public void Synchronise(ZBool force)
		{
			Synchroniser?.Synchronise(force);
		}

		public void SetMovementType(ZString headerType)
		{
			throw new NotImplementedException();
		}

		#endregion

		ZBool Integration.Customs.EU.NCTS.ICusInBondHeader.IsSecurityDeclaration => false;

		public ZBool IsInPhase5TransitionPeriod => false;
	}
}
