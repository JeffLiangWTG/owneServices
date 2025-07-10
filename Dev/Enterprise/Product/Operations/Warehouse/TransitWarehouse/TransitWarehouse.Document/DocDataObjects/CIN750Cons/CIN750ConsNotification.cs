using System.Collections.Generic;
using CargoWise.Types;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class CIN750ConsNotification : CIN750Notification
	{
		public CIN750ConsNotification(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		#region GoodsDetails

		public IReadOnlyCollection<DocPackingLine> FromGoods
		{
			get => fromGoods;
			set => fromGoods = SetChildCollection(fromGoods, value);
		}

		public DocPackingLine ToGoods
		{
			get => toGoods;
			set => toGoods = SetChild(toGoods, value);
		}

		IReadOnlyCollection<DocPackingLine> fromGoods;
		DocPackingLine toGoods;

		#endregion

		#region HasOvps

		public bool HasOvps { get; set; }

		#endregion

		#region HasOvps

		public IReadOnlyDictionary<ZString, ZString> FromGoodRCNSourceId { get; set; }

		#endregion

		#region OriginalPackagesInfo

		public IReadOnlyCollection<NotificationHistoryInfo> OriginalPackagesInfo;

		#endregion

		#region HistoryInfo

		public IReadOnlyCollection<NotificationHistoryInfo> ConsHistoryInfo { get; set; }

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

				var docPackingLineNoteHelper = new DocPackingLineTransitNoteHelper();
				if (FromGoods.Count > 0)
				{
					var docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750FromGoodsNoteHeader, FromGoods,
											DocPackingLineColumn.Quantity,
											DocPackingLineColumn.Weight,
											DocPackingLineColumn.Description,
											DocPackingLineColumn.RefType,
											DocPackingLineColumn.RefCode,
											DocPackingLineColumn.PNTS);
					noteBuilder.AppendLine(docPackingLineNote);
				}

				if (ToGoods != null)
				{
					var docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750ToGoodsNoteHeader, new DocPackingLine[] { ToGoods },
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

		public override ZString NotificationType => TransitDocDataConstants.NotificationTypes.CIN750ConsNotification;

		#endregion
	}
}
