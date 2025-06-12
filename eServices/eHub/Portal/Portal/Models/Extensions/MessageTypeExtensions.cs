using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class MessageTypeExtensions
	{
		public static IQueryable<eHubMessageType> GetMessageTypeQuery(this IeHubTransactionsContext _context)
		{
			return from mt in _context.eHubMessageTypes select mt;
		}

		public static eHubMessageType GetMessageType(this IeHubTransactionsContext _context, Guid? id)
		{
			return (from t in _context.eHubMessageTypes where t.DT_PK == id select t).ToList().FirstOrDefault();
		}

		public static MessageTypeView[] FindMessageTypeViewList(this IeHubTransactionsContext _context, string text)
		{
			return  (from t in _context.eHubMessageTypes
					where t.DT_Code.Contains(text)
					orderby t.DT_Code
					select new MessageTypeView { Id = t.DT_PK, Name = t.DT_Code }).Distinct().ToArray();
		}
	}
}