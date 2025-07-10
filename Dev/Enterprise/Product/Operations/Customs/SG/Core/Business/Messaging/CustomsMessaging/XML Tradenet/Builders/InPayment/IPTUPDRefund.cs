using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class IPTUPDRefund : IPTDEC
	{
		public IPTUPDRefund(IIPTUPD cusDec)
			: base(cusDec)
		{
		}

		public override string MessageType => CommonAccessReferenceCodeList.Codes.IPTUPD;
		public override string MessageSubType => CUSDECEDIMessage.Refund;
		protected override ZString UpdateIndicatorCode => CusDec.AdditionalMessageInformation.UpdateIndicator;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var inPaymentUpdate = inboundMessage.InPaymentUpdate = new InPaymentUpdate();
			inPaymentUpdate.Update = BuildUpdate();
			inPaymentUpdate.RefundOnly = BuildRefundOnly();
		}

		protected override bool SupportsUpdateRefundSection => true;

		protected RefundOnly BuildRefundOnly()
		{
			RefundOnly refundOnly = new RefundOnly();
			refundOnly.RefundHeader = BuildRefundHeader();
			refundOnly.DeclarantParty = BuildDeclarantParty();
			refundOnly.SupportingDocumentReference = BuildSupportingDocumentReference();
			refundOnly.RefundItem = BuildRefundItems();
			refundOnly.RefundSummary = BuildRefundSummary();

			return refundOnly;
		}

		#region RefundHeader
		RefundOnlyRefundHeader BuildRefundHeader()
		{
			var header = new RefundOnlyRefundHeader();
			BuildCommonHeaderSection(header);

			return header;
		}
		#endregion

		#region DeclarantParty
		DeclarantParty BuildDeclarantParty()
		{
			DeclarantParty declarantParty = null;

			var declarant = CusDec.Declarant;
			if (declarant != null)
			{
				declarantParty = BuildDeclarantPartyCore(declarant);
				declarantParty.PersonInformation = BuildDeclarantPartyPersonInformationCore(declarant);
			}

			return declarantParty;
		}
		#endregion

		#region SupportingDocumentReference
		SupportingDocumentReference[] BuildSupportingDocumentReference()
		{
			var list = BuildSupportingDocumentReferenceCore();
			return list.Any() ? list.ToArray() : null;
		}
		#endregion

		#region RefundItems
		RefundOnlyRefundItem[] BuildRefundItems()
		{
			RefundOnlyRefundItem[] refundItems = null;

			if (IsPRS)
			{
				var index = 1;
				refundItems = CusItems.Select(i => BuildRefundItem(index++, i)).ToArray();
			}

			return refundItems;
		}

		RefundOnlyRefundItem BuildRefundItem(int index, ICusItem cusItem)
		{
			var item = new RefundOnlyRefundItem
			{
				ItemSequenceNumeric = index,
				ItemSequenceNumericSpecified = true,
				ItemHarmonizedSystemCode = cusItem.HSCode
			};

			item.TariffRefund = BuildTariffRefund(cusItem);

			return item;
		}

		RefundOnlyRefundItemTariffRefund BuildTariffRefund(ICusItem cusItem)
		{
			RefundOnlyRefundItemTariffRefund tariffRefund = null;

			var itemDutyRefund = cusItem.ItemDutyRefund;
			var itemExciseRefund = cusItem.ItemExciseRefund;
			var itemGSTRefund = cusItem.ItemGSTRefund;

			if (itemDutyRefund > 0 || itemExciseRefund > 0 || itemGSTRefund > 0)
			{
				tariffRefund = new RefundOnlyRefundItemTariffRefund();
				if (itemDutyRefund > 0)
				{
					tariffRefund.CustomsDutyRefundAmount = ApplyDecimalPlacesForAmountValues(itemDutyRefund);
					tariffRefund.CustomsDutyRefundAmountSpecified = true;
				}

				if (itemExciseRefund > 0)
				{
					tariffRefund.ExciseDutyRefundAmount = ApplyDecimalPlacesForAmountValues(itemExciseRefund);
					tariffRefund.ExciseDutyRefundAmountSpecified = true;
				}

				if (itemGSTRefund > 0)
				{
					tariffRefund.GoodsAndServicesTaxRefundAmount = ApplyDecimalPlacesForAmountValues(itemGSTRefund);
					tariffRefund.GoodsAndServicesTaxRefundAmountSpecified = true;
				}
			}

			return tariffRefund;
		}
		#endregion

		#region RefundSummary
		RefundOnlyRefundSummary BuildRefundSummary()
		{
			var refundSummary = new RefundOnlyRefundSummary();

			if (IsPRS)
			{
				refundSummary.NumberOfItems = CusItems.Count;
			}

			refundSummary.TotalTariffRefund = BuildTotalTariffRefund();

			return refundSummary;
		}

		RefundOnlyRefundSummaryTotalTariffRefund BuildTotalTariffRefund()
		{
			RefundOnlyRefundSummaryTotalTariffRefund totalTariffRefund = new RefundOnlyRefundSummaryTotalTariffRefund();

			var additionalInformation = CusDec.AdditionalMessageInformation;
			if (additionalInformation != null)
			{
				ZDecimal totalDutyRefund = 0;
				ZDecimal totalExciseRefund = 0;
				ZDecimal totalGSTRefund = 0;

				var updateIndicator = additionalInformation.UpdateIndicator;

				if (updateIndicator == UpdateIndicatorCodeList.Codes.PRS)
				{
					// as per Iptupd09bRefund, PRS totals are calculated from included items.
					foreach (ICusItem item in CusItems)
					{
						totalDutyRefund += item.ItemDutyRefund;
						totalExciseRefund += item.ItemExciseRefund;
						totalGSTRefund += item.ItemGSTRefund;
					}
				}
				else
				{
					// as per Iptupd09bRefund, totals are obtained from AdditionalMessageInformation.
					// as per XML specification and samples, Excise & CustomsDuty are excluded unless FRF.
					if (updateIndicator == UpdateIndicatorCodeList.Codes.FRF)
					{
						totalDutyRefund = additionalInformation.DutyRefundAmount;
						totalExciseRefund = additionalInformation.ExciseRefundAmount;
					}

					// as per XML specification and samples, GST is always included.
					totalGSTRefund = additionalInformation.GSTRefundAmount;
				}

				if (totalDutyRefund > 0)
				{
					totalTariffRefund.TotalCustomsDutyRefundAmount = ApplyDecimalPlacesForAmountValues(totalDutyRefund);
					totalTariffRefund.TotalCustomsDutyRefundAmountSpecified = true;
				}

				if (totalExciseRefund > 0)
				{
					totalTariffRefund.TotalExciseDutyRefundAmount = ApplyDecimalPlacesForAmountValues(totalExciseRefund);
					totalTariffRefund.TotalExciseDutyRefundAmountSpecified = true;
				}

				if (totalGSTRefund > 0)
				{
					totalTariffRefund.TotalGoodsAndServicesTaxRefundAmount = ApplyDecimalPlacesForAmountValues(totalGSTRefund);
					totalTariffRefund.TotalGoodsAndServicesTaxRefundAmountSpecified = true;
				}
			}

			return totalTariffRefund;
		}
		#endregion

		ZDecimal ApplyDecimalPlacesForAmountValues(ZDecimal amount)
		{
			if (ZDecimal.TryParse(Utilities.FormatNumber(amount, SGConstants.NumericFormatting.DecimalPlacesForAmountValues, CultureInfo.InvariantCulture), out var formattedAmount))
			{
				return formattedAmount;
			}
			return amount;
		}

		protected new ReadOnlyCollection<ICusItem> CusItems => cusItems ?? (cusItems = new ReadOnlyCollection<ICusItem>(base.CusItems.ToArray()));
		ReadOnlyCollection<ICusItem> cusItems;
	}
}
