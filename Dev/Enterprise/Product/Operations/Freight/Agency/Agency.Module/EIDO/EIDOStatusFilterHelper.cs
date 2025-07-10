using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	internal static class EIDOStatusFilterHelper
	{
		public static ZQuery GetEIDOStatusFilter(ZString mode)
		{
			switch (mode)
			{
				case EIDOFilterList.Codes.Failed:
					return GetEIDOStatusFilter(
						new string[] { EIDOMessageTypes.Codes.Original, EIDOMessageTypes.Codes.Cancellation },
						new string[] { EIDOMessage.Status.Failed },
						false);

				case EIDOFilterList.Codes.PendingResponse:
					return GetEIDOStatusFilter(
						new string[] { EIDOMessageTypes.Codes.Original, EIDOMessageTypes.Codes.Cancellation },
						new string[] { EDIMessage.Status.Queued, EDIMessage.Status.Sent },
						false);

				case EIDOFilterList.Codes.Rejected:
					return GetEIDOStatusFilter(
						new string[] { EIDOMessageTypes.Codes.Original, EIDOMessageTypes.Codes.Cancellation },
						new string[] { EDIMessage.Status.Rejected },
						false);

				case EIDOFilterList.Codes.Accepted:
					return GetEIDOStatusFilter(
						new string[] { EIDOMessageTypes.Codes.Original },
						new string[] { EDIMessage.Status.Received },
						false);

				case EIDOFilterList.Codes.Acknowledged:
					return GetEIDOStatusFilter(
						new string[] { EIDOMessageTypes.Codes.Original },
						new string[] { EDIMessage.Status.Acknowledged },
						false);

				case EIDOFilterList.Codes.NotSent:
					return GetEIDOStatusFilter(
						new string[] { EIDOMessageTypes.Codes.Cancellation },
						new string[] { EDIMessage.Status.Received, EDIMessage.Status.Acknowledged },
						true);

				default:
					return new ZQuery();
			}
		}

		static ZQuery GetEIDOStatusFilter(string[] subTypes, string[] status, bool includeMessageless)
		{
			var sqlExcludingMessageless = string.Format(CultureInfo.InvariantCulture,
				"{0} in " +
				"( " +
				"select {1} " +
				"from {2} " +
				"join " +
				"( " +
				"select {1} as ParentPK, max({3}) as lastMessageTime " +
				"from {2} " +
				"where {4} = 'EDO' and {5} = 'TRX' " +
				"group by {1} " +
				") lastMessageDate on {1} = ParentPK and {3} = lastMessageTime " +
				"where " +
				"{4} = 'EDO' " +
				"and " +
				"{5} = 'TRX' " +
				"and " +
				"{6} in ('{7}') " +
				"and " +
				"{8} in ('{9}') " +
				")" +
				"",
				JobContainerSchema.Constants.PK, // 0
				EDIMessageSchema.Constants.EM_LinkUniqueID, // 1
				EDIMessageSchema.Constants.TableName, // 2
				EDIMessageSchema.Constants.EM_SystemCreateTimeUtc, // 3
				EDIMessageSchema.Constants.EM_ApplicationCode, // 4
				EDIMessageSchema.Constants.EM_ReceiveTransmit, // 5
				EDIMessageSchema.Constants.EM_MessageSubType, // 6
				"{0}", // 7
				EDIMessageSchema.Constants.EM_Status, // 8
				"{1}"); // 9

			var sqlIncludingMessageless = string.Format(CultureInfo.InvariantCulture,
				"{0} in " +
				"( " +
				"select {0} " +
				"from {1} " +
				"left join  " +
				"( " +
				"select {2}, {3}, {4} from {5} join " +
				"( " +
				"select {2} as ParentPK, max({6}) as lastMessageTime " +
				"from {5} " +
				"where {7} = 'EDO' " +
				"and " +
				"{8} = 'TRX' " +
				"group by {2} " +
				") lastMessageDate on {2} = ParentPK and {6} = lastMessageTime " +
				"where " +
				"{7} = 'EDO' " +
				"and " +
				"{8} = 'TRX' " +
				") lastMessage on {0} = {2} " +
				"where " +
				"( " +
				"{4} in ('{9}') " +
				"and " +
				"{3} in ('{10}') " +
				") " +
				"or " +
				"( " +
				"{2} is null " +
				") " +
				") " +
				"",
				JobContainerSchema.Constants.PK, // 0
				JobContainerSchema.Constants.TableName, // 1
				EDIMessageSchema.Constants.EM_LinkUniqueID, // 2
				EDIMessageSchema.Constants.EM_Status, // 3
				EDIMessageSchema.Constants.EM_MessageSubType, // 4
				EDIMessageSchema.Constants.TableName, // 5
				EDIMessageSchema.Constants.EM_SystemCreateTimeUtc, // 6
				EDIMessageSchema.Constants.EM_ApplicationCode, // 7
				EDIMessageSchema.Constants.EM_ReceiveTransmit, // 8
				"{0}", // 9
				"{1}"); // 10

			string sql = includeMessageless ? sqlIncludingMessageless : sqlExcludingMessageless;

			ZDBOnlyQuery statusFilter = new ZDBOnlyQuery(typeof(EDIMessage));
			statusFilter.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, sql,
				string.Join("','", subTypes),
				string.Join("','", status)), new ZSqlParameterCollection());

			return statusFilter;
		}
	}
}


