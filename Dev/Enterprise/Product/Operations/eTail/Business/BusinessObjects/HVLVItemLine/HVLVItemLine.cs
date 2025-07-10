using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.eTail.Business
{
	[DependentBusinessObject(typeof(HVLVItem), nameof(HVLVItem.Lines))]
	public class HVLVItemLine : AutoHVLVItemLine,
		IInvoiceLinePartClassificationTariffDescriptionSyncroniser,
		IInvoiceLinePartDetails,
		IHVLVItemLine
	{
		public HVLVItemLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			factory.SetBulkCopyOnTable(HVLVItemLineSchema.Constants.TableName, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, fireTriggers: true);
		}

		public new class Schema : AutoHVLVItemLine.Schema
		{
			public const string HVS_FormattedOriginTariff = "HVS_FormattedOriginTariff";
			public const string HVS_FormattedDestinationTariff = "HVS_FormattedDestinationTariff";
			public const int FormattedTariffMaxLength = 12;
		}

		#region Related Business Objects

		public HVLVItem ParentItem => parentItem ?? (parentItem = Factory.Load<HVLVItem>(HVS_HVI_HVLVItem));

		HVLVItem parentItem;

		#endregion

		#region Properties

		[DecimalPlaces(2)]
		public override ZDecimal HVS_IntrinsicValue
		{
			get { return base.HVS_IntrinsicValue; }
			set
			{
				base.HVS_IntrinsicValue = value;
				ClearPreScreeningStatus();
			}
		}

		[DecimalPlaces(2)]
		public override ZDecimal HVS_CustomsValue
		{
			get { return base.HVS_CustomsValue; }
			set
			{
				base.HVS_CustomsValue = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVS_GoodsDescription
		{
			get
			{
				return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVS_GoodsDescription);
			}
			set
			{
				base.HVS_GoodsDescription = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVS_OriginGoodsDescription
		{
			get
			{
				return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVS_OriginGoodsDescription);
			}
			set
			{
				base.HVS_OriginGoodsDescription = value;
				ClearPreScreeningStatus();
			}
		}

		public override ZString HVS_ItemURL
		{
			get
			{
				return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVS_ItemURL);
			}
			set
			{
				base.HVS_ItemURL = value;
			}
		}

		[RelatedBusinessObject(nameof(ParentItem))]
		public override ZGuid HVS_HVI_HVLVItem
		{
			get { return base.HVS_HVI_HVLVItem; }
			set
			{
				if (base.HVS_HVI_HVLVItem != value)
				{
					base.HVS_HVI_HVLVItem = value;

					if (ParentItem is HVLVItem parentItem)
					{
						parentItem.MarkForReloadItemsFromLocalCache();
						HVS_ClusterKey = parentItem.HVI_ClusterKey;
					}

					RefreshPartSyncManagerActiveDeciderPK();
				}
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ProductCodeReadonly))]
		[List("Lookups.Products", "OP_PartNum", "OP_PartNum")]
		public override ZString HVS_ProductCode
		{
			get => base.HVS_ProductCode;
			set
			{
				if (!IsCopying && base.HVS_ProductCode != value)
				{
					base.HVS_ProductCode = value;

					if (!((ISupportUXMLDataImporting)ParentItem).IsUXMLImportingData)
					{
						PartSyncManager.Refresh();
						LoadDefaultValuesFromProduct();
					}
				}
			}
		}

		OrgSupplierPart Product
		{
			get
			{
				var query = Lookups.Products.CompleteFilter;
				query.AddToFilter(OrgSupplierPartSchema.OP_PartNum, HVS_ProductCode);
				return Factory.LoadTop1<OrgSupplierPart>(query);
			}
		}

		void LoadDefaultValuesFromProduct()
		{
			var product = Product;
			if (product != null)
			{
				HVS_NetWeight = product.OP_NetWeight;
				HVS_GrossWeight = product.OP_Weight;
				HVS_WeightUnit = product.OP_WeightUQ;

				foreach (Enterprise.Integration.Customs.Shared.ICusClassPartPivot pivot in product.PivotsForBinding)
				{
					switch (pivot.CI_ChildType)
					{
						case ClassificationTypeList.Codes.HTI:
							HVS_DestinationTariff = pivot.CI_TariffNum;
							break;
						case ClassificationTypeList.Codes.HTE:
							HVS_OriginTariff = pivot.CI_TariffNum;
							break;
					}
				}
			}
		}

		bool ProductCodeReadonly => ParentItem.ETailer == null;

		[List("Lookups.WeightUnitList")]
		public override ZString HVS_WeightUnit
		{
			get => base.HVS_WeightUnit;
			set
			{
				base.HVS_WeightUnit = value.ToUpper();
				if (HVS_GrossWeight > 0)
				{
					ParentItem?.CalculateManifestedWeight();
				}
			}
		}

		[ReadOnly(true)]
		public ZString ShipmentOriginCountryCode
		{
			get
			{
				return ParentItem.Shipment?.JS_RL_NKOrigin.SubstringSafe(0, 2) ?? ZString.Empty;
			}
		}

		[ReadOnly(true)]
		public ZString ShipmentDestinationCountryCode
		{
			get
			{
				return ParentItem.Shipment?.JS_RL_NKDestination.SubstringSafe(0, 2) ?? ZString.Empty;
			}
		}

		[List("Lookups.ClassificationList")]
		[RelatedBusinessObject("ClassificationLookup")]
		public override ZGuid HVS_CC_Lookup
		{
			get => base.HVS_CC_Lookup;
			set
			{
				if (base.HVS_CC_Lookup != value)
				{
					var shouldSetDescription = PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;
					base.HVS_CC_Lookup = value;
					UpdateOriginAndDestinationTarrifIfRequired();
					if (shouldSetDescription)
					{
						PartClassificationTariffDescriptionSyncroniser.SetDescription();
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.FormattedTariffMaxLength)]
		public ZString HVS_FormattedDestinationTariff
		{
			get => TariffFormatterDecider.GetByCountryCode(ShipmentDestinationCountryCode).DisplayFormat(base.HVS_DestinationTariff);
			set => HVS_DestinationTariff = TariffFormatterDecider.GetByCountryCode(ShipmentDestinationCountryCode).Format(value);
		}

		public ZPropertyInfo HVS_FormattedDestinationTariffInfo => GetWrappedZPropertyInfo(nameof(HVS_FormattedDestinationTariff), (x) => HVS_DestinationTariffInfo);

		public override ZString HVS_DestinationTariff
		{
			get => base.HVS_DestinationTariff;
			set
			{
				base.HVS_DestinationTariff = value.Replace(".", "");
				if (value.IsEmpty)
				{
					ClearUpClassificaitonLookupAndGoodsDescription();
				}

				UpdateTariffDescription(DestinationTariffDescription);
				var shouldSetDescription = PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;
				if (shouldSetDescription)
				{
					PartClassificationTariffDescriptionSyncroniser.SetDescription();
				}

				ClearPreScreeningStatus();
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.FormattedTariffMaxLength)]
		public ZString HVS_FormattedOriginTariff
		{
			get => TariffFormatterDecider.GetByCountryCode(HVS_RN_NKOriginCountryCode).DisplayFormat(base.HVS_OriginTariff);
			set => HVS_OriginTariff = TariffFormatterDecider.GetByCountryCode(HVS_RN_NKOriginCountryCode).Format(value);
		}

		public ZPropertyInfo HVS_FormattedOriginTariffInfo => GetWrappedZPropertyInfo(nameof(HVS_FormattedOriginTariff), (x) => HVS_OriginTariffInfo);

		public override ZString HVS_OriginTariff
		{
			get => base.HVS_OriginTariff;
			set
			{
				base.HVS_OriginTariff = value.Replace(".", "");
				if (value.IsEmpty)
				{
					ClearUpClassificaitonLookupAndGoodsDescription();
				}

				UpdateTariffDescription(OriginTariffDescription);
				var shouldSetDescription = PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;
				if (shouldSetDescription)
				{
					PartClassificationTariffDescriptionSyncroniser.SetDescription();
				}

				ClearPreScreeningStatus();
			}
		}

		public override ZDecimal HVS_GrossWeight
		{
			get => base.HVS_GrossWeight;
			set
			{
				var oldValue = base.HVS_GrossWeight;
				if (oldValue != value)
				{
					base.HVS_GrossWeight = value;

					if (oldValue != ZDecimal.Zero || value > ZDecimal.Zero)
					{
						ParentItem?.CalculateManifestedWeight();
					}
				}
			}
		}

		public BaseCusClassification ClassificationLookup
		{
			get { return Factory.Load<BaseCusClassification>(HVS_CC_Lookup); }
		}

		void UpdateOriginAndDestinationTarrifIfRequired()
		{
			if (ClassificationLookup != null)
			{
				var tariffNumber = ClassificationLookup.CC_TariffNum;
				var directionOfTrade = Consignment?.DirectionOfTrade;
				if (ClassificationLookup.CC_ClassificationType == ClassificationType.IMP || directionOfTrade == Directions.Import)
				{
					HVS_DestinationTariff = tariffNumber;
				}
				else if (ClassificationLookup.CC_ClassificationType == ClassificationType.EXP || directionOfTrade == Directions.Export)
				{
					HVS_OriginTariff = tariffNumber;
				}
				else
				{
					HVS_DestinationTariff = tariffNumber;
					HVS_OriginTariff = tariffNumber;
				}
			}
		}

		void ClearUpClassificaitonLookupAndGoodsDescription()
		{
			HVS_CC_Lookup = ZGuid.Empty;
		}

		void ClearPreScreeningStatus()
		{
			if (Consignment != null)
			{
				Consignment.ClearPreScreeningStatus();
			}
		}

		HVLVConsignment Consignment => ParentItem?.Consignment;

		#endregion

		#region PartClassificationTariffDescriptionSyncroniser

		internal InvoiceLinePartClassificationTariffDescriptionSyncroniser PartClassificationTariffDescriptionSyncroniser
		{
			get { return partClassificationTariffDescriptionSyncroniser ?? (partClassificationTariffDescriptionSyncroniser = new InvoiceLinePartClassificationTariffDescriptionSyncroniser(this)); }
		}

		InvoiceLinePartClassificationTariffDescriptionSyncroniser partClassificationTariffDescriptionSyncroniser;

		#endregion

		#region IInvoiceLinePartClassificationTariffDescriptionSyncroniser

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.Description
		{
			get { return HVS_GoodsDescription; }
			set { HVS_GoodsDescription = value; }
		}

		BaseCusClassification IInvoiceLinePartClassificationTariffDescriptionSyncroniser.Classification => ClassificationLookup;

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.ClassificationDescription
		{
			get { return ClassificationLookup != null ? ClassificationLookup.CC_Description.Trim().ToUpper() : ZString.Empty; }
		}

		OrgSupplierPart IInvoiceLinePartClassificationTariffDescriptionSyncroniser.Part
		{
			get
			{
				OrgSupplierPart result = null;
				if (PartSyncManager.Part != null && !PartSyncManager.Part.IsDeleted)
				{
					result = PartSyncManager.Part;
				}

				return result;
			}
		}

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.PartDescription => Product?.OP_Desc ?? ZString.Empty;

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.ExtraInfoForClassification { get; set; }

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.PartExtendedCommercialDescription => ZString.Empty;

		bool IInvoiceLinePartClassificationTariffDescriptionSyncroniser.IsExtendedCommercialDescriptionEnabled => false;

		ZPropertyInfo IInvoiceLinePartClassificationTariffDescriptionSyncroniser.DescriptionInfo => base.HVS_GoodsDescriptionInfo;

		ZString IInvoiceLinePartClassificationTariffDescriptionSyncroniser.TariffDescription
		{
			get
			{
				var directionOfTrade = Consignment?.DirectionOfTrade;
				if (directionOfTrade == Directions.Export)
				{
					return OriginTariffDescription;
				}
				else if (directionOfTrade == Directions.Import)
				{
					return DestinationTariffDescription;
				}
				else
				{
					return TariffDescription;
				}
			}
		}

		void UpdateTariffDescription(ZString tariffDescription)
		{
			TariffDescription = tariffDescription;
		}

		ZString OriginTariffDescription => ZString.Empty;

		ZString DestinationTariffDescription => ZString.Empty;

		ZString TariffDescription;

		BaseJobDeclaration IInvoiceLinePartClassificationTariffDescriptionSyncroniser.Declaration => null;

		#endregion

		#region HVLVItemLinePartSynchronisationManager

		public JobComInvoiceLinePartSynchronisationManager PartSyncManager => partSyncManager ?? (partSyncManager = new JobComInvoiceLinePartSynchronisationManager(this));

		protected JobComInvoiceLinePartSynchronisationManager partSyncManager;

		#endregion

		#region IInvoiceLinePartDetails

		public ZString CustomsCountryCode
		{
			get
			{
				if (!customsCountryCode.HasValue)
				{
					customsCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}

				return customsCountryCode.Value;
			}
		}

		ZString? customsCountryCode;

		internal void RefreshPartSyncManagerActiveDeciderPK()
		{
			partSyncManagerActiveDeciderPK = null;
		}

		ZGuid IInvoiceLinePartDetails.PartSyncManagerActiveDeciderPK
		{
			get
			{
				if (!partSyncManagerActiveDeciderPK.HasValue)
				{
					partSyncManagerActiveDeciderPK = GetPartSyncManagerActiveDeciderPKWithFallback();
				}

				return partSyncManagerActiveDeciderPK.Value;
			}
		}

		ZGuid? partSyncManagerActiveDeciderPK;

		ZGuid GetPartSyncManagerActiveDeciderPKWithFallback()
		{
			return Consignment?.ConsignmentHeader?.PK ?? Consignment?.BookingHeader?.PK ?? ZGuid.Empty;
		}

		void IInvoiceLinePartDetails.UpdateDetailsOnPartChange()
		{
			if (!IsCopying && !IsDeleted && !IsNull)
			{
				var shouldSetDescription = PartClassificationTariffDescriptionSyncroniser.IsDefaultDescription;

				var product = PartSyncManager.Part;

				if (product == null || product.IsDeleted)
				{
					HVS_ProductCode = ZString.Empty;
				}
				else
				{
					HVS_ProductCode = product.OP_PartNum;
					if (PartSyncManager == null)
					{
						throw new InvalidOperationException("invoiceLine.PartSyncManager is null");
					}
				}

				PartClassificationTariffDescriptionSyncroniser.RefreshPartGeneratedLineDescription();
				if (shouldSetDescription)
				{
					PartClassificationTariffDescriptionSyncroniser.SetDescription();
				}
			}
		}

		OrgHeader IInvoiceLinePartDetails.Importer => ParentItem?.Shipment?.Consignee;
		OrgHeader IInvoiceLinePartDetails.Supplier => ParentItem.Shipment?.Consignor ?? Consignment?.BookingHeader?.BillToParty?.Header;
		BaseJobComInvoiceHeader IInvoiceLinePartDetails.Header => null;
		bool IInvoiceLinePartDetails.IsForImportSectionOfDrawback => false;
		bool IInvoiceLinePartDetails.IsForExportSectionOfDrawback => false;
		bool IInvoiceLinePartDetails.IsDrawback => false;
		bool IInvoiceLinePartDetails.Enabled => true;
		ZGuid IInvoiceLinePartDetails.PartPK
		{
			get => Product?.PK ?? ZGuid.Empty;
			set
			{
				var product = Factory.Load(((IInvoiceLinePartDetails)this).TypeOfPartUsed, value) as OrgSupplierPart;
				HVS_ProductCode = product?.OP_PartNum ?? ZString.Empty;
			}
		}

		ZString IInvoiceLinePartDetails.PartNo => HVS_ProductCode;
		RefCountry IInvoiceLinePartDetails.InvoiceCountry => GlbCompany.CurrentCompany.Country;
		Type IInvoiceLinePartDetails.TypeOfPartUsed => MasterFiles.Business.OrgSupplierPartTypeDecider.GetOrgSupplierPartType(CustomsCountryCode);
		bool IInvoiceLinePartDetails.JustUpdatedByDataRefresh => false;

		#endregion

		#region IHVLVItemLine

		IHVLVItem IHVLVItemLine.ParentItem => ParentItem;

		IHVLVConsignment IHVLVItemLine.Consignment => Consignment;

		IRefCountry IHVLVItemLine.OriginCountryCode => OriginCountryCode;

		Enterprise.Integration.Customs.IBaseCusClassification IHVLVItemLine.ClassificationLookup => ClassificationLookup;

		#endregion
	}
}
