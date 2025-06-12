using System;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Helpers
{
	[Serializable]
	public enum QueryType
	{
		Status,
		List,
		EORI,
		NOP,
		VAT
	}

	public static class QueryTypeExtensions
	{
		private static readonly Dictionary<QueryType, string> _messageQueryTypeStrings = new Dictionary<QueryType, string>
		{
			{ QueryType.Status, "status" },
			{ QueryType.List, "list" },
			{ QueryType.EORI, "EORI" },
			{ QueryType.NOP, "NOP" },
			{ QueryType.VAT, "VAT" }
		};

		public static string ToStringValue(this QueryType queryType)
		{
			return _messageQueryTypeStrings[queryType];
		}

		public static bool ExistsInList(this QueryType queryType, List<QueryType> queryTypes)
		{
			return queryTypes.Exists(_ => _.Equals(queryType));
		}


		public static QueryType FromMessageStringValue(string stringValue)
		{
			foreach (var keyValuePair in _messageQueryTypeStrings)
			{
				if (keyValuePair.Value.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
				{
					return keyValuePair.Key;
				}
			}
			throw new Exception($"The NotificationType [{stringValue}] is invalid.");
		}
	}
}
