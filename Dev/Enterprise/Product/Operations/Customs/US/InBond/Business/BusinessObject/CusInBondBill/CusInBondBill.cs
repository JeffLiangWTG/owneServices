using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header.UniversalCopyIgnoreElement;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	[UniversalCopyIgnoreElement(CusAddInfos, CusCodeDatas, CusInbondBillAddRefs, Schema.B0_MessageStatus, Schema.B0_ReleaseStatus, Schema.B0_ReleaseStatusDate)]
	[UniversalCopyAssociateElement("Addresses", "DocAddresses")]
	[UniversalCopyWithExtendedEntities]
	public class CusInBondBill : Customs.Business.CusInBondBill,
		Integration.Customs.US.InBond.ICusInBondBill,
		IDocAddresses,
		IMessageAttachee,
		IResetToOriginal,
		IInBondQPBill,
		ICusInbondBillAddRefTypeSupporter
	{
		public CusInBondBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.CusInBondBill.Schema
		{
			public const string B0_PlaceOfReceiptDCode = "B0_PlaceOfReceiptDCode";
			public const int B0_PlaceOfReceiptDCodeMaxLength = 4;
		}

		#region New Properties

		public ZBool ShouldSend
		{
			get { return fShouldSend; }
			set { fShouldSend = value; }
		}
		ZBool fShouldSend;

		public bool IsAir
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}

		public bool IsSea
		{
			get
			{
				var header = Header;
				return header != null && header.IsSea;
			}
		}

		public bool IsFTZMove
		{
			get
			{
				var header = Header;
				return header != null && header.BH_FTZMove;
			}
		}

		public ZString BillUniqueCode
		{
			get
			{
				if (billUniqueCodeCached == null)
				{
					billUniqueCodeCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = new ZStringBuilder();
						if (IsAir || ShouldDisplayHouseBillInUniqueCode())
						{
							result.AppendIfNotEmpty(B0_HouseBillNumber);
							result.Append(" (");
						}

						if (Header?.BH_FTZMove ?? false)
						{
							result.AppendIfNotEmpty(B0_MasterBillNumber);
						}
						else
						{
							result.AppendIfNotEmpty(IssuerCodeAndMasterBillNumber);
						}

						if (IsAir || ShouldDisplayHouseBillInUniqueCode())
						{
							result.Append(")");
						}
						return result.ToString();
					});
				}
				return billUniqueCodeCached.Value;
			}
		}
		CachedProperty<ZString> billUniqueCodeCached;

		bool ShouldDisplayHouseBillInUniqueCode()
		{
			return ZZCustomsFunctionality.IsAMSHBREffective && IsSea && !B0_HouseBillNumber.IsEmpty;
		}

		public ZString IssuerCodeAndMasterBillNumber
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.AppendIfNotEmpty(B0_IssuerCode);
				result.AppendIfNotEmpty(B0_MasterBillNumber);
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		USCarrierCombined MasterBillCarrier
		{
			get { return Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, B0_IssuerCode)); }
		}

		public bool MasterBillCarrierHasNumericBillPrefix
		{
			get
			{
				var masterBillCarrier = MasterBillCarrier;
				return masterBillCarrier != null && !masterBillCarrier.UI_AirwayBillPrefix.IsEmpty && masterBillCarrier.UI_AirwayBillPrefix.IsNumbersOnlyOrEmpty;
			}
		}

		public JobDocAddress ForeignShipper
		{
			get
			{
				if (foreignShipper == null || foreignShipper.IsDeleted)
				{
					foreignShipper = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ForeignShipperDocumentaryAddress));
					foreignShipper.IgnoreValidationStatusError = IsAir;
				}

				return foreignShipper;
			}
		}
		JobDocAddress foreignShipper;

		public JobDocAddress Consignee
		{
			get
			{
				if (consignee == null || consignee.IsDeleted)
				{
					consignee = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ConsigneeAddress));
					consignee.IgnoreValidationStatusError = IsAir;
				}

				return consignee;
			}
		}
		JobDocAddress consignee;

		public JobDocAddress NotifyParty
		{
			get
			{
				if (notifyParty == null || notifyParty.IsDeleted)
				{
					notifyParty = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.NotifyParty));
					notifyParty.IgnoreValidationStatusError = IsAir;
				}

				return notifyParty;
			}
		}
		JobDocAddress notifyParty;

		public void RefreshDocAddresses()
		{
			var foreignShipperAddress = foreignShipper ?? DocAddresses.FindByDocAddressType(DocAddressType.ForeignShipperDocumentaryAddress);
			if (foreignShipperAddress != null)
			{
				foreignShipperAddress.IgnoreValidationStatusError = IsAir;
			}

			var consigneeAddress = consignee ?? DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress);
			if (consigneeAddress != null)
			{
				consigneeAddress.IgnoreValidationStatusError = IsAir;
			}

			var notifyPartyAddress = notifyParty ?? DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
			if (notifyPartyAddress != null)
			{
				notifyPartyAddress.IgnoreValidationStatusError = IsAir;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.RegionDistrictPorts))]
		[MaxLength(Schema.B0_PlaceOfReceiptDCodeMaxLength)]
		public ZString B0_PlaceOfReceiptDCode
		{
			get { return base.B0_PlaceOfReceipt.Left(Schema.B0_PlaceOfReceiptDCodeMaxLength); }
			set
			{
				ZString oldValue = B0_PlaceOfReceiptDCode;
				CheckMaximumLength(B0_PlaceOfReceiptDCodeInfo, value);
				base.B0_PlaceOfReceipt = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateB0_PlaceOfReceiptDCode();
				}
				B0_PlaceOfReceiptDCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo B0_PlaceOfReceiptDCodeInfo
		{
			get { return GetZPropertyInfo(Schema.B0_PlaceOfReceiptDCode); }
		}

		public ZWeight Weight
		{
			get { return new ZWeight(B0_Weight, B0_WeightUQ); }
		}

		public ZVolume Volume
		{
			get { return new ZVolume(B0_Volume, B0_VolumeUQ); }
		}

		public bool IsDetailedInBond
		{
			get
			{
				CusInBondHeader header = Header;
				return header != null && header.IsDetailedInBond;
			}
		}

		internal void SetIssuerCodeAndFTZForeignPortOfLadingIfRequired()
		{
			var header = Header;

			if (header != null && header.BH_FTZMove)
			{
				B0_IssuerCode = GetDefaultinBondCarrierSCACCode;

				if (B0_PortOfLadingKCode.IsEmpty)
				{
					B0_PortOfLadingKCode = FTZForeignPortOfLading;
				}
			}
		}
		internal const string FTZForeignPortOfLading = "99999";

		ZString GetDefaultinBondCarrierSCACCode
		{
			get
			{
				var result = ZString.Empty;

				var header = Header;
				if (header != null)
				{
					var inBondCarrierScacCodes = header.MovementHeaders.Select(x => x.BM_InBondCarrierSCAC).Where(x => !x.IsEmpty).Distinct().Take(2).ToArray();
					if (inBondCarrierScacCodes.Length == 1)
					{
						result = inBondCarrierScacCodes[0];
					}
				}
				return result;
			}
		}

		bool CopyParentDefault
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		internal bool ActiveInMessaging
		{
			get { return MoveDetails.Any(moveDetail => moveDetail.ActiveInMessaging); }
		}

		string ParentTableCode
		{
			get
			{
				var header = Header;
				return header != null ? header.ParentTableName : string.Empty;
			}
		}

		public bool IsWaitingForResponse
		{
			get { return LogManager.IsAnAwaitingStatus(B0_MessageStatus); }
		}

		public bool IsAcceptedByCustoms
		{
			get { return LogManager.IsAcceptedByCustoms(B0_MessageStatus); }
		}

		public bool HasDepartureBeenLodgedAtCustoms
		{
			get
			{
				var result = false;
				if (!B0_MessageStatus.IsEmpty)
				{
					result = !IsWaitingForResponse && LogManager.HasAClearLog;
				}
				else if (MovementDetail?.MoveHeader is CusInBondMoveHeader moveHeader)
				{
					result = !moveHeader.IsWaitingForResponse && moveHeader.LogManager.HasAClearLog;
				}
				return result;
			}
		}

		public bool IsWithdrawn
		{
			get { return LogManager.IsWithdrawn(B0_MessageStatus); }
		}

		internal ImportMessageStatusList StatusList => LogManager.StatusList;
		#endregion

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondBill|B0_MessageStatus", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. Status")]
		public override ZString B0_MessageStatus { get => base.B0_MessageStatus; set => base.B0_MessageStatus = value; }

		#region B0_BH

		[RelatedBusinessObject("Header")]
		public override ZGuid B0_BH
		{
			get { return base.B0_BH; }
			set
			{
				base.B0_BH = value;
				MoveDetails.MarkAsNeedingValidationIncludingChildren();
			}
		}

		public new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.CarrierAndFIRMSCollection))]
		public override ZString B0_IssuerCode
		{
			get { return base.B0_IssuerCode; }
			set
			{
				var oldValue = B0_IssuerCode;
				base.B0_IssuerCode = value;
				if (!IsCopying && oldValue != B0_IssuerCode)
				{
					if (IsAir)
					{
						var masterBillCarrier = MasterBillCarrier;
						var airwayBillPrefix = masterBillCarrier != null ? masterBillCarrier.UI_AirwayBillPrefix : ZString.Empty;
						if (!airwayBillPrefix.IsEmpty && (B0_MasterBillNumber.IsEmpty || B0_MasterBillNumber.Length == 3))
						{
							B0_MasterBillNumber = airwayBillPrefix;
						}
					}
					MoveDetails.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.CarrierAndFIRMSCollection))]
		public override ZString B0_HouseBillIssuerCode
		{
			get { return base.B0_HouseBillIssuerCode; }
			set
			{
				base.B0_HouseBillIssuerCode = value;
				CheckMaximumLength(B0_HouseBillIssuerCodeInfo, value);
			}
		}

		public bool IsIssuerCodeComesFromFrimCode
		{
			get
			{
				return Factory.GetCachedValue("USInBondCusInBondBill" + B0_IssuerCode, () =>
				{
					var query = new ZQuery(USCCarrierAndFIRMSSchema.US_Code, B0_IssuerCode);
					query.AddToFilter(USCCarrierAndFIRMSSchema.US_Type, "FIRMS");
					return Factory.LoadTop1<USCCarrierAndFIRMS>(query) != null;
				});
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.WeightUnitList))]
		public override ZString B0_WeightUQ
		{
			get { return base.B0_WeightUQ; }
			set { base.B0_WeightUQ = value; }
		}

		[MeasureUnit(Schema.B0_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal B0_Weight
		{
			get { return base.B0_Weight; }
			set { base.B0_Weight = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.VolumeUnitList))]
		public override ZString B0_VolumeUQ
		{
			get { return base.B0_VolumeUQ; }
			set { base.B0_VolumeUQ = value; }
		}

		[MeasureUnit(Schema.B0_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal B0_Volume
		{
			get { return base.B0_Volume; }
			set { base.B0_Volume = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ForeignPorts))]
		public override ZString B0_PortOfLadingKCode
		{
			get { return base.B0_PortOfLadingKCode; }
			set { base.B0_PortOfLadingKCode = value; }
		}

		public override ZInt B0_ManifestQty
		{
			get { return base.B0_ManifestQty; }
			set
			{
				var oldValue = base.B0_ManifestQty;
				base.B0_ManifestQty = value;
				if (oldValue != base.B0_ManifestQty)
				{
					MoveDetails.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondBillLookups.ManifestUnitList))]
		public override ZString B0_ManifestUQ
		{
			get { return base.B0_ManifestUQ; }
			set { base.B0_ManifestUQ = value; }
		}

		[ReadOnlyMember(nameof(B0_MasterBillNumber_ReadOnly))]
		public override ZString B0_MasterBillNumber
		{
			get { return base.B0_MasterBillNumber; }
			set
			{
				base.B0_MasterBillNumber = value;
				MoveDetails.MarkAsNeedingValidationIncludingChildren();
			}
		}

		bool B0_MasterBillNumber_ReadOnly
		{
			get { return IsInDatabase && MoveDetails.IsWaitingForResponseOrHasBeenReportedToCustoms; }
		}

		public new CusInBondBillLookups Lookups
		{
			get { return (CusInBondBillLookups)base.Lookups; }
		}

		public new CusInBondBillValidation Validation
		{
			get { return (CusInBondBillValidation)base.Validation; }
		}

		public new CusInBondMoveDetail MovementDetail
		{
			get { return (CusInBondMoveDetail)base.MovementDetail; }
		}

		public override bool CanDelete
		{
			get
			{
				var result = !IsInDatabase || base.CanDelete;
				return result && !ActiveInMessaging && !CopyParentDefault;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					var tempMsg = string.Format("This bill cannot be deleted {0}.", ActiveInMessaging
					? " as it has been submitted to Customs.\r\nPlease send an In-Bond Delete message first before deleting this bill"
					: (CopyParentDefault ? ValidationConstants.Synchronize.SynchronizedFromParent(ParentTableCode) : string.Empty));

					result = (NoResString)tempMsg;
				}
				return result;
			}
		}
		#endregion

		#region Override Methods

		public override void Delete()
		{
			AdditionalReferences.DeleteAll();
			MoveDetails.DeleteAll();
			DocAddresses.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Related Objects

		[ChildEditable]
		[UniversalCopyCollectionEntity(CusInbondBillAddRefSchema.Constants.TableName, CusInbondBillAddRefSchema.Constants.BR_B0)]
		public CusInbondBillAddRefCollection AdditionalReferences
		{
			get
			{
				if (additionalReferences == null)
				{
					additionalReferences = new CusInbondBillAddRefCollection(this);
					RegisterEditableChildObject(additionalReferences);
				}
				return additionalReferences;
			}
		}
		CusInbondBillAddRefCollection additionalReferences;

		[ChildEditable]
		public CusInBondMoveDetailCollection MoveDetails
		{
			get
			{
				if (moveDetails == null)
				{
					moveDetails = new CusInBondMoveDetailCollection(this);
					RegisterEditableChildObject(moveDetails);
				}
				return moveDetails;
			}
		}
		CusInBondMoveDetailCollection moveDetails;

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return "Bill Of Lading " + BillUniqueCode;
			}
		}

		protected override Type MovementDetailType
		{
			get { return typeof(CusInBondMoveDetail); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInBondBillLookups GetNewLookups()
		{
			return new CusInBondBillLookups(this);
		}

		protected override Customs.Business.CusInBondBillValidation GetNewValidation()
		{
			return new CusInBondBillValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (IsAir)
			{
				RefreshDocAddresses();
			}
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable]
		[UniversalCopySplitCollection("Foreign Shipper", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.ForeignShipperDocumentaryAddress + "'")]
		[UniversalCopySplitCollection("Consignee", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.ConsigneeAddress + "'")]
		[UniversalCopySplitCollection("Notify Party", JobDocAddressSchema.Constants.E2_AddressType + " = '" + AutoDocAddressTypes.Codes.NotifyParty + "'")]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					docAddresses.Sort(JobDocAddress.Schema.E2_AddressSequence);
					RegisterEditableChildObject(docAddresses);
				}
				return docAddresses;
			}
		}
		internal JobDocAddressDependentCollection docAddresses;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ForeignShipperDocumentaryAddress:
				case DocAddressType.ConsigneeAddress:
				case DocAddressType.NotifyParty:
					return DocAddressRequirement.GetJobDocAddressRequirement(addressType);
			}

			return null;
		}

		public BillJobDocAddressRequirement DocAddressRequirement
		{
			get
			{
				return Factory.GetCachedValue("BillJobDocAddressRequirement", delegate
				{
					return new BillJobDocAddressRequirement();
				});
			}
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ForeignShipperDocumentaryAddress,
					DocAddressType.ConsigneeAddress,
					DocAddressType.NotifyParty
				};
			}
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		IControllerIDProvider ControllerIDProvider
		{
			get { return Header; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#region IMessageAttachee
		ZString IMessageAttachee.MessageStatus { get => B0_MessageStatus; set => B0_MessageStatus = value; }

		CBPEDIMessageCollection IMessageAttachee.Messages => Messages;

		GlbBranch IMessageAttachee.Branch => Header?.Branch;

		BusinessObject IMessageAttachee.TopLevelBusinessObject => Header;

		string IMessageAttachee.TopLevelBizObjReferenceNumber => Header?.BH_JobReference ?? string.Empty;

		Logs IMessageAttachee.TopLevelBusinessObjectLogs => Header?.Logs;

		#endregion

		#region IResetToOriginal
		ZString IResetToOriginal.InBondNumber => MovementDetail?.MoveHeader.InBondNumber ?? ZString.Empty;

		ZString IResetToOriginal.MovementDescription => MovementDetail?.MoveHeader.MovementDescription ?? ZString.Empty;

		ZString IResetToOriginal.CustomsStatus => B0_MessageStatus;

		ZString IResetToOriginal.BillNumber => B0_MasterBillNumber;

		ZString IResetToOriginal.ContainerNumber => ZString.Empty;

		ZString IResetToOriginal.Level => "Bills of Lading";

		public StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs)); }
		}
		StatusLogManager fLogManager;

		void IResetToOriginal.ResetStatus(ZString reason)
		{
			Logs.AddNew(Events.ResetEntryMessageItemFunction, string.Format(CultureInfo.CurrentCulture, ResetStatusMessage, reason));
			B0_MessageStatus = ZString.Empty;
		}
		public const string ResetStatusMessage = "Message Status Reset Reason: {0}.";

		public ZBool IsResetableToOriginal => IsWaitingForResponse || (!LogManager.HasAWithdrawnLog && LogManager.HasAClearLog);
		#endregion

		#region IInBondQPBill Members

		IInBondQPHeader IInBondQPBill.Header => MovementDetail?.MoveHeader;

		#endregion

		#region ICusInbondBillAddRefTypeSupporter

		Type ICusInbondBillAddRefTypeSupporter.AddRefType
		{
			get
			{
				return typeof(CusInbondBillAddRef);
			}
		}

		#endregion
	}
}
