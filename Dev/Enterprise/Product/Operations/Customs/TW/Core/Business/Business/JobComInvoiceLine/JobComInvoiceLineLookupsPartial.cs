using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobComInvoiceLineLookups
	{
		public CodeDescriptionPairList CarConditionCodeList => Factory.GetCachedValue<CarConditionCodeList>();

		public CodeDescriptionPairList CatalystConverterPrintModeList => Factory.GetCachedValue<CatalystConverterPrintModeList>();

		public CodeDescriptionPairList EngineTypeCodeList => Factory.GetCachedValue<EngineTypeCodeList>();

		public CodeDescriptionPairList EquipmentPrintModeList => Factory.GetCachedValue<EquipmentPrintModeList>();

		public CodeDescriptionPairList LeftSideSteeringCodeList => Factory.GetCachedValue<LeftSideSteeringCodeList>();

		public CodeDescriptionPairList TransmissionCodeList => Factory.GetCachedValue<TransmissionCodeList>();

		public CodeDescriptionPairList GoodsTypeList
		{
			get
			{
				var cacheType = GoodsTypeListCacheKey;
				var cachedKey = ZString.Format((NoResString)"Enterprise.Customs.TW.Business.GoodsTypeList_{0}", cacheType);
				return Factory.GetCachedValue(cachedKey, () =>
				{
					var result = new UntranslatableCodeDescriptionPairList((NoResString)"Goods Type List");
					switch (cacheType)
					{
						case Constants.GoodsTypeListCacheType.DNOnly:
							result.AddRange(new CPT_107_301_GoodsTypeList());
							break;
						case Constants.GoodsTypeListCacheType.IFOrDH:
							result.AddRange(new CPT_107_601_GoodsTypeList());
							break;
						case Constants.GoodsTypeListCacheType.All:
							result.AddRange(new CPT_107_301_GoodsTypeList());
							result.AddRange(new CPT_107_601_GoodsTypeList());
							break;
					}
					return result;
				});
			}
		}

		ZString GoodsTypeListCacheKey
		{
			get
			{
				var result = ZString.Empty;
				var includeDN = Parent.IsForCAHeaderDN;
				var includeIFOrDH = Parent.IsForCAHeaderIF || Parent.IsForCAHeaderDH;
				if (includeDN && !includeIFOrDH)
				{
					result = Constants.GoodsTypeListCacheType.DNOnly;
				}
				else if (!includeDN && includeIFOrDH)
				{
					result = Constants.GoodsTypeListCacheType.IFOrDH;
				}
				else if (includeDN && includeIFOrDH)
				{
					result = Constants.GoodsTypeListCacheType.All;
				}
				return result;
			}
		}

		public CodeDescriptionPairList BondedGoodsCodeList => Factory.GetCachedValue<BondedGoodsCodeList>();

		public CodeDescriptionPairList CarTypeCodeList => Factory.GetCachedValue<CarTypeCodeList>();

		public Customs.Business.OrgSupplierPartCollection NewOwnerProducts
		{
			get
			{
				var invoiceLine = Parent;
				var owner = invoiceLine.EntryInstruction?.Owner;
				var result = new Customs.Business.OrgSupplierPartCollection(Factory, invoiceLine, null, owner, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
				if (!invoiceLine.JI_NewOwnerPartNo.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", invoiceLine.JI_NewOwnerPartNo));
				}
				if (owner != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", owner.PK));
				}
				return result;
			}
		}

		public CodeDescriptionPairList InnerPackageTypeList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.InnerPackageTypeList", () =>
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Inner Packing Type List");
			result.AddRange(new CPT_115_InnerPackageTypeList());
			return result;
		});

		public CodeDescriptionPairList InnerPackingMaterialList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.InnerPackingMaterialList", () =>
		{
			var result = new UntranslatableCodeDescriptionPairList((NoResString)"Inner Packing Material List");
			result.AddRange(new CPT_114_InnerPackingMaterialList());
			return result;
		});

		public CodeDescriptionPairList ContainerCapacityList => Factory.GetCachedValue<ContainerCapacityList>();

		public CodeDescriptionPairList ContainerMaterialList => Factory.GetCachedValue<ContainerMaterialList>();

		public CodeDescriptionPairList ContainerMaterialNumberList => Factory.GetCachedValue<ContainerMaterialNumberList>();

		public CodeDescriptionPairList RAPRORCurrencyList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var localCurr = Parent?.LocalCurrency;
				if (localCurr != null)
				{
					result.Add(localCurr);
				}
				var invoiceCurr = Parent?.InvoiceHeader?.Invoice_Currency;
				if (invoiceCurr != null && invoiceCurr != localCurr)
				{
					result.Add(invoiceCurr);
				}
				return result;
			}
		}

		public CodeDescriptionPairList TextileWidthUQList => Factory.GetCachedValue<TextileWidthUQList>();

		public CodeDescriptionPairList DutyOrTaxPaymentMethodList
		{
			get
			{
				var isROR = Parent.IsROR;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.JobComInvoiceLineLookups.DutyOrTaxPaymentMethodList{isROR}", () =>
				{
					var result = new CodeDescriptionPairList(new DutyTaxPaymentMethodList());
					if (!isROR)
					{
						result.RemoveCode(DutyTaxPaymentMethodList.Codes.RorPayment);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList TariffPrintLengthList => Factory.GetCachedValue<TariffPrintLengthList>();

		public CodeDescriptionPairList CPT_124_OriginCriteriaCodeList_EN => Factory.GetCachedValue<CPT_124_OriginCriteriaCodeList_EN>();

		public CodeDescriptionPairList ManufacturerRelationshipCodes
		{
			get
			{
				var certificateType = Parent.NX101CertificateType;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.JobComInvoiceLineLookups.ManufacturerRelationshipCodes_{certificateType}", () =>
				{
					CodeDescriptionPairList result;
					switch (certificateType)
					{
						case CertificateTypeList.Codes.Code9:
						case CertificateTypeList.Codes.Code11:
							result = new CPT_126_09_11_ManufacturerRelationship();
							break;
						case CertificateTypeList.Codes.Code13:
						case CertificateTypeList.Codes.Code14:
						case CertificateTypeList.Codes.Code18:
						case CertificateTypeList.Codes.Code19:
							result = new CPT_126_13_14_18_19_ManufacturerRelationship();
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList PTCriteriaCodes
		{
			get
			{
				var certificateType = Parent.NX101CertificateType;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.JobComInvoiceLineLookups.PTCriteriaCodes_{certificateType}", () =>
				{
					CodeDescriptionPairList result;
					switch (certificateType)
					{
						case CertificateTypeList.Codes.Code9:
							result = new CPT_125_09_PTCriteria();
							break;
						case CertificateTypeList.Codes.Code11:
							result = new CPT_125_11_PTCriteria();
							break;
						case CertificateTypeList.Codes.Code13:
							result = new CPT_125_13_PTCriteria();
							break;
						case CertificateTypeList.Codes.Code14:
							result = new CPT_125_14_PTCriteria();
							break;
						case CertificateTypeList.Codes.Code15:
							result = new CPT_125_15_PTCriteria();
							break;
						case CertificateTypeList.Codes.Code19:
							result = new CPT_125_19_PTCriteria();
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList PTCriteria2List
		{
			get
			{
				var certificateType = Parent.NX101CertificateType;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.JobComInvoiceLineLookups.PTCriteria2List{certificateType}", () =>
				{
					CodeDescriptionPairList result;
					switch (certificateType)
					{
						case CertificateTypeList.Codes.Code9:
						case CertificateTypeList.Codes.Code11:
						case CertificateTypeList.Codes.Code13:
							result = new CPT_125_09_11_13OtherPTCriteria();
							break;
						case CertificateTypeList.Codes.Code14:
							result = new CPT_125_14OtherPTCriteria();
							break;
						case CertificateTypeList.Codes.Code15:
							result = new CPT_125_15OtherPTCriteria();
							break;
						case CertificateTypeList.Codes.Code18:
							result = new CPT_125_18OtherPTCriteria();
							break;
						case CertificateTypeList.Codes.Code19:
							result = new CPT_125_19OtherPTCriteria();
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					return result;
				});
			}
		}
	}
}
