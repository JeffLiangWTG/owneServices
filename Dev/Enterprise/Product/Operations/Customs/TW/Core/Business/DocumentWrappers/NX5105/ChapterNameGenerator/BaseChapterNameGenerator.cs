using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	abstract class BaseChapterNameGenerator
	{
		protected BaseChapterNameGenerator(ImportCustomsDeclarationDocumentWrapper wrapper)
		{
			Argument.NotNull(wrapper, "wrapper");
			documentWrapper = wrapper;
			goodsItems = documentWrapper.GovernmentAgencyGoodsItems;
		}

		protected ImportCustomsDeclarationDocumentWrapper documentWrapper;

		protected IEnumerable<IGovernmentAgencyGoodsItem> goodsItems;

		#region SuppressResourceStringsCheckRegion
		protected class BranchName
		{
			public const string Branch11 = "分估一課一股";
			public const string Branch12 = "分估一課二股";
			public const string Branch13 = "分估一課三股";
			public const string Branch14 = "分估一課四股";
			public const string Branch21 = "分估二課一股";
			public const string Branch22 = "分估二課二股";
			public const string Branch23 = "分估二課三股";
			public const string Branch31 = "分估三課一股";
			public const string Branch32 = "分估三課二股";
			public const string Branch33 = "分估三課三股";
			public const string Branch1 = "分估一股";
			public const string Branch2 = "分估二股";
		}

		protected class FormName
		{
			public const string Over10000USD = "/大單";
			public const string AtMost10000USD = "/小單";
		}
		#endregion

		public virtual ZString GetTariffChapterName()
		{
			var result = ZString.Empty;
			var dic = new Dictionary<TariffChapter, ZDecimal>();

			foreach (var goodsItem in goodsItems)
			{
				var commodity = goodsItem.Commodity;
				var classificationId = commodity?.Classifications?.FirstOrDefault(x => x.IdentificationTypeCode == MessageConstants.IdentificationTypeCodes.HS)?.ID ?? ZString.Empty;
				if (ZInt.TryParse(classificationId.SubstringSafe(0, 4), out var prefix4))
				{
					var chapter = GetTariffChapter(prefix4, classificationId);
					if (chapter != TariffChapter.None)
					{
						var adValoremTaxBaseAmount = commodity.DutyTaxFee.AdValoremTaxBaseAmount;
						if (dic.ContainsKey(chapter))
						{
							dic[chapter] += adValoremTaxBaseAmount;
						}
						else
						{
							dic.Add(chapter, adValoremTaxBaseAmount);
						}
					}
				}
			}

			if (dic.Any())
			{
				var largestValue = dic.Values.Max();
				var largestChapter = dic.FirstOrDefault(x => x.Value == largestValue).Key;

				TariffChapterDictionary.TryGetValue(largestChapter, out result);
			}
			return GetCompleteChapterName(result);
		}

		protected ZString GetCompleteChapterName(ZString chapterName) => chapterName.IsEmpty ? chapterName : ZString.Format("{0}{1}", chapterName, documentWrapper.TotalCIFAmountInUSD > 10000m ? FormName.Over10000USD : FormName.AtMost10000USD);

		protected abstract Dictionary<TariffChapter, ZString> TariffChapterDictionary { get; }

		protected abstract TariffChapter GetTariffChapter(ZInt prefix4, ZString classificationId);

		protected enum TariffChapter
		{
			None,
			FirstTariffChapter,
			SecondTariffChapter,
			ThirdTariffChapter,
			FourthTariffChapter,
			FifthTariffChapter,
			SixthTariffChapter,
			SeventhTariffChapter,
			EighthTariffChapter,
			NinthTariffChapter,
			TenthTariffChapter,
			EleventhTariffChapter,
			TwelfthTariffChapter,
		}
	}
}
