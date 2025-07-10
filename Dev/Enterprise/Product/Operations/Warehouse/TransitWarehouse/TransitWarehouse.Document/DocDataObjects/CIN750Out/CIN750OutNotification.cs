using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class CIN750OutNotification : CIN750Notification
	{
		public CIN750OutNotification(ZString sourceType, ZString sourceID) : base(sourceType, sourceID)
		{
		}

		#region ToCTO

		public Address ToCTO
		{
			get => toCTO;
			set => toCTO = SetChild(toCTO, value);
		}

		Address toCTO;

		#endregion

		#region ToCTOCIN

		public ZString ToCTOCIN
		{
			get => toCTOCIN;
			set
			{
				if (SetNonPersistentPropertyValue(ToCTOCINInfo, ref toCTOCIN, value))
				{
					Validate(ToCTOCINInfo);
				}
			}
		}
		ZString toCTOCIN;

		public ZPropertyInfo ToCTOCINInfo => GetZPropertyInfo(nameof(ToCTOCIN));

		public RegistrationNumber ToCTOCINNumber
		{
			get => toCTOCINNumber;
			set => toCTOCINNumber = SetChild(toCTOCINNumber, value);
		}

		RegistrationNumber toCTOCINNumber;

		#endregion

		#region CustomsDocument

		public IReadOnlyCollection<CIN750CustomsDocument> CustomsDocuments
		{
			get => customsDocuments;
			set => customsDocuments = SetChildCollection(customsDocuments, value);
		}

		IReadOnlyCollection<CIN750CustomsDocument> customsDocuments;

		#endregion

		#region CustomsStatus

		public ICodeDescription CustomsStatus
		{
			get => customsStatus;
			set => customsStatus = SetChild(customsStatus, value);
		}
		ICodeDescription customsStatus;

		#endregion

		#region Validation Values

		public ZInt MaxQuantity { get; set; }
		public ZDecimal MaxWeight { get; set; }

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
								CIN750NotificationColumn.ToCTO,
								CIN750NotificationColumn.ToCINCode,
								CIN750NotificationColumn.CFSWarehouse,
								CIN750NotificationColumn.CFSCINCode);
				noteBuilder.AppendLine(noteHeader);

				if (Goods.Count > 0)
				{
					var docPackingLineNoteHelper = new DocPackingLineTransitNoteHelper();
					var docPackingLineNote = docPackingLineNoteHelper.GetTable(CIN750GoodsNoteHeader, Goods,
											DocPackingLineColumn.Quantity,
											DocPackingLineColumn.Weight,
											DocPackingLineColumn.Description,
											DocPackingLineColumn.PNTS);
					noteBuilder.AppendLine(docPackingLineNote);
				}

				noteBuilder.AppendLine();
				return noteBuilder.ToString();
			}
		}

		public override ZString NotificationType => TransitDocDataConstants.NotificationTypes.CIN750OutNotification;

		#endregion
	}
}
