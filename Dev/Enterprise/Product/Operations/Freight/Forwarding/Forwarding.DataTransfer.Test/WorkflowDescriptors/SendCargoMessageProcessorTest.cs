using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class SendCargoMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "AUC";
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var cusCode = company.OrgProxy.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_CustomsRegNo = "14 001 592 650";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AUB";
			branch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();

			var hAWBfilter = new ZQuery(CusHAWBSchema.CS_JS, AirConsol.Shipments[0].PK);
			var mAWBfilter = new ZQuery(CusMAWBSchema.CM_JK, AirConsol.PK);
			AssertEquals(0, Factory.Load<Enterprise.Integration.Customs.Shared.ICusHAWB>(hAWBfilter).Length);
			AssertEquals(0, Factory.Load<Enterprise.Integration.Customs.Shared.ICusMAWB>(mAWBfilter).Length);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			using (Env.Registry.RawRegistry.AUCCompanyCertificateData.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 }))
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.DataType.SuspendValidation())
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.SetTemporaryValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "pwd"))
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var processor = new SendCargoMessageProcessor(AirConsol);
				processor.Process(new NotificationBuffer(), new CancellationToken());
			}

			var factory2 = new BusinessObjectFactory();
			var hAWBs = (BusinessObject[])factory2.Load<Enterprise.Integration.Customs.Shared.ICusHAWB>(hAWBfilter);
			var mAWBs = (BusinessObject[])factory2.Load<Enterprise.Integration.Customs.Shared.ICusMAWB>(mAWBfilter);
			AssertEquals(0, hAWBs.Length);
			AssertEquals(0, mAWBs.Length);
			AssertEquals(0, factory2.GetDatabaseCount(typeof(EDIMessage)));

			Factory.Save();

			hAWBs = (BusinessObject[])factory2.Load<Enterprise.Integration.Customs.Shared.ICusHAWB>(hAWBfilter);
			mAWBs = (BusinessObject[])factory2.Load<Enterprise.Integration.Customs.Shared.ICusMAWB>(mAWBfilter);
			AssertEquals(1, hAWBs.Length);
			AssertEquals(1, mAWBs.Length);
			AssertEquals(1, factory2.GetDatabaseCount(typeof(EDIMessage)));
		}

		#region Implementation

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
