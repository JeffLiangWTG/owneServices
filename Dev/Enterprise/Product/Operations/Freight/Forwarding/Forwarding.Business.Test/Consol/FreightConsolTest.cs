using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class FreightConsolTest : CommonConsolTest2
	{
		class SyncroniserForTesting : ICanSyncroniseFromConsol
		{
			public bool SyncroniseCalled { get; set; }

			public void Syncronise(CommonConsol consol)
			{
				SyncroniseCalled = true;
			}
		}

		public void TestSyncroniserIsTriggeredOnSaving()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var syncroniser = new SyncroniserForTesting();
			consol.Syncroniser = syncroniser;
			Assert("Pre-condition", !syncroniser.SyncroniseCalled);
			Factory.Save();
			Assert(syncroniser.SyncroniseCalled);

			void AssertSyncronized(Action change)
			{
				syncroniser.SyncroniseCalled = false;
				change.Invoke();
				Factory.Save();
				Assert(syncroniser.SyncroniseCalled);
			}
			AssertSyncronized(() => consol.JK_MasterBillNum = "08111111111");
			AssertSyncronized(() => consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgAddress>().PK);

			var interestingLeg = consol.Transports.MostInterestingTransport;
			if (interestingLeg != null)
			{
				AssertSyncronized(() => interestingLeg.JW_ATD = new ZDateTime(2022, 01, 01));
				AssertSyncronized(() => interestingLeg.JW_ATA = new ZDateTime(2022, 01, 01));
				AssertSyncronized(() => interestingLeg.JW_VoyageFlight = "DM1");
				AssertSyncronized(() => interestingLeg.JW_Vessel = "MSC");
				AssertSyncronized(() => interestingLeg.JW_RL_NKLoadPort = "USLAX");
				AssertSyncronized(() => interestingLeg.JW_RL_NKDiscPort = "CNSHA");
			}
		}

		public void TestOverallComplianceRisk()
		{
			Test("Override Clear", "OVR");
			Test("Potential Risk", "PSK");
			Test("Clear", "CLR");

			void Test(ZString expectedOverallComplianceRisk, string overallRiskForSetup)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var consol = Factory.New<ForwardingConsol>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = consol.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = consol.PK;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = overallRiskForSetup;
				Factory.Save();

				AssertEquals(expectedOverallComplianceRisk, consol.OverallComplianceRisk);
			}
		}

		#region TestAirCargoSynchroniser

		public void TestAirCargoSynchroniser()
		{
			AirCargoBridgeTestHelper bridge = new AirCargoBridgeTestHelper();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.SetAirCargoSynchroniserRetriever(delegate
			{ return bridge; });

			Transport transport = consol.Transports[0];

			string mAWBNumber = "08111111111";
			string flightNumber = "QF123";
			ZDateTime arrivalDate = new ZDateTime(2005, 12, 24);
			string loadPort = "USLAX";
			string dischargePort = "AUSYD";

			consol.JK_MasterBillNum = mAWBNumber;
			AssertEquals(mAWBNumber, bridge.MAWBNumberData);

			transport.JW_VoyageFlight = flightNumber;
			AssertEquals(flightNumber, bridge.FlightNumberData);

			transport.JW_ETA = arrivalDate;
			AssertEquals(arrivalDate, bridge.ArrivalDateData);

			transport.JW_RL_NKLoadPort = loadPort;
			AssertEquals(loadPort, bridge.LoadPortData);

			transport.JW_RL_NKDiscPort = dischargePort;
			AssertEquals(dischargePort, bridge.DischargePortData);
			AssertEquals(true, bridge.IsAir);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, bridge.IsAir);
		}

		public class AirCargoBridgeTestHelper : IUpdateFromConsol
		{
			#region IUpdateFromConsol Members

			public AirCargoBridgeTestHelper()
			{
				IsAir = true;
			}

			public ZString MAWBNumberData;
			public ZString MAWBNumber
			{
				set
				{
					MAWBNumberData = value;
				}
			}

			public ZString FlightNumberData;
			public ZString FlightNumber
			{
				set
				{
					FlightNumberData = value;
				}
			}

			public ZDateTime ArrivalDateData;
			public ZDateTime ArrivalDate
			{
				set
				{
					ArrivalDateData = value;
				}
			}

			public ZDateTime DepartureDateData;
			public ZDateTime DepartureDate
			{
				set
				{
					DepartureDateData = value;
				}
			}

			public ZString LoadPortData;
			public ZString LoadPort
			{
				set
				{
					LoadPortData = value;
				}
			}

			public ZString DischargePortData;
			public ZString DischargePort
			{
				set
				{
					DischargePortData = value;
				}
			}

			bool fIsAir;
			public bool IsAir
			{
				get { return fIsAir; }
				set { fIsAir = value; }
			}

			public void UpdateLoadPort(Transport transport)
			{
				LoadPort = transport.JW_RL_NKLoadPort;
			}

			public void UpdateDischargePort(Transport transport)
			{
				DischargePort = transport.JW_RL_NKDiscPort;
			}
			#endregion
		}

		#endregion

		#region Shipment Collection

		public void TestConsolTransportReturnReferenceToForwardingConsol()
		{
			var factory2 = new BusinessObjectFactory();
			var consol1 = Factory.New<ForwardingConsol>();

			var transport1 = consol1.Transports[0];
			transport1.JW_JX = ExportSailing1.PK;

			consol1.Containers.AddNew();
			var shipment = consol1.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].JL_PackageCount = 20;
			Factory.Save();
			shipment.JS_HouseBill = "House 1";
			var consol2 = factory2.New<ForwardingConsol>();
			var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
			consol2.Shipments.Add(shipment2);
			consol1.Shipments.Remove(shipment);
			shipment2.JS_HouseBill = "House 2";
			var transport = consol2.Transports[0];
			AssertNotNull("Consol", transport.Parent);
			AssertEquals("Consol values", transport.Parent, consol2);
		}

		#endregion

		#region Delivery Agents Event Tests

		public void TestDeliveryAgentPackGetDataSourceEvent()
		{
			ForwardingConsol consol = (ForwardingConsol)GetNewConsol();

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Delivery Agent Pack";

			var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>();

			Factory.SetValue(() => queryProvider.Object);

			//Cancel
			queryProvider
				.Setup(m => m.GetDeliveryAgentsToPrint(It.IsAny<DeliveryAgentToSelectFromForPrintingCollection>()))
				.Returns((DeliveryAgentOrgHeader[])null);//.Repeat.Once();

			DocumentSupporterDataState docState = consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Cancel Delivery Agents to print", false, docState.IsValid);

			queryProvider
				.Verify(m => m.GetDeliveryAgentsToPrint(It.IsAny<DeliveryAgentToSelectFromForPrintingCollection>()), Times.Once);

			//Accept
			ForwardingShipment shipment1 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			ForwardingShipment shipment2 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			ForwardingShipment shipment3 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			shipment1.JS_OH_DeliveryAgent = org1.PK;
			shipment2.JS_OH_DeliveryAgent = org2.PK;
			shipment3.JS_OH_DeliveryAgent = org2.PK;

			queryProvider.Setup(m => m.GetDeliveryAgentsToPrint(It.IsAny<DeliveryAgentToSelectFromForPrintingCollection>()))
				.Returns(new[] { Factory.Load<DeliveryAgentOrgHeader>(org1.PK) });//.Repeat.Once();

			docState = consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Accept Delivery Agents to print", true, docState.IsValid);

			queryProvider
				.Verify(m => m.GetDeliveryAgentsToPrint(It.IsAny<DeliveryAgentToSelectFromForPrintingCollection>()), Times.Exactly(2));
		}

		#endregion

		#region ICDArchive

		public void TestCDInvoiceNumbers()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "11111";
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;
			InvoiceCreationTestHelper invoiceCreatorHelper = new InvoiceCreationTestHelper(Factory);

			AccTransactionHeader transHeader1 = invoiceCreatorHelper.SetupTransaction(GlbBranch.CurrentBranch, "00001001", org, "11111", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			AccTransactionHeader transHeader2 = invoiceCreatorHelper.SetupTransaction(GlbBranch.CurrentBranch, "00001006", org, @"11111\A", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			AccTransactionHeader transHeader3 = invoiceCreatorHelper.SetupTransaction(GlbBranch.CurrentBranch, "00001010", org, @"11111\B", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			AccTransactionHeader transHeader4 = invoiceCreatorHelper.SetupTransaction(GlbBranch.CurrentBranch, "00001050", org, @"11111\C", ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Invoice);

			AssertEquals("Invoice numbers", @"11111", consol.CDArchiveInfo.InvoiceNumbers);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.UseJobNumberBasedInvoiceNumbers).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Invoice Numbers using normal invoice numbers", "00001001", consol.CDArchiveInfo.InvoiceNumbers);
			}
		}

		public void TestCDOrderNumbers()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment1.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "on1";
			shipment2.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "on2";
			AssertEquals("OrderNumbers", "on1, on2", consol.CDArchiveInfo.OrderNumbers);
		}

		#endregion

		public void TestAutoCompleteAirlinePrefix()
		{
			RefAirline airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport = consol.Transports.AddNew();
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "XX1234567";

			AssertEquals("MasterBillAirlinePrefix", "666", consol.MasterBillAirlinePrefix);
		}

		[ExpectNoExceptions()]
		public void TestINZManifestHeader()
		{
			var consol = Factory.New<ForwardingConsol>();
			Assert("ForwardingConsol should have implemented the interface INZManifestHeader", consol is INZManifestHeader);

			consol.JK_MasterBillNum = "BILL1234567";
			var iNZManifestHeader = (INZManifestHeader)consol;
			AssertEquals("INZManifestHeader.JobName", "Consol", iNZManifestHeader.JobName);
			AssertEquals("INZManifestHeader.DocumentParentType", "CON", iNZManifestHeader.DocumentParentType);
			AssertSame("INZManifestHeader.Logs", consol.Logs, iNZManifestHeader.Logs);
			AssertEquals("INZManifestHeader.MasterBillNumber", "BILL1234567", iNZManifestHeader.MasterBillNumber);
			var consolOCREntryNo = iNZManifestHeader.LoadAndCreateRegistrationNumber();
			AssertNotNull("LoadAndCreateRegistrationNumber", consolOCREntryNo);
			consolOCREntryNo.CE_EntryNum = "63289015";
		}

		#region Implementation

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		#endregion
	}
}
