using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[UseSnapshotProtection]
	sealed class ShipmentSendingarnumerGeneratorTest : TestCase
	{
		public void TestGenerateStandardSendingarnumers()
		{
			TestCarrierNumberNumberFountain(airConsol, SendingarnumerNumberFountains.Instance.SendingarnumerExportAir, airPrefix);
			SwapUNLOCOs(airConsol);
			TestCarrierNumberNumberFountain(airConsol, SendingarnumerNumberFountains.Instance.SendingarnumerImportAir, airPrefix);

			TestCarrierNumberNumberFountain(seaConsol, SendingarnumerNumberFountains.Instance.SendingarnumerExportSea, seaPrefix);
			SwapUNLOCOs(seaConsol);
			TestCarrierNumberNumberFountain(seaConsol, SendingarnumerNumberFountains.Instance.SendingarnumerImportSea, seaPrefix);
		}

		public void TestGenerateAirExpressSendingarnumers()
		{
			airConsol.JK_AWBServiceLevel = FreightDataRegistry.Instance.AirExpressShipmentServiceLevelCode.Value;
			ZString airCRN = airConsol.JK_CRN + "-" + Sendingarnumer.AirExpressPrefix + "0";
			ShipmentSendingarnumerGenerator generator = new ShipmentSendingarnumerGenerator(airConsol);
			generator.GenerateSendingarnumers();
			AssertEquals(string.Format("should contain {0}", airCRN), ZBool.True, HasShipmentWithGGGG(airConsol, airCRN));

			ForwardingConsol airConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			airConsol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			airConsol2.JK_AWBServiceLevel = FreightDataRegistry.Instance.AirExpressShipmentServiceLevelCode.Value;
			airConsol2.JK_CRN = airConsol.JK_CRN;
			airConsol2.JK_RL_NKLoadPort = airConsol.JK_RL_NKLoadPort;
			airConsol2.JK_RL_NKDischargePort = airConsol.JK_RL_NKDischargePort;
			airConsol2.Shipments.AddNew();
			airConsol2.Shipments.AddNew();
			airCRN = airConsol2.JK_CRN + "-" + Sendingarnumer.AirExpressPrefix + "1";
			generator = new ShipmentSendingarnumerGenerator(airConsol2);
			generator.GenerateSendingarnumers();
			AssertEquals(string.Format("should contain {0}", airCRN), ZBool.True, HasShipmentWithGGGG(airConsol2, airCRN));
		}

		#region Implementation

		ForwardingConsol airConsol;
		ForwardingConsol seaConsol;
		ZString airPrefix;
		ZString seaPrefix;
		const string airCRN = "F-789-0808-8-IS-REY";
		const string seaCRN = "H-A98-0908-9-IS-KEF";
		BusinessObjectFactory Factory { get; } = new BusinessObjectFactory();

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			airPrefix = FreightDataRegistry.Instance.ForwarderSendingarnumerCodeForAirfreight.Value;
			seaPrefix = FreightDataRegistry.Instance.ForwarderSendingarnumerCodeForSeafreight.Value;

			airConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			airConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			airConsol.JK_CRN = airCRN;
			airConsol.JK_RL_NKLoadPort = "ISREY";
			airConsol.JK_RL_NKDischargePort = "AUSYD";
			airConsol.Shipments.AddNew();
			airConsol.Shipments.AddNew();
			airConsol.Shipments.AddNew();

			seaConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			seaConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			seaConsol.JK_CRN = seaCRN;
			seaConsol.JK_RL_NKLoadPort = "ISKEF";
			seaConsol.JK_RL_NKDischargePort = "NZAKL";
			seaConsol.Shipments.AddNew();
			seaConsol.Shipments.AddNew();
			seaConsol.Shipments.AddNew();
		}

		void SwapUNLOCOs(ForwardingConsol consol)
		{
			ZString unloco = consol.JK_RL_NKLoadPort;
			consol.JK_RL_NKLoadPort = consol.JK_RL_NKDischargePort;
			consol.JK_RL_NKDischargePort = unloco;
		}

		ZBool HasShipmentWithGGGG(ForwardingConsol consol, ZString gggg)
		{
			ZBool result = ZBool.False;

			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.CustomsEntryNumber.Contains(gggg))
				{
					result = ZBool.True;
					break;
				}
			}

			return result;
		}

		void TestCarrierNumberNumberFountain(ForwardingConsol consol, INumberFountainProxy numFountain, ZString prefix)
		{
			numFountain.Reset();
			ShipmentSendingarnumerGenerator generator = new ShipmentSendingarnumerGenerator(consol);
			generator.GenerateSendingarnumers();
			AssertEquals(string.Format("should contain {0}001", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "001"));
			AssertEquals(string.Format("should contain {0}002", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "002"));
			AssertEquals(string.Format("should contain {0}003", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "003"));

			generator.GenerateSendingarnumers();
			AssertEquals(string.Format("should contain {0}004", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "004"));
			AssertEquals(string.Format("should contain {0}005", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "005"));
			AssertEquals(string.Format("should contain {0}006", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "006"));

			// this is to bump the number up close enough to test number 'rolling'
			SendingarnumerNumberFountainTestHelper.SetNextForNumberFountain(numFountain, 898);
			for (var i = 0; i < 100; i++)
			{
				Db.Connection.BeginTransaction();       // Testing the number fountain
				try
				{
					numFountain.GetNextFormatted(Db.Connection);
					Db.Connection.CommitTransaction();      // Testing the number fountain
				}
				catch
				{
					Db.Connection.RollbackTransaction();        // Testing the number fountain
					throw;
				}
			}

			generator.GenerateSendingarnumers();

			AssertEquals(string.Format("should contain {0}001", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "001"));
			AssertEquals(string.Format("should contain {0}002", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "002"));
			AssertEquals(string.Format("should contain {0}003", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "003"));

			SendingarnumerNumberFountainTestHelper.SetNextForNumberFountain(numFountain, 845);
			generator.GenerateSendingarnumers();
			AssertEquals(string.Format("should contain {0}845", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "845"));
			AssertEquals(string.Format("should contain {0}846", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "846"));
			AssertEquals(string.Format("should contain {0}847", prefix), ZBool.True, HasShipmentWithGGGG(consol, prefix + "847"));
		}

		#endregion
	}
}
