using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using PkgPackageCollection = Enterprise.Freight.Business.PkgPackageCollection;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackLine :
		PackLine, Integration.Forwarding.IForwardingPackLine,
		ICartageLooseCargo,
		IRequiredTemperature,
		ICanDelete,
		ICargoDimensions,
		IAdditionalReferenceNumberTypeProvider
	{
		public ForwardingPackLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : PackLine.Schema
		{
			public const string ProductsCollection = "Products";
		}

		#endregion

		#region Related BusinessObjects

		public new ForwardingPackageCollection PkgPackageCollection => (ForwardingPackageCollection)base.PkgPackageCollection;

		protected override PkgPackageCollection GetPackageCollectionCore() => new ForwardingPackageCollection(this);

		public new ForwardingShipment Shipment => (ForwardingShipment)base.Shipment;

		public IQuotedBooking QuotedBooking
		{
			get
			{
				if (quotedBooking == null)
				{
					var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
					quotedBooking = quotedBookingBuilder.Load(Factory, JL_JS) as IQuotedBooking;
				}

				return quotedBooking;
			}
		}

		IQuotedBooking quotedBooking;

		public new ForwardingContainerManyToManyCollection Containers
		{
			get { return (ForwardingContainerManyToManyCollection)base.Containers; }
		}

		protected override CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			return new ForwardingContainerManyToManyCollection(this);
		}

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<ForwardingShipment>(JL_JS);
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<ForwardingContainer>(containerPK);
		}

		#region Products

		[ChildEditable(true)]
		public PackProductCollection Products
		{
			get
			{
				if (fProducts == null)
				{
					fProducts = new PackProductCollection(this);
					RegisterEditableChildObject(fProducts);
				}

				return fProducts;
			}
		}
		PackProductCollection fProducts;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			Products.DeleteAll();
			PortReferences.DeleteAll();
			AdditionalReferenceNumbers.RemoveAndDeleteAll();

			var portMessagingHelper = ObjectFactory.Get<Integration.Forwarding.IPortMessagingForwardingHelper>();
			if (portMessagingHelper != null)
			{
				portMessagingHelper.OnPacklineDelete(Factory, PK);
			}

			using (Shipment?.MonitorRequireTEUChange(new CO2eStatusChangedReason((NoResString)"PackLine removed")))
			{
				base.Delete();
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return (!IsInDatabase || !HasAnyClass7Substances_WithoutSecurityRights()) && base.CanDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return HasAnyClass7Substances_WithoutSecurityRights()
					? ResString.GetMultilingualString(
						"7f2aa53a-7f13-4878-8b50-65a4569fb90c",
						"You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment."
					)
					: base.ReasonForNotAbleToDelete;
			}
		}

		bool HasAnyClass7Substances_WithoutSecurityRights() =>
			!Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed &&
			UNDGs.OfType<ForwardingUNDGDataItem>().Any(undg => undg.Validation.IsClass7RadioactiveSubstance());

		#endregion

		#region Validation

		public new ForwardingPackLineValidation Validation
		{
			get { return (ForwardingPackLineValidation)base.Validation; }
		}

		protected override JobPackLinesValidation GetNewValidation()
		{
			return new ForwardingPackLineValidation(this);
		}

		#endregion

		#region Properties

		public override ZGuid JL_JC
		{
			get { return base.JL_JC; }
			set
			{
				if (base.JL_JC != value)
				{
					base.JL_JC = value;
					TriggerSynchronise();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_JC();
						Validation.ValidateJL_PackageCount();
					}
				}
			}
		}

		public override ZInt JL_PackageCount
		{
			get { return base.JL_PackageCount; }
			set
			{
				if (base.JL_PackageCount != value)
				{
					base.JL_PackageCount = value;
					TriggerSynchronise();

					if (!IsValidationSuspended)
					{
						foreach (ForwardingContainer container in Containers)
						{
							container.Validation.ValidateJC_IsEmptyContainer();
						}
					}
				}
			}
		}

		public override ZString JL_F3_NKPackType
		{
			get { return base.JL_F3_NKPackType; }
			set
			{
				if (base.JL_F3_NKPackType != value)
				{
					base.JL_F3_NKPackType = value;
					TriggerSynchronise();
				}
			}
		}

		public override ZString JL_MarksAndNumbers
		{
			get { return base.JL_MarksAndNumbers; }
			set
			{
				if (base.JL_MarksAndNumbers != value)
				{
					base.JL_MarksAndNumbers = value;
					TriggerSynchronise();
				}
			}
		}

		public override ZDecimal JL_ActualWeight
		{
			get { return base.JL_ActualWeight; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobPackLinesSchema.JL_ActualWeight, JL_ActualWeightInfo, value);

				if (base.JL_ActualWeight != roundedValue)
				{
					base.JL_ActualWeight = roundedValue;
					TriggerSynchronise();
				}
			}
		}

		public override ZString JL_ActualWeightUQ
		{
			get { return base.JL_ActualWeightUQ; }
			set
			{
				if (base.JL_ActualWeightUQ != value)
				{
					base.JL_ActualWeightUQ = value;
					TriggerSynchronise();
				}
			}
		}

		public bool BlindPackageAttached
		{
			get => blindPackageAttached;
			set => blindPackageAttached = value;
		}
		bool blindPackageAttached;

		#region JL_RequiredTemperatureUnit

		public override ZString JL_RequiredTemperatureUnit
		{
			get => base.JL_RequiredTemperatureUnit;
			set
			{
				if (base.JL_RequiredTemperatureUnit != value)
				{
					base.JL_RequiredTemperatureUnit = value;
					UNDGs.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JL_IsHighRisk

		public override ZBool JL_IsHighRisk
		{
			get
			{
				return base.JL_IsHighRisk;
			}
			set
			{
				if (base.JL_IsHighRisk != value)
				{
					if (!JL_IsHighRiskHasChanges)
					{
						JL_IsHighRiskOriginalValue = JL_IsHighRisk;
						JL_IsHighRiskHasChanges = true;
					}

					if (!value)
					{
						JL_AdditionalInspectionTypeCode = AdditionalInspectionTypeCodeDefault;
					}

					base.JL_IsHighRisk = value;

					RedefaultShipmentAdditionalInspectionTypeCodes();

					if (value
						&& Shipment != null
						&& !Shipment.JS_IsHighRisk)
					{
						Shipment.ApplyShipmentIsHighRisk = true;
						Shipment.IsUpdatedByIsHighRiskPackLineLevel = true;
						Shipment.JS_IsHighRisk = true;
					}
					else if (!value
						&& Shipment != null
						&& Shipment.JS_IsHighRisk
						&& !Shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(x => x.JL_IsHighRisk))
					{
						Shipment.ApplyShipmentIsHighRisk = true;
						Shipment.IsUpdatedByIsHighRiskPackLineLevel = true;
						Shipment.JS_IsHighRisk = false;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_IsHighRisk();
						Shipment?.Validation.ValidateJS_AdditionalInspectionTypeCode();
					}
				}
			}
		}

		public bool JL_IsHighRiskHasChanges { get; set; }

		public ZBool JL_IsHighRiskOriginalValue { get; set; }

		public bool JL_IsHighRisk_ReadOnly => Shipment.AviationSecurity.SupplyChainSecurityConfiguration.JL_IsHighRisk_ReadOnly(Shipment) || !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed;

		internal void ReDefaultIsHighRisk(bool redefaultFromShipment)
		{
			if (redefaultFromShipment)
			{
				JL_IsHighRisk = Shipment.JS_IsHighRisk;
			}

			Validation.ValidateJL_IsHighRisk();
		}

		#endregion

		#region JL_AdditionalInspectionTypeCode

		public override ZString JL_AdditionalInspectionTypeCode
		{
			get { return base.JL_AdditionalInspectionTypeCode; }
			set
			{
				if (base.JL_AdditionalInspectionTypeCode != value)
				{
					if (!JL_AdditionalInspectionTypeCodeHasChanges)
					{
						JL_AdditionalInspectionTypeCodeOriginalValue = JL_AdditionalInspectionTypeCode;
					}
					base.JL_AdditionalInspectionTypeCode = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_AdditionalInspectionTypeCode();
						Shipment?.Validation.ValidateJS_AdditionalInspectionTypeCode();
					}

					RedefaultShipmentAdditionalInspectionTypeCodes();
				}
			}
		}

		public ZString JL_AdditionalInspectionTypeCodeOriginalValue { get; set; }

		void RedefaultShipmentAdditionalInspectionTypeCodes()
		{
			if (Shipment != null)
			{
				using (Shipment.OverrideChangingAdditionalInspectionStatusReason((NoResString)"Additional Inspection Packlines level updated"))
				{
					if (Shipment.JS_IsHighRisk && !JL_AdditionalInspectionTypeCodeInfo.HasErrors() && Shipment.HaveFinishedRedefault)
					{
						Shipment.IsUpdatedByAdditionalInspectionTypePackLineLevel = true;
						Shipment.ApplyShipmentAdditionalInspectionType = true;
						var distinctAdditionalInspectionTypeCodes = Shipment.OuterPackLines.Cast<ForwardingPackLine>()
							.Where(x => x.JL_IsHighRisk)
							.Select(x => x.JL_AdditionalInspectionTypeCode)
							.GroupBy(x => x)
							.Select(x => x.Key);

						if (distinctAdditionalInspectionTypeCodes.Contains(FreightDataRegistry.AviationSecurity_Unknown_Code) && Shipment.JS_AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code)
						{
							Shipment.JS_AdditionalInspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
						}
						else if (distinctAdditionalInspectionTypeCodes.Count() == 1 && Shipment.JS_AdditionalInspectionTypeCode != distinctAdditionalInspectionTypeCodes.First())
						{
							Shipment.JS_AdditionalInspectionTypeCode = distinctAdditionalInspectionTypeCodes.First();
						}
						else if (distinctAdditionalInspectionTypeCodes.Count() > 1 && !distinctAdditionalInspectionTypeCodes.Contains(FreightDataRegistry.AviationSecurity_Unknown_Code) && Shipment.JS_AdditionalInspectionTypeCode != BaseJobShipmentLookups.InspectionType_Screened)
						{
							Shipment.JS_AdditionalInspectionTypeCode = BaseJobShipmentLookups.InspectionType_Screened;
						}
						Shipment.IsUpdatedByAdditionalInspectionTypePackLineLevel = false;
					}
				}
			}
		}

		protected override ZString AdditionalInspectionTypeCodeDefault => Shipment.GetAdditionalInspectionTypeCodeDefault();

		protected override bool JL_AdditionalInspectionTypeCode_ReadOnly
		{
			get
			{
				if (Shipment == null)
				{
					return true;
				}
				return Shipment.AviationSecurity.SupplyChainSecurityConfiguration.JL_AdditionalInspectionTypeCode_ReadOnly(Shipment) || !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed || !JL_IsHighRisk;
			}
		}

		internal void ReDefaultAdditionalInspectionTypeCode(bool redefaultFromShipment)
		{
			if (JL_AdditionalInspectionTypeCodeInfo.ReadOnly)
			{
				return;
			}
			if (Shipment == null
				|| !Shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsHighRiskApplicable)
			{
				JL_AdditionalInspectionTypeCode = ZString.Empty;
			}
			else if (redefaultFromShipment)
			{
				JL_AdditionalInspectionTypeCode = Shipment.JS_AdditionalInspectionTypeCode;
			}

			Validation.ValidateJL_AdditionalInspectionTypeCode();
		}

		#endregion

		#region JL_InspectionTypeCode

		public override ZString JL_InspectionTypeCode
		{
			get => base.JL_InspectionTypeCode;
			set
			{
				if (value != JL_InspectionTypeCode)
				{
					if (!JL_InspectionTypeCodeHasChanges)
					{
						JL_InspectionTypeCodeOriginalValue = JL_InspectionTypeCode;
					}

					base.JL_InspectionTypeCode = value;

					if (Shipment != null
						&& !Shipment.IsRedefaultingInspectionTypeCodesSuspended
						&& (value != FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code || Shipment.IsMarkingPackLinesAsSecuredAllowed))
					{
						using (Shipment.SuspendRedefaultingInspectionTypeCodes())
						{
							var shipmentPackLines = Shipment.OuterPackLines.Cast<ForwardingPackLine>();

							if (!value.IsEmpty)
							{
								foreach (var packline in shipmentPackLines.Where(p => p.JL_InspectionTypeCode.IsEmpty))
								{
									packline.JL_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
								}
							}

							if (shipmentPackLines.Any(p => p.JL_InspectionTypeCode.IsEmpty || p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
							{
								Shipment.JS_InspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
							}
							else if (shipmentPackLines.All(p => p.JL_InspectionTypeCode == value))
							{
								Shipment.JS_InspectionTypeCode = value;
							}
							else
							{
								Shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Screened;
							}
						}
					}

					Shipment?.Validation.ValidateJS_InspectionTypeCode();
				}
			}
		}

		public ZString JL_InspectionTypeCodeOriginalValue { get; set; }

		protected override ZString InspectionTypeCodeDefault
		{
			get
			{
				if (Shipment == null
					|| !Shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(Shipment)
					|| Shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved)
				{
					return ZString.Empty;
				}

				var inspectionTypeDefault = Shipment.AviationSecurity.SupplyChainSecurityConfiguration.InspectionTypeDefault;

				if (!Shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningRequired(Shipment))
				{
					if (Shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened
						|| Shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(p => p.InspectionTypeCusEntryNumber != null && !p.InspectionTypeCusEntryNumber.CE_EntryNum.IsEmpty))
					{
						return inspectionTypeDefault;
					}

					return inspectionTypeDefault == FreightDataRegistry.AviationSecurity_Unknown_Code ? ZString.Empty : inspectionTypeDefault;
				}

				return inspectionTypeDefault;
			}
		}

		protected override bool JL_InspectionTypeCode_ReadOnly
		{
			get
			{
				if (Shipment == null)
				{
					return true;
				}

				return Shipment.AviationSecurity.SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(Shipment)
					|| !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed
					|| (JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code && !JL_InspectionTypeCodeHasChanges);
			}
		}

		internal void ReDefaultInspectionTypeCode(bool redefaultFromShipment)
		{
			if (Shipment == null
				|| !Shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(Shipment))
			{
				JL_InspectionTypeCode = ZString.Empty;
			}
			else if (redefaultFromShipment)
			{
				JL_InspectionTypeCode = Shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved ? InspectionTypeCodeDefault : Shipment.JS_InspectionTypeCode;
			}
			else if (Shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
				|| (Shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningRequired(Shipment)
					&& (JL_InspectionTypeCode.IsEmpty || JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code)))
			{
				JL_InspectionTypeCode = InspectionTypeCodeDefault;
			}

			Validation.ValidateJL_InspectionTypeCode();
		}

		#endregion

		public override bool ReadOnly
		{
			get { return !SuspendReadOnly && base.ReadOnly; }
		}

		bool SuspendReadOnly
		{
			get
			{
				foreach (var collection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					var forwardingPackLineCollection = collection as ForwardingPackLineCollection;

					if (forwardingPackLineCollection != null && forwardingPackLineCollection.SuspendReadOnly)
					{
						return true;
					}
				}
				return false;
			}
		}

		void TriggerSynchronise()
		{
			if (Shipment != null && Shipment.PackLineSynchroniser != null)
			{
				Shipment.PackLineSynchroniser.MarkSyncDirty();
			}
		}

		#region LooseCargoContainerType

		[BusinessObjectMaxLengthTestExclude]
		[List("LooseCargoContainerType_List")]
		[MaxLength(10)]
		public ZString LooseCargoContainerType
		{
			get
			{
				if (ContainerType != null)
				{
					return ContainerType.RC_Code;
				}

				return ZString.Empty;
			}

			set
			{
				var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, value);
				if (container != null)
				{
					JL_RC_ContainerType = container.PK;
				}
				else
				{
					JL_RC_ContainerType = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLooseCargoContainerType();
				}

				JL_RC_ContainerTypeInfo.RefreshBinding();
				LooseCargoContainerTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LooseCargoContainerTypeInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(LooseCargoContainerType)); }
		}

		#endregion

		#region Q-value

		[DecimalPlaces(1)]
		public ZDecimal QValue
		{
			get
			{
				var qValue = GetQValue();
				return MathExtensionHelper.RoundUp(qValue, 1);
			}
		}

		public ZPropertyInfo QValueInfo
		{
			[DebuggerStepThrough]
			get { return GetZPropertyInfo(nameof(QValue)); }
		}

		public bool AreMultipleDGsPacked => UNDGs.Count > 0 && UNDGs.Select(undg => undg.DI_DG).Distinct().IsCountMoreThan(1);

		ZDecimal GetQValue()
		{
			if (Shipment == null || !AreMultipleDGsPacked)
			{
				return 0;
			}

			var qValue = new ZDecimal(0);

			var isCargoOnly = Shipment.DepartureConsol?
				.MostInterestingTransportForBinding
				.Cast<Transport>().FirstOrDefault()
				?.JW_IsCargoOnly ?? true;

			foreach (var undg in ShippersDeclarationUNDGExclusions.GetUNDGsRequiringForQValue(UNDGs))
			{
				var undgHasWeightUnit = Constants.Weight.ContainsCode(undg.DI_UnitOfWeight);
				var undgHasVolumeUnit = Constants.Volume.ContainsCode(undg.DI_UnitOfVolume);

				var substance = undg.Substance;
				var isLimitedQuantity = undg.DI_IsLimitedQuantity;
				var maxAllowed = new ZDecimal();

				if (substance == null)
				{
					continue;
				}

				if (LithiumBatteryConstants.UNNOCodes.CodesList.Contains(substance.DG_UNNO))
				{
					var packingInstructionSection = undg.DI_PackingInstructionSection;
					var packingInstruction = isCargoOnly
						? (substance?.DG_CargoPackIns ?? ZString.Empty)
						: (substance?.DG_PaxPackIns ?? ZString.Empty);

					var permissibleQuantities = UNDGPermissableQuantitiesHelper.GetUNDGPermissibleQuantities();
					var undgPermissibleQuantity = permissibleQuantities.FirstOrDefault(permissibleQuantity => permissibleQuantity.PackingInstruction == packingInstruction
					&& permissibleQuantity.PackingInstructionSection == packingInstructionSection);

					maxAllowed = Core.Constants.Weight.ConvertSafe(undgPermissibleQuantity.CaoLimit, undgPermissibleQuantity.CaoLimitUnits, undg.DI_UnitOfWeight);
				}
				else
				{
					maxAllowed = isLimitedQuantity
					? substance.DG_LQMaxAmt
					: isCargoOnly ? substance.DG_CargoMaxAmt : substance.DG_LQ2OrPaxMaxAmt;
				}

				if (maxAllowed <= 0)
				{
					continue;
				}

				var limitType = isLimitedQuantity
					? substance.DG_LQMaxAmtType
					: isCargoOnly ? substance.DG_CargoPackAmtType : substance.DG_LQ2OrPaxMaxAmtType;

				var maxAmoutUQ = isLimitedQuantity
					? substance.DG_LQMaxAmtUQ
					: isCargoOnly ? substance.DG_CargoMaxAmtUQ : substance.DG_LQ2OrPaxMaxAmtUQ;

				var shouldCalculateQValue = limitType == UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode || limitType == UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;
				var isWeightMaxAmountUQ = Constants.Weight.ContainsCode(maxAmoutUQ.ToUpperInvariant());
				var isVolumeMaxAmountUQ = Constants.Volume.ContainsCode(maxAmoutUQ.ToUpperInvariant());
				var quantity = 0m;

				if (undgHasWeightUnit && (maxAmoutUQ.IsEmpty || isWeightMaxAmountUQ))
				{
					quantity = Constants.Weight.Convert(undg.DI_DGWeight, undg.DI_UnitOfWeight, Core.Constants.Weight.Kilograms);
				}
				else if (undgHasVolumeUnit && (maxAmoutUQ.IsEmpty || isVolumeMaxAmountUQ))
				{
					quantity = Constants.Volume.Convert(undg.DI_DGVolume, undg.DI_UnitOfVolume, Core.Constants.Volume.Litre);
				}

				if (shouldCalculateQValue && quantity > 0)
				{
					qValue += quantity / maxAllowed;
				}
			}
			return qValue;
		}

		#endregion

		#endregion

		#region SetContainer

		public override void SetContainer(CommonConsol consol, CommonContainer newValue)
		{
			base.SetContainer(consol, newValue);
			TriggerSynchronise();
		}

		#endregion

		#region ICartageLooseCargo Members

		ZString ICartageLooseCargo.BookedPackType
		{
			get { return JL_F3_NKPackType; }
		}

		ZInt ICartageLooseCargo.BookedPackages
		{
			get { return JL_PackageCount; }
		}

		ZDecimal ICartageLooseCargo.BookedVolume
		{
			get { return JL_ActualVolume; }
		}

		ZString ICartageLooseCargo.BookedVolumeUnit
		{
			get { return JL_ActualVolumeUQ; }
		}

		ZDecimal ICartageLooseCargo.BookedWeight
		{
			get { return JL_ActualWeight; }
		}

		ZString ICartageLooseCargo.BookedWeightUnit
		{
			get { return JL_ActualWeightUQ; }
		}

		ZDecimal ICartageLooseCargo.BookedHeight
		{
			get { return JL_Height; }
		}

		ZDecimal ICartageLooseCargo.BookedWidth
		{
			get { return JL_Width; }
		}

		ZDecimal ICartageLooseCargo.BookedLength
		{
			get { return JL_Length; }
		}

		ZString ICartageLooseCargo.BookedDimensionUnit
		{
			get { return JL_UnitOfDimension; }
		}

		IReadOnlyCollection<UNDGDataItem> ICartageLooseCargo.DangerousGoods
		{
			get { return UNDGs.ToArray(); }
		}
		#endregion

		#region ICheckFitsInConsol

		ZString ICargoDimensions.DimensionsUnits => JL_UnitOfDimension;

		ZDecimal ICargoDimensions.Length => JL_Length;

		ZDecimal ICargoDimensions.Width => JL_Width;

		ZDecimal ICargoDimensions.Height => JL_Height;

		#endregion

		#region UNDG

		protected override UNDGDataItemCollection GetNewUNDGs()
		{
			return new ForwardingUNDGDataItemCollection(this);
		}

		#endregion

		#region CommodityCode

		public override ZString JL_RH_NKCommodityCode
		{
			get => base.JL_RH_NKCommodityCode;
			set
			{
				if (base.JL_RH_NKCommodityCode != value)
				{
					base.JL_RH_NKCommodityCode = value;

					var refCommodityCodeBO = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, value);
					if (refCommodityCodeBO != null && (refCommodityCodeBO.RH_ReeferMinTemperature != 0 || refCommodityCodeBO.RH_ReeferMaxTemperature != 0))
					{
						JL_RequiredTemperatureMinimum = refCommodityCodeBO.RH_ReeferMinTemperature;
						JL_RequiredTemperatureMaximum = refCommodityCodeBO.RH_ReeferMaxTemperature;
						JL_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;
						JL_RequiresTemperatureControl = true;
					}

					if (Shipment != null)
					{
						Shipment.IsHazardousInfo.RefreshBinding();
						Shipment.IsPerishableInfo.RefreshBinding();
						Shipment.IsFlammableInfo.RefreshBinding();
						Shipment.IsTimberInfo.RefreshBinding();
						Shipment.IsContainerVentRequiredInfo.RefreshBinding();
						Shipment.CommodityCodesInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region Inner Packs

		public ZInt InnerPackCount
		{
			get
			{
				return InnerPackLines.Sum(packline => packline.JL_PackageCount);
			}
		}

		public ZPropertyInfo InnerPackCountInfo => GetZPropertyInfo(nameof(InnerPackCount));

		public ZString InnerPackType
		{
			get
			{
				if (InnerPackLines.Count == 0)
				{
					return ZString.Empty;
				}

				var uniqueInnerPackTypes = InnerPackLines
					.Select(packLine => packLine.JL_F3_NKPackType)
					.Distinct()
					.ToArray();

				if (uniqueInnerPackTypes.Length > 1)
				{
					return Constants.PkgUnit.Package;
				}

				return uniqueInnerPackTypes[0];
			}
		}

		public ZPropertyInfo InnerPackTypeInfo => GetZPropertyInfo(nameof(InnerPackType));

		[DecimalPlaces(3)]
		public ZDecimal InnerPackTotalWeight
		{
			get
			{
				var unit = JL_ActualWeightUQ;
				ZDecimal result = 0;
				foreach (var innerPackLine in InnerPackLines)
				{
					if (unit == innerPackLine.JL_ActualWeightUQ)
					{
						result += innerPackLine.JL_ActualWeight;
					}
					else
					{
						result += Constants.Weight.ConvertSafe(innerPackLine.JL_ActualWeight, innerPackLine.JL_ActualWeightUQ, unit, false);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo InnerPackTotalWeightInfo => GetZPropertyInfo(nameof(InnerPackTotalWeight));

		[DecimalPlaces(3)]
		public ZDecimal InnerPackTotalVolume
		{
			get
			{
				var unit = JL_ActualVolumeUQ;
				ZDecimal result = 0;
				foreach (var innerPackLine in InnerPackLines)
				{
					if (unit == innerPackLine.JL_ActualVolumeUQ)
					{
						result += innerPackLine.JL_ActualVolume;
					}
					else
					{
						result += Constants.Volume.ConvertSafe(innerPackLine.JL_ActualVolume, innerPackLine.JL_ActualVolumeUQ, unit, false);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo InnerPackTotalVolumeInfo => GetZPropertyInfo(nameof(InnerPackTotalVolume));

		[ChildEditable(true)]
		public NestedInnerPackLineCollection InnerPackLines
		{
			get
			{
				if (innerPackLines == null)
				{
					innerPackLines = new NestedInnerPackLineCollection(this);
					RegisterEditableChildObject(innerPackLines);
				}

				return innerPackLines;
			}
		}

		NestedInnerPackLineCollection innerPackLines;

		#endregion

		#region JL_JSL_BookingLine

		[RelatedBusinessObject("JobSupplierBookingLine")]
		[List("SupplierBookingLine_List")]
		public override ZGuid JL_JSL_BookingLine
		{
			get => base.JL_JSL_BookingLine;
			set => base.JL_JSL_BookingLine = value;
		}

		public JobSupplierBookingLine JobSupplierBookingLine => Factory.Load<JobSupplierBookingLine>(JL_JSL_BookingLine);

		public bool JL_JSL_BookingLine_ReadOnly => JL_JSL_BookingLine.IsEmpty || LoadListLine == null;

		#endregion

		#region Container Load List

		public ContainerLoadListLine LoadListLine => Factory.LoadTop1<ContainerLoadListLine>(new ZQuery(ContainerLoadListLineSchema.CLL_JL_PackLine, PK));

		public bool IsPackedForOrderPlanning => LoadListLine != null;

		#endregion

		#region JL_RequiresTemperatureControl

		public override ZBool JL_RequiresTemperatureControl
		{
			get
			{
				return base.JL_RequiresTemperatureControl;
			}
			set
			{
				if (base.JL_RequiresTemperatureControl != value)
				{
					base.JL_RequiresTemperatureControl = value;
					UNDGs.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JL_RequiredTemperatureMaximum

		public override ZDecimal JL_RequiredTemperatureMaximum
		{
			get => base.JL_RequiredTemperatureMaximum;
			set
			{
				if (base.JL_RequiredTemperatureMaximum != value)
				{
					base.JL_RequiredTemperatureMaximum = value;

					if (JL_RequiresTemperatureControl)
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateJL_RequiresTemperatureControl();
						}
					}
					else
					{
						JL_RequiresTemperatureControl = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_RequiredTemperatureMinimum();
					}

					UNDGs.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JL_RequiredTemperatureMinimum

		public override ZDecimal JL_RequiredTemperatureMinimum
		{
			get => base.JL_RequiredTemperatureMinimum;
			set
			{
				if (base.JL_RequiredTemperatureMinimum != value)
				{
					base.JL_RequiredTemperatureMinimum = value;

					if (JL_RequiresTemperatureControl)
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateJL_RequiresTemperatureControl();
						}
					}
					else
					{
						JL_RequiresTemperatureControl = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_RequiredTemperatureMaximum();
					}
				}
			}
		}

		#endregion

		#region JL_Dimensions

		public override ZDecimal JL_Length
		{
			get => base.JL_Length;
			set
			{
				base.JL_Length = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJL_Width();
					Validation.ValidateJL_Height();
					Validation.ValidateJL_UnitOfDimension();
				}
			}
		}

		public override ZDecimal JL_Width
		{
			get => base.JL_Width;
			set
			{
				base.JL_Width = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJL_Length();
					Validation.ValidateJL_Height();
					Validation.ValidateJL_UnitOfDimension();
				}
			}
		}

		public override ZDecimal JL_Height
		{
			get => base.JL_Height;
			set
			{
				base.JL_Height = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJL_Length();
					Validation.ValidateJL_Width();
					Validation.ValidateJL_UnitOfDimension();
				}
			}
		}

		public override ZString JL_UnitOfDimension
		{
			get => base.JL_UnitOfDimension;
			set
			{
				base.JL_UnitOfDimension = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJL_Length();
					Validation.ValidateJL_Width();
					Validation.ValidateJL_Height();
				}
			}
		}

		#endregion

		#region LastKnownTransitWarehouseStatusForBinding

		[List("JL_LastKnownTransitWarehouseStatus_List")]
		public ZString LastKnownTransitWarehouseStatusForBinding
		{
			get { return JL_LastKnownTransitWarehouseStatus; }
			set
			{
				if (value != JL_LastKnownTransitWarehouseStatus)
				{
					if (this.IsInDatabase
						&& JL_OriginTransitWarehouseStatus != FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed
						&& JL_OriginTransitWarehouseStatus != FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown
						&& value != (ZString)JL_LastKnownTransitWarehouseStatusInfo.OriginalValue)
					{
						if (CanChangeLastKnownTransitWarehouseStatus(JL_LastKnownTransitWarehouseStatus, value))
						{
							JL_LastKnownTransitWarehouseStatus = value;
						}
					}
					else
					{
						JL_LastKnownTransitWarehouseStatus = value;
					}
				}
			}
		}

		ZBool CanChangeLastKnownTransitWarehouseStatus(ZString oldValue, ZString newValue)
		{
			if (Shipment != null && LastKnownTransitWarehouseStatusChanging != null)
			{
				var args = new CancelEventArgs();
				LastKnownTransitWarehouseStatusChanging(this, args);
				return !args.Cancel;
			}

			return true;
		}

		public event EventHandler<CancelEventArgs> LastKnownTransitWarehouseStatusChanging;

		public ZPropertyInfo LastKnownTransitWarehouseStatusForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LastKnownTransitWarehouseStatusForBinding), x => JL_LastKnownTransitWarehouseStatusInfo); }
		}

		#endregion

		#region PortReferences

		public IPortReferenceCollection PortReferences
		{
			get
			{
				if (portReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<IPortReferenceCollectionProvider>();
					portReferenceNumbers = provider.GetCollection(this);
				}

				return portReferenceNumbers;
			}
		}
		IPortReferenceCollection portReferenceNumbers;

		#endregion

		#region AdditionalReferenceNumbers

		[ChildEditable]
		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					additionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					additionalReferenceNumbers.Load();
					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}
		CusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			return new ForwardingPackingLineAdditionalReferenceNumberTypes();
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			ForwardingPackLine clonedPackLine = (ForwardingPackLine)base.CloneInternal(args);

			if (IsOuterPackType && !args.IsExcludedFromCloning(Schema.ProductsCollection))
			{
				foreach (PackProduct product in Products)
				{
					List<string> properties = new List<string>();
					properties.Add(JobPackProductSchema.D2_JL.Name);
					PackProduct clonedProduct = (PackProduct)product.Clone(new BusinessObjectCloneArgs(properties));
					clonedProduct.D2_JL = clonedPackLine.PK;
				}
			}

			return clonedPackLine;
		}

		#endregion

		#region GeneratePackageWithID

		public PkgPackage GeneratePackageWithIDs()
		{
			var pkg = PkgPackageCollection.AddNew();
			pkg.KP_KJ_ParentPackageJob = Shipment.GetParentPackageJob().PK;

			pkg.KP_F3_NKPackType = JL_F3_NKPackType;
			pkg.KP_DimensionUQ = JL_UnitOfDimension;
			pkg.KP_WeightUQ = JL_ActualWeightUQ;
			pkg.KP_VolumeUQ = JL_ActualVolumeUQ;

			pkg.KP_RequiresTemperatureControl = JL_RequiresTemperatureControl;
			pkg.KP_RequiredTemperatureMaximum = JL_RequiredTemperatureMaximum;
			pkg.KP_RequiredTemperatureMinimum = JL_RequiredTemperatureMinimum;
			pkg.KP_RequiredTemperatureUnit = JL_RequiredTemperatureUnit;

			pkg.KP_MarksAndNumbers = JL_MarksAndNumbers;
			pkg.KP_GoodsDescription = JL_Description;
			pkg.KP_HSCode = JL_HarmonisedCode.Substring(0, AutoPkgPackage.Schema.KP_HSCodeMaxLength);
			pkg.KP_RH_NKCommodityCode = JL_RH_NKCommodityCode;

			if (UNDGs.Any())
			{
				foreach (var undg in UNDGs)
				{
					pkg.UNDGs.AddNew().CopyPersistentValuesFrom(undg, new BusinessObjectCloneArgs(new string[] { UNDGDataItemSchema.DI_ParentID.Name, UNDGDataItemSchema.DI_ParentTableCode.Name }));
				}
			}

			pkg.KP_Height = JL_Height;
			pkg.KP_Width = JL_Width;
			pkg.KP_Length = JL_Length;
			if (JL_PackageCount == ZInt.Zero)
			{
				pkg.KP_Weight = ZDecimal.Zero;
				pkg.KP_Volume = ZDecimal.Zero;
			}
			else
			{
				pkg.KP_Weight = Math.Round(JL_ActualWeight / JL_PackageCount, 3, MidpointRounding.AwayFromZero);
				pkg.KP_Volume = Math.Round(JL_ActualVolume / JL_PackageCount, 3, MidpointRounding.AwayFromZero);
			}

			((ISupportPackageIDGeneration)pkg).ShouldGenerateIDOnSaving = true;

			return pkg;
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList LooseCargoContainerType_List
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var containers = QuotedBooking?.QuotedBookingContainers?.ToArray<ForwardingContainer>();

				if (containers?.Any() ?? false)
				{
					foreach (var container in containers)
					{
						if (container.RefContainer != null && !result.ContainsCode(container.RefContainer.RC_Code))
						{
							result.Add(new CodeDescriptionPair(container.RefContainer.RC_Code.ToString(), container.RefContainer.RC_DescriptionMultilingual.ToString()));
						}
					}
				}

				return result;
			}
		}

		public JobSupplierBookingLineCollection SupplierBookingLine_List
		{
			get
			{
				if (this.JL_JSL_BookingLine_ReadOnly)
				{
					return new JobSupplierBookingLineCollection(Factory);
				}

				var subQuery = new ZDBOnlySubQuery(typeof(JobSupplierBooking),JobSupplierBookingSchema.PK);
				subQuery.AddToFilter(JobSupplierBookingSchema.JSB_LoadMode, this.JobSupplierBookingLine?.SupplierBooking?.JSB_LoadMode ?? ZString.Empty);
				subQuery.AddToFilter(JobSupplierBookingSchema.JSB_Status,
					SQLComparisonOperator.NotEqual,
					new string[] { Constants.SupplierBookingStatus.Cancelled, Constants.SupplierBookingStatus.Incomplete, Constants.SupplierBookingStatus.Rejected, Constants.SupplierBookingStatus.Placed });
				var query = new ZDBOnlyQuery(typeof(JobSupplierBookingLine));
				query.AddSubQuery(JobSupplierBookingLineSchema.JSL_JSB_Booking, subQuery, JoinCondition.And);
				var collection = new JobSupplierBookingLineCollection(Factory, query);

				return collection;
			}
		}
		#endregion

		#region Clone

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobPackLinesSchema.Constants.JL_JSL_BookingLine,
			};
		}

		#endregion
	}
}
