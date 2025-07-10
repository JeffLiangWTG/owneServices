using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public enum PaymentTermType
	{
		Incoterm,
		PrepaidCollect,
		DomesticPaymentTerm
	}

	public class PaymentTermInfos
	{
		public ReadOnlyCollection<PaymentTermInfo> PaymentTermInfoCollection => new(paymentTermInfoCollection);
		readonly Collection<PaymentTermInfo> paymentTermInfoCollection = new();

		public void AddOrReplace(PaymentTermInfo info)
		{
			var searchInfo = this.GetPaymentTermInfo(info.CostOrSell);

			if (searchInfo != null)
			{
				paymentTermInfoCollection.Remove(searchInfo);
			}

			paymentTermInfoCollection.Add(info);
		}

		public bool Remove(PaymentTermType infoType)
		{
			var infosToRemove = paymentTermInfoCollection.Where(c => c.InfoType == infoType).ToList();
			foreach (var info in infosToRemove)
			{
				paymentTermInfoCollection.Remove(info);
			}

			return infosToRemove.Any();
		}

		#region static methods

		public static Directions GetExportImport(CostSell costOrSell, Directions direction, OrgHeader billTo, OrgHeader agent, OrgHeader consignor, OrgHeader consignee)
		{
			var result = Directions.Unknown;

			switch (direction)
			{
				case Directions.Import:
				case Directions.Export:
					result = direction;
					break;
				default:
					if (costOrSell == CostSell.Revenue)
					{
						if (billTo != null)
						{
							if (consignee.PKEquals(billTo))
							{
								result = Directions.Import;
							}
							else if (consignor.PKEquals(billTo))
							{
								result = Directions.Export;
							}
						}

						if (result == Directions.Unknown && agent != null)
						{
							if (consignee.PKEquals(agent))
							{
								result = Directions.Export;
							}
							else if (consignor.PKEquals(agent))
							{
								result = Directions.Import;
							}
						}
					}

					break;
			}

			return result;
		}

		public static string GetPrepaidCollect(string chargeGroup)
		{
			switch (chargeGroup)
			{
				case ChargeCodeGroupList.Codes.Origin:
				case ChargeCodeGroupList.Codes.OriginBrokerage:
				case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
				case ChargeCodeGroupList.Codes.Loading:
					return Constants.PaymentType.Prepaid;

				case ChargeCodeGroupList.Codes.Unloading:
				case ChargeCodeGroupList.Codes.Destination:
				case ChargeCodeGroupList.Codes.CustomsDuty:
				case ChargeCodeGroupList.Codes.Brokerage:
				case ChargeCodeGroupList.Codes.BrokerageOnly:
					return Constants.PaymentType.Collect;

				default:
					return default;
			}
		}

		#endregion
	}

	// This extension class is not a good programming practice because it is an anemic class.
	// It is created to avoid NullReferenceException. Better to merge it with PaymentTermInfos
	public static class PaymentTermInfosExtensions
	{
		public static bool IsEmpty(this PaymentTermInfos paymentTermInfos) => paymentTermInfos == null || paymentTermInfos.PaymentTermInfoCollection.Count == 0;

		public static PaymentTermInfo GetPaymentTermInfo(this PaymentTermInfos paymentTermInfos, CostSell costSell)
			=> paymentTermInfos != null
				? paymentTermInfos.PaymentTermInfoCollection.SingleOrDefault(c => c.CostOrSell == costSell)
				: null;

		public static ChargedParty GetChargedParty(this PaymentTermInfos paymentTermInfos, string chargeGroup, Directions direction, Directions exportImport, CostSell costOrSell)
		{
			if (paymentTermInfos.IsThirdParty(costOrSell))
			{
				return ChargedParty.None;
			}

			var paymentType = GetPrepaidCollect(paymentTermInfos, costOrSell, chargeGroup);

			if (string.IsNullOrEmpty(paymentType))
			{
				return ChargedParty.Unknown;
			}

			var chargedParty = paymentType == Constants.PaymentType.Collect
				? ChargedParty.Consignee
				: ChargedParty.Consignor;

			var isDomestic = direction == Directions.Domestic;

			if (exportImport != Directions.Unknown)
			{
				if ((exportImport == Directions.Import && paymentType == Constants.PaymentType.Collect)
					|| (exportImport == Directions.Export && paymentType == Constants.PaymentType.Prepaid))
				{
					chargedParty = ChargedParty.LocalClient | chargedParty;
				}
				else if (!isDomestic)
				{
					chargedParty = ChargedParty.Agent | chargedParty;
				}
			}

			if (isDomestic && !chargedParty.HasFlag(ChargedParty.LocalClient))
			{
				chargedParty = ChargedParty.LocalClient | chargedParty;
			}

			return chargedParty;
		}

		public static ChargedPartyForCrossTradeJob GetChargedPartyForCrossTradeJob(this PaymentTermInfos paymentTermInfos, string chargeGroup, CostSell costOrSell, CrossTradeDebtorDefaultingParam debtorDefaultingParam)
		{
			if (paymentTermInfos.IsThirdParty(costOrSell))
			{
				return ChargedPartyForCrossTradeJob.Unknown;
			}

			var paymentType = GetPrepaidCollect(paymentTermInfos, costOrSell, chargeGroup);
			if (string.IsNullOrEmpty(paymentType))
			{
				return ChargedPartyForCrossTradeJob.Unknown;
			}

			var billToParty = CrossTradeDebtorDefaulting.GetDefaultPartyToChargeForCrossTradeJob(debtorDefaultingParam.JobType.Code, debtorDefaultingParam.TransportMode, paymentType == Constants.PaymentType.Collect);
			return billToParty;
		}

		#region IsThirdParty

		public static bool IsThirdParty(this PaymentTermInfos paymentTermInfos, CostSell costSell)
		{
			var info = paymentTermInfos.GetPaymentTermInfo(costSell);
			return info != null && paymentTermInfos.IsThirdParty(info.InfoType);
		}

		public static bool IsThirdParty(this PaymentTermInfos paymentTermInfos, PaymentTermType infoType)
		{
			var info = paymentTermInfos.GetPaymentTermInfo(CostSell.Revenue);
			return info != null
				&& info.InfoType == infoType
				&& info.Value == Constants.DomesticPaymentTerms.CollectThirdParty;
		}

		#endregion

		#region Get Prepaid/Collect

		public static string GetPrepaidCollect(this PaymentTermInfos paymentTermInfos, CostSell costSell, string chargeGroup = "")
		{
			var info = paymentTermInfos.GetPaymentTermInfo(costSell);
			return info != null
				? GetPrepaidCollect(chargeGroup, info.InfoType, info.Value)
				: GetPrepaidCollect(chargeGroup, (PaymentTermType)(-1), null);
		}

		static string GetPrepaidCollect(string chargeGroup, PaymentTermType paymentTermType, string value)
		{
			if (paymentTermType == PaymentTermType.Incoterm)
			{
				return IncoTermRegistry.GetPrepaidCollect(chargeGroup, value);
			}

			var paymentType = PaymentTermInfos.GetPrepaidCollect(chargeGroup);
			if (!string.IsNullOrEmpty(paymentType))
			{
				return paymentType;
			}

			switch (chargeGroup)
			{
				case ChargeCodeGroupList.Codes.ContainerStorage:
				case ChargeCodeGroupList.Codes.ShippingDisbursements:
					return default;
			}

			switch (paymentTermType)
			{
				case PaymentTermType.PrepaidCollect:
					return value.In(Constants.PaymentType.Prepaid, Constants.PaymentType.Collect)
						? value
						: default;
				case PaymentTermType.DomesticPaymentTerm:
					switch (value)
					{
						case Constants.DomesticPaymentTerms.Collect:
						case Constants.DomesticPaymentTerms.CollectCOD:
						case Constants.DomesticPaymentTerms.CollectThirdParty:
							return Constants.PaymentType.Collect;
						case Constants.DomesticPaymentTerms.Prepaid:
							return Constants.PaymentType.Prepaid;
						default:
							return default;
					}
				default:
					return default;
			}
		}

		#endregion
	}
}
