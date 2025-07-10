using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CAMProcessorTest : TestCaseWithFactory
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
						new CAMProcessor(consol).Process(notifications, new CancellationToken());
					}
				});

				AssertEquals("US AMS was created but not be saved.", false, ((BusinessObject)consol.USAMS).IsInDatabase);
			}
		}

		public void TestEndToEndForAir()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "08111111111";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var cancellationToken = new CancellationToken();
			new CAMProcessor(consol).Process(notifications, cancellationToken);
			AssertEquals("Consol is not valid of US AMS", notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();
			new CAMProcessor(consol).Process(notifications, cancellationToken);
			AssertEquals("This Consol does not require a manifest.", notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertEquals(0, consol.GetGlobalManifestHeaders().Length);
			new CAMProcessor(consol).Process(notifications, cancellationToken);
			AssertEquals("", notifications.AsString.Trim());
			AssertEquals(1, new LogsForNominatedEvent(consol.Logs, Events.AddedARecordToTheSystem).Count);
			var manifests = consol.GetGlobalManifestHeaders();
			AssertEquals(1, manifests.Length);
			AssertEquals("US AMS Import Air Manifest was created by processor", "IAM", manifests[0].AMA_ManifestType);
		}

		public void TestEndToEndForSea()
		{
			AssertEndToEnd(Constants.TransportModes.Sea);
		}

		public void TestEndToEndForRail()
		{
			AssertEndToEnd(Constants.TransportModes.Rail);
		}

		void AssertEndToEnd(string transportMode)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "08111111111";
			Factory.Save();

			var notifications = new NotificationBuffer();
			var cancellationToken = new CancellationToken();
			new CAMProcessor(consol).Process(notifications, cancellationToken);
			AssertEquals("Consol is not valid of US AMS", notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_TransportMode = transportMode;
			Factory.Save();
			new CAMProcessor(consol).Process(notifications, cancellationToken);
			AssertEquals("The automatic creation of US AMS is only applicable to Consols transported via US/PR.", notifications.AsString.Trim());

			notifications.Clear();
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "USLAX";
			Factory.Save();
			AssertNull("Precondition", consol.USAMS);
			new CAMProcessor(consol).Process(notifications, cancellationToken);
			AssertEquals("", notifications.AsString.Trim());
			AssertEquals(1, new LogsForNominatedEvent(consol.Logs, Events.AddedARecordToTheSystem).Count);
			AssertNotNull("US AMS was created by processor", consol.USAMS);
		}

		ForwardingConsol GetValidConsol(BusinessObjectFactory factory)
		{
			var forwarder = factory.NewWithValidTestData<OrgHeader>();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			factory.Save();

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "202104070120";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB001";

			return consol;
		}
	}
}
