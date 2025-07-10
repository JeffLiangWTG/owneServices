using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsHeader : EU.NCTS.Business.NctsHeader
		, Integration.Customs.TR.ICusInBondHeader
		, ICusSupportingInfoTypeSupporter
		, ICusAddInfoTypeSupporter
		, IMessageAttachee
		, IEDIMessageCollectionOwner
	{
		public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : AutoCusInBondHeader.Schema
		{
			public const string Trailer1 = "Trailer1";
			public const string Trailer2 = "Trailer2";

			public const string StampDuty = nameof(StampDuty);
			public const string StampDutyStatus = nameof(StampDutyStatus);
			public const string RegistrationDate = nameof(RegistrationDate);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			StampDutyStatus = StampDutyStatusCodeList.Codes.D1;
			if(!IsPhase5)
			{
				CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			}
		}

		protected override void CloneCustomsOffices(EU.NCTS.Business.NctsHeader newHeader, bool newHeaderIsDepartureMovement)
		{
			var customsOfficesToBeCloned = newHeaderIsDepartureMovement ? CustomsOfficesForDeparture.ToList<EU.NCTS.Business.NctsEuOfficeCode>() : CustomsOffices.ToList<EU.NCTS.Business.NctsEuOfficeCode>();
			var newCustomsOffices = newHeader.CustomsOffices;
			if (customsOfficesToBeCloned.Any(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit))
			{
				newCustomsOffices.RemoveAndDeleteAll();
			}
			foreach (var office in customsOfficesToBeCloned)
			{
				var newOffice = (EU.NCTS.Business.NctsEuOfficeCode)new EU.NCTS.Business.NctsDeepCloneStrategy(office, newHeader.PK).Clone();
				newCustomsOffices.Add(newOffice);
			}
		}

		protected override CusInBondHeaderValidation GetNewPhase4Validation() => new NctsHeaderValidation(this);

		protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

		public new INctsHeaderValidation Validation => (INctsHeaderValidation)base.Validation;

		public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

		protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

		public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalDocuments;
		protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		[ResourceStringData("TRNctsHeader.Trailer1", Caption = "Trailer 1")]
		public ZString Trailer1
		{
			get
			{
				return Equipment1.BJ_RegistrationNumber;
			}
			set
			{
				var oldValue = Trailer1;
				if (oldValue != value)
				{
					Equipment1.BJ_RegistrationNumber = value;
				}
			}
		}

		public ZPropertyInfo Trailer1Info
		{
			get { return GetWrappedZPropertyInfo(Schema.Trailer1, x => Equipment1.BJ_RegistrationNumberInfo); }
		}

		[ResourceStringData("TRNctsHeader.Trailer2", Caption = "Trailer 2")]
		public ZString Trailer2
		{
			get
			{
				return Equipment2.BJ_RegistrationNumber;
			}
			set
			{
				var oldValue = Trailer2;
				if (oldValue != value)
				{
					Equipment2.BJ_RegistrationNumber = value;
				}
			}
		}

		public ZPropertyInfo Trailer2Info
		{
			get { return GetWrappedZPropertyInfo(Schema.Trailer2, x => Equipment2.BJ_RegistrationNumberInfo); }
		}

		[ChildEditable]
		public CusInBondEquipmentCollection Equipments
		{
			get
			{
				if (equipment == null)
				{
					equipment = new CusInBondEquipmentCollection(this);
					RegisterEditableChildObject(equipment);
				}
				return equipment;
			}
		}

		CusInBondEquipmentCollection equipment;

		public CusInBondEquipment Equipment1 => GetEquipment(Schema.Trailer1);

		public CusInBondEquipment Equipment2 => GetEquipment(Schema.Trailer2);

		CusInBondEquipment GetEquipment(ZString aceid)
		{
			var result = Equipments.FirstOrDefault(e => e.BJ_ACEID == aceid && !e.IsDeleted);
			if (result == null)
			{
				result = CusInBondEquipment.LoadOrCreateEquipment(this, aceid);
				RegisterEditableChildObject(result);
			}
			return result;
		}

		public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

		#region ICusSupportingInfoTypeSupporter Remarks

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result.Add(CusSupportingInfoTypeList.Codes.MTO, typeof(NctsManifestsToOpen));
			return result;
		}
		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

		#endregion ICusSupportingInfoTypeSupporter Remarks

		#region ManifestsToOpenList
		[ChildEditable(true)]
		public NctsManifestsToOpenCollection ManifestsToOpenList
		{
			get
			{
				if (manifestsToOpenList == null)
				{
					manifestsToOpenList = new NctsManifestsToOpenCollection(this, CusSupportingInfoTypeList.Codes.MTO);
					manifestsToOpenList.Load();
					RegisterEditableChildObject(manifestsToOpenList);
				}
				return manifestsToOpenList;
			}
		}
		NctsManifestsToOpenCollection manifestsToOpenList;
		#endregion

		#region IMessageAttachee

		ZString IMessageAttachee.MessageStatus { get => EffectiveMessageStatus; set => EffectiveMessageStatus = value; }

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

		#region ICusInBondHeader

		IBusinessObjectCollection Integration.Customs.TR.ICusInBondHeader.Messages => Messages;

		ZString Integration.Customs.TR.ICusInBondHeader.BM_CustomsStatus
		{
			get { return MovementHeader?.BM_CustomsStatus ?? ZString.Empty; }
			set
			{
				if (MovementHeader != null)
				{
					MovementHeader.BM_CustomsStatus = value;
				}
			}
		}

		#endregion

		[ChildEditable(true)]
		public new EU.NCTS.Business.INctsGuaranteeCollection<NctsGuarantee> Guarantees => IsPhase5Departure ? MovementHeader.Guarantees : (EU.NCTS.Business.INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

		protected override EU.NCTS.Business.INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new EU.NCTS.Business.NctsGuaranteeCollection<NctsGuarantee>(this);

		public new EU.NCTS.Business.INctsBillCollection<NctsBill> Bills => (EU.NCTS.Business.INctsBillCollection<NctsBill>)base.Bills;

		protected override EU.NCTS.Business.INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new EU.NCTS.Business.NctsBillCollection<NctsBill>(this);

		protected override Type BillTypeCore => typeof(NctsBill);

		[MaxLength(14)]
		public override ZString BH_JobReference
		{
			get => base.BH_JobReference;
			set => base.BH_JobReference = value;
		}

		#region Validate Company Length

		protected override void ValidatePrincipalAddress(JobDocAddressValidation validation)
		{
			base.ValidatePrincipalAddress(validation);
			var warningMessages = NctsCompanyValidationHelper.CheckAddressLength(Principal);
			foreach (var msg in warningMessages)
			{
				Principal.E2_OA_AddressInfo.AddWarning(msg);
			}
		}

		protected override void ValidateConsignorAddress(JobDocAddressValidation validation)
		{
			base.ValidateConsignorAddress(validation);
			var warningMessages = NctsCompanyValidationHelper.CheckAddressLength(Consignor);
			foreach (var msg in warningMessages)
			{
				Consignor.E2_OA_AddressInfo.AddWarning(msg);
			}
		}

		protected override void ValidateConsigneeAddress(JobDocAddressValidation validation)
		{
			base.ValidateConsigneeAddress(validation);
			var warningMessages = NctsCompanyValidationHelper.CheckAddressLength(Consignee);
			foreach (var msg in warningMessages)
			{
				Consignee.E2_OA_AddressInfo.AddWarning(msg);
			}
		}

		#endregion

		#region Stamp Duty CargoDesc

		[ChildEditable(true)]
		public StampDutyCargoDescCollection StampDutyCargoDescs
		{
			get
			{
				if (fStampDutyCargoDescs == null)
				{
					fStampDutyCargoDescs = new StampDutyCargoDescCollection(this);
					RegisterEditableChildObject(fStampDutyCargoDescs);
				}
				return fStampDutyCargoDescs;
			}
		}
		StampDutyCargoDescCollection fStampDutyCargoDescs;

		StampDutyCargoDesc SingleStampDutyCargoDesc
		{
			get
			{
				if (fSingleStampDutyCargoDesc == null || fSingleStampDutyCargoDesc.IsDeleted)
				{
					fSingleStampDutyCargoDesc = StampDutyCargoDescs.FirstOrDefault(e => e != null && !e.IsDeleted);
				}
				return fSingleStampDutyCargoDesc;
			}
		}
		StampDutyCargoDesc fSingleStampDutyCargoDesc;

		StampDutyCargoDesc CreateStampDutyCargoDesc()
		{
			using (SuspendSettingHasChanges())
			using (SuspendMarkingAsNeedingValidation())
			{
				return StampDutyCargoDescs.AddNew();
			}
		}

		[ResourceStringData("TR.NCTS.Business.NctsHeader|StampDuty", Caption = "Stamp Duty")]
		public ZDecimal StampDuty
		{
			get => SingleStampDutyCargoDesc?.StampDuty ?? ZDecimal.Zero;
			set
			{
				var oldValue = StampDuty;
				if (oldValue != value)
				{
					var stampDutyCargoDesc = SingleStampDutyCargoDesc ?? CreateStampDutyCargoDesc();
					stampDutyCargoDesc.StampDuty = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateStampDuty();
					}

					StampDutyInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo StampDutyInfo => GetZPropertyInfo(Schema.StampDuty);

		[MaxLength(1)]
		[ResourceStringData("TR.NCTS.Business.NctsHeader|StampDutyStatus", Caption = "Stamp Duty Status")]
		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.StampDutyStatusCodeList))]
		public ZString StampDutyStatus
		{
			get => SingleStampDutyCargoDesc?.StampDutyStatus ?? ZString.Empty;
			set
			{
				var oldValue = StampDutyStatus;
				if (oldValue != value)
				{
					var stampDutyCargoDesc = SingleStampDutyCargoDesc ?? CreateStampDutyCargoDesc();
					stampDutyCargoDesc.StampDutyStatus = value;

					CheckMaximumLength(StampDutyStatusInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateStampDutyStatus();
					}

					StampDutyStatusInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo StampDutyStatusInfo => GetZPropertyInfo(Schema.StampDutyStatus);

		[ResourceStringData("TR.NCTS.Business.NctsHeader|RegistrationDate", Caption = "Stamp Duty Ledger Registration Date", ShortCaption = "Registration Date")]
		public ZDate RegistrationDate
		{
			get => SingleStampDutyCargoDesc?.RegistrationDate ?? ZDate.Today;
		}

		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		#endregion

		#region LRN Register Number and Date

		[ReadOnly(true)]
		[ResourceStringData("953C428A-17CC-47AC-8F9F-08FFB98472DE", Caption = "LRN", MediumCaption = "LRN", ShortCaption = "LRN", MultipleKey = Phase4CaptionKey)]
		[ResourceStringData("562B29D0-160C-460A-99B1-BE0354200461", Caption = "Local Registration Number", ShortCaption = "LRN", MultipleKey = Phase5CaptionKey)]
		public ZString LrnRegistrationNumber
		{
			get => GetCusEntryNumber(CusEntryNumberTypes.Standard.LocalReferenceNumber).CE_EntryNum;
			set
			{
				var oldValue = LrnRegistrationNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(LrnRegisterNumberInfo, value);
					GetCusEntryNumber(CusEntryNumberTypes.Standard.LocalReferenceNumber).CE_EntryNum = value;
					LrnRegisterNumberInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo LrnRegisterNumberInfo => GetZPropertyInfo(nameof(LrnRegistrationNumber));

		[ReadOnly(true)]
		[ResourceStringData("953C428A-17CC-47AC-8F9F-08FFB98472DE", Caption = "Issue Date", MediumCaption = "Issue Date", ShortCaption = "Issue Date")]
		public ZDateTime LrnRegistrationDate
		{
			get => GetCusEntryNumber(CusEntryNumberTypes.Standard.LocalReferenceNumber)?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var oldValue = GetCusEntryNumber(CusEntryNumberTypes.Standard.LocalReferenceNumber)?.CE_IssueDate ?? ZDateTime.Empty;
				if (oldValue != value)
				{
					GetCusEntryNumber(CusEntryNumberTypes.Standard.LocalReferenceNumber).CE_IssueDate = value;
					LrnRegistrationDateInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo LrnRegistrationDateInfo => GetZPropertyInfo(nameof(LrnRegistrationDate));

		public CusEntryNumber GetCusEntryNumber(string entryType)
		{
			{
				if (cusEntryNumber == null || cusEntryNumber.IsDeleted)
				{
					cusEntryNumber = CusEntryNumber.LoadOrCreate(this, entryType, CountryCode);
					RegisterEditableChildObject(cusEntryNumber);
				}
				return cusEntryNumber;
			}
		}
		CusEntryNumber cusEntryNumber;

		#endregion

		#region MRN Date

		[ResourceStringData("953C428A-17CC-47AC-8F9F-08FFB98472DE", Caption = "MRN Date", MediumCaption = "MRN Date", ShortCaption = "MRN Date")]
		public ZDateTime MrnIssueDateFromUser
		{
			get => MovementReferenceEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var oldValue = MovementReferenceEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				if (oldValue != value)
				{
					MovementReferenceEntryNumber.CE_IssueDate = value;
					MrnIssueDateFromUserInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo MrnIssueDateFromUserInfo => GetZPropertyInfo(nameof(MrnIssueDateFromUser));

		#endregion

		public BusinessObject MessageOwner => this;

		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => this.Messages;

		public override void Delete()
		{
			Equipments.DeleteAll();
			StampDutyCargoDescs.DeleteAll();

			base.Delete();
		}
	}
}
