using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class CIN750CorNotification : CIN750Notification
	{
		public CIN750CorNotification(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		public ZInt ReceivedPackageQuantity { get; set; }

		#region CorHistoryInfo

		public IReadOnlyCollection<NotificationHistoryInfo> CorHistoryInfo { get; set; }

		#endregion

		#region InHistoryInfo

		public IReadOnlyCollection<NotificationHistoryInfo> InHistoryInfo { get; set; }

		#endregion

		#region RCNPackageState

		public List<WhsItemPackageState> RCNPackageStates { get; set; }

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

				if (Goods.Count > 0)
				{
					var docPackingLineNoteHelper = new DocPackingLineTransitNoteHelper();
					var docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750GoodsNoteHeader, Goods,
											DocPackingLineColumn.Quantity,
											DocPackingLineColumn.Weight,
											DocPackingLineColumn.PNTS,
											DocPackingLineColumn.Description);
					noteBuilder.AppendLine(docPackingLineNote);
				}

				noteBuilder.AppendLine();
				return noteBuilder.ToString();
			}
		}

		public override ZString NotificationType => TransitDocDataConstants.NotificationTypes.CIN750CorNotification;

		#endregion
	}
}
