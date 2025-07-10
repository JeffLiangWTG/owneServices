using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.TW.Business
{
	public class DocPkgPackageCollection : DocumentWrapperCollection
	{
		internal static readonly string SymbolPlus = "+";

		public DocPkgPackageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DocPkgPackageCollection(PkgPackageCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocPkgPackage this[int index] => (DocPkgPackage)Elements[index];

		internal ZString PackTypeSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(PackTypeSummaryInfo);

		internal ZString PackTypeDescriptionSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionaryForPrefix(PackDescriptionSummaryInfo, SymbolPlus);

		internal ZString QuantitySummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(QuantitySummaryInfo, MaxPackedQtyDecimalPlace);

		internal ZString NetWeightSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(NetWeightSummaryInfo, MaxPackedNetWeightDecimalPlace);

		internal ZString GrossWeightSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(GrossWeightSummaryInfo, MaxPackedGrossWeightDecimalPlace);

		internal ZString VolumeSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(VolumeSummaryInfo);

		internal IDictionary<ZString, ZDecimal> PackTypeSummaryInfo { get; set; }

		internal IDictionary<ZString, ZDecimal> PackDescriptionSummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> QuantitySummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> NetWeightSummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> GrossWeightSummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> VolumeSummaryInfo { get; set; }

		internal ZInt MaxPackedQtyDecimalPlace
		{
			get => maxPackedQtyDecimalPlace;
			set
			{
				maxPackedQtyDecimalPlace = value;
				this.Cast<DocPkgPackage>().ForEach(package => package.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>().ForEach(p => p.PackedQtyDecimalPlace = value));
			}
		}
		ZInt maxPackedQtyDecimalPlace;

		internal ZInt MaxPackedNetWeightDecimalPlace
		{
			get => maxPackedNetWeightDecimalPlace;
			set
			{
				maxPackedNetWeightDecimalPlace = value;
				this.Cast<DocPkgPackage>().ForEach(package =>
				{
					package.MaxPackedNetWeightDecimalPlace = value;
					package.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>().ForEach(p => p.NetWeightDecimalPlace = value);
				});
			}
		}
		ZInt maxPackedNetWeightDecimalPlace;

		internal ZInt MaxPackedGrossWeightDecimalPlace
		{
			get => maxPackedGrossWeightDecimalPlace;
			set
			{
				maxPackedGrossWeightDecimalPlace = value;
				this.Cast<DocPkgPackage>().ForEach(package => package.MaxPackedGrossWeightDecimalPlace = value);
			}
		}
		ZInt maxPackedGrossWeightDecimalPlace;

		internal ZInt MaxPackedVolumeDecimalPlace
		{
			get => maxPackedVolumeDecimalPlace;
			set
			{
				maxPackedVolumeDecimalPlace = value;
				this.Cast<DocPkgPackage>().ForEach(package => package.MaxPackedVolumeDecimalPlace = value);
			}
		}
		ZInt maxPackedVolumeDecimalPlace;

		IEnumerable<(ZString PackType, int PackageQty)> PackCollectionWithStar => packCollectionWithStar ??= this.Cast<DocPkgPackage>().Where(c => c.MarksAndNumbers.StartsWith(DocCusPackingList.SymbolStar)).GroupBy(r => r.PackType).Select(y => (PackType: y.Key, PackageQty: y.Sum(c => c.PackageQty)));
		IEnumerable<(ZString PackType, int PackageQty)> packCollectionWithStar;

		IEnumerable<(ZString PackType, int PackageQty)> PackCollectionWithoutStar => packCollectionWithoutStar ??= this.Cast<DocPkgPackage>().Where(c => !c.MarksAndNumbers.StartsWith(DocCusPackingList.SymbolStar)).GroupBy(r => r.PackType).Select(y => (PackType: y.Key, PackageQty: y.Sum(c => c.PackageQty)));
		IEnumerable<(ZString PackType, int PackageQty)> packCollectionWithoutStar;

		internal void SetPackingSummaries(ZString packageDescription)
		{
			PackTypeSummaryInfo = new Dictionary<ZString, ZDecimal>();
			PackDescriptionSummaryInfo = new Dictionary<ZString, ZDecimal>();
			QuantitySummaryInfo = new Dictionary<ZString, ZDecimal>();
			NetWeightSummaryInfo = new Dictionary<ZString, ZDecimal>();
			GrossWeightSummaryInfo = new Dictionary<ZString, ZDecimal>();
			VolumeSummaryInfo = new Dictionary<ZString, ZDecimal>();

			foreach (DocPkgPackage package in this)
			{
				DocumentWrapperHelper.AddOrUpdateDictionary(GrossWeightSummaryInfo, package.WeightUQ, package.Weight);
				if (!package.PackNoInfo.StartsWith(DocCusPackingList.SymbolStar))
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(VolumeSummaryInfo, package.VolumeUQInfo, package.Volume);
					if (package.Package?.NetWeightSpecifiedOnPackItems ?? false)
					{
						package.PackedItemRelations.ForEach(x =>
						{
							DocumentWrapperHelper.AddOrUpdateDictionary(NetWeightSummaryInfo, x.NetWeightUQ, x.NetWeight);
							DocumentWrapperHelper.AddOrUpdateDictionary(QuantitySummaryInfo, x.PackableUQ, x.PackedQty);
						});
					}
					else
					{
						DocumentWrapperHelper.AddOrUpdateDictionary(NetWeightSummaryInfo, package.WeightUQ, package.NetWeight);
						package.PackedItemRelations.ForEach(x => DocumentWrapperHelper.AddOrUpdateDictionary(QuantitySummaryInfo, x.PackableUQ, x.PackedQty));
					}
				}
			}
			SetPackTypeSummaries(packageDescription);
		}

		void SetPackTypeSummaries(ZString packageDescription)
		{
			var packCollectionWithStarCount = PackCollectionWithStar.Count();
			var packCollectionWithoutStarCount = PackCollectionWithoutStar.Count();
			SetPackTypeSummaryInfo(packCollectionWithStarCount, packCollectionWithoutStarCount);
			if (!packageDescription.IsEmpty)
			{
				SetPackDescriptionSummaryInfoFromPackageDescription(packageDescription);
			}
			else
			{
				SetPackDescriptionSummaryInfo(packCollectionWithStarCount, packCollectionWithoutStarCount);
			}
		}

		void SetPackTypeSummaryInfo(int packCollectionWithStarCount, int packCollectionWithoutStarCount)
		{
			if (packCollectionWithStarCount == 0)
			{
				if (packCollectionWithoutStarCount > 1)
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(PackTypeSummaryInfo, Core.Constants.PkgUnit.Package, PackCollectionWithoutStar.Sum(c => c.PackageQty));
				}
				else if (packCollectionWithoutStarCount == 1)
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(PackTypeSummaryInfo, PackCollectionWithoutStar.ElementAt(0).PackType, PackCollectionWithoutStar.ElementAt(0).PackageQty);
				}
			}
			else if (packCollectionWithStarCount == 1)
			{
				DocumentWrapperHelper.AddOrUpdateDictionary(PackTypeSummaryInfo, PackCollectionWithStar.ElementAt(0).PackType, PackCollectionWithStar.ElementAt(0).PackageQty);
			}
			else
			{
				DocumentWrapperHelper.AddOrUpdateDictionary(PackTypeSummaryInfo, Core.Constants.PkgUnit.Package, PackCollectionWithStar.Sum(c => c.PackageQty));
			}
		}

		void SetPackDescriptionSummaryInfoFromPackageDescription(ZString packageDescription)
		{
			var packageArray = Regex.Replace(packageDescription, @"[(=)]", string.Empty).Split(SymbolPlus.ToCharArray());
			if (packageArray.All(packType => Regex.IsMatch(packType, @"^[0-9]+ *[A-Z,a-z]+$")))
			{
				foreach (var package in packageArray)
				{
					var packType = Regex.Replace(package, @"[^A-Z,a-z]", string.Empty);
					var packageQty = ZDecimal.ParseSafe(Regex.Replace(package, @"[^\d]", string.Empty), 0);
					DocumentWrapperHelper.AddOrUpdateDictionary(PackDescriptionSummaryInfo, packType, packageQty);
				}
			}
		}

		void SetPackDescriptionSummaryInfo(int packCollectionWithStarCount, int packCollectionWithoutStarCount)
		{
			if ((packCollectionWithStarCount == 0 && packCollectionWithoutStarCount > 1) || packCollectionWithStarCount == 1)
			{
				foreach (var pack in PackCollectionWithoutStar)
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(PackDescriptionSummaryInfo, pack.PackType, pack.PackageQty);
				}
			}
			else if (packCollectionWithStarCount > 1)
			{
				foreach (var pack in PackCollectionWithStar)
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(PackDescriptionSummaryInfo, pack.PackType, pack.PackageQty);
				}
			}
		}
	}
}
