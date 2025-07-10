using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[DependentBusinessObject(typeof(DtbConsignmentAddress), "Actions")]
	[UniversalDataContext(DataContextType.LandTransportConsignmentAction)]
	[CodeProperty("LTA_ActionID")]
	public class DtbConsignmentAction : AutoDtbConsignmentAction, IConsignmentAction, IDtbConsignmentAction, INumberFountainConsumer, IDocManagerSupport
	{
		public DtbConsignmentAction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		#region Consignment

		public DtbConsignment Consignment
		{
			get { return ConsignmentAddress?.Consignment; }
		}

		#endregion

		#region RunSheetInstruction

		public DtbConsignmentRunSheetInstruction RunSheetInstruction
		{
			get { return Factory.Load<DtbConsignmentRunSheetInstruction>(LTA_K1_RunSheetInstruction); }
		}

		#endregion

		#region ConsignmentAddress

		public DtbConsignmentAddress ConsignmentAddress
		{
			get { return Factory.Load<DtbConsignmentAddress>(LTA_LTS_ConsignmentAddress); }
		}

		#endregion

		#endregion

		#region Properties

		#region LTA_ActionType
		
		[ResourceStringData("DtbConsignmentAction|LTA_ActionType", Caption = "Job Type", ShortCaption = "Job")]
		public override ZString LTA_ActionType
		{
			get { return base.LTA_ActionType; }
			set { base.LTA_ActionType = value; }
		}

		#endregion

		#region KK_KN_BookingInstruction

		[RelatedBusinessObject("ConsignmentAddress")]
		public override ZGuid LTA_LTS_ConsignmentAddress
		{
			get { return base.LTA_LTS_ConsignmentAddress; }
			set
			{
				base.LTA_LTS_ConsignmentAddress = value;

				if (ConsignmentAddress != null)
				{
					ConsignmentAddress.UpdateStatus();
				}
			}
		}

		#endregion

		public DtbConsignmentLegCollection PickupLegs
		{
			get { return new DtbConsignmentLegCollection(this, DtbConsignmentLegSchema.LTG_LTA_Pickup); }
		}

		public DtbConsignmentLegCollection DeliveryLegs
		{
			get { return new DtbConsignmentLegCollection(this, DtbConsignmentLegSchema.LTG_LTA_Delivery); }
		}
		#endregion

		#region Flags

		#region IsPickUp

		public bool IsPickUp
		{
			get { return LTA_ActionType.EqualsIgnoringCase(ActionTypes.Codes.PickUp); }
		}

		#endregion

		#region IsDelivery

		public bool IsDelivery
		{
			get { return LTA_ActionType.EqualsIgnoringCase(ActionTypes.Codes.Delivery); }
		}

		#endregion

		#region IsOwnDepot

		public ZBool IsOwnDepot
		{
			get { return ConsignmentAddress != null && ConsignmentAddress.IsOwnDepot; }
		}

		#endregion

		#endregion

		#region LoosePackageIds

		public IEnumerable<ZString> LoosePackageIds => Consignment?.LoosePackages.Select(p => p.KP_PackageID) ?? Enumerable.Empty<ZString>();

		#endregion

		#region PackageTotalsBusinessObject

		PackageTotals PackageTotalsBusinessObject
		{
			get
			{
				if (packageTotalsBusinessObject == null && KeyForCache != null)
				{
					CalculatePackageTotalsForAllActions();
				}

				return packageTotalsBusinessObject;
			}
			set { packageTotalsBusinessObject = value; }
		}

		void CalculatePackageTotalsForAllActions()
		{
			var packageTotals = Factory.GetCachedActionsTotals(KeyForCache);
			if (packageTotals != null)
			{
				Func<PackageTotals> result;
				if (packageTotals.TryGetValue(PK, out result))
				{
					PackageTotalsBusinessObject = result();
					packageTotals.Remove(PK); // remove all references held by the delegate
				}
			}
		}

		PackageTotals packageTotalsBusinessObject;

		#endregion

		#region Calculated Properties

		[ResourceStringData("DtbConsignmentAction|IsHazardous", Caption = "Hazardous", ShortCaption = "Haz.")]
		public ZBool IsHazardous
		{
			get { return Consignment.LTC_IsHazardous; }
		}

		[ResourceStringData("DtbConsignmentAction|RequiresRefrigeration", Caption = "Refrigeration", ShortCaption = "Re-frig.")]
		public ZBool RequiresRefrigeration
		{
			get { return Consignment.LTC_RequiresRefrigeration; }
		}

		public GroupedPackTypeCounts BookedPickupPackageList
		{
			get { return PackageTotalsBusinessObject != null ? PackageTotalsBusinessObject.BookedPickupPackageList : GroupedPackTypeCounts.Empty; }
		}

		#endregion

		#region IConsignmentAction

		public string KeyForCache
		{
			get { return keyForCache; }
			set
			{
				if (value == null)
				{
					throw new InvalidOperationException("Should not be setting KeyForCache to null.");
				}
				else if (keyForCache != null)
				{
					throw new InvalidOperationException("Should not be setting KeyForCache twice.");
				}

				keyForCache = value;
			}
		}
		string keyForCache;

		[ResourceStringData("DtbConsignmentAction|TotalPackages", Caption = "No. Packages", MediumCaption = "Packages", ShortCaption = "Packs")]
		public ZInt TotalPackages
		{
			get { return GetTotal(DtbConsignmentConfirmationTotalsCache.TotalPackages, c => c.TotalPackages); }
		}

		[ResourceStringData("DtbConsignmentAction|TotalVolume", Caption = "Volume", ShortCaption = "Vol.")]
		public ZDecimal TotalVolume
		{
			get { return GetTotal(DtbConsignmentConfirmationTotalsCache.TotalVolume, c => c.TotalVolume); }
		}

		[ResourceStringData("DtbConsignmentAction|TotalWeight", Caption = "Weight", ShortCaption = "Wgt.")]
		public ZDecimal TotalWeight
		{
			get { return GetTotal(DtbConsignmentConfirmationTotalsCache.TotalWeight, c => c.TotalWeight); }
		}

		[ResourceStringData("DtbConsignmentAction|TotalWeightUnit", Caption = "Weight Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public ZString TotalWeightUnit
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		[ResourceStringData("DtbConsignmentAction|TotalVolumeUnit", Caption = "Volume Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public ZString TotalVolumeUnit
		{
			get { return DtbTransportTotalsHelper.TotalVolumeUnit; }
		}

		public ZInt ActionQuantity
		{
			get { return (ZInt)Consignment.PackageJob?.Packages.Sum(p => p.KP_PackageQty); }
		}

		public ZDateTime Estimated
		{
			get { return LTA_EstimatedTime.ToLocalZDateTime(); }
		}

		public ZDateTime RequiredFrom
		{
			get { return LTA_RequiredFrom.ToLocalZDateTime(); }
		}

		public ZDateTime RequiredTo
		{
			get { return LTA_RequiredTo.ToLocalZDateTime(); }
		}

		public ZString ReferenceNumber
		{
			get { return LTA_ReferenceNumber; }
		}

		public ZString ActionType
		{
			get { return LTA_ActionType; }
		}

		IConsignmentAddress IConsignmentAction.ConsignmentAddress
		{
			get { return ConsignmentAddress; }
		}

		AutoDtbBookingInstructionPkgDivot IConsignmentAction.PackageDivot => PackageDivot;

		public DtbTransportInstructionPkgDivot PackageDivot
		{
			get { return null; }
		}

		public ZBool IsEmptyContainer
		{
			get { return false; }
		}

		#region ConsignorOrConsigneeAddress

		public ZString ConsignorOrConsigneeAddress
		{
			get
			{
				var result = "";
				if (IsPickUp)
				{
					result = GetInlineAddress(Consignment.PickupAddress?.Address);
				}
				else
				{
					result = GetInlineAddress(Consignment.DeliveryAddress?.Address);
				}
				return result;
			}
		}

		string GetInlineAddress(JobDocAddress address)
		{
			var result = "";
			if (address != null)
			{
				var cityAndState = address.City + " " + address.State;
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(address.CompanyName);
				builder.AppendIfNotEmpty(cityAndState.Trim());
				{
					result = builder.ToStringWithDelimiterBetweenAppends(" - ");
				}
			}

			return result;
		}

		#endregion

		#region GetTotal

		T GetTotal<T>(ZString cachePropertyName, Func<DtbConsignmentAction, T> getTotalForDepot)
			where T : IZType
		{
			return GetTotal(() => (T)PackageTotalsBusinessObject[cachePropertyName], getTotalForDepot);
		}

		T GetTotal<T>(Func<T> getSelfValue, Func<DtbConsignmentAction, T> getTotalForDepot)
			where T : IZType
		{
			var result = default(T);

			if (PackageTotalsBusinessObject != null)
			{
				result = getSelfValue();
			}
			else if (IsOwnDepot)
			{
				var relatedAction = GetRelatedActionForDepot();
				result = getTotalForDepot(relatedAction);
			}

			return result;
		}

		#endregion

		#region GetRelatedActionForDepot

		DtbConsignmentAction GetRelatedActionForDepot()
		{
			if (!IsOwnDepot)
			{
				throw new InvalidOperationException("GetRelatedActionForDepot() should only be invoked from a Depot Action.");
			}

			return IsPickUp
				? Consignment.DeliveryAddress?.DeliveryAction
				: Consignment.PickupAddress?.PickupAction;
		}

		#endregion

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => LTA_ActionID;
			set => LTA_ActionID = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.DtbConsignmentActionID;

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
			=> docManagerInfo ?? (docManagerInfo = DocManagerInfo.New(this, Core.Constants.DocManagerCodes.LandTransportConsignmentAction));

		DocManagerInfo docManagerInfo;

		#endregion

		#region Calculated Properties

		#region HasSignature

		[ResourceStringData("DtbConsignmentAction|HasSignature", ShortCaption = "Signature", Caption = "Has Signature")]
		public ZBool HasSignature
		{
			get { return !ReceivedBySignature.IsEmpty; }
		}

		public ZBlob ReceivedBySignature
		{
			get { return FallbackToRunSheetInstructionIfEmpty(LTA_SignedBySignature, rsi => rsi.K1_ReceivedBySignature); }
		}

		#endregion

		#region ReceivedBy

		[ResourceStringData("DtbConsignmentAction|ReceivedBy", Caption = "Signed By")]
		public ZString ReceivedBy
		{
			get { return FallbackToRunSheetInstructionIfEmpty(LTA_SignedBy, rsi => rsi.K1_ReceivedBy); }
		}

		#endregion

		#region FallbackToRunSheetInstructionIfEmpty

		T FallbackToRunSheetInstructionIfEmpty<T>(T onAction, Func<DtbConsignmentRunSheetInstruction, T> getFromRunSheetInstruction)
			where T : IZType
		{
			var value = onAction;

			if (value.IsEmpty)
			{
				var runSheetInstruction = RunSheetInstruction;
				value = runSheetInstruction != null ? getFromRunSheetInstruction(runSheetInstruction) : value;
			}

			return value;
		}

		#endregion

		#region BillToPartyCode

		[ResourceStringData("DtbConsignmentAction|BillToPartyCode", Caption = "Bill to Party", ShortCaption = "Bill To")]
		public ZString BillToPartyCode
		{
			get
			{
				ZString result = ZString.Empty;

				var job = Consignment.Job;
				if (job != null)
				{
					var billToParty = job.LocalCharges;
					result = (billToParty != null) ? billToParty.OH_Code : ZString.Empty;
				}

				return result;
			}
		}

		#endregion

		#region BookingID

		[ResourceStringData("DtbConsignmentAction|BookingID", Caption = "Booking Number", MediumCaption = "Booking #", ShortCaption = "Booking")]
		public ZString BookingID
		{
			get { return ConsignmentAddress?.Consignment?.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportCommonAdditionalReferenceTypes.Codes.BookingJobId)?.CE_EntryNum ?? ZString.Empty; }
		}

		#endregion

		#region ConsignmentID

		[ResourceStringData("DtbConsignmentAction|ConsignmentID", Caption = "Consignment ID")]
		public ZString ConsignmentID
		{
			get { return ConsignmentAddress?.Consignment?.LTC_JobID ?? ZString.Empty; }
		}

		#endregion

		#region Consignor

		#region ConsignorAddressAsSingleLine

		[ResourceStringData("DtbConsignmentAction|ConsignorAddressAsSingleLine", Caption = "Origin Full Address", ShortCaption = "Origin")]
		public ZString ConsignorAddressAsSingleLine
		{
			get { return Consignment.PickupAddress?.Address?.GetAddressLine(Factory) ?? ZString.Empty; }
		}

		#endregion

		#region ConsignorCity

		[ResourceStringData("DtbConsignmentAction|ConsignorCity", Caption = "Origin City")]
		public ZString ConsignorCity
		{
			get { return Consignment.PickupAddress?.Address?.E2_City ?? ZString.Empty; }
		}

		#endregion

		#region ConsignorName

		[ResourceStringData("DtbConsignmentAction|ConsignorName", Caption = "Consignor")]
		public ZString ConsignorName
		{
			get { return Consignment.PickupAddress?.Address?.E2_CompanyNameTruncated ?? ZString.Empty; }
		}

		#endregion

		#region ConsignorPostcode

		[ResourceStringData("DtbConsignmentAction|ConsignorPostcode", Caption = "Origin Postcode", ShortCaption = "Origin P/C")]
		public ZString ConsignorPostcode
		{
			get { return Consignment.PickupAddress?.Address?.E2_Postcode ?? ZString.Empty; }
		}

		#endregion

		#region ConsignorState

		[ResourceStringData("DtbConsignmentAction|ConsignorState", Caption = "Origin State")]
		public ZString ConsignorState
		{
			get { return Consignment.PickupAddress?.Address?.E2_State ?? ZString.Empty; }
		}

		#endregion

		#region ConsignorReference

		[ResourceStringData("DtbConsignmentAction|ConsignorReference", Caption = "Consignor Reference", MediumCaption = "Consignor Ref.", ShortCaption = "CNR Ref.")]
		public ZString ConsignorReference
		{
			get { return IsPickUp ? LTA_ReferenceNumber : Consignment.PickupAddress?.Actions[0]?.LTA_ReferenceNumber ?? ZString.Empty; }
		}

		#endregion

		#endregion

		#region Consinee

		#region ConsigneeAddressAsSingleLine

		[ResourceStringData("DtbConsignmentAction|ConsigneeAddressAsSingleLine", Caption = "Destination Full Address", MediumCaption = "Destination", ShortCaption = "Dest.")]
		public ZString ConsigneeAddressAsSingleLine
		{
			get { return Consignment.DeliveryAddress?.Address?.GetAddressLine(Factory) ?? ZString.Empty; }
		}

		#endregion

		#region ConsigneeCity

		[ResourceStringData("DtbConsignmentAction|ConsigneeCity", Caption = "Destination City", ShortCaption = "Dest. City")]
		public ZString ConsigneeCity
		{
			get { return Consignment.DeliveryAddress?.Address?.E2_City ?? ZString.Empty; }
		}

		#endregion

		#region ConsigneeName

		[ResourceStringData("DtbConsignmentAction|ConsigneeName", Caption = "Consignee")]
		public ZString ConsigneeName
		{
			get { return Consignment.DeliveryAddress?.Address?.E2_CompanyNameTruncated ?? ZString.Empty; }
		}

		#endregion

		#region ConsigneePostcode

		[ResourceStringData("DtbConsignmentAction|ConsigneePostcode", Caption = "Destination Postcode", MediumCaption = "Destination P/C", ShortCaption = "Dest. P/C")]
		public ZString ConsigneePostcode
		{
			get { return Consignment.DeliveryAddress?.Address?.E2_Postcode ?? ZString.Empty; }
		}

		#endregion

		#region ConsigneeState

		[ResourceStringData("DtbConsignmentAction|ConsigneeState", Caption = "Destination State", ShortCaption = "Dest. State")]
		public ZString ConsigneeState
		{
			get { return Consignment.DeliveryAddress?.Address?.E2_State ?? ZString.Empty; }
		}

		#endregion

		#region ConsigneeReference

		[ResourceStringData("DtbConsignmentAction|ConsigneeReference", Caption = "Consignee Reference", MediumCaption = "Consignee Ref.", ShortCaption = "CNE Ref.")]
		public ZString ConsigneeReference
		{
			get { return IsDelivery ? LTA_ReferenceNumber : Consignment.DeliveryAddress?.Actions[0]?.LTA_ReferenceNumber ?? ZString.Empty; }
		}

		public DtbConsignmentActionPackageDivotCollection PackageDivots
		{
			get { return new DtbConsignmentActionPackageDivotCollection(this); }
		}

		#endregion

		#endregion

		#region Packages

		public IEnumerable<PkgPackage> Packages
		{
			get
			{
				return PackageDivots.Select(d => d.Package);
			}
		}

		#endregion

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (LTA_LTS_ConsignmentAddress.IsEmpty)
			{
				var helper = new Testing.TransportConsignmentTestHelper(Factory);
				var consignment = helper.CreateConsignment(ZString.Empty);
				consignment.FillWithValidTestData(kind, null);

				var address = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
				address.LTS_Sequence = 1;
				address.FillWithValidTestData(kind, null);

				LTA_LTS_ConsignmentAddress = address.PK;
			}

			if (LTA_ActionType.IsEmpty)
			{
				LTA_ActionType = ActionTypeList.Codes.PickUp;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
