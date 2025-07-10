using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveDetail : US.Business.CusInBondMoveDetail,
		Integration.Customs.US.USAMS.ICusInBondMoveDetail,
		IACEBillOfLading,
		IBaseBillOfLading,
		IMovemenDetails
	{
		public CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : US.Business.CusInBondMoveDetail.Schema
		{
			public const string InBondDepartureStatus = "InBondDepartureStatus";
			public const string InBondDepartureStatusDesc = "InBondDepartureStatusDesc";
			public const string InBondArrivalStatus = "InBondArrivalStatus";
			public const string InBondArrivalStatusDesc = "InBondArrivalStatusDesc";
			public const string InBondExportationStatus = "InBondExportationStatus";
			public const string InBondExportationStatusDesc = "InBondExportationStatusDesc";
			public const string InBondTransferOfLiabilityStatus = "InBondTransferOfLiabilityStatus";
			public const string InBondTransferOfLiabilityStatusDesc = "InBondTransferOfLiabilityStatusDesc";
		}

		public bool ShouldSynchroniseWithConsol
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		public bool IsBillAlreadyOnFile
		{
			get { return B9_CustomsStatus == AMSBillCustomsStatusList.Codes.OnFile; }
		}

		public bool IsMessagingInProgress
		{
			get { return AMSBillMessageStatusList.IsMessagingInProgressType(B9_MessageStatus); }
		}

		public bool IsAcceptedByCustoms
		{
			get { return AMSBillMessageStatusList.IsAcceptedByCustoms(B9_MessageStatus); }
		}

		public bool IsMessageRejectedByError
		{
			get { return AMSBillMessageStatusList.IsMessageRejectedByError(B9_MessageStatus); }
		}

		public bool IsPTTMovement
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsPTTMovement;
			}
		}

		public bool IsAMSMovement
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsAMSMovement;
			}
		}

		public bool IsInBondMovement
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsInBondMovement;
			}
		}

		public bool IsSubsequentInBondMovement
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsInBondMovement && moveHeader.BM_IsSubsequentInBond;
			}
		}

		public bool IsImmediateTransportEntryType
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsImmediateTransportEntryType;
			}
		}

		public bool IsTransportandExportEntryType
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsTransportandExportEntryType;
			}
		}

		public bool IsImmediateExportEntryType
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsImmediateExportEntryType;
			}
		}

		#region InBond Status

		#region InBond Departure Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondDepartureStatus", Caption = "In-Bond Departure Status", MediumCaption = "Departure Status", ShortCaption = "Dept. Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.InBondStatusList))]
		public ZString InBondDepartureStatus
		{
			get
			{
				if (inBondDepartureStatusCached == null)
				{
					inBondDepartureStatusCached = new CachedProperty<ZString>(Factory, () =>
						{
							return GetInBondStatus(AMSApplicationIdentifierCodeList.Codes.SubsequentInBond, new ZString[] { AMSMessageSubTypeList.Codes.SubsequentInBondOriginal, AMSMessageSubTypeList.Codes.SubsequentInBondAmendment, AMSMessageSubTypeList.Codes.SubsequentInBondDelete }, (x) =>
								{
									var result = ZString.Empty;
									if (x != null)
									{
										var infoBlocks = x.MessageBlock.MessageBlocks.OfType<TARW01>();
										result = infoBlocks == null ? AMSBillMessageStatusList.Codes.ClearDeparture : AMSBillMessageStatusList.Codes.Error;
									}
									else
									{
										result = AMSBillMessageStatusList.Codes.AwaitingDeparture;
									}
									return result;
								});
						});
				}
				return inBondDepartureStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondDepartureStatusCached;

		public ZPropertyInfo InBondDepartureStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondDepartureStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondDepartureStatusDesc", Caption = "In-Bond Departure Status Description", MediumCaption = "Departure Status Desc.", ShortCaption = "Dept. Status  Desc.")]
		public ZString InBondDepartureStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondDepartureStatus); }
		}

		public ZPropertyInfo InBondDepartureStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondDepartureStatusDesc); }
		}

		#endregion

		#region InBond Arrival Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondArrivalStatus", Caption = "In-Bond Arrival Status", MediumCaption = "Arrival Status", ShortCaption = "Arr. Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.InBondStatusList))]
		public ZString InBondArrivalStatus
		{
			get
			{
				if (inBondArrivalStatusCached == null)
				{
					inBondArrivalStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						var billActionCodeList = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondArrival);
						return GetInBondStatus(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, new ZString[] { AMSMessageSubTypeList.Codes.PaperlessInBondOrVesselArrival, AMSMessageSubTypeList.Codes.InBondArrival },
							(x) =>
								{
									var result = ZString.Empty;
									if (x != null)
									{
										var infoBlocks = x.MessageBlock.MessageBlocks.OfType<TARW01>().FirstOrDefault();
										var icmh01 = x.OriginalMessage.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault();
										result = infoBlocks == null ? InBondAndVesselEventMessageCodeList.IsCancelInBondArrival(icmh01.MessageCode) ? AMSBillMessageStatusList.Codes.Deleted : AMSBillMessageStatusList.Codes.ClearArrival : AMSBillMessageStatusList.Codes.Error;
									}
									else
									{
										result = AMSBillMessageStatusList.Codes.AwaitingArrival;
									}
									return result;
								},
							(x) =>
								{
									return x.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault(y => billActionCodeList.ContainsCode(y.MessageCode)) != null;
								});
					});
				}
				return inBondArrivalStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondArrivalStatusCached;

		public ZPropertyInfo InBondArrivalStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondArrivalStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondArrivalStatusDesc", Caption = "In-Bond Arrival Status Description", MediumCaption = "Arrival Status Desc.", ShortCaption = "Arr. Status Desc.")]
		public ZString InBondArrivalStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondArrivalStatus); }
		}

		public ZPropertyInfo InBondArrivalStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondArrivalStatusDesc); }
		}

		#endregion

		#region InBond Exportation Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondExportationStatus", Caption = "In-Bond Exportation Status", MediumCaption = "Exportation Status", ShortCaption = "Exp. Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.InBondStatusList))]
		public ZString InBondExportationStatus
		{
			get
			{
				if (inBondExportationStatusCached == null)
				{
					inBondExportationStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						var billActionCodeList = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondExportation);
						return GetInBondStatus(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, new ZString[] { AMSMessageSubTypeList.Codes.PaperlessInBondOrVesselArrival, AMSMessageSubTypeList.Codes.InBondExportation },
							(x) =>
							{
								var result = ZString.Empty;
								if (x != null)
								{
									var infoBlocks = x.MessageBlock.MessageBlocks.OfType<TARW01>();
									var icmh01 = x.OriginalMessage.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault();
									result = infoBlocks == null ? InBondAndVesselEventMessageCodeList.IsCancelInBondExportation(icmh01.MessageCode) ? AMSBillMessageStatusList.Codes.Deleted : AMSBillMessageStatusList.Codes.ClearExportation : AMSBillMessageStatusList.Codes.Error;
								}
								else
								{
									result = AMSBillMessageStatusList.Codes.AwaitingExportation;
								}
								return result;
							},
							(x) =>
							{
								return x.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault(y => billActionCodeList.ContainsCode(y.MessageCode)) != null;
							});
					});
				}
				return inBondExportationStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondExportationStatusCached;

		public ZPropertyInfo InBondExportationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondExportationStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondExportationStatusDesc", Caption = "In-Bond Exportation Status Description", MediumCaption = "Exportation Status Desc.", ShortCaption = "Exp. Status Desc.")]
		public ZString InBondExportationStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondExportationStatus); }
		}

		public ZPropertyInfo InBondExportationStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondExportationStatusDesc); }
		}

		#endregion

		#region InBond TransferOfLiability Status

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondTransferOfLiabilityStatus", Caption = "In-Bond Transfer Of Liability Status", MediumCaption = "Transfer Of Liability Status", ShortCaption = "TOL Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.InBondStatusList))]
		public ZString InBondTransferOfLiabilityStatus
		{
			get
			{
				if (inBondTransferOfLiabilityStatusCached == null)
				{
					inBondTransferOfLiabilityStatusCached = new CachedProperty<ZString>(Factory, () =>
					{
						var billActionCodeList = InBondAndVesselEventMessageCodeList.GetBillActionCodeList(Factory, ActionCode.InBondTransferOfLiability);
						return GetInBondStatus(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, new ZString[] { AMSMessageSubTypeList.Codes.PaperlessInBondOrVesselArrival, AMSMessageSubTypeList.Codes.InBondTransferOfLiability },
							(x) =>
							{
								var result = ZString.Empty;
								if (x != null)
								{
									var infoBlocks = x.MessageBlock.MessageBlocks.OfType<TARW01>();
									var icmh01 = x.OriginalMessage.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault();
									result = infoBlocks == null ? InBondAndVesselEventMessageCodeList.IsCancelInBondTransferOfLiability(icmh01.MessageCode) ? AMSBillMessageStatusList.Codes.Deleted : AMSBillMessageStatusList.Codes.ClearTransferOfLiability : AMSBillMessageStatusList.Codes.Error;
								}
								else
								{
									result = AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability;
								}
								return result;
							},
							(x) =>
							{
								return x.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault(y => billActionCodeList.ContainsCode(y.MessageCode)) != null;
							});
					});
				}
				return inBondTransferOfLiabilityStatusCached.Value;
			}
		}
		CachedProperty<ZString> inBondTransferOfLiabilityStatusCached;

		public ZPropertyInfo InBondTransferOfLiabilityStatusInfo
		{
			get { return GetZPropertyInfo(Schema.InBondTransferOfLiabilityStatus); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail|InBondTransferOfLiabilityStatusDesc", Caption = "In-Bond Transfer Of Liability Status Description", MediumCaption = "Transfer Of Liability Status Desc.", ShortCaption = "TOL Status Desc.")]
		public ZString InBondTransferOfLiabilityStatusDesc
		{
			get { return Lookups.InBondStatusList.GetDescriptionFromCode(InBondTransferOfLiabilityStatus); }
		}

		public ZPropertyInfo InBondTransferOfLiabilityStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.InBondTransferOfLiabilityStatusDesc); }
		}

		#endregion

		ZString GetInBondStatus(ZString applicationIdentifier, ZString[] messagesSubTypes, Func<US.Messaging.Business.CBPEDIMessage, ZString> getStatus, Func<AMSEDIMessage, bool> extraMessageMatch = null)
		{
			var result = ZString.Empty;
			var moveHeader = MoveHeader;
			if (moveHeader != null && moveHeader.IsInBondMovement)
			{
				var billNumber = this.MasterBillNumber;
				var outwardMessage = moveHeader.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.AMS, new[] { applicationIdentifier }, EDIMessage.Direction.Transmit, false, System.ComponentModel.ListSortDirection.Descending)
					.OfType<AMSEDIMessage>()
						.Where(x => messagesSubTypes.Contains(x.EM_MessageSubType) && x.MessageBlock.MessageBlocks.OfType<IINPM02>()
							.FirstOrDefault(y => y.CarrierAssignedBatchNumber.StartsWith(billNumber)) != null &&
							(extraMessageMatch == null || extraMessageMatch(x))).OrderByDescending(o => o.EM_SystemCreateTimeUtc).FirstOrDefault();

				if (outwardMessage != null)
				{
					result = getStatus(outwardMessage.ResponseMessage);
				}
			}
			return result;
		}

		#endregion

		#region Override Properties

		[System.ComponentModel.ReadOnly(true)]
		public override ZString B9_CustomsStatus
		{
			get { return base.B9_CustomsStatus; }
			set
			{
				var oldValue = B9_CustomsStatus;
				base.B9_CustomsStatus = value;
				if (!IsCopying && oldValue != B9_CustomsStatus)
				{
					UpdateMoveHeaderStatusIfNeeded();
					MarkAsNeedingValidation(Bill);
				}
			}
		}

		[System.ComponentModel.ReadOnly(true)]
		public override ZString B9_MessageStatus
		{
			get { return base.B9_MessageStatus; }
			set
			{
				base.B9_MessageStatus = value;
			}
		}

		#region B9_BM

		[RelatedBusinessObject("MoveHeader")]
		public override ZGuid B9_BM
		{
			get { return base.B9_BM; }
			set
			{
				var oldBill = B9_BM != value && B9_BM.IsValid ? Bill : null;
				var oldValue = B9_BM;
				base.B9_BM = value;
				if (!IsCopying && oldValue != B9_BM)
				{
					MarkAsNeedingValidation(oldBill);
					MarkAsNeedingValidation(Bill);
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		public new CusInBondMoveHeader MoveHeader
		{
			get { return (CusInBondMoveHeader)base.MoveHeader; }
		}

		#endregion

		#region B9_B0

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveDetailLookups.RelatedBills))]
		[RelatedBusinessObject("Bill")]
		public override ZGuid B9_B0
		{
			get { return base.B9_B0; }
			set
			{
				var oldBill = B9_B0 != value && B9_B0.IsValid ? Bill : null;
				var oldValue = B9_B0;
				base.B9_B0 = value;
				if (!IsCopying && oldValue != B9_B0)
				{
					if (IsInBondMovement)
					{
						var bill = Bill;
						if (bill != null)
						{
							B9_InBoundQty = bill.B0_ManifestQty;
							var movementDetail = bill.MovementDetail;
							if (movementDetail != null)
							{
								B9_MonetaryValue = movementDetail.Containers.TotalMonetaryValue;
							}
						}
					}
					MarkAsNeedingValidation(oldBill);
					MarkAsNeedingValidation(Bill);
					Containers.MarkAsNeedingValidation();
					foreach (var container in Containers)
					{
						container.Vehicles.MarkAsNeedingValidation();
						container.Commodities.MarkAsNeedingValidation();
					}
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

		public override ZString B9_ExportLadenOn
		{
			get { return GetEffectiveValueToReturn(base.B9_ExportLadenOn, CusInBondMoveHeader.Schema.BM_ExportLadenOn, Schema.B9_ExportLadenOn); }
			set { base.B9_ExportLadenOn = GetEffectiveValueToSet(value, CusInBondMoveHeader.Schema.BM_ExportLadenOn); }
		}

		public override ZString B9_ForeignDestPortKCode
		{
			get { return GetEffectiveValueToReturn(base.B9_ForeignDestPortKCode, CusInBondMoveHeader.Schema.BM_ForeignDestPortKCode, Schema.B9_ForeignDestPortKCode); }
			set { base.B9_ForeignDestPortKCode = GetEffectiveValueToSet(value, CusInBondMoveHeader.Schema.BM_ForeignDestPortKCode); }
		}

		public override ZDateTime B9_ExportDate
		{
			get { return GetEffectiveValueToReturn(base.B9_ExportDate, CusInBondMoveHeader.Schema.BM_ExportDate, Schema.B9_ExportDate); }
			set { base.B9_ExportDate = GetEffectiveValueToSet(value, CusInBondMoveHeader.Schema.BM_ExportDate); }
		}

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
			get { return base.CanDelete && !ActiveInMessaging; }
		}

		internal bool ActiveInMessaging
		{
			get
			{
				return IsBillAlreadyOnFile || IsMessagingInProgress || IsAcceptedByCustoms;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					result = ValidationConstants.MoveDetail.CannotDeleteMoveDetailBeforeMessageDelete(IsInBondMovement ? "an In-Bond Cancellation" : IsPTTMovement ? "a Permit To Transfer Cancel" : "a Delete");
				}
				return result;
			}
		}

		#endregion

		#region Override Methods

		public override void OnSaving()
		{
			base.OnSaving();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(B9_CustomsStatus), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(B9_MessageStatus), ConcurrencyPolicy.Strict);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsInDatabase && B9_MessageStatusInfo.HasChanges)
			{
				var bill = Bill;
				if (bill != null)
				{
					bill.Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1}", B9_MessageStatus, Lookups.MessageStatusList.GetDescriptionFromCode(B9_MessageStatus)));
				}
			}
		}

		#endregion

		#region Related Objects

		public CusInBondMoveHeader InBondMovement
		{
			get
			{
				var result = MoveHeader;
				if (result != null && !result.IsInBondMovement && result.IsAMSMovement)
				{
					var bill = Bill;
					result = bill == null ? null : bill.MasterInBondMovement;
				}
				return result;
			}
		}

		public CusInBondMoveDetail InBondMoveDetail
		{
			get
			{
				var moveDetail = this;
				var moveHeader = MoveHeader;
				var inBondMovement = InBondMovement;
				if (moveHeader != inBondMovement)
				{
					moveDetail = inBondMovement == null || B9_B0.IsEmpty ? null : inBondMovement.MovementDetails.FirstOrDefault(x => x.B9_B0 == B9_B0);
				}
				return moveDetail;
			}
		}

		public bool HasBillOnFile => InBondMovement?.MovementDetails.Any(x => x.IsBillAlreadyOnFile) ?? false;

		public CusInBondHeader Header
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader == null ? null : moveHeader.Header;
			}
		}

		[ChildEditable]
		public new CusInBondContainerCollection Containers
		{
			get { return (CusInBondContainerCollection)base.Containers; }
		}

		protected override ICusInBondContainerCollection GetContainersCollection()
		{
			var result = new CusInBondContainerCollection(this);
			result.CountChanged += OnNoOfContainersChanged;
			return result;
		}

		void OnNoOfContainersChanged(object sender, EventArgs e)
		{
			if (!this.IsDeleted)
			{
				var bill = Bill;
				if (bill != null)
				{
					bill.B0_NoOfAMSContainersInfo.RefreshBinding();
				}
				var header = Header;
				if (header != null)
				{
					header.BH_NoOfAMSContainersInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region Validation Modes

		public bool IsPermitToTransferValidationMode
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsPTTMovement;
			}
		}

		public bool IsSubsequentInBondValidationMode
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsSubsequentInBondValidationMode;
			}
		}

		public bool IsInBondArrivalValidationMode
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsInBondArrivalValidationMode;
			}
		}

		public bool IsInBondExportationValidationMode
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsInBondExportationValidationMode;
			}
		}

		public bool IsInBondTOLValidationMode
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader != null && moveHeader.IsInBondTOLValidationMode;
			}
		}

		#endregion

		#region Implementation

		void UpdateMoveHeaderStatusIfNeeded()
		{
			var moveHeader = MoveHeader;
			if (moveHeader != null)
			{
				moveHeader.RefreshCustomsStatus();
			}
		}

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInCusInBondMoveHeader, string cusInBondMoveDetailFieldName) where T : IZType
		{
			var result = baseValue;

			if (result.IsEmpty)
			{
				var moveHeader = MoveHeader;
				if (moveHeader != null)
				{
					var effectiveValue = GetEffectiveValue(cusInBondMoveDetailFieldName) ?? (IZType)moveHeader[fieldNameInCusInBondMoveHeader];
					result = (T)effectiveValue;
				}
			}

			return result;
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInCusInBondMoveHeader) where T : IZType
		{
			var result = valuePassed;

			if (!valuePassed.IsDefault)
			{
				var moveHeader = MoveHeader;
				if (moveHeader != null && moveHeader[fieldNameInCusInBondMoveHeader].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
			}

			return result;
		}

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType moveHeaderValue)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { MoveHeaderValue = moveHeaderValue };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		IZType GetEffectiveValue(string fieldName)
		{
			EffectiveValueSuspender holder;
			return EffectiveValueSuspenders.TryGetValue(fieldName, out holder) ? holder.MoveHeaderValue : null;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(CusInBondMoveDetail moveDetail, string fieldName)
			{
				this.moveDetail = moveDetail;
				this.fieldName = fieldName;
			}

			public IZType MoveHeaderValue { get; set; }

			readonly CusInBondMoveDetail moveDetail;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				moveDetail.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}

		#endregion

		protected override Type ContainerTypeCore
		{
			get { return typeof(CusInBondContainer); }
		}

		protected override Type MoveLineItemTypeCore => typeof(CusInBondMoveLineItem);

		protected override Type PackTypeCore => typeof(CusInvPack);

		void MarkAsNeedingValidation(BusinessObject bizObj)
		{
			if (bizObj != null)
			{
				bizObj.MarkAsNeedingValidation();
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInBondMoveDetailLookups GetNewLookups()
		{
			return new CusInBondMoveDetailLookups(this);
		}

		protected override Customs.Business.CusInBondMoveDetailValidation GetNewValidation()
		{
			return new CusInBondMoveDetailValidation(this);
		}

		public override void Delete()
		{
			Containers.CountChanged -= OnNoOfContainersChanged;
			base.Delete();
		}

		#endregion

		#region IACEBillOfLading Members

		IEnumerable<IACEContainer> IACEBillOfLading.Containers
		{
			get
			{
				var bill = Bill;
				if (bill != null)
				{
					var amsMoveDetail = bill.MovementDetail;
					if (amsMoveDetail != null)
					{
						foreach (IACEContainer container in amsMoveDetail.Containers)
						{
							yield return container;
						}
					}
				}
			}
		}

		IMovemenDetails IACEBillOfLading.MovemenDetails
		{
			get { return this; }
		}

		#endregion

		#region ICommonBillOfLading Members

		ZString ICommonBillOfLading.BillActionCode
		{
			get { throw new NotImplementedException(); }
		}

		ZString ICommonBillOfLading.AmendmentCode
		{
			get { throw new NotImplementedException(); }
		}

		ZString ICommonBillOfLading.IssuerCode
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_IssuerCode;
			}
		}

		ZString ICommonBillOfLading.BillOfLadingSequenceNumber
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_MasterBillNumber;
			}
		}

		ZString ICommonBillOfLading.ForeignPort
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_PortOfLadingKCode;
			}
		}

		ZDecimal ICommonBillOfLading.ManifestQuantity
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZDecimal.Zero : new ZDecimal(bill.B0_ManifestQty);
			}
		}

		ZString ICommonBillOfLading.ManifestUnits
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_ManifestUQ;
			}
		}

		ZDecimal ICommonBillOfLading.Weight
		{
			get
			{
				var result = ZDecimal.Zero;
				var bill = Bill;
				if (bill != null)
				{
					result = bill.B0_Weight;
					if (!result.IsEmpty && !bill.B0_WeightUQ.IsEmpty)
					{
						var unit = WeightUnitList.ConvertFromFreightWeight(bill.B0_WeightUQ);
						if (unit == WeightUnitList.Codes.Kilograms)
						{
							result = bill.Weight.InKilogramsSafe;
						}
					}
				}
				return result.Round(0);
			}
		}

		ZString ICommonBillOfLading.WeightUnit
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : WeightUnitList.ConvertFromFreightWeight(bill.B0_WeightUQ);
			}
		}

		ZString ICommonBillOfLading.BillOfLadingStatusIndicator
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_BillStatus;
			}
		}

		ZBool ICommonBillOfLading.IsMasterInbond
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.B0_MasterInBondIndicator;
			}
		}

		ZString ICommonBillOfLading.HouseBillNumber
		{
			get { return ZString.Empty; } // The Bill Of Lading is the House Bill
		}

		ZString ICommonBillOfLading.FIRMS
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_Firms;
			}
		}

		ZDecimal ICommonBillOfLading.Volume
		{
			get
			{
				var result = ZDecimal.Zero;
				var bill = Bill;
				if (bill != null)
				{
					result = bill.B0_Volume;
					if (!result.IsEmpty && !bill.B0_VolumeUQ.IsEmpty)
					{
						var unit = VolumeUnitList.ConvertFromFreightVolume(bill.B0_VolumeUQ);
						if (unit == VolumeUnitList.Codes.CubicMeters)
						{
							result = bill.Volume.InCubicMetres;
						}
					}
				}
				return result.Round(0);
			}
		}

		ZString ICommonBillOfLading.VolumeUnit
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : VolumeUnitList.ConvertFromFreightVolume(bill.B0_VolumeUQ);
			}
		}

		ZString ICommonBillOfLading.PlaceOfReceiptByCarrier
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_PlaceOfReceipt;
			}
		}

		ZString ICommonBillOfLading.SpaceCharterBLReference
		{
			get { return ZString.Empty; }
		}

		ZString ICommonBillOfLading.SecondNotifyParty1
		{
			get
			{
				var secondaryNotifyParties = SecondaryNotifyParties;
				return secondaryNotifyParties.Length > 0 ? secondaryNotifyParties[0].CY_Data : ZString.Empty;
			}
		}

		ZString ICommonBillOfLading.SecondNotifyParty2
		{
			get
			{
				var secondaryNotifyParties = SecondaryNotifyParties;
				return secondaryNotifyParties.Length > 1 ? secondaryNotifyParties[1].CY_Data : ZString.Empty;
			}
		}

		ZString ICommonBillOfLading.LastForeignPortBeforeDepartingForTheUS
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_LastForeignPortKCode;
			}
		}

		ZString ICommonBillOfLading.ModeOfTransportationFromThePlacePriorToLoading
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_TransportModeToPortOfLading;
			}
		}

		ZString ICommonBillOfLading.MethodOfPaymentForTransportation
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_TransportPaymentMethod;
			}
		}

		ZString ICommonBillOfLading.ContractualPossessionForeignPort
		{
			get
			{
				var bill = Bill;
				return bill == null ? ZString.Empty : bill.B0_ForeignPortOfContractKCode;
			}
		}

		IEnumerable<IShipmentReferenceDetail> ICommonBillOfLading.ShipmentReferenceDetails(ActionCode actionCode)
		{
			var bill = Bill;
			if (bill != null)
			{
				var list = (actionCode == ActionCode.SubsequentInBondOriginal || actionCode == ActionCode.SubsequentInBondAmendment || actionCode == ActionCode.SubsequentInBondDelete) ? BillReferenceList.GetCachedACEM1InBondList(Factory) : null;
				if (bill.IsNVOCCHeader && bill.ShipmentReferenceDetails[BillReferenceList.Codes.OB] == null)
				{
					if (list == null || list.ContainsCode(BillReferenceList.Codes.OB))
					{
						var oceanBill = bill.Header.OceanBill;
						if (oceanBill != null && (!oceanBill.B0_IssuerCode.IsEmpty || !oceanBill.B0_MasterBillNumber.IsEmpty))
						{
							yield return new ShipmentReferenceDetail() { Qualifier = BillReferenceList.Codes.OB, ReferenceIdentifier = oceanBill.B0_IssuerCode + oceanBill.B0_MasterBillNumber };
						}
					}
				}

				foreach (IShipmentReferenceDetail shipmentReferenceDetail in bill.ShipmentReferenceDetails)
				{
					if (list == null || list.ContainsCode(shipmentReferenceDetail.Qualifier))
					{
						yield return shipmentReferenceDetail;
					}
				}
			}
		}

		JobDocAddressParty ForeignShipperParty
		{
			get
			{
				if (foreignShipperPartyCached == null)
				{
					foreignShipperPartyCached = new CachedProperty<JobDocAddressParty>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? null : new JobDocAddressParty(bill.ForeignShipper, EntityIDCodeList.Codes.Shipper);
					});
				}
				return foreignShipperPartyCached.Value;
			}
		}
		CachedProperty<JobDocAddressParty> foreignShipperPartyCached;

		JobDocAddressConsignee ConsigneeParty
		{
			get
			{
				if (consigneePartyCached == null)
				{
					consigneePartyCached = new CachedProperty<JobDocAddressConsignee>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? null : new JobDocAddressConsignee(bill.Consignee);
					});
				}
				return consigneePartyCached.Value;
			}
		}
		CachedProperty<JobDocAddressConsignee> consigneePartyCached;

		JobDocAddressParty NotifyParty1Party
		{
			get
			{
				if (notifyParty1PartyCached == null)
				{
					notifyParty1PartyCached = new CachedProperty<JobDocAddressParty>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? null : new JobDocAddressParty(bill.NotifyParty1, EntityIDCodeList.Codes.NotifyParty1);
					});
				}
				return notifyParty1PartyCached.Value;
			}
		}
		CachedProperty<JobDocAddressParty> notifyParty1PartyCached;

		JobDocAddressParty NotifyParty2Party
		{
			get
			{
				if (notifyParty2PartyCached == null)
				{
					notifyParty2PartyCached = new CachedProperty<JobDocAddressParty>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? null : new JobDocAddressParty(bill.NotifyParty2, EntityIDCodeList.Codes.NotifyParty2);
					});
				}
				return notifyParty2PartyCached.Value;
			}
		}
		CachedProperty<JobDocAddressParty> notifyParty2PartyCached;

		JobDocAddressParty ShipToParty
		{
			get
			{
				if (shipToPartyCached == null)
				{
					shipToPartyCached = new CachedProperty<JobDocAddressParty>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? null : new JobDocAddressParty(bill.ShipToParty, EntityIDCodeList.Codes.ShipTo);
					});
				}
				return shipToPartyCached.Value;
			}
		}
		CachedProperty<JobDocAddressParty> shipToPartyCached;

		JobDocAddressParty BookingParty
		{
			get
			{
				if (bookingPartyCached == null)
				{
					bookingPartyCached = new CachedProperty<JobDocAddressParty>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? null : new JobDocAddressParty(bill.BookingParty, ACEEntityIDCodeList.Codes.BookingParty);
					});
				}
				return bookingPartyCached.Value;
			}
		}
		CachedProperty<JobDocAddressParty> bookingPartyCached;

		SecondaryNotifyParty[] SecondaryNotifyParties
		{
			get
			{
				if (secondaryNotifyPartiesCached == null)
				{
					secondaryNotifyPartiesCached = new CachedProperty<SecondaryNotifyParty[]>(Factory, delegate
					{
						var bill = Bill;
						return bill == null ? Array.Empty<SecondaryNotifyParty>() : bill.SecondaryNotifyParties.GetNonEmptySNPInSortOrder();
					});
				}
				return secondaryNotifyPartiesCached.Value;
			}
		}
		CachedProperty<SecondaryNotifyParty[]> secondaryNotifyPartiesCached;

		class Entity : IEntity
		{
			#region IEntity Members
			public ZString EntityCode { get; set; }
			public ZString EntityName { get; set; }
			public ZString CodeQualifier { get; set; }
			public ZString IDCode { get; set; }
			public ZString AddressLine1 { get; set; }
			public ZString AddressLine2 { get; set; }
			public ZString AddressLine1Part2 { get; set; }
			public ZString AddressLine2Part2 { get; set; }
			public ZString CityName { get; set; }
			public ZString StateProvince { get; set; }
			public ZString PostalCode { get; set; }
			public ZString CountryCode { get; set; }
			public INotifyPartyContact AdminContact { get; set; }
			#endregion
		}

		IEnumerable<IEntity> GetSecondaryNotifyParties()
		{
			var secondNotifyParties = SecondaryNotifyParties;
			for (var i = 2; i < secondNotifyParties.Length; i++)
			{
				var secondNotifyParty = secondNotifyParties[i];
				if (secondNotifyParty != null && !secondNotifyParty.IsDeleted && !secondNotifyParty.CY_Data.IsEmpty)
				{
					yield return new Entity() { EntityCode = EntityIDCodeList.Codes.SecondaryNotifyParty, CodeQualifier = SCACOrFIRMSOfSNPQualifier, IDCode = secondNotifyParty.CY_Data };
				}
			}
		}

		IEnumerable<IEntity> IACEBillOfLading.Entities(ActionCode actionCode)
		{
			var bill = Bill;
			if (bill != null)
			{
				var arePartiesAllowed = actionCode == ActionCode.Creating || actionCode == ActionCode.AmendingAdd;
				if (arePartiesAllowed)
				{
					if (!bill.ForeignShipper.IsEmpty)
					{
						yield return ForeignShipperParty;
					}

					if (!bill.Consignee.IsEmpty)
					{
						yield return ConsigneeParty;
					}

					if (!bill.NotifyParty1.IsEmpty)
					{
						yield return NotifyParty1Party;
					}

					if (!bill.NotifyParty2.IsEmpty)
					{
						yield return NotifyParty2Party;
					}
				}

				foreach (var snp in GetSecondaryNotifyParties())
				{
					yield return snp;
				}

				var customsBroker = bill.CustomsBroker;
				if (!customsBroker.IsEmpty)
				{
					var entityIDCode = ActionCodeTool.IsInBondType(actionCode) ? EntityIDCodeList.Codes.SecondaryNotifyParty : EntityIDCodeList.Codes.CustomsBroker;
					yield return new Entity() { EntityCode = entityIDCode, CodeQualifier = ABIRoutingCodeSNPQualifier, IDCode = customsBroker.E2_GovRegNum, EntityName = customsBroker.E2_CompanyName.Left(35) };
				}

				if (arePartiesAllowed && (BillOfLadingStatusIndicatorList.IsISF(((ICommonBillOfLading)this).BillOfLadingStatusIndicator, ZZCustomsFunctionality.IsAMSHBREffective)))
				{
					if (!bill.ShipToParty.IsEmpty)
					{
						yield return ShipToParty;
					}
					if (!bill.BookingParty.IsEmpty)
					{
						yield return BookingParty;
					}
				}
			}
		}
		public const string DUNSQualifier = "1 ";
		public const string SCACOrFIRMSOfSNPQualifier = "2 ";
		public const string ABIRoutingCodeSNPQualifier = "17";

		#endregion

		#region IBaseBillOfLading

		IManifestMessageAttachee IBaseBillOfLading.MessageAttachee
		{
			get { return MoveHeader; }
		}

		#endregion

		#region IMovemenDetails Members

		ZString IMovemenDetails.PreviousInBondNumber
		{
			get
			{
				var moveDetail = InBondMoveDetail;
				return moveDetail == null ? ZString.Empty : moveDetail.B9_PreviousITNumber;
			}
		}

		ZInt IMovemenDetails.InBondQuantity
		{
			get
			{
				var moveDetail = InBondMoveDetail;
				return moveDetail == null ? ZInt.Zero : moveDetail.B9_InBoundQty;
			}
		}

		ZString IMovemenDetails.InbondEntryType
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_InBondEntryType;
			}
		}

		ZBool IMovemenDetails.IsBTAFDA
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader != null && moveHeader.IsBTAFDA;
			}
		}

		ZString InBondNumber
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.InBondNumber;
			}
		}

		ZString IMovemenDetails.ConventionalInbondNumber
		{
			get
			{
				var inbondNumber = InBondNumber;
				return inbondNumber.Length < 10 ? inbondNumber : ZString.Empty;
			}
		}

		ZString IMovemenDetails.InbondCarrierCode
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_InBondCarrierSCAC;
			}
		}

		ZString IMovemenDetails.USPortOfDestination
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_DestinationPortCode;
			}
		}

		ZString IMovemenDetails.ForeignDestination
		{
			get
			{
				var moveDetail = InBondMoveDetail;
				return moveDetail == null ? ZString.Empty : moveDetail.B9_ForeignDestPortKCode;
			}
		}

		ZInt IMovemenDetails.Value
		{
			get
			{
				var moveDetail = InBondMoveDetail;
				return moveDetail == null ? ZInt.Zero : moveDetail.B9_MonetaryValue.ToZInt();
			}
		}

		ZString IMovemenDetails.BondedCarrierID
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_InBondCarrierID;
			}
		}

		ZString IMovemenDetails.PaperlessInbondNumber
		{
			get
			{
				var inbondNumber = InBondNumber;
				return inbondNumber.Length > 9 ? inbondNumber : ZString.Empty;
			}
		}

		ZString IMovemenDetails.ExportVesselName
		{
			get
			{
				var moveDetail = InBondMoveDetail;
				return moveDetail == null ? ZString.Empty : moveDetail.B9_ExportLadenOn;
			}
		}

		ZDateTime IMovemenDetails.ArrivalDateTime
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZDateTime.Empty : moveHeader.BM_ArrivalDate;
			}
		}

		ZDateTime IMovemenDetails.ExportDateTime
		{
			get
			{
				var moveDetail = InBondMoveDetail;
				return moveDetail == null ? ZDateTime.Empty : moveDetail.B9_ExportDate;
			}
		}

		ZString IMovemenDetails.TOLInBondCarrierCode
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_TOLCarrierCode;
			}
		}

		ZString IMovemenDetails.TOLBondedCarrierID
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_TOLCarrierID;
			}
		}

		ZDateTime IMovemenDetails.TOLDateTime
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZDateTime.Empty : moveHeader.BM_TOLDate;
			}
		}

		ZString IMovemenDetails.TOLCityName
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_TOLCityName;
			}
		}

		ZString IMovemenDetails.TOLStateCode
		{
			get
			{
				var moveHeader = InBondMovement;
				return moveHeader == null ? ZString.Empty : moveHeader.BM_TOLStateCode;
			}
		}

		#endregion
	}
}
