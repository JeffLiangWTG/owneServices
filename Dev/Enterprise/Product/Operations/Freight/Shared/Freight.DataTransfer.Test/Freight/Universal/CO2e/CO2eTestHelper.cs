using System.Net.Http;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using TransportMode = Enterprise.UniversalDataBuss.DataObjects.Universal.TransportMode;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class CO2eTestHelper
	{
		public static CommonShipment CreateForwardingShipmentWithLegs(BusinessObjectFactory factory)
		{
			var shipment = factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "VNVNH";
			AddTransportLeg(shipment, factory, "AUSYD", "SGSIN", "SEA", "VY1", isLinked: true);
			AddTransportLeg(shipment, factory, "SGSIN", "VNVNH", "AIR", "FL1", "N95");

			return shipment;
		}

		public static CommonConsol CreateForwardingConsolWithLegs(BusinessObjectFactory factory, string loadPort = "AUSYD", string discPort = "VNVNH", string transitPort = "SGSIN", string transitTransportMode = "AIR")
		{
			var consol = factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as CommonConsol;
			consol.JK_TransportMode = "SEA";
			consol.JK_TotalShipmentActWeightCheck = 1m;
			consol.JK_TotalShipmentActOtherUnit = "T";
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = discPort;
			consol.Transports[0].JW_RL_NKDiscPort = transitPort;
			consol.Transports[0].JW_VoyageFlight = "VY1";
			AddTransportLeg(consol, factory, transitPort, discPort, transitTransportMode, "FL1", "N95", isLinked: true);

			return consol;
		}

		static void AddTransportLeg(ITransportParent transportParent, BusinessObjectFactory factory, string loadPort, string discPort, string transportMode, string voyageFlight, string aircraftType = "", bool isLinked = false)
		{
			var leg = transportParent.Transports.AddNew();
			leg.JW_RL_NKLoadPort = loadPort;
			leg.JW_RL_NKDiscPort = discPort;
			leg.JW_TransportMode = transportMode;
			leg.JW_VoyageFlight = voyageFlight;
			leg.JW_AircraftType = aircraftType;
			leg.JW_IsLinked = isLinked;
			if (isLinked)
			{
				leg.JW_JX = AddSailing(factory, transportMode, loadPort, discPort, voyageFlight, aircraftType)?.PK ?? ZGuid.Empty;
			}
		}

		static JobSailing AddSailing(BusinessObjectFactory factory, string mode, string loadPort, string discPort, string voyageFlightNum, string aircraftType)
		{
			var voyage = factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = voyageFlightNum;
			voyage.JV_AircraftType = aircraftType;
			voyage.JV_AirSeaRoad = mode;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discPort;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(5);

			voyage.GenerateSailings();

			return voyage.Sailings.GetSailingFromLoadAndDischarge(loadPort, discPort);
		}

		public static CommonShipment CreateForwardingShipmentRequiringTEU(BusinessObjectFactory factory)
		{
			var shipment = CreateForwardingShipmentWithLegs(factory);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer(factory, "20GP111", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer(factory, "40REHC111", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 0.5;
			packline1.JL_ActualWeightUQ = Weight.Tonnes;
			container2.AddPackLine(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 500;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;
			container1.AddPackLine(packline2);

			shipment.TransportsIncludingRelated[2].Delete();
			return shipment;
		}

		public static CommonConsol CreateForwardingConsolRequiringTEU(BusinessObjectFactory factory)
		{
			var consol = CreateForwardingConsolWithLegs(factory);

			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			((ICO2eProvider)consol).SetCO2eStatus(CO2eStatusList.Codes.Current);

			consol.Containers.RemoveAll();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var refContainer1 = NewRefContainer(factory, "20GP111", "22G0", 1m, 2280m);
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			var refContainer2 = NewRefContainer(factory, "40REHC111", "45R0", 2.3m, 4420m);
			container2.JC_RC = refContainer2.PK;

			return consol;
		}

		static RefContainer NewRefContainer(BusinessObjectFactory factory, ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		public static UniversalShipment GetSampleCO2eResponseDataObject(ForwardingShipment forwardingShipment = null)
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.DataProviderForCodeMapping = CO2eDataTransferHelper.CO2eCalculationProvider;
			dataObject.TotalWeight = 1m;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "T" };
			dataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "VNVNH" };
			dataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 10000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			var shipmentWeight = forwardingShipment?.JS_ActualWeight ?? 1m;
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>()
			{
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "AUSYD" },
					PortOfDischarge = new UNLOCO { Code = "SGSIN" },
					TransportMode = TransportMode.Sea,
					VoyageFlightNo = "VY1",
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = 8000 * shipmentWeight,
						CO2eUnit = new UnitOfWeight { Code = "KG" },
						CO2ePerTonne = 8,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "T" }
					},
					LegOrder = 0
				},
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "SIN" },
					PortOfDischarge = new UNLOCO { Code = "VII" },
					TransportMode = TransportMode.Air,
					VoyageFlightNo = "FL1",
					AircraftType = new CodeDescriptionPair { Code = "N95" },
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = 2000 * shipmentWeight,
						CO2eUnit = new UnitOfWeight { Code = "KG" },
						CO2ePerTonne = 2000,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
					},
					LegOrder = 0
				}
			});

			return dataObject;
		}

		public static UniversalShipment GetSampleCO2eResponseDataObject_WithCO2eDistanceInKM()
		{
			var dataObject = GetSampleCO2eResponseDataObject();
			dataObject.GreenhouseGasEmission.CO2eDistanceInKm = 10_000m;
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2eDistanceInKm = 6_000m;
			dataObject.TransportLegCollection[1].GreenhouseGasEmission.CO2eDistanceInKm = 4_000m;

			return dataObject;
		}

		public static UniversalShipment GetSampleCO2eResponseDataObject_WithTEU()
		{
			var dataObject = GetSampleCO2eResponseDataObject();

			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTEU = 150m;
			dataObject.TransportLegCollection[0].GreenhouseGasEmission.CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" };

			dataObject.TransportLegCollection[1].GreenhouseGasEmission.CO2ePerTEU = 50m;
			dataObject.TransportLegCollection[1].GreenhouseGasEmission.CO2ePerTEUUnit = new UnitOfWeight { Code = "KG" };

			return dataObject;
		}

		public static UniversalShipment GetSampleCO2eResponseDataObjectWithZeroGreenhouseGasEmissionForLegs()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.DataProviderForCodeMapping = "WTG Greenhouse Gas Emission";
			dataObject.TotalWeight = 1m;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "T" };
			dataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			dataObject.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "VNVNH" };
			dataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 10000m,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>()
			{
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "AUSYD" },
					PortOfDischarge = new UNLOCO { Code = "SGSIN" },
					TransportMode = TransportMode.Sea,
					VoyageFlightNo = "VY1",
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2ePerTonne = 0,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "T" }
					}
				},
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "SIN" },
					PortOfDischarge = new UNLOCO { Code = "VII" },
					TransportMode = TransportMode.Air,
					VoyageFlightNo = "FL1",
					AircraftType = new CodeDescriptionPair { Code = "N95" },
				}
			});

			return dataObject;
		}

		public static UniversalDataBuss.DataObjects.Universal.Event GetSampleCO2eErrorEvent()
		{
			var dataContext = DataContextFactory.New();
			var universalEvent = UniversalEventCreator.Create(dataContext, Events.InterchangeRejectedCode);
			universalEvent.ContextCollection.Add(new Context() { Type = "FailureReason", Value = "ERROR! ERROR! and more ERROR..." });

			return universalEvent;
		}

		public static CommonShipment CreateForwardingShipmentWithIncompleteLegs(BusinessObjectFactory factory)
		{
			var shipment = factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "VNVNH";
			shipment.JS_ActualWeight = 1m;
			shipment.JS_UnitOfWeight = "T";
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var leg1 = consol.Transports[0];
			leg1.JW_VoyageFlight = "FL1";
			leg1.JW_AircraftType = "N95";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "";
			leg2.JW_RL_NKDiscPort = "";
			leg2.JW_TransportMode = "RAI";
			var leg3 = consol.Transports.AddNew();
			leg3.JW_RL_NKLoadPort = "VNSGN";
			leg3.JW_RL_NKDiscPort = "";
			leg2.JW_TransportMode = "ROA";
			return shipment;
		}

		public static CommonConsol CreateForwardingConsolWithIncompleteLegs(BusinessObjectFactory factory)
		{
			var consol = factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as CommonConsol;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "VNVNH";
			consol.JK_TransportMode = "AIR";
			consol.JK_TotalShipmentActWeightCheck = 1m;
			consol.JK_TotalShipmentActOtherUnit = "T";
			var leg1 = consol.Transports[0];
			leg1.JW_RL_NKLoadPort = "AUSYD";
			leg1.JW_RL_NKDiscPort = "SGSIN";
			leg1.JW_VoyageFlight = "FL1";
			leg1.JW_AircraftType = "N95";
			var leg2 = consol.Transports.AddNew();
			leg2.JW_RL_NKLoadPort = "";
			leg2.JW_RL_NKDiscPort = "";
			leg2.JW_TransportMode = "RAI";
			var leg3 = consol.Transports.AddNew();
			leg3.JW_RL_NKLoadPort = "VNSGN";
			leg3.JW_RL_NKDiscPort = "";
			leg2.JW_TransportMode = "ROA";
			return consol;
		}

		public static UniversalShipment GetSampleCO2eResponseDataObjectWithVirtualLegs()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.DataProviderForCodeMapping = "WTG Greenhouse Gas Emission";
			dataObject.TotalWeight = 1m;
			dataObject.TotalWeightUnit = new UnitOfWeight { Code = "T" };
			dataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			dataObject.PortOfLoading = new UNLOCO { Code = "MEL" };
			dataObject.PortOfDischarge = new UNLOCO { Code = "VII" };
			dataObject.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = 12500m,
				CO2eUnit = new UnitOfWeight { Code = "KG" }
			};

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>()
			{
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "MEL" },
					PortOfDischarge = new UNLOCO { Code = "SYD" },
					TransportMode = TransportMode.Air,
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = 8,
						CO2eUnit = new UnitOfWeight { Code = "T" },
						CO2ePerTonne = 8,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "T" }
					}
				},
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "SYD" },
					PortOfDischarge = new UNLOCO { Code = "SIN" },
					TransportMode = TransportMode.Air,
					VoyageFlightNo = "FL1",
					AircraftType = new CodeDescriptionPair { Code = "N95" },
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = 2000,
						CO2eUnit = new UnitOfWeight { Code = "KG" },
						CO2ePerTonne = 2000,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
					}
				},
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "SIN" },
					PortOfDischarge = new UNLOCO { Code = "SGN" },
					TransportMode = TransportMode.Air,
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = 2000,
						CO2eUnit = new UnitOfWeight { Code = "KG" },
						CO2ePerTonne = 2000,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
					}
				},
				new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "SGN" },
					PortOfDischarge = new UNLOCO { Code = "VII" },
					TransportMode = TransportMode.Air,
					GreenhouseGasEmission = new GreenhouseGasEmission
					{
						CO2e = 500,
						CO2eUnit = new UnitOfWeight { Code = "KG" },
						CO2ePerTonne = 500,
						CO2ePerTonneUnit = new UnitOfWeight { Code = "KG" }
					}
				}
			});

			return dataObject;
		}

		public static string CO2eUniversalShipment_Response
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(Assembly.Load("Enterprise.Freight.DataTransfer.Test")))
				{
					return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CO2eUniversalShipment_Response.xml");
				}
			}
		}

		public static string CO2eUniversalEvent_Response
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(Assembly.Load("Enterprise.Freight.DataTransfer.Test")))
				{
					return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CO2eUniversalEvent_Response.xml");
				}
			}
		}

		public static string CO2eUniversalEvent_Response_TB
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(Assembly.Load("Enterprise.Freight.DataTransfer.Test")))
				{
					return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CO2eUniversalEvent_Response_TB.xml");
				}
			}
		}

		public static string CO2eUniversalShipment_Response_TB
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(Assembly.Load("Enterprise.Freight.DataTransfer.Test")))
				{
					return resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CO2eUniversalShipment_Response_TB.xml");
				}
			}
		}

		public static IApiResponse<EmissionResult> GenerateEmissionResponse(string str, string mediaType = "application/xml")
		{
			var content = new StringContent(str);
			content.Headers.ContentType.MediaType = mediaType;
			var serializer = new EmissionSerializer();
			var result = serializer.FromHttpContentAsync<EmissionResult>(content).GetAwaiter().GetResult();
			var responseMock = new Mock<IApiResponse<EmissionResult>>();
			responseMock.SetupGet(x => x.Content).Returns(result);
			return responseMock.Object;
		}

		public static IDtbBooking CreateTransportBooking(IDtbBookingParent parent, string direction, BusinessObjectFactory factory)
		{
			var tConsolidation = factory.New<IDtbBookingConsolidation>();
			tConsolidation.KB_ParentTableCode = parent.TablePrefix;
			tConsolidation.KB_ParentID = parent.PK;
			tConsolidation.KB_JobDirection = direction;
			var tBooking = factory.New<IDtbBooking>();
			tBooking.KM_KB_Booking = tConsolidation.PK;

			var pkgPackageJob = factory.New(ObjectFactory.GetType<IPkgPackageJob>());
			pkgPackageJob[PkgPackageJobSchema.KJ_ParentID] = tConsolidation.PK;
			pkgPackageJob[PkgPackageJobSchema.KJ_ParentTableCode] = ((BusinessObject)tConsolidation).TablePrefix;

			var package = factory.New(ObjectFactory.GetType<IPkgPackage>());
			package[PkgPackageSchema.KP_KJ_ParentPackageJob] = pkgPackageJob.PK;
			package[PkgPackageSchema.KP_F3_NKPackType] = "PKG";
			package[PkgPackageSchema.KP_PackageQty] = 4;
			package[PkgPackageSchema.KP_Weight] = 1000;

			var pic = CreateInstruction(tBooking, "PIC", factory);
			CreatePackageDivot(pic, package, 4, factory);

			var dlv = CreateInstruction(tBooking, "DLV", factory);
			CreatePackageDivot(dlv, package, 4, factory);

			return tBooking;
		}

		static IDtbBookingInstruction CreateInstruction(IDtbBooking booking, string type, BusinessObjectFactory factory)
		{
			var instruction = factory.New<IDtbBookingInstruction>();
			instruction.KN_InstructionType = type;
			instruction.KN_KM_BookingMovement = booking.PK;
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			instruction.Address.E2_OA_Address = orgHeader.MainAddress.PK;
			return instruction;
		}

		static IDtbBookingInstructionPkgDivot CreatePackageDivot(IDtbBookingInstruction instruction, BusinessObject package, int quantity, BusinessObjectFactory factory)
		{
			var divot = instruction.PackageDivots.AddNew();
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = quantity;
			return divot;
		}
	}
}
