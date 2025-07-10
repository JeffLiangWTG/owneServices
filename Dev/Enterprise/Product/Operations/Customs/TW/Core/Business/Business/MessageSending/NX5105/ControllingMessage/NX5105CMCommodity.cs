using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMCommodity : NX5105GovernmentAgencyGoodsItem_Commodity
	{
		public NX5105CMCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine) : base(entryLine, invoiceLine)
		{
		}

		protected override IConstituent ConstituentCore
		{
			get
			{
				IConstituent result = null;
				var compositions = EntryLine.CL_Specification;
				var levelID = InvoiceLine.JI_ProductGrade;
				var thickness = InvoiceLine.JI_ProductThickness;
				if (!compositions.IsEmpty || !levelID.IsEmpty || !thickness.IsEmpty)
				{
					result = new ConstituentWrapper(compositions, levelID, thickness);
				}
				return result;
			}
		}

		protected override ZString GoodsGroupNameCodeCore => InvoiceLine.JI_GoodsType;

		protected override ZString BarCodeCore => InvoiceLine.JI_BarCode.IsEmpty ? ZString.Empty : InvoiceLine.JI_BarCode.PadRight(13, '0');

		protected override ZString TariffCodeExtensionCodeCore => InvoiceLine.JI_TariffExtensionCode;

		protected override ICommodityRelatedPackaging CommodityRelatedPackagingCore => CommodityRelatedPackagingWrapper.GetCommodityRelatedPackaging(InvoiceLine);

		protected override IEnumerable<ZString> HandlingInstructionsCodesCore => InvoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.Cast<StorageAndShippingConditionJobComInvLineRefs>()
			.Where(x => !x.JG_ReferenceNumber.IsEmpty)
			.OrderBy(x => x.JG_SystemCreateTimeUtc)
			.Select(x => x.JG_ReferenceNumber)
			.Take(9)
			.ToArray();

		protected override IQuarantine GetQuarantineCore() => new QuarantineWrapper(InvoiceLine);

		protected override IFood GetFoodCore()
		{
			IFood result = null;
			ZDecimal parseResult;
			ZDecimal? phValue = null;
			ZDecimal? sterilizationValue = null;
			if (!InvoiceLine.JI_PHValue.IsEmpty && ZDecimal.TryParse(InvoiceLine.JI_PHValue, out parseResult))
			{
				phValue = parseResult;
			}
			if (!InvoiceLine.JI_SterilizationValue.IsEmpty && ZDecimal.TryParse(InvoiceLine.JI_SterilizationValue, out parseResult))
			{
				sterilizationValue = parseResult;
			}
			var constituents = InvoiceLine.FoodDataCollection.Cast<FoodData>().Where(x => !x.CY_Data.IsEmpty).Select(x => new FoodConstituentWrapper(x.CY_Data, x.Content));
			if (phValue != null || sterilizationValue != null || constituents.Any())
			{
				result = new FoodWrapper(phValue, sterilizationValue, constituents);
			}
			return result;
		}

		protected override IWine WineCore => !InvoiceLine.JI_AlcoholPercentage.IsEmpty ? new NX5105CMCommodityWine(InvoiceLine) : null;

		public override ZString Description => InvoiceLine.IsForCMHeaderMessageTypeNX301_DN ? ZString.Empty : base.Description;

		bool shouldPopulateEnglishOrChineseDescriptionTypes
		{
			get
			{
				return InvoiceLine.IsForCMHeaderMessageTypeNX301 ||
				InvoiceLine.IsForCMHeaderMessageTypeNX301_DN ||
				InvoiceLine.IsForCMHeaderMessageTypeNX401 ||
				InvoiceLine.IsForCMHeaderMessageTypeNX601 ||
				InvoiceLine.IsForCMHeaderMessageTypeNX603;
			}
		}

		protected override ZString GetEnglishDescriptionCore()
		{
			return shouldPopulateEnglishOrChineseDescriptionTypes ? InvoiceLine.JI_Description : ZString.Empty;
		}

		protected override ZString GetChineseDescriptionCore()
		{
			return shouldPopulateEnglishOrChineseDescriptionTypes ? InvoiceLine.JI_NDescription.ExcludeNonValidXMLCharacters() : ZString.Empty;
		}
	}
}
