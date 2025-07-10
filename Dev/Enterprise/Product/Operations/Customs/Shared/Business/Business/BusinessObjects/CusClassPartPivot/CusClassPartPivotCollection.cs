using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusClassPartPivotCollection<out TCusClassPartPivot> : IBusinessObjectCollection<TCusClassPartPivot>
		where TCusClassPartPivot : BaseCusClassPartPivot
	{
		new TCusClassPartPivot this[int index] { get; }
		new TCusClassPartPivot AddNew();
		TCusClassPartPivot GetImportMatch(ZGuid importerPK, ZGuid supplierPK, params CusAttributeFilter.AttributeValue[] attribs);
		TCusClassPartPivot GetImportMatch(ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate, params CusAttributeFilter.AttributeValue[] attribs);
		TCusClassPartPivot GetExportMatch(bool isSchedB, ZGuid importerPK, ZGuid supplierPK);
		TCusClassPartPivot GetExportMatch(bool isSchedB, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate);
		TCusClassPartPivot GetMatch(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs);
		TCusClassPartPivot GetMatch(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs);
		TCusClassPartPivot GetAnotherHTIPivotWithAttributeSetupFor(CusAttributeFilter.AttributeFilterName filterName, BaseCusClassPartPivot pivotToExclude);
		TCusClassPartPivot[] GetMatchesIgnoringAttributes(string typeOfPivot, ZGuid importerPK, ZGuid supplierPK);
		TCusClassPartPivot[] GetMatchesIgnoringAttributes(IEnumerable<BaseCusClassPartPivot> pivots, string typeOfPivot, ZGuid importerPK, ZGuid supplierPK);
		ZString CountryCode { get; }
	}

	public class CusClassPartPivotCollection<TCusClassPartPivot> : DependentBusinessObjectCollection<TCusClassPartPivot, BusinessObject>, ICusClassPartPivotCollection<TCusClassPartPivot>
		where TCusClassPartPivot : BaseCusClassPartPivot
	{
		public CusClassPartPivotCollection(OrgSupplierPart part, string countryCode)
			: base(part, GetPartPivotFilter(countryCode))
		{
			this.countryCode = countryCode;
			this.fkSchemaColumnInDependent = CusClassPartPivotSchema.CI_OP;
			this.classificationTypeProvider = ClassificationTypeProvider.GetProviderFor(countryCode);
		}

		public static ZQuery GetPartPivotFilter(params string[] countryCode)
		{
			var countries = countryCode.WhereNotNull();
			var result = new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, null);
			if (countries.Any())
			{
				result.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, countries);
			}
			return result;
		}

		public CusClassPartPivotCollection(BaseCusClassPartPivot pivot, ZString countryCode)
			: base(pivot, GetPivotChildFilter(pivot.CI_OP, pivot.PK, countryCode))
		{
			this.pivot = pivot;
			this.countryCode = countryCode;
			this.fkSchemaColumnInDependent = CusClassPartPivotSchema.CI_CI_Parent;
			this.classificationTypeProvider = ClassificationTypeProvider.GetProviderFor(countryCode);
		}

		static ZQuery GetPivotChildFilter(ZGuid partPK, ZGuid pivotPK, ZString countryCode)
		{
			var result = new ZQuery(CusClassPartPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotPK);
			result.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, countryCode);
			if (!partPK.IsEmpty)
			{
				result.AddToFilter(CusClassPartPivotSchema.CI_OP, partPK);
			}
			return result;
		}

		readonly BaseCusClassPartPivot pivot;
		readonly IClassificationTypeProvider classificationTypeProvider;
		public readonly ZString countryCode;

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return fkSchemaColumnInDependent; }
		}
		readonly SchemaGuidColumn fkSchemaColumnInDependent;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (BaseCusClassPartPivot)child;
			if (!countryCode.IsEmpty)
			{
				newElement.CI_RN_NKCountry = countryCode;
			}

			if (pivot != null)
			{
				newElement.CI_OP = pivot.CI_OP;
				newElement.CI_ChildListOrder = (ZByte)(Count + 1);
			}
		}

		public BaseCusClassPartPivot[] GetNonDeletedPivots() => this.OfType<BaseCusClassPartPivot>().GetNonDeletedPivots().ToArray();

		ZString HTICode => classificationTypeProvider.HTICode;
		ZString HTECode => classificationTypeProvider.HTECode;
		ZString SHBCode => classificationTypeProvider.SHBCode;
		ZString HTBCode => classificationTypeProvider.HTBCode;

		Dictionary<CusAttributeFilter.AttributeFilterName, List<BaseCusClassPartPivot>> AttributeFilterNameSpecifiedForHTI
		{
			get
			{
				if (attributeFilterNameSpecifiedForHTICached == null)
				{
					attributeFilterNameSpecifiedForHTICached = new CachedProperty<Dictionary<CusAttributeFilter.AttributeFilterName, List<BaseCusClassPartPivot>>>(Factory, delegate
					{
						var pivotsWithAttrib1Setup = new List<BaseCusClassPartPivot>();
						var pivotsWithAttrib2Setup = new List<BaseCusClassPartPivot>();
						var pivotsWithAttrib3Setup = new List<BaseCusClassPartPivot>();
						var pivotsWithSerialNumberSetup = new List<BaseCusClassPartPivot>();
						foreach (var partPivot in GetNonDeletedPivots())
						{
							if (partPivot.IsImportClassification)
							{
								if (partPivot.Attributes1.Count > 0)
								{
									pivotsWithAttrib1Setup.Add(partPivot);
								}
								if (partPivot.Attributes2.Count > 0)
								{
									pivotsWithAttrib2Setup.Add(partPivot);
								}
								if (partPivot.Attributes3.Count > 0)
								{
									pivotsWithAttrib3Setup.Add(partPivot);
								}
							}
						}
						var result = new Dictionary<CusAttributeFilter.AttributeFilterName, List<BaseCusClassPartPivot>>();
						result.Add(CusAttributeFilter.AttributeFilterName.AT1, pivotsWithAttrib1Setup);
						result.Add(CusAttributeFilter.AttributeFilterName.AT2, pivotsWithAttrib2Setup);
						result.Add(CusAttributeFilter.AttributeFilterName.AT3, pivotsWithAttrib3Setup);
						return result;
					});
				}
				return attributeFilterNameSpecifiedForHTICached.Value;
			}
		}

		CachedProperty<Dictionary<CusAttributeFilter.AttributeFilterName, List<BaseCusClassPartPivot>>> attributeFilterNameSpecifiedForHTICached;

		public IEnumerator<TCusClassPartPivot> GetEnumerator() => Elements.Cast<TCusClassPartPivot>().GetEnumerator();

		public TCusClassPartPivot GetImportMatch(ZGuid importerPK, ZGuid supplierPK, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return ((ICusClassPartPivotCollection<TCusClassPartPivot>)this).GetMatch(HTICode, importerPK, supplierPK, false, attribs);
		}

		public TCusClassPartPivot GetImportMatch(ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return ((ICusClassPartPivotCollection<TCusClassPartPivot>)this).GetMatch(HTICode, importerPK, supplierPK, expiringDate, false, attribs);
		}

		public TCusClassPartPivot GetExportMatch(bool isSchedB, ZGuid importerPK, ZGuid supplierPK)
		{
			var typeOfPivot = isSchedB ? SHBCode : HTECode;
			return ((ICusClassPartPivotCollection<TCusClassPartPivot>)this).GetMatch(typeOfPivot, importerPK, supplierPK, true);
		}

		public TCusClassPartPivot GetExportMatch(bool isSchedB, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate)
		{
			var typeOfPivot = isSchedB ? SHBCode : HTECode;
			return ((ICusClassPartPivotCollection<TCusClassPartPivot>)this).GetMatch(typeOfPivot, importerPK, supplierPK, expiringDate, true);
		}

		public TCusClassPartPivot GetMatch(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return (TCusClassPartPivot)new ClassPartPivotMatcher(GetNonDeletedPivots(), HTBCode).GetMatch(typeOfPivot, importerPK, supplierPK, expiringDate, ignoreAttribs, attribs);
		}

		public TCusClassPartPivot GetMatch(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return (TCusClassPartPivot)new ClassPartPivotMatcher(GetNonDeletedPivots(), HTBCode).GetMatch(typeOfPivot, importerPK, supplierPK, ignoreAttribs, attribs);
		}

		public TCusClassPartPivot GetAnotherHTIPivotWithAttributeSetupFor(CusAttributeFilter.AttributeFilterName filterName, BaseCusClassPartPivot pivotToExclude)
		{
			TCusClassPartPivot result = null;
			List<BaseCusClassPartPivot> list;
			if (AttributeFilterNameSpecifiedForHTI.TryGetValue(filterName, out list) && list.Count > 0)
			{
				var orgPK = pivotToExclude.CI_OH;
				var isOrgPKEmpty = orgPK.IsEmpty;
				foreach (var otherPivot in list)
				{
					if (otherPivot != pivotToExclude && (isOrgPKEmpty || otherPivot.CI_OH.IsEmpty || otherPivot.CI_OH == orgPK))
					{
						result = (TCusClassPartPivot)otherPivot;
						break;
					}
				}
			}
			return result;
		}

		public TCusClassPartPivot[] GetMatchesIgnoringAttributes(string typeOfPivot, ZGuid importerPK, ZGuid supplierPK)
		{
			return Array.ConvertAll(new ClassPartPivotMatcher(GetNonDeletedPivots(), HTBCode).GetMatchesIgnoringAttributes(typeOfPivot, importerPK, supplierPK), x => (TCusClassPartPivot)x);
		}

		public TCusClassPartPivot[] GetMatchesIgnoringAttributes(IEnumerable<BaseCusClassPartPivot> pivots, string typeOfPivot, ZGuid importerPK, ZGuid supplierPK)
		{
			return Array.ConvertAll(new ClassPartPivotMatcher(pivots.Where(p => !p.IsDeleted), HTBCode).GetMatchesIgnoringAttributes(typeOfPivot, importerPK, supplierPK), x => (TCusClassPartPivot)x);
		}

		ZString ICusClassPartPivotCollection<TCusClassPartPivot>.CountryCode => countryCode;
	}
}
