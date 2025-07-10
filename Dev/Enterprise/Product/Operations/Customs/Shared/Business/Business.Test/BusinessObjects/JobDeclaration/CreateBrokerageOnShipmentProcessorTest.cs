using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CreateBrokerageOnShipmentProcessorTest : TestCaseWithFactory
	{
		public void TestGetCreateDeclarationHelperLoadsCountrySpecificCreator()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_GoodsDescription = "SOME STUFF";
			Factory.Save();

			var logsFactory = new BusinessObjectFactory();
			var logsShipment = logsFactory.Load<ForwardingShipment>(shipment.PK);
			var processor = new CreateBrokerageOnShipmentProcessor(logsShipment);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Eritrea))
			{
				AssertEquals("Enterprise.Customs.Business.CreateDeclarationHelper", processor.GetCreateDeclarationHelper().GetType().FullName);
			}
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("Enterprise.Customs.AU.Declaration.Business.CreateDeclarationHelper", processor.GetCreateDeclarationHelper().GetType().FullName);
			}
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.France))
			{
				AssertEquals("Enterprise.Customs.EU.Business.CreateDeclarationHelper", processor.GetCreateDeclarationHelper().GetType().FullName);
			}
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("Enterprise.Customs.US.Business.CreateDeclarationHelper", processor.GetCreateDeclarationHelper().GetType().FullName);
			}
		}

		public void TestCreateBrokerageOnShipmentRequiresShipment()
		{
			var exception = AssertExceptionThrown<System.ArgumentNullException>(() => new CreateBrokerageOnShipmentProcessor(null));
#if NET
			AssertEquals("Value cannot be null. (Parameter 'shipment')", exception.Message);
#else
			AssertEquals("Value cannot be null.\r\nParameter name: shipment", exception.Message);
#endif
			var shipment = Factory.New<ForwardingShipment>();
			AssertNoExceptionThrown(() => new CreateBrokerageOnShipmentProcessor(shipment));
		}

		public void TestCreateBrokerageOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_GoodsDescription = "SOME STUFF";
			Factory.Save();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var logsFactory = new BusinessObjectFactory();
				var logsShipment = logsFactory.Load<ForwardingShipment>(shipment.PK);
				AssertEquals("Precondition - no declarations", 0, logsShipment.Declarations.Length);

				var processor = new CreateBrokerageOnShipmentProcessor(logsShipment);
				var notifications = new NotificationBuffer();
				processor.Process(notifications, new CancellationToken());
				AssertEquals("A Declaration was created.", notifications.AsString.Trim());
				logsFactory.Save();

				var declaration = (BaseJobDeclaration)logsShipment.GetDeclaration();
				AssertEquals("Synchronized - JE_RL_NKOrigin", "AUSYD", declaration.JE_RL_NKOrigin);
				AssertEquals("Synchronized - JE_RL_NKFinalDestination", "NZAKL", declaration.JE_RL_NKFinalDestination);
				AssertEquals("Synchronized - JE_GoodsDescription", "SOME STUFF", declaration.JE_GoodsDescription);
				AssertEquals("Enterprise.Customs.NZ.Business.Declaration.JobDeclaration", declaration.GetType().FullName);
				AssertEquals("Just one Declaration was created by the processor", 1, logsShipment.Declarations.Length);

				notifications = new NotificationBuffer();
				processor.Process(notifications, new CancellationToken());
				AssertEquals("Shipment already has a Declaration.", notifications.AsString.Trim());
				logsFactory.Save();

				AssertEquals("A second Declaration is NOT created by the processor", 1, logsShipment.Declarations.Length);
				AssertEquals("The existing declaration has not been replaced", declaration.PK, logsShipment.GetDeclaration().PK);
			}
		}

		public void TestCreateBrokerageOnShipmentWithMutex()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var processor = new CreateBrokerageOnShipmentProcessor(shipment);
			var notifications = new NotificationBuffer();

			using (var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK))
			{
				mutex.Lock();
				processor.Process(notifications, new CancellationToken());
				AssertEquals("Another Declaration Create is in progress.", notifications.AsString.Trim());
				mutex.Unlock();
			}

			notifications.Clear();
			processor.Process(notifications, new CancellationToken());
			AssertEquals("A Declaration was created.", notifications.AsString.Trim());

			using (var anotherMutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK))
			{
				Assert("Mutex in DeclarationBeingCreatedForShipmentMutexCreator should not be unlocked after process", !anotherMutex.Lock());

				Factory.Save();
				Assert("Mutex in DeclarationBeingCreatedForShipmentMutexCreator should be unlocked after factory saved", anotherMutex.Lock());
				anotherMutex.Unlock();
			}
		}
	}
}
