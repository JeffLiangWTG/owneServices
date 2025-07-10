using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.AutoRating.Serialization
{
	public static class MoneyTypeSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			var sb = new StringBuilder();

			var moneyType = (MoneyType)objectToSerialize;
			if (moneyType != null)
			{
				foreach (var typeAndMoneys in moneyType.Values)
				{
					sb.AppendLine(ZString.Format((NoResString)"Money Type: {0}", typeAndMoneys.Key));  // displays Object Information

					foreach (var money in typeAndMoneys.Value)
					{
						sb.AppendLine(BuildMoneyDetails(money));
						sb.AppendLine();
					}
				}
			}

			return sb.ToString();
		}

		static string BuildMoneyDetails(Money money)
		{
			return new[]
			{
				ZString.Format((NoResString)"Money Detail: {0}", money), // displays Object Information
				ZString.Format((NoResString)"Invalid: {0}", money.IsValid), // displays Object Information
				ZString.Format((NoResString)"Empty: {0}", money.IsEmpty), // displays Object Information
			}
				.ToStringWithNewLineBetweenStrings();
		}
	}
}
