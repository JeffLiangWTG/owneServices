using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class USLowValueConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<USLowValueConverter>
	{
		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates()
		{
			var transportModeList = new string[] { TransportModes.Road, TransportModes.Sea, TransportModes.Air };

			foreach (var transportMode in transportModeList)
			{
				var shipment = SetupTestShipment(transportMode);
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
				{
					var success = converter.TryConvert(out var errorMsg);
					Assert("Convert successfully", success);
					AssertNullOrEmpty("No error occured", errorMsg);

					var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;
					AssertNotNull("new clearance should be created", clearance);
					Assert("new clearance should not be in DataBase", !clearance.IsInDatabase);

					var consignments = clearance.CusUSLVConsignments;
					AssertEquals("2 consignments should be created", 2, consignments.Count);

					var converterFactory = clearance.Factory;
					converterFactory.Save();

					var hlrLogs = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "HLR");
					AssertEquals("1 HLR event should be added to shipment", 1, hlrLogs.Count());
					AssertEquals("New HLR event should have reason Cargo Report Created", "|RES=Cargo Report Created", hlrLogs.First().SL_Reference);
				}
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_SCAC_Sea()
		{
			var shipment = SetupTestShipment(TransportModes.Sea);

			var shippingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = shippingOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "MEDU";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode2 = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "CCC";
			cusCode2.OK_CustomsRegNo = "ABCD";
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			var consol = shipment.ArrivalConsol;
			consol.JK_OA_ShippingLineAddress = shippingOrg.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;

				AssertNotNull("New master bill should be created", clearance);
				AssertEquals("Should contain issuer SCAC code", "MEDU", clearance.ULH_MasterBillIssuerSCAC);
				AssertEquals("Should contain carrier SCAC code", "MEDU", clearance.ULH_CarrierSCAC);

				var consignment = clearance.CusUSLVConsignments[0];

				AssertNotNull("New house bill should be created", consignment);
				AssertEquals("Should contain house bill issuer SCAC code", "ABCD", consignment.ULB_HouseBillIssuerSCAC);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_SCAC_Rail()
		{
			var shipment = SetupTestShipment(TransportModes.Rail);

			var shippingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = shippingOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "MEDU";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode2 = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "CCC";
			cusCode2.OK_CustomsRegNo = "ABCD";
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			var consol = shipment.ArrivalConsol;
			consol.JK_OA_ShippingLineAddress = shippingOrg.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;

				AssertNotNull("New master bill should be created", clearance);
				AssertEquals("Should contain issuer SCAC code", "MEDU", clearance.ULH_MasterBillIssuerSCAC);
				AssertEquals("Should contain carrier SCAC code", "MEDU", clearance.ULH_CarrierSCAC);

				var consignment = clearance.CusUSLVConsignments[0];

				AssertNotNull("New house bill should be created", consignment);
				AssertEquals("Should contain house bill issuer SCAC code", "ABCD", consignment.ULB_HouseBillIssuerSCAC);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_SCAC_Air()
		{
			var shipment = SetupTestShipment(TransportModes.Air);

			var shippingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = shippingOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "MEDU";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode2 = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "CCC";
			cusCode2.OK_CustomsRegNo = "ABCD";
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			var consol = shipment.ArrivalConsol;
			consol.JK_OA_ShippingLineAddress = shippingOrg.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;

				AssertNotNull("New master bill should be created", clearance);
				AssertNotEquals("Should not contain issuer SCAC code", "MEDU", clearance.ULH_MasterBillIssuerSCAC);
				AssertNotEquals("Should not contain carrier SCAC code", "MEDU", clearance.ULH_CarrierSCAC);

				var consignment = clearance.CusUSLVConsignments[0];

				AssertNotNull("New house bill should be created", consignment);
				AssertNotEquals("Should not contain house bill issuer SCAC code", "ABCD", consignment.ULB_HouseBillIssuerSCAC);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_SCAC_FallbackToCurrentOrgProxy_Sea()
		{
			var shipment = SetupTestShipment(TransportModes.Sea);

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgProxy.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;
				var consignment = clearance.CusUSLVConsignments[0];

				AssertNotNull("New house bill should be created", consignment);
				AssertEquals("Should contain house bill issuer SCAC code from org proxy", "ABCD", consignment.ULB_HouseBillIssuerSCAC);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_SCAC_FallbackToCurrentOrgProxy_Rail()
		{
			var shipment = SetupTestShipment(TransportModes.Rail);

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgProxy.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;
				var consignment = clearance.CusUSLVConsignments[0];

				AssertNotNull("New house bill should be created", consignment);
				AssertEquals("Should contain house bill issuer SCAC code from org proxy", "ABCD", consignment.ULB_HouseBillIssuerSCAC);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_SCAC_FallbackToCurrentOrgProxy_Air()
		{
			var shipment = SetupTestShipment(TransportModes.Air);

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgProxy.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;
				var consignment = clearance.CusUSLVConsignments[0];

				AssertNotNull("New house bill should be created", consignment);
				AssertNotEquals("Should not contain house bill issuer SCAC code from org proxy", "ABCD", consignment.ULB_HouseBillIssuerSCAC);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_NumberOfPacks()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = CountryCodes.UnitedStates;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_HouseBill = "UWM97N872947";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "08138374491";
			consol.Shipments.Add(shipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item1 = consignment.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var item2 = consignment.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var item3 = consignment.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = item1.Lines.AddNew();
			var itemLine2 = item1.Lines.AddNew();
			var itemLine3 = item1.Lines.AddNew();
			itemLine1.HVS_Quantity = 1;
			itemLine2.HVS_Quantity = 2;
			itemLine3.HVS_Quantity = 4;

			var itemLine4 = item2.Lines.AddNew();
			var itemLine5 = item2.Lines.AddNew();
			itemLine4.HVS_Quantity = 1;
			itemLine5.HVS_Quantity = 1;

			var itemLine6 = item3.Lines.AddNew();
			itemLine6.HVS_Quantity = 1;

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var success = converter.TryConvert(out var errorMsg);

				CombineAssertions(() =>
				{
					Assert("The shipment was not converted for customs successfully", success);
					AssertNullOrEmpty("The following error occured when converting shipment for customs ", errorMsg);
				});

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;
				var cusUSLVConsignment = clearance.CusUSLVConsignments.FirstOrDefault() as CusUSLVConsignment;

				AssertNotNull("New house bill should be created", consignment);
				AssertEquals("Expected House Bill to have a quantity of 3", (ZShort)3, cusUSLVConsignment.ULB_NumberOfPacks);
			}
		}

		public void TestConvertShipmentToCargoReport_EndToEnd_UnitedStates_PortOfEntry()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "US000";
			unloco.RL_PortName = "USTestPort";

			CreateLocoMapIfNotExists("3786", "US000", USLocoMapSystemUsageList.Codes.Sea, isSystem: true);
			CreateLocoMapIfNotExists("4444", "US000", USLocoMapSystemUsageList.Codes.SCD, isSystem: true);

			var shipment = SetupTestShipment(TransportModes.Sea);

			var consol = shipment.ArrivalConsol;
			consol.JK_RL_NKPortOfFirstArrival = "US000";

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				converter.TryConvert(out var errorMsg);

				var clearance = converter.CustomsRelatedBusinessCollection.Single() as CusUSLVClearance;

				AssertEquals("Sea SchD port code should be selected", "3786", clearance.ULH_PortOfEntry);
			}
		}

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new USLowValueCommand(shipment);

		protected override ZString LoginCountry => CountryCodes.UnitedStates;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Road };

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment) => Factory.NewWithValidTestData<CusUSLVClearance>();

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((CusUSLVClearance)existingJob).ULH_JobNumber;

		void CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			if (isSystem)
			{
				codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);
			}

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}
		}
	}
}
