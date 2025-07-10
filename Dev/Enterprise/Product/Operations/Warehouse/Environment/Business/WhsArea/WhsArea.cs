using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Environment.Business
{
	[DependentBusinessObject(typeof(WhsWarehouse), "Areas")]
	[CodeProperty(AutoWhsArea.Schema.WA_Name), DescriptionProperty(WhsArea.Schema.WA_Name)]
	public sealed class WhsArea : AutoWhsArea, Integration.IWhsArea, ICanDelete, IDocumentSupportable, IAffectLocationView
	{
		public WhsArea(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoWhsArea.Schema
		{
			public const string WA_CalcMaxVolume = "WA_CalcMaxVolume";
			public const string WA_CalcMaxWeight = "WA_CalcMaxWeight";
			public const string WA_CalcWeightUnit = "WA_CalcWeightUnit";
			public const string WA_CalcVolumeUnit = "WA_CalcVolumeUnit";
			public const string WA_CalcCurrentWeight = "WA_CalcCurrentWeight";
			public const string WA_CalcCurrentVolume = "WA_CalcCurrentVolume";
			public const string WA_CalcAvailableWeight = "WA_CalcAvailableWeight";
			public const string WA_CalcAvailableVolume = "WA_CalcAvailableVolume";
			public const string RFPickPackPrinterPK = "RFPickPackPrinterPK";
			public const string WA_CalcTransitClientDescription = "WA_CalcTransitClientDescription";
		}

		#endregion

		#region Public Const

		public const string naString = "N/A";

		#endregion

		#region Business Object Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WA_AreaType = CodeLists.AreaTypes.Codes.FreeStore;
		}

		public override void Delete()
		{
			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, this);
			if (defaultPrinter != null)
			{
				defaultPrinter.Delete(); // Tested in StmDefaultPrinterTest
			}

			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			UnsetOtherDefaultAreaIfSet(a => a.WA_IsDefaultPickAreaInfo);
			UnsetOtherDefaultAreaIfSet(a => a.WA_IsDefaultPutawayAreaInfo);
			LocationViewReloaderService.AddLocationReloaderService(Factory);
		}

		void UnsetOtherDefaultAreaIfSet(Func<WhsArea, ZPropertyInfo> getInfo)
		{
			var info = getInfo(this);
			if ((ZBool)info.Value && (info.HasChanges || !IsInDatabase))
			{
				foreach (var area in Warehouse.Areas.Where(a => a != this))
				{
					getInfo(area).Value = ZBool.False;
				}
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsAreaFetchStrategy(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var areaName = WA_NameMultilingual;
				return areaName.IsEmpty ?
					Res.GetString("8DBFE793-F8EF-4FB0-81FA-3B1254A407E8", "Area") :
					Res.GetString("943C973D-97D7-406C-9CD5-7FB6B0E537B1", "Area {0}", areaName);
			}
		}

		public override ZBool WA_IsPickingArea
		{
			get { return base.WA_IsPickingArea; }
			set
			{
				base.WA_IsPickingArea = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWA_IsPutawayArea();
				}
			}
		}

		public override ZBool WA_IsPutawayArea
		{
			get { return base.WA_IsPutawayArea; }
			set
			{
				base.WA_IsPutawayArea = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWA_IsPickingArea();
				}
			}
		}

		#endregion

		#region Related Entities

		#region Warehouse

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public WhsWarehouse Warehouse
		{
			get { return (WhsWarehouse)Factory.Load(typeof(WhsWarehouse), WA_WW_Whs); }
		}

		#endregion

		#region PickLocations

		public WhsLocationAreaCollection PickLocations => pickLocations ?? (pickLocations = new WhsLocationAreaCollection(this, Factory, WhsLocationViewSchema.WLV_WA_PickingArea));
		WhsLocationAreaCollection pickLocations;

		#endregion

		#region PutawayLocations

		public WhsLocationAreaCollection PutawayLocations => putawayLocations ?? (putawayLocations = new WhsLocationAreaCollection(this, Factory, WhsLocationViewSchema.WLV_WA_PutawayArea));
		WhsLocationAreaCollection putawayLocations;

		#endregion

		#endregion

		#region Properties

		#region RFPickPackPrinterPK

		[List("Lookups.Printers")]
		[ResourceStringData("WhsArea|RFPickPackPrinterPK", Caption = "RF Pick Pack Printer", ShortCaption = "Pick Pack Printer")]
		public ZGuid RFPickPackPrinterPK
		{
			get
			{
				var pickPackPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, this);
				return pickPackPrinter != null ? pickPackPrinter.SDP_SQ_Printer : ZGuid.Empty;
			}
			set
			{
				StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, this, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRFPickPackPrinterPK();
				}

				RFPickPackPrinterPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RFPickPackPrinterPKInfo
		{
			get { return GetZPropertyInfo(Schema.RFPickPackPrinterPK); }
		}

		#endregion

		#region WA_WW_Whs

		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		[ReadOnlyMember(nameof(IsWarehouseReadOnly))]
		public override ZGuid WA_WW_Whs
		{
			get { return base.WA_WW_Whs; }
			set { base.WA_WW_Whs = value; }
		}

		bool IsWarehouseReadOnly => IsInDatabase;

		#endregion

		#region WA_AreaType

		[List(nameof(Lookups) + "." + nameof(WhsAreaLookups.AreaTypes))]
		public override ZString WA_AreaType
		{
			get { return base.WA_AreaType; }
			set
			{
				var previousValue = WA_AreaType;
				base.WA_AreaType = value;

				if (previousValue != WA_AreaType)
				{
					RefreshAreaTypeTextOnWarehouse();
				}
			}
		}

		void RefreshAreaTypeTextOnWarehouse()
		{
			var warehouse = Warehouse;
			if (warehouse != null)
			{
				warehouse.FreeStoreTextInfo.RefreshBinding();
				warehouse.BondedTextInfo.RefreshBinding();
				warehouse.ExciseTextInfo.RefreshBinding();
				warehouse.InwardProcessingTextInfo.RefreshBinding();
				warehouse.VATFiscalTextInfo.RefreshBinding();
			}
		}

		#endregion

		#region WA_OH_TransitClient

		[RelatedBusinessObject("TransitClient")]
		[List("Lookups.TransitClients")]
		public override ZGuid WA_OH_TransitClient
		{
			get { return base.WA_OH_TransitClient; }
			set
			{
				if (base.WA_OH_TransitClient != value)
				{
					base.WA_OH_TransitClient = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateWA_OH_TransitClient();
					}
				}
			}
		}

		#endregion

		#region WA_Name

		[BusinessObjectTestExclude]
		[TranslatableDataField(Schema.TableName, Schema.WA_Name, MaxLength = Schema.WA_NameMaxLength, Type = typeof(WhsArea), Asmid = ResString.AssemblyId)]
		public override ZString WA_Name
		{
			get { return base.WA_Name; }
			set { base.WA_Name = value; }
		}

		public MultilingualString WA_NameMultilingual
		{
			get { return GetMultilingual(WA_NameInfo); }
		}

		#endregion

		#region Calculated Volumes

		[List("Lookups.VolumeUnitTypes")]
		public ZString WA_CalcVolumeUnit
		{
			get { return Env.Registry.PackageVolumeUnit; }
		}

		public ZDecimal WA_CalcCurrentVolume
		{
			get { return WeightAndVolumeHelper.CalculatedVolume; }
		}

		public ZDecimal WA_CalcAvailableVolume
		{
			get { return WA_CalcMaxVolume - WA_CalcCurrentVolume; }
		}

		public ZDecimal WA_CalcMaxVolume
		{
			get { return WeightAndVolumeHelper.CalculateMaxVolume(); }
		}

		public ZPropertyInfo WA_CalcVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcVolumeUnit); }
		}

		public ZPropertyInfo WA_CalcCurrentVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcCurrentVolume); }
		}

		public ZPropertyInfo WA_CalcAvailableVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcAvailableVolume); }
		}

		public ZPropertyInfo WA_CalcMaxVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcMaxVolume); }
		}

		#endregion

		#region Calculated Weights

		[List("Lookups.WeightUnitTypes")]
		public ZString WA_CalcWeightUnit
		{
			get { return Env.Registry.PackageWeightUnit; }
		}

		public ZDecimal WA_CalcCurrentWeight
		{
			get { return WeightAndVolumeHelper.CalculatedWeight; }
		}

		public ZDecimal WA_CalcAvailableWeight
		{
			get { return WA_CalcMaxWeight - WA_CalcCurrentWeight; }
		}

		public ZDecimal WA_CalcMaxWeight
		{
			get { return WeightAndVolumeHelper.CalculateMaxWeight(); }
		}

		public ZPropertyInfo WA_CalcWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcWeightUnit); }
		}

		public ZPropertyInfo WA_CalcCurrentWeightInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcCurrentWeight); }
		}

		public ZPropertyInfo WA_CalcAvailableWeightInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcAvailableWeight); }
		}

		public ZPropertyInfo WA_CalcMaxWeightInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcMaxWeight); }
		}

		#endregion

		public ZString WA_CalcTransitClientDescription
		{
			get
			{
				return Warehouse != null && Warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit
										? WA_OH_TransitClient.IsEmpty ? new ZString("") : TransitClient.OH_FullName
										: (ZString)naString;
			}
		}

		public ZPropertyInfo WA_CalcTransitClientDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.WA_CalcTransitClientDescription); }
		}

		#endregion

		#region WeightAndVolumeHelper

		internal WeightAndVolumeCalculation WeightAndVolumeHelper
		{
			get { return weightAndVolumeHelper ?? (weightAndVolumeHelper = new WeightAndVolumeCalculation(this)); }
		}

		WeightAndVolumeCalculation weightAndVolumeHelper;

		public void ResetCalculatedTotalsCache()
		{
			weightAndVolumeHelper = null;
		}

		#endregion

		#region class WeightAndVolumeCalculation

		internal class WeightAndVolumeCalculation
		{
			internal WeightAndVolumeCalculation(WhsArea area)
			{
				Area = area;
			}

			readonly WhsArea Area;

			#region Properties

			#region Max Weight and Volume

			#region CalculateMaxWeight

			internal ZDecimal CalculateMaxWeight()
			{
				WeightAndVolumeConverterForLocation.ResetWeight();

				foreach (WhsLocation location in Area.PickLocations)
				{
					WeightAndVolumeConverterForLocation.ConvertWeightValue(location, location.WLV_MaxWeight, location.WLV_MaxWeightUnit, Area.WA_CalcWeightUnit, applyRoundingToConvertedValue: false);
				}

				return WeightAndVolumeConverterForLocation.CalculatedWeight;
			}

			#endregion

			#region CalculateMaxVolume

			internal ZDecimal CalculateMaxVolume()
			{
				WeightAndVolumeConverterForLocation.ResetVolume();

				foreach (WhsLocation location in Area.PickLocations)
				{
					WeightAndVolumeConverterForLocation.ConvertVolumeValue(location, location.WLV_MaxCubic, location.WLV_MaxCubicUnit, Area.WA_CalcVolumeUnit, applyRoundingToConvertedValue: false);
				}

				return WeightAndVolumeConverterForLocation.CalculatedVolume;
			}

			#endregion

			#region WeightAndVolumeConverterForLocation

			LocationWeightAndVolumeConverter WeightAndVolumeConverterForLocation
			{
				get { return weightAndVolumeConverterForLocation ?? (weightAndVolumeConverterForLocation = new LocationWeightAndVolumeConverter()); }
			}

			LocationWeightAndVolumeConverter weightAndVolumeConverterForLocation;

			#endregion

			#region HasMaxWeightWarningMessage

			internal bool HasMaxWeightWarningMessage
			{
				get { return weightAndVolumeConverterForLocation != null && WeightAndVolumeConverterForLocation.HasWeightWarningMessage; }
			}

			#endregion

			#region HasMaxVolumeWarningMessage

			internal bool HasMaxVolumeWarningMessage
			{
				get { return weightAndVolumeConverterForLocation != null && WeightAndVolumeConverterForLocation.HasVolumeWarningMessage; }
			}

			#endregion

			#region MaxWeightWarningMessage

			internal string MaxWeightWarningMessage
			{
				get { return WeightAndVolumeConverterForLocation.WeightWarningMessage; }
			}

			#endregion

			#region MaxVolumeWarningMessage

			internal string MaxVolumeWarningMessage
			{
				get { return WeightAndVolumeConverterForLocation.VolumeWarningMessage; }
			}

			#endregion

			#endregion

			#region Current Weight and Volume

			#region CalculatedWeight

			internal ZDecimal CalculatedWeight
			{
				get
				{
					if (weightAndVolumeConverterForInventory == null)
					{
						CalculateVolumeAndWeightsForArea();
					}

					return WeightAndVolumeConverterForInventory.CalculatedWeight;
				}
			}

			#endregion

			#region CalculatedVolume

			internal ZDecimal CalculatedVolume
			{
				get
				{
					if (weightAndVolumeConverterForInventory == null)
					{
						CalculateVolumeAndWeightsForArea();
					}

					return WeightAndVolumeConverterForInventory.CalculatedVolume;
				}
			}

			#endregion

			#region WeightAndVolumeConverterForInventory

			InventoryWeightAndVolumeConverter WeightAndVolumeConverterForInventory
			{
				get { return weightAndVolumeConverterForInventory ?? (weightAndVolumeConverterForInventory = new InventoryWeightAndVolumeConverter()); }
			}

			InventoryWeightAndVolumeConverter weightAndVolumeConverterForInventory;

			#endregion

			#region HasWeightWarningMessage

			internal bool HasWeightWarningMessage
			{
				get { return weightAndVolumeConverterForInventory != null && WeightAndVolumeConverterForInventory.HasWeightWarningMessage; }
			}

			#endregion

			#region HasVolumeWarningMessage

			internal bool HasVolumeWarningMessage
			{
				get { return weightAndVolumeConverterForInventory != null && WeightAndVolumeConverterForInventory.HasVolumeWarningMessage; }
			}

			#endregion

			#region WeightWarningMessage

			internal string WeightWarningMessage
			{
				get { return WeightAndVolumeConverterForInventory.WeightWarningMessage; }
			}

			#endregion

			#region VolumeWarningMessage

			internal string VolumeWarningMessage
			{
				get { return WeightAndVolumeConverterForInventory.VolumeWarningMessage; }
			}

			#endregion

			#endregion

			#endregion

			#region CalculateVolumeAndWeightsForArea

			void CalculateVolumeAndWeightsForArea()
			{
				var result = new DynamicBusinessObjectCollection(Area.Factory);

				string sql = @"
					select op_partnum as ProductCode,
							op_weightuq as WeightUQ,
							op_cubicuq as VolumeUQ,
							sum(wi_totalunits * op_weight) as Weight,
							sum(wi_totalunits * op_cubic) as Volume
					from	dbo.whsarea 
					join	dbo.whslocation on wl_wa_pickingarea = wa_pk
					join	dbo.WhsInventoryView on wi_wl = wl_pk
					join	dbo.orgsupplierpart on op_pk = wi_op
					where	wa_pk = @areaPK
					group by op_weightuq,
							 op_cubicuq,
							 op_partnum";

				var viewParams = new ZSqlParameterCollection();
				viewParams.Add(ZSqlParameter.New("@areaPK", Area.PK, WhsAreaSchema.PK));
				result.Load(sql, viewParams);

				foreach (DynamicBusinessObject obj in result)
				{
					var weightUQ = (ZString)obj["WeightUQ"];
					var volumeUQ = (ZString)obj["VolumeUQ"];

					var weight = (ZDecimal)obj["Weight"];
					WeightAndVolumeConverterForInventory.ConvertWeightValue(obj, weight, weightUQ, Area.WA_CalcWeightUnit, applyRoundingToConvertedValue: true);

					var volume = (ZDecimal)obj["Volume"];
					WeightAndVolumeConverterForInventory.ConvertVolumeValue(obj, volume, volumeUQ, Area.WA_CalcVolumeUnit, applyRoundingToConvertedValue: true);
				}
			}

			#endregion

			#region class WeightAndVolumeConverter

			abstract class WeightAndVolumeConverter<T>
				where T : BusinessObject
			{
				internal WeightAndVolumeConverter()
				{
					WeightConverter = new WeightConverter<T>(this);
					VolumeConverter = new VolumeConverter<T>(this);
				}

				readonly WeightConverter<T> WeightConverter;
				readonly VolumeConverter<T> VolumeConverter;

				internal void ResetWeight()
				{
					WeightConverter.ResetValue();
				}

				internal void ResetVolume()
				{
					VolumeConverter.ResetValue();
				}

				internal void ConvertWeightValue(T relatedBizO, ZDecimal sourceValue, ZString sourceUnit, ZString targetUnit, bool applyRoundingToConvertedValue)
				{
					WeightConverter.ConvertValue(relatedBizO, sourceValue, sourceUnit, targetUnit, applyRoundingToConvertedValue);
				}

				internal void ConvertVolumeValue(T relatedBizO, ZDecimal sourceValue, ZString sourceUnit, ZString targetUnit, bool applyRoundingToConvertedValue)
				{
					VolumeConverter.ConvertValue(relatedBizO, sourceValue, sourceUnit, targetUnit, applyRoundingToConvertedValue);
				}

				internal ZDecimal CalculatedWeight
				{
					get { return WeightConverter.CalculatedValue; }
				}

				internal ZDecimal CalculatedVolume
				{
					get { return VolumeConverter.CalculatedValue; }
				}

				internal bool HasWeightWarningMessage
				{
					get { return WeightConverter.HasWarningMessage; }
				}

				internal string WeightWarningMessage
				{
					get { return WeightConverter.WarningMessage; }
				}

				internal bool HasVolumeWarningMessage
				{
					get { return VolumeConverter.HasWarningMessage; }
				}

				internal string VolumeWarningMessage
				{
					get { return VolumeConverter.WarningMessage; }
				}

				internal abstract string GetHumanReadableName(T relatedBizO);
			}

			#endregion

			#region class LocationWeightAndVolumeConverter

			class LocationWeightAndVolumeConverter : WeightAndVolumeConverter<WhsLocation>
			{
				internal override string GetHumanReadableName(WhsLocation location)
				{
					return Res.GetString("f607ad90-6a86-41d1-a699-04977c730329", "Location '{0}'", location.ToLocationString());
				}
			}

			#endregion

			#region class InventoryWeightAndVolumeConverter

			class InventoryWeightAndVolumeConverter : WeightAndVolumeConverter<DynamicBusinessObject>
			{
				internal override string GetHumanReadableName(DynamicBusinessObject relatedBizO)
				{
					return Res.GetString("5350bd59-fb73-47f2-8e2d-019246f5f35f", "Product '{0}'", relatedBizO["ProductCode"]);
				}
			}

			#endregion

			#region class ValueConverter

			abstract class ValueConverter<T>
				where T : BusinessObject
			{
				protected ValueConverter(WeightAndVolumeConverter<T> weightAndVolumeConverter)
				{
					WeightAndVolumeConverter = weightAndVolumeConverter;
				}

				readonly WeightAndVolumeConverter<T> WeightAndVolumeConverter;

				#region ConvertValue

				internal void ConvertValue(T relatedBizO, ZDecimal sourceValue, ZString sourceUnit, ZString targetUnit, bool applyRoundingToConvertedValue)
				{
					if (sourceValue > 0)
					{
						if (!ContainsCode(sourceUnit))
						{
							var humanReadableName = WeightAndVolumeConverter.GetHumanReadableName(relatedBizO);
							var message = sourceUnit.IsEmpty
								? Res.GetString("f75ba625-1579-4c6c-9ba4-af45f555e45b", "{0} has no {1} Unit.", humanReadableName, UnitName)
								: Res.GetString("cb675411-ebaf-4c43-912d-6a716f302d18", "{0} has an invalid {1} Unit '{2}'.", humanReadableName, UnitName, sourceUnit);

							WarningStringBuilder.Append("\t" + message);
						}
						else if (!HasWarningMessage)
						{
							CalculatedValue += applyRoundingToConvertedValue
								? Utilities.Round(Convert(sourceValue, sourceUnit, targetUnit), RoundingScale)
								: Convert(sourceValue, sourceUnit, targetUnit);
						}
					}

					if (HasWarningMessage)
					{
						CalculatedValue = 0m;
					}
				}

				internal ZDecimal CalculatedValue { get; private set; }

				#endregion

				#region ResetValue

				internal void ResetValue()
				{
					CalculatedValue = 0m;
					warningStringBuilder = null;
				}

				#endregion

				#region HasWarningMessage

				internal bool HasWarningMessage
				{
					get { return warningStringBuilder != null && !warningStringBuilder.IsEmpty; }
				}

				#endregion

				#region WarningMessage

				internal string WarningMessage
				{
					get { return WarningStringBuilder.ToStringWithNewLineBetweenAppends(); }
				}

				#endregion

				#region WarningStringBuilder

				ZStringBuilder WarningStringBuilder
				{
					get { return warningStringBuilder ?? (warningStringBuilder = new ZStringBuilder()); }
				}

				ZStringBuilder warningStringBuilder;

				#endregion

				protected delegate bool ContainsCodeDelegate(string sourceUnit);
				protected abstract ContainsCodeDelegate ContainsCode { get; }

				protected delegate decimal ConvertUnitsDelegate(decimal sourceValue, string sourceUnit, string targetUnit, bool applyDefaultRounding = true);
				protected abstract ConvertUnitsDelegate Convert { get; }

				protected abstract string UnitName { get; }
				protected abstract int RoundingScale { get; }
			}

			#endregion

			#region class WeightConverter

			sealed class WeightConverter<T> : ValueConverter<T>
				where T : BusinessObject
			{
				internal WeightConverter(WeightAndVolumeConverter<T> weightAndVolumeConverter)
					: base(weightAndVolumeConverter)
				{
				}

				protected override ContainsCodeDelegate ContainsCode
				{
					get { return Constants.Weight.ContainsCode; }
				}

				protected override ConvertUnitsDelegate Convert
				{
					get { return Constants.Weight.Convert; }
				}

				protected override string UnitName
				{
					get { return Res.GetString("ce39048a-c58a-4f5d-801d-d5220ef1bebd", "Weight"); }
				}

				protected override int RoundingScale
				{
					get { return 2; }
				}
			}

			#endregion

			#region class VolumeConverter

			sealed class VolumeConverter<T> : ValueConverter<T>
				where T : BusinessObject
			{
				internal VolumeConverter(WeightAndVolumeConverter<T> weightAndVolumeConverter)
					: base(weightAndVolumeConverter)
				{
				}

				protected override ContainsCodeDelegate ContainsCode
				{
					get { return Constants.Volume.ContainsCode; }
				}

				protected override ConvertUnitsDelegate Convert
				{
					get { return Constants.Volume.Convert; }
				}

				protected override string UnitName
				{
					get { return Res.GetString("cd5dce1d-1edf-45dd-a1fa-c021c18ff877", "Volume"); }
				}

				protected override int RoundingScale
				{
					get { return 4; }
				}
			}

			#endregion
		}

		#endregion

		// interfaces

		#region IAllowUserToDelete Members

		public override bool CanDelete
		{
			get { return CanDeleteCore; }
		}

		bool CanDeleteCore
		{
			get { return Warehouse != null && Warehouse.Areas.Count > 1; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return CanDelete ? null : ResString.GetMultilingualString("f0e0c13b-c685-41a2-8e03-1f06c05aa2e1", "A warehouse must have at least one area"); }
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new WhsAreaDocumentSupporter(this); }
		}

		#endregion

		#region IAffectLocationView

		SchemaColumn[] IAffectLocationView.GetColumnsThatAffectLocationView()
		{
			return new SchemaColumn[]
			{
				WhsAreaSchema.WA_AreaType,
			};
		}

		void IAffectLocationView.ReloadLocationsFromDB()
		{
			PickLocations.RefreshFromDb();
			PutawayLocations.RefreshFromDb();
		}

		ZGuid IAffectLocationView.ParentThatMayReloadMyLocations => WA_WW_Whs;

		#endregion
	}

	#region Document Supporter

	public class WhsAreaDocumentSupporter : DocumentSupporter
	{
		public WhsAreaDocumentSupporter(WhsArea area)
			: base(area)
		{
		}

		protected WhsArea Area
		{
			get { return (WhsArea)BusinessObject; }
		}

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsArea; }
		}

		#endregion

		#region GetSupportedDataContexts

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.WhsArea,
				Enterprise.Core.Constants.DataContext.GenericFreightJob
			};
		}

		#endregion

		#region CustomisationSecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.WhsConfigWarehouseCustomiseDocuments; }
		}

		#endregion

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Area);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.WhsArea, Area) };
		}

		#endregion

		#region ShowReasonForNotPrinting

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion
	}

	#endregion
}
