using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class BookingRequestEventProcessorNoShippingLineTest : DataObjects.Testing.OCBEventProcessorTest
	{
		#region Implementation

		protected override DocDataObject DocDataObject
		{
			get
			{
				var parameters = new DummyDocDataObjectParameters()
				{
					LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
				};

				return new BookingRequestBuilder(Consol, parameters).Build();
			}
		}

		protected override string Context => DataContext.BookingRequest;
		protected override string DocumentName => ConsolDocumentNames.BookingRequest;
		protected override string DataStoreName => ConsolDocumentDataStoreNames.SeaBookingRequest2;
		protected override string ExpectedOrgMessage => "|CMP=9876|MAX=10.50|NEW=10.50|OLD=0|QTY=10.50|STA=ORG|TYP=Booking Request";
		protected override string ExpectedAmdMessage => "|CMP=9876|MAX=150.00|NEW=150.00|OLD=10.50|QTY=139.50|STA=AMD|TYP=Booking Request";
		protected override string ExpectedWthMessage => "|CMP=9876|MAX=150.00|NEW=150.00|OLD=150.00|QTY=0.00|STA=WTH|TYP=Booking Request";
		protected override string ExpectedDocumentDeliveryMessage => "|ACT=Document Delivery|CMP=9876|MAX=10.50|NEW=10.50|OLD=0|QTY=10.50|STA=ORG|TYP=Booking Request";

		protected override ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.AgentConsol;
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_AgentsReference = "AgentRef002";
			consol.JK_MasterBillNum = "1112222222";
			consol.JK_CoLoadMasterBill = "COLOAD004";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAERSK";
			org.OH_RL_NKClosestPort = "DKAAL";
			org.MainAddress.Address1 = "Unit 13";
			org.MainAddress.Address2 = "4 Lost Lane";
			org.MainAddress.City = "Aalborg";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.OA_RN_NKCountryCode = "DK";
			org.OH_IsShippingProvider = true;
			org.OH_IsShippingLine = true;
			org.OH_IsSeaWholesaler = false;

			var customscode = org.CustomsCodes.AddNew();
			customscode.OK_RN_NKCodeCountry = "US";
			customscode.OK_CodeType = "CCC";
			customscode.OK_CustomsRegNo = "9876";

			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "9876";
			shippingLine.RSL_CargoWiseOneCode = "AAAA";
			shippingLine.RSL_BookingRequestAvailable = true;
			shippingLine.RSL_ShippingInstructionAvailable = false;
			shippingLine.RSL_ShippingOrderAvailable = false;
			shippingLine.RSL_IsNVO = false;

			var mainTransport = consol.Transports[0];
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			mainTransport.JW_Vessel = vessel.RV_Code;
			mainTransport.JW_VoyageFlight = "F9999";

			var refcontainer1 = Factory.New<RefContainer>();
			refcontainer1.RC_Code = "Test";
			refcontainer1.RC_TEU = 5m;

			var refcontainer2 = Factory.New<RefContainer>();
			refcontainer2.RC_Code = "T_st";
			refcontainer2.RC_TEU = 0m;
			refcontainer2.RC_ISOType = "1";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 2;
			container1.JC_RC = refcontainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_RC = refcontainer2.PK;

			Factory.Save();

			return consol;
		}

		protected override void UpdateConsol()
		{
			Consol.Containers.RemoveAndDeleteAll();

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "Te_t";
			refcontainer.RC_TEU = 5m;

			var container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA00000121";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_ContainerCount = 30;
			container1.JC_RC = refcontainer.PK;

			Factory.Save();
		}

		#endregion
	}
}
