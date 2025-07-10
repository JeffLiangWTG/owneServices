using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class CIN750DeconsNotification : CIN750Notification
	{
		public CIN750DeconsNotification(ZString sourceType, ZString sourceID) : base(sourceType, sourceID) { }

		#region Goods

		public IReadOnlyCollection<Tuple<DocPackingLine, DocPackingLine>> GoodsPairs
		{
			get => goodsPairs;
			set => goodsPairs = SetChildCollection(goodsPairs, value);
		}

		IReadOnlyCollection<Tuple<DocPackingLine, DocPackingLine>> goodsPairs;

		#endregion

		#region OriginalPackagesInfo

		public IReadOnlyCollection<NotificationHistoryInfo> OriginalPackagesInfo;

		#endregion

		#region HistoryInfo

		public IReadOnlyCollection<NotificationHistoryInfo> DeconsHistoryInfo { get; set; }

		public IReadOnlyDictionary<ZString, List<NotificationHistoryInfo>> InHistoryInfo { get; set; }

		public IReadOnlyDictionary<ZString, List<NotificationHistoryInfo>> CorHistoryInfo { get; set; }

		#endregion

		#region CIN750MessageNote

		protected override ZString CIN750MessageNote
		{
			get
			{
				var noteBuilder = new ZStringBuilder();

				var noteHelper = new CIN750NotificationNoteHelper();
				var noteHeader = noteHelper.GetTable(CIN750NotificationNoteHeader, new CIN750Notification[] { this },
								CIN750NotificationColumn.MessageType,
								CIN750NotificationColumn.Result,
								CIN750NotificationColumn.RefType,
								CIN750NotificationColumn.RefCode,
								CIN750NotificationColumn.EnterpriseCode,
								CIN750NotificationColumn.CFSWarehouse,
								CIN750NotificationColumn.CFSCINCode);
				noteBuilder.AppendLine(noteHeader);

				if (GoodsPairs.Count > 0)
				{
					var fromGoods = GoodsPairs.Select(g => g.Item1);
					var toGoods = GoodsPairs.Select(g => g.Item2);

					var docPackingLineNoteHelper = new DocPackingLineTransitNoteHelper();
					var docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750FromGoodsNoteHeader, fromGoods,
											DocPackingLineColumn.Quantity,
											DocPackingLineColumn.Weight,
											DocPackingLineColumn.Description,
											DocPackingLineColumn.RefType,
											DocPackingLineColumn.RefCode,
											DocPackingLineColumn.PNTS);
					noteBuilder.AppendLine(docPackingLineNote);

					docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750ToGoodsNoteHeader, toGoods,
											DocPackingLineColumn.Quantity,
											DocPackingLineColumn.Weight,
											DocPackingLineColumn.Description,
											DocPackingLineColumn.RefType,
											DocPackingLineColumn.RefCode,
											DocPackingLineColumn.PNTS);
					noteBuilder.AppendLine(docPackingLineNote);
				}

				noteBuilder.AppendLine();
				return noteBuilder.ToString();
			}
		}

		public override ZString NotificationType => TransitDocDataConstants.NotificationTypes.CIN750DeconsNotification;

		#endregion
	}
}
