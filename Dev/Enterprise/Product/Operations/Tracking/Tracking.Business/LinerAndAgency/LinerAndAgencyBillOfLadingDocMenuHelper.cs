using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Business
{
	public class LinerAndAgencyBillOfLadingDocMenuHelper : DocumentsMenuHelper
	{
		public LinerAndAgencyBillOfLadingDocMenuHelper(ZGuid pk)
			: this(new BusinessObjectFactory().Load<BillOfLading>(pk))
		{
		}

		public LinerAndAgencyBillOfLadingDocMenuHelper(BillOfLading bill)
		{
			billOfLading = bill;
		}

		readonly BillOfLading billOfLading;

		#region DocumentsMenuHelper Members

		public TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		public override List<DocumentsMenuItem> GetAvailableDocuments()
		{
			List<DocumentsMenuItem> menuItems = new List<DocumentsMenuItem>();

			DocumentCommandCollection documentCommands = new DocumentCommandCollection(billOfLading);
			documentCommands.LoadWithMoreFiltering(GetDocumentsFilter());

			foreach (DocumentCommand documentCommand in documentCommands)
			{
				if (documentCommand.IsApplicable && SiteUser != null && !SiteUser.IsShipmentQuickViewUser)
				{
					if (documentCommand.SU_MenuName == BillOfLadingDocumentName)
					{
						string menuItemName = Res.GetString("1cf18a18-3046-4b6f-89a6-15c5053a429c", "Draft Bill of Lading");
						menuItems.Add(new DocumentsMenuItem(documentCommand, DataContentTypes.Pdf, menuItemName));
					}
					else
					{
						menuItems.Add(new DocumentsMenuItem(documentCommand, DataContentTypes.Pdf));
					}
				}
			}

			return menuItems;
		}

		public override ZGuid PKForBizOCreation
		{
			get
			{
				return billOfLading.PK;
			}
		}

		public override IDocumentSupportable GetDocumentSupportable()
		{
			return billOfLading;
		}

		public override void SetupDocumentPack(DocumentPack pack)
		{
			base.SetupDocumentPack(pack);

			// Leave one copy
			IDeliverable copy = null;
			foreach (IDeliverable report in pack)
			{
				if (report.Name.ToUpper() == "COPY")
				{
					copy = report;
					break;
				}
			}
			pack.RemoveAll();
			if (copy != null)
			{
				pack.Add(copy);
			}

			pack.DeliveryInstructions.IsDraft = true; // Only Draft Bill of Lading
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		const string BillOfLadingDocumentName = "Bill of Lading";

		ZQuery GetDocumentsFilter()
		{
			var query = new ZQuery();

			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, billOfLading.DocumentSupporter.BusinessContext);
			query.AddToFilter(StmMenuItemSchema.SU_IsPublished, ZBool.True);
			query.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuName, BillOfLadingDocumentName);
			query.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuType, SQLComparisonOperator.NotEqual, Core.Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(GetMenuPathFilter(), JoinCondition.And);

			query.OrderBy = MasterFiles.Business.AutoStmMenuItem.Schema.SU_MenuPath + ", " + MasterFiles.Business.AutoStmMenuItem.Schema.SU_MenuIndex;

			return query;
		}

		ZQuery GetMenuPathFilter()
		{
			var query = new ZQuery();

			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Export");
			query.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_MenuPath, Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments + "/Export");

			return query;
		}

		#endregion

		#endregion

	}
}
