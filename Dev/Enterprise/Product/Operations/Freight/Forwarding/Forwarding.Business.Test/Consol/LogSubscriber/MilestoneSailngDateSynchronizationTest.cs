using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(JobVoyageEventLogSynchronizer))]
	sealed class MilestoneSailngDateSynchronizationTest : LogSubscriberTest<JobVoyageEventLogSynchronizer>
	{
		public void TestSynchronize()
		{
				SetupMilestones();
				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				JobSailing loadedSailing = newFactory.Load<JobSailing>(Consol.Transports.MostInterestingTransport.Sailing.PK);
				loadedSailing.Destination.JB_A_ARV = ZDateTime.Now.AddDays(-3);
				loadedSailing.Destination.JB_AvailabilityDate = ZDateTime.Now.AddDays(-2);

				newFactory.Save();

				RunLogWalkerCycleForTest();

				ProcessTask loadedDeclarationMilestone = (new BusinessObjectFactory()).Load<ProcessTask>(DeclarationMilestone.PK);
				AssertEquals(loadedSailing.Destination.JB_AvailabilityDate.Date, new ZDateTimeOffset(loadedDeclarationMilestone.P9_ActualDateInfo.PersistentValue).Date);
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get { return true; }
		}

		#region Implementation

		void SetupMilestones()
		{
			ShipmentMilestone.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;

			DeclarationMilestone.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;
			var declarationTransport = Factory.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, Declaration.PK))[0];
			DeclarationMilestone.P9_ReferencedID = declarationTransport.PK;
			DeclarationMilestone.P9_ReferencedTableCode = declarationTransport.TablePrefix;
		}

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
					consol.JK_RL_NKLoadPort = "AUSYD";
					consol.JK_RL_NKDischargePort = "USLAX";
					consol.Transports.MostInterestingTransport.JW_Vessel = Vessel.RV_FK;
					consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Voyage";
					consol.Transports.MostInterestingTransport.CarrierPK = ShippingLine.PK;
					consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "SGSIN";
					consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

					var transport2 = consol.Transports.AddNew();
					transport2.JW_RL_NKLoadPort = "SGSIN";
					transport2.JW_RL_NKDiscPort = "USLAX";
					transport2.JW_Vessel = "Vessel2";
					transport2.JW_VoyageFlight = "222";
					transport2.JW_IsLinked = true;
				}
				return consol;
			}
		}
		ForwardingConsol consol;

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Consol.Shipments.AddNew();
					shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
					shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
					shipment.JS_TransportMode = consol.JK_TransportMode;
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.JE_TransportMode] = Core.Constants.TransportModes.Sea;
					declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "AUSYD";
					declaration[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "SGSIN";
					declaration[JobDeclarationSchema.JE_VesselName] = Vessel.RV_Name;
					declaration[JobDeclarationSchema.JE_LloydsIMO] = vessel.RV_LloydsNumber;
					declaration[JobDeclarationSchema.JE_VoyageFlightNo] = "Voyage";
					declaration[JobDeclarationSchema.JE_OH_ShippingLine] = ShippingLine.PK;
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		ProcessTask ShipmentMilestone
		{
			get
			{
				if (shipmentMilestone == null)
				{
					shipmentMilestone = ((IWorkflowProvider)Shipment).WorkflowItems.Milestones.AddNew();
				}
				return shipmentMilestone;
			}
		}
		ProcessTask shipmentMilestone;

		ProcessTask DeclarationMilestone
		{
			get
			{
				if (declarationMilestone == null)
				{
					declarationMilestone = ((IWorkflowProvider)Declaration).WorkflowItems.Milestones.AddNew();
				}
				return declarationMilestone;
			}
		}
		ProcessTask declarationMilestone;

		RefVessel Vessel
		{
			get { return vessel ?? (vessel = Factory.LoadTop1<RefVessel>(new ZQuery())); }
		}
		RefVessel vessel;

		OrgHeader ShippingLine
		{
			get { return shippingLine ?? (shippingLine = new VoyageTestHelper(Factory).CreateCarrier("MAERSK")); }
		}
		OrgHeader shippingLine;

		#endregion
	}
}
