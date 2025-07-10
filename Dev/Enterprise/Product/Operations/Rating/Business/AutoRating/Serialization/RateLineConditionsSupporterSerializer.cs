using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.AutoRating.Serialization
{
	public static class RateLineConditionsSupporterSerializer
	{
		public static string Serialize(object objectToSerialize)
		{
			if (objectToSerialize == null)
			{
				return string.Empty;
			}

			var supporter = (RateLineConditionsSupporter)objectToSerialize;

			return new[]
			{
				GetString("ArrivalCFS", supporter.ArrivalCFS),
				GetString("DepartureCFS", supporter.DepartureCFS),

				GetString("ImportBroker", supporter.ImportBroker),
				GetString("ExportBroker", supporter.ExportBroker),

				GetString("SendingAgent", supporter.SendingAgent),
				GetString("ReceivingAgent", supporter.ReceivingAgent),

				GetString("ControllingAgent", supporter.ControllingAgent),
			}
			.ToStringWithNewLineBetweenStrings();
		}

		static string GetString(string name, OrgHeader org)
		{
			return org != null
				? ZString.Format("{0}: {1}\r\n{2}", name, org.OH_Code, org.OH_FullNameTruncated)
				: (ZString)Res.GetString("c395f5e5-dc3d-4b21-9c46-a2ffedb06672", "{0}: (null)", name);
		}
	}
}
