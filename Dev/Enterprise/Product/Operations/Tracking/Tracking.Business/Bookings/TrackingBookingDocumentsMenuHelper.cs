using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Business
{
	public enum TrackingDocumentTypes
	{
		Any,
		FreightLabels,
		HouseBills
	}

	public class TrackingBookingDocumentsMenuHelper : DocumentsMenuHelper
	{
		public TrackingBookingDocumentsMenuHelper(ZGuid pk)
			: this(TrackingBooking.GetFromRefPK(new BusinessObjectFactory(), pk, (TrackingSiteUser)WebEnv.AppInstance.SiteUser))
		{
		}

		public TrackingBookingDocumentsMenuHelper(TrackingBooking trackingBooking, TrackingDocumentTypes trackingDocuments = TrackingDocumentTypes.Any)
		{
			this.trackingBooking = trackingBooking;
			this.trackingDocuments = trackingDocuments;
		}

		readonly TrackingBooking trackingBooking;
		readonly TrackingDocumentTypes trackingDocuments;

		#region DocumentsMenuHelper Members

		public override List<DocumentsMenuItem> GetAvailableDocuments()
		{
			return trackingBooking != null ? GetBookingDocuments() : new List<DocumentsMenuItem>();
		}

		List<DocumentsMenuItem> GetBookingDocuments()
		{
			var documentCommands = new DocumentCommandCollection(trackingBooking);
			documentCommands.LoadWithMoreFiltering(GetDocumentsFilter());

			return documentCommands.Where(d => ((DocumentCommand)d).IsApplicable).Select(d => new DocumentsMenuItem((DocumentCommand)d, DataContentTypes.Pdf)).ToList();
		}

		public override ZGuid PKForBizOCreation => trackingBooking.BookingPK;

		public override IDocumentSupportable GetDocumentSupportable() => trackingBooking;

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		const string FreightLabelDocName = "Freight Label";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		const string LaserHAWBDocName = "Booking House Bill";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		const string BillOfLadingDocName = "Bill Of Lading";

		ZQuery GetFreightLabelFilter()
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Domestic");
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuName, FreightLabelDocName);
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, trackingBooking.DocumentSupporter.BusinessContext);
			return result;
		}

		ZQuery GetLaserHAWBFilter()
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(StmMenuItemSchema.SU_MenuPath, "");
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuName, LaserHAWBDocName);
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, trackingBooking.DocumentSupporter.BusinessContext);
			return result;
		}

		ZQuery GetBillOfLadingFilter()
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuName, BillOfLadingDocName);
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, trackingBooking.Booking.DocumentSupporter.BusinessContext);
			result.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuType, SQLComparisonOperator.NotEqual, Core.Constants.StmMenuItemTypes.Forms);
			return result;
		}

		bool AddHouseBills => trackingDocuments == TrackingDocumentTypes.Any || trackingDocuments == TrackingDocumentTypes.HouseBills;

		bool AddFreightLabels => trackingDocuments == TrackingDocumentTypes.Any || trackingDocuments == TrackingDocumentTypes.FreightLabels;

		ZQuery GetDocumentsFilter()
		{
			ZQuery query = new ZQuery();

			if (AddHouseBills)
			{
				if (trackingBooking.Booking.Consignor != null &&
					trackingBooking.Booking.Consignor.MiscServ.OM_EXAllowedToPrintOriginalBL &&
					trackingBooking.Booking.IsSea)
				{
					query.AddToFilter(GetBillOfLadingFilter(), JoinCondition.Or);
				}
				else
				{
					query.AddToFilter(GetLaserHAWBFilter(), JoinCondition.Or);
				}
			}

			if (AddFreightLabels && !trackingBooking.IsFCL)
			{
				query.AddToFilter(GetFreightLabelFilter(), JoinCondition.Or);
			}

			if (query.IsEmpty)
			{
				query = ZQuery.NoResultQuery;
			}
			else
			{
				query
					.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, ZBool.True)
					.OrderBy = MasterFiles.Business.AutoStmMenuItem.Schema.SU_MenuPath + ", " + MasterFiles.Business.AutoStmMenuItem.Schema.SU_MenuIndex;
			}

			return query;
		}

		#endregion

		#endregion

	}
}
