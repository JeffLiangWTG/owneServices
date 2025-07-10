using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Customs.TW.Business
{
	public class DocCusPackingList : DocBaseWrapper
	{
		readonly JobDeclaration jobDeclaration;
		readonly CodeDescriptionPairList codeDescriptionPairList;
		internal static readonly string SymbolStar = "*";

		public CusPackingList PackingList => (CusPackingList)WrappedObject;

		public DocCusPackingList(CusPackingList cusPackingList, BusinessObjectFactory factoryToWrap = null)
			: base(cusPackingList, factoryToWrap ?? cusPackingList.Factory)
		{
			jobDeclaration = PackingList.Declaration;
			codeDescriptionPairList = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory, "EN");
			SetPackedDecimalPlace();
		}

		public static DocCusPackingList New(CusPackingList cusPackingList, BusinessObjectFactory factoryToWrap)
		{
			return new DocCusPackingList(cusPackingList, factoryToWrap);
		}

		public DocumentaryAddressDetailsWrapper SellerAddressData => Factory.GetValue(ref sellerAddressDataCached, GetSellerAddressDataCore);
		CachedProperty<DocumentaryAddressDetailsWrapper> sellerAddressDataCached;

		protected virtual DocumentaryAddressDetailsWrapper GetSellerAddressDataCore() => Declaration.SupplierAddressData;

		public DocJobDocAddress SellerDocAddress => Factory.GetValue(ref sellerDocAddressCached, GetSellerDocAddressCore);
		CachedProperty<DocJobDocAddress> sellerDocAddressCached;

		protected virtual DocJobDocAddress GetSellerDocAddressCore() => Declaration.SupplierJobDocAddress;

		public DocumentaryAddressDetailsWrapper BuyerAddressData => Factory.GetValue(ref buyerAddressDataCached, GetBuyerAddressDataCore);
		CachedProperty<DocumentaryAddressDetailsWrapper> buyerAddressDataCached;

		protected virtual DocumentaryAddressDetailsWrapper GetBuyerAddressDataCore() => Declaration.ImporterAddressData;

		public DocJobDocAddress BuyerDocAddress => Factory.GetValue(ref buyerDocAddressCached, GetBuyerDocAddressCore);
		CachedProperty<DocJobDocAddress> buyerDocAddressCached;

		protected virtual DocJobDocAddress GetBuyerDocAddressCore() => Declaration.ImporterJobDocAddress;

		public ZString MarksAndNumbers => Factory.GetValue(ref marksAndNumbersCached, GetMarksAndNumbersCore);
		CachedProperty<ZString> marksAndNumbersCached;

		protected virtual ZString GetMarksAndNumbersCore() => new PackingWeightListDocumentWrapper(jobDeclaration, Factory).MarksAndNumbers;

		public ZString PackNumber => PackingList.CUL_PackingListNumber;

		public ZString Remarks => PackingList.CUL_Remarks;

		public ZString PackDate => PackingList.CUL_PackingListDate.ToISO8601ShortDateString();

		public ZString GoodsDescription => PackingList.CUL_Description;

		#region Custom Fields
		public ZString CustomAttribute1 => PackingList.CUL_CustomAttribute1;

		public ZString CustomAttribute2 => PackingList.CUL_CustomAttribute2;

		public ZBool CustomFlag1 => PackingList.CUL_CustomFlag1;

		public ZBool CustomFlag2 => PackingList.CUL_CustomFlag2;

		public ZDateTime CustomDate1 => PackingList.CUL_CustomDate1;

		public ZDateTime CustomDate2 => PackingList.CUL_CustomDate2;

		public ZDecimal CustomDecimal1 => PackingList.CUL_CustomDecimal1;

		public ZDecimal CustomDecimal2 => PackingList.CUL_CustomDecimal2;
		#endregion

		public ZString PackTypeSummary
		{
			get
			{
				var result = new ZStringBuilder();
				var packTypeSummary = Packages.PackTypeSummary;
				if (!packTypeSummary.IsEmpty)
				{
					result.AppendLine(packTypeSummary);
				}

				var packTypeDescriptionSummary = Packages.PackTypeDescriptionSummary;
				if (!packTypeDescriptionSummary.IsEmpty)
				{
					result.AppendLine(string.Format("(={0})", packTypeDescriptionSummary));
				}

				return DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(result.ToString().Trim(), 8);
			}
		}

		public ZString QuantitySummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(Packages.QuantitySummary, 10);

		public ZString NetWeightSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(Packages.NetWeightSummary, 8);

		public ZString GrossWeightSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(Packages.GrossWeightSummary, 8);

		public ZString VolumeSummary => DocumentWrapperHelper.GetPackingListSummaryLineWithTailFlag(Packages.VolumeSummary, 10);

		public ZString SayTotalDescription
		{
			get
			{
				var result = new ZStringBuilder(GetSayTotalDescriptionFromSummaryInfo(Packages.PackTypeSummaryInfo, true));
				var sayTotalDescriptionFromSummaryInfo = GetSayTotalDescriptionFromSummaryInfo(Packages.PackDescriptionSummaryInfo, false);
				if (!sayTotalDescriptionFromSummaryInfo.IsEmpty)
				{
					result.AppendLine(string.Format("(={0})", sayTotalDescriptionFromSummaryInfo));
				}
				return result.ToString().Trim();
			}
		}

		ZString GetSayTotalDescriptionFromSummaryInfo(IDictionary<ZString, ZDecimal> summaryInfo, bool showSayTotalText)
		{
			var summary = summaryInfo.Where(info => !info.Key.IsEmpty);
			var typeCount = summary.Count();
			var result = ZString.Empty;
			var count = 0;
			foreach (var keyValuePair in summary)
			{
				var code = keyValuePair.Key;
				var quantity = (int)keyValuePair.Value;
				var description = codeDescriptionPairList.GetDescriptionFromCode(code);
				if (description == null)
				{
					continue;
				}

				++count;
				if (count == 1 && showSayTotalText)
				{
					result += (NoResString)"SAY TOTAL ";
				}
				if (count > 1 && count == typeCount)
				{
					result += " AND ";
				}
				else if (count > 1)
				{
					result += ", ";
				}

				description = description.ToUpper(CultureInfo.InvariantCulture);
				var quantityInEnglish = Enterprise.DocumentEngine.MacroValueProviders.Utilities.NumberToString_EN.ConvertNumberToWords(quantity).ToUpper(CultureInfo.InvariantCulture);
				if (quantity > 1)
				{
					description = Grammar.Instance.Pluralize(description).ToUpper(CultureInfo.InvariantCulture);
				}

				result += string.Format(CultureInfo.InvariantCulture, "{0} ({1}) {2}", quantityInEnglish, quantity, description);
			}

			if (!result.IsEmpty)
			{
				result += " ONLY.";
			}

			return result;
		}

		public DeclarationWrapper Declaration => declaration ??= DeclarationWrapper.New(jobDeclaration, Factory);
		DeclarationWrapper declaration;

		#region Collection
		public DocPkgPackageCollection Packages
		{
			get
			{
				if (packages == null)
				{
					var packagesCollection = new DocPkgPackageCollection(PackingList.PackageJob?.Packages, Factory);
					packagesCollection.SetPackingSummaries(PackingList.CUL_PackageDescription);
					packages = packagesCollection;
				}
				return packages;
			}
		}
		DocPkgPackageCollection packages;
		#endregion

		void SetPackedDecimalPlace()
		{
			Packages.MaxPackedQtyDecimalPlace = MaxPackedQtyDecimalPlace;
			Packages.MaxPackedNetWeightDecimalPlace = MaxNetWeightDecimalPlace;
			Packages.MaxPackedGrossWeightDecimalPlace = MaxGrossWeightDecimalPlace;
			Packages.MaxPackedVolumeDecimalPlace = MaxVolumeDecimalPlace;
		}

		public ZInt MaxPackedQtyDecimalPlace => Factory.GetValue(ref maxPackedQtyDecimalPlaceCached, () =>
		{
			var resultDecimalPlace = Packages.Count <= 0 ? 0 : Packages.Cast<DocPkgPackage>().Max(c =>
			{
				var result = 0;
				if (c.PackedItemRelationsRelation.Count > 0)
				{
					result = c.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>().Max(p => p.PackedQtyDetailInfo.MaxDecimalPlace);
				}
				return result;
			});
			return resultDecimalPlace;
		});
		CachedProperty<ZInt> maxPackedQtyDecimalPlaceCached;

		public ZInt MaxNetWeightDecimalPlace => Factory.GetValue(ref maxNetWeightDecimalPlaceCached, () =>
		{
			return Packages.Count <= 0 ? 0 : Packages.Cast<DocPkgPackage>().Max(c =>
			{
				if (c.Package.NetWeightSpecifiedOnPackItems)
				{
					return c.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>().Max(x => x.NetWeightQtyDetailInfo.MaxDecimalPlace);
				}
				else
				{
					return c.PackedNetWeightDetailInfo.MaxDecimalPlace;
				}
			});
		});
		CachedProperty<ZInt> maxNetWeightDecimalPlaceCached;

		public ZInt MaxGrossWeightDecimalPlace => Factory.GetValue(ref maxGrossWeightDecimalPlaceCached, () => Packages.Count > 0 ? Packages.Cast<DocPkgPackage>().Max(c => c.PackedGrossWeightDetailInfo.MaxDecimalPlace) : 0);
		CachedProperty<ZInt> maxGrossWeightDecimalPlaceCached;

		public ZInt MaxVolumeDecimalPlace => Factory.GetValue(ref maxVolumeDecimalPlaceCached, () => Packages.Count > 0 ? Packages.Cast<DocPkgPackage>().Max(c => c.PackedVolumeDetailInfo.MaxDecimalPlace) : 0);
		CachedProperty<ZInt> maxVolumeDecimalPlaceCached;

		public BusinessObjectCollectionWrapper<PackableItemListSection> PackedItemListSections => PackedItemList?.Sections ?? new();

		PackagesItemListPrintWrapper packedItemList;
		protected PackagesItemListPrintWrapper PackedItemList
		{
			get
			{
				if (packedItemList == null)
				{
					packedItemList = new PackagesItemListPrintWrapper(Packages);
					packedItemList.ProcessData();
				}
				return packedItemList;
			}
		}

		public virtual ZString PortOfOriginName => Declaration.PortOfOriginName;

		public virtual ZString FinalDestinationName => Declaration.FinalDestinationName;

		public virtual ZString Transportation => Declaration.Transportation;

		public virtual DocOrganisation Notify => Declaration.Notify;

		public virtual IPartyDetails NotifyPartyDetails => Declaration.NotifyPartyDetails;
	}

	public class PackagesItemListPrintWrapper
	{
		public PackagesItemListPrintWrapper(DocPkgPackageCollection packages)
		{
			Sections = new BusinessObjectCollectionWrapper<PackableItemListSection>();
			packagesCollection = packages;
		}
		readonly DocPkgPackageCollection packagesCollection;

		PackableItemListSection CurrentSection
		{
			get
			{
				if (currentSection == null)
				{
					currentSection = new PackableItemListSection();
					Sections.Add(currentSection);
				}
				return currentSection;
			}
		}
		PackableItemListSection currentSection;

		internal BusinessObjectCollectionWrapper<PackableItemListSection> Sections { get; }

		void BreakCurrentSection()
		{
			currentSection = null;
		}

		internal void ProcessData()
		{
			var packages = packagesCollection.Cast<DocPkgPackage>().OrderBy(c => c.PackageSequence);
			foreach (var item in packages)
			{
				var isPackNoStartWithStar = item.PackNoInfo.StartsWith(DocCusPackingList.SymbolStar);
				var netWeightSpecifiedOnPackItems = item.Package.NetWeightSpecifiedOnPackItems;
				CurrentSection.SetDocPkgPackage(item, isPackNoStartWithStar);
				foreach (var packableitem in item.PackedItemRelationsRelation.Cast<DocCusPackageCusPackableItemRelation>())
				{
					CurrentSection.SetDocCusPackageCusPackableItemRelation(packableitem, netWeightSpecifiedOnPackItems, isPackNoStartWithStar);
					BreakCurrentSection();
				}
				BreakCurrentSection();
			}
		}
	}

	public class PackableItemListSection : NonPersistentBusinessObject
	{
		DocPkgPackage pkgPackage;
		DocCusPackageCusPackableItemRelation packableItemRelation;
		bool haveAnyItemNeight;
		bool isPackNoStartWithStar;

		internal void SetDocPkgPackage(DocPkgPackage pkgPackage, bool isPackNoStartWithStar)
		{
			this.pkgPackage = pkgPackage;
			this.isPackNoStartWithStar = isPackNoStartWithStar;
		}

		internal void SetDocCusPackageCusPackableItemRelation(DocCusPackageCusPackableItemRelation packableItemRelation, bool hasAnyItemNeight, bool isPackNoStartWithStar)
		{
			this.packableItemRelation = packableItemRelation;
			this.haveAnyItemNeight = hasAnyItemNeight;
			this.isPackNoStartWithStar = isPackNoStartWithStar;
		}

		public ZString Summary => pkgPackage?.Summary ?? ZString.Empty;

		public ZString PackNo => isPackNoStartWithStar ? ZString.Empty : (pkgPackage?.PackNoInfo ?? ZString.Empty);

		public ZString GoodsDescription => packableItemRelation?.GoodsDescription ?? ZString.Empty;

		public ZString Grouping => packableItemRelation?.Grouping ?? ZString.Empty;

		public ZString PackedQtyInfo => isPackNoStartWithStar ? ZString.Empty : (packableItemRelation?.PackedQtyInfo ?? ZString.Empty);

		public ZString NetWeightInfo => isPackNoStartWithStar ? ZString.Empty : (haveAnyItemNeight ? (packableItemRelation?.NetWeightInfo ?? ZString.Empty) : (pkgPackage?.NetWeightInfo ?? ZString.Empty));

		public ZString GrossWeightInfo => pkgPackage?.GrossWeightInfo ?? ZString.Empty;

		public ZString VolumeInfo => isPackNoStartWithStar ? ZString.Empty : pkgPackage?.VolumeInfo ?? ZString.Empty;

		public ZInt PackageSequence => pkgPackage?.PackageSequence ?? ZInt.Zero;
	}
}
