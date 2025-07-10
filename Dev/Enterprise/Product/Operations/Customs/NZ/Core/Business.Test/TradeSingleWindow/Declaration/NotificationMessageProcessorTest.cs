using Enterprise.BatchProcessor;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.TradeSingleWindow.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Core;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.Registry.Business;

	public class NotificationMessageProcessorTest : TestCaseWithFactory
	{
		#region IM1

		public void TestIM1()
		{
			var im1NotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Delivery Notification Party</strong>. The <strong>Import Declaration</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 11243008<br />\r\nTSW Acceptance Time\t\t\t: 06-Nov-19 17:23<br />\r\nSender's Reference Number\t: B00004731<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 345<br />\r\n<br />\r\nMaster Bill: 08100023505<br />House Bill: G06238<br /><br />Submitter: CargoWise 2<br />Agent: CargoWise 2<br />Carrier: AIR NEW ZEALAND (NZ) LIMITED<br />Flight No.: QF11<br />Port of Discharge: NZAKL<br /><br />Importer: Importer for ECT<br />\r\n<br />Packaging: 200 PK<br />\r\n<br />Message Status: (819) Delivery Order Herewith<br />method of Payment as specified.<br />Delivery Instructions<br />200 LOOSE PACKAGE(S) OR ITEM(S)\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(IM1_Response, "New Zealand Customs Service - Import Declaration - TSW Notification Message", im1NotificationHtmlBody);
		}

		public void TestIM1WithMultipleContainers()
		{
			var im1ContainersHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Delivery Notification Party</strong>. The <strong>Import Declaration</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 34969632<br />\r\nTSW Acceptance Time\t\t\t: 04-Nov-19 14:13<br />\r\nSender's Reference Number\t: B00004727<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 337<br />\r\n<br />\r\nMaster Bill: G00562388<br />House Bill: G062388<br /><br />Submitter: CargoWise 2<br />Agent: CargoWise 2<br />Carrier: <br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 721E<br />Port of Discharge: NZAKL<br /><br />Importer: Importer for ECT<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Container</th><th>Container Status</th><th>Seal Numbers</th><th>Number of Packages</th><th>Type of Package</th></tr></thead><tr><td>GAZU0423985</td><td>Full</td><td>T04298-K, H823482X</td><td>100</td><td>PK</td></tr><tr><td>MSKU0428340</td><td>Full</td><td>Y59277</td><td>100</td><td>CT</td></tr></table><br />Message Status: (819) Delivery Order Herewith<br />method of Payment as specified.<br />Delivery Instructions<br />2 FCL(S) SAID TO CONTAIN 200 PACKAGE(S) OR ITEM(S)\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(IM1Notification, "New Zealand Customs Service - Import Declaration - TSW Notification Message", im1ContainersHtmlBody);
		}

		public void TestIM1_BioResponse()
		{
			var im1BioHtmlBody = string.Format("The following notification message has been received from <strong>the Ministry of Primary Industries Biosecurity</strong> for your information.<br />\r\nThe message is for the: <strong>Delivery Notification Party</strong>. The <strong>Import Declaration</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nInspection / Audit requirements<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 34969632<br />\r\nTSW Acceptance Time\t\t\t: 04-Nov-19 14:13<br />\r\nSender's Reference Number\t: B00004727<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 338<br />\r\n<br />\r\nMaster Bill: G00562388<br />House Bill: G062388<br /><br />Submitter: CargoWise 2<br />Agent: CargoWise 2<br />Carrier: <br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 721E<br />Port of Discharge: NZAKL<br /><br />Importer: Importer for ECT<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Container</th><th>Container Status</th><th>Seal Numbers</th><th>Number of Packages</th><th>Type of Package</th></tr></thead><tr><td>GAZU0423985</td><td>Full</td><td>T04298-K</td><td>100</td><td>PK</td></tr><tr><td>MSKU0428340</td><td>Full</td><td>Y59277</td><td>100</td><td>CT</td></tr></table><br />Message Status: (B06) MPI Biosecurity - Directions Given (Final)<br />Delivery Instructions<br />MPI APPROVAL TO MOVE TO ATF 34 North Street Wellington City Wellington New Zealand 6144\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(IM1_BioResponse, "Ministry of Primary Industries Biosecurity - Import Declaration - TSW Notification Message", im1BioHtmlBody);
		}

		public void TestIM1_MPIResponse()
		{
			var im1MPIHtmlBody = string.Format("The following notification message has been received from <strong>the Ministry of Primary Industries Food</strong> for your information.<br />\r\nThe message is for the: <strong>Delivery Notification Party</strong>. The <strong>Import Declaration</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 34969632<br />\r\nTSW Acceptance Time\t\t\t: 04-Nov-19 14:12<br />\r\nSender's Reference Number\t: B00004727<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 336<br />\r\n<br />\r\nMaster Bill: G00562388<br />House Bill: G062388<br /><br />Submitter: CargoWise 2<br />Agent: CargoWise 2<br />Carrier: <br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 721E<br />Port of Discharge: NZAKL<br /><br />Importer: Importer for ECT<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Container</th><th>Container Status</th><th>Seal Numbers</th><th>Number of Packages</th><th>Type of Package</th></tr></thead><tr><td>GAZU0423985</td><td>Full</td><td>T04298-K</td><td>100</td><td>PK</td></tr><tr><td>MSKU0428340</td><td>Full</td><td>Y59277</td><td>100</td><td>CT</td></tr></table><br />Message Status: (F04) MPI Food - Cleared\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(IM1_MPIResponse, "Ministry of Primary Industries Food - Import Declaration - TSW Notification Message", im1MPIHtmlBody);
		}

		#endregion

		#region EX1

		public void TestEX1()
		{
			var ex1HtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Export Declaration</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 88161191<br />\r\nTSW Acceptance Time\t\t\t: 23-Oct-19 19:17<br />\r\nSender's Reference Number\t: B00004707<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2800<br />\r\n<br />\r\nMaster Bill: 08111223343<br />House Bill: BKG191023HBL3<br /><br />Submitter: CargoWise 2<br />Agent: CargoWise 2<br />Carrier: AIR NEW ZEALAND (NZ) LIMITED<br />Flight No.: QF111<br />Port of Loading: NZAKL<br /><br />Exporter: Importer for ECT<br />\r\n<br />\r\n<br />Packaging: 1 PK<br />\r\n<br />Message Status: (819) Delivery Order Herewith<br />method of Payment as specified.<br />Delivery Instructions<br />1 LOOSE PACKAGE(S) OR ITEM(S)\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(EX1Notification, "New Zealand Customs Service - Export Declaration - TSW Notification Message", ex1HtmlBody);
		}

		#endregion

		#region ICR

		public void TestICR_Air()
		{
			var icrNotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Inward Cargo Report</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 82974072<br />\r\nTSW Acceptance Time\t\t\t: 16-Oct-19 19:14<br />\r\nSender's Reference Number\t: X00001700<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2771<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Flight No.: QF111<br />Arrival Date: 15-Oct-19<br />Port of Arrival: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>08111223343</td><td>BILL1</td><td>Importer for ECT</td><td>TEST SUPPLIER</td><td>Consignment Written Off/Cleared</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (C06) Customs Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(ICR_AirResponse, "New Zealand Customs Service - Inward Cargo Report - TSW Notification Message", icrNotificationHtmlBody);
		}

		public void TestICR_Sea()
		{
			var icrNotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Inward Cargo Report</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 41513525<br />\r\nTSW Acceptance Time\t\t\t: 22-Oct-19 14:58<br />\r\nSender's Reference Number\t: X00001710<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2785<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 9876<br />Arrival Date: 22-Oct-19<br />Port of Arrival: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Containers</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>BKG191022OBL1</td><td>HBILL1</td><td>Importer for ECT</td><td>TEST SUPPLIER</td><td>BKGU1122440</td><td>Consignment Written Off/Cleared</td><td>&nbsp;</td></tr><tr><td>BKG191022OBL1</td><td>HBILL2</td><td>TEST CLIENT DEFERRED 4</td><td>TEST SUPPLIER</td><td>BKGU1122440, BKGU8798652</td><td>Consignment Written Off/Cleared</td><td>&nbsp;</td></tr><tr><td>BKG191022OBL1</td><td>HBILL3</td><td>DODD GEE</td><td>TEST SUPPLIER</td><td>BKGU8798652</td><td>Consignment Held</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (C06) Customs Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(ICR_SeaResponse, "New Zealand Customs Service - Inward Cargo Report - TSW Notification Message", icrNotificationHtmlBody);
		}

		public void TestICR_Customs_EmptyContainers()
		{
			var icrCustomsEmptyContainersHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Inward Cargo Report</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 84491265<br />\r\nTSW Acceptance Time\t\t\t: 11-Nov-19 14:05<br />\r\nSender's Reference Number\t: X00001742<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2856<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 44<br />Arrival Date: 11-Nov-19<br />Port of Arrival: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Containers</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>&nbsp;</td><td>OBL</td><td>Importer for ECT</td><td>TEST SUPPLIER</td><td>AALU4715644, AALU7235490, AALU1475350</td><td>Consignment Written Off/Cleared</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (C06) Customs Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(ICRNotification_Customs, "New Zealand Customs Service - Inward Cargo Report - TSW Notification Message", icrCustomsEmptyContainersHtmlBody);
		}

		public void TestICR_Bio()
		{
			var icrBiosecurityHtmlBody = string.Format("The following notification message has been received from <strong>the Ministry of Primary Industries Biosecurity</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Inward Cargo Report</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 41513525<br />\r\nTSW Acceptance Time\t\t\t: 22-Oct-19 14:57<br />\r\nSender's Reference Number\t: X00001710<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2784<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 9876<br />Arrival Date: 22-Oct-19<br />Port of Arrival: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Containers</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>BKG191022OBL1</td><td>HBILL1</td><td>Importer for ECT</td><td>TEST SUPPLIER</td><td>BKGU1122440</td><td>Consignment Held</td><td>&nbsp;</td></tr><tr><td>BKG191022OBL1</td><td>HBILL2</td><td>TEST CLIENT DEFERRED 4</td><td>TEST SUPPLIER</td><td>BKGU1122440, BKGU8798652</td><td>Consignment Held</td><td>&nbsp;</td></tr><tr><td>BKG191022OBL1</td><td>HBILL3</td><td>DODD GEE</td><td>TEST SUPPLIER</td><td>BKGU8798652</td><td>Consignment Held</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(ICRNotification_Bio, "Ministry of Primary Industries Biosecurity - Inward Cargo Report - TSW Notification Message", icrBiosecurityHtmlBody);
		}

		#endregion

		#region CRE

		public void TestCRE_Air()
		{
			var creNotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Cargo Report Export</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 58182442<br />\r\nTSW Acceptance Time\t\t\t: 27-Sep-19 13:35<br />\r\nSender's Reference Number\t: X00001682<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2695<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Flight No.: QF118<br />Departure Date: 27-Sep-19<br />Port of Departure: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>&nbsp;</td><td>BILL1</td><td>TEST SUPPLIER</td><td>ADBOOKS LTD (NZ CUSTOMS)</td><td>Consignment Written Off/Cleared</td><td>&nbsp;</td></tr><tr><td>&nbsp;</td><td>BILL2</td><td>A CONSIGNEE</td><td>ADBOOKS LTD (NZ CUSTOMS)</td><td>Consignment Written Off/Cleared</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (C06) Customs Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(CRE_AirResponse, "New Zealand Customs Service - Cargo Report Export - TSW Notification Message", creNotificationHtmlBody);
		}

		public void TestCRE_Sea_Customs()
		{
			var creNotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Cargo Report Export</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 61654102<br />\r\nTSW Acceptance Time\t\t\t: 23-Sep-19 15:16<br />\r\nSender's Reference Number\t: X00001671<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2671<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 6645<br />Departure Date: 24-Sep-19<br />Port of Departure: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Containers</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>&nbsp;</td><td>BKG190923HBL3</td><td>SUVA ELECTRIC LAUNDRY</td><td>TEST SUPPLIER</td><td>BKGU9999991</td><td>International Transhipment Approved</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (C06) Customs Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(CRE_SeaResponse_NZCS, "New Zealand Customs Service - Cargo Report Export - TSW Notification Message", creNotificationHtmlBody);
		}

		public void TestCRE_Sea_Biosecurity()
		{
			var creNotificationHtmlBody = string.Format("The following notification message has been received from <strong>the Ministry of Primary Industries Biosecurity</strong> for your information.<br />\r\nThe message is for the: <strong>Depot (Location of Goods)</strong>. The <strong>Cargo Report Export</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 61654102<br />\r\nTSW Acceptance Time\t\t\t: 23-Sep-19 15:15<br />\r\nSender's Reference Number\t: X00001671<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2670<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Vessel: AAL FREMANTLE<br />IMO Number: 4823981<br />Voyage: 6645<br />Departure Date: 24-Sep-19<br />Port of Departure: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Consignee</th><th>Consignor</th><th>Containers</th><th>Clearance Status</th><th>Movement Status</th></tr></thead><tr><td>&nbsp;</td><td>BKG190923HBL3</td><td>SUVA ELECTRIC LAUNDRY</td><td>TEST SUPPLIER</td><td>BKGU9999991</td><td>International Transhipment Approved</td><td>&nbsp;</td></tr></table><br />\r\n<br />Message Status: (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(CRE_SeaResponse_Bio, "Ministry of Primary Industries Biosecurity - Cargo Report Export - TSW Notification Message", creNotificationHtmlBody);
		}

		#endregion

		#region OCR

		public void TestOCR()
		{
			var ocrNotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Delivery Notification Party</strong>. The <strong>Outward Cargo Report</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 66600386<br />\r\nTSW Acceptance Time\t\t\t: 09-Oct-19 11:11<br />\r\nSender's Reference Number\t: C00001622<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2728<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Departure Date: 10-Oct-19<br />Port of Departure: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Export Delivery Order</th></tr></thead><tr><td>BKG191009OBL1</td><td>BKG191009HBL1</td><td>9014883</td></tr><tr><td>&nbsp;</td><td>BKG19109HBL2</td><td>25619707</td></tr></table><br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Container</th><th>Container Status</th></tr></thead><tr><td>BKGU1111110</td><td>Full contains multiple LCL consignments</td></tr></table><br />\r\n<br />Message Status: (C07) Customs Outward Cargo Report Notification<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(OCRNotification, "New Zealand Customs Service - Outward Cargo Report - TSW Notification Message", ocrNotificationHtmlBody);
		}

		public void TestOCR_MultipleConsignments()
		{
			var ocrNotificationHtmlBody = string.Format("The following notification message has been received from <strong>New Zealand Customs Service</strong> for your information.<br />\r\nThe message is for the: <strong>Delivery Notification Party</strong>. The <strong>Outward Cargo Report</strong> Lodgement has been updated.<br />\r\nBelow are the details contained in the message:<br />\r\n<br />\r\nClearance / Acceptance Instructions<br />\r\n<br />\r\n<strong>TSW Reference Number\t\t: 10669815<br />\r\nTSW Acceptance Time\t\t\t: 15-Nov-19 13:55<br />\r\nSender's Reference Number\t: C00001651<br />\r\n</strong>\r\nMessage No\t\t\t\t\t: 2880<br />\r\n<br />\r\nSubmitter: CargoWise 2<br />Departure Date: 15-Nov-19<br />Port of Departure: NZAKL<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Master Bill</th><th>House Bill</th><th>Export Delivery Order</th></tr></thead><tr><td>GAZ00523487</td><td>G65827</td><td>63672211</td></tr><tr><td>&nbsp;</td><td>Y-5024829</td><td>13741679</td></tr><tr><td>&nbsp;</td><td>G59289</td><td>93387449</td></tr></table><br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Container</th><th>Container Status</th></tr></thead><tr><td>CHSU0023949</td><td>Full contains multiple LCL consignments</td></tr><tr><td>HLMU9682738</td><td>Full contains multiple LCL consignments</td></tr></table><br />\r\n<br />Message Status: (C07) Customs Outward Cargo Report Notification<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\n<br />\r\n<p class=\"MsoNormal\" style=\"margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt\">\r\n    The above mentioned job has received the above clearance information.</p>\r\n<br />\r\n\r\n<br />\r\n<hr />\r\n<br />\r\n<!--EndSection Details-->\r\nRegards,<br />\r\n<br />\r\nCargoWise One Administrative Messages Sender", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			AssertProcessMessage(OCR_MultipleConsignments, "New Zealand Customs Service - Outward Cargo Report - TSW Notification Message", ocrNotificationHtmlBody);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var unsolicitedDOGroup = Factory.New<GlbGroup>();
			unsolicitedDOGroup.GG_Code = "USR";
			unsolicitedDOGroup.GG_Desc = "Unsolicited Delivery Order Responses";
			var staff = unsolicitedDOGroup.Staff.AddNew();
			staff.GS_Code = "TST";
			staff.GS_FullName = "John Tester";
			staff.GS_LoginName = "JT";
			staff.GS_EmailAddress = "JohnTester@testingCompany.com";
			Factory.Save();

			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);
		}

		void AssertProcessMessage(string messageText, string emailSubject, string emailBody)
		{
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);

			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = "RES";
			message.EM_MessageText = messageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor.ProcessMessage(message);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this message", email);
			AssertEquals(1, email.CCRecipients.Count);

			AssertEquals("JohnTester@testingCompany.com", email.CCRecipients[0].Email);
			AssertEquals("Response Email Subject", emailSubject, email.Subject);
			Assert("Response Email Body Equals", email.Body.Contains(emailBody));
		}

		#region TestMessages

		const string IM1_Response = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>25004</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191106172323</IssueDateTime>
    <FunctionalReferenceID>345</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>200 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>11243008</ID>
        <AcceptanceDateTime formatCode=""204"">20191106172323</AcceptanceDateTime>
        <FunctionalReferenceID>B00004731</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">200</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20191106</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF11</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>08100023505</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>G06238</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>200</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191106172323</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20191106172323</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string IM1Notification = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>25004</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191104141309</IssueDateTime>
    <FunctionalReferenceID>337</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>2 FCL(S) SAID TO CONTAIN 200 PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>34969632</ID>
        <AcceptanceDateTime formatCode=""204"">20191104141310</AcceptanceDateTime>
        <FunctionalReferenceID>B00004727</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">5000</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20191112</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <JourneyID>721E</JourneyID>
        </BorderTransportMeans>
        <GoodsShipment>
          <Consignment>
            <TransportContractDocument>
              <ID>G00562388</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>G062388</ID>
              <TypeCode>BM</TypeCode>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>31B</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>2</SequenceNumeric>
                <DocumentSectionCode>31B</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
            <TransportEquipment>
              <SequenceNumeric>1</SequenceNumeric>
              <FullnessCode>5</FullnessCode>
              <ID>GAZU0423985</ID>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
              <Seal>
                <SequenceNumeric>1</SequenceNumeric>
                <ID>T04298-K</ID>
              </Seal>
              <Seal>
                <SequenceNumeric>2</SequenceNumeric>
                <ID>H823482X</ID>
              </Seal>
            </TransportEquipment>
            <TransportEquipment>
              <SequenceNumeric>2</SequenceNumeric>
              <FullnessCode>5</FullnessCode>
              <ID>MSKU0428340</ID>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>2</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
              <Seal>
                <SequenceNumeric>1</SequenceNumeric>
                <ID>Y59277</ID>
              </Seal>
            </TransportEquipment>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>100</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <Packaging>
          <SequenceNumeric>2</SequenceNumeric>
          <QuantityQuantity>100</QuantityQuantity>
          <TypeCode>CT</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191104141309</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20191104141309</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string IM1_BioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>25004</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191104141319</IssueDateTime>
    <FunctionalReferenceID>338</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_9FB9A99B-C5B4-4CB3-9275-68651EE67D5C"" filename=""PDF9-34969632-2019-11-04-1411889013.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>MPI APPROVAL TO MOVE TO ATF 34 North Street Wellington City Wellington New Zealand 6144</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>34969632</ID>
        <AcceptanceDateTime formatCode=""204"">20191104141319</AcceptanceDateTime>
        <FunctionalReferenceID>B00004727</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">5000</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20191112</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <JourneyID>721E</JourneyID>
        </BorderTransportMeans>
        <GoodsShipment>
          <Consignment>
            <TransportContractDocument>
              <ID>G00562388</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>G062388</ID>
              <TypeCode>BM</TypeCode>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>31B</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>2</SequenceNumeric>
                <DocumentSectionCode>31B</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
            <TransportEquipment>
              <SequenceNumeric>1</SequenceNumeric>
              <FullnessCode>5</FullnessCode>
              <ID>GAZU0423985</ID>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
              <Seal>
                <SequenceNumeric>1</SequenceNumeric>
                <ID>T04298-K</ID>
              </Seal>
            </TransportEquipment>
            <TransportEquipment>
              <SequenceNumeric>2</SequenceNumeric>
              <FullnessCode>5</FullnessCode>
              <ID>MSKU0428340</ID>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>2</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
              <Seal>
                <SequenceNumeric>1</SequenceNumeric>
                <ID>Y59277</ID>
              </Seal>
            </TransportEquipment>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>100</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <Packaging>
          <SequenceNumeric>2</SequenceNumeric>
          <QuantityQuantity>100</QuantityQuantity>
          <TypeCode>CT</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191104141319</EffectiveDateTime>
      <NameCode>B06</NameCode>
      <ReleaseDateTime formatCode=""204"">20191104141319</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string IM1_MPIResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>25004</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191104141258</IssueDateTime>
    <FunctionalReferenceID>336</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>34969632</ID>
        <AcceptanceDateTime formatCode=""204"">20191104141258</AcceptanceDateTime>
        <FunctionalReferenceID>B00004727</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">5000</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20191112</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <JourneyID>721E</JourneyID>
        </BorderTransportMeans>
        <GoodsShipment>
          <Consignment>
            <TransportContractDocument>
              <ID>G00562388</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>G062388</ID>
              <TypeCode>BM</TypeCode>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>31B</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>2</SequenceNumeric>
                <DocumentSectionCode>31B</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
            <TransportEquipment>
              <SequenceNumeric>1</SequenceNumeric>
              <FullnessCode>5</FullnessCode>
              <ID>GAZU0423985</ID>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
              <Seal>
                <SequenceNumeric>1</SequenceNumeric>
                <ID>T04298-K</ID>
              </Seal>
            </TransportEquipment>
            <TransportEquipment>
              <SequenceNumeric>2</SequenceNumeric>
              <FullnessCode>5</FullnessCode>
              <ID>MSKU0428340</ID>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>2</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
              <Seal>
                <SequenceNumeric>1</SequenceNumeric>
                <ID>Y59277</ID>
              </Seal>
            </TransportEquipment>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>100</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <Packaging>
          <SequenceNumeric>2</SequenceNumeric>
          <QuantityQuantity>100</QuantityQuantity>
          <TypeCode>CT</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191104141258</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20191104141258</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string EX1Notification = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191023191755</IssueDateTime>
    <FunctionalReferenceID>2800</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>88161191</ID>
        <AcceptanceDateTime formatCode=""204"">20191023191755</AcceptanceDateTime>
        <FunctionalReferenceID>B00004707</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">1</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF111</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <Exporter>
          <Name>Importer for ECT</Name>
        </Exporter>
        <GoodsShipment>
          <ExitDateTime formatCode=""102"">20191023</ExitDateTime>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>08111223343</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>BKG191023HBL3</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <LoadingLocation>
          <ID>NZAKL</ID>
        </LoadingLocation>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>1</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191023191755</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20191023191755</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string ICR_AirResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191016191407</IssueDateTime>
    <FunctionalReferenceID>2771</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>82974072</ID>
        <AcceptanceDateTime formatCode=""204"">20191016191407</AcceptanceDateTime>
        <FunctionalReferenceID>X00001700</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>QF111</Name>
          <TypeCode>4</TypeCode>
          <ArrivalDateTime formatCode=""102"">20191015</ArrivalDateTime>
          <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>Importer for ECT</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>BILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191016191407</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20191016191407</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string ICR_SeaResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191022145800</IssueDateTime>
    <FunctionalReferenceID>2785</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>41513525</ID>
        <AcceptanceDateTime formatCode=""204"">20191022145800</AcceptanceDateTime>
        <FunctionalReferenceID>X00001710</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <ArrivalDateTime formatCode=""102"">20191022</ArrivalDateTime>
          <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
          <JourneyID>9876</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>BKG191022OBL1</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>Importer for ECT</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU1122440</ID>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>BKG191022OBL1</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>TEST CLIENT DEFERRED 4</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <ConsignmentItem>
            <SequenceNumeric>2</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU1122440</ID>
          </TransportEquipment>
          <TransportEquipment>
            <SequenceNumeric>2</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU8798652</ID>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>BKG191022OBL1</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>DODD GEE</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU8798652</ID>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191022145800</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20191022145800</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string ICRNotification_Customs = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191111140526</IssueDateTime>
    <FunctionalReferenceID>2856</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>84491265</ID>
        <AcceptanceDateTime formatCode=""204"">20191111140526</AcceptanceDateTime>
        <FunctionalReferenceID>X00001742</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <ArrivalDateTime formatCode=""102"">20191111</ArrivalDateTime>
          <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
          <JourneyID>44</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <Consignee>
            <Name>Importer for ECT</Name>
          </Consignee>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>OBL</ID>
            <TypeCode>BM</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>4</FullnessCode>
            <ID>AALU4715644</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>S123</ID>
            </Seal>
          </TransportEquipment>
          <TransportEquipment>
            <SequenceNumeric>2</SequenceNumeric>
            <FullnessCode>4</FullnessCode>
            <ID>AALU7235490</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>S789</ID>
            </Seal>
          </TransportEquipment>
          <TransportEquipment>
            <SequenceNumeric>3</SequenceNumeric>
            <FullnessCode>4</FullnessCode>
            <ID>AALU1475350</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>S456</ID>
            </Seal>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191111140526</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20191111140526</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string ICRNotification_Bio = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191022145750</IssueDateTime>
    <FunctionalReferenceID>2784</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_E293A158-E04C-471E-A18D-CDD20800F711"" filename=""PDF7-41513525-2019-10-22-141062057.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <OverallDeclaration>
      <Declaration>
        <ID>41513525</ID>
        <AcceptanceDateTime formatCode=""204"">20191022145750</AcceptanceDateTime>
        <FunctionalReferenceID>X00001710</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <ArrivalDateTime formatCode=""102"">20191022</ArrivalDateTime>
          <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
          <JourneyID>9876</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held pending valid movement request,issuedDate:Tuesday, 22 October 2019 2:57:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <StatementDescription>seq:2,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 22 October 2019 2:57:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>BKG191022OBL1</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>Importer for ECT</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU1122440</ID>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held pending valid movement request,issuedDate:Tuesday, 22 October 2019 2:57:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <StatementDescription>seq:2,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 22 October 2019 2:57:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>BKG191022OBL1</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>TEST CLIENT DEFERRED 4</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <ConsignmentItem>
            <SequenceNumeric>2</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU1122440</ID>
          </TransportEquipment>
          <TransportEquipment>
            <SequenceNumeric>2</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU8798652</ID>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held pending valid movement request,issuedDate:Tuesday, 22 October 2019 2:57:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <StatementDescription>seq:2,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 22 October 2019 2:57:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>BKG191022OBL1</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <Consignee>
            <Name>DODD GEE</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU8798652</ID>
          </TransportEquipment>
          <UnloadingLocation>
            <ID>NZAKL</ID>
          </UnloadingLocation>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191022145750</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string CRE_AirResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESCRE</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190927133504</IssueDateTime>
    <FunctionalReferenceID>2695</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>58182442</ID>
        <AcceptanceDateTime formatCode=""204"">20190927133504</AcceptanceDateTime>
        <FunctionalReferenceID>X00001682</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>QF118</Name>
          <TypeCode>4</TypeCode>
          <DepartureDateTime formatCode=""102"">20190927</DepartureDateTime>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <Consignee>
            <Name>TEST SUPPLIER</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <ConsignmentItem>
            <SequenceNumeric>2</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>ADBOOKS LTD (NZ CUSTOMS)</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <LoadingLocation>
            <ID>NZAKL</ID>
          </LoadingLocation>
          <TransportContractDocument>
            <ID>BILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <Consignee>
            <Name>A CONSIGNEE</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">2</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>2</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>ADBOOKS LTD (NZ CUSTOMS)</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <LoadingLocation>
            <ID>NZAKL</ID>
          </LoadingLocation>
          <TransportContractDocument>
            <ID>BILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190927133504</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190927133504</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string CRE_SeaResponse_NZCS = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESCRE</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190923151612</IssueDateTime>
    <FunctionalReferenceID>2671</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>61654102</ID>
        <AcceptanceDateTime formatCode=""204"">20190923151612</AcceptanceDateTime>
        <FunctionalReferenceID>X00001671</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <DepartureDateTime formatCode=""102"">20190924</DepartureDateTime>
          <JourneyID>6645</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode>ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>AAL NEWCASTLE,1,20190920,27</StatementDescription>
            <StatementTypeCode>ITA</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <StatementCode>1C</StatementCode>
            <StatementTypeCode>MTT</StatementTypeCode>
          </AdditionalInformation>
          <Consignee>
            <Name>SUVA ELECTRIC LAUNDRY</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <LoadingLocation>
            <ID>NZAKL</ID>
          </LoadingLocation>
          <TransportContractDocument>
            <ID>BKG190923HBL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>5</FullnessCode>
            <ID>BKGU9999991</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>123</ID>
            </Seal>
          </TransportEquipment>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190923151612</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190923151612</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string CRE_SeaResponse_Bio = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESCRE</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190923151559</IssueDateTime>
    <FunctionalReferenceID>2670</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_4C021192-5F58-49FE-AB98-F412A00BD95B"" filename=""PDF18-61654102-2019-09-23-1509647015.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <OverallDeclaration>
      <Declaration>
        <ID>61654102</ID>
        <AcceptanceDateTime formatCode=""204"">20190923151559</AcceptanceDateTime>
        <FunctionalReferenceID>X00001671</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <DepartureDateTime formatCode=""102"">20190924</DepartureDateTime>
          <JourneyID>6645</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <GoodsStatusCode>ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>AAL NEWCASTLE,1,20190920,27</StatementDescription>
            <StatementTypeCode>ITA</StatementTypeCode>
          </AdditionalInformation>
          <AdditionalInformation>
            <StatementCode>1C</StatementCode>
            <StatementTypeCode>MTT</StatementTypeCode>
          </AdditionalInformation>
          <Consignee>
            <Name>SUVA ELECTRIC LAUNDRY</Name>
          </Consignee>
          <ConsignmentItem>
            <SequenceNumeric>1</SequenceNumeric>
            <GoodsMeasure>
              <GrossMassMeasure unitCode=""KGM"">1</GrossMassMeasure>
            </GoodsMeasure>
            <Packaging>
              <SequenceNumeric>1</SequenceNumeric>
              <QuantityQuantity>1</QuantityQuantity>
              <TypeCode>PK</TypeCode>
            </Packaging>
          </ConsignmentItem>
          <Consignor>
            <Name>TEST SUPPLIER</Name>
          </Consignor>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <LoadingLocation>
            <ID>NZAKL</ID>
          </LoadingLocation>
          <TransportContractDocument>
            <ID>BKG190923HBL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>5</FullnessCode>
            <ID>BKGU9999991</ID>
            <Seal>
              <SequenceNumeric>1</SequenceNumeric>
              <ID>123</ID>
            </Seal>
          </TransportEquipment>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190923151559</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190923151559</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string OCRNotification = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESOCR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191009111147</IssueDateTime>
    <FunctionalReferenceID>2728</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>66600386</ID>
        <AcceptanceDateTime formatCode=""204"">20191009111148</AcceptanceDateTime>
        <FunctionalReferenceID>C00001622</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <DepartureDateTime formatCode=""102"">20191010</DepartureDateTime>
          <JourneyID>82476</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalDocument>
            <ID>9014883</ID>
            <TypeCode>EDO</TypeCode>
          </AdditionalDocument>
          <AssociatedTransportDocument>
            <ID>BKG191009HBL1</ID>
            <TypeCode>HWB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BKG191009OBL1</ID>
            <TypeCode>MB</TypeCode>
            <Consolidator>
              <Name>EDI DEMONSTRATION SYSTEM NZ BASED IN MAKAU IN AUKL</Name>
            </Consolidator>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>BKGU1111110</ID>
          </TransportEquipment>
        </Consignment>
        <Consignment>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalDocument>
            <ID>25619707</ID>
            <TypeCode>EDO</TypeCode>
          </AdditionalDocument>
          <AssociatedTransportDocument>
            <ID>BKG19109HBL2</ID>
            <TypeCode>HWB</TypeCode>
          </AssociatedTransportDocument>
        </Consignment>
        <ExitOffice>
          <ID>NZAKL</ID>
        </ExitOffice>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191009111147</EffectiveDateTime>
      <NameCode>C07</NameCode>
      <ReleaseDateTime formatCode=""204"">20191009111147</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string OCR_MultipleConsignments = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESOCR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191115135503</IssueDateTime>
    <FunctionalReferenceID>2880</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>10669815</ID>
        <AcceptanceDateTime formatCode=""204"">20191115135503</AcceptanceDateTime>
        <FunctionalReferenceID>C00001651</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <BorderTransportMeans>
          <Name>AAL FREMANTLE</Name>
          <ID>4823981</ID>
          <TypeCode>1</TypeCode>
          <DepartureDateTime formatCode=""102"">20191115</DepartureDateTime>
          <JourneyID>712N</JourneyID>
        </BorderTransportMeans>
        <Consignment>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalDocument>
            <ID>63672211</ID>
            <TypeCode>EDO</TypeCode>
          </AdditionalDocument>
          <AssociatedTransportDocument>
            <ID>G65827</ID>
            <TypeCode>HWB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>GAZ00523487</ID>
            <TypeCode>MB</TypeCode>
            <Consolidator>
              <Name>EDI DEMONSTRATION SYSTEM NZ BASED IN MAKAU IN AUKL</Name>
            </Consolidator>
          </TransportContractDocument>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>CHSU0023949</ID>
          </TransportEquipment>
          <TransportEquipment>
            <SequenceNumeric>2</SequenceNumeric>
            <FullnessCode>7</FullnessCode>
            <ID>HLMU9682738</ID>
          </TransportEquipment>
        </Consignment>
        <Consignment>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalDocument>
            <ID>13741679</ID>
            <TypeCode>EDO</TypeCode>
          </AdditionalDocument>
          <AssociatedTransportDocument>
            <ID>Y-5024829</ID>
            <TypeCode>HWB</TypeCode>
          </AssociatedTransportDocument>
        </Consignment>
        <Consignment>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalDocument>
            <ID>93387449</ID>
            <TypeCode>EDO</TypeCode>
          </AdditionalDocument>
          <AssociatedTransportDocument>
            <ID>G59289</ID>
            <TypeCode>HWB</TypeCode>
          </AssociatedTransportDocument>
        </Consignment>
        <ExitOffice>
          <ID>NZAKL</ID>
        </ExitOffice>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191115135503</EffectiveDateTime>
      <NameCode>C07</NameCode>
      <ReleaseDateTime formatCode=""204"">20191115135503</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#endregion
	}
}
