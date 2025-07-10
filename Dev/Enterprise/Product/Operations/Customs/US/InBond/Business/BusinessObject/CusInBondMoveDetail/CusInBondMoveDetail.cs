using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header;
using static Enterprise.Integration.Customs;
using VolumeCalculator = Enterprise.Customs.US.Messaging.Business.VolumeCalculator;

namespace Enterprise.Customs.US.InBond.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[SystemDefinedValues]
	[CodeProperty("InbondNumberAndBillNumber"), DescriptionProperty("Description")]
	[UniversalCopyIgnoreElement(UniversalCopyIgnoreElement.InBondMoveDetail, UniversalCopyIgnoreElement.CusInBondBill, UniversalCopyIgnoreElement.CusInBondContainers, Schema.B9_CustomsStatus, Schema.B9_MessageStatus)]
	[UniversalCopyWithExtendedEntities]
	public class CusInBondMoveDetail : US.Business.CusInBondMoveDetail
		, Integration.Customs.US.InBond.ICusInBondMoveDetail
		, IInBondBillDetails
		, IMessageAttachee
		, IDispositionCodeDateParent
		, IDispositionCodeColumnsForFastSearchProvider
		, IInBondQXBillDetails
		, ISequenceNumberHeader
		, ICusAddInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, IBaseBillOfLading
		, ISynchroniserReadOnlyMembersProvider
		, IProcessHandlingInfoProvider
		, IWorkflowTriggerEventSource
	{
		public CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : US.Business.CusInBondMoveDetail.Schema
		{
			public const string B9_FirstSecondaryNotifyParty = "B9_FirstSecondaryNotifyParty";
			public const string B9_SecondSecondaryNotifyParty = "B9_SecondSecondaryNotifyParty";
			public const string B9_ThirdSecondaryNotifyParty = "B9_ThirdSecondaryNotifyParty";
			public const string B9_FourthSecondaryNotifyParty = "B9_FourthSecondaryNotifyParty";
			public const string B9_BillStatus = "B9_BillStatus";
			public const string B9_BillStatusDescription = "B9_BillStatusDescription";

			public const int SecondaryNotifyPartyMaxLength = 9;
		}

		#region New Properties
		public ZString InbondNumberAndBillNumber
		{
			get
			{
				if (MoveHeader != null && Bill != null)
				{
					return MoveHeader.InBondNumber + "-" + Bill.B0_MasterBillNumber;
				}
				return ZString.Empty;
			}
		}

		public ZString Description => ZString.Empty;

		public bool IsDetailedInBond
		{
			get
			{
				CusInBondMoveHeader moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsDetailedInBond;
			}
		}

		#region InBondHeaderPK

		public ZGuid InBondHeaderPK
		{
			get
			{
				EnsureInBondHeaderPKIsCorrect();
				return fInBondHeaderPK;
			}
			internal set
			{
				CusInBondMoveHeader moveHeader = MoveHeader;
				if (moveHeader != null)
				{
					ErrorReporter.ReportOnce("Should not set InBondHeaderPK if MoveHeader is not null");
				}
				fInBondHeaderPK = value;
			}
		}
		ZGuid fInBondHeaderPK;

		public CusInBondHeader InBondHeader
		{
			get { return Factory.Load<CusInBondHeader>(InBondHeaderPK); }
		}

		#endregion

		#region B9_FirstSecondaryNotifyParty

		[RelatedBusinessObject("FirstSecondaryNotifyParty")]
		[MaxLength(Schema.SecondaryNotifyPartyMaxLength)]
		public ZString B9_FirstSecondaryNotifyParty
		{
			get { return FirstSecondaryNotifyParty.CY_Data.Left(Schema.SecondaryNotifyPartyMaxLength); }
			set
			{
				CheckMaximumLength(B9_FirstSecondaryNotifyPartyInfo, value);
				FirstSecondaryNotifyParty.CY_Data = value;
				B9_FirstSecondaryNotifyPartyInfo.RefreshBinding();
				secondaryNotifyParties = null;
				if (!IsValidationSuspended)
				{
					Validation.ValidateB9_FirstSecondaryNotifyParty();
				}
			}
		}

		public ZPropertyInfo B9_FirstSecondaryNotifyPartyInfo
		{
			get { return GetZPropertyInfo(Schema.B9_FirstSecondaryNotifyParty); }
		}

		public SecondaryNotifyParty FirstSecondaryNotifyParty
		{
			get { return GetSecondaryNotifyParty(ref firstSecondaryNotifyParty, SecondaryNotifyPartyCodeList.Codes.First); }
		}
		SecondaryNotifyParty firstSecondaryNotifyParty;

		#endregion

		#region B9_SecondSecondaryNotifyParty

		[RelatedBusinessObject("SecondSecondaryNotifyParty")]
		[MaxLength(Schema.SecondaryNotifyPartyMaxLength)]
		public ZString B9_SecondSecondaryNotifyParty
		{
			get { return SecondSecondaryNotifyParty.CY_Data.Left(Schema.SecondaryNotifyPartyMaxLength); }
			set
			{
				CheckMaximumLength(B9_SecondSecondaryNotifyPartyInfo, value);
				SecondSecondaryNotifyParty.CY_Data = value;
				B9_SecondSecondaryNotifyPartyInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateB9_SecondSecondaryNotifyParty();
				}
			}
		}

		public ZPropertyInfo B9_SecondSecondaryNotifyPartyInfo
		{
			get { return GetZPropertyInfo(Schema.B9_SecondSecondaryNotifyParty); }
		}

		public SecondaryNotifyParty SecondSecondaryNotifyParty
		{
			get { return GetSecondaryNotifyParty(ref secondSecondaryNotifyParty, SecondaryNotifyPartyCodeList.Codes.Second); }
		}
		SecondaryNotifyParty secondSecondaryNotifyParty;

		#endregion

		#region B9_ThirdSecondaryNotifyParty

		[RelatedBusinessObject("ThirdSecondaryNotifyParty")]
		[MaxLength(Schema.SecondaryNotifyPartyMaxLength)]
		public ZString B9_ThirdSecondaryNotifyParty
		{
			get { return ThirdSecondaryNotifyParty.CY_Data.Left(Schema.SecondaryNotifyPartyMaxLength); }
			set
			{
				CheckMaximumLength(B9_ThirdSecondaryNotifyPartyInfo, value);
				ThirdSecondaryNotifyParty.CY_Data = value;
				B9_ThirdSecondaryNotifyPartyInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateB9_ThirdSecondaryNotifyParty();
				}
			}
		}

		public ZPropertyInfo B9_ThirdSecondaryNotifyPartyInfo
		{
			get { return GetZPropertyInfo(Schema.B9_ThirdSecondaryNotifyParty); }
		}

		public SecondaryNotifyParty ThirdSecondaryNotifyParty
		{
			get { return GetSecondaryNotifyParty(ref thirdSecondaryNotifyParty, SecondaryNotifyPartyCodeList.Codes.Third); }
		}
		SecondaryNotifyParty thirdSecondaryNotifyParty;

		#endregion

		#region B9_FourthSecondaryNotifyParty

		[RelatedBusinessObject("FourthSecondaryNotifyParty")]
		[MaxLength(Schema.SecondaryNotifyPartyMaxLength)]
		public ZString B9_FourthSecondaryNotifyParty
		{
			get { return FourthSecondaryNotifyParty.CY_Data.Left(Schema.SecondaryNotifyPartyMaxLength); }
			set
			{
				CheckMaximumLength(B9_FourthSecondaryNotifyPartyInfo, value);
				FourthSecondaryNotifyParty.CY_Data = value;
				B9_FourthSecondaryNotifyPartyInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateB9_FourthSecondaryNotifyParty();
				}
			}
		}

		public ZPropertyInfo B9_FourthSecondaryNotifyPartyInfo
		{
			get { return GetZPropertyInfo(Schema.B9_FourthSecondaryNotifyParty); }
		}

		public SecondaryNotifyParty FourthSecondaryNotifyParty
		{
			get { return GetSecondaryNotifyParty(ref fourthSecondaryNotifyParty, SecondaryNotifyPartyCodeList.Codes.Fourth); }
		}
		SecondaryNotifyParty fourthSecondaryNotifyParty;

		#endregion

		public bool IsWaitingForResponse
		{
			get
			{
				CusInBondMoveHeader moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsWaitingForResponse;
			}
		}

		public bool CanSendDeparture
		{
			get
			{
				CusInBondMoveHeader moveHeader = MoveHeader;
				return moveHeader != null && !moveHeader.LogManager.HasAClearLog;
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
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && (moveHeader.IsWaitingForResponse || (!moveHeader.LogManager.HasAWithdrawnLog && LogManager.HasAClearLog));
			}
		}

		string ParentTableName
		{
			get
			{
				var header = Header;
				return header != null ? header.ParentTableName : string.Empty;
			}
		}

		#endregion

		#region Override Properties

		#region CustomsStatus

		public override ZString B9_CustomsStatus
		{
			get { return base.B9_CustomsStatus; }
			set
			{
				ZString oldValue = B9_CustomsStatus;
				base.B9_CustomsStatus = value;
				var newValue = B9_CustomsStatus;
				if (!IsCopying && oldValue != newValue)
				{
					oldValue = newValue == ImportMessageStatusList.Codes.ClearDepartureWithdraw ? ZString.Empty : oldValue;
					LogManager.AddALogIfNecessary(oldValue, newValue);
				}
			}
		}

		#endregion

		#region Bill Status

		public ZString B9_BillStatus
		{
			get
			{
				DispositionData latestDisposition = LatestDisposition;
				return latestDisposition == null ? ZString.Empty : latestDisposition.US_Code;
			}
		}

		public ZPropertyInfo B9_BillStatusInfo
		{
			get { return GetZPropertyInfo(Schema.B9_BillStatus); }
		}

		public ZString B9_BillStatusDescription
		{
			get { return DispositionCodeDescriptionList.GetDescriptionFromCode(B9_BillStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo B9_BillStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.B9_BillStatusDescription); }
		}

		public DispositionData LatestDisposition
		{
			get
			{
				if (latestDispositionCached == null)
				{
					latestDispositionCached = new CachedProperty<DispositionData>(Factory, () => DispositionCodes.GetLatestDisposition());
				}
				return latestDispositionCached.Value;
			}
		}
		CachedProperty<DispositionData> latestDispositionCached;

		public DispositionData LatestDispositionForInBondClosedDate
		{
			get
			{
				if (latestDispositionForInBondClosedDateCached == null)
				{
					latestDispositionForInBondClosedDateCached = new CachedProperty<DispositionData>(Factory, () => DispositionCodes.GetLatestDispositionForInBondClosedDate(Header?.BH_ImportTransportMode ?? ZString.Empty));
				}
				return latestDispositionForInBondClosedDateCached.Value;
			}
		}
		CachedProperty<DispositionData> latestDispositionForInBondClosedDateCached;

		#endregion

		#region B9_SeqNo

		[ReadOnly(true)]
		public override ZString B9_SeqNo
		{
			get { return base.B9_SeqNo; }
			set { base.B9_SeqNo = value; }
		}

		#endregion

		#region B9_BM

		[RelatedBusinessObject("MoveHeader")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.MoveHeaders))]
		[ReadOnlyMember(nameof(B9_BM_ReadOnly))]
		public override ZGuid B9_BM
		{
			get { return base.B9_BM; }
			set
			{
				ZGuid oldValue = B9_BM;
				base.B9_BM = value;
				if (!IsCopying && oldValue != B9_BM)
				{
					CusInBondBill bill = Bill;
					if (bill != null)
					{
						bill.MarkAsNeedingValidation();
						Containers.MarkAsNeedingValidationIncludingChildren();
					}
					CBP7512Lines.MarkAsNeedingValidation();
				}
				EnsureInBondHeaderPKIsCorrect();
				DefaultFirstSecondaryNotifyParty();
			}
		}

		public new CusInBondMoveHeader MoveHeader
		{
			get { return Factory.Load<CusInBondMoveHeader>(B9_BM); }
		}

		protected bool B9_BM_ReadOnly
		{
			get
			{
				CusInBondHeader inBondHeader = InBondHeader;
				return inBondHeader != null && (!inBondHeader.SelectedMovementHeader.IsEmpty || inBondHeader.MovementHeaders.Count <= 1);
			}
		}

		#endregion

		#region B9_B0

		[RelatedBusinessObject("Bill")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.MasterBillsAndHouseBills))]
		[ReadOnlyMember(nameof(B9_B0_ReadOnly))]
		public override ZGuid B9_B0
		{
			get { return base.B9_B0; }
			set
			{
				ZGuid oldValue = B9_B0;
				base.B9_B0 = value;
				if (!IsCopying && oldValue != B9_B0)
				{
					UpdateInBondQty();
				}
			}
		}

		public new CusInBondBill Bill
		{
			get { return (CusInBondBill)base.Bill; }
		}

		protected bool B9_B0_ReadOnly
		{
			get { return !B9_SeqNo.IsEmpty; }
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.PreviousEntryTypeList))]
		public override ZString B9_PreviousITType
		{
			get { return base.B9_PreviousITType; }
			set { base.B9_PreviousITType = value; }
		}

		#region B9_PreviousITPortDCode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.RegionDistrictPorts))]
		[RelatedBusinessObject("PreviousITPortDCode")]
		public override ZString B9_PreviousITPortDCode
		{
			get { return base.B9_PreviousITPortDCode; }
			set { base.B9_PreviousITPortDCode = value; }
		}

		public ZZRefCusCodeListCombined PreviousITPortDCode
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, B9_PreviousITPortDCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		#endregion

		public new CusInBondMoveDetailLookups Lookups
		{
			get { return (CusInBondMoveDetailLookups)base.Lookups; }
		}

		public new CusInBondMoveDetailValidation Validation
		{
			get { return (CusInBondMoveDetailValidation)base.Validation; }
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !CopyParentDefault && (B9_SeqNo.IsEmpty || !ActiveInMessaging); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					result = MultilingualString.Join(" ", ResString.GetMultilingualString("ACF20E66-F94B-429D-9092-38ECE27E8B8D", "This Movement Detail cannot be deleted"),
						CopyParentDefault
						? ValidationConstants.Synchronize.SynchronizedFromParent(ParentTableName)
						: ResString.GetMultilingualString("303E27AE-B9CA-436B-A13F-26A3C90D2E23", "as it has been submitted to Customs.\r\nPlease send an In-Bond Delete message first before deleting this Movement Detail."));
				}
				return result;
			}
		}

		public bool ShouldSynchronise
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.ShouldSynchronise;
			}
		}

		#endregion

		#region Override Methods

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Containers.DeleteAll();
				SecondaryNotifyParties.RemoveAndDeleteAll();
				CBP7512Lines.DeleteAll();
			}
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(B9_CustomsStatus), ConcurrencyPolicy.Strict);
		}

		void DefaultFirstSecondaryNotifyParty()
		{
			var party = SubmitterABICode;
			if (!party.IsEmpty)
			{
				B9_FirstSecondaryNotifyParty = party;
			}
		}

		internal ZString SubmitterABICode
		{
			get
			{
				var iMessageAttachee = (IMessageAttacheeWithCBPSenderReference)MoveHeader;
				return iMessageAttachee == null ? string.Empty : iMessageAttachee.ProcessingDistrictPort + iMessageAttachee.EntryFilerCode + iMessageAttachee.ProcessingOfficeCode;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return ZString.Format("Move Detail {0}", base.HumanReadableNameCore); }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region Related Objects

		public StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs)); }
		}
		StatusLogManager fLogManager;

		public CusInBondHeader Header
		{
			get
			{
				return MoveHeader?.Header;
			}
		}

		[ChildEditable]
		[UniversalCopyCollectionEntity(CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.Constants.BC_ParentID, CusInBondContainerSchema.Constants.BC_ParentTableCode)]
		public new CusInBondContainerCollection Containers
		{
			get { return (CusInBondContainerCollection)base.Containers; }
		}

		protected override ICusInBondContainerCollection GetContainersCollection()
		{
			return new CusInBondContainerCollection(this);
		}

		[ChildEditable]
		public SecondaryNotifyPartyCollection SecondaryNotifyParties
		{
			get
			{
				if (secondaryNotifyParties == null)
				{
					secondaryNotifyParties = new SecondaryNotifyPartyCollection(this);
					RegisterEditableChildObject(secondaryNotifyParties);
					secondaryNotifyParties.Load();
				}
				return secondaryNotifyParties;
			}
		}
		SecondaryNotifyPartyCollection secondaryNotifyParties;

		[ReadOnly(true)]
		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
				}
				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		[ChildEditable]
		[UniversalCopyCollectionEntity(CusInBondMoveLineItemSchema.Constants.TableName, CusInBondMoveLineItemSchema.Constants.BI_B9)]
		public CusInBondMoveLineItemCollection CBP7512Lines
		{
			get
			{
				if (cbp7512Lines == null)
				{
					cbp7512Lines = new CusInBondMoveLineItemCollection(this);
					RegisterEditableChildObject(cbp7512Lines);
				}
				return cbp7512Lines;
			}
		}
		CusInBondMoveLineItemCollection cbp7512Lines;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID, AutoCusAddInfo.Schema.B7_ParentTableCode)]
		public WarehouseDetailCollection WarehouseDetails
		{
			get
			{
				if (fWarehouseDetails == null)
				{
					fWarehouseDetails = new WarehouseDetailCollection(this);
					fWarehouseDetails.Load();
					RegisterEditableChildObject(fWarehouseDetails);
				}
				return fWarehouseDetails;
			}
		}
		WarehouseDetailCollection fWarehouseDetails;

		#endregion

		#region Implementation

		protected override Type ContainerTypeCore
		{
			get { return typeof(CusInBondContainer); }
		}

		protected override Type PackTypeCore => typeof(CusInvPack);

		protected override Type MoveLineItemTypeCore => typeof(CusInBondMoveLineItem);

		void UpdateInBondQty()
		{
			CusInBondBill bill = Bill;
			if (bill != null)
			{
				int totalQty = 0;
				foreach (CusInBondMoveDetail moveDetail in bill.MoveDetails)
				{
					if (moveDetail != this)
					{
						totalQty += moveDetail.B9_InBoundQty;
					}
				}
				int leftoverQty = bill.B0_ManifestQty - totalQty;
				B9_InBoundQty = leftoverQty > 0 ? leftoverQty : 0;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		void EnsureInBondHeaderPKIsCorrect()
		{
			CusInBondMoveHeader moveHeader = MoveHeader;
			if (moveHeader != null)
			{
				fInBondHeaderPK = moveHeader.BM_BH;
			}
		}

		SecondaryNotifyParty GetSecondaryNotifyParty(ref SecondaryNotifyParty snp, ZString code)
		{
			if (snp == null || snp.IsDeleted)
			{
				if (snp != null)
				{
					UnRegisterEditableChildObject(snp);
				}
				ZQuery query = new ZQuery(CusCodeDataSchema.CY_ParentID, PK);
				query.AddToFilter(CusCodeDataSchema.CY_Code, code);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				snp = Factory.LoadTop1<SecondaryNotifyParty>(query);
				if (snp == null)
				{
					snp = Factory.New<SecondaryNotifyParty>();
					using (snp.SuspendSettingHasChanges())
					{
						snp.CY_Code = code;
						snp.Parent = this;
					}
				}
				RegisterEditableChildObject(snp);
			}
			return snp;
		}

		protected override Customs.Business.CusInBondMoveDetailLookups GetNewLookups()
		{
			return new CusInBondMoveDetailLookups(this);
		}

		protected override Customs.Business.CusInBondMoveDetailValidation GetNewValidation()
		{
			return new CusInBondMoveDetailValidation(this);
		}

		#endregion

		#region IInBondBillDetails Members

		IEnumerable<IInBondLineDetailsHeader> IInBondBillDetails.LineDetailsHeaders
		{
			get
			{
				foreach (IInBondLineDetailsHeader detailsHeader in Containers)
				{
					yield return detailsHeader;
				}
			}
		}

		JobDocAddress IInBondBillDetails.ConsigneeAddress
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? null : bill.Consignee;
			}
		}

		ZString IInBondBillDetails.ForeignLadingPortLocalCode
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_PortOfLadingKCode;
			}
		}

		JobDocAddress IInBondBillDetails.ForeignShipperAddress
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? null : bill.ForeignShipper;
			}
		}

		ZDecimal IInBondBillDetails.GoodsValueInLocalCurrency
		{
			get { return ZDecimal.Zero; } // We don't use this field
		}

		ZInt IInBondBillDetails.InBondQuantity
		{
			get
			{
				var result = 0;
				var header = Header;
				if (header != null && !header.IsAir)
				{
					result = B9_InBoundQty;
				}
				return result;
			}
		}

		ZBool IInBondBillDetails.IsDetailedInBond
		{
			get { return IsDetailedInBond; }
		}

		ZInt IInBondBillDetails.ManifestQuantity
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZInt.Zero : bill.B0_ManifestQty;
			}
		}

		ZString IInBondBillDetails.ManifestUQ
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_ManifestUQ;
			}
		}

		ZString IInBondBillDetails.MasterBillIssuerSCAC
		{
			get
			{
				var result = ZString.Empty;
				var bill = Bill;
				if (bill != null)
				{
					var header = bill.Header;
					if (header != null && header.IsAir)
					{
						result = bill.B0_MasterBillNumber.SubstringSafe(0, 3);
					}
					else
					{
						result = MasterBillIssuerCode;
					}
				}
				return result;
			}
		}

		ZString MasterBillIssuerCode
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_IssuerCode;
			}
		}

		ZString IInBondBillDetails.MasterBillNumber
		{
			get
			{
				var result = ZString.Empty;
				var bill = Bill;
				if (bill != null)
				{
					var header = bill.Header;
					if (header != null)
					{
						if (!header.IsAir && header.BH_FTZMove)
						{
							result = bill.B0_MasterBillNumber.Replace("-", "");
						}
						else if (header.IsAir)
						{
							result = bill.B0_MasterBillNumber.SubstringSafe(3, 8);
						}
						else
						{
							result = bill.B0_MasterBillNumber;
						}
					}
				}
				return result;
			}
		}

		ZString IInBondBillDetails.HouseBillNumber
		{
			get
			{
				var result = ZString.Empty;
				var bill = Bill;
				if (bill != null)
				{
					var header = bill.Header;
					result = (header != null && (header.IsAir || header.IsSea)) ? bill.B0_HouseBillNumber : ZString.Empty;
				}
				return result;
			}
		}

		ZString IInBondBillDetails.HouseBillIssuerCode
		{
			get
			{
				var result = ZString.Empty;
				var bill = Bill;
				if (bill != null)
				{
					var header = bill.Header;
					result = (header != null && header.IsSea) ? bill.B0_HouseBillIssuerCode : ZString.Empty;
				}
				return result;
			}
		}

		JobDocAddress IInBondBillDetails.NotifyPartyAddress
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? null : bill.NotifyParty;
			}
		}

		ZString IInBondBillDetails.PlaceOfPreReceipt
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_PlaceOfReceiptDCode;
			}
		}

		ZString IInBondBillDetails.PreviousITNumber
		{
			get { return B9_PreviousITNumber; }
		}

		ZString IInBondBillDetails.PreviousITType
		{
			get { return B9_PreviousITType; }
		}

		IEnumerable<IInBondBillReferenceNumber> IInBondBillDetails.RefNumbers
		{
			get
			{
				CusInBondBill bill = Bill;
				if (bill != null)
				{
					foreach (IInBondBillReferenceNumber additionalReference in bill.AdditionalReferences)
					{
						yield return additionalReference;
					}
				}
			}
		}

		ZString IInBondBillDetails.SequenceNumber
		{
			get { return B9_SeqNo; }
			set { B9_SeqNo = value; }
		}

		ZDecimal IInBondBillDetails.WeightInWholeNumber
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZDecimal.Zero : WeightCalculator.Calculate(bill.Weight);
			}
		}

		ZString IInBondBillDetails.WeightUQ
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : WeightCalculator.CalculateUQ(bill.B0_WeightUQ);
			}
		}

		ZDecimal IInBondBillDetails.VolumeInWholeNumber
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZDecimal.Zero : VolumeCalculator.Calculate(bill.Volume);
			}
		}

		ZString IInBondBillDetails.VolumeUQ
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_VolumeUQ;
			}
		}

		IEnumerable<ZString> IInBondBillDetails.SecondaryNotifyParties
		{
			get
			{
				if (!B9_FirstSecondaryNotifyParty.IsEmpty)
				{
					yield return B9_FirstSecondaryNotifyParty;
				}
				if (!B9_SecondSecondaryNotifyParty.IsEmpty)
				{
					yield return B9_SecondSecondaryNotifyParty;
				}
				if (!B9_ThirdSecondaryNotifyParty.IsEmpty)
				{
					yield return B9_ThirdSecondaryNotifyParty;
				}
				if (!B9_FourthSecondaryNotifyParty.IsEmpty)
				{
					yield return B9_FourthSecondaryNotifyParty;
				}
			}
		}

		#endregion

		#region IInBondQXBillDetails Members

		ZString IInBondQXBillDetails.MasterBillNumber
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_MasterBillNumber;
			}
		}

		ZString IInBondQXBillDetails.HouseBillNumber
		{
			get
			{
				CusInBondBill bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_HouseBillNumber;
			}
		}

		ZString IInBondQXBillDetails.PreviousInBondNumber
		{
			get { return B9_PreviousITNumber; }
		}

		#endregion

		#region IControllerIDProvider Members

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

		#endregion

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get
			{
				return Header?.Branch;
			}
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return B9_CustomsStatus; }
			set { B9_CustomsStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return MoveHeader.Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Header; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Header?.BH_JobReference ?? string.Empty; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get
			{
				return Header?.Logs;
			}
		}

		#endregion

		#region IBaseBillOfLading

		IManifestMessageAttachee IBaseBillOfLading.MessageAttachee
		{
			get { return MoveHeader; }
		}

		#endregion

		#region IIMessageAttacheeWithDisposition Members

		void IIMessageAttacheeWithDisposition.UpdateDispositionInformation(ZString dispositionCode, ZDateTime dispositionDate)
		{
			// we only want code & date/time to be the determination of adding new disposition, as messages could be reprocessed & message sequence simply increments...
			var latestDisposition = LatestDisposition;
			var latestDispositionForInBondClosedDate = LatestDispositionForInBondClosedDate;
			var newDisposition = DispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate);
			latestDispositionCached = null;
			latestDispositionForInBondClosedDateCached = null;
			CloseOrReopenInbondIfRequired(latestDispositionForInBondClosedDate, newDisposition);

			if (latestDisposition == null || latestDisposition.US_Code != newDisposition.US_Code)
			{
				this.Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1}", ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification, newDisposition.US_Code));
			}
		}

		void CloseOrReopenInbondIfRequired(DispositionData latestDispositionForCloseDate, DispositionData newDisposition)
		{
			var moveHeader = MoveHeader;
			if (moveHeader != null)
			{
				if (moveHeader.BM_InBondClosedDate.IsValid && moveHeader.CanOpenInBond(newDisposition.US_Code))
				{
					if (latestDispositionForCloseDate != null && latestDispositionForCloseDate.US_DispositionDate < newDisposition.US_DispositionDate)
					{
						moveHeader.BM_InBondClosedDate = ZDateTime.Empty;
					}
				}
				else
				{
					moveHeader.CloseMoveHeader();
				}
			}
		}

		void IIMessageAttacheeWithDisposition.MarkPreviousDispositionsInactive(ZDateTime dispositionDate)
		{
			foreach (DispositionData disposition in DispositionCodes)
			{
				if (disposition.US_DispositionDate <= dispositionDate)
				{
					if (!disposition.US_IsInactive)
					{
						disposition.US_IsInactive = ZBool.True;
					}
				}
			}
		}

		#endregion

		#region IDispositionCodeDateParent Members

		public CodeDescriptionPairList DispositionCodeDescriptionList
		{
			get { return DispositionCodeListLoader.GetDispositionCodes(Header?.BH_ImportTransportMode ?? ZString.Empty, Factory); }
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return "";
		}

		#endregion

		#region IDispositionCodeColumnsForFastSearchProvider Members

		SchemaColumn[] IDispositionCodeColumnsForFastSearchProvider.GetColumnsForFastSearch()
		{
			return new SchemaColumn[] { USDispositionDataAddInfoSchema.US_Code };
		}

		#endregion

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new CusInBondMoveDetailProcessHandlingInfo(this); }
		}

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines
		{
			get { return new TypedEnumerable<ISequenceNumberLine>(CBP7512Lines); }
		}

		public IDisposable PrintingSequenceNoRenumberingSuspender()
		{
			return PrintingSequenceNoGenerator.GetLineNumberSuspender();
		}

		internal ShortSequenceNumberGenerator PrintingSequenceNoGenerator
		{
			get { return printingSequenceNoGenerator ?? (printingSequenceNoGenerator = new ShortSequenceNumberGenerator(this)); }
		}
		ShortSequenceNumberGenerator printingSequenceNoGenerator;

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.SecondaryNotifyParty, typeof(SecondaryNotifyParty));
			return result;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, ObjectFactory.GetType<Integration.Customs.US.IDispositionData>());
			result.Add(CusAddInfoTypeAttribute.Codes.USWarehouseDetail, typeof(WarehouseDetail));
			return result;
		}

		#endregion

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return MoveHeader?.Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var moveHeader = MoveHeader;
				if (moveHeader != null)
				{
					list.Add(moveHeader);
				}

				return list;
			}
		}

		#endregion
	}
}
