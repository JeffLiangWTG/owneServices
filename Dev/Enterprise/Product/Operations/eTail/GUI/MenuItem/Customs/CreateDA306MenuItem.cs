using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
namespace Enterprise.eTail.GUI
{
	public class CreateDA306MenuItem : BaseHVLVMenuItem
	{
		public CreateDA306MenuItem(ForwardingShipment shipment)
			: base(MenuItemName, shipment)
		{
		}

		static ResourceString MenuItemName => ResString.GetMultilingualString("ffdcdb97-74d1-4271-a3c0-ddbb30cff120", "Create DA 306 (Section 38 Release)");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of StmMenuItem")]
		const string DA306ReportName = "DA 306 (Section 38 Release)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "StmMenuItem.SU_MenuPath")]
		const string Customs_HVLV = "Customs/HVLV";

		protected override Action MenuAction => () =>
		{
			var reportCommand = GetDocumentCommand();
			if (reportCommand != null)
			{
				reportCommand.Parent = shipment;
				var runner = new DocumentRunner();
				runner.Run(reportCommand);
			}
			else
			{
				throw new ZException($"Couldn't find menu item {DA306ReportName}.");
			}
		};

		protected DocumentCommand GetDocumentCommand()
		{
			return DocumentCommand.GetDocumentCommand(Factory, shipment, DA306ReportName, Customs_HVLV, ZString.Empty);
		}

		public override void UpdateVisibilityAndCaption()
		{
			Visible = shipment.JobDirection == Directions.Import
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.SouthAfrica
				&& shipment.Destination.Country.Code == CountryCodes.SouthAfrica;
		}

		BusinessObjectFactory Factory => shipment.Factory;
	}
}

