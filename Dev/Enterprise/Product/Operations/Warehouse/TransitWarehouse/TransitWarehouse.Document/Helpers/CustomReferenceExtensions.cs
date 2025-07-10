using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Document
{
	public static class CustomReferenceExtensions
	{
		public static IEnumerable<CusEntryNumber> GetCustomsReleaseNumbersFallbackCustomsNumbers(this ICustomsReferenceCollection references)
		{
			var result = GetCusEntryNumByType(references.Cast<CusEntryNumber>(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber).OrderBy(c => c.CE_EntryNum);
			if (!result.Any())
			{
				result = GetCusEntryNumByType(references.Cast<CusEntryNumber>(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber).OrderBy(c => c.CE_EntryNum);
			}
			return result;
		}

		public static bool HasCustomsReleaseNumber(this WhsItemReceiveConsignment rcn) => GetCusEntryNumByType(rcn.CustomsReferenceNumbers.Cast<CusEntryNumber>(), TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber).Any();

		public static (ZString RefType, ZString RefCode) GetCustomsReferenceNumbersRefAndCode(this WhsItemReceiveConsignment rcn, bool includingPackageLevel)
		{
			var refType = "";
			var refCode = "";

			var sourceTypePairs = rcn.CustomsReferenceNumbers.GetCustomsReleaseNumbersFallbackCustomsNumbers().GetSourceTypeReferencePair(containsEmptySource: false);

			if (includingPackageLevel)
			{
				var pkgSourceTypePairs = rcn.PackageStates.SelectMany(p => p.Package.CustomsReferenceNumbers.GetCustomsReleaseNumbersFallbackCustomsNumbers()).GetSourceTypeReferencePair(containsEmptySource: true).Distinct().ToList();
				pkgSourceTypePairs.RemoveAll(p => p.SourceType.IsEmpty && sourceTypePairs.Select(rcnp => rcnp.EntryNum).Contains(p.EntryNum));
				pkgSourceTypePairs = pkgSourceTypePairs.Select(pair =>
				{
					var newType = pair.SourceType;
					if (newType.IsEmpty)
					{
						newType = sourceTypePairs.Any() ? sourceTypePairs.First().SourceType : (ZString)TransitDocDataConstants.AccompanyingDocumentTypes.DefaultDocumentType;
					}
					return (SourceType: newType, pair.EntryNum);
				}).OrderBy(p => p.SourceType).ThenBy(p => p.EntryNum).ToList();
				sourceTypePairs = sourceTypePairs.Concat(pkgSourceTypePairs).Distinct().ToList();
			}

			refType = string.Join(", ", sourceTypePairs.Select(n => n.SourceType));
			refCode = string.Join(", ", sourceTypePairs.Select(n => n.EntryNum));
			return (refType, refCode);
		}

		static List<(ZString SourceType, ZString EntryNum)> GetSourceTypeReferencePair(this IEnumerable<CusEntryNumber> cusEntryNumbers, bool containsEmptySource)
			=> cusEntryNumbers.Select(n => (SourceType: n.GetAddOnValues(a => a.XV_Name == "SourceType").FirstOrDefault()?.XV_Data ?? string.Empty, EntryNum: n.CE_EntryNum))
								.Where(p => containsEmptySource || !p.SourceType.IsEmpty)
								.OrderBy(p => p.SourceType)
								.ThenBy(p => p.EntryNum)
								.ToList();

		public static ZString GetTempStorageDeclaration(this ICustomsReferenceCollection references) => GetCusEntryNumByType(references.Cast<CusEntryNumber>(), TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration).FirstOrDefault()?.CE_EntryNum ?? ZString.Empty;

		static IEnumerable<CusEntryNumber> GetCusEntryNumByType(IEnumerable<CusEntryNumber> references, string entryType) => references.Where(r => r.CE_EntryType == entryType && !r.CE_EntryNum.IsEmpty);

		public static ZString GetForwardingShipmentDescription(this ICusEntryNumAdditionalReferenceCollection references)
		{
			var description = ZString.Empty;
			if (references != null)
			{
				description = GetCusEntryNumByType(references.Cast<CusEntryNumber>(), WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription).FirstOrDefault()?.CE_EntryNum ?? ZString.Empty;
			}
			return description;
		}
	}
}
