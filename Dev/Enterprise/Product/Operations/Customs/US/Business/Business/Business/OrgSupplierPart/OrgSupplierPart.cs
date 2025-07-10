using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart, Integration.Customs.US.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override MasterFiles.Business.OrgSupplierPartValidation GetNewValidation()
		{
			return new OrgSupplierPartValidation(this);
		}

		#region New Properties

		public GlbStaffCollection Staffs
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public new OrgSupplierPartValidation Validation
		{
			get { return (OrgSupplierPartValidation)base.Validation; }
		}

		public ZString CountryOfOrigin
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_UC_NKCountryOfOrigin.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_UC_NKCountryOfOrigin.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_UC_NKCountryOfOrigin.ToString();
			}
		}

		public ZString CountryOfExport
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_UC_NKCountryOfExport.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_UC_NKCountryOfExport.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_UC_NKCountryOfExport.ToString();
			}
		}

		public ZString SPIIndicator
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_SPI.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_SPI.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_SPI.ToString();
			}
		}

		public ZString ProductClaim
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_ProductClaim.IsEmpty)))
				{
					return "MULTI";
				}

				var classifications = pivots.Where(x => !x.CD_ProductClaim.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? string.Empty : classifications.Length > 1 ? "MULTI" : classifications[0].CD_ProductClaim.ToString();
			}
		}

		public ZString RulingType
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_RulingType.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_RulingType.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_RulingType.ToString();
			}
		}

		public ZString RulingNum
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_RulingNumber.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_RulingNumber.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_RulingNumber.ToString();
			}
		}

		public ZString ImportLaceyActPGAIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importLaceyActPGAIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_LaceyActIndicator);
			}
		}
		CachedProperty<ZString> importLaceyActPGAIndicator;

		public ZString ImportATFIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importATFIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_ATFIndicator);
			}
		}
		CachedProperty<ZString> importATFIndicator;

		public ZString ImportDDTCIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importDDTCIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, (x) => x.Details.CD_DDTCIndicator);
			}
		}
		CachedProperty<ZString> importDDTCIndicator;

		ZString GetEffectiveValueFromAllPivots(ref CachedProperty<ZString> cachedProperty, ZString[] parentPivotTypes, Func<CusClassPartPivot, ZString> selector)
		{
			return Factory.GetValue(ref cachedProperty, () =>
			{
				var distinctIndicators = GetDistinctValuesFromAllPivots(parentPivotTypes, selector).Take(2).ToArray();
				return distinctIndicators.Length == 0 ? string.Empty : distinctIndicators.Length > 1 ? "MULTI" : distinctIndicators[0].ToString();
			});
		}

		IEnumerable<ZString> GetDistinctValuesFromAllPivots(ZString[] parentPivotTypes, Func<CusClassPartPivot, ZString> selector)
		{
			var pivots = GetUSPivots();
			var list = pivots.Where(x => parentPivotTypes.Contains(x.CI_ChildType)).Select(selector);
			list = list.Concat(pivots.SelectMany<CusClassPartPivot, CusClassPartPivot>(x => x.Children).Cast<CusClassPartPivot>().Select(selector));
			return list.Distinct();
		}

		public ZString ImportFWSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importFWSIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_FWSIndicator);
			}
		}
		CachedProperty<ZString> importFWSIndicator;

		public ZString ImportNMFS370Indicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importNMFS370Indicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_NMFS370Indicator);
			}
		}
		CachedProperty<ZString> importNMFS370Indicator;

		public ZString ImportNMFSAMRIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importNMFSAMRIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_NMFSAMRIndicator);
			}
		}
		CachedProperty<ZString> importNMFSAMRIndicator;

		public ZString ImportNMFSHMSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importNMFSHMSIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_NMFSHMSIndicator);
			}
		}
		CachedProperty<ZString> importNMFSHMSIndicator;

		public ZString ImportNMFSSIMPIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importNMFSSIMPIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_NMFSSIMPIndicator);
			}
		}
		CachedProperty<ZString> importNMFSSIMPIndicator;

		public ZString TSCAIndicator
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_TSCAIndicator.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_TSCAIndicator.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_TSCAIndicator.ToString();
			}
		}

		public ZString CottonFeeExempt
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_CottonFeeExempt.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_CottonFeeExempt.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_CottonFeeExempt.ToString();
			}
		}

		public ZString ADDCaseNum
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_ADDCaseNo.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_ADDCaseNo.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_ADDCaseNo.ToString();
			}
		}

		public ZString CVDCaseNum
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_CVDCaseNo.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_CVDCaseNo.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_CVDCaseNo.ToString();
			}
		}

		public ZString TaxApplicability
		{
			get
			{
				var pivots = GetUSPivots();
				if (pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => !y.CD_TaxApplicability.IsEmpty)))
				{
					return "MULTI";
				}
				var classifications = pivots.Where(x => !x.CD_TaxApplicability.IsEmpty && x.IsImportClassification).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_TaxApplicability.ToString();
			}
		}

		public ZString ExportCode
		{
			get
			{
				var classifications = GetUSPivots().Where(x => !x.CD_ExportCode.IsEmpty && (x.IsHTE || x.IsSHB)).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_ExportCode.ToString();
			}
		}

		public ZString OriginIndicator
		{
			get
			{
				var classifications = GetUSPivots().Where(x => !x.CD_OriginIndicator.IsEmpty && (x.IsHTE || x.IsSHB)).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_OriginIndicator.ToString();
			}
		}

		public ZString LicenseType
		{
			get
			{
				var classifications = GetUSPivots().Where(x => !x.CD_LicenceType.IsEmpty && (x.IsHTE || x.IsSHB)).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_LicenceType.ToString();
			}
		}

		public ZString ECCN
		{
			get
			{
				var classifications = GetUSPivots().Where(x => !x.CD_ECCN.IsEmpty && (x.IsHTE || x.IsSHB)).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_ECCN.ToString();
			}
		}

		public ZString ITARExemptionNum
		{
			get
			{
				var classifications = GetUSPivots().Where(x => !x.CD_ITARExemptionNo.IsEmpty && (x.IsHTE || x.IsSHB)).ToArray();
				return classifications.Length == 0 ? "" : classifications.Length > 1 ? "MULTI" : classifications[0].CD_ITARExemptionNo.ToString();
			}
		}

		public ZString AttributeValues
		{
			get
			{
				var attributes = GetUSPivots().Where(x => x.IsImportClassification && (x.Attributes1.Count > 0 || x.Attributes2.Count > 0 || x.Attributes3.Count > 0)).ToArray();

				if (attributes == null || attributes.Length == 0)
				{
					return "";
				}
				else if (attributes.Length > 1)
				{
					return "MULTI";
				}
				else if (attributes[0].Attributes1.Count == 0 && attributes[0].Attributes2.Count == 0 && attributes[0].Attributes3.Count == 0)
				{
					return "";
				}
				else if (attributes[0].Attributes1.Count > 1 || attributes[0].Attributes2.Count > 1 || attributes[0].Attributes3.Count > 1)
				{
					return "MULTI";
				}

				if (attributes[0].Attributes1.Count == 1)
				{
					return (attributes[0].Attributes2.Count == 1 || attributes[0].Attributes3.Count == 1) ? "MULTI" : attributes[0].Attributes1[0].BG_AttributeValue1.ToString();
				}
				else if (attributes[0].Attributes2.Count == 1)
				{
					return (attributes[0].Attributes3.Count == 1) ? "MULTI" : attributes[0].Attributes2[0].BG_AttributeValue1.ToString();
				}
				else if (attributes[0].Attributes3.Count == 1)
				{
					return attributes[0].Attributes3[0].BG_AttributeValue1.ToString();
				}

				return "";
			}
		}

		public ZBool MultiTariffIndicator
		{
			get
			{
				return GetUSPivots().Any(x => x.Children.Count > 0);
			}
		}

		public ZString TariffDescription
		{
			get
			{
				var pivots = GetUSPivots();
				return pivots.Length == 0 ? "" : (pivots.Length > 1 ? "MULTI" : pivots[0].TariffDescription.ToString());
			}
		}

		public ZString ProvProgTariff
		{
			get
			{
				var multiTariffs = GetUSPivots().Where(x => !x.CI_SupplementalTariff.IsEmpty);
				return multiTariffs == null ? "" : multiTariffs.Count() > 1 ? "MULTI" : multiTariffs.Select(x => x.CI_SupplementalTariff).FirstOrDefault().ToString();
			}
		}

		public ZString TariffType
		{
			get
			{
				var pivots = GetUSPivots();
				return pivots.Length == 0 ? "" : pivots.Length > 1 ? "MULTI" : pivots[0].CI_ChildType.ToString();
			}
		}

		#region Indicator column style changed from checkbox to textbox
		public ZString ImportACEFDAIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importACEFDAIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_ACEFDAIndicator);
			}
		}
		CachedProperty<ZString> importACEFDAIndicator;

		public ZString ImportNHTSAIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importNHTSAIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_NHTSAIndicator);
			}
		}
		CachedProperty<ZString> importNHTSAIndicator;

		public ZString ImportTSCAClaimIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importTSCAClaimIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_TSCAClaimIndicator);
			}
		}
		CachedProperty<ZString> importTSCAClaimIndicator;

		public ZString ImportOMCIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importOMCIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_OMCIndicator);
			}
		}
		CachedProperty<ZString> importOMCIndicator;

		public ZString ImportODSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importODSIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_ODSIndicator);
			}
		}
		CachedProperty<ZString> importODSIndicator;

		public ZString ImportCPSCIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importCPSCIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_CPSCIndicator);
			}
		}
		CachedProperty<ZString> importCPSCIndicator;

		public ZString ImportAMSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importAMSIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_AMSIndicator);
			}
		}
		CachedProperty<ZString> importAMSIndicator;

		public ZString ImportNOPIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importNOPIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.Details.CD_NOPIndicator);
			}
		}
		CachedProperty<ZString> importNOPIndicator;

		public ZString ImportTTBIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importTTBIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_TTBIndicator);
			}
		}
		CachedProperty<ZString> importTTBIndicator;

		#endregion

		public ZString ImportPSTIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importPSTIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_PSTIndicator);
			}
		}
		CachedProperty<ZString> importPSTIndicator;

		public ZString ImportVNEIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importVNEIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_VNEIndicator);
			}
		}
		CachedProperty<ZString> importVNEIndicator;

		public ZString ImportDEAIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importDEAIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_DEAIndicator);
			}
		}
		CachedProperty<ZString> importDEAIndicator;

		public ZString ImportAPHISIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref importAPHISIndicator, new ZString[] { ClassificationTypeList.Codes.HTI }, x => x.CD_APHISIndicator);
			}
		}
		CachedProperty<ZString> importAPHISIndicator;

		public ZBool HasLaceyData
		{
			get
			{
				var pivots = GetUSPivots();
				bool hasPGA = pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => y.PGAs.Count > 0));

				if (!hasPGA)
				{
					hasPGA = pivots.Any(x => x.IsImportClassification && x.PGAs.Count > 0);
				}

				return hasPGA;
			}
		}

		[ChildEditable(true)]
		public ATFCollection ATFLines
		{
			get
			{
				if (atfLines == null)
				{
					atfLines = new ATFCollection(this);
					atfLines.Load();
					RegisterEditableChildObject(atfLines);
				}
				return atfLines;
			}
		}
		ATFCollection atfLines;

		public ZBool HasDDTCData
		{
			get
			{
				var pivots = GetUSPivots();
				bool hasPGA = pivots.Any(x => x.IsImportClassification &&
					x.Children.OfType<CusClassPartPivot>().Any(y => y.HasImportDDTCData));

				if (!hasPGA)
				{
					hasPGA = pivots.Any(x => x.IsImportClassification && x.HasImportDDTCData);
				}

				return hasPGA;
			}
		}

		public ZString Manufacturer
		{
			get
			{
				var manufacturers = GetUSPivots().Where(x => x.IsImportClassification && x.ManufacturerAddress != null && !x.ManufacturerAddress.Header.OH_Code.IsEmpty);
				return manufacturers.Count() > 1 ? "MULTI" : manufacturers.Select(x => x.ManufacturerAddress.Header.OH_Code).FirstOrDefault().ToString();
			}
		}

		#endregion

		#region Export PGA Indicator

		public ZString ExportAMSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportAMSIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, x => x.Details.CD_AMSIndicator);
			}
		}
		CachedProperty<ZString> exportAMSIndicator;

		public ZString ExportATFIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportATFIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, x => x.CD_ATFIndicator);
			}
		}
		CachedProperty<ZString> exportATFIndicator;

		public ZString ExportDEAIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportDEAIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, x => x.CD_DEAIndicator);
			}
		}
		CachedProperty<ZString> exportDEAIndicator;

		public ZString ExportEPAIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportPSTIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, x => x.CD_PSTIndicator);
			}
		}
		CachedProperty<ZString> exportPSTIndicator;

		public ZString ExportFWSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportFWSIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, x => x.Details.CD_FWSIndicator);
			}
		}
		CachedProperty<ZString> exportFWSIndicator;

		public ZString ExportNMFSIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportNMFSHMSIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, x => x.Details.CD_NMFSHMSIndicator);
			}
		}
		CachedProperty<ZString> exportNMFSHMSIndicator;

		public ZString ExportTTBIndicator
		{
			get
			{
				return GetEffectiveValueFromAllPivots(ref exportTTBIndicator, new ZString[] { ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.SHB }, (x) => x.CD_TTBIndicator);
			}
		}
		CachedProperty<ZString> exportTTBIndicator;

		#endregion

		#region Business Object Overrides

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(PartUnits.ToArray());
				result.AddRange(GetUSPivots());
				return result.ToArray();
			}
		}

		#endregion

		public CusClassPartPivot[] GetUSPivots() => GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.UnitedStates);

		[ChildEditable(true)]
		public new CusClassPartPivotCollection PivotsForBinding => (CusClassPartPivotCollection)base.PivotsForBinding;

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection(this);

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.UnitedStates);

		protected override ZQuery PivotFilter
		{
			get
			{
				ZQuery result = new ZQuery(CusClassPartPivotSchema.CI_OP, PK);
				result.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
				return result;
			}
		}

		protected override bool DoesPartMatchPivotForInactiveCheckCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = true;
			var pivotQuery = new ZQuery(CusClassPartPivotSchema.CI_OP, PK);
			pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
			var pivots = Factory.Load<CusClassPartPivot>(pivotQuery);
			if (pivots.Length > 1)
			{
				result = false;
			}
			else if (pivots.Length == 1)
			{
				var pivot = pivots[0];
				var declaration = invoiceLine.Declaration;
				var countryCode = declaration == null ? ZString.Empty : declaration.CountryCode;
				result = pivot.CI_RN_NKCountry == countryCode && pivot.CI_ChildType == invoiceLine.GetPartPivotType();
			}
			return result;
		}
	}
}
