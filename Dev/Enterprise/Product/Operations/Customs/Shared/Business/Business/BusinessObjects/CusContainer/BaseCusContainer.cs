using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using IContainer = Enterprise.Freight.Business.IContainer;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow()]
	[DependentBusinessObject(typeof(BaseJobDeclaration), "CusContainers"), CodeProperty(BaseCusContainer.Schema.CO_ContainerNumber), DescriptionProperty(BaseCusContainer.Schema.CO_ContainerNumber)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalCopyIgnoreElement("JobDeclaration")]
	public class BaseCusContainer : AutoCusContainer, Integration.Customs.Shared.IBaseCusContainer
		, IContainer
		, ISupportDataImporting
		, ILandedCostDistributeTo
		, IDocAddresses
		, IDocumentSupportable
		, IContainerForBinding
		, IDeclarationProvider
		, ICartageContainer
		, IDocManagerSupport
		, ISynchroniserReadOnlyMembersProvider
		, IClusterKeyWorker
		, ITypeDeciderContext
		, IAddInfoChildSupporter
		, IGlobalSearchBusinessObjectProvider
		, IDataModelSupporter
	{
		#region Schema

		public new class Schema : AutoCusContainer.Schema
		{
			// Calculated from dbo.RefContainer
			public const string CO_Ref_StorageClass = "CO_Ref_StorageClass";
			public const string CO_Ref_ContainerCapacity = "CO_Ref_ContainerCapacity";
			public const string CO_Ref_TareWeight = "CO_Ref_TareWeight";
			public const string CO_Ref_MaxGrossWeight = "CO_Ref_MaxGrossWeight";
			public const string CO_Ref_Length = "CO_Ref_Length";
			public const string CO_Ref_Width = "CO_Ref_Width";
			public const string CO_Ref_Height = "CO_Ref_Height";

			// Calculated from Actual Sizes entered against this container
			public const string CO_Calc_ActualCapacity = "CO_Calc_ActualCapacity";
			public const string CO_Calc_OverhangHeight = "CO_Calc_OverhangHeight";
			public const string CO_Calc_OverhangLength = "CO_Calc_OverhangLength";
			public const string CO_Calc_OverhangWidth = "CO_Calc_OverhangWidth";

			public const string RH_NKContainerCommodityCode = "RH_NKContainerCommodityCode";
			public const string OA_DepartureContainerYardAddress = "OA_DepartureContainerYardAddress";
			public const string OA_ArrivalContainerYardAddress = "OA_ArrivalContainerYardAddress";

			public const string CO_Calc_TotalPackages = "CO_Calc_TotalPackages";
			public const string CO_Calc_TotalPackagesUnit = "CO_Calc_TotalPackagesUnit";
		}

		#endregion

		#region Constants
		public static class ContainerModes
		{
			public const string FullContainerLoad = "FCL";
			public const string LessContainerLoad = "LCL";
			public const string FCX = "FCX";
			public const string BreakBulk = "BBK";
		}
		#endregion

		public static readonly BaseCusContainerTypeDecider TypeDecider = new BaseCusContainerTypeDecider();

		public BaseCusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			savedJobDeclarationPK = CO_JE;
		}

		public CommonContainer JobContainer
		{
			get
			{
				if (IsDeleted)
				{
					return null;
				}

				var declaration = Declaration;

				var result = Factory.Load<ForwardingContainer>(CO_JC);
				if (result == null && !isDeletingJobContainer)
				{
					var isUnCommittedRow = ((IBusinessObjectInternals)this).IsUnCommittedRow;
					using (isUnCommittedRow ? SuspendSettingHasChanges() : DisposableAction.NoAction)
					{
						result = FindContainerOnShipmentByContainerNumber(declaration, CO_ContainerNumber);
						if (result != null)
						{
							result.AddLoggingDetail((NoResString)$"Linked (CO_PK:'{PK}', CO_ContainerNumber:'{CO_ContainerNumber}') and previous CO_JC:'{CO_JC}'");
							CO_JC = result.PK;
						}
						else
						{
							result = Factory.New<ForwardingContainer>();
							using (result.GetValidationSuspender())
							using (isUnCommittedRow ? result.SuspendSettingHasChanges() : DisposableAction.NoAction)
							{
								result.CreatedFromCusContainer = true;
								result.JC_GrossWeightUQ = CO_WeightUQ;
								result.JC_RC = CO_RC;
								result.JC_ContainerMode = ModeConverter.ConvertCustomsToFreight(CO_FCL_LCL_AIR);
								result.JC_SealNum = CO_Seal;
								result.JC_AdditionalSealNum = CO_SecondSeal;
								result.JC_ContainerNum = CO_ContainerNumber;

								CO_JC = result.PK;
								declaration?.RelevantConsol?.Containers.Add(result);
							}
						}
					}
				}

				if (jobContainer != result)
				{
					if (jobContainer != null)
					{
						UnregisterPropertyChanged(jobContainer);
						UnRegisterJobContainer(jobContainer);
						UnRegisterListChangedCalledRefreshBinding(jobContainer);
					}
					jobContainer = result;
					if (jobContainer != null)
					{
						RegisterPropertyChanged(jobContainer);
						RegisterJobContainer(jobContainer);
						RegisterListChangedCalledRefreshBinding(jobContainer);
					}
				}

				if (jobContainer != null && declaration != null && declaration.Shipment == null)
				{
					jobContainer.StandAloneCustomsContainer = true;
				}

				return jobContainer;
			}
		}
		ForwardingContainer jobContainer;

		protected virtual void RegisterJobContainer(ForwardingContainer forwardingContainer)
		{
			RegisterEditableChildObject(forwardingContainer);
		}

		protected virtual void UnRegisterJobContainer(ForwardingContainer forwardingContainer)
		{
			UnRegisterEditableChildObject(forwardingContainer);
		}

		#region PropertyChanged

		void RegisterPropertyChanged(CommonContainer container)
		{
			foreach (var name in NeedValidationPropertyInfoNames)
			{
				var info = container.FindPropertyInfo(name);
				if (info != null)
				{
					info.ValueChanged += ForwardingContainerChanged;
				}
			}
		}

		void UnregisterPropertyChanged(CommonContainer container)
		{
			foreach (var name in NeedValidationPropertyInfoNames)
			{
				var info = container.FindPropertyInfo(name);
				if (info != null)
				{
					info.ValueChanged -= ForwardingContainerChanged;
				}
			}
		}

		//setting JC_IsValid triggers this
		void ForwardingContainerChanged(object sender, EventArgs e)
		{
			if (jobContainer.HasChanges && !IsMarkingAsNeedingValidationSuspended && !jobContainer.IsMarkingAsNeedingValidationSuspended)
			{
				MarkAsNeedingValidation();
			}
		}

		protected virtual IEnumerable<string> NeedValidationPropertyInfoNames
		{
			get
			{
				return new[]
				{
					AutoJobContainer.Schema.JC_GrossWeight,
					AutoJobContainer.Schema.JC_GrossWeightUQ,
					AutoJobContainer.Schema.JC_SealParty,
					AutoJobContainer.Schema.JC_AdditionalSealParty,
					AutoJobContainer.Schema.JC_Additional2SealParty,
					AutoJobContainer.Schema.JC_GrossWeightVerificationType,
					AutoJobContainer.Schema.JC_AdditionalSealNum
				};
			}
		}

		#endregion

		public ModeConverter ModeConverter
		{
			get
			{
				if (modeConverter == null)
				{
					modeConverter = CreatePopulatedModeConverter();
				}
				return modeConverter;
			}
		}
		ModeConverter modeConverter;

		protected virtual ModeConverter CreatePopulatedModeConverter()
		{
			ModeConverter result = new ModeConverter();
			result.AddConversion(Enterprise.Core.Constants.ContainerModes.BuyersConsol, Enterprise.Core.Constants.ContainerModes.FCLMixedShipper);
			result.AddConversion("", Enterprise.Core.Constants.ContainerModes.BreakBulk);
			return result;
		}

		#region CO_JC

		public override ZGuid CO_JC
		{
			get { return base.CO_JC; }
			set
			{
				//#warning needs a test
				if (CO_JC != value)
				{
					DeleteJobContainer();
				}
				base.CO_JC = value;
			}
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			// PlugIn clears HasChanges after sync on JobDeclaration and its children. So we cannot do this OnSaving
			var declaration = Declaration;
			if (declaration != null && declaration.ShouldDeleteContainers)
			{
				Delete();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var declaration = Declaration;
			if (!IsDeleted && declaration != null && !declaration.ShouldDeleteContainers && !CO_JC.IsValid)
			{
				var makeContainerIfNotExists = JobContainer;
			}
		}

		#region JobContainer proxies

		[List(nameof(JobContainer) + "." + nameof(CommonContainer.DeliveryMode_ListForBinding))]
		public ZString DeliveryModeForBinding
		{
			get { return JobContainer.JC_DeliveryMode; }
			set { JobContainer.JC_DeliveryMode = value; }
		}

		public ZPropertyInfo DeliveryModeForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DeliveryModeForBinding), x => JobContainer.JC_DeliveryModeInfo); }
		}

		public ZDecimal GrossWeight
		{
			get { return JobContainer.JC_GrossWeight; }
		}

		public ZPropertyInfo GrossWeightInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GrossWeight), x => JobContainer.JC_GrossWeightInfo); }
		}

		public ZString GrossWeightUQ
		{
			get { return JobContainer.JC_GrossWeightUQ; }
		}

		public ZPropertyInfo GrossWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GrossWeightUQ), x => JobContainer.JC_GrossWeightUQInfo); }
		}

		public ZDecimal TareWeight
		{
			get { return JobContainer.JC_TareWeight; }
			set { JobContainer.JC_TareWeight = value; }
		}

		public ZPropertyInfo TareWeightInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TareWeight), x => JobContainer.JC_TareWeightInfo); }
		}

		public ZDecimal DunnageWeight
		{
			get { return JobContainer.JC_DunnageWeight; }
			set { JobContainer.JC_DunnageWeight = value; }
		}

		public ZPropertyInfo DunnageWeightInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DunnageWeight), x => JobContainer.JC_DunnageWeightInfo); }
		}

		public ZDecimal NetWeight
		{
			get { return JobContainer.JC_Calc_NetWeight; }
		}

		public ZPropertyInfo NetWeightInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NetWeight), x => JobContainer.JC_Calc_NetWeightInfo); }
		}

		public ZDecimal TotalWidth
		{
			get { return JobContainer.JC_TotalWidth; }
			set { JobContainer.JC_TotalWidth = value; }
		}

		public ZPropertyInfo TotalWidthInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TotalWidth), x => JobContainer.JC_TotalWidthInfo); }
		}

		public ZDecimal TotalLength
		{
			get { return JobContainer.JC_TotalLength; }
			set { JobContainer.JC_TotalLength = value; }
		}

		public ZPropertyInfo TotalLengthInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TotalLength), x => JobContainer.JC_TotalLengthInfo); }
		}

		public ZDecimal TotalHeight
		{
			get { return JobContainer.JC_TotalHeight; }
			set { JobContainer.JC_TotalHeight = value; }
		}

		public ZPropertyInfo TotalHeightInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TotalHeight), x => JobContainer.JC_TotalHeightInfo); }
		}

		[ZDateTimeDurationValueExclude1900]
		public ZDateTime DepartureTruckWaitTime
		{
			get { return JobContainer.DepartureTruckWaitTime; }
			set { JobContainer.DepartureTruckWaitTime = value; }
		}

		public ZPropertyInfo DepartureTruckWaitTimeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureTruckWaitTime), x => JobContainer.DepartureTruckWaitTimeInfo); }
		}

		[ZDateTimeDurationValueExclude1900]
		public ZDateTime ArrivalTruckWaitTime
		{
			get { return JobContainer.ArrivalTruckWaitTime; }
			set { JobContainer.ArrivalTruckWaitTime = value; }
		}

		public ZPropertyInfo ArrivalTruckWaitTimeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalTruckWaitTime), x => JobContainer.ArrivalTruckWaitTimeInfo); }
		}

		public ZDateTime ContainerYardEmptyReturnGateIn
		{
			get { return JobContainer.JC_ContainerYardEmptyReturnGateIn; }
			set { JobContainer.JC_ContainerYardEmptyReturnGateIn = value; }
		}

		public ZPropertyInfo ContainerYardEmptyReturnGateInInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ContainerYardEmptyReturnGateIn), x => JobContainer.JC_ContainerYardEmptyReturnGateInInfo); }
		}

		public ZDateTime ArrivalCartageComplete
		{
			get { return JobContainer.JC_ArrivalCartageComplete; }
			set { JobContainer.JC_ArrivalCartageComplete = value; }
		}

		public ZPropertyInfo ArrivalCartageCompleteInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalCartageComplete), x => JobContainer.JC_ArrivalCartageCompleteInfo); }
		}

		public ZDateTime DepartureCartageComplete
		{
			get { return JobContainer.JC_DepartureCartageComplete; }
			set { JobContainer.JC_DepartureCartageComplete = value; }
		}

		public ZPropertyInfo DepartureCartageCompleteInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureCartageComplete), x => JobContainer.JC_DepartureCartageCompleteInfo); }
		}

		public ZBool IsControlledAtmosphere
		{
			get { return JobContainer.JC_IsControlledAtmosphere; }
			set { JobContainer.JC_IsControlledAtmosphere = value; }
		}

		public ZPropertyInfo IsControlledAtmosphereInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsControlledAtmosphere), x => JobContainer.JC_IsControlledAtmosphereInfo); }
		}

		public ZString RH_NKContainerCommodityCode
		{
			get { return JobContainer.JC_RH_NKContainerCommodityCode; }
			set { JobContainer.JC_RH_NKContainerCommodityCode = value; }
		}

		public ZPropertyInfo RH_NKContainerCommodityCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.RH_NKContainerCommodityCode, x => JobContainer.JC_RH_NKContainerCommodityCodeInfo); }
		}

		public ZDateTime EmptyRequired
		{
			get { return JobContainer.JC_EmptyRequired; }
			set { JobContainer.JC_EmptyRequired = value; }
		}

		public ZPropertyInfo EmptyRequiredInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EmptyRequired), x => JobContainer.JC_EmptyRequiredInfo); }
		}

		public ZAddress OA_DepartureContainerYardAddress_ZAddress
		{
			get { return JobContainer.JC_OA_DepartureContainerYardAddress_ZAddress; }
		}

		public ZGuid OA_DepartureContainerYardAddress
		{
			get { return JobContainer.JC_OA_DepartureContainerYardAddress; }
		}

		public ZPropertyInfo OA_DepartureContainerYardAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OA_DepartureContainerYardAddress, x => JobContainer.JC_OA_DepartureContainerYardAddressInfo); }
		}

		public ZBool DepartureDeliveryByRail
		{
			get { return JobContainer.JC_DepartureDeliveryByRail; }
			set { JobContainer.JC_DepartureDeliveryByRail = value; }
		}

		public ZPropertyInfo DepartureDeliveryByRailInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureDeliveryByRail), x => JobContainer.JC_DepartureDeliveryByRailInfo); }
		}

		public ZString DepartureSlotReference
		{
			get { return JobContainer.JC_DepartureSlotReference; }
			set { JobContainer.JC_DepartureSlotReference = value; }
		}

		public ZPropertyInfo DepartureSlotReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureSlotReference), x => JobContainer.JC_DepartureSlotReferenceInfo); }
		}

		public ZDateTime DepartureSlotDateTime
		{
			get { return JobContainer.JC_DepartureSlotDateTime; }
			set { JobContainer.JC_DepartureSlotDateTime = value; }
		}

		public ZPropertyInfo DepartureSlotDateTimeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureSlotDateTime), x => JobContainer.JC_DepartureSlotDateTimeInfo); }
		}

		public ZDateTime DepartureEstimatedPickup
		{
			get { return JobContainer.JC_DepartureEstimatedPickup; }
			set { JobContainer.JC_DepartureEstimatedPickup = value; }
		}

		public ZPropertyInfo DepartureEstimatedPickupInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureEstimatedPickup), x => JobContainer.JC_DepartureEstimatedPickupInfo); }
		}

		public ZDateTime DepartureCartageAdvised
		{
			get { return JobContainer.JC_DepartureCartageAdvised; }
			set { JobContainer.JC_DepartureCartageAdvised = value; }
		}

		public ZPropertyInfo DepartureCartageAdvisedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureCartageAdvised), x => JobContainer.JC_DepartureCartageAdvisedInfo); }
		}

		public ZDecimal SetPointTemp
		{
			get { return JobContainer.JC_SetPointTemp; }
			set { JobContainer.JC_SetPointTemp = value; }
		}

		public ZPropertyInfo SetPointTempInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SetPointTemp), x => JobContainer.JC_SetPointTempInfo); }
		}

		public ZString SetPointTempUnit
		{
			get { return JobContainer.JC_SetPointTempUnit; }
			set { JobContainer.JC_SetPointTempUnit = value; }
		}

		public ZPropertyInfo SetPointTempUnitInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SetPointTempUnit), x => JobContainer.JC_SetPointTempUnitInfo); }
		}

		public ZString RefrigGeneratorID
		{
			get { return JobContainer.JC_RefrigGeneratorID; }
			set { JobContainer.JC_RefrigGeneratorID = value; }
		}

		public ZPropertyInfo RefrigGeneratorIDInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RefrigGeneratorID), x => JobContainer.JC_RefrigGeneratorIDInfo); }
		}

		public ZByte HumidityPercent
		{
			get { return JobContainer.JC_HumidityPercent; }
			set { JobContainer.JC_HumidityPercent = value; }
		}

		public ZPropertyInfo HumidityPercentInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(HumidityPercent), x => JobContainer.JC_HumidityPercentInfo); }
		}

		public ZDecimal AirVentFlow
		{
			get { return JobContainer.JC_AirVentFlow; }
			set { JobContainer.JC_AirVentFlow = value; }
		}

		public ZPropertyInfo AirVentFlowInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AirVentFlow), x => JobContainer.JC_AirVentFlowInfo); }
		}

		public ZString AirVentFlowRateUnit
		{
			get { return JobContainer.JC_AirVentFlowRateUnit; }
			set { JobContainer.JC_AirVentFlowRateUnit = value; }
		}

		public ZPropertyInfo AirVentFlowRateUnitInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AirVentFlowRateUnit), x => JobContainer.JC_AirVentFlowRateUnitInfo); }
		}

		public ZDateTime ArrivalCartageAdvised
		{
			get { return JobContainer.JC_ArrivalCartageAdvised; }
			set { JobContainer.JC_ArrivalCartageAdvised = value; }
		}

		public ZPropertyInfo ArrivalCartageAdvisedInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalCartageAdvised), x => JobContainer.JC_ArrivalCartageAdvisedInfo); }
		}

		public ZDecimal ArrivalTruckWaitCost
		{
			get { return JobContainer.ArrivalTruckWaitCost; }
			set { JobContainer.ArrivalTruckWaitCost = value; }
		}

		public ZPropertyInfo ArrivalTruckWaitCostInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalTruckWaitCost), x => JobContainer.ArrivalTruckWaitCostInfo); }
		}

		public ZDateTime ArrivalEstimatedDelivery
		{
			get { return JobContainer.JC_ArrivalEstimatedDelivery; }
			set { JobContainer.JC_ArrivalEstimatedDelivery = value; }
		}

		public ZPropertyInfo ArrivalEstimatedDeliveryInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalEstimatedDelivery), x => JobContainer.JC_ArrivalEstimatedDeliveryInfo); }
		}

		public ZBool ArrivalPickupByRail
		{
			get { return JobContainer.JC_ArrivalPickupByRail; }
			set { JobContainer.JC_ArrivalPickupByRail = value; }
		}

		public ZPropertyInfo ArrivalPickupByRailInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalPickupByRail), x => JobContainer.JC_ArrivalPickupByRailInfo); }
		}

		public ZDateTime ArrivalSlotDateTime
		{
			get { return JobContainer.JC_ArrivalSlotDateTime; }
			set { JobContainer.JC_ArrivalSlotDateTime = value; }
		}

		public ZPropertyInfo ArrivalSlotDateTimeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalSlotDateTime), x => JobContainer.JC_ArrivalSlotDateTimeInfo); }
		}

		public ZString ArrivalSlotReference
		{
			get { return JobContainer.JC_ArrivalSlotReference; }
			set { JobContainer.JC_ArrivalSlotReference = value; }
		}

		public ZPropertyInfo ArrivalSlotReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalSlotReference), x => JobContainer.JC_ArrivalSlotReferenceInfo); }
		}

		public ZBool IsChiller
		{
			get { return JobContainer.IsChiller; }
		}

		public ZPropertyInfo IsChillerInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsChiller), x => JobContainer.IsChillerInfo); }
		}

		public ZBool IsFreezer
		{
			get { return JobContainer.IsFreezer; }
			set { JobContainer.IsFreezer = value; }
		}

		public ZPropertyInfo IsFreezerInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsFreezer), x => JobContainer.IsFreezerInfo); }
		}

		public ZDecimal DepartureTruckWaitCost
		{
			get { return JobContainer.DepartureTruckWaitCost; }
			set { JobContainer.DepartureTruckWaitCost = value; }
		}

		public ZPropertyInfo DepartureTruckWaitCostInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DepartureTruckWaitCost), x => JobContainer.DepartureTruckWaitCostInfo); }
		}

		public ZDateTime EmptyReturnedBy
		{
			get { return JobContainer.JC_EmptyReturnedBy; }
			set { JobContainer.JC_EmptyReturnedBy = value; }
		}

		public ZPropertyInfo EmptyReturnedByInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(EmptyReturnedBy), x => JobContainer.JC_EmptyReturnedByInfo); }
		}

		public ZDecimal ArrivalCTOStorageCost
		{
			get { return JobContainer.ArrivalCTOStorageCost; }
			set { JobContainer.ArrivalCTOStorageCost = value; }
		}

		public ZPropertyInfo ArrivalCTOStorageCostInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalCTOStorageCost), x => JobContainer.ArrivalCTOStorageCostInfo); }
		}

		public ZByte ArrivalCTOStorageDays
		{
			get { return JobContainer.ArrivalCTOStorageDays; }
			set { JobContainer.ArrivalCTOStorageDays = value; }
		}

		public ZPropertyInfo ArrivalCTOStorageDaysInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalCTOStorageDays), x => JobContainer.ArrivalCTOStorageDaysInfo); }
		}

		public ZDecimal GrossWeight_ReadOnly
		{
			get { return JobContainer.JC_GrossWeight; }
		}

		public ZPropertyInfo GrossWeight_ReadOnlyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GrossWeight_ReadOnly), x => JobContainer.JC_GrossWeightInfo); }
		}

		public ZAddress OA_ArrivalContainerYardAddress_ZAddress
		{
			get { return JobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress; }
		}

		public ZGuid OA_ArrivalContainerYardAddress
		{
			get { return JobContainer.JC_OA_ArrivalContainerYardAddress; }
		}

		public ZPropertyInfo OA_ArrivalContainerYardAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OA_ArrivalContainerYardAddress, x => JobContainer.JC_OA_ArrivalContainerYardAddressInfo); }
		}

		public ZDateTime LCLAvailable
		{
			get { return JobContainer.JC_LCLAvailable; }
			set { JobContainer.JC_LCLAvailable = value; }
		}

		public ZPropertyInfo LCLAvailableInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LCLAvailable), x => JobContainer.JC_LCLAvailableInfo); }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return JobContainer.JC_LCLStorageCommences; }
			set { JobContainer.JC_LCLStorageCommences = value; }
		}

		public ZPropertyInfo LCLStorageCommencesInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LCLStorageCommences), x => JobContainer.JC_LCLStorageCommencesInfo); }
		}

		public ZDateTime FCLAvailable
		{
			get { return JobContainer.JC_FCLAvailable; }
			set { JobContainer.JC_FCLAvailable = value; }
		}

		public ZPropertyInfo FCLAvailableInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(FCLAvailable), x => JobContainer.JC_FCLAvailableInfo); }
		}

		public ZDateTime ArrivalCTOStorageStartDate
		{
			get { return JobContainer.JC_ArrivalCTOStorageStartDate; }
			set { JobContainer.JC_ArrivalCTOStorageStartDate = value; }
		}

		public ZPropertyInfo ArrivalCTOStorageStartDateInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ArrivalCTOStorageStartDate), x => JobContainer.JC_ArrivalCTOStorageStartDateInfo); }
		}

		public ZString VolumeCapacityUQ
		{
			get { return JobContainer.JC_VolumeCapacityUQ; }
			set { JobContainer.JC_VolumeCapacityUQ = value; }
		}

		public ZPropertyInfo VolumeCapacityUQInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(VolumeCapacityUQ), x => JobContainer.JC_VolumeCapacityUQInfo); }
		}

		public ZDecimal VolumeCapacity
		{
			get { return JobContainer.JC_VolumeCapacity; }
			set { JobContainer.JC_VolumeCapacity = value; }
		}

		public ZPropertyInfo VolumeCapacityInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(VolumeCapacity), x => JobContainer.JC_VolumeCapacityInfo); }
		}

		public ZString WeightCapacityUQ
		{
			get { return JobContainer.JC_WeightCapacityUQ; }
			set { JobContainer.JC_WeightCapacityUQ = value; }
		}

		public ZPropertyInfo WeightCapacityUQInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(WeightCapacityUQ), x => JobContainer.JC_WeightCapacityUQInfo); }
		}

		public ZDecimal WeightCapacity
		{
			get { return JobContainer.JC_WeightCapacity; }
			set { JobContainer.JC_WeightCapacity = value; }
		}

		public ZPropertyInfo WeightCapacityInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(WeightCapacity), x => JobContainer.JC_WeightCapacityInfo); }
		}

		#endregion

		#region Container Messaging Code

		protected EDIMessage LastMessage
		{
			get
			{
				EDIMessage result = null;
				if (Messages.Count > 0)
				{
					result = Messages[Messages.Count - 1];
				}
				return result;
			}
		}

		public virtual EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = GetNewMessagesCollection();
					fMessages.Load();
					fMessages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Ascending);
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		protected virtual EDIMessageCollection GetNewMessagesCollection()
		{
			return new EDIMessageCollection(this, Factory);
		}

		protected EDIMessageCollection fMessages;

		#endregion

		#region Business Object Overrides

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (BaseCusContainer)base.CloneInternal(args);
			if (result != null)
			{
				using (result.GetValidationSuspender())
				using (result.SuspendSettingHasChanges())
				{
					result.CO_JC = !CO_JC.IsEmpty ? JobContainer.Clone().PK : ZGuid.Empty;
				}

				result.CO_AddInfoInfo.RefreshBinding();
			}

			return result;
		}

		protected override bool SupportsCloneCore() => true;

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateDataModelIfNeeded();

			var declaration = this.Declaration;
			if (declaration != null)
			{
				declaration.UpdateJE_ContainerCount(this, IsDeleting);
			}
		}

		public override void Delete()
		{
			var declaration = this.Declaration;
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				this.DeleteChildren<CusContainerInvoiceLinePivot>(CusContainerInvoiceLinePivotSchema.C2_CO);
				this.DeleteChildren<CusContainerEntryInstructionPivot>(CusContainerEntryInstructionPivotSchema.CEP_CO_Container);
				DeleteJobContainer();
				PivotsToEntries.DeleteAll();

				if (declaration != null)
				{
					PackingGroups.DeleteOrRemoveReferenceToContainer();

					if (declaration.CusContainers.Contains(this))
					{
						declaration.UpdateJE_ContainerCount(this, true);
					}
				}
			}

			base.Delete();

			if (declaration != null)
			{
				declaration.UpdateETADeliveryIfPortDeliveryTimeFound();
			}
		}

		void DeleteJobContainer()
		{
			if (!isDeletingJobContainer && !IsDeleted && !CO_JC.IsEmpty)
			{
				isDeletingJobContainer = true;
				try
				{
					var container = Factory.Load<ForwardingContainer>(CO_JC);
					if (container != null && !container.IsDeleted && !container.IsDeleting && (container.JC_JK.IsEmpty || !container.IsInDatabase))
					{
						UnregisterPropertyChanged(container);
						UnRegisterJobContainer(container);

						using (((ISingleElementListInternal)this).SuspendListChanged())
						{
							container.AddLoggingDetail((NoResString)$"IsInDatabase:{IsInDatabase}, CO_PK:'{PK}', CO_JE:'{CO_JE}', CO_ContainerNumber:'{CO_ContainerNumber}', JC_JK:'{container.JC_JK}'");
							container.Delete();
						}
					}
				}
				finally
				{
					isDeletingJobContainer = false;
				}
			}
		}
		bool isDeletingJobContainer;

		bool Integration.Customs.Shared.IBaseCusContainer.IsDeletingJobContainer => isDeletingJobContainer;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.CO_WeightUQ = Core.Constants.Weight.Kilograms;
		}

		protected override ZString HumanReadableNameCore => CO_ContainerNumber.IsEmpty ? Res.GetString("1c42633b-75f2-42fc-8b4a-719dd949149c", "Container") : (Res.GetString("5db66386-b417-45db-9c20-6a9157e08f81", "Container '{0}'", CO_ContainerNumber));

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			base.CO_ContainerNumber = Guid.NewGuid().ToString().Substring(0, CO_ContainerNumberInfo.MaxLength);
		}
