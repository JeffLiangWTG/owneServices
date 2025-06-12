using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class TransformationSetExtensions
	{
		public static eHubTransformationSet GetTransformationSet(this IeHubTransactionsContext _context, Guid id)
		{
			return (from t in _context.eHubTransformationSets where t.TS_PK == id select t).ToList().FirstOrDefault();
		}

		public static List<eHubTransformationSet> GetTransformationSetList(this IeHubTransactionsContext _context, Guid sender, Guid recipient)
		{
			return (from t in _context.eHubTransformationSets
					where (t.TS_CC_Sender ?? Guid.Empty) == sender && (t.TS_CC_Recipient ?? Guid.Empty) == recipient
					orderby t.TS_Name
					select t).ToList();
		}

		public static List<eHubTransformationSet> GetTransformationSetList(this IeHubTransactionsContext _context, Guid messageType)
		{
			return (from t in _context.eHubTransformationSets
					where t.TS_DT_Source == messageType
					orderby t.TS_Name
					select t).ToList();
		}

		public static TransformationSetView GetTransformationSetView(this IeHubTransactionsContext _context, Guid id)
		{
			var details = new TransformationSetView();
			details.TransformationSet = _context.GetTransformationSet(id);
			if (details.TransformationSet == null) return null;

			var sender = _context.GetClient(details.TransformationSet.TS_CC_Sender);
			if (sender == null) details.Sender = new SelectValueView() { Type = "Sender" };
			else details.Sender = new SelectValueView() { Id = sender.CC_PK, Code = sender.CC_ID, Name = sender.CC_FriendlyName, Type = "Sender" };

			var recipient = _context.GetClient(details.TransformationSet.TS_CC_Recipient);
			if (recipient == null) details.Recipient = new SelectValueView() { Type = "Recipient" };
			else details.Recipient = new SelectValueView() { Id = recipient.CC_PK, Code = recipient.CC_ID, Name = recipient.CC_FriendlyName, Type = "Recipient" };

			var messageType = _context.GetMessageType(details.TransformationSet.TS_DT_Source);
			if (messageType == null) details.Source = new MessageTypeView();
			else details.Source = new MessageTypeView() { Id = messageType.DT_PK, Name = messageType.DT_Code };

			var billOther = details.TransformationSet.eHubClient_BillOther;
			if (billOther == null)
			{
				details.BillOther = new SelectValueView() { Type = "BillOther" };
			}
			else
			{
				details.BillOther = new SelectValueView()
				{
					Id = billOther.CC_PK,
					Code = billOther.CC_ID,
					Name = billOther.CC_FriendlyName,
					Type = "BillOther"
				};
			}

			details.MappingList = _context.GetTransformationMappingViewList(details.TransformationSet.TS_PK);

			return details;
		}

		public static TransformationSetView CreateTransformationSetView()
		{
			return new TransformationSetView() { Recipient = new SelectValueView() { Type = "Recipient" }, Sender = new SelectValueView() { Type = "Sender" }, Source = new MessageTypeView(), BillOther = new SelectValueView(), MappingList = new List<TransformationMappingView>(), TransformationSet = new eHubTransformationSet() { TS_PK = Guid.NewGuid() } };
		}
	}
}