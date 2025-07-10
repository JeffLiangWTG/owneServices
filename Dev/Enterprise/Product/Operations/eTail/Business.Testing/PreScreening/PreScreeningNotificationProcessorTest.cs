using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.eTail.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.eTail.Business.Testing
{
	class PreScreeningNotificationProcessorTest : TestCaseWithFactory
	{
		public void TestResultForBookingHeader_WhenPreScreeningFailed_IsCreatedAndSent()
		{
			SetUpPreScreeningConfiguration(true);
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Goods Description";
			field1.ValidationRule = "ERR";
			field1.MessageText = "Error Message On Field Level";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "XBOX";
			screeningValue1.ScreeningComparisonOperatorCode = "Contains";
			screeningValue1.MessageTextPerValue = "Error Message On Value Level - XBOX";

			var billToParty = GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = GenerateAddress("AD2", "XY2", "XY 2");
			var originDepot = GenerateAddress("AD3", "XY3", "XY 3");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;
			bookingHeader.HVH_OA_OriginDepot = originDepot.PK;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "TST";
			group.GG_DomainName = "Tester";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";
			staff.GS_FullName = "TesterStaff";
			group.Staff.Add(staff);
			Factory.Save();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "WAYBILL001";
			consignment1.HVC_GoodsDescription = "I HAVE ONE XBOX";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "WAYBILL002";
			consignment2.HVC_GoodsDescription = "I HAVE ONE PlayStation";

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var provider = new ETailPreScreeningProvider(bookingHeader);
				provider.Screen();
				provider.SendPreScreeningNotificationEmail();
			}

			AssertEquals("1 email has been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var emailCreated = Env.OutgoingMailManager.EmailsCreated[0];
			var expectedSubject = "HVLV Pre-Screening Report – FAL Consignments for HVLV Booking Header M00000050";
			var expectedTable = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Consignment ID</th><th>Consignment Waybill #</th><th>Consignment Shipper Ref #</th><th>Pre-Screening Status</th>" +
				"<th>Validation Rule</th><th>Message</th><th>Consignment Hyperlink</th></tr></thead><tr><td>&nbsp;</td>" +
				"<td>WAYBILL001</td><td>&nbsp;</td><td>FAL</td><td>ERR</td><td>Goods Description Screening Error - Error Message On Value Level - XBOX.</td><td>" +
				$"<a href=\"{ShowEditFormUrlHandler.Instance.Create(ControllerIDs.HVLVConsignment, consignment1.PK)}";

			AssertEquals(staff.GS_EmailAddress, emailCreated.Recipients[0].Email);
			AssertEquals(expectedSubject, emailCreated.Subject);
			AssertEquals(staff.GS_EmailAddress, emailCreated.Recipients[0].Email);
			AssertEquals(expectedSubject, emailCreated.Subject);
			AssertContains("BOOKING HEADER DETAILS:", emailCreated.Body);
			AssertContains("Bill To Party: XY 1", emailCreated.Body);
			AssertContains("Dispatch Address: XY 2", emailCreated.Body);
			AssertContains("Origin Depot: XY 3", emailCreated.Body);
			AssertContains("HVLV Pre-Screening Report:", emailCreated.Body);
			AssertContains(expectedTable, emailCreated.Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestResultForBookingHeader_WhenPreScreeningFailedViaPRETrigger_IsCreatedAndSent()
		{
			SetUpPreScreeningConfiguration(true);
			var field1 = rule.Fields.AddNew();
			field1.FieldDescription = "Goods Description";
			field1.ValidationRule = "ERR";
			field1.MessageText = "Error Message On Field Level";

			var screeningValue1 = field1.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "XBOX";
			screeningValue1.ScreeningComparisonOperatorCode = "Contains";
			screeningValue1.MessageTextPerValue = "Error Message On Value Level - XBOX";

			var billToParty = GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = GenerateAddress("AD2", "XY2", "XY 2");
			var originDepot = GenerateAddress("AD3", "XY3", "XY 3");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;
			bookingHeader.HVH_OA_OriginDepot = originDepot.PK;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "TST";
			group.GG_DomainName = "Tester";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";
			staff.GS_FullName = "TesterStaff";
			group.Staff.Add(staff);
			Factory.Save();

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "WAYBILL001";
			consignment1.HVC_GoodsDescription = "I HAVE ONE XBOX";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "WAYBILL002";
			consignment2.HVC_GoodsDescription = "I HAVE ONE PlayStation";

			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
			{
				var processor = new HVLVPreScreeningProcessor(bookingHeader);
				processor.Process(new NotificationCollection());
			}

			AssertEquals("1 email has been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var emailCreated = Env.OutgoingMailManager.EmailsCreated[0];
			var expectedSubject = "HVLV Pre-Screening Report – FAL Consignments for HVLV Booking Header M00000050";
			var expectedTable = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
				"<tr class=\"tableheadings\"><th>Consignment ID</th><th>Consignment Waybill #</th><th>Consignment Shipper Ref #</th><th>Pre-Screening Status</th>" +
				"<th>Validation Rule</th><th>Message</th><th>Consignment Hyperlink</th></tr></thead><tr><td>&nbsp;</td>" +
				"<td>WAYBILL001</td><td>&nbsp;</td><td>FAL</td><td>ERR</td><td>Goods Description Screening Error - Error Message On Value Level - XBOX.</td><td>" +
				$"<a href=\"{ShowEditFormUrlHandler.Instance.Create(ControllerIDs.HVLVConsignment, consignment1.PK)}";

			AssertEquals(staff.GS_EmailAddress, emailCreated.Recipients[0].Email);
			AssertEquals(expectedSubject, emailCreated.Subject);
			AssertEquals(staff.GS_EmailAddress, emailCreated.Recipients[0].Email);
			AssertEquals(expectedSubject, emailCreated.Subject);
			AssertContains("BOOKING HEADER DETAILS:", emailCreated.Body);
			AssertContains("Bill To Party: XY 1", emailCreated.Body);
			AssertContains("Dispatch Address: XY 2", emailCreated.Body);
			AssertContains("Origin Depot: XY 3", emailCreated.Body);
			AssertContains("HVLV Pre-Screening Report:", emailCreated.Body);
			AssertContains(expectedTable, emailCreated.Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestResultForShipment_WhenPreScreeningFailed_IsCreatedAndSent()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				SetUpPreScreeningConfiguration(true, HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment);
				var field1 = rule.Fields.AddNew();
				field1.FieldDescription = "Goods Description";
				field1.ValidationRule = "ERR";
				field1.MessageText = "Error Message On Field Level";

				var screeningValue1 = field1.ScreeningValues.AddNew();
				screeningValue1.ScreeningValue = "XBOX";
				screeningValue1.ScreeningComparisonOperatorCode = "Contains";
				screeningValue1.MessageTextPerValue = "Error Message On Value Level - XBOX";

				var eTailer = GenerateAddress("AD1", "XY1", "XY 1");
				var consignee = GenerateAddress("AD2", "XY2", "XY 2");

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_TransportMode = TransportMode.AIR;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = eTailer.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.PK;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";

				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Code = "TST";
				group.GG_DomainName = "Tester";

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "staff@test.com";
				staff.GS_FullName = "TesterStaff";
				group.Staff.Add(staff);

				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_ConsignmentId = string.Empty;
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_WaybillNumber = "WAYBILL001";
				consignment1.HVC_ShipperReference = "SHIPPERREFERENCE001";
				consignment1.HVC_GoodsDescription = "I HAVE ONE XBOX";

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_ConsignmentId = string.Empty;
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_WaybillNumber = "WAYBILL002";
				consignment2.HVC_ShipperReference = "SHIPPERREFERENCE002";
				consignment2.HVC_GoodsDescription = "I HAVE ONE PlayStation";

				Factory.Save();

				using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
				{
					var provider = new ETailPreScreeningProvider(shipment.GetHVLVConsignmentHeader());
					provider.Screen();
					provider.SendPreScreeningNotificationEmail();
				}

				AssertEquals("1 email has been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var emailCreated = Env.OutgoingMailManager.EmailsCreated[0];
				var expectedSubject = "HVLV Pre-Screening Report – FAL Consignments for Shipment EBM22Q33TU475BXH3P60";
				var expectedTable = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
					"<thead><tr class=\"tableheadings\"><th>Consignment ID</th><th>Consignment Waybill #</th><th>Consignment Shipper Ref #</th><th>Pre-Screening Status</th>" +
					"<th>Validation Rule</th><th>Message</th><th>Consignment Hyperlink</th></tr></thead><tr><td>WAYBILL001</td><td>WAYBILL001</td>" +
					"<td>SHIPPERREFERENCE001</td><td>FAL</td><td>ERR</td><td>Goods Description Screening Error - Error Message On Value Level - XBOX.</td><td>" +
					$"<a href=\"{ShowEditFormUrlHandler.Instance.Create(ControllerIDs.HVLVConsignment, consignment1.PK)}";
				AssertEquals(staff.GS_EmailAddress, emailCreated.Recipients[0].Email);
				AssertEquals(expectedSubject, emailCreated.Subject);
				AssertContains("Transport Mode: AIR", emailCreated.Body);
				AssertContains("eTailer: XY 1", emailCreated.Body);
				AssertContains("Consignee: XY 2", emailCreated.Body);
				AssertContains("Origin: USLAX", emailCreated.Body);
				AssertContains("Destination: AUSYD", emailCreated.Body);
				AssertContains("HVLV Pre-Screening Report:", emailCreated.Body);
				AssertContains(expectedTable, emailCreated.Body);

				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestResultForShipment_WhenPreScreeningFailedViaPRETrigger_IsCreatedAndSent()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				SetUpPreScreeningConfiguration(true, HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment);
				var field1 = rule.Fields.AddNew();
				field1.FieldDescription = "Goods Description";
				field1.ValidationRule = "ERR";
				field1.MessageText = "Error Message On Field Level";

				var screeningValue1 = field1.ScreeningValues.AddNew();
				screeningValue1.ScreeningValue = "XBOX";
				screeningValue1.ScreeningComparisonOperatorCode = "Contains";
				screeningValue1.MessageTextPerValue = "Error Message On Value Level - XBOX";

				var eTailer = GenerateAddress("AD1", "XY1", "XY 1");
				var consignee = GenerateAddress("AD2", "XY2", "XY 2");

				var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
				shipment.JS_TransportMode = TransportMode.AIR;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = eTailer.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.PK;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";

				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Code = "TST";
				group.GG_DomainName = "Tester";

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_EmailAddress = "staff@test.com";
				staff.GS_FullName = "TesterStaff";
				group.Staff.Add(staff);

				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_ConsignmentId = string.Empty;
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_WaybillNumber = "WAYBILL001";
				consignment1.HVC_ShipperReference = "SHIPPERREFERENCE001";
				consignment1.HVC_GoodsDescription = "I HAVE ONE XBOX";

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_ConsignmentId = string.Empty;
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_WaybillNumber = "WAYBILL002";
				consignment2.HVC_ShipperReference = "SHIPPERREFERENCE002";
				consignment2.HVC_GoodsDescription = "I HAVE ONE PlayStation";

				Factory.Save();

				using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, PreScreeningConfiguration))
				{
					var processor = new HVLVPreScreeningProcessor(shipment);
					processor.Process(new NotificationCollection());
				}

				AssertEquals("1 email has been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var emailCreated = Env.OutgoingMailManager.EmailsCreated[0];
				var expectedSubject = "HVLV Pre-Screening Report – FAL Consignments for Shipment EBM22Q33TU475BXH3P60";
				var expectedTable = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
					"<thead><tr class=\"tableheadings\"><th>Consignment ID</th><th>Consignment Waybill #</th><th>Consignment Shipper Ref #</th><th>Pre-Screening Status</th>" +
					"<th>Validation Rule</th><th>Message</th><th>Consignment Hyperlink</th></tr></thead><tr><td>WAYBILL001</td><td>WAYBILL001</td>" +
					"<td>SHIPPERREFERENCE001</td><td>FAL</td><td>ERR</td><td>Goods Description Screening Error - Error Message On Value Level - XBOX.</td><td>" +
					$"<a href=\"{ShowEditFormUrlHandler.Instance.Create(ControllerIDs.HVLVConsignment, consignment1.PK)}";
				AssertEquals(staff.GS_EmailAddress, emailCreated.Recipients[0].Email);
				AssertEquals(expectedSubject, emailCreated.Subject);
				AssertContains("Transport Mode: AIR", emailCreated.Body);
				AssertContains("eTailer: XY 1", emailCreated.Body);
				AssertContains("Consignee: XY 2", emailCreated.Body);
				AssertContains("Origin: USLAX", emailCreated.Body);
				AssertContains("Destination: AUSYD", emailCreated.Body);
				AssertContains("HVLV Pre-Screening Report:", emailCreated.Body);
				AssertContains(expectedTable, emailCreated.Body);

				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		#region Implementation

		HVLVDetailsPreScreeningConfiguration PreScreeningConfiguration;

		HVLVPreScreeningRule rule;

		void SetUpPreScreeningConfiguration(bool isEnable, string moduleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader)
		{
			PreScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration();
			PreScreeningConfiguration.IsEnabled = isEnable;

			rule = PreScreeningConfiguration.Rules.AddNew();
			rule.ModuleType = moduleType;
			if (moduleType != HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader)
			{
				rule.TransportMode = TransportMode.AIR;
			}

			rule.EmailNotificationType = Core.Constants.EmailTo.NominatedGroup;
			rule.EmailNotificationGroup = "TST";
		}

		public OrgAddress GenerateAddress(string addressCode, string headerCode, string headerFullName)
		{
			var orgHeader = GenerateOrganisation(headerCode, headerFullName);
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = addressCode;
			orgAddress.OA_OH = orgHeader.PK;

			return orgAddress;
		}

		public OrgHeader GenerateOrganisation(string headerCode, string headerFullName)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = headerCode;
			orgHeader.OH_FullName = headerFullName;
			orgHeader.OH_IsShippingProvider = true;

			return orgHeader;
		}

		#endregion
	}
}
