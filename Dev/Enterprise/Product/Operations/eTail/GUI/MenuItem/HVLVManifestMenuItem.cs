using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.GUI
{
	public class HVLVManifestMenuItem : BaseHVLVMenuItem
	{
		public HVLVManifestMenuItem(ForwardingShipment shipment)
			: base(MenuItemName, shipment)
		{
		}

		static ResourceString MenuItemName => ResString.GetMultilingualString("df304645-b282-4e2f-a513-3617f3398216", "HVLV Manifest");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of StmMenuItem")]
		const string HVLVShipmentItemDetailsReportName = "HVLV Shipments Item Details Report";

		DocumentPack PrepareDocumentPack()
		{
			var reportCommand = Factory.LoadTop1<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, HVLVShipmentItemDetailsReportName))
				?? throw new ZException($"Cound't find menu item {HVLVShipmentItemDetailsReportName}.");

			var pack = new DocumentPack(reportCommand);
			var report = pack.GetFirstReport();
#if DEBUG
			ReportForTesting = report;
#endif
			report.PrepareForRender();
			((LookupField)report.FilterCollection["Shipment ID"]).Value = shipment.PK.ToGuid();

			return pack;
		}

		protected override Action MenuAction => () =>
		{
			var pack = PrepareDocumentPack();

			var task = new PrintTask();
			task.Add(pack);
			var formProvider = ObjectFactory.Get<IPrintTaskUIProvider>("IPrintTaskUIProvider.WinForms");
			formProvider.ShowRuntimeOptionsUI(task, AllowedDeliveryOptions.All, new DeliveryInstructions(pack), null);
		};

		BusinessObjectFactory Factory => shipment.Factory;

#if DEBUG
		public Report ReportForTesting { get; set; }

		public void PrepareDocumentPackForInvokeAction()
		{
			PrepareDocumentPack();
		}
#endif
	}
}
