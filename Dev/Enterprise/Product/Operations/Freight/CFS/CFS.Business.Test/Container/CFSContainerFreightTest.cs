using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSContainerFreightTest : BaseFreightTest
	{
		public void TestJC_PackUnpackDate()
		{
			AssertEquals("PreCon: PackUnpackStatusHelper..PackUnpackStatus.InvalidSamePort", PackUnpackStatusHelper.PackUnpackStatus.InvalidSamePort, Container.PackOrUnpackStatus);
			ZDateTime now = ZDateTime.Now;
			Container.JC_PackDate = now;
			Container.JC_LCLUnpack = now.AddDays(1);
			AssertEquals("JC_PackUnpackDate return the unpack date", now.AddDays(1), Container.JC_PackUnpackDate);

			Origin.JA_RL_NKPortOfLoading = OverseasPort;
			Destination.JB_RL_NKPortOfDischarge = HomePort;
			AssertEquals("coming in = unpack", PackUnpackStatusHelper.PackUnpackStatus.Unpack, Container.PackOrUnpackStatus);
			AssertEquals("JC_PackUnpackDate return the unpack date", now.AddDays(1), Container.JC_PackUnpackDate);

			Origin.JA_RL_NKPortOfLoading = HomePort;
			Destination.JB_RL_NKPortOfDischarge = OverseasPort;
			AssertEquals("going out = pack", PackUnpackStatusHelper.PackUnpackStatus.Pack, Container.PackOrUnpackStatus);
			AssertEquals("JC_PackUnpackDate return the pack date", now, Container.JC_PackUnpackDate);
		}

		public void TestPackOrUnpackStatus()
		{
			Origin.JA_RL_NKPortOfLoading = HomePort;
			Destination.JB_RL_NKPortOfDischarge = OverseasPort;
			AssertEquals("going out = pack", PackUnpackStatusHelper.PackUnpackStatus.Pack, Container.PackOrUnpackStatus);

			Origin.JA_RL_NKPortOfLoading = OverseasPort;
			Destination.JB_RL_NKPortOfDischarge = HomePort;
			AssertEquals("coming in = unpack", PackUnpackStatusHelper.PackUnpackStatus.Unpack, Container.PackOrUnpackStatus);

			Origin.JA_RL_NKPortOfLoading = HomePort;
			Destination.JB_RL_NKPortOfDischarge = HomePort;
			AssertEquals("they can't both be the same port", PackUnpackStatusHelper.PackUnpackStatus.InvalidSamePort, Container.PackOrUnpackStatus);

			GlbBranch melDepot = CreateDepot(AlternateHomePort);

			Origin.JA_RL_NKPortOfLoading = OverseasPort;
			Destination.JB_RL_NKPortOfDischarge = AlternateHomePort;
			AssertEquals("none", PackUnpackStatusHelper.PackUnpackStatus.None, Container.PackOrUnpackStatus);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
			AssertEquals("should barf if there's no home port to test against", PackUnpackStatusHelper.PackUnpackStatus.InvalidBranchHomePort, Container.PackOrUnpackStatus);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = HomePort;
		}

		public void TestPackOrUnpackStatusUsesTransportPortsIfSailingIsEmpty()
		{
			CFSLoadListConsol cfsLLC = Factory.NewWithValidTestData<CFSLoadListConsol>();
			Container.JC_JK = cfsLLC.PK;

			Transport interestingTransport = cfsLLC.Transports.MostInterestingTransport;
			interestingTransport.JW_JX = Sailing.PK;
			interestingTransport.JW_IsLinked = true;

			Origin.JA_RL_NKPortOfLoading = HomePort;
			Destination.JB_RL_NKPortOfDischarge = OverseasPort;
			AssertEquals("Sailing was used", PackUnpackStatusHelper.PackUnpackStatus.Pack, Container.PackOrUnpackStatus);

			interestingTransport.JW_IsLinked = false;
			AssertNull(Container.Sailing);

			interestingTransport.JW_RL_NKLoadPort = OverseasPort;
			interestingTransport.JW_RL_NKDiscPort = HomePort;
			AssertEquals("Consol's MostInterestingTransport was used", PackUnpackStatusHelper.PackUnpackStatus.Unpack, Container.PackOrUnpackStatus);
		}

		public void TestCargoStatusAdvice()
		{
			var releaseTime = ZDateTime.Now.AddHours(-2);
			var testContainer = Factory.New<CFSContainer>();
			var cargoStatusAdviceString = OldCFSShipmentStatusProvider.CMRGatePassStatuses.Clear;
			var cargoStatusAdvice = testContainer.Logs.AddNew(Events.SeaCargoDepotEvent, cargoStatusAdviceString, releaseTime.ToOffset());

			AssertEquals("TestContainer Cargo Status Advice Status", cargoStatusAdviceString, testContainer.JC_CargoStatusAdviceStatus);
			AssertEquals("TestContainer Cargo Status Advice Time", releaseTime, testContainer.JC_CargoStatusAdviceDate);
			cargoStatusAdvice.Cancel();
			AssertEquals("TestContainer Cargo Status Advice Status", "", testContainer.JC_CargoStatusAdviceStatus);
			cargoStatusAdviceString = OldCFSShipmentStatusProvider.CMRGatePassStatuses.Detained;
			cargoStatusAdvice = testContainer.Logs.AddNew(Events.SeaCargoDepotEvent, cargoStatusAdviceString);
			AssertEquals("TestContainer Cargo Status Advice Status", cargoStatusAdviceString, testContainer.JC_CargoStatusAdviceStatus);
			cargoStatusAdvice.Cancel();
			AssertEquals("TestContainer Cargo Status Advice Status", "", testContainer.JC_CargoStatusAdviceStatus);
		}

		public void TestImpendingArrival()
		{
			var arrivalTime = ZDateTime.Now.AddHours(2);
			var testContainer = Factory.New<CFSContainer>();
			var impendingArrivalString = "IMPENDING ARRIVAL";
			var impendingArrivalEvent = testContainer.Logs.AddNew(Events.SeaCargoDepotEvent, impendingArrivalString, arrivalTime.ToOffset());
			AssertEquals("TestContainer Cargo Status Advice", impendingArrivalString, testContainer.JC_ImpendingArrivalStatus);
			AssertEquals("TestContainer Cargo Status Advice", arrivalTime, testContainer.JC_ImpendingArrivalDate);
			impendingArrivalEvent.Cancel();
			AssertEquals("TestContainer Cargo Status Advice Status", "", testContainer.JC_ImpendingArrivalStatus);
		}

		#region Document Tests
		public void TestContainsImportShipments()
		{
			CFSContainer containerRego = Factory.New<CFSContainer>();
			AssertEquals(ZBool.False, containerRego.ContainsImportShipments);
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.IMP, null));

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Destination.JB_RL_NKPortOfDischarge = "USCHI";
			containerRego.JC_JX = Sailing.PK;

			CreatePackUnpackShipment("USCHI", containerRego);
			CreatePackUnpackShipment("SGSIN", containerRego);

			Assert(containerRego.ContainsImportShipments);
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.IMP, null));
		}

		public void TestContainsTranshipments()
		{
			CFSContainer containerRego = Factory.New<CFSContainer>();
			AssertEquals(ZBool.False, containerRego.ContainsTranshipments);

			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			containerRego.JC_JX = Sailing.PK;

			CreatePackUnpackShipment("MYKUL", containerRego);
			CreatePackUnpackShipment("SGSIN", containerRego);
			CreatePackUnpackShipment("USCHI", containerRego);

			Assert(containerRego.ContainsTranshipments);
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.TRN, null));
		}

		public void TestContainsOnForwardingShipments()
		{
			CFSContainer containerRego = Factory.New<CFSContainer>();
			AssertEquals(ZBool.False, containerRego.ContainsOnForwardingShipments);
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OFW, null));

			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			containerRego.JC_JX = Sailing.PK;

			CreatePackUnpackShipment("AUBNE", containerRego);
			CreatePackUnpackShipment("SGSIN", containerRego);
			CreatePackUnpackShipment("AUSYD", containerRego);

			Assert(containerRego.ContainsTranshipments);
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OFW, null));
		}

		public void TestGetMenuTemplateFilterValue()
		{
			CFSContainer containerRego = Factory.New<CFSContainer>();
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.IMP, null));
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OFW, null));
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.TRN, null));

			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Destination.JB_RL_NKPortOfDischarge = "AUMEL";
			containerRego.JC_JX = Sailing.PK;

			CreatePackUnpackShipment("AUMEL", containerRego);
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.IMP, null));
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OFW, null));
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.TRN, null));

			CreatePackUnpackShipment("SGSIN", containerRego);
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.IMP, null));
			AssertEquals("", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OFW, null));
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.TRN, null));

			CreatePackUnpackShipment("AUSYD", containerRego);
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.IMP, null));
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OFW, null));
			AssertEquals("PRINT", containerRego.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.TRN, null));
		}
		#endregion

		#region Implementation

		protected CFSContainer Container;
		protected JobSailing Sailing;
		protected VoyageOrigin Origin;
		protected VoyageDestination Destination;
		protected JobVoyage Voyage;
		protected ZString CurrentBranchHomePort;

		protected CFSShipment CreatePackUnpackShipment(ZString destination, CFSContainer containerRego)
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKDestination = destination;
			containerRego.PackUnpackShipments.Add(shipment);
			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();

			CurrentBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			Voyage = Factory.New<JobVoyage>();
			Origin = Voyage.Origins.AddNew();
			Destination = Voyage.Destinations.AddNew();
			Sailing = Factory.New<JobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;

			Container = Factory.New<CFSContainer>();
			Container.JC_JX = Sailing.PK;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CurrentBranchHomePort;
		}

		protected GlbBranch CreateDepot(ZString uNLOCO)
		{
			GlbBranch result = Factory.New<GlbBranch>();
			result.GB_Address1 = "Address 1 " + uNLOCO;
			result.GB_Code = "DEP";
			result.GB_BranchName = "Depot Branch " + uNLOCO;
			result.GB_RL_NKHomePort = uNLOCO;
			result.GB_OH_OrgProxy = CreateProxyOrg(uNLOCO).PK;
			return result;
		}

		protected OrgHeader CreateProxyOrg(ZString uNLOCO)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Proxy Org Name" + uNLOCO;
			result.MainAddress.OA_Address1 = "Address 1 " + uNLOCO;
			result.OH_RL_NKClosestPort = uNLOCO;
			result.OH_IsUnpackDepot = true;
			return result;
		}

		protected BusinessObject GetBusinessObject()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = TestImportSailing.PK;
			CFSContainer result = loadList.Containers.AddNew();
			return result;
		}

		protected ZString ForeignPort
		{
			get { return (GlbBranch.CurrentBranch.GB_RL_NKHomePort == "SGSIN") ? "HKHKG" : "SGSIN"; }
		}

		protected JobSailing TestImportSailing
		{
			get
			{
				if (fTestImportSailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.JV_VoyageType = Enterprise.Core.Constants.TransportModes.Sea;
					voyage.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
					voyage.JV_VoyageFlight = "43";
					voyage.Origins.AddNew();
					voyage.Origins[0].JA_RL_NKPortOfLoading = ForeignPort;
					voyage.Origins[0].JA_E_DEP = ZDateTime.Today.AddDays(-5);
					voyage.Destinations.AddNew();
					voyage.Destinations[0].JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(5);
					voyage.GenerateSailings();
					fTestImportSailing = voyage.Sailings[0];
				}
				return fTestImportSailing;
			}
		}
		JobSailing fTestImportSailing;

		#endregion

		public void TestLoadingPopulatesSailings()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader client = newFactory.New<OrgHeader>();
			client.OH_FullName = "TEST ORG";
			client.OH_RL_NKClosestPort = "AUSYD";
			client.MainAddress.OA_Address1 = "TEST ADDRESS";

			RefContainer containerType = newFactory.New<RefContainer>();
			containerType.RC_Code = "MG01";
			containerType.RC_ContainerType = Enterprise.Core.Constants.ContainerTypes.FlatRack;

			SailingsForTestClasses helper = new SailingsForTestClasses(newFactory);

			CFSContainer newContainer = newFactory.New<CFSContainer>();
			newContainer.JC_JX = helper.SydLaxSailing.PK;
			newContainer.JC_ContainerNum = "123";
			newContainer.JC_OH_CFSClient = client.PK;
			newContainer.JC_RC = containerType.PK;

			AssertNotNull("Not expecting Sailing to be null.", newContainer.Sailing);

			// move into propertys' test
			Assert("Not expecting calculated vessel to be empty.", !newContainer.JC_JV_NKVessel.IsEmpty);
			Assert("Not expecting calculated voyage no to be empty.", !newContainer.JC_JV_VoyageFlight.IsEmpty);
			Assert("Not expecting calculated eta to be empty.", !newContainer.JC_JB_E_ARV.IsEmpty);
			Assert("Not expecting calculated etd to be empty.", !newContainer.JC_JA_E_DEP.IsEmpty);
			Assert("Not expecting calculated load port to be empty.", !newContainer.JC_JA_NKPortOfLoading.IsEmpty);
			Assert("Not expecting calculated discharge port to be empty.", !newContainer.JC_JB_NKPortOfDischarge.IsEmpty);

			newFactory.Save();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			CFSContainer loadedContainer = loadFactory.Load<CFSContainer>(newContainer.PK);

			AssertNotNull("Not expecting Sailing to be null.", loadedContainer.Sailing);

			Assert("Not expecting calculated vessel to be empty.", !loadedContainer.JC_JV_NKVessel.IsEmpty);
			Assert("Not expecting calculated voyage no to be empty.", !loadedContainer.JC_JV_VoyageFlight.IsEmpty);
			Assert("Not expecting calculated eta to be empty.", !loadedContainer.JC_JB_E_ARV.IsEmpty);
			Assert("Not expecting calculated etd to be empty.", !loadedContainer.JC_JA_E_DEP.IsEmpty);
			Assert("Not expecting calculated load port to be empty.", !loadedContainer.JC_JA_NKPortOfLoading.IsEmpty);
			Assert("Not expecting calculated discharge port to be empty.", !loadedContainer.JC_JB_NKPortOfDischarge.IsEmpty);
		}

		public void TestJC_TrainWagonNumber()
		{
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, Container.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_TrainWagonNumber", "", Container.JC_TrainWagonNumber);

			Container.JC_TrainWagonNumber = "123";
			Container.JC_TransportMode = Constants.TransportModes.Air;
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, Container.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_TrainWagonNumber", "", Container.JC_TrainWagonNumber);

			Container.JC_TrainWagonNumber = "123";
			Container.JC_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, Container.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_TrainWagonNumber", "", Container.JC_TrainWagonNumber);

			Container.JC_TrainWagonNumber = "123";
			Container.JC_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", false, Container.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_TrainWagonNumber", "123", Container.JC_TrainWagonNumber);

			Container.JC_TransportMode = Constants.TransportModes.Road;
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, Container.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("JC_TrainWagonNumber", "", Container.JC_TrainWagonNumber);

			Factory.Save();

			var loadedContainer = Factory.Load<CFSContainer>(Container.PK);
			AssertEquals("LoadedContainer.JC_TrainWagonNumberInfo.ReadOnly", true, loadedContainer.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("LoadedContainer.JC_TrainWagonNumber", "", loadedContainer.JC_TrainWagonNumber);

			Container.JC_TrainWagonNumber = "123";
			Container.JC_TransportMode = Constants.TransportModes.Rail;
			loadedContainer = Factory.Load<CFSContainer>(Container.PK);
			AssertEquals("LoadedContainer.JC_TrainWagonNumberInfo.ReadOnly", false, loadedContainer.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("LoadedContainer.JC_TrainWagonNumber", "123", loadedContainer.JC_TrainWagonNumber);
		}
	}
}
