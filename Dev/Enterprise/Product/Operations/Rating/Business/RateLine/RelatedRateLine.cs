using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	[DependentBusinessObject(typeof(RateEntry), "RelatedRateLines")]
	public class RelatedRateLine : RateLine
	{
		public RelatedRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		protected override RateLinesValidation GetNewValidation()
		{
			return new RelatedRateLinesValidation(this);
		}

		public new RelatedRateLinesValidation Validation
		{
			get { return (RelatedRateLinesValidation)base.Validation; }
		}

		#endregion

		#region Properties

		#region TL_TI

		public override ZGuid TL_TI
		{
			get { return base.TL_TI; }
			set
			{
				if (base.TL_TI != value)
				{
					base.TL_TI = value;
					RateLineItems.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		#endregion

		#region TL_RateCalculator

		public override ZString TL_RateCalculator
		{
			get { return base.TL_RateCalculator; }
			set
			{
				if (base.TL_RateCalculator != value)
				{
					base.TL_RateCalculator = value;
					RateLineItems.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		#endregion

		#region Supplier and Carrier

		#region Supplier

		[List("Lookups.Suppliers")]
		public ZGuid Supplier
		{
			get
			{
				var result = ZGuid.Empty;

				if (RatingHeader.IsTariff())
				{
					result = Parent.TI_OH_Supplier;
				}
				else if (RatingHeader.IsCosting())
				{
					result = RatingHeader.TH_OH;
				}

				return result;
			}
		}

		public ZPropertyInfo SupplierInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Supplier)); }
		}

		#endregion

		#region Carrier

		[List("Lookups.Suppliers")]
		public ZGuid Carrier
		{
			get { return Parent.TI_OH_TransportProvider; }
		}

		public ZPropertyInfo CarrierInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Carrier)); }
		}

		#endregion

		public bool SupplierIsCarrier
		{
			get
			{
				var supplierOrg = Factory.Load<OrgHeader>(Supplier);
				return supplierOrg != null && supplierOrg.OH_IsShippingProvider && supplierOrg.OH_IsAirLine;
			}
		}

		#endregion

		#region Consignee

		[List("Lookups.Consignees")]
		public ZGuid Consignee
		{
			get { return Parent.TI_OH_Consignee; }
		}

		public ZPropertyInfo ConsigneeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Consignee)); }
		}

		#endregion

		#region Consignor

		[List("Lookups.Consignors")]
		public ZGuid Consignor
		{
			get { return Parent.TI_OH_Consignor; }
		}

		public ZPropertyInfo ConsignorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Consignor)); }
		}

		#endregion

		#region Charge Code Description

		public ZString ChargeCodeDescription
		{
			get { return TL_RateDesc; }
		}

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCodeDescription)); }
		}

		#endregion

		#region Rate Type

		public ZString RateType
		{
			get { return Header.RatingHeaderTypeDescription; }
		}

		public ZPropertyInfo RateTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(RateType)); }
		}

		public bool IsTariff
		{
			get { return RatingHeader.IsTariff(); }
		}

		public bool IsCosting
		{
			get { return RatingHeader.IsCosting(); }
		}

		public bool IsClientRate
		{
			get { return RatingHeader.IsClientRate(); }
		}

		#endregion

		#region Mode

		public ZString Mode
		{
			get { return Parent.TI_Mode; }
		}

		public ZPropertyInfo ModeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Mode)); }
		}

		#endregion

		#region Origin

		[List("Lookups.Locations")]
		public ZString Origin
		{
			get { return Parent.TI_OriginLRC; }
		}

		public ZPropertyInfo OriginInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Origin)); }
		}

		#endregion

		#region Destination

		[List("Lookups.Locations")]
		public ZString Destination
		{
			get { return Parent.TI_DestinationLRC; }
		}

		public ZPropertyInfo DestinationInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Destination)); }
		}

		#endregion

		#region Via

		[List("Lookups.Locations")]
		public ZString Via
		{
			get { return Parent.TI_ViaLRC; }
		}

		public ZPropertyInfo ViaInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Via)); }
		}

		#endregion

		#region PlannedLoad

		[List("Lookups.Locations")]
		public ZString PlannedLoad
		{
			get { return Parent.TI_PlannedLoadLRC; }
		}

		public ZPropertyInfo PlannedLoadInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(PlannedLoad)); }
		}

		#endregion

		#region PlannedDischarge

		[List("Lookups.Locations")]
		public ZString PlannedDischarge
		{
			get { return Parent.TI_PlannedDischargeLRC; }
		}

		public ZPropertyInfo PlannedDischargeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(PlannedDischarge)); }
		}

		#endregion

		#region RateOrigin

		[List("Lookups.Locations")]
		public ZString RateOrigin
		{
			get { return Parent.TI_RateOrigin; }
		}

		public ZPropertyInfo RateOriginInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(RateOrigin)); }
		}

		#endregion

		#region RateDestination

		[List("Lookups.Locations")]
		public ZString RateDestination
		{
			get { return Parent.TI_RateDestination; }
		}

		public ZPropertyInfo RateDestinationInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(RateDestination)); }
		}

		#endregion

		#region Commodity Code

		[List("Lookups.CommodityCodes")]
		public ZString CommodityCode
		{
			get { return Parent.TI_RH_NKCommodityCode; }
		}

		public ZPropertyInfo CommodityCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CommodityCode)); }
		}

		#endregion

		#region Service Level

		[List("Lookups.ServiceLevels")]
		public ZString ServiceLevel
		{
			get { return Parent.TI_RS_NKServiceLevel_NI; }
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ServiceLevel)); }
		}

		#endregion

		#region CarrierServiceLevel

		public ZString CarrierServiceLevel
		{
			get { return Parent.TI_PL_NKCarrierServiceLevel; }
		}

		public ZPropertyInfo CarrierServiceLevelInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CarrierServiceLevel)); }
		}

		#endregion

		#region Transit Time

		public ZString TransitTime
		{
			get { return Parent.TI_TransitTime; }
		}

		public ZPropertyInfo TransitTimeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(TransitTime)); }
		}

		#endregion

		#region Frequency

		public ZInt Frequency
		{
			get { return Parent.TI_Frequency; }
		}

		public ZPropertyInfo FrequencyInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(Frequency)); }
		}

		#endregion

		#region FrequencyUnit

		public ZString FrequencyUnit
		{
			get { return Parent.TI_FrequencyUnit; }
		}

		public ZPropertyInfo FrequencyUnitInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(FrequencyUnit)); }
		}

		#endregion

		#region ContainerCode

		public ZString ContainerCode
		{
			get { return Parent.Container == null ? ZString.Empty : Parent.Container.RC_Code; }
		}

		public ZPropertyInfo ContainerCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ContainerCode)); }
		}

		#endregion

		#region ContainerPK

		public ZGuid ContainerPK
		{
			get { return Parent.TI_RC; }
		}

		public ZPropertyInfo ContainerPKInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ContainerPK)); }
		}

		#endregion

		#region Start Date

		public ZDateTime StartDate
		{
			get { return Parent.TI_RateStartDate; }
		}

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(nameof(StartDate)); }
		}

		#endregion

		#region End Date

		public ZDateTime EndDate
		{
			get { return Parent.TI_RateEndDate; }
		}

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(nameof(EndDate)); }
		}

		#endregion

		#region Fees And Charges Type

		[List("Lookups.FeeChargeTypes")]
		public override ZString TL_FeeChargeType
		{
			get { return base.TL_FeeChargeType; }
			set
			{
				if (base.TL_FeeChargeType != value)
				{
					RateLineItems.MarkAsNeedingValidationIncludingChildren();
				}

				base.TL_FeeChargeType = value;
			}
		}

		#endregion

		#region FMC Tariff ID

		[List("Lookups.FMCTariffID")]
		public ZString FMCTariffID
		{
			get { return Parent.TI_FMCTariffID; }
		}

		public ZPropertyInfo FMCTariffIDInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(FMCTariffID)); }
		}

		#endregion

		#region IsNonOperatedReefer

		[List("Lookups.IsNonOperationalReefer")]
		public ZString IsNonOperatedReefer
		{
			get { return Parent.TI_IsNonOperatedReefer; }
		}

		public ZPropertyInfo IsNonOperatedReeferInfo
		{
			get { return GetZPropertyInfo(nameof(IsNonOperatedReefer)); }
		}

		#endregion

		public bool IsSelectedLineChargeCode
		{
			get { return Master != null && ChargeCode.PK == Master.SelectedLineChargeCode; }
		}

		#endregion

		#region Related Business Objects

		[ChildEditable(true)]
		public override RateLineItemsCollection RateLineItems
		{
			get
			{
				if (fRateLineItems == null)
				{
					fRateLineItems = new RelatedRateLineItemsCollection(this);
					fRateLineItems.Load();
					fRateLineItems.Sort();
					RegisterEditableChildObject(RateLineItems);
				}
				return fRateLineItems;
			}
		}

		protected override RateEntry GetParentFromCollections()
		{
			return null;
		}

		public RatingHeader RatingHeader
		{
			get
			{
				if (fRatingHeader == null && Parent != null)
				{
					fRatingHeader = Factory.Load<RatingHeader>(Parent.TI_TH);
				}

				return fRatingHeader;
			}
		}

		RatingHeader fRatingHeader;

		public RateEntry Master
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0 && ((IBusinessObjectInternals)this).ParentCollections[0] is RelatedRateLinesCollection)
				{
					return ((RelatedRateLinesCollection)((IBusinessObjectInternals)this).ParentCollections[0]).Master;
				}
				else
				{
					return null;
				}
			}
		}

		protected override void ResetParentCollection()
		{
			Master.InvalidateRelatedRateLines();
		}

		#endregion

		#region Company Tariff

		public override bool IsTariffLineInherited
		{
			get
			{
				if (RatingHeader != null && Master != null && RatingHeader.IsTariff())
				{
					var direction = OrgRateTariffLevel.GetOrgRateTariffLevelDirection(Master.JobDirection());
					var maxMasterCompanyTariffLevel = Master.GetCompanyTariffLevels(direction).Max();
					return TL_CompanyTariffLevel < maxMasterCompanyTariffLevel;
				}

				return false;
			}
		}

		public override ZDecimal CompanyTariffDiscount
		{
			get
			{
				if (TL_FeeChargeType.IsEmpty && Parent != null && Master != null)
				{
					var direction = OrgRateTariffLevel.GetOrgRateTariffLevelDirection(Master.JobDirection());
					return Parent.GetCompanyTariffDiscountForLevel((byte)Master.GetCompanyTariffLevels(direction).Max());
				}

				return 0m;
			}
		}

		#endregion

		#region AddtionalActionsOnMasterSaving

		protected override void AdditionalActionsOnMasterSaving()
		{
		}

		#endregion
	}
}

