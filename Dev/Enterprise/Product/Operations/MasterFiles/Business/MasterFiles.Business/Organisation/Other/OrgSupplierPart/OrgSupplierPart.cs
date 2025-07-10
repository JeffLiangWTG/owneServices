using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	#region VolumeCalculator

	public class VolumeCalculator
	{
		public VolumeCalculator(ZPropertyInfo<ZDecimal> length, ZPropertyInfo<ZDecimal> width, ZPropertyInfo<ZDecimal> height, ZPropertyInfo<ZString> dimensionUQ, ZPropertyInfo<ZString> volumeUQ)
		{
			Argument.NotNull(length, "length");
			Argument.NotNull(width, "width");
			Argument.NotNull(height, "height");
			Argument.NotNull(dimensionUQ, "dimensionUQ");
			Argument.NotNull(volumeUQ, "volumeUQ");

			Length = length;
			Width = width;
			Height = height;
			DimensionUQ = dimensionUQ;
			VolumeUQ = volumeUQ;
		}

		readonly ZPropertyInfo<ZDecimal> Length;
		readonly ZPropertyInfo<ZDecimal> Width;
		readonly ZPropertyInfo<ZDecimal> Height;
		readonly ZPropertyInfo<ZString> DimensionUQ;
		readonly ZPropertyInfo<ZString> VolumeUQ;

		public ZDecimal Calculate()
		{
			return Calculate(Length.Value, Width.Value, Height.Value, DimensionUQ.Value, VolumeUQ.Value);
		}

		public static ZDecimal Calculate(ZDecimal length, ZDecimal width, ZDecimal height, ZString dimensionUQ, ZString volumeUQ)
		{
			var dimensionUnit = Constants.Length.Metres;
			var cubicUnit = Constants.Volume.CubicMetres;

			if (dimensionUQ == Constants.Length.Feet && volumeUQ == Constants.Volume.CubicFeet ||
				dimensionUQ == Constants.Length.Inches && volumeUQ == Constants.Volume.CubicInches ||
				dimensionUQ == Constants.Length.Yards && volumeUQ == Constants.Volume.CubicYards)
			{
				dimensionUnit = dimensionUQ;
				cubicUnit = volumeUQ;
			}

			var factor = Constants.Length.Convert(1m, dimensionUQ, dimensionUnit);
			var volume = length * factor * height * factor * width * factor;

			var result = Constants.Volume.Convert(volume, cubicUnit, volumeUQ);
			result = decimal.Round(result, OrgSupplierPartSchema.OP_Cubic.Scale);

			return result;
		}
	}

	#endregion

	[RestrictedFilteredItem]
	[CodeProperty(AutoOrgSupplierPart.Schema.OP_PartNum), DescriptionProperty(AutoOrgSupplierPart.Schema.OP_Desc)]
	[UserDefinedValues]
	[UniversalDataContext(UniversalDataBuss.Integration.DataContextType.Product)]
	[DeferTriggerAndRunBeforeCommit("TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQ", "UpdateOrgPartRelationUnitsPerClientUQByOrgSupplierPart", OrgSupplierPartSchema.Constants.PK, typeof(IUpdateOrgPartRelationStrategy_OrgSupplierPartUpdate))]
	public abstract class OrgSupplierPart : AutoOrgSupplierPart,
		IOrgSupplierPart,
		ICustomFieldProvider,
		ICustomLabelsConfigOrgProvider,
		ISupportDataImporting,
		IDocManagerSupport,
		ILandedCostHistoryMaster,
		IPartProvider,
		ITemplateCopyable,
		IUNDGDataItemProvider,
		IUnitConverterDataProvider,
		IWorkflowProvider,
		IJobNumberForWorkflow,
		IJobNumber,
		ISetterSuspenderSupporter,
		IHaveRequiredDocumentsWithAttributes,
		IAuditParent
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string OwnerAndBarcodeOfProductAreUnique = "The products with a same owner/code are not allowed to have the same barcode.";

		#region Construction/Type Decider

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OP_RH_NKCommodityCodeInfo.ValueChanged += delegate
			{
				if (CommodityCode != null)
				{
					CommodityCode.RH_FN_NKNMFCInfo.ValueChanged -= TriggerHasChange;
					CommodityCode.RH_FN_NKNMFCInfo.ValueChanged += TriggerHasChange;
				}
			};
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(OP_UnitsPerPallet), ConcurrencyPolicy.Ignore);
		}

		void TriggerHasChange(object sender, EventArgs e)
		{
			HasChanges = true;
		}

		public static OrgSupplierPart New(BusinessObjectFactory factory)
		{
			return factory.New<OrgSupplierPart>();
		}

		public static readonly TypeDecider TypeDecider = new OrgSupplierPartTypeDecider();

		#endregion

		protected override bool EnableLightValidationIfAvailable
		{
			get
			{
				var result = base.EnableLightValidationIfAvailable;
				if (OP_IsComponentPickedOnSalesOrderInfo.HasChanges)
				{
					result = false;
				}
				return result;
			}
		}

		#region Schema

		public abstract new class Schema : AutoOrgSupplierPart.Schema
		{
			public const string AllOwners = "AllOwners";
			public const string AllSuppliers = "AllSuppliers";
			public const string AllLocalParts = "AllLocalParts";
			public const string AllLocalPartDescriptions = "AllLocalPartDescriptions";
			public const string AllProductCategories = "AllProductCategories";

			public const string OP_Calc_DGClass = "OP_Calc_DGClass";
			public const string OP_Calc_DGPG = "OP_Calc_DGPG";
			public const string OP_Calc_DGState = "OP_Calc_DGState";
			public const string OP_Calc_DGSubLabel1 = "OP_Calc_DGSubLabel1";
			public const string OP_Calc_DGSubLabel2 = "OP_Calc_DGSubLabel2";
			public const string OP_Calc_DGPackIns = "OP_Calc_DGPackIns";
			public const string OP_Calc_DGPackProv = "OP_Calc_DGPackProv";
			public const string OP_Calc_DGMP = "OP_Calc_DGMP";
			public const string OP_Calc_DGEMS = "OP_Calc_DGEMS";
			public const string OP_Calc_DGExpLim = "OP_Calc_DGExpLim";
			public const string OP_Calc_DGFP = "OP_Calc_DGFP";
			public const string OP_Calc_DGCodedStow = "OP_Calc_DGCodedStow";

			public const string IsBOMProduct = "IsBOMProduct";
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			// for stand-alone part or part module screen
			public Loader(BusinessObjectFactory factory, bool enableExactMatch = true)
				: this(factory, typeof(OrgSupplierPart), enableExactMatch)
			{
			}

			// for declaration
			public Loader(BusinessObjectFactory factory, Type typeofPart, bool enableExactMatch = true)
				: base(factory)
			{
				this.typeofPart = typeofPart;
				this.enableExactMatch = enableExactMatch;
			}

			protected readonly Type typeofPart;
			protected readonly bool enableExactMatch;

			public OrgSupplierPart Load(ZString partCode, ZGuid buyerPK, ZGuid supplierPK, bool throwIfAmbiguousMatchDetected = false, bool allowInactive = false, bool isExportJob = false)
			{
				OrgHeader buyer = Factory.Load<OrgHeader>(buyerPK);
				OrgHeader supplier = Factory.Load<OrgHeader>(supplierPK);
				return Load(partCode, buyer, supplier, throwIfAmbiguousMatchDetected, allowInactive, isExportJob: isExportJob);
			}

			public OrgSupplierPart Load(ZString partCode, OrgHeader buyer, OrgHeader supplier, bool throwIfAmbiguousMatchDetected = false, bool allowInactive = false, bool isExportJob = false)
			{
				ProductLoadResult result = LoadAndReturnMatchingCount(partCode, buyer, supplier, false, throwIfAmbiguousMatchDetected, allowInactive, isExportJob: isExportJob);
				return result.BestMatchingProduct;
			}

			public ProductLoadResult LoadAndReturnMatchingCount(ZString partCode, OrgHeader buyer, OrgHeader supplier, bool lookForLocalCode, bool throwIfAmbiguousMatchDetected = false, bool allowInactive = false, bool isExportJob = false)
			{
				TotalMatchCountWithMaximumScore = 0;
				TotalNumberOfPartsTested = 0;
				closestMatch = null;

				if (!partCode.IsEmpty)
				{
					BuyerPK = buyer != null ? buyer.PK : ZGuid.Empty;
					SupplierPK = supplier != null ? supplier.PK : ZGuid.Empty;
					maximumMatchScore = 0;
					if (lookForLocalCode)
					{
						var relationsFilter = new ZQuery();
						if (!BuyerPK.IsEmpty)
						{
							var buyerFilter = new ZQuery(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
							buyerFilter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, BuyerPK);
							relationsFilter.AddToFilter(buyerFilter);
							var buyerFilter2 = new ZQuery(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
							buyerFilter2.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, BuyerPK);
							relationsFilter.AddToFilter(buyerFilter2, JoinCondition.Or);
						}

						if (!SupplierPK.IsEmpty)
						{
							var supplierFilter = new ZQuery(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Supplier);
							supplierFilter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, SupplierPK);
							relationsFilter.AddToFilter(supplierFilter, JoinCondition.Or);
							var supplierFilter2 = new ZQuery(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
							supplierFilter2.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, SupplierPK);
							relationsFilter.AddToFilter(supplierFilter2, JoinCondition.Or);
						}

						var partRelationsFilter = new ZQuery(OrgPartRelationSchema.OU_LocalPartNumber, partCode);
						partRelationsFilter.AddToFilter(relationsFilter);
						var relationships = Factory.Load<OrgPartRelation>(partRelationsFilter).ToList();
						if (allowInactive)
						{
							relationships = relationships.OrderBy(x =>
							{
								var supplierPart = x.SupplierPart;
								return supplierPart == null
									? 1
									: supplierPart.OP_IsActive
										? -1
										: 0;
							}).ToList();
						}
						foreach (var relationship in relationships)
						{
							var supplierPart = relationship.SupplierPart;
							if (supplierPart != null)
							{
								ProcessOnePart(supplierPart, isExportJob, false, allowInactive);
							}
						}
					}

					var partQuery = new ZQuery(OrgSupplierPartSchema.OP_PartNum, partCode);
					partQuery.OrderBy = OrgSupplierPartSchema.OP_SystemCreateTimeUtc.Name;
					var loadedParts = Factory.Load(GetTypeOfBusinessObjectToLoad(), partQuery);
					if (loadedParts.Length == 0)
					{
						loadedParts = GetPartsByBarcode(partCode);
					}
					var parts = loadedParts.Cast<OrgSupplierPart>().ToList();
					if (allowInactive)
					{
						parts = parts.OrderBy(x => x.OP_IsActive ? 0 : 1).ToList();
					}
					foreach (var part in parts)
					{
						ProcessOnePart(part, isExportJob, throwIfAmbiguousMatchDetected, allowInactive);
					}
				}

				return new ProductLoadResult(closestMatch, TotalMatchCountWithMaximumScore, TotalNumberOfPartsTested);
			}

			// Loaded in two phases so that barcodes created in memory are handled.
			BusinessObject[] GetPartsByBarcode(ZString barcode)
			{
				var loadedParts = Array.Empty<BusinessObject>();
				var barcodeQuery = new ZQuery(OrgSupplierPartBarcodeSchema.PH_Barcode, barcode);
				var barcodes = Factory.Load<OrgSupplierPartBarcode>(barcodeQuery);
				if (barcodes.Length > 0)
				{
					var partByPKQuery = new ZQuery(OrgSupplierPartSchema.PK, barcodes.Select(b => b.PH_OP));
					partByPKQuery.OrderBy = OrgSupplierPartSchema.OP_SystemCreateTimeUtc.Name;
					loadedParts = Factory.Load(GetTypeOfBusinessObjectToLoad(), partByPKQuery);
				}

				return loadedParts;
			}

			void ProcessOnePart(OrgSupplierPart part, bool isExportJob, bool throwIfAmbiguousMatchDetected, bool allowInactive)
			{
				TotalNumberOfPartsTested += 1;
				int thisPartMatchScore = GetPartMatch(part, isExportJob, allowInactive);
				if (thisPartMatchScore > maximumMatchScore)
				{
					TotalMatchCountWithMaximumScore = 1;
					maximumMatchScore = thisPartMatchScore;
					closestMatch = part;
				}
				else if (thisPartMatchScore > 0 && thisPartMatchScore == maximumMatchScore)
				{
					TotalMatchCountWithMaximumScore++;
					if (throwIfAmbiguousMatchDetected)
					{
						throw new InvalidOperationException(string.Format(DefaultCulture.Instance, "Ambiguous match on Product '{0}', processing aborted.", part.OP_PartNum));
					}
				}
			}

			int maximumMatchScore;
			int TotalMatchCountWithMaximumScore;
			int TotalNumberOfPartsTested;
			OrgSupplierPart closestMatch;
			ZGuid BuyerPK;
			ZGuid SupplierPK;

			int GetPartMatch(OrgSupplierPart part, bool isExportJob, bool allowInactive)
			{
				var result = 0;
				if (allowInactive || part.OP_IsActive)
				{
					var isExactMatchEnabled = enableExactMatch && (bool)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.Value;
					var matchScoreProvider = new ProductMatchScoreProvider(part, BuyerPK, SupplierPK, isExportJob, isExactMatchEnabled);
					result = matchScoreProvider.GetMatchScore();
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeofPart;
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgSupplierPartFetchStrategy(this);
		}

		#endregion

		#region Base Business Object Overrides

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region SetDefaultValues
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OP_StockKeepingUnit = Env.Registry.DefaultStockUnit;
			OP_IsActive = true;
			OP_WeightUQ = Env.Registry.PackageWeightUnit;
			OP_CubicUQ = Env.Registry.PackageVolumeUnit;
			OP_CanDisassembleKit = true;
		}
		#endregion

		#region OnFactorySavingBeforeTransaction

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsLightSaving)
			{
				if (!IsImportedFromXML)
				{
					PartUnits.UpdateConversionToStockKeepingUnit(OP_WeightUQInfo, OP_Weight);
					PartUnits.UpdateConversionToStockKeepingUnit(OP_CubicUQInfo, OP_Cubic);
				}
			}
		}

		public bool IsImportedFromXML
		{
			get;
			set;
		}

		public bool IsLightSaving
		{
			get;
			set;
		}

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && IsLightSaving)
			{
				IsLightSaving = false;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			IsDeletingChildCollections = true;
			try
			{
				base.Delete();
				PartUnits.RemoveAndDeleteAll();
				Locations.RemoveAndDeleteAll();
				BillOfMaterials.DeleteAll();
				RelatedOrganisations.RemoveAndDeleteAll();
				PartBarcodes.RemoveAndDeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				SecondaryParts.DeleteAll(); // Tested in SaveAndDelete
			}
			finally
			{
				IsDeletingChildCollections = false;
			}
		}
		bool IsDeletingChildCollections;

		protected override void OnSaveRollback()
		{
			base.OnSaveRollback();
			PartUnits.Load();
			Locations.Load();
			RelatedOrganisations.Load();
			PartBarcodes.Load();
		}

		#endregion

		protected override ZString HumanReadableNameCore => string.Join(" ", Res.GetString("0e020725-7db7-44fc-8843-60df259d90a1", "Part"), OP_PartNum);

		#region BusinessObjectsWithRelatedEvents
		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList result = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(this.RelatedOrganisations);
				result.AddRange(this.Locations);
				return (BusinessObject[])result.ToArray(typeof(BusinessObject));
			}
		}
		#endregion

		public bool JustUpdatedByDataRefresh;

		#endregion

		#region Properties

		#region OP_ExtendedCommercialDescription
		public ZString OP_ExtendedCommercialDescription
		{
			get
			{
				StmNote extendedCommercialDescriptionNote = GetExtendedCommercialDescriptionNote();
				return extendedCommercialDescriptionNote != null ? extendedCommercialDescriptionNote.ST_NoteText : ZString.Empty;
			}
		}

		protected StmNote GetExtendedCommercialDescriptionNote()
		{
			StmNote[] extendedCommercialDescriptionNotes = Notes.FindByDescription(PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description);
			return extendedCommercialDescriptionNotes.Length > 0 ? extendedCommercialDescriptionNotes[0] : null;
		}
		#endregion

		public override ZString OP_Brand
		{
			get { return base.OP_Brand; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.OP_Brand))
				{
					base.OP_Brand = value;
				}
			}
		}

		public override ZString OP_Model
		{
			get { return base.OP_Model; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.OP_Model))
				{
					base.OP_Model = value;
				}
			}
		}

		#region OP_PartNum

		public override ZString OP_PartNum
		{
			get { return base.OP_PartNum; }
			set
			{
				base.OP_PartNum = value.ToUpper();
				RelatedOrganisations.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region OP_RX_NKLastWeightedCostCurr

		[List("Lookups.WeightedCostCurrencyList")]
		public override ZString OP_RX_NKLastWeightedCostCurr
		{
			get { return base.OP_RX_NKLastWeightedCostCurr; }
			set { base.OP_RX_NKLastWeightedCostCurr = value; }
		}

		#endregion

		#region OP_RH_NKCommodityCode
		[List("Lookups.CommodityCodes")]
		public override ZString OP_RH_NKCommodityCode
		{
			get
			{
				return base.OP_RH_NKCommodityCode;
			}
			set
			{
				base.OP_RH_NKCommodityCode = value;
			}
		}
		#endregion

		#region OP_MeasureUQ
		[List("Lookups.OP_MeasureUQ_List")]
		public override ZString OP_MeasureUQ
		{
			get { return base.OP_MeasureUQ; }
			set
			{
				using (PostponedCubicUpdate(OP_MeasureUQ != value))
				{
					base.OP_MeasureUQ = value;
				}
			}
		}
		#endregion

		#region OP_WeightUQ
		[List("Lookups.OP_WeightUQ_List")]
		public override ZString OP_WeightUQ
		{
			get { return base.OP_WeightUQ; }
			set { base.OP_WeightUQ = value; }
		}
		#endregion

		#region OP_Weight
		[MeasureUnit(Schema.OP_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal OP_Weight
		{
			get { return base.OP_Weight; }
			set { base.OP_Weight = value; }
		}

		#endregion

		#region OP_Depth
		[MeasureUnit(Schema.OP_MeasureUQ, MeasureUnitType.Length)]
		public override ZDecimal OP_Depth
		{
			get { return base.OP_Depth; }
			set
			{
				using (PostponedCubicUpdate(OP_Depth != value))
				{
					base.OP_Depth = value;
				}
			}
		}
		#endregion

		#region OP_Height
		[MeasureUnit(Schema.OP_MeasureUQ, MeasureUnitType.Length)]
		public override ZDecimal OP_Height
		{
			get { return base.OP_Height; }
			set
			{
				using (PostponedCubicUpdate(OP_Height != value))
				{
					base.OP_Height = value;
				}
			}
		}
		#endregion

		#region OP_Width
		[MeasureUnit(Schema.OP_MeasureUQ, MeasureUnitType.Length)]
		public override ZDecimal OP_Width
		{
			get { return base.OP_Width; }
			set
			{
				using (PostponedCubicUpdate(OP_Width != value))
				{
					base.OP_Width = value;
				}
			}
		}
		#endregion

		#region OP_CubicUQ
		[List("Lookups.OP_CubicUQ_List")]
		public override ZString OP_CubicUQ
		{
			get { return base.OP_CubicUQ; }
			set
			{
				using (PostponedCubicUpdate(OP_CubicUQ != value))
				{
					base.OP_CubicUQ = value;
				}
			}
		}
		#endregion

		#region OP_Cubic
		[MeasureUnit(Schema.OP_CubicUQ, MeasureUnitType.Volume)]
		public override ZDecimal OP_Cubic
		{
			get { return base.OP_Cubic; }
			set
			{
				base.OP_Cubic = value;
				ForbidCubicUpdate();
			}
		}

		#endregion

		#region OP_Calc_TotalLocationsStockTakeCount
		public ZDecimal OP_Calc_TotalLocationsStockTakeCount
		{
			get
			{
				ZDecimal result = 0.0m;
				foreach (OrgPartLocation location in Locations)
				{
					result += location.OR_StockTakeCount;
				}
				return result;
			}
		}
		#endregion

		#region OP_OH_FormLayoutController
		public ZGuid OP_OH_FormLayoutController
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				foreach (OrgPartRelation relation in RelatedOrganisations)
				{
					if (!relation.IsDeleted && relation.OU_FormLayoutController)
					{
						result = relation.OU_OH;
					}
				}
				return result;
			}
		}
		#endregion

		#region OP_OrderMultipleUnit
		[List("Lookups.OP_ProductUQ_List")]
		public override ZString OP_OrderMultipleUnit
		{
			get
			{
				return base.OP_OrderMultipleUnit;
			}
			set
			{
				base.OP_OrderMultipleUnit = value;
			}
		}
		#endregion

		#region OP_F3_NKPackType
		[List("Lookups.OP_ProductUQ_List")]
		public override ZString OP_F3_NKPackType
		{
			get
			{
				return base.OP_F3_NKPackType;
			}
			set
			{
				base.OP_F3_NKPackType = value;
			}
		}
		#endregion

		#region StockKeepingUnitProxy
		public ZString StockKeepingUnitProxy
		{
			get { return OP_StockKeepingUnit; }
		}

		public ZPropertyInfo StockKeepingUnitProxyInfo
		{
			get { return GetZPropertyInfo(nameof(StockKeepingUnitProxy)); }
		}
		#endregion

		#region OP_StockKeepingUnitPerPallet

		internal class StockUnitPairList : UntranslatableCodeDescriptionPairList
		{
			public StockUnitPairList()
				: this(new BusinessObjectFactory())
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
			public StockUnitPairList(BusinessObjectFactory factory)
				: base("Description values are stored directly in the database")
			{
				AddRange(new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits());
			}
		}

		[ActionField(CollectionType = typeof(StockUnitPairList))]
		[List("Lookups.OP_ProductUQ_List")]
		public override ZString OP_StockKeepingUnit
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.OP_StockKeepingUnit; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.OP_StockKeepingUnit = value;
				this.PartUnits.MarkAsNeedingValidation();
			}
		}

		public ZDecimal OP_StockKeepingUnitPerPallet
		{
			get
			{
				ZDecimal result = this.UnitConverter.Convert(1, "PLT", OP_StockKeepingUnit);

				if (!ValidatingOP_StockKeepingUnitPerPallet)
				{
					try
					{
						ValidatingOP_StockKeepingUnitPerPallet = true;
						Validation.ValidateOP_StockKeepingUnitPerPallet();
					}
					finally
					{
						ValidatingOP_StockKeepingUnitPerPallet = false;
					}
				}
				return result;
			}
		}
		bool ValidatingOP_StockKeepingUnitPerPallet;

		public ZPropertyInfo OP_StockKeepingUnitPerPalletInfo
		{
			get { return GetZPropertyInfo(nameof(OP_StockKeepingUnitPerPallet)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Measurement Units")]
		public ZString OP_StockKeepingUnitForPalletProxy
		{
			get
			{
				ZString proxy = Res.GetString("47181f95-6a52-4964-ac34-c81f490d9da9", "Units");
				if (!OP_StockKeepingUnit.IsEmpty)
				{
					proxy = Lookups.OP_ProductUQ_List.GetDescriptionFromCode(OP_StockKeepingUnit);
				}
				proxy = Grammar.Instance.Pluralize(proxy);
				return proxy == "s" ? "" : "(" + proxy + ")";
			}
		}

		public ZPropertyInfo OP_StockKeepingUnitForPalletProxyInfo
		{
			get { return GetZPropertyInfo(nameof(OP_StockKeepingUnitForPalletProxy)); }
		}

		#endregion

		#region Local Part Code / Desc

		public ZString GetLocalPartNum(OrgHeader org, string relationshipType)
		{
			ZString result = OP_PartNum;

			OrgPartRelation relation = RelatedOrganisations.FindByOrganisationAndRelationship(org, relationshipType);
			if (relation != null && !relation.OU_LocalPartNumber.IsEmpty)
			{
				result = relation.OU_LocalPartNumber;
			}

			return result;
		}

		public ZString GetLocalPartNumForWarehouseConsignee(OrgHeader consignee)
		{
			return GetLocalPartNum(consignee, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
		}

		public ZString GetLocalPartDesc(OrgHeader org, string relationshipType)
		{
			ZString result = OP_Desc;

			OrgPartRelation relation = RelatedOrganisations.FindByOrganisationAndRelationship(org, relationshipType);
			if (relation != null && !relation.OU_LocalPartDescription.IsEmpty)
			{
				result = relation.OU_LocalPartDescription;
			}

			return result;
		}

		public ZString GetLocalPartDescForWarehouseConsignee(OrgHeader consignee)
		{
			return GetLocalPartDesc(consignee, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
		}

		#endregion

		#region OP_IsActive

		[ActionField(ReadOnly = true)]
		public override ZBool OP_IsActive
		{
			get { return base.OP_IsActive; }
			set
			{
				base.OP_IsActive = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateOP_PartNum();
				}
			}
		}

		#endregion

		#region OP_IsBarcoded

		[ActionField(ReadOnly = true)]
		public override ZBool OP_IsBarcoded
		{
			get { return base.OP_IsBarcoded; }
			set
			{
				base.OP_IsBarcoded = value;

				foreach (OrgPartRelation relation in RelatedOrganisations)
				{
					relation.Validation.ValidateOU_RFAttributeConfirm();
				}

				foreach (OrgSupplierPartBarcode barcode in PartBarcodes)
				{
					barcode.Validation.ValidatePH_Barcode();
				}
				RelatedOrganisations.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region OP_IsComponentPickedOnSalesOrder

		public override ZBool OP_IsComponentPickedOnSalesOrder
		{
			get { return base.OP_IsComponentPickedOnSalesOrder; }
			set
			{
				base.OP_IsComponentPickedOnSalesOrder = value;
				BillOfMaterials.MarkAsNeedingValidation();
			}
		}

		#endregion

		public ZString DutyRateForCurrentCountry
		{
			get { return GetDutyRateForCurrentCountry(); }
		}

		protected virtual ZString GetDutyRateForCurrentCountry()
		{
			return "";
		}

		public bool IsClassifiedFor(ZString countryCode, ZString[] classificationTypes)
		{
			ZDBOnlyQuery classificationQuery = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusClassification>());
			classificationQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, countryCode);
			classificationQuery.AddToFilter(CusClassificationSchema.CC_ClassificationType, classificationTypes);

			ZDBOnlySubQuery pivotQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusClassPartPivot>(), CusClassPartPivotSchema.CI_CC);
			pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_OP, PK);
			classificationQuery.AddSubQuery(pivotQuery, JoinCondition.And);

			return Factory.Load(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseCusClassification>(), classificationQuery).Length > 0;
		}

		#region HasStockOnHandOrInTransit

		public bool HasStockOnHandOrInTransit
		{
			get
			{
				var query = new ZQuery(WhsInventoryViewSchema.WI_OP, PK);
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

				return Factory.LoadTop1<IWhsInventoryView>(query) != null;
			}
		}

		#endregion

		#region HasAsnLineOnUnfinalisedReceive

		public bool HasAsnLineOnUnfinalisedReceive
		{
			get
			{
				var subQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsAsnLineSchema.WN_WD);
				subQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, new[] { "FIN", "CAN" });
				subQuery.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");

				var query = new ZDBOnlyQuery(typeof(IWhsAsnLine));
				query.AddToFilter(WhsAsnLineSchema.WN_OP, PK);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return Factory.LoadTop1<IWhsAsnLine>(query) != null;
			}
		}

		#endregion

		#region HasActiveOperatorTransaction

		public bool HasActiveOperatorTransaction
		{
			get
			{
				var query = new ZQuery
				{
					MaximumRows = 1,
				};
				query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_Status, new ZString[] { "QUE", "VAL" });
				query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OP_Product, PK);

				return Factory.ExistsInDatabase(CusWHSOperatorTransactionSchema.Constants.TableName, query);
			}
		}

		#endregion

		#region Warehouse_Specific

		[ActionFieldFollow(typeof(IWhsProduct))]
		public IWhsProduct Warehouse_Specific
		{
			get
			{
				if (warehouse_Specific == null)
				{
					Type whsProductType = ObjectFactory.GetType<IWhsProduct>();
					var info = whsProductType.GetMethod("GetWhsProduct", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(OrgSupplierPart) }, Array.Empty<ParameterModifier>());
					warehouse_Specific = (IWhsProduct)info.Invoke(null, new object[] { this });
				}
				return warehouse_Specific;
			}
		}

		IWhsProduct warehouse_Specific;

		#endregion

		#endregion

		#region Related Business Objects

		#region ProductCategories

		public ZString AllProductCategories
		{
			get
			{
				if (allProductCategories == ZString.Empty)
				{
					var result = new ZStringBuilder();
					foreach (OrgPartRelation relation in RelatedOrganisations)
					{
						if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
								relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
						{
							if (relation.Category != null)
							{
								result.Append(string.Format("{0}({1}) ", relation.Category.OPC_CategoryCode, relation.Organisation.OH_Code));
							}
						}
					}
					allProductCategories = result.ToString().TrimEnd(' ');
				}
				return allProductCategories;
			}
		}

		ZString allProductCategories;

		public ZPropertyInfo AllProductCategoriesInfo
		{
			get { return GetZPropertyInfo(Schema.AllProductCategories); }
		}

		#endregion

		#region RelatedOrganisations

		[ActionFieldFollow(true)]
		[ChildEditable(true)]
		public OrgPartRelationCollection RelatedOrganisations
		{
			get
			{
				if (fRelatedOrganisations == null)
				{
					fRelatedOrganisations = new OrgPartRelationCollection(this, Factory);
					fRelatedOrganisations.Load();
					RegisterEditableChildObject(fRelatedOrganisations);
					fRelatedOrganisations.CountChanged += new CollectionCountChangedEventHandler(fRelatedOrganisations_CountChanged);
					((IBindingList)fRelatedOrganisations).ListChanged += new ListChangedEventHandler(OnRelatedOrganisations_ListChanged);
				}
				return fRelatedOrganisations;
			}
		}
		OrgPartRelationCollection fRelatedOrganisations;

		void fRelatedOrganisations_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsDeleted)
			{
				Validation.ValidateOP_PartNum();
			}
		}

		protected virtual void OnRelatedOrganisations_ListChanged(object sender, ListChangedEventArgs ev)
		{
			if (!IsDeletingChildCollections)
			{
				if (ev.ListChangedType == ListChangedType.ItemChanged || ev.ListChangedType == ListChangedType.ItemDeleted)
				{
					if (OP_OH_FormLayoutControllerChanged != null)
					{
						OP_OH_FormLayoutControllerChanged(this, EventArgs.Empty);
					}
				}
				Validation.ValidateOP_PartNum();
			}
		}
		public event EventHandler OP_OH_FormLayoutControllerChanged;

		public string GetProductDefaultHoldCode(ZGuid clientPK)
		{
			var defaultHoldCode = string.Empty;

			var orgPartRelation = RelatedOrganisations.FindByOrganisationPKAndRelationship(clientPK, OrgPartRelation.RelationshipTypes.Owner);
			if (orgPartRelation != null && orgPartRelation.OU_WHC_DefaultInventoryHoldCode != ZGuid.Empty)
			{
				defaultHoldCode = Factory.Load<IWhsInventoryHeldCode>(orgPartRelation.OU_WHC_DefaultInventoryHoldCode).WHC_Code;
			}

			return defaultHoldCode;
		}

		#region AllOwners

		public ZString AllOwners
		{
			get
			{
				if (fAllOwners == ZString.Empty)
				{
					ZStringBuilder result = new ZStringBuilder();
					foreach (OrgPartRelation relation in RelatedOrganisations)
					{
						if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ||
								relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
						{
							result.Append(relation.Organisation.OH_Code + " ");
						}
					}
					fAllOwners = result.ToString().TrimEnd(' ');
				}
				return fAllOwners;
			}
		}
		ZString fAllOwners;

		public ZPropertyInfo AllOwnersInfo
		{
			get { return GetZPropertyInfo(Schema.AllOwners); }
		}

		#endregion

		#region AllSuppliers

		public ZString AllSuppliers
		{
			get
			{
				if (fAllSuppliers == ZString.Empty)
				{
					ZStringBuilder result = new ZStringBuilder();
					foreach (OrgPartRelation relation in RelatedOrganisations)
					{
						if (relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Supplier ||
								relation.OU_Relationship == OrgPartRelation.RelationshipTypes.Both)
						{
							result.Append(relation.Organisation.OH_Code + " ");
						}
					}
					fAllSuppliers = result.ToString().TrimEnd(' ');
				}
				return fAllSuppliers;
			}
		}
		ZString fAllSuppliers;

		public ZPropertyInfo AllSuppliersInfo
		{
			get { return GetZPropertyInfo(Schema.AllSuppliers); }
		}

		#endregion

		#region AllLocalParts

		public ZString AllLocalParts
		{
			get
			{
				if (allLocalParts.IsEmpty)
				{
					ZStringBuilder result = new ZStringBuilder();
					foreach (OrgPartRelation relation in RelatedOrganisations)
					{
						if (!relation.OU_LocalPartNumber.IsEmpty)
						{
							result.Append(relation.OU_LocalPartNumber);
						}
					}
					allLocalParts = result.ToStringWithDelimiterBetweenAppends(", ");
				}
				return allLocalParts;
			}
		}
		ZString allLocalParts;

		public ZPropertyInfo AllLocalPartsInfo
		{
			get { return GetZPropertyInfo(Schema.AllLocalParts); }
		}

		#endregion

		#region AllLocalPartDescriptions

		public ZString AllLocalPartDescriptions
		{
			get
			{
				if (allLocalPartDescriptions.IsEmpty)
				{
					ZStringBuilder result = new ZStringBuilder();
					foreach (OrgPartRelation relation in RelatedOrganisations)
					{
						if (!relation.OU_LocalPartDescription.IsEmpty)
						{
							result.Append(relation.OU_LocalPartDescription);
						}
					}
					allLocalPartDescriptions = result.ToStringWithDelimiterBetweenAppends(", ");
				}
				return allLocalPartDescriptions;
			}
		}
		ZString allLocalPartDescriptions;

		public ZPropertyInfo AllLocalPartDescriptionsInfo
		{
			get { return GetZPropertyInfo(Schema.AllLocalPartDescriptions); }
		}

		#endregion

		public bool RemoveNonEssentialValidationForBulkTariffUpdate
		{
			get { return fRemoveNonEssentialValidationForBulkTariffUpdate; }
			set { fRemoveNonEssentialValidationForBulkTariffUpdate = value; }
		}
		bool fRemoveNonEssentialValidationForBulkTariffUpdate;

		#endregion

		#region Locations

		[ChildEditable(true)]
		public OrgPartLocationCollection Locations
		{
			get
			{
				if (fLocations == null)
				{
					fLocations = new OrgPartLocationCollection(this, Factory);
					fLocations.Load();
					RegisterEditableChildObject(fLocations);
				}
				return fLocations;
			}
		}
		OrgPartLocationCollection fLocations;

		#endregion

		#region ShouldPreventReceiveOfPartWithNoWeightOrDims

		public bool ShouldPreventReceiveOfPartWithNoWeightOrDims(OrgHeader client)
		{
			switch (client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive)
			{
				case ProductReceiveWeightOrDimsCheckTypeList.Codes.DoNotCheckWeightOrDims:
					return false;
				case ProductReceiveWeightOrDimsCheckTypeList.Codes.StockKeepingUnitOnly:
					return CheckStockKeepingUnitNonComplianceWeightOrDims();
				case ProductReceiveWeightOrDimsCheckTypeList.Codes.ConversionsAndStockKeepingUnit:
					return CheckStockKeepingUnitNonComplianceWeightOrDims() || CheckConversionsUnitNonComplianceWeightOrDims();
				default:
					return false;
			}

			bool CheckStockKeepingUnitNonComplianceWeightOrDims()
			{
				return OP_Cubic <= 0m && OP_Weight <= 0m && OP_Depth <= 0m && OP_Width <= 0m && OP_Height <= 0m;
			}

			bool CheckConversionsUnitNonComplianceWeightOrDims()
			{
				var scannableConversions = PartUnits.Where(c => !c.OF_ParentPackType.EqualsIgnoringCase(OP_StockKeepingUnit));
				return scannableConversions.Any(pu => pu.OF_Depth <= 0 && pu.OF_Height <= 0 && pu.OF_Width <= 0 && pu.OF_Weight <= 0 && pu.OF_Cubic <= 0);
			}
		}

		#endregion

		#region PartUnits

		[ChildEditable(true)]
		public OrgSupplierPartBarcodeCollection PartBarcodes
		{
			get
			{
				if (fPartBarcodes == null)
				{
					fPartBarcodes = new OrgSupplierPartBarcodeCollection(this, Factory);
					fPartBarcodes.Load();
					RegisterEditableChildObject(fPartBarcodes);
				}

				return fPartBarcodes;
			}
		}

		OrgSupplierPartBarcodeCollection fPartBarcodes;

		#endregion

		#region PartUnits

		[ChildEditable(true)]
		public OrgPartUnitCollection PartUnits
		{
			get
			{
				if (fPartUnits == null)
				{
					fPartUnits = new OrgPartUnitCollection(this, Factory);
					fPartUnits.Load();
					RegisterEditableChildObject(fPartUnits);
				}

				return fPartUnits;
			}
		}
		OrgPartUnitCollection fPartUnits;

		#endregion

		#region OrgPartBOM

		[ChildEditable(true)]
		public OrgPartBOMCollection BillOfMaterials
		{
			get
			{
				if (fBillOfMaterials == null)
				{
					fBillOfMaterials = new OrgPartBOMCollection(this);
					RegisterEditableChildObject(fBillOfMaterials);
				}
				return fBillOfMaterials;
			}
		}
		OrgPartBOMCollection fBillOfMaterials;

		[ChildEditable(true)]
		public OrgPartBOMCollection BillOfMaterialsForVirtualWarehouse
		{
			get
			{
				if (fBillOfMaterialsForVirtualWarehouse == null)
				{
					fBillOfMaterialsForVirtualWarehouse =
						new OrgPartBOMCollection(
								this,
								new ZQuery(OrgPartBOMSchema.OE_ExcludeForVirtualWarehouse, false));
					RegisterEditableChildObject(fBillOfMaterialsForVirtualWarehouse);
				}
				return fBillOfMaterialsForVirtualWarehouse;
			}
		}
		OrgPartBOMCollection fBillOfMaterialsForVirtualWarehouse;

		public OrgPartBomViewCollection BillOfMaterialsView
		{
			get
			{
				if (fBillOfMaterialsView == null)
				{
					fBillOfMaterialsView = new OrgPartBomViewCollection(Factory);
				}
				return fBillOfMaterialsView;
			}
		}
		OrgPartBomViewCollection fBillOfMaterialsView;

		#endregion

		#region SecondaryParts

		[ChildEditable(true)]
		public OrgSecondaryPartBOMCollection SecondaryParts
		{
			get
			{
				if (secondaryParts == null)
				{
					secondaryParts = new OrgSecondaryPartBOMCollection(this);
					RegisterEditableChildObject(secondaryParts);
				}

				return secondaryParts;
			}
		}

		OrgSecondaryPartBOMCollection secondaryParts;

		#endregion

		#region IsBOMProduct

		public ZBool IsBOMProduct => BillOfMaterials.Count > 0;

		#endregion

		#region SmallestStockKeepingUnitSize

		public decimal SmallestStockKeepingUnitSize
		{
			get
			{
				return OP_CountDecimalPlaces < 1 || OP_CountDecimalPlaces > 9
					? 1m
					: (decimal)Math.Pow(0.1, OP_CountDecimalPlaces);
			}
		}

		#endregion

		#region
		[MaxLength(1)]
		public override ZByte OP_CountDecimalPlaces
		{
			get
			{
				return base.OP_CountDecimalPlaces;
			}

			set
			{
				base.OP_CountDecimalPlaces = value;
			}
		}
		#endregion

		#region UNDGs

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		#endregion

		#region Unit Converter

		public UnitConverter UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new UnitConverter(this)); }
		}
		UnitConverter fUnitConverter;

		OrgSupplierPart IUnitConverterDataProvider.Product
		{
			get { return this; }
		}

		ZString IUnitConverterDataProvider.CountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		ZGuid IUnitConverterDataProvider.SupplierFK
		{
			get { return ZGuid.Empty; }
		}

		BusinessObjectFactory IUnitConverterDataProvider.Factory
		{
			get { return Factory; }
		}

		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
		{
			return PartUnits.Cast<IUnitConverter>();
		}

		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions
		{
			get { return PartUnits.Count > 0; }
		}

		ZString IUnitConverterDataProvider.Type
		{
			get { return RPTypeList.Codes.AllAreas; }
		}

		#endregion

		#endregion

		#region Cubic Calculation Helper Methods

		#region PostponedCubicUpdate
		// Should be used to improve perfomance. For example:
		// using (PostponedCubicUpdate())
		// {
		//   OP_Depth = 1m;
		//   OP_Height = 1m;
		//   OP_Width = 1m;
		//   OP_MeasureUQ = Constants.Length.Metres;
		// }
		//
		// OP_Cubic will be recalculated only once, at the end of the code above.
		public IDisposable PostponedCubicUpdate()
		{
			return PostponedCubicUpdate(false);
		}

		IDisposable PostponedCubicUpdate(bool forceUpdate)
		{
			return new CubicUpdater(this, forceUpdate);
		}

		void ForbidCubicUpdate()
		{
			if (cubicUpdater != null)
			{
				cubicUpdater.ForbidCubicUpdate();
			}
		}

		CubicUpdater cubicUpdater;

		class CubicUpdater : IDisposable
		{
			#region fields
			readonly OrgSupplierPart target;
			bool updateIsForbidden;
			bool updateIsRequired;

			#endregion

			#region ctor
			public CubicUpdater(OrgSupplierPart target, bool forceUpdate)
			{
				this.target = target;
				if (target.cubicUpdater == null)
				{
					target.cubicUpdater = this;
					updateIsForbidden = target.OP_Cubic != 0 && target.CubicIsInvalid();
				}
				else
				{
					updateIsForbidden = true;
				}
				if (forceUpdate)
				{
					target.cubicUpdater.updateIsRequired = forceUpdate;
				}
			}

			#endregion

			#region ForbidCubicUpdate()
			public void ForbidCubicUpdate()
			{
				updateIsForbidden = true;
			}

			#endregion

			#region IDisposable Members
			public void Dispose()
			{
				if (!updateIsForbidden && updateIsRequired)
				{
					target.SetCubic();
				}
				if (this == target.cubicUpdater)
				{
					if (updateIsForbidden && !target.CubicIsInvalid())
					{
						target.Validation.ValidateOP_Cubic();
					}
					target.cubicUpdater = null;
				}
			}

			#endregion
		}

		#endregion

		#region CubicIsInvalid

		public bool CubicIsInvalid()
		{
			ZDecimal calculatedVolume;
			try
			{
				calculatedVolume = VolumeCalculator.Calculate();
			}
			catch (ArgumentException)
			{
				return true;
			}
			return (!CubicCalculationIsSuspended && OP_Cubic != calculatedVolume);
		}

		#endregion

		#region VolumeCalculator

		VolumeCalculator VolumeCalculator
		{
			get
			{
				return volumeCalculator ?? (volumeCalculator = new VolumeCalculator(
						(ZPropertyInfoDecimal)OP_DepthInfo,
						(ZPropertyInfoDecimal)OP_WidthInfo,
						(ZPropertyInfoDecimal)OP_HeightInfo,
						(ZPropertyInfoString)OP_MeasureUQInfo,
						(ZPropertyInfoString)OP_CubicUQInfo));
			}
		}

		VolumeCalculator volumeCalculator;

		#endregion

		#region SetCubic

		void SetCubic()
		{
			if (!CubicCalculationIsSuspended)
			{
				OP_Cubic = VolumeCalculator.Calculate();
			}
		}

		#endregion

		bool CubicCalculationIsSuspended
		{
			get { return IsValidationSuspended || OP_MeasureUQ.IsEmpty || OP_MeasureUQInfo.HasErrors() || OP_CubicUQ.IsEmpty || OP_CubicUQInfo.HasErrors(); }
		}

		#endregion

		#region UpdateWeightedCostFromLocations

		public void UpdateWeightedCostFromLocations()
		{
			ZDecimal totalWeightedCost = 0m;
			ZDecimal totalInStock = 0m;

			foreach (OrgPartLocation location in Locations)
			{
				totalWeightedCost += location.OR_WeightCostThisLocation * location.OR_InStock;
				totalInStock += location.OR_InStock;
			}
			OP_WeightedCost = (totalInStock != 0) ? totalWeightedCost / totalInStock : 0;
		}

		#endregion

		#region Note Types

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.ExtendedCommercialDescription);
				fNoteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
				return fNoteTypes;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Product);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region ILandedCostHistoryMaster Members

		ZGuid ILandedCostHistoryMaster.PK
		{
			get { return PK; }
		}

		SchemaGuidColumn ILandedCostHistoryMaster.FKSchemaColumnInLandedCostHistory
		{
			get { return LandedCostHistorySchema.LH_OP; }
		}

		ZBool ILandedCostHistoryMaster.ShouldMarginPercentagesReadOnly
		{
			get { return true; }
		}

		ZString ILandedCostHistoryMaster.CountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		IEnumerable<ICustomsChargeLCItemSetting> ILandedCostHistoryMaster.CustomsChargeLCItemSettings
		{
			get
			{
				return customsChargeLCItemSettings ?? (customsChargeLCItemSettings = ObjectFactory.Get<ICustomsChargeLCItemSettingsProvider>("ICustomsChargeLCItemSettingsProvider").GetCustomsChargeLCItemSettings(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, GlbCompany.CurrentCompany.PK));
			}
		}
		IEnumerable<ICustomsChargeLCItemSetting> customsChargeLCItemSettings;

		#endregion

		#region ICustomLabelsProvider

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider configOrgProvider)
			{
				Argument.NotNull(configOrgProvider, "ConfigOrgProvider");
				this.fConfigOrgProvider = configOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return fConfigOrgProvider; }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(OrgSupplierPart), configOrg, ResString.GetMultilingualString("edde8f35-6409-4b8b-9a8a-f26537cb32c9", "the related organization with the 'form' column ticked"), factory);
				result.Add(Constants.CustomLabels.Parts.CustomAttribute1, Schema.OP_CustomAttrib1, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.Parts.CustomAttribute2, Schema.OP_CustomAttrib2, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.Parts.CustomAttribute3, Schema.OP_CustomAttrib3, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.Parts.CustomAttribute4, Schema.OP_CustomAttrib4, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.Parts.CustomAttribute5, Schema.OP_CustomAttrib5, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.Parts.CustomFlag1, Schema.OP_CustomFlag1, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.Parts.CustomFlag2, Schema.OP_CustomFlag2, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.Parts.CustomFlag3, Schema.OP_CustomFlag3, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.Parts.CustomFlag4, Schema.OP_CustomFlag4, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.Parts.CustomFlag5, Schema.OP_CustomFlag5, Constants.CustomLabels.Descriptions.CustomFlag(5));
				result.Add(Constants.CustomLabels.Parts.CustomDecimal1, Schema.OP_CustomDecimal1, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.Parts.CustomDecimal2, Schema.OP_CustomDecimal2, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.Parts.CustomDecimal3, Schema.OP_CustomDecimal3, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.Parts.CustomDecimal4, Schema.OP_CustomDecimal4, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.Parts.CustomDecimal5, Schema.OP_CustomDecimal5, Constants.CustomLabels.Descriptions.CustomNumber(5));
				result.Add(Constants.CustomLabels.Parts.CustomDate1, Schema.OP_CustomDate1, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.Parts.CustomDate2, Schema.OP_CustomDate2, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.Parts.CustomDate3, Schema.OP_CustomDate3, Constants.CustomLabels.Descriptions.CustomDate(3));
				result.Add(Constants.CustomLabels.Parts.CustomDate4, Schema.OP_CustomDate4, Constants.CustomLabels.Descriptions.CustomDate(4));
				result.Add(Constants.CustomLabels.Parts.CustomDate5, Schema.OP_CustomDate5, Constants.CustomLabels.Descriptions.CustomDate(5));

				result.Add(Constants.CustomLabels.Parts.OrderMultiple, Schema.OP_OrderMultipleQty, Constants.CustomLabels.Parts.Descriptions.OrderMultiple, CustomLabelStyles.ShowByDefault);
				result.Add(Constants.CustomLabels.Parts.VendorPack, Schema.OP_VendorPackQty, Constants.CustomLabels.Parts.Descriptions.VendorPack, CustomLabelStyles.ShowByDefault);
				result.Add(Constants.CustomLabels.Parts.Department, Schema.OP_Department, Constants.CustomLabels.Parts.Descriptions.Department, CustomLabelStyles.ShowByDefault);
				result.Add(Constants.CustomLabels.Parts.Division, Schema.OP_Division, Constants.CustomLabels.Parts.Descriptions.Division, CustomLabelStyles.ShowByDefault);
				return result;
			}

			protected ICustomLabelsConfigOrgProvider fConfigOrgProvider;
		}

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}
		bool fIsImportingData;

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { OP_OH_FormLayoutControllerChanged += value; }
			remove { OP_OH_FormLayoutControllerChanged -= value; }
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), OP_OH_FormLayoutController); }
		}

		#endregion

		#region IPartProvider Members

		OrgSupplierPart IPartProvider.Part
		{
			get { return this; }
		}

		#endregion

		#region Test Data

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OrgHeader customLabelsConfigOrg = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			OrgPartRelation customLabelsConfigOrgRelation = RelatedOrganisations.AddOrganisationIfNotExist(customLabelsConfigOrg.PK, OrgPartRelation.RelationshipTypes.Supplier);
			customLabelsConfigOrgRelation.OU_FormLayoutController = true;
		}
