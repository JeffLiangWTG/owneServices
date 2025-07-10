using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ConsolCargoMessageSenderTest : TestCaseWithFactory
	{
		#region Send Cargo Message

		public void TestSendAirCargoMessage_NotGlobalRun()
		{
			var branch = SetUpAustralianBranch();
			Factory.Save();

			var messageHepler = new ConsolCustomsCargoMessageHelper(AirConsol, Notifications, false);
			int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			RunAction(branch, () => { messageHepler.SendConsolCargoMessage(); });
			AssertEquals(messageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

			SystemDataRegistry.Instance.AutomaticallySendAirCargoMessage.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			RunAction(branch, () => { messageHepler.SendConsolCargoMessage(); });
			AssertEquals(messageCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals(branch, Factory.LoadTop1<EDIMessage>(new ZQuery()).Branch);
		}

		public void TestSendAirCargoMessage_GlobalRun()
		{
			var branch = SetUpAustralianBranch();
			var anotherBranch = SetUpOtherAustralianCompanyBranch();
			Factory.Save();

			var oneTestAustraliaCompany = GlbCompany.CurrentCompany;
			var messageHepler = new ConsolCustomsCargoMessageHelper(AirConsol, Notifications);
			int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			RunAction(branch, () => { messageHepler.SendConsolCargoMessage(); });
			AssertEquals(messageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			Assert(Notifications.AsString.Contains("No AU company is set up with CMR company key file - No message sent."));

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(anotherBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(oneTestAustraliaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			RunAction(branch, () => { messageHepler.SendConsolCargoMessage(); });
			AssertEquals(messageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			Assert(Notifications.AsString.Contains("Multiple AU companies are set up with CMR company key files - No message sent."));

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(oneTestAustraliaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			RunAction(branch, () => { messageHepler.SendConsolCargoMessage(); });
			AssertEquals(messageCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			var ediMessageQuery1 = new ZQuery(EDIMessageSchema.EM_GB, branch.PK);
			AssertNotNull(Factory.Load<EDIMessage>(ediMessageQuery1));

			var cusMAWB = AirConsol.AUCusMAWB as CusMAWB;
			cusMAWB.ChildBills[0].CS_MsgStatus = "NOT";
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			RunAction(branch, () => { messageHepler.SendConsolCargoMessage(); });
			AssertEquals(messageCount + 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
			var ediMessageQuery2 = new ZQuery(EDIMessageSchema.EM_GB, anotherBranch.PK);
			AssertNotNull(Factory.Load<EDIMessage>(ediMessageQuery2));
		}

		public void TestSendSeaCargoMessage_NotGlobalRun()
		{
			var branch = SetUpAustralianBranch();
			ObjectFactory.New<Enterprise.Integration.Customs.AU.ICertificateManagerHelper>(Factory).CreateCustomsCertificates();
			Factory.Save();

			var messageHelper = new ConsolCustomsCargoMessageHelper(SeaConsol, Notifications, false);
			int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			RunAction(branch, () =>
			{
				messageHelper.SendConsolCargoMessage();
			});
			AssertEquals(messageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

			SystemDataRegistry.Instance.AutomaticallySendSeaCargoMessage.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			Env.Registry.RawRegistry.AUCCompanyCertificatePassword.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "pwd");
			RunAction(branch, () =>
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "TEST";
				messageHelper.SendConsolCargoMessage();
			});
			AssertEquals(messageCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals(branch, Factory.LoadTop1<EDIMessage>(new ZQuery()).Branch);
		}

		public void TestSendSeaCargoMessage_GlobalRun()
		{
			var branch = SetUpAustralianBranch();
			var anotherBranch = SetUpOtherAustralianCompanyBranch();
			ObjectFactory.New<Enterprise.Integration.Customs.AU.ICertificateManagerHelper>(Factory).CreateCustomsCertificates();
			Factory.Save();

			var oneTestAustraliaCompany = GlbCompany.CurrentCompany;
			var messageHepler = new ConsolCustomsCargoMessageHelper(SeaConsol, Notifications);
			int messageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			RunAction(branch, () =>
			{
				messageHepler.SendConsolCargoMessage();
			});
			AssertEquals(messageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			Assert(Notifications.AsString.Contains("No AU company is set up with CMR company key file - No message sent."));

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(oneTestAustraliaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(anotherBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			RunAction(branch, () =>
			{
				messageHepler.SendConsolCargoMessage();
			});
			AssertEquals(messageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
			Assert(Notifications.AsString.Contains("Multiple AU companies are set up with CMR company key files - No message sent."));

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(oneTestAustraliaCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			Env.Registry.RawRegistry.AUCCompanyCertificatePassword.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "pwd");
			RunAction(branch, () =>
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "TEST";
				messageHepler.SendConsolCargoMessage();
			});
			AssertEquals(messageCount + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			var messageBranchQuery1 = new ZQuery(EDIMessageSchema.EM_GB, branch.PK);
			AssertNotNull(Factory.Load<EDIMessage>(messageBranchQuery1));

			var oceanBill = SeaConsol.AUCMRCusSCAOceanBill as BaseCusSCAOceanBill;
			var houseBill = oceanBill.LoadChildren<DefaultCusSCAHouse>(CusSCAHouseSchema.CA_CB)[0];
			houseBill.CA_MessageStatus = "NOT";

			RunAction(branch, () =>
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "TEST";
				messageHepler.SendConsolCargoMessage();
			});
			AssertEquals(messageCount + 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
			var messageBranchQuery2 = new ZQuery(EDIMessageSchema.EM_GB, anotherBranch.PK);
			AssertNotNull(Factory.Load<EDIMessage>(messageBranchQuery2));
		}

		#endregion

		#region Create Cargo Job With SAC

		public void TestCreateCargoJobWithSAC_NotGlobalRun()
		{
			var branch = SetUpAustralianBranch();
			Factory.Save();

			var messageHepler = new ConsolCustomsCargoMessageHelper(AirConsol, Notifications, false);

			var masterBillQuery = new ZQuery(CusMAWBSchema.CM_JK, AirConsol.PK);
			int messageCount = Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length;
			RunAction(branch, () => { messageHepler.CreateConsolCargoJobWithSACFlag(); });
			AssertEquals(messageCount, Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length);

			SystemDataRegistry.Instance.AutomaticallyCreateAirCargoJob.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			RunAction(branch, () => { messageHepler.CreateConsolCargoJobWithSACFlag(); });
			AssertEquals(messageCount + 1, Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length);
		}

		public void TestCreateCargoJobWithSAC_GlobalRun()
		{
			var branch = SetUpAustralianBranch();
			var anotherBranch = SetUpOtherAustralianCompanyBranch();
			Factory.Save();

			var messageHepler = new ConsolCustomsCargoMessageHelper(AirConsol, Notifications);

			var masterBillQuery = new ZQuery(CusMAWBSchema.CM_JK, AirConsol.PK);
			int messageCount = Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length;
			RunAction(branch, () => { messageHepler.CreateConsolCargoJobWithSACFlag(); });
			AssertEquals(messageCount, Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length);
			Assert(Notifications.AsString.Contains("No AU company is set up with CMR company key file - No message sent."));

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(anotherBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			RunAction(branch, () => { messageHepler.CreateConsolCargoJobWithSACFlag(); });
			AssertEquals(messageCount, Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length);
			Assert(Notifications.AsString.Contains("Multiple AU companies are set up with CMR company key files - No message sent."));

			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 });
			RunAction(branch, () => { messageHepler.CreateConsolCargoJobWithSACFlag(); });
			AssertEquals(messageCount + 1, Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length);
			var cusMAWBBranchQuery1 = new ZQuery(CusMAWBSchema.CM_GB, branch.PK);
			AssertNotNull(Factory.Load<CusMAWB>(cusMAWBBranchQuery1));

			var cusMAWB = Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery) as CusMAWB[];
			cusMAWB.DeleteAll();
			messageCount = Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length;
			Env.Registry.RawRegistry.AUCCompanyCertificateData.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			RunAction(branch, () => { messageHepler.CreateConsolCargoJobWithSACFlag(); });
			AssertEquals(messageCount + 1, Factory.Load<Enterprise.Integration.Customs.AU.ICusMAWB>(masterBillQuery).Length);
			var cusMAWBBranchQuery2 = new ZQuery(CusMAWBSchema.CM_GB, anotherBranch.PK);
			AssertNotNull(Factory.Load<CusMAWB>(cusMAWBBranchQuery2));
		}

		#endregion

		#region Implementation

		void RunAction(GlbBranch branch, Action action)
		{
			using (branch.SetAsTemporaryContext())
			{
				action();
			}
		}

		GlbBranch SetUpAustralianBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var cusCode = company.OrgProxy.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_CustomsRegNo = "14 001 592 650";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "TMP";
			branch.GB_RL_NKHomePort = "AUSYD";

			return branch;
		}

		GlbBranch SetUpOtherAustralianCompanyBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var cusCode = company.OrgProxy.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_CustomsRegNo = "14 001 592 651";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "TMM";
			branch.GB_RL_NKHomePort = "AUSYD";

			return branch;
		}

		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		ForwardingConsol SeaConsol
		{
			get
			{
				if (seaConsol == null)
				{
					seaConsol = Factory.NewWithValidTestData<ForwardingConsol>();
					seaConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					seaConsol.JK_MasterBillNum = "32236346553";
					seaConsol.JK_RL_NKLoadPort = "UAIEV";
					seaConsol.JK_RL_NKDischargePort = "AUSYD";
					seaConsol.JK_PrepaidCollect = "PPD";
					var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
					shippingLine.OH_RL_NKClosestPort = "AUSYD";
					var abn = shippingLine.CustomsCodes.AddNew("ABN", "23112936991");
					var shippingLineAddress = shippingLine.Addresses.AddNew();
					shippingLineAddress.OA_Address1 = "address1";
					seaConsol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;
					var transport = seaConsol.Transports.AddNew();
					transport.JW_VoyageFlight = "VV2346";
					transport.JW_RL_NKLoadPort = "UAIEV";
					transport.JW_RL_NKDiscPort = "AUSYD";
					transport.JW_ETA = ZDateTime.Today;
					transport.JW_ETD = ZDateTime.Today.AddDays(1);
					transport.JW_Vessel = "ADMIRALENGRACHT";
					var container = seaConsol.Containers.AddNew();
					container.JC_ContainerNum = "CONTAINER";
					container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_UniqueConsignRef = "S000001";
					shipment.JS_INCO = Core.Constants.IncoTerms.CostInsuranceAndFreight;
					shipment.JS_PackingMode = "LCL";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_FullName = "consignor";
					consignor.OH_Code = "CONSNOR";
					consignor.OH_RL_NKClosestPort = "UAIEV";
					shipment.ConsignorPK = consignor.PK;
					var consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_FullName = "consignee";
					consignee.OH_Code = "CONSNEE";
					consignee.OH_RL_NKClosestPort = "AUSYD";
					shipment.ConsigneePK = consignee.PK;
					shipment.JS_GoodsDescription = "Downsized Developers";
					shipment.JS_HouseBill = "363463634";
					shipment.JS_RL_NKOrigin = "UAIEV";
					shipment.JS_RL_NKDestination = "AUSYD";
					shipment.JS_ActualWeight = 1;
					shipment.JS_OuterPacks = 1;
					shipment.JS_GoodsValue = 1;
					shipment.JS_RX_NKGoodsValueCurr = "AUD";
					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
					seaConsol.Shipments.Add(shipment);

					var packline = shipment.OuterPackLines.AddNew();
					packline.JL_JC = container.PK;
				}

				return seaConsol;
			}
		}
		ForwardingConsol seaConsol;

		ForwardingConsol AirConsol
		{
			get
			{
				if (airConsol == null)
				{
					airConsol = Factory.NewWithValidTestData<ForwardingConsol>();
					airConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
					airConsol.JK_MasterBillNum = "32236346553";
					airConsol.JK_RL_NKLoadPort = "UAIEV";
					airConsol.JK_RL_NKDischargePort = "AUSYD";
					airConsol.JK_PrepaidCollect = "PPD";
					var transport = airConsol.Transports.AddNew();
					transport.JW_VoyageFlight = "VV2346";
					transport.JW_RL_NKLoadPort = "UAIEV";
					transport.JW_RL_NKDiscPort = "AUSYD";
					transport.JW_ETA = ZDateTime.Today;
					transport.JW_ETD = ZDateTime.Today.AddDays(1);

					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_UniqueConsignRef = "S000001";
					shipment.JS_INCO = Core.Constants.IncoTerms.CostInsuranceAndFreight;
					shipment.JS_PackingMode = "LCL";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_FullName = "consignor";
					consignor.OH_Code = "CONSNOR";
					consignor.MainAddress.OA_Address1 = "CONSIGNOR ADDRESS";
					consignor.MainAddress.OA_RN_NKCountryCode = "UA";
					shipment.ConsignorPK = consignor.PK;
					var consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_FullName = "consignee";
					consignee.OH_Code = "CONSNEE";
					consignee.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS";
					shipment.ConsigneePK = consignee.PK;
					shipment.JS_GoodsDescription = "Downsized Developers";
					shipment.JS_HouseBill = "363463634";
					shipment.JS_RL_NKOrigin = "UAIEV";
					shipment.JS_RL_NKDestination = "AUSYD";
					shipment.JS_ActualWeight = 1;
					shipment.JS_OuterPacks = 1;
					shipment.JS_GoodsValue = 1;
					shipment.JS_RX_NKGoodsValueCurr = "AUD";
					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
					airConsol.Shipments.Add(shipment);
				}
				return airConsol;
			}
		}
		ForwardingConsol airConsol;

		#endregion
	}
}
