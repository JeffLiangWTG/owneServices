using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.JobShipment)]
	public class CoLoadShipmentCollection : DependentBusinessObjectCollection<CommonShipment, CommonShipment>
	{
		public CoLoadShipmentCollection(CommonShipment master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			CommonShipment newShipment = (CommonShipment)child;

			newShipment.JS_TransportMode = Master.JS_TransportMode;
			if (!Master.IsCoLoadMaster)
			{
				newShipment.ConsigneePK = Master.ConsigneePK;
			}
			newShipment.JS_PackingMode = Master.JS_PackingMode;
			newShipment.JS_RL_NKOrigin = Master.JS_RL_NKOrigin;
			newShipment.JS_RL_NKDestination = Master.JS_RL_NKDestination;
		}

		#endregion

		#region OnLoaded

		protected override void OnLoaded()
		{
			base.OnLoaded();
			_ = OldSubsCommonUnits;
			_ = OldSubsTotalValuesDecimal;
			_ = OldSubsTotalValuesInt;
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return !Master.IsLeadOrMaster; }
		}

		protected override bool AllowNewCore
		{
			get { return Master.IsLeadOrMaster && AllowAddNew; }
		}

		public bool AllowAddNew { get; set; }

		#endregion

		#region Relationship Management

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return Master.GetType();
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobShipmentSchema.JS_JS_ColoadMasterShipment; }
		}

		#endregion

		#region Adding / Removing

		protected override BusinessObject AddNewCore()
		{
			BusinessObject result;
			IsAddingNew = true;

			try
			{
				result = base.AddNewCore();
			}
			finally
			{
				IsAddingNew = false;
			}

			return result;
		}

		bool IsAddingNew;

		public override void Add(BusinessObject businessObject)
		{
			if (!IsAddingNew)
			{
				base.Add(businessObject);
			}

			UnhookSubShipment(businessObject as CommonShipment);
			HookSubShipment(businessObject as CommonShipment);

			if (!IsAddingRange)
			{
				using (Master.SuspendSettingHasChanges())
				{
					RefreshBindingsForMaster();
					RefreshTotalsValidationForMaster();
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var subShipment = bizOAdded as CommonShipment;

			base.OnAdded(subShipment);

			UnhookSubShipment(subShipment);
			HookSubShipment(subShipment);

			if (!IsAddingRange)
			{
				RecalculateMasterValues();
			}
		}

		protected override void OnAddingRange()
		{
			IsAddingRange = true;
		}

		bool IsAddingRange;

		protected override void OnAddedRange(IEnumerable businessObjects)
		{
			RecalculateMasterValues();
			using (Master.SuspendSettingHasChanges())
			{
				RefreshBindingsForMaster();
				RefreshTotalsValidationForMaster();
			}
			base.OnAddedRange(businessObjects);
			IsAddingRange = false;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			var subShipment = bizO as CommonShipment;

			base.OnRemoving(subShipment);

			UnhookSubShipment(bizO as CommonShipment);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (bizO != null && bizO is CommonShipment && !bizO.IsDeleted)
			{
				((CommonShipment)bizO).JS_JS_ColoadMasterShipment = ZGuid.Empty;
			}

			if (!IsRemovingRange)
			{
				RecalculateMasterValues();
				RefreshBindingsForMaster();
				RefreshTotalsValidationForMaster();
			}
		}

		protected override void OnRemovingRange()
		{
			base.OnRemovingRange();
			IsRemovingRange = true;
		}

		bool IsRemovingRange;

		protected override void OnRemovedRange(IEnumerable businessObjects)
		{
			RecalculateMasterValues();
			RefreshBindingsForMaster();
			RefreshTotalsValidationForMaster();
			base.OnRemovedRange(businessObjects);
			IsRemovingRange = false;
		}

		void RecalculateMasterValues()
		{
			try
			{
				RemoveMasterPackLinesFromMasterShipment();
				Master.UpdatingShipmentFromRelated = true;
				UpdateMasterValues();
			}
			finally
			{
				Master.UpdatingShipmentFromRelated = false;
			}
		}

		#endregion

		#region Master Shipment Update

		void RemoveMasterPackLinesFromMasterShipment()
		{
			if (!IsLoading && IsLoaded && Count == 1 && (Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster))
			{
				Master.InnerPackLines.RemoveAndDeleteAll();
				Master.OuterPackLines.RemoveAndDeleteAll();
			}
		}

		void UpdateMasterValues()
		{
			if (!IsLoading)
			{
				CalculateMasterVolume(null);
				CalculateMasterWeight(null);
				CalculateMasterJS_ActualChargeable(null, null);
				CalculateMasterOuterPackage();
				CalculateMasterInnerPackage();
				CalculateMasterInsuranceValue();
				CalculateMasterGoodsValue();
			}
		}

		void RefreshBindingsForMaster()
		{
			if (!IsLoading)
			{
				Master.JS_ActualWeightInfo.RefreshBinding();
				Master.JS_UnitOfWeightInfo.RefreshBinding();
				Master.JS_ActualVolumeInfo.RefreshBinding();
				Master.JS_UnitOfVolumeInfo.RefreshBinding();
				Master.JS_OuterPacksInfo.RefreshBinding();
				Master.JS_F3_NKPackTypeInfo.RefreshBinding();
				Master.JS_GoodsValueInfo.RefreshBinding();
				Master.JS_RX_NKGoodsValueCurrInfo.RefreshBinding();
				Master.JS_InsuranceValueInfo.RefreshBinding();
				Master.JS_RX_NKInsuranceCurrencyInfo.RefreshBinding();
				Master.JS_TotalPackageCountInfo.RefreshBinding();
				Master.JS_F3_NKTotalCountPackTypeInfo.RefreshBinding();
				Master.JS_ActualChargeableInfo.RefreshBinding();
			}
		}

		void RefreshTotalsValidationForMaster()
		{
			if (!IsLoading && !Master.HasCircularCoLoadMasterReference())
			{
				Master.ReloadInnerPackLines();
				Master.ReloadOuterPackLines();

				CommonShipment masterShipment = Master;
				List<CommonShipment> meAndAllMyParents = new List<CommonShipment>();
				while (masterShipment != null && !meAndAllMyParents.Contains(masterShipment))
				{
					if (!masterShipment.IsValidationSuspended)
					{
						masterShipment.Validation.ValidateJS_ActualWeight();
						masterShipment.Validation.ValidateJS_ActualVolume();
						masterShipment.Validation.ValidateJS_OuterPacks();
					}
					meAndAllMyParents.Add(masterShipment);
					masterShipment = masterShipment.CoLoadMasterShipment;
				}

				Master.ReloadOuterPackLines();
				Master.ReloadInnerPackLines();
			}
		}

		#endregion

		#region Events

		void HookSubShipment(CommonShipment newShipment)
		{
			if (newShipment != null)
			{
				newShipment.JS_ActualVolumeInfo.ValueChanged += new EventHandler(CalculateMasterJS_ActualVolume);
				newShipment.JS_ActualWeightInfo.ValueChanged += new EventHandler(CalculateMasterJS_ActualWeight);
				newShipment.JS_ActualChargeableInfo.ValueChanged += new EventHandler(CalculateMasterJS_ActualChargeable);
				newShipment.JS_OuterPacksInfo.ValueChanged += new EventHandler(CalculateMasterJS_OuterPacks);
				newShipment.JS_TotalPackageCountInfo.ValueChanged += new EventHandler(CalculateMasterJS_TotalPackageCount);
				newShipment.JS_GoodsValueInfo.ValueChanged += new EventHandler(CalculateMasterJS_GoodsValue);
				newShipment.JS_InsuranceValueInfo.ValueChanged += new EventHandler(CalculateMasterJS_InsuranceValue);
				newShipment.JS_UnitOfVolumeInfo.ValueChanged += new EventHandler(CalculateMasterJS_UnitOfVolume);
				newShipment.JS_UnitOfWeightInfo.ValueChanged += new EventHandler(CalculateMasterJS_UnitOfWeight);

				newShipment.JS_RX_NKGoodsValueCurrInfo.ValueChanged += new EventHandler(CalculateMasterJS_RX_NKGoodsValueCurr);
				newShipment.JS_RX_NKInsuranceCurrencyInfo.ValueChanged += new EventHandler(CalculateMasterJS_RX_NKInsuranceCurrency);
				newShipment.JS_F3_NKTotalCountPackTypeInfo.ValueChanged += new EventHandler(CalculateMasterJS_F3_NKTotalCountPackType);
				newShipment.JS_F3_NKPackTypeInfo.ValueChanged += new EventHandler(CalculateMasterJS_F3_NKPackType);
			}
		}

		void UnhookSubShipment(CommonShipment removedShipment)
		{
			if (removedShipment != null)
			{
				removedShipment.JS_ActualVolumeInfo.ValueChanged -= new EventHandler(CalculateMasterJS_ActualVolume);
				removedShipment.JS_ActualWeightInfo.ValueChanged -= new EventHandler(CalculateMasterJS_ActualWeight);
				removedShipment.JS_ActualChargeableInfo.ValueChanged -= new EventHandler(CalculateMasterJS_ActualChargeable);
				removedShipment.JS_OuterPacksInfo.ValueChanged -= new EventHandler(CalculateMasterJS_OuterPacks);
				removedShipment.JS_TotalPackageCountInfo.ValueChanged -= new EventHandler(CalculateMasterJS_TotalPackageCount);
				removedShipment.JS_GoodsValueInfo.ValueChanged -= new EventHandler(CalculateMasterJS_GoodsValue);
				removedShipment.JS_RX_NKGoodsValueCurrInfo.ValueChanged -= new EventHandler(CalculateMasterJS_RX_NKGoodsValueCurr);
				removedShipment.JS_InsuranceValueInfo.ValueChanged -= new EventHandler(CalculateMasterJS_InsuranceValue);
				removedShipment.JS_RX_NKInsuranceCurrencyInfo.ValueChanged -= new EventHandler(CalculateMasterJS_RX_NKInsuranceCurrency);
				removedShipment.JS_F3_NKPackTypeInfo.ValueChanged -= new EventHandler(CalculateMasterJS_F3_NKPackType);
				removedShipment.JS_F3_NKTotalCountPackTypeInfo.ValueChanged -= new EventHandler(CalculateMasterJS_F3_NKTotalCountPackType);
				removedShipment.JS_UnitOfVolumeInfo.ValueChanged -= new EventHandler(CalculateMasterJS_UnitOfVolume);
				removedShipment.JS_UnitOfWeightInfo.ValueChanged -= new EventHandler(CalculateMasterJS_UnitOfWeight);
			}
		}

		#endregion

		#region Sub changed events

		#region Weight

		void CalculateMasterJS_UnitOfWeight(object sender, EventArgs e)
		{
			CommonShipment changedShipment = sender as CommonShipment;
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterWeight(changedShipment);
			}
		}

		void CalculateMasterJS_ActualWeight(object sender, EventArgs e)
		{
			CommonShipment changedShipment = (CommonShipment)sender;
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterWeight(changedShipment);
			}
		}

		void CalculateMasterWeight(CommonShipment changedShipment)
		{
			if (Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster)
			{
				ZString newCommonUnit = GetCommonUnit(JobShipmentSchema.JS_UnitOfWeight.Name, Env.Registry.FreightWeightUnit);
				ZDecimal newTotalValue = CalculateSubsTotalWeight();

				ZString oldSubsCommonUnit = OldSubsCommonUnits.GetValueOrDefault(JobShipmentSchema.JS_UnitOfWeight.Name);
				ZDecimal oldSubsTotalValue = OldSubsTotalValuesDecimal.GetValueOrDefault(JobShipmentSchema.JS_ActualWeight.Name);

				bool unitNeedsChanging = Master.JS_ActualWeight == 0 || (Master.JS_UnitOfWeight == oldSubsCommonUnit && Master.JS_UnitOfWeight != newCommonUnit);
				if (unitNeedsChanging)
				{
					Master.JS_UnitOfWeight = newCommonUnit;
				}
				OldSubsCommonUnits[JobShipmentSchema.JS_UnitOfWeight.Name] = newCommonUnit;

				bool isMasterValueManullaySet = Master.JS_ActualWeight != oldSubsTotalValue && Master.JS_ActualWeight != 0;
				if (!isMasterValueManullaySet)
				{
					if (Master.IsValidationSuspended && !IsValueWithinSqlPrecisionAndScale(JobShipmentSchema.JS_ActualWeight, newTotalValue))
					{
						var newWeightUnit = Master.JS_UnitOfWeight;

						new WeightConversionStrategy().ReScale(ref newTotalValue, ref newWeightUnit, JobShipmentSchema.JS_ActualWeight.Precision, JobShipmentSchema.JS_ActualWeight.Scale);

						Master.JS_UnitOfWeight = newWeightUnit;
						Master.JS_ActualWeight = newTotalValue;
					}
					else
					{
						Master.JS_ActualWeight = newTotalValue;
					}
				}
				OldSubsTotalValuesDecimal[JobShipmentSchema.JS_ActualWeight.Name] = newTotalValue;

				if (changedShipment != null && !changedShipment.IsValidationSuspended && !Master.JS_ActualWeight.IsEmpty && Master.JS_ActualWeightInfo.HasChanges)
				{
					changedShipment.Validation.ValidateJS_JS_ColoadMasterShipment();
				}
			}
		}

		protected ZDecimal CalculateSubsTotalWeight()
		{
			ZString commonUnit = GetCommonUnit(JobShipmentSchema.JS_UnitOfWeight.Name, Env.Registry.FreightWeightUnit);
			ZDecimal totalWeight = 0;
			foreach (CommonShipment shipment in this)
			{
				if (shipment.PK != Master.PK)
				{
					if (Constants.Weight.ContainsCode(shipment.JS_UnitOfWeight))
					{
						var value = Constants.Weight.Convert(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, commonUnit, false);
						var roundedValue = shipment.GetRoundedValue(JobShipmentSchema.JS_ActualWeight, shipment.JS_ActualWeightInfo, value);

						totalWeight += roundedValue;
					}
				}
			}
			return totalWeight;
		}

		#endregion

		#region Volume

		void CalculateMasterJS_UnitOfVolume(object sender, EventArgs e)
		{
			CommonShipment changedShipment = sender as CommonShipment;
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterVolume(changedShipment);
			}
		}

		void CalculateMasterJS_ActualVolume(object sender, EventArgs e)
		{
			CommonShipment changedShipment = (CommonShipment)sender;
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterVolume(changedShipment);
			}
		}

		void CalculateMasterVolume(CommonShipment changedShipment)
		{
			if (Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster)
			{
				ZString newCommonUnit = GetCommonUnit(JobShipmentSchema.JS_UnitOfVolume.Name, Env.Registry.FreightVolumeUnit);
				ZDecimal newTotalValue = CalculateSubsTotalVolume();

				ZString oldSubsCommonUnit = OldSubsCommonUnits.GetValueOrDefault(JobShipmentSchema.JS_UnitOfVolume.Name);
				ZDecimal oldSubsTotalValue = OldSubsTotalValuesDecimal.GetValueOrDefault(JobShipmentSchema.JS_ActualVolume.Name);

				bool unitNeedsChanging = Master.JS_ActualVolume == 0 || (Master.JS_UnitOfVolume == oldSubsCommonUnit && Master.JS_UnitOfVolume != newCommonUnit);
				if (unitNeedsChanging)
				{
					Master.JS_UnitOfVolume = newCommonUnit;
				}
				OldSubsCommonUnits[JobShipmentSchema.JS_UnitOfVolume.Name] = newCommonUnit;

				bool isMasterValueManullaySet = Master.JS_ActualVolume != oldSubsTotalValue && Master.JS_ActualVolume != 0;
				if (!isMasterValueManullaySet)
				{
					if (Master.IsValidationSuspended && !IsValueWithinSqlPrecisionAndScale(JobShipmentSchema.JS_ActualVolume, newTotalValue))
					{
						var newVolumeUnit = Master.JS_UnitOfVolume;

						new VolumeConversionStrategy().ReScale(ref newTotalValue, ref newVolumeUnit, JobShipmentSchema.JS_ActualVolume.Precision, JobShipmentSchema.JS_ActualVolume.Scale);

						Master.JS_UnitOfVolume = newVolumeUnit;
						Master.JS_ActualVolume = newTotalValue;
					}
					else
					{
						Master.JS_ActualVolume = newTotalValue;
					}
				}
				OldSubsTotalValuesDecimal[JobShipmentSchema.JS_ActualVolume.Name] = newTotalValue;

				if (changedShipment != null && !changedShipment.IsValidationSuspended && !Master.JS_ActualVolume.IsEmpty && Master.JS_ActualVolumeInfo.HasChanges)
				{
					changedShipment.Validation.ValidateJS_JS_ColoadMasterShipment();
				}
			}
		}

		protected ZDecimal CalculateSubsTotalVolume()
		{
			ZString commonUnit = GetCommonUnit(JobShipmentSchema.JS_UnitOfVolume.Name, Env.Registry.FreightVolumeUnit);
			ZDecimal totalVolume = 0;
			foreach (CommonShipment shipment in this)
			{
				if (shipment.PK != Master.PK)
				{
					if (Constants.Volume.ContainsCode(shipment.JS_UnitOfVolume))
					{
						var value = Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume, commonUnit, false);
						var roundedValue = shipment.GetRoundedValue(JobShipmentSchema.JS_ActualVolume, shipment.JS_ActualVolumeInfo, value);

						totalVolume += roundedValue;
					}
				}
			}
			return totalVolume;
		}

		#endregion

		#region Inner Packs

		void CalculateMasterJS_F3_NKTotalCountPackType(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterInnerPackage();
			}
		}

		void CalculateMasterJS_TotalPackageCount(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterInnerPackage();
			}
		}

		void CalculateMasterInnerPackage()
		{
			if (Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster)
			{
				ZString newCommonUnit = GetCommonUnit(JobShipmentSchema.JS_F3_NKTotalCountPackType.Name, Constants.PkgUnit.Package);
				ZInt newTotalValue = CalculateSubsTotalInnerPacks();

				ZString oldSubsCommonUnit = OldSubsCommonUnits.GetValueOrDefault(JobShipmentSchema.JS_F3_NKTotalCountPackType.Name);
				ZInt oldSubsTotalValue = OldSubsTotalValuesInt.GetValueOrDefault(JobShipmentSchema.JS_TotalPackageCount.Name);

				bool unitNeedsChanging = (Master.JS_F3_NKTotalCountPackType != newCommonUnit && Master.JS_F3_NKTotalCountPackType == oldSubsCommonUnit) || Master.JS_TotalPackageCount == 0;
				if (unitNeedsChanging)
				{
					Master.JS_F3_NKTotalCountPackType = newCommonUnit;
				}
				OldSubsCommonUnits[JobShipmentSchema.JS_F3_NKTotalCountPackType.Name] = newCommonUnit;

				bool valueNeedsChanging = Master.JS_TotalPackageCount == oldSubsTotalValue || Master.JS_TotalPackageCount == 0;
				if (valueNeedsChanging)
				{
					Master.JS_TotalPackageCount = newTotalValue;
				}
				OldSubsTotalValuesInt[JobShipmentSchema.JS_TotalPackageCount.Name] = newTotalValue;

				Master.ReloadInnerPackLines();
			}
		}

		protected ZInt CalculateSubsTotalInnerPacks()
		{
			ZInt totalPacks = 0;
			foreach (CommonShipment shipment in this)
			{
				if (shipment.PK != Master.PK)
				{
					totalPacks += shipment.JS_TotalPackageCount;
				}
			}
			return totalPacks;
		}

		#endregion

		#region Outer Packs

		void CalculateMasterJS_F3_NKPackType(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterOuterPackage();
			}
		}

		void CalculateMasterJS_OuterPacks(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterOuterPackage();
			}
		}

		void CalculateMasterOuterPackage()
		{
			if (Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster)
			{
				ZString newCommonUnit = GetCommonUnit(JobShipmentSchema.JS_F3_NKPackType.Name, Constants.PkgUnit.Package);
				ZInt newTotalValue = CalculateSubsTotalOuterPacks();

				ZString oldSubsCommonUnit = OldSubsCommonUnits.GetValueOrDefault(JobShipmentSchema.JS_F3_NKPackType.Name);
				ZInt oldSubsTotalValue = OldSubsTotalValuesInt.GetValueOrDefault(JobShipmentSchema.JS_OuterPacks.Name);

				bool unitNeedsChanging = (Master.JS_F3_NKPackType != newCommonUnit && Master.JS_F3_NKPackType == oldSubsCommonUnit) || Master.JS_OuterPacks == 0;
				if (unitNeedsChanging)
				{
					Master.JS_F3_NKPackType = newCommonUnit;
				}
				OldSubsCommonUnits[JobShipmentSchema.JS_F3_NKPackType.Name] = newCommonUnit;

				bool valueNeedsChanging = Master.JS_OuterPacks == oldSubsTotalValue || Master.JS_OuterPacks == 0;
				if (valueNeedsChanging)
				{
					Master.JS_OuterPacks = newTotalValue;
				}
				OldSubsTotalValuesInt[JobShipmentSchema.JS_OuterPacks.Name] = newTotalValue;

				Master.ReloadOuterPackLines();
			}
		}

		protected ZInt CalculateSubsTotalOuterPacks()
		{
			ZInt totalPacks = 0;
			foreach (CommonShipment shipment in this)
			{
				if (shipment.PK != Master.PK)
				{
					totalPacks += shipment.JS_OuterPacks;
				}
			}
			return totalPacks;
		}

		#endregion

		#region Insurance

		void CalculateMasterJS_RX_NKInsuranceCurrency(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterInsuranceValue();
			}
		}

		void CalculateMasterJS_InsuranceValue(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterInsuranceValue();
			}
		}

		void CalculateMasterInsuranceValue()
		{
			CalculateMasterCurrencyAndValues(JobShipmentSchema.JS_RX_NKInsuranceCurrency.Name, JobShipmentSchema.JS_InsuranceValue.Name);
		}

		protected ZDecimal OldInsuranceValue { get; set; }

		#endregion

		#region Goods Value

		void CalculateMasterJS_RX_NKGoodsValueCurr(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterGoodsValue();
			}
		}

		void CalculateMasterJS_GoodsValue(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs)
			{
				CalculateMasterGoodsValue();
			}
		}

		void CalculateMasterGoodsValue()
		{
			CalculateMasterCurrencyAndValues(JobShipmentSchema.JS_RX_NKGoodsValueCurr.Name, JobShipmentSchema.JS_GoodsValue.Name);
		}

		#endregion

		#region Calculate Currency and Values

		protected void CalculateMasterCurrencyAndValues(ZString currencyPropertyName, ZString valuePropertyName)
		{
			if (Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster)
			{
				ZString newSubsCommonCurrencyUnit = GetCommonUnit(currencyPropertyName, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				RefCurrency newSubsCommonCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, newSubsCommonCurrencyUnit));
				ZDecimal newSubsTotalValue = CalculateSubsTotalGoodsAndInsuranceValue(currencyPropertyName, valuePropertyName);

				ZString masterCurrencyUnit = (ZString)Master[currencyPropertyName];
				RefCurrency masterCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, masterCurrencyUnit));
				ZDecimal masterValue = (ZDecimal)Master[valuePropertyName];
				ZDecimal masterValueInNewCommon = 0;
				if (masterCurrency != null)
				{
					masterValueInNewCommon = masterCurrency.ConvertUsingSellRate(ZDateTime.Now, masterValue, newSubsCommonCurrency);
				}

				ZString oldSubsCommonCurrencyUnit = OldSubsCommonUnits.GetValueOrDefault(currencyPropertyName);
				ZDecimal oldSubsTotalValue = OldSubsTotalValuesDecimal.GetValueOrDefault(valuePropertyName);
				ZDecimal oldGoodsValueInNewCommon = 0;
				if (oldSubsCommonCurrencyUnit != ZString.Empty)
				{
					RefCurrency oldCommonCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, oldSubsCommonCurrencyUnit));
					oldGoodsValueInNewCommon = oldCommonCurrency.ConvertUsingSellRate(ZDateTime.Now, oldSubsTotalValue, newSubsCommonCurrency);
				}

				bool isMasterValueManullaySet = masterValueInNewCommon != oldGoodsValueInNewCommon && masterValue != 0;

				bool currencyNeedsChanging = masterValue == 0 || (masterCurrencyUnit == oldSubsCommonCurrencyUnit && masterCurrencyUnit != newSubsCommonCurrencyUnit);
				if (currencyNeedsChanging && !isMasterValueManullaySet)
				{
					Master[currencyPropertyName] = newSubsCommonCurrencyUnit;
				}
				OldSubsCommonUnits[currencyPropertyName] = newSubsCommonCurrencyUnit;

				if (!isMasterValueManullaySet)
				{
					Master[valuePropertyName] = newSubsTotalValue;
				}
				OldSubsTotalValuesDecimal[valuePropertyName] = newSubsTotalValue;
			}
		}

		protected ZDecimal CalculateSubsTotalGoodsAndInsuranceValue(ZString currencyPropertyName, ZString valuePropertyName)
		{
			ZString commonCurrencyUnit = GetCommonUnit(currencyPropertyName, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			RefCurrency commonCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, commonCurrencyUnit));
			ZDecimal totalValue = 0;

			foreach (CommonShipment shipment in this)
			{
				if (shipment.PK != Master.PK)
				{
					RefCurrency shipmentCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, shipment[currencyPropertyName]));

					if (shipmentCurrency != null)
					{
						totalValue += shipmentCurrency.ConvertUsingSellRate(ZDateTime.Now, (ZDecimal)shipment[valuePropertyName], commonCurrency);
					}
				}
			}
			return totalValue;
		}

		#endregion

		#region Calculate Chargeable

		void CalculateMasterJS_ActualChargeable(object sender, EventArgs e)
		{
			if ((Master.IsCoLoadMaster || Master.IsBlindCoLoadMaster || Master.IsAssemblyMaster) && !Master.HasCircularCoLoadMasterReference())
			{
				Master.UpdateChargeableWeights();

				var changedSubShipment = (CommonShipment)sender;
				if (changedSubShipment != null && !changedSubShipment.IsValidationSuspended && !Master.JS_ActualChargeable.IsEmpty && Master.JS_ActualChargeableInfo.HasChanges)
				{
					changedSubShipment.Validation.ValidateJS_JS_ColoadMasterShipment();
				}
			}
		}

		#endregion

		#endregion

		#region Common Unit Calculation

		Dictionary<ZString, ZString> oldSubsCommonUnits;

		protected Dictionary<ZString, ZString> OldSubsCommonUnits
		{
			get
			{
				if (oldSubsCommonUnits == null)
				{
					oldSubsCommonUnits = new Dictionary<ZString, ZString>();
					oldSubsCommonUnits[JobShipmentSchema.JS_UnitOfWeight.Name] = GetCommonUnit(JobShipmentSchema.JS_UnitOfWeight.Name, Env.Registry.FreightWeightUnit);
					oldSubsCommonUnits[JobShipmentSchema.JS_UnitOfVolume.Name] = GetCommonUnit(JobShipmentSchema.JS_UnitOfVolume.Name, Env.Registry.FreightVolumeUnit);
					oldSubsCommonUnits[JobShipmentSchema.JS_F3_NKTotalCountPackType.Name] = GetCommonUnit(JobShipmentSchema.JS_F3_NKTotalCountPackType.Name, Constants.PkgUnit.Package);
					oldSubsCommonUnits[JobShipmentSchema.JS_F3_NKPackType.Name] = GetCommonUnit(JobShipmentSchema.JS_F3_NKPackType.Name, Constants.PkgUnit.Package);
					oldSubsCommonUnits[JobShipmentSchema.JS_RX_NKGoodsValueCurr.Name] = GetCommonUnit(JobShipmentSchema.JS_RX_NKGoodsValueCurr.Name, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					oldSubsCommonUnits[JobShipmentSchema.JS_RX_NKInsuranceCurrency.Name] = GetCommonUnit(JobShipmentSchema.JS_RX_NKInsuranceCurrency.Name, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				}
				return oldSubsCommonUnits;
			}
		}

		Dictionary<ZString, ZDecimal> oldSubsTotalValuesDecimal;

		protected Dictionary<ZString, ZDecimal> OldSubsTotalValuesDecimal
		{
			get
			{
				if (oldSubsTotalValuesDecimal == null)
				{
					oldSubsTotalValuesDecimal = new Dictionary<ZString, ZDecimal>();
					oldSubsTotalValuesDecimal[JobShipmentSchema.JS_ActualWeight.Name] = CalculateSubsTotalWeight();
					oldSubsTotalValuesDecimal[JobShipmentSchema.JS_ActualVolume.Name] = CalculateSubsTotalVolume();
					oldSubsTotalValuesDecimal[JobShipmentSchema.JS_GoodsValue.Name] = CalculateSubsTotalGoodsAndInsuranceValue(JobShipmentSchema.JS_RX_NKGoodsValueCurr.Name, JobShipmentSchema.JS_GoodsValue.Name);
					oldSubsTotalValuesDecimal[JobShipmentSchema.JS_InsuranceValue.Name] = CalculateSubsTotalGoodsAndInsuranceValue(JobShipmentSchema.JS_RX_NKInsuranceCurrency.Name, JobShipmentSchema.JS_InsuranceValue.Name);
				}
				return oldSubsTotalValuesDecimal;
			}
		}

		Dictionary<ZString, ZInt> oldSubsTotalValuesInt;

		protected Dictionary<ZString, ZInt> OldSubsTotalValuesInt
		{
			get
			{
				if (oldSubsTotalValuesInt == null)
				{
					oldSubsTotalValuesInt = new Dictionary<ZString, ZInt>();
					oldSubsTotalValuesInt[JobShipmentSchema.JS_TotalPackageCount.Name] = CalculateSubsTotalInnerPacks();
					oldSubsTotalValuesInt[JobShipmentSchema.JS_OuterPacks.Name] = CalculateSubsTotalOuterPacks();
				}
				return oldSubsTotalValuesInt;
			}
		}

		public ZString GetCommonUnit(string propertyName, string defaultValue)
		{
			var unitsOfChildren = Select(child => (ZString)child[propertyName].ToString()).Distinct();
			var commonUnit = unitsOfChildren.Count() == 1 ? unitsOfChildren.First() : (ZString)defaultValue;
			return commonUnit;
		}

		#endregion

		ZBool IsValueWithinSqlPrecisionAndScale(SchemaDecimalColumn column, ZDecimal value)
		{
			return value.IsWithinSqlPrecisionAndScale(column.Precision, column.Scale);
		}
	}
}