#endif

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region Property Overrides

		[List(nameof(Lookups) + "." + nameof(CusContainerLookups.MessageStatusList))]
		public override ZString CO_MessageStatus
		{
			get { return base.CO_MessageStatus; }
			set { base.CO_MessageStatus = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusContainerLookups.ContainerSizeList))]
		public override ZString CO_ContainerSize
		{
			get { return base.CO_ContainerSize; }
			set { base.CO_ContainerSize = value; }
		}

		public virtual ZBool UseContainerSize => false;

		[ResourceStringData("7874E1B8-F57D-4A5F-96DA-298AA4EEFA78", Caption = "Container Number")]
		public override ZString CO_ContainerNumber
		{
			get { return base.CO_ContainerNumber; }
			set
			{
				var originalContainerNumber = IsContainerNumberToUpper ? CO_ContainerNumber.ToUpper() : CO_ContainerNumber;
				base.CO_ContainerNumber = IsContainerNumberToUpper ? value.ToUpper() : value;
				if (!IsCopying && originalContainerNumber != CO_ContainerNumber)
				{
					var declaration = Declaration;
					if (declaration != null)
					{
						if (originalContainerNumber == "" && declaration.CusContainers.Contains(this) && ShouldDefaultPackingInformation)
						{
							DefaultPackingInformationIfNeeded();
						}

						var container = FindContainerOnShipmentByContainerNumber(declaration, CO_ContainerNumber);
						if (container != null)
						{
							var oldContainer = Factory.Load<ForwardingContainer>(CO_JC);
							if (oldContainer != null && container != oldContainer)
							{
								oldContainer.AddLoggingDetail((NoResString)$"Unlinked (CO_PK:'{PK}', CO_ContainerNumber:'{CO_ContainerNumber}') and linked to CO_PK:'{container.PK}'");
								container.AddLoggingDetail((NoResString)$"Linked (CO_PK:'{PK}', CO_ContainerNumber:'{CO_ContainerNumber}') and unlinked from CO_PK:'{oldContainer.PK}'");
							}
							CO_JC = container.PK;
						}

						if (declaration.ContainersRequired)
						{
							declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(this);
							declaration.AddToContainersAndEquipmentsOnDeclaration_ListIfNeeded(CO_ContainerNumberInfo, true);
						}
					}

					var jobContainer = JobContainer;
					if (jobContainer != null && jobContainer.JC_ContainerNum != CO_ContainerNumber)
					{
						jobContainer.JC_ContainerNum = CO_ContainerNumber;
					}

					SetGoodsWeightFromShipment();
					Validation.ValidateCO_ContainerNumber();
				}
			}
		}

		protected virtual ZBool IsContainerNumberToUpper => ZBool.True;

		void SetGoodsWeightFromShipment()
		{
			var jobContainer = JobContainer;
			if (jobContainer != null)
			{
				var declaration = Declaration;
				if (!jobContainer.CreatedFromCusContainer && declaration != null && declaration.Shipment != null)
				{
					ZDecimal goodsWeight = 0;
					foreach (PackLine packLine in jobContainer.PackLines)
					{
						if (packLine.Shipment != null && packLine.Shipment.PK == declaration.Shipment.PK && Core.Constants.Weight.ContainsCode(packLine.JL_ActualWeightUQ) && Core.Constants.Weight.ContainsCode(CO_WeightUQ))
						{
							goodsWeight += Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, CO_WeightUQ, false);
						}
					}
					using (jobContainer.SuspendSettingGrossWeightFromCombinedWeights())
					{
						CO_Weight = goodsWeight;
					}
				}
			}
		}

		ForwardingContainer FindContainerOnShipmentByContainerNumber(BaseJobDeclaration declaration, ZString containerNumber)
		{
			ForwardingContainer result = null;

			if (!containerNumber.IsEmpty)
			{
				var containers = declaration?.RelevantConsol?.Containers;
				if (containers != null)
				{
					containers.ReloadIfTimeout(true, 30);
					result = (ForwardingContainer)containers.FindAnyByContainerNumber(containerNumber);
				}
			}

			return result;
		}

		public override ZString CO_FCL_LCL_AIR
		{
			get { return base.CO_FCL_LCL_AIR; }
			set
			{
				var declaration = Declaration;
				var jobContainer = JobContainer;
				if (jobContainer != null && !IsCopying && (declaration == null || declaration.Shipment == null || !jobContainer.IsInDatabase))
				{
					jobContainer.JC_ContainerMode = ModeConverter.ConvertCustomsToFreight(value);
				}
				base.CO_FCL_LCL_AIR = value;

				if (declaration != null)
				{
					declaration.UpdateETADeliveryIfPortDeliveryTimeFound();
					declaration.DefaultCartageEquipment();
				}
			}
		}

		[ResourceStringData("44DDC0E5-24E6-4761-A485-1C23BCF0DFCF", Caption = "Seal Number")]
		public override ZString CO_Seal
		{
			get { return base.CO_Seal; }
			set
			{
				if (!IsCopying)
				{
					var jobContainer = JobContainer;
					if (jobContainer != null)
					{
						jobContainer.JC_SealNum = value.ToUpper();
					}
				}
				base.CO_Seal = IsSealToUpper ? value.ToUpper() : value;
			}
		}

		public override ZString CO_SecondSeal
		{
			get { return base.CO_SecondSeal; }
			set
			{
				if (!IsCopying)
				{
					var jobContainer = JobContainer;
					if (jobContainer != null)
					{
						jobContainer.JC_AdditionalSealNum = value.ToUpper();
					}
				}
				base.CO_SecondSeal = IsSealToUpper ? value.ToUpper() : value;
			}
		}

		protected virtual ZBool IsSealToUpper => ZBool.True;

		[RelatedBusinessObject("Declaration")]
		public override ZGuid CO_JE
		{
			get { return base.CO_JE; }
			set
			{
				ZGuid oldValue = CO_JE;
				base.CO_JE = value;
				if (value != ZGuid.Empty)
				{
					savedJobDeclarationPK = value;
					CO_ClusterKey = Declaration?.JE_ClusterKey ?? ZInt.Zero;
				}
			}
		}
		ZGuid savedJobDeclarationPK;

		public override ZGuid CO_RC
		{
			get { return base.CO_RC; }
			set
			{
				if (!IsCopying)
				{
					var jobContainer = JobContainer;
					if (jobContainer != null)
					{
						jobContainer.JC_RC = value;
					}
				}
				base.CO_RC = value;

				this.RefreshBinding();
			}
		}

		[MeasureUnit(Schema.CO_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CO_Weight
		{
			get { return base.CO_Weight; }
			set
			{
				base.CO_Weight = value;

				var declaration = this.Declaration;
				if (!IsCopying && (declaration == null || !declaration.ShouldSynchroniseWithShipment()))
				{
					var jobContainer = JobContainer;
					if (jobContainer != null)
					{
						jobContainer.SetGrossWeightFromCombinedWeights(CO_Weight, CO_WeightUQ);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusContainerLookups.WeightUnits))]
		public override ZString CO_WeightUQ
		{
			get { return base.CO_WeightUQ; }
			set
			{
				base.CO_WeightUQ = value;
				var declaration = this.Declaration;

				//synch JobContainer Weight Unit
				if (declaration != null && !declaration.ShouldSynchroniseWithShipment())
				{
					var jobContainer = JobContainer;
					if (jobContainer != null)
					{
						jobContainer.JC_GrossWeightUQ = value;

						if (!IsCopying && !CO_JC.IsEmpty)
						{
							jobContainer.SetGrossWeightFromCombinedWeights(CO_Weight, CO_WeightUQ);
						}
					}
				}
			}
		}

		public override ZString CO_DataModel
		{
			get { return base.CO_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(CO_DataModelInfo, value);
				base.CO_DataModel = value;
			}
		}

		#endregion

		#region DefaultPackingInformationIfNeeded
		void DefaultPackingInformationIfNeeded()
		{
			var declaration = Declaration;
			if (declaration != null && PackingGroups.Count == 0 &&
				declaration.ShouldDefaultPackingInfoFromDeclarationToBills &&
				declaration.LowestBills.Count == 1)
			{
				var packGroup = declaration.PackingGroups.GetElementWithNoContainer();
				if (packGroup != null)
				{
					packGroup.CR_CO_Container = PK;
					if (packGroup.Packages.Count == 0)
					{
						packGroup.Packages.AddNew();
					}
				}
				else if (declaration.PackingInformationCollection != null)
				{
					var houseBill = declaration.LowestBills[0];
					var packingInformation = declaration.PackingInformationCollection.AddNew();
					packingInformation.HouseBillContainer = new HouseBillContainer(houseBill, this);
				}
			}
		}

		protected virtual bool ShouldDefaultPackingInformation => true;

		#endregion

		#region New Properties

		#region Declaration

		public BaseJobDeclaration Declaration
		{
			get
			{
				if (!IsDeleted && (fDeclaration == null || fDeclaration.PK != CO_JE))
				{
					fDeclaration = Factory.Load<BaseJobDeclaration>(CO_JE);
					if (fDeclaration == null)
					{
						fDeclaration = Factory.Load<BaseJobDeclaration>(savedJobDeclarationPK);
					}
				}
				return fDeclaration == null || fDeclaration.IsDeleted ? null : fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected virtual Type GetJobDeclarationType()
		{
			return typeof(BaseJobDeclaration);
		}

		#endregion

		#region CusContainerInvoiceLinePivotCollection

		public CusContainerInvoiceLinePivotCollection InvoiceLinePivotCollection
		{
			get
			{
				if (fInvoiceLinePivotCollection == null)
				{
					fInvoiceLinePivotCollection = new CusContainerInvoiceLinePivotCollection(this, Factory);
					fInvoiceLinePivotCollection.Load();
					fInvoiceLinePivotCollection.IsManagedForDataRefresh = true;
				}
				return fInvoiceLinePivotCollection;
			}
		}
		CusContainerInvoiceLinePivotCollection fInvoiceLinePivotCollection;

		internal bool IsInvoiceLinePivotCollectionLoaded
		{
			get { return fInvoiceLinePivotCollection != null; }
		}

		#endregion

		#region IsFullContainer
		public virtual bool IsFullContainer
		{
			get { return CO_FCL_LCL_AIR == Core.Constants.ContainerModes.FCL; }
		}
		#endregion

		#region CO_Calc_TotalPackages

		public ZInt CO_Calc_TotalPackages
		{
			get { return new ZDecimal(TotalCalculation.GetTotal(Packages.ToArray(), BasePackage.Schema.CW_PackQty)).ToZInt(); }
		}

		public ZPropertyInfo CO_Calc_TotalPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Calc_TotalPackages); }
		}

		#endregion

		#region CO_Calc_TotalPackagesUnit

		[List(nameof(Lookups) + "." + nameof(CusContainerLookups.TotalPackagesUnit_List))]
		public ZString CO_Calc_TotalPackagesUnit
		{
			get
			{
				var result = ZString.Empty;

				foreach (var package in Packages)
				{
					if (result.IsEmpty)
					{
						result = package.CW_PackType;
					}
					else if (result != package.CW_PackType)
					{
						result = Constants.PkgUnit.Piece; //Combination
						break;
					}
				}

				if (result.IsEmpty)
				{
					if (CO_Calc_TotalPackages.IsEmpty)
					{
						var dec = Declaration;
						if (dec != null)
						{
							result = dec.JE_TotalNoOfPacksPackType;
						}
					}
					else
					{
						result = Constants.PkgUnit.Piece; //Default
					}
				}
				return result;
			}
		}

		public ZPropertyInfo CO_Calc_TotalPackagesUnitInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Calc_TotalPackagesUnit); }
		}

		#endregion

		public BasePackingGroupContainerCollection PackingGroups
		{
			get
			{
				if (fPackingGroups == null)
				{
					fPackingGroups = GetPackingGroupCollection();
					fPackingGroups.Rebuild();
				}
				return fPackingGroups;
			}
		}
		BasePackingGroupContainerCollection fPackingGroups;

		protected virtual BasePackingGroupContainerCollection GetPackingGroupCollection()
		{
			return new BasePackingGroupContainerCollection(this);
		}

		public List<BasePackage> Packages
		{
			get
			{
				if (packagesCached == null)
				{
					packagesCached = new CachedProperty<List<BasePackage>>(Factory, delegate
					{
						List<BasePackage> result = new List<BasePackage>();
						foreach (BasePackingGroup packingGroup in PackingGroups)
						{
							result.AddRange((BasePackage[])packingGroup.Packages.ToArray(typeof(BasePackage)));
						}
						return result;
					}
					);
				}
				return packagesCached.Value;
			}
		}
		CachedProperty<List<BasePackage>> packagesCached;

		public bool IsWaitingForResponse
		{
			get
			{
				bool result;
				EDIMessage message = LastMessage;
				if (message == null)
				{
					result = false;
				}
				else
				{
					result = (message.EM_ReceiveTransmit == "TRX");
				}

				return result;
			}
		}

		#region GrossWeightForBinding

		[DecimalPlaces(3)]
		public ZDecimal GrossWeightForBinding
		{
			get { return JobContainer.JC_GrossWeight; }
			set
			{
				if (JobContainer.StandAloneCustomsContainer)
				{
					JobContainer.JC_GrossWeight = value;
				}
			}
		}

		public bool GrossWeightForBinding_ReadOnly
		{
			get { return !JobContainer.StandAloneCustomsContainer || JobContainer.IsGrossWeightReadOnly; }
		}

		public ZPropertyInfo GrossWeightForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GrossWeightForBinding), x => JobContainer.JC_GrossWeightInfo); }
		}

		#endregion

		#region PivotsToEntries
		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ContainerCusContainerEntryHeaderPivotCollection PivotsToEntries
		{
			get
			{
				if (pivotsToEntries == null)
				{
					pivotsToEntries = new ContainerCusContainerEntryHeaderPivotCollection(this);
					if (Declaration?.SupportContainerEntryHeaderPivot ?? false)
					{
						pivotsToEntries.Load();
						RegisterEditableChildObject(pivotsToEntries);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)pivotsToEntries).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						pivotsToEntries.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return pivotsToEntries;
			}
		}
		ContainerCusContainerEntryHeaderPivotCollection pivotsToEntries;
		#endregion PivotsToEntries

		#endregion

		#region New Methods

		public ZString GetContainerModeFromFreight(ZString containerModeInFreight)
		{
			if (containerModeInFreight == Enterprise.Core.Constants.ContainerModes.ShippersConsol)
			{
				var isMultiShipmentsPackedIntoAContainer = JobContainer.GetParentShipments().Count() > 1;
				var isImport = JobContainer.IsImport();
				var loginCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				return ModeConverter.ConvertFreightToCustomsForSCNContainer(isMultiShipmentsPackedIntoAContainer, isImport, loginCountry);
			}
			else
			{
				return ModeConverter.ConvertFreightToCustoms(containerModeInFreight);
			}
		}

		#endregion

		#region Ref Container Properties

		#region CO_Ref_StorageClass

		public ZString CO_Ref_StorageClass => Container?.RC_StorageClass ?? ZString.Empty;

		public ZPropertyInfo CO_Ref_StorageClassInfo => GetZPropertyInfo(Schema.CO_Ref_StorageClass);

		#endregion

		#region CO_Ref_ContainerCapacity

		public ZDecimal CO_Ref_ContainerCapacity
		{
			get { return (Container != null) ? Container.RC_CubicCapacity : new ZDecimal(0m); }
		}

		public ZPropertyInfo CO_Ref_ContainerCapacityInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Ref_ContainerCapacity); }
		}

		#endregion

		#region CO_Ref_MaxGrossWeight

		public ZDecimal CO_Ref_MaxGrossWeight
		{
			get { return (Container != null) ? Container.RC_GrossWeight : new ZDecimal(0m); }
		}

		public ZPropertyInfo CO_Ref_MaxGrossWeightInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Ref_MaxGrossWeight); }
		}

		#endregion

		#region CO_Ref_TareWeight

		public ZDecimal CO_Ref_TareWeight
		{
			get { return (Container != null) ? Container.RC_TareWeight : new ZDecimal(0m); }
		}

		public ZPropertyInfo CO_Ref_TareWeightInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Ref_TareWeight); }
		}

		#endregion

		#region CO_Ref_Length

		public ZDecimal CO_Ref_Length
		{
			get { return (Container != null) ? Container.RC_Length : new ZDecimal(0m); }
		}

		public ZPropertyInfo CO_Ref_LengthInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Ref_Length); }
		}

		#endregion

		#region CO_Ref_Width

		public ZDecimal CO_Ref_Width
		{
			get { return (Container != null) ? Container.RC_Width : new ZDecimal(0m); }
		}

		public ZPropertyInfo CO_Ref_WidthInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Ref_Width); }
		}

		#endregion

		#region CO_Ref_Height

		public ZDecimal CO_Ref_Height
		{
			get { return (Container != null) ? Container.RC_Height : new ZDecimal(0m); }
		}

		public ZPropertyInfo CO_Ref_HeightInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Ref_Height); }
		}

		#endregion

		#endregion

		#region Calc Properties

		#region CO_Calc_ActualCapacity

		public ZDecimal CO_Calc_ActualCapacity
		{
			get { return Core.Constants.Volume.Convert(TotalHeight * TotalWidth * TotalLength, Core.Constants.Volume.CubicFeet, Core.Constants.Volume.CubicMetres); }
		}

		public ZPropertyInfo CO_Calc_ActualCapacityInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Calc_ActualCapacity); }
		}

		#endregion

		#region CO_Calc_OverhangLength

		public ZDecimal CO_Calc_OverhangLength
		{
			get { return (TotalLength > CO_Ref_Length) ? TotalLength - CO_Ref_Length : 0m; }
		}

		public ZPropertyInfo CO_Calc_OverhangLengthInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Calc_OverhangLength); }
		}

		#endregion

		#region CO_Calc_OverhangWidth

		public ZDecimal CO_Calc_OverhangWidth
		{
			get { return (TotalWidth > CO_Ref_Width) ? TotalWidth - CO_Ref_Width : 0m; }
		}

		public ZPropertyInfo CO_Calc_OverhangWidthInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Calc_OverhangWidth); }
		}

		#endregion

		#region CO_Calc_OverhangHeight

		public ZDecimal CO_Calc_OverhangHeight
		{
			get { return (TotalHeight > CO_Ref_Height) ? TotalHeight - CO_Ref_Height : 0m; }
		}

		public ZPropertyInfo CO_Calc_OverhangHeightInfo
		{
			get { return GetZPropertyInfo(Schema.CO_Calc_OverhangHeight); }
		}

		#endregion

		#endregion

		#region JobServices

		public class JobServiceCollectionWrapper : BusinessObjectCollectionView<JobService>
		{
			public JobServiceCollectionWrapper(JobServiceDependentCollection collection)
				: base(collection)
			{
			}

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return true;
			}
		}

		public JobServiceCollectionWrapper Services
		{
			get
			{
				if (services == null)
				{
					services = new JobServiceCollectionWrapper(JobContainer.Services);
				}
				else
				{
					services.SwapCollectionToFilter(JobContainer.Services);
				}
				return services;
			}
		}
		JobServiceCollectionWrapper services;

		#endregion

		#region ImportPenalties

		public ContainerPenaltyCollection ImportPenalties => JobContainer.ImportPenalties;

		#endregion

		#region ExportPenalties

		public ContainerPenaltyCollection ExportPenalties => JobContainer.ExportPenalties;

		#endregion

		#region Fetch Hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.BaseCusContainerFetchStrategy(this);
		}

		#endregion

		#region ICustomLabelsProvider

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				this.fConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(BaseCusContainer), configOrg, ResString.GetMultilingualString("7465dd47-06d1-4c4c-bf4e-0c60e1533a0b", "buyer on the declaration"), factory);
				result.Add(Constants.CustomLabels.CusContainer.CustomAttribute1, BaseCusContainer.Schema.CO_CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.CusContainer.CustomFlag1, BaseCusContainer.Schema.CO_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.CusContainer.CustomDate1, BaseCusContainer.Schema.CO_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.CusContainer.CustomDecimal1, BaseCusContainer.Schema.CO_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				return result;
			}

			protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
		}

		#endregion

		#region IContainer

		ZString IContainer.ContainerMode
		{
			get { return CO_FCL_LCL_AIR == Core.Constants.TransportModes.Air ? ZString.Empty : CO_FCL_LCL_AIR; }
		}

		ZBool IContainer.IsCustomsHold
		{
			get { return false; }
		}

		ZBool IContainer.IsFumigationRequired
		{
			get { return false; }
		}

		ZBool IContainer.IsQuarantineRequired
		{
			get { return false; }
		}

		#endregion

		#region IContainerExtraParent

		public ZDecimal GoodsWeight
		{
			get { return CO_Weight; }
		}

		public ZString GoodsWeightUQ
		{
			get { return CO_WeightUQ; }
		}

		#endregion

		#region IDocAddresses

		#region DocAddresses

		[ChildEditable(false)]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		#endregion

		#region DocAddressManager

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager();
				}
				return fDocAddressManager;
			}
		}

		JobDocAddressManager fDocAddressManager;

		#endregion

		public virtual JobDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return GetSupportedAddressTypesCore(); }
		}

		protected virtual DocAddressType[] GetSupportedAddressTypesCore()
		{
			return Array.Empty<DocAddressType>();
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}
		#endregion

		#region ISupportDataImporting Members

		bool fIsImportingData;
		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region ITreeViewNode Members

		public ZPropertyInfo LabelInfo
		{
			get { return CO_ContainerNumberInfo; }
		}

		#endregion

		#region ILandedCostDistributeTo Members

		ZString ILandedCostDistributeTo.UniqueCode
		{
			get { return (NoResString)"Container " + CO_ContainerNumber; }
		}

		ZString ILandedCostDistributeTo.Description
		{
			get { return ((ILandedCostDistributeTo)this).UniqueCode; }
		}

		ZGuid ILandedCostDistributeTo.PK
		{
			get { return PK; }
		}

		ZString ILandedCostDistributeTo.TableCode
		{
			get { return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(BusinessObjectFactory.GetTableNameFromType(typeof(BaseCusContainer))); }
		}

		IEnumerable<IUltimateDistributee> ILandedCostDistributeTo.UltimateDistributees
		{
			get { return new TypedEnumerable<IUltimateDistributee>(InvoiceLinePivotCollection.InvoiceLinesAssociated); }
		}

		#endregion
		#region IContainerForBinding Members

		[ResourceStringData("78752F08-30E1-44E0-8738-D75C30605328", Caption = "Container", FullDescription = "Container Number")]
		public virtual ZString ContainerNumberForBinding
		{
			get { return CO_ContainerNumber; }
			set { CO_ContainerNumber = value; }
		}

		public ZPropertyInfo ContainerNumberForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ContainerNumberForBinding), x => CO_ContainerNumberInfo); }
		}

		public virtual ZString SealNumberForBinding
		{
			get { return CO_Seal; }
			set { CO_Seal = value; }
		}

		public ZPropertyInfo SealNumberForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SealNumberForBinding), x => CO_SealInfo); }
		}

		[List(nameof(JobContainer) + "." + nameof(CommonContainer.RefContainer_List))]
		public ZGuid RCForBinding
		{
			get { return CO_RC; }
			set { CO_RC = value; }
		}

		public ZPropertyInfo RCForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RCForBinding), x => CO_RCInfo); }
		}

		[List(nameof(ContainerMode_ListForBinding))]
		public ZString ContainerModeForBinding
		{
			get { return CO_FCL_LCL_AIR; }
			set { CO_FCL_LCL_AIR = value; }
		}

		public ZPropertyInfo ContainerModeForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ContainerModeForBinding), x => CO_FCL_LCL_AIRInfo); }
		}

		[DecimalPlaces("GoodsWeightForBindingDecimalPlaces")]
		[MeasureUnit("WeightUnitForBinding", MeasureUnitType.Weight)]
		public ZDecimal GoodsWeightForBinding
		{
			get { return CO_Weight; }
			set { CO_Weight = value; }
		}

		public ZPropertyInfo GoodsWeightForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GoodsWeightForBinding), x => CO_WeightInfo); }
		}

		[List(nameof(JobContainer) + "." + nameof(CommonContainer.TotalWeightUnit_List))]
		public ZString WeightUnitForBinding
		{
			get { return CO_WeightUQ; }
			set { CO_WeightUQ = value; }
		}

		public ZPropertyInfo WeightUnitForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(WeightUnitForBinding), x => CO_WeightUQInfo); }
		}

		public CodeDescriptionPairList ContainerMode_ListForBinding
		{
			get { return Lookups.CO_FCL_LCL_NCT_List; }
		}

		public int GoodsWeightForBindingDecimalPlaces
		{
			get
			{
				var result = 0;
				var jobContainer = JobContainer;
				if (jobContainer != null)
				{
					result = jobContainer.ShouldApplyDefaultNumberOfDecimalsRegistry
						? DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(jobContainer, jobContainer.JC_GrossWeightInfo.PropertyDescriptor)
						: jobContainer.GetDecimalPlacesMetaDataIgnoringRegistry(jobContainer.JC_GrossWeightInfo.PropertyDescriptor);
				}
				return result;
			}
		}

		#endregion

		#region IDocumentSupportable
		public DocumentSupporter DocumentSupporter
		{
			get { return CreateNewDocumentSupporter(); }
		}

		protected virtual DocumentSupporter CreateNewDocumentSupporter()
		{
			return new CusContainerDocumentSupporter(this);
		}
		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CustomsContainer);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion
		#region ICartageContainer Members

		ZString ICartageContainer.ContainerMode
		{
			get { return ModeConverter.ConvertCustomsToFreight(CO_FCL_LCL_AIR); }
		}

		ZString ICartageContainer.ContainerNumber
		{
			get { return CO_ContainerNumber; }
		}

		ZGuid ICartageContainer.ContainerRC
		{
			get { return CO_RC; }
		}

		ZGuid ICartageContainer.JobContainerPK
		{
			get { return JobContainer.PK; }
		}

		IReadOnlyCollection<ICartageLooseCargo> ICartageContainer.LooseCargo
		{
			get { return Array.Empty<ICartageLooseCargo>(); }
		}

		ZDecimal ICartageContainer.NetWeight
		{
			get { return CO_Weight; }
		}

		ZString ICartageContainer.Seal
		{
			get { return CO_Seal; }
		}
		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CO_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CO_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(CusContainerInvoiceLinePivot), CusContainerInvoiceLinePivotSchema.C2_CO);
			}
		}

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();
		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();
		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => RegisterListChangedCalledRefreshBinding(element);
		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);
		#endregion

		#region IGlobalSearchBusinessObjectProvider

		BusinessObject IGlobalSearchBusinessObjectProvider.BusinessObjectForController => Declaration;

		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded() => this.PopulateDataModelFromParentIfNeeded(Declaration);

		ZString IDataModelSupporter.DataModel { get => CO_DataModel; set => CO_DataModel = value; }

		#endregion
	}

	#region CusContainerDocumentSupporter

	public class CusContainerDocumentSupporter : DocumentSupporter
	{
		public CusContainerDocumentSupporter(BaseCusContainer container)
			: base(container)
		{
		}

		protected BaseCusContainer Container
		{
			get { return (BaseCusContainer)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
				{
					Enterprise.Core.Constants.DataContext.Service
				};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusContainer; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.Service)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateCustomsContainerWrapperWithDeclaration(Container, Container.Declaration, Container.Declaration != null ? Container.Declaration.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode) };
			}
			return null;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		#endregion
	}

	#endregion
}
