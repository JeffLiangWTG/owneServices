using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CCRProcessorTest : TestCaseWithFactory
	{
		public void TestFactorySaveShouldNotBeCalled()
		{
			var factory = NewFactory();

			var consol = GetValidConsol(factory);
			AssertEquals("Precondition.", false, consol.IsInDatabase);

			using (new FactorySaveAlerter(() => "FactorySaveShouldNotBeCalledByLogSubscribers", "Factory.Save() should not be called."))
			{
				AssertNoExceptionThrown("Factory.Save() should not be called.", () =>
				{
					using (factory.AddDisposableService())
					{
						var notifications = new NotificationBuffer();
						new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, new CancellationToken());
					}
				});

				AssertEquals("AU CusSCAOceanBill was created but not be saved.", false, ((BusinessObject)consol.AUCMRCusSCAOceanBill).IsInDatabase);
			}
		}

		public void TestEndToEndForAUSea()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "08111111111";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var cancellationToken = new CancellationToken();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals(string.Format("Automated creation of AU Customs Cargo Record is not available for this transport mode {0}.", consol.HumanReadableName), notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("Automated creation of AU Customs Cargo Record is available for AU import consolidations only.", notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Factory.Save();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("Receiving Agent is required for automated creation of AU Customs Cargo Record.", notifications.AsString.Trim());

			notifications.Clear();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			Factory.Save();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("Receiving Agent entered does not match any Organization of active AU companies or its branches.", notifications.AsString.Trim());

			notifications.Clear();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;
			company.Branches.AddNew();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			AssertNull("Precondition", consol.AUCusMAWB);
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("", notifications.AsString.Trim());
			AssertNotNull("AU OceanBill was created by processor", consol.AUCMRCusSCAOceanBill);
			AssertEquals(1, new LogsForNominatedEvent(consol.Logs, Events.CargoReportRecordsCreated).Count);
		}

		public void TestEndToEndForAUAir()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "08111111111";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB002";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var cancellationToken = new CancellationToken();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals(string.Format("Automated creation of AU Customs Cargo Record is not available for this transport mode {0}.", consol.HumanReadableName), notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("Automated creation of AU Customs Cargo Record is available for AU import consolidations only.", notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Factory.Save();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("Receiving Agent is required for automated creation of AU Customs Cargo Record.", notifications.AsString.Trim());

			notifications.Clear();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			Factory.Save();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("Receiving Agent entered does not match any Organization of active AU companies or its branches.", notifications.AsString.Trim());

			notifications.Clear();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;
			company.Branches.AddNew();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			AssertNull("Precondition", consol.AUCusMAWB);
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, cancellationToken);
			AssertEquals("", notifications.AsString.Trim());
			AssertNotNull("AU MAWB was created by processor", consol.AUCusMAWB);
			AssertEquals(1, new LogsForNominatedEvent(consol.Logs, Events.CargoReportRecordsCreated).Count);
		}

		public void TestCreatedOceanBillIsInDatabase()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			company.GC_OH_OrgProxy = forwarder.PK;
			company.Branches.AddNew();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "08111111111";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB001";
			Factory.Save();

			var notifications = new NotificationBuffer();
			new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, new CancellationToken());
			AssertEquals("", notifications.AsString.Trim());
			AssertNotNull("AU OceanBill was created by processor", consol.AUCMRCusSCAOceanBill);
			AssertEquals(1, new LogsForNominatedEvent(consol.Logs, Events.CargoReportRecordsCreated).Count);

			var query = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK);
			var newFactory = new BusinessObjectFactory();
			var bills = newFactory.Load(typeof(BaseCusSCAOceanBill), query);

			AssertEquals("Created ocean bill can be retrieved from another factory", consol.AUCMRCusSCAOceanBill.PK, bills.FirstOrDefault()?.PK);
			AssertEquals("There is only one ocean bill", 1, bills.Length);

			using (var mutex2 = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				Assert("Mutex is released (can be re-locked) after processing", mutex2.Lock());
			}
		}

		ForwardingConsol GetValidConsol(BusinessObjectFactory factory)
		{
			var forwarder = factory.NewWithValidTestData<OrgHeader>();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			factory.Save();

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "202104070120";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB001";

			return consol;
		}
	}
}
