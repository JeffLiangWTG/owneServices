using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.AutoRating.Serialization
{
	public static class PaymentTermInfosSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var str = new ZStringBuilder();

			var infos = (PaymentTermInfos)objectToSerialize;
			if (infos != null)
			{
				var costInfo = infos.GetPaymentTermInfo(CostSell.Cost);
				var revenueInfo = infos.GetPaymentTermInfo(CostSell.Revenue);

				str.AppendIfNotEmpty(BuildInfoDetails(costInfo));
				str.AppendIfNotEmpty(BuildInfoDetails(revenueInfo));
			}

			return str.ToStringWithNewLineBetweenAppends();
		}

		static string BuildInfoDetails(PaymentTermInfo info)
		{
			return info != null
				? string.Format("{0}: {1} ({2})", info.CostOrSell, info.Value, info.InfoType)
				: string.Empty;
		}
	}
}
