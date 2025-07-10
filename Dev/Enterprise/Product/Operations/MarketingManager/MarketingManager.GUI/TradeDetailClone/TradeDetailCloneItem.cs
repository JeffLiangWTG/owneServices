using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class TradeDetailCloneItem : TradeDetailSelectionItem
	{
		public TradeDetailCloneItem(OrgTradeDetail tradeDetail)
			: base(tradeDetail)
		{
			SetDetailDefaultValues();
		}

		void SetDetailDefaultValues()
		{
			var prospect = TradeDetail.ProspectDetail;
			var period = TradeDetail.CurrentProspectPeriod;

			recurrenceType = prospect.PAP_RecurrenceType;
			containerType = prospect.PAP_RC_NKContainer;
			containerCount = period.PAS_Units;
			weight = period.PAS_Weight;
			weightUQ = period.PAS_WeightUQ;
			volume = period.PAS_Volume;
			volumeUQ = period.PAS_VolumeUQ;
			rateOffered = period.PAS_RateOffered;
			jobCount = period.PAS_RepeatsMnth;
			estimatedValue = period.PAS_EstimatedProfit;
			currency = period.PAS_RX_NKCurrency;
		}

		void ResetDetailDefaultValues()
		{
			SetDetailDefaultValues();
			RunPreSaveValidation();

			RecurrenceTypeInfo.RefreshBinding();
			ContainerTypeInfo.RefreshBinding();
			ContainerCountInfo.RefreshBinding();
			WeightInfo.RefreshBinding();
			WeightUQInfo.RefreshBinding();
			VolumeInfo.RefreshBinding();
			VolumeUQInfo.RefreshBinding();
			RateOfferedInfo.RefreshBinding();
			JobCountInfo.RefreshBinding();
			EstimatedValueInfo.RefreshBinding();
			CurrencyInfo.RefreshBinding();
		}

		#region Properties

		#region Selected

		public override ZBool Selected
		{
			get => base.Selected;
			set
			{
				if (base.Selected != value)
				{
					base.Selected = value;
					if (!value)
					{
						ResetDetailDefaultValues();
					}
				}
			}
		}

		#endregion

		#region Recurrence Type

		[List("RecurrenceTypes")]
		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|RecurrenceType", Caption = "Recurrence")]
		public ZString RecurrenceType
		{
			get { return recurrenceType; }
			set
			{
				SetNonPersistentPropertyValue(RecurrenceTypeInfo, ref recurrenceType, value);
				if (!IsValidationSuspended)
				{
					ValidateRecurrenceType();
				}

				TotalEstimatedValueInfo.RefreshBinding();
			}
		}
		ZString recurrenceType;
		public ZPropertyInfo RecurrenceTypeInfo => GetZPropertyInfo(nameof(RecurrenceType));

		public CodeDescriptionPairList RecurrenceTypes => tradeDetail.ProspectDetail.Lookups.RecurrenceTypes;

		public bool RecurrenceType_ReadOnly => !Selected;

		#endregion

		#region Container Type

		[List("Containers")]
		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|ContainerType", Caption = "Container")]
		public ZString ContainerType
		{
			get { return containerType; }
			set
			{
				SetNonPersistentPropertyValue(ContainerTypeInfo, ref containerType, value);
				ContainerCount = value.IsEmpty ? 0 : 1;
				if (!IsValidationSuspended)
				{
					ValidateContainerType();
				}
			}
		}
		ZString containerType;
		public ZPropertyInfo ContainerTypeInfo => GetZPropertyInfo(nameof(ContainerType));

		public bool ContainerType_ReadOnly
		{
			get
			{
				if (!Selected)
				{
					return true;
				}
				else
				{
					var product = tradeDetail.Sales?.Product;
					return product != null && !product.IsContainerTypeAllowed(tradeDetail);
				}
			}
		}

		public RefContainer Container => Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, ContainerType);

		public RefContainerCollection Containers => tradeDetail.ProspectDetail.Lookups.Containers;

		#endregion

		#region Container Count

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|ContainerCount", Caption = "Container Count", ShortCaption = "CN Count")]
		public ZLong ContainerCount
		{
			get { return containerCount; }
			set
			{
				SetNonPersistentPropertyValue(ContainerCountInfo, ref containerCount, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerCount();
				}

				RecalculateEstimatedRevenueIfProspect();
			}
		}
		ZLong containerCount;
		public ZPropertyInfo ContainerCountInfo => GetZPropertyInfo(nameof(ContainerCount));

		public bool ContainerCount_ReadOnly => !Selected || Container == null;

		#endregion

		#region TEU

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|TEUQuantity", Caption = "TEU")]
		public ZDecimal TEUQuantity
		{
			get
			{
				var result = ZDecimal.Zero;
				var container = Container;
				if (container != null)
				{
					result = container.RC_TEU * ContainerCount;
				}
				return result;
			}
		}
		public ZPropertyInfo TEUQuantityInfo => GetZPropertyInfo(nameof(TEUQuantity));

		public bool TEUQuantity_ReadOnly => !Selected;

		#endregion

		#region Weight

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|Weight", Caption = "Weight")]
		public ZDecimal Weight
		{
			get { return weight; }
			set
			{
				SetNonPersistentPropertyValue(WeightInfo, ref weight, value);
				if (!IsValidationSuspended)
				{
					ValidateWeight();
				}

				RecalculateEstimatedRevenueIfProspect();
			}
		}
		ZDecimal weight;
		public ZPropertyInfo WeightInfo => GetZPropertyInfo(nameof(Weight));

		public bool Weight_ReadOnly => !Selected;

		#endregion

		#region Weight UQ

		[List("UnitOfWeightList")]
		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|WeightUQ", Caption = "Weight Unit", ShortCaption = "Unit")]
		public ZString WeightUQ
		{
			get { return weightUQ; }
			set
			{
				SetNonPersistentPropertyValue(WeightUQInfo, ref weightUQ, value);
				if (!IsValidationSuspended)
				{
					ValidateWeightUQ();
				}

				RecalculateEstimatedRevenueIfProspect();
			}
		}
		ZString weightUQ;
		public ZPropertyInfo WeightUQInfo => GetZPropertyInfo(nameof(WeightUQ));

		public bool WeightUQ_ReadOnly => !Selected;

		public CodeDescriptionPairList UnitOfWeightList => tradeDetail.CurrentProspectPeriod.Lookups.UnitOfWeightList;

		#endregion

		#region Volume

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|Volume", Caption = "Volume")]
		public ZDecimal Volume
		{
			get { return volume; }
			set
			{
				SetNonPersistentPropertyValue(VolumeInfo, ref volume, value);
				if (!IsValidationSuspended)
				{
					ValidateVolume();
				}

				RecalculateEstimatedRevenueIfProspect();
			}
		}
		ZDecimal volume;
		public ZPropertyInfo VolumeInfo => GetZPropertyInfo(nameof(Volume));

		public bool Volume_ReadOnly => !Selected;

		#endregion

		#region Volume UQ

		[List("UnitOfVolumeList")]
		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|VolumeUQ", Caption = "Volume Unit", ShortCaption = "Unit")]
		public ZString VolumeUQ
		{
			get { return volumeUQ; }
			set
			{
				SetNonPersistentPropertyValue(VolumeUQInfo, ref volumeUQ, value);
				if (!IsValidationSuspended)
				{
					ValidateVolumeUQ();
				}

				RecalculateEstimatedRevenueIfProspect();
			}
		}
		ZString volumeUQ;
		public ZPropertyInfo VolumeUQInfo => GetZPropertyInfo(nameof(VolumeUQ));

		public bool VolumeUQ_ReadOnly => !Selected;

		public CodeDescriptionPairList UnitOfVolumeList => tradeDetail.CurrentProspectPeriod.Lookups.UnitOfVolumeList;

		#endregion

		#region Offer Rate

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|RateOffered", Caption = "Offer Rate")]
		public ZDecimal RateOffered
		{
			get { return rateOffered; }
			set
			{
				SetNonPersistentPropertyValue(RateOfferedInfo, ref rateOffered, value);
				if (!IsValidationSuspended)
				{
					ValidateRateOffered();
				}

				RecalculateEstimatedRevenueIfProspect();
			}
		}
		ZDecimal rateOffered;
		public ZPropertyInfo RateOfferedInfo => GetZPropertyInfo(nameof(RateOffered));

		public bool RateOffered_ReadOnly => !Selected;

		#endregion

		#region Job Count

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|JobCount", Caption = "Job Count")]
		public ZDecimal JobCount
		{
			get { return jobCount; }
			set
			{
				SetNonPersistentPropertyValue(JobCountInfo, ref jobCount, value);
				if (!IsValidationSuspended)
				{
					ValidateJobCount();
				}

				TotalEstimatedValueInfo.RefreshBinding();
			}
		}
		ZDecimal jobCount;
		public ZPropertyInfo JobCountInfo => GetZPropertyInfo(nameof(JobCount));

		public bool JobCount_ReadOnly => !Selected;

		#endregion

		#region Estimated Value

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|EstimatedValue", Caption = "Estimated Value", ShortCaption = "Est Value")]
		public ZDecimal EstimatedValue
		{
			get { return estimatedValue; }
			set
			{
				SetNonPersistentPropertyValue(EstimatedValueInfo, ref estimatedValue, value);
				if (!IsValidationSuspended)
				{
					ValidateEstimatedValue();
				}

				TotalEstimatedValueInfo.RefreshBinding();
			}
		}
		ZDecimal estimatedValue;
		public ZPropertyInfo EstimatedValueInfo => GetZPropertyInfo(nameof(EstimatedValue));

		public bool EstimatedValue_ReadOnly => !Selected;

		void RecalculateEstimatedRevenueIfProspect()
		{
			if (TradeDetail.ShouldDefaultProperties)
			{
				var period = TradeDetail.CurrentProspectPeriod;
				EstimatedValue = OrgTradePeriod.GetCalculatedEstimatedRevenue(
					TradeDetail.SalesProductCode,
					(ZDecimal)period.PAS_PalletCount,
					TradeDetail.IsFreight,
					TradeDetail.IsSeaFcl,
					containerCount,
					GetChargeable(period),
					rateOffered
					);
			}
		}

		ZDecimal GetChargeable(OrgTradePeriod period)
		{
			if (!TradeDetail.CurrentProspectPeriod.PAS_IsTraded && !VolumeInfo.HasErrors() && !WeightInfo.HasErrors())
			{
				return OrgTradePeriod.CalculateChargeable(Weight, WeightUQ, Volume, VolumeUQ,
					period.PAS_Calc_ChargeableUQ, TradeDetail.Sales.IsDomestic, TradeDetail.TransportMode);
			}
			return period.PAS_Chargeable;
		}

		#endregion

		#region Currency

		[List("Currencies")]
		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|Currency", Caption = "Currency", ShortCaption = "Cur")]
		public ZString Currency
		{
			get { return currency; }
			set
			{
				SetNonPersistentPropertyValue(CurrencyInfo, ref currency, value);
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
			}
		}
		ZString currency;
		public ZPropertyInfo CurrencyInfo => GetZPropertyInfo(nameof(Currency));

		public bool Currency_ReadOnly => !Selected;

		public RefCurrencyCollection Currencies => tradeDetail.CurrentProspectPeriod.Lookups.Currencies;

		#endregion

		#region Annual Estimated Value

		[ResourceStringData("Enterprise.MarketingManager.GUI.TradeDetailCloneItem|TotalEstimatedValue",
			Caption = "Total Estimate Value (p.a)", ShortCaption = "Total Est Value")]
		public ZDecimal TotalEstimatedValue
		{
			get
			{
				var repeats = (JobCount > 0) ? JobCount : 1;
				return OrgTradeDetail.GetAnnualAmount(repeats, EstimatedValue, RecurrenceType);
			}
		}

		public ZPropertyInfo TotalEstimatedValueInfo => GetZPropertyInfo(nameof(TotalEstimatedValue));

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRecurrenceType();
			ValidateContainerType();
			ValidateWeight();
			ValidateWeightUQ();
			ValidateVolume();
			ValidateVolumeUQ();
			ValidateRateOffered();
			ValidateJobCount();
			ValidateEstimatedValue();
			ValidateCurrency();
		}

		void ValidateRecurrenceType()
		{
			RecurrenceTypeInfo.ClearAllNotifications();
			if (Selected)
			{
				MandatoryValidation.CheckEntered(RecurrenceTypeInfo);
				ListValidation.ErrorIfInvalidCode(RecurrenceTypeInfo);
			}
		}

		void ValidateContainerType()
		{
			ContainerTypeInfo.ClearAllNotifications();
			if (Selected)
			{
				ListValidation.ErrorIfInvalidCode(ContainerTypeInfo);
			}
		}

		void ValidateContainerCount()
		{
			ContainerCountInfo.ClearAllNotifications();
			if (Selected)
			{
				CompareValidation.CheckNumberNotNegative(ContainerCountInfo);
			}
		}

		void ValidateWeight()
		{
			WeightInfo.ClearAllNotifications();
			if (Selected)
			{
				CompareValidation.CheckNumberNotNegative(WeightInfo);
			}
		}

		void ValidateWeightUQ()
		{
			WeightUQInfo.ClearAllNotifications();
			if (Selected)
			{
				ListValidation.ErrorIfInvalidCode(WeightUQInfo);
			}
		}

		void ValidateVolume()
		{
			VolumeInfo.ClearAllNotifications();
			if (Selected)
			{
				CompareValidation.CheckNumberNotNegative(VolumeInfo);
			}
		}

		void ValidateVolumeUQ()
		{
			VolumeUQInfo.ClearAllNotifications();
			if (Selected)
			{
				ListValidation.ErrorIfInvalidCode(VolumeUQInfo);
			}
		}

		void ValidateRateOffered()
		{
			RateOfferedInfo.ClearAllNotifications();
			if (Selected)
			{
				CompareValidation.CheckNumberNotNegative(RateOfferedInfo);
			}
		}

		void ValidateJobCount()
		{
			JobCountInfo.ClearAllNotifications();
			if (Selected)
			{
				CompareValidation.CheckNumberNotNegative(JobCountInfo);
			}
		}

		void ValidateEstimatedValue()
		{
			EstimatedValueInfo.ClearAllNotifications();
			if (Selected)
			{
				CompareValidation.CheckNumberNotNegative(EstimatedValueInfo);
			}
		}

		void ValidateCurrency()
		{
			CurrencyInfo.ClearAllNotifications();
			if (Selected)
			{
				MandatoryValidation.CheckEntered(CurrencyInfo);
				ListValidation.ErrorIfInvalidCode(CurrencyInfo);
			}
		}

		#endregion
	}
}
