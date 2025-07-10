using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class CIN750InNotification : CIN750Notification
	{
		public CIN750InNotification(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
			InHistoryInfo = new List<NotificationHistoryInfo>();
		}

		#region FromCTO

		public Address FromCTO
		{
			get => fromCTO;
			set => fromCTO = SetChild(fromCTO, value);
		}

		Address fromCTO;

		#endregion

		#region FromCTOCIN

		public ZString FromCTOCIN
		{
			get => fromCTOCIN;
			set
			{
				if (SetNonPersistentPropertyValue(FromCTOCINInfo, ref fromCTOCIN, value))
				{
					Validate(FromCTOCINInfo);
				}
			}
		}
		ZString fromCTOCIN;

		public ZPropertyInfo FromCTOCINInfo => GetZPropertyInfo(nameof(FromCTOCIN));

		public RegistrationNumber FromCTOCINNumber
		{
			get => fromCTOCINNumber;
			set => fromCTOCINNumber = SetChild(fromCTOCINNumber, value);
		}

		RegistrationNumber fromCTOCINNumber;

		#endregion

		#region CustomsStatus

		public ICodeDescription CustomsStatus
		{
			get => customsStatus;
			set => customsStatus = SetChild(customsStatus, value);
		}
		ICodeDescription customsStatus;

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
								CIN750NotificationColumn.CustomsStatus,
								CIN750NotificationColumn.FromCTO,
								CIN750NotificationColumn.CTOCINCode,
								CIN750NotificationColumn.CFSWarehouse,
								CIN750NotificationColumn.CFSCINCode);
				noteBuilder.AppendLine(noteHeader);

				if (Goods.Count > 0)
				{
					var docPackingLineNoteHelper = new DocPackingLineTransitNoteHelper();
					var docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750GoodsNoteHeader, Goods,
											DocPackingLineColumn.Quantity,
											DocPackingLineColumn.Weight,
											DocPackingLineColumn.AccompanyingDocumentType,
											DocPackingLineColumn.AccompanyingDocumentRef,
											DocPackingLineColumn.TemporaryStorageDeclaration,
											DocPackingLineColumn.Description);
					noteBuilder.AppendLine(docPackingLineNote);
				}

				noteBuilder.AppendLine();
				return noteBuilder.ToString();
			}
		}

		public override ZString NotificationType => TransitDocDataConstants.NotificationTypes.CIN750InNotification;

		#endregion

		#region ReceivedPackagesInfo

		public ZInt ReceivedPackageQuantity { get; set; }
		public ZDecimal ReceivedPackageWeight
		{
			get => receivedPackageWeight;
			set
			{
				var weight = value.RoundTo3Digits();
				receivedPackageWeight = weight;
			}
		}
		decimal receivedPackageWeight;

		#endregion

		#region InHistoryInfo

		public IReadOnlyCollection<NotificationHistoryInfo> InHistoryInfo { get; set; }

		public IReadOnlyCollection<NotificationHistoryInfo> CorHistoryInfo { get; set; }

		#endregion
	}
}
