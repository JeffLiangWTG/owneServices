#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class NZOrgSupplierPartDataLoad : GlobalOrgSupplierPartDataLoad, Integration.Customs.NZ.INZCusOrgSupplierPartDataLoad
	{
		protected override void AddDataToPivot(BaseCusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			base.AddDataToPivot(pivot, partsData);
			if (pivot is CusClassPartPivot nzPivot)
			{
				nzPivot.CI_RN_NKCountryOfOrigin = partsData.PartOrigin.Left(pivot.CI_RN_NKCountryOfOriginInfo.MaxLength);
			}
		}

		#region GetClassificationPK

		protected override ZString GetClassificationType(PartsDataToLoad dataToLoad)
		{
			return ClassTypeBoth;
		}

		protected override ZGuid GetClassificationPK(string classificationCode, string classificationType)
		{
			var filter = new ZQuery(CusClassificationSchema.CC_LookupCode, classificationCode);
			filter.AddToFilter(CusClassificationSchema.CC_ClassificationType, ClassTypeBoth);
			filter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.NewZealand);
			var classification = Factory.LoadTop1<CusClassification>(filter);

			return classification?.PK ?? base.GetClassificationPK(classificationCode, ClassTypeBoth);
		}

		#endregion

		#region GetUQFromTariff

		protected override void SetRecordUQFromTariff(PartsDataToLoad record)
		{
			var partClassification = record.ClassificationLookup;
			if (!partClassification.IsEmpty)
			{
				var classificationType = GetClassificationType(record);
				var classification = GetClassification(partClassification, classificationType) as CusClassification;
				if (classification?.Tariff != null)
				{
					record.PartUQ = UniversalTariffHelper.GetStatisticalUnit(classification.Tariff).ToUpper();
				}
			}
		}

		#endregion

		#region FindExistingPivot

		protected override BaseCusClassPartPivot FindExistingPivot(Customs.Business.OrgSupplierPart part, ZString pivotType)
		{
			// A special condition for when the part has both HTE and HTI entries.
			// In this case, the line type will be respected and just that entry will be updated.
			// To update both entries the file will need two lines.  A line for type=HTE and another for HTI.
			// Should the line type not be HTE or HTI then an error will be reported.
			// Otherwise the line type will be ignored and the entry type will be set to HTB.

			BaseCusClassPartPivot pivot = null;

			var pivots = part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.NewZealand).GetNonDeletedPivots();
			var pivotCount = pivots.Length;

			if (pivotCount == 1)
			{
				pivot = pivots.FirstOrDefault();
				pivot.CI_ChildType = PivotTypeBoth;
			}
			else if (pivotCount == 2 && IsEffectiveHTBTypePart(pivots))
			{
				pivot = base.FindExistingPivot(part, pivotType);
				if (pivot == null)
				{
					throw new ArgumentException(Res.GetString("414AF0D5-9D02-4A14-8857-C1164649DA69", "Part has HTE and HTI entries which must be updated separately."));
				}
			}
			else if (pivotCount > 0)
			{
				var types = string.Join(",", pivots.Select(x => x.CI_ChildType));
				throw new ArgumentException(Res.GetString("45AB6BB0-87C3-4890-B112-B1630EF12360", "Part has unexpected entries: {0}", types));
			}

			return pivot;
		}

		protected override BaseCusClassPartPivot CreatePivot(Customs.Business.OrgSupplierPart part, ZString requestedPivotType)
		{
			var pivotType = PivotTypeBoth;

			var pivots = part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.NewZealand).GetNonDeletedPivots();
			if (pivots.Length == 1)
			{
				var existingPivotType = pivots[0].CI_ChildType;
				if (existingPivotType == ClassificationTypeList.Codes.HTE)
				{
					pivotType = ClassificationTypeList.Codes.HTI;
				}
				else if (existingPivotType == ClassificationTypeList.Codes.HTI)
				{
					pivotType = ClassificationTypeList.Codes.HTE;
				}
			}

			return base.CreatePivot(part, pivotType);
		}

		bool IsEffectiveHTBTypePart(IEnumerable<BaseCusClassPartPivot> pivots)
		{
			return pivots
				.Where(x => x.CI_ChildType == ClassificationTypeList.Codes.HTE || x.CI_ChildType == ClassificationTypeList.Codes.HTI)
				.DistinctBy(x => x.CI_ChildType)
				.Count() == 2;
		}

		#endregion

		const string ClassTypeBoth = CusClassification.ClassificationType.Both;
		const string PivotTypeBoth = ClassificationTypeList.Codes.HTB;
	}
}