#endif

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return TemplateCopyCore();
		}

		protected virtual IBusiness TemplateCopyCore()
		{
			Type partTypeToCopyTo = OrgSupplierPartTypeDecider.GetTypeForBinding(GetType());

			OrgSupplierPart part = (OrgSupplierPart)Factory.New(partTypeToCopyTo);

			using (part.GetValidationSuspender())
			{
				part.OP_StockKeepingUnit = OP_StockKeepingUnit;
				part.OP_Cubic = OP_Cubic;
				part.OP_CubicUQ = OP_CubicUQ;
				part.OP_Department = OP_Department;
				part.OP_Depth = OP_Depth;
				part.OP_Division = OP_Division;
				part.OP_Height = OP_Height;
				part.OP_IsActive = OP_IsActive;
				part.OP_MeasureUQ = OP_MeasureUQ;
				part.OP_OrderMultipleQty = OP_OrderMultipleQty;
				part.OP_Desc = OP_Desc;
				part.OP_OrderMultipleUnit = OP_OrderMultipleUnit;
				part.OP_RH_NKCommodityCode = OP_RH_NKCommodityCode;
				part.OP_Width = OP_Width;
				part.OP_WeightUQ = OP_WeightUQ;
				part.OP_Weight = OP_Weight;
			}

			OrgPartRelation[] relationsFromPart = (OrgPartRelation[])Factory.Load(typeof(OrgPartRelation), new ZQuery(OrgPartRelationSchema.OU_OP, PK));
			if (relationsFromPart != null)
			{
				foreach (OrgPartRelation partRelation in relationsFromPart)
				{
					TemplateCopyOrgPartRelation(part, partRelation);
				}
			}

			return part;
		}

		protected virtual OrgPartRelation TemplateCopyOrgPartRelation(OrgSupplierPart part, OrgPartRelation partRelation)
		{
			OrgPartRelation newPartRelation = (OrgPartRelation)partRelation.Clone();
			newPartRelation.OU_OP = part.PK;
			return newPartRelation;
		}

		#endregion

		#region IWorkflowProvider Members

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new OrgSupplierPartProcessTasksCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, OP_OH_FormLayoutController, ZGuid.Empty);
			return result;
		}

		public ZString WorkflowType
		{
			get { return WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode; }
		}

		#endregion

		#region JobNumber

		public string JobNumber
		{
			get { return OP_PartNum; }
		}

		#endregion

		#region ICustomFieldProvider Members

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this);
			AddOrganisationCustomFields(properties);
			AddWorkflowCustomFields(properties);
			return new CustomBusinessObject(Factory, this, properties);
		}

		void AddOrganisationCustomFields(UserDefinedPropertyCollection properties)
		{
			var customLabelsProvider = new CustomLabelsProvider(this);
			var list = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, Factory);
			foreach (CustomLabelInfo field in list)
			{
				if (field.IsEnabled && !PropertiesToExcludeFromCustomFieldsControl.Contains(field.PropertyName))
				{
					properties.Add(ZGuid.NewZGuid(), field.PropertyName, field.Caption);
				}
			}
		}

		void AddWorkflowCustomFields(UserDefinedPropertyCollection properties)
		{
			properties.WithWorkflowTemplateCustomFields(this);
		}

		HashSet<string> PropertiesToExcludeFromCustomFieldsControl
		{
			get
			{
				return propertiesToExcludeFromCustomFieldsControl ??
					(propertiesToExcludeFromCustomFieldsControl = new HashSet<string>() { Schema.OP_OrderMultipleQty, Schema.OP_VendorPackQty, Schema.OP_Department, Schema.OP_Division });
			}
		}
		HashSet<string> propertiesToExcludeFromCustomFieldsControl;

		#endregion

		#region ISetterSuspenderSupporter

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		IEnumerable<string> ISetterSuspenderSupporter.SupportedFields => supportedFields ?? (supportedFields = GetSupportedFields());
		IEnumerable<string> supportedFields;

		protected virtual IEnumerable<string> GetSupportedFields()
		{
			yield return Schema.OP_Brand;
			yield return Schema.OP_Model;
		}

		#endregion

		#region IHaveRequiredDocumentsWithAttributes Members
		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return OP_PartNum; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return ZString.Empty; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return ZString.Empty; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return OrgSupplierPartSchema.Constants.Prefix; }
		}

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return new ZString[] { Core.Constants.ReferenceTypes.SupplyChainLogistics }; }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
		}

		#endregion

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(CusClassPartPivotSchema.CI_OP, null);
				yield return new AuditChildInfo(OrgPartRelationSchema.OU_OP, null);
				yield return new AuditChildInfo(OrgPartUnitSchema.OF_OP, null);
				yield return new AuditChildInfo(OrgSupplierPartBarcodeSchema.PH_OP, OrgSupplierPartBarcodeSchema.PH_Barcode);
				yield return new AuditChildInfo(UNDGDataItemSchema.DI_ParentID, null);
			}
		}
	}
}
