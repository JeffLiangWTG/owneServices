using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	sealed class BrokeragePlugInOneToOneTest : TestCaseWithFactory
	{
		public void TestLicenceCheckPoint()
		{
			AssertLicenceCheckPoint(ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name), Env.Licence.Broker);
		}

		public void TestGetDeclarationFromShipment()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;

			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				AssertEquals("TestDec is retrieved", testDec.PK, plugIn.JobDeclaration.PK);
			}
		}

		public void TestOnJobDeclarationInitialised()
		{
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				AssertEquals("JobDec not initialised", false, plugIn.IsInitialised);
			}

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;

			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				AssertEquals("OnJobdeclaration called", true, plugIn.IsInitialised);
				AssertNotNull("JobDeclaration set", plugIn.JobDeclaration);
			}
		}

		[ExpectNoExceptions]
		public void TestOnGUIShownDoesntThrowException()
		{
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn.OnGUIShown();
			}
		}

		public void TestMutexUnlockedWhenDisposed()
		{
			ZGlobalMutex mutex = null;
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn.OnGUIShown();
				mutex = plugIn.Mutex;
				AssertEquals("Locked by this instance", true, mutex.HasLock);
			}
			AssertEquals("Unlocked as this is disposed", false, mutex.IsLocked);
		}

		public void TestMutexLock()
		{
			Factory.Save();
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn.OnGUIShown();
				AssertNotNull("Declaration has been created", plugIn.JobDeclaration);

				BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
				ForwardingShipment anotherInstanceOfShipment = anotherFactory.Load<ForwardingShipment>(shipment.PK);
				using (BrokeragePlugInOneToOneForTest anotherPlugIn = new BrokeragePlugInOneToOneForTest(anotherInstanceOfShipment))
				{
					anotherPlugIn.OnGUIShown();
					AssertNull("New declaration is not created", anotherPlugIn.JobDeclaration);
				}
			}
		}

		public void TestMutexUnlockedWhenSaved()
		{
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn.OnGUIShown();
				AssertEquals("mutex locked", true, plugIn.Mutex.HasLock);
				Factory.Save();
				AssertEquals("unlocked", false, plugIn.Mutex.HasLock);
			}
		}

		public void TestInitialSynchroniseBetweenJobDecAndShipment()
		{
			shipment.JS_HouseBill = "11111";
			shipment.JS_TransportMode = "AIR";
			shipment.ConsigneePK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			shipment.JS_RL_NKDestination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 100;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "XXX";
			shipment.JS_GoodsDescription = "TestDescription";

			ForwardingConsol consol = shipment.Consols.AddNew();

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF23";
			consol.SetDefaultShippingLineAddress(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			consol.JK_RL_NKLoadPort = "AUSYD";

			consol.JK_RL_NKPortOfFirstArrival = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			consol.JK_OA_ArrivalCTOAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			consol.JK_OA_UnpackDepotAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			consol.JK_MasterBillNum = "08188888888";

			BaseJobDeclaration jobDeclaration;
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn.OnGUIShown();
				plugIn.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				jobDeclaration = plugIn.JobDeclaration;
			}

			AssertEquals(jobDeclaration.JE_HouseBill, shipment.JS_HouseBill);
			AssertEquals(jobDeclaration.JE_RL_NKFinalDestination, shipment.JS_RL_NKDestination);
			AssertEquals(jobDeclaration.JE_RL_NKOrigin, shipment.JS_RL_NKOrigin);
			AssertEquals(jobDeclaration.JE_TransportMode, shipment.JS_TransportMode);
			AssertEquals(jobDeclaration.JE_OH_Supplier, shipment.ConsignorPK);
			AssertEquals(jobDeclaration.JE_OH_Importer, shipment.ConsigneePK);
			AssertEquals(jobDeclaration.JE_TotalWeight, shipment.JS_ActualWeight);
			AssertEquals(jobDeclaration.JE_TotalWeightUnit, shipment.JS_UnitOfWeight);
			AssertEquals(jobDeclaration.JE_TotalVolume, shipment.JS_ActualVolume);
			AssertEquals(jobDeclaration.JE_TotalVolumeUnit, shipment.JS_UnitOfVolume);
			AssertEquals(jobDeclaration.JE_TotalNoOfPacks, shipment.JS_OuterPacks);
			AssertEquals(jobDeclaration.JE_TotalNoOfPacksPackType, shipment.JS_F3_NKPackType);
			AssertEquals(jobDeclaration.JE_GoodsDescription, shipment.JS_GoodsDescription);

			AssertEquals(jobDeclaration.JE_VoyageFlightNo, consol.JK_JX_JV_VoyageFlight);
			AssertEquals(jobDeclaration.JE_OH_ShippingLine, consol.ShippingLinePK);
			AssertEquals(jobDeclaration.JE_RL_NKPortOfArrival, consol.JK_JX_JB_RL_NKPortOfDischarge);
			AssertEquals(consol.JK_JX_JB_RL_NKPortOfDischarge, consol.JK_RL_NKPortOfFirstArrival);
			AssertEquals("Should be empty, because not used in base Customs", ZString.Empty, jobDeclaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals(jobDeclaration.JE_RL_NKPortOfLoading, consol.JK_JX_JA_RL_NKPortOfLoading);
			AssertEquals(jobDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_Address, consol.JK_OA_ArrivalCTOAddress);
			AssertEquals(jobDeclaration.DepotDocAddress.E2_OA_Address, consol.JK_OA_UnpackDepotAddress);
			AssertEquals(jobDeclaration.JE_MasterBill, consol.JK_MasterBillNum);
		}

		public void TestOngoingSynchroniseBetweenJobDecAndShipment()
		{
			ZQuery localPortFilter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			RefUNLOCO localPort = Factory.LoadTop1<RefUNLOCO>(localPortFilter);
			localPortFilter.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, localPort.Code);
			RefUNLOCO localPort2 = Factory.LoadTop1<RefUNLOCO>(localPortFilter);
			RefUNLOCO overseasPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, localPort.RL_RN_NKCountryCode));

			shipment.JS_HouseBill = "11111";
			shipment.JS_RL_NKDestination = overseasPort.Code;
			shipment.JS_RL_NKOrigin = localPort.Code;

			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = overseasPort.Code;
			consol.JK_RL_NKLoadPort = localPort.Code;
			consol.JK_RL_NKPortOfFirstArrival = overseasPort.Code;

			BaseJobDeclaration jobDeclaration;
			using (BrokeragePlugInOneToOneForTest plugIn = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn.OnGUIShown();
				jobDeclaration = plugIn.JobDeclaration;
			}

			AssertEquals(jobDeclaration.JE_HouseBill, shipment.JS_HouseBill);
			AssertEquals(jobDeclaration.JE_RL_NKFinalDestination, shipment.JS_RL_NKDestination);
			AssertEquals(jobDeclaration.JE_RL_NKOrigin, shipment.JS_RL_NKOrigin);
			AssertEquals(jobDeclaration.JE_RL_NKPortOfArrival, consol.JK_RL_NKDischargePort);
			AssertEquals(consol.JK_RL_NKDischargePort, consol.JK_RL_NKPortOfFirstArrival);
			AssertEquals("Should be empty, because not used in base Customs", ZString.Empty, jobDeclaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals(jobDeclaration.JE_RL_NKPortOfLoading, consol.JK_RL_NKLoadPort);

			shipment.JS_HouseBill = "22222";
			shipment.JS_RL_NKOrigin = localPort2.Code;
			consol.JK_RL_NKLoadPort = localPort2.Code;
			AssertEquals(jobDeclaration.JE_HouseBill, shipment.JS_HouseBill);
			AssertEquals(jobDeclaration.JE_RL_NKFinalDestination, shipment.JS_RL_NKDestination);
			AssertEquals(jobDeclaration.JE_RL_NKOrigin, shipment.JS_RL_NKOrigin);
			AssertEquals(jobDeclaration.JE_RL_NKPortOfArrival, consol.JK_RL_NKDischargePort);
			AssertEquals(consol.JK_RL_NKDischargePort, consol.JK_RL_NKPortOfFirstArrival);
			AssertEquals("Should be empty, because not used in base Customs", ZString.Empty, jobDeclaration.JE_RL_NKPortOfFirstArrival);
			AssertEquals(jobDeclaration.JE_RL_NKPortOfLoading, consol.JK_RL_NKLoadPort);
		}

		public void TestGetDeclarationFromShipmentWhenMutexWasLockedBefore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Factory.Save();

			using (BrokeragePlugInOneToOneForTest plugIn1 = new BrokeragePlugInOneToOneForTest(shipment))
			{
				plugIn1.OnGUIShown();
				AssertEquals("PreCondition: Mutex is locked by this instance", true, plugIn1.Mutex.HasLock);

				BaseJobDeclaration decCreated = plugIn1.JobDeclaration;

				BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
				ForwardingShipment shipmentLoadedInDiffFac = anotherFactory.Load<ForwardingShipment>(shipment.PK);
				using (BrokeragePlugInOneToOneForTest plugIn2 = new BrokeragePlugInOneToOneForTest(shipmentLoadedInDiffFac))
				{
					plugIn2.OnGUIShown();
					AssertNull("PreCondition:JobDec is not created for this plugin", plugIn2.TriggerGetDeclarationFromShipment());

					// Done this way to avoid query caches getting cleared to emulate a second instance of the 
					// enterprise application.
					((IBusinessObjectFactoryInternals)Factory).DisableQueryCacheReset = true;
					Factory.Save();

					AssertEquals("Should have picked the dec created for plugin1", decCreated.PK, plugIn2.TriggerGetDeclarationFromShipment().PK);
				}
			}
		}

		ForwardingShipment shipment;

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
		}

		void AssertLicenceCheckPoint(IMainFormModule mainFormModule, LicenceCheckpoint expected)
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (var module = mainFormModule.CreateZModule())
			using (var form = ((IFilterModuleInternalsForTesting)module).ShowNewForm())
			using (var plugin = new BrokeragePlugInOneToOneForTest(shipment))
			{
				((IPlugInInternals)plugin).InitializePlugin(null, (ZForm)form);
				AssertEquals("LicenceCheckPoint: " + mainFormModule.ID, expected, ((IPlugInInternals)plugin).LicenceCheckPoint);
			}
		}

		sealed class BrokeragePlugInOneToOneForTest : BrokeragePlugInOneToOne
		{
			public bool IsInitialised;
			public BrokeragePlugInOneToOneForTest(ForwardingShipment shipment) : base(shipment) { }

			protected override void OnJobDeclarationSet()
			{
				base.OnJobDeclarationSet();
				IsInitialised = true;
			}

			MenuItem fTopLevelMenu;
			protected override MenuItem GetNewTopLevelMenuCore()
			{
				if (fTopLevelMenu == null)
				{
					fTopLevelMenu = new EDIMenu();
				}
				return fTopLevelMenu;
			}

			public BaseJobDeclaration TriggerGetDeclarationFromShipment() => GetDeclarationFromShipment();
		}
	}
}
