using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolManyToManyCollection))]
	public class CFSLoadListConsolManyToManyCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTranshipToOtherCFSNotResetWhenAddingDomesticLoadList()
		{
			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "AUSYD";

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_TranshipToOtherCFS = true;

			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);

			Assert(shipment.JS_TranshipToOtherCFS);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CFSShipment parent = Factory.New<CFSShipment>();
			parent.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			parent.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2)))).RL_Code;
			Factory.Save();
			return new CFSLoadListConsolManyToManyCollection(parent);
		}

		public void TestGetEarliestConsol()
		{
			CFSLoadListConsol consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_AgentsReference = "CONSOL1";
			Transport transport1 = consol1.Transports[0];
			transport1.JW_ETD = new ZDateTime(2006, 1, 3);
			transport1.JW_ETA = new ZDateTime(2006, 1, 3);

			CFSLoadListConsol consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_AgentsReference = "CONSOL2";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_ETD = new ZDateTime(2006, 1, 2);
			transport2.JW_ETA = new ZDateTime(2006, 1, 2);
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUBNE";

			CFSLoadListConsol consol3 = Factory.New<CFSLoadListConsol>();
			consol3.JK_AgentsReference = "CONSOL3";
			Transport transport3 = consol3.Transports[0];
			transport3.JW_ETD = new ZDateTime(2006, 1, 2);
			transport3.JW_ETA = new ZDateTime(2006, 1, 3);
			transport2.JW_RL_NKLoadPort = "HKHKG";
			transport2.JW_RL_NKDiscPort = "AUPER";

			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			AssertEquals("shipment.Consols.GetEarliestConsol().JK_AgentsReference", "CONSOL2", shipment.Consols.GetEarliestConsol().JK_AgentsReference);
		}

		public void TestShipmentClientRefisSetToFirstConsolClientRef()
		{
			CFSLoadListConsol consol1 = Factory.New<CFSLoadListConsol>();
			CFSLoadListConsol consol2 = Factory.New<CFSLoadListConsol>();
			consol1.JK_AgentsReference = "ABC";
			consol2.JK_AgentsReference = "123";
			Factory.Save();

			CFSShipment shipment = Factory.New<CFSShipment>();
			Factory.Save();
			shipment.Consols.Add(consol1);
			AssertEquals("Shipment Client Ref should set to Consol value", consol1.JK_AgentsReference, shipment.JS_ConsolReference);
			shipment.Consols.Add(consol2);
			AssertEquals("Shipment Client Ref should not set to Consol2 value", consol1.JK_AgentsReference, shipment.JS_ConsolReference);
			shipment.Consols.RemoveAll();
			shipment.JS_ConsolReference = "12345";
			shipment.Consols.Add(consol1);
			AssertEquals("Shipment Client Ref should not set to Consol1 value", "12345", shipment.JS_ConsolReference);

			shipment.Consols.RemoveAll();
			shipment.JS_ConsolReference = ZString.Empty;
			CFSLoadListConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_AgentsReference = "9999";
			AssertEquals("Shipment Client Ref should set to Consol3 value", consol3.JK_AgentsReference, shipment.JS_ConsolReference);
		}

		#region HandledOnBehalfOfForwarder

		public void TestHandledOnBehalfOfForwarderOnAttachFromShipment()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "bbb";
			org2.OH_FullName = "ccc";

			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";
			consol1.JK_OH_Forwarder = org1.PK;

			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "AUMEL";
			consol2.JK_OH_Forwarder = org2.PK;

			var shipment = Factory.New<CFSShipment>();
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			Factory.Save();

			AssertEquals("Load Lists attached to the shipment", org1.PK, shipment.JS_OH_HandledOnBehalfOfForwarder);
		}

		public void TestHandledOnBehalfOfForwarderOnAttachFromLoadList()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "bbb";

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_RL_NKLoadPort = "AUMEL";
			loadList.JK_RL_NKDischargePort = "AUSYD";
			loadList.JK_OH_Forwarder = org1.PK;

			var shipment = Factory.New<CFSShipment>();

			loadList.Shipments.Add(shipment);
			Factory.Save();

			AssertEquals("shipment attached to load list", org1.PK, shipment.JS_OH_HandledOnBehalfOfForwarder);
		}

		#endregion

		int CurrentVoyageIndex;
		ArrayList fVoyageNumbers;
		protected string NextVoyageNumber()
		{
			if (fVoyageNumbers == null)
			{
				fVoyageNumbers = new ArrayList();
				fVoyageNumbers.Add("392");
				fVoyageNumbers.Add("452");
				fVoyageNumbers.Add("658");
				fVoyageNumbers.Add("497");
				fVoyageNumbers.Add("666");
				fVoyageNumbers.Add("777");
			}
			if (CurrentVoyageIndex > fVoyageNumbers.Count)
			{
				CurrentVoyageIndex = 0;
			}
			return (string)fVoyageNumbers[CurrentVoyageIndex++];
		}

		int CurrentOriginUNOCOIndex;
		ArrayList fOriginUNLOCOs;
		protected string NextOriginUNLOCO()
		{
			if (fOriginUNLOCOs == null)
			{
				fOriginUNLOCOs = new ArrayList();
				fOriginUNLOCOs.Add("NZAKL");
				fOriginUNLOCOs.Add("AUSYD");
				fOriginUNLOCOs.Add("SGSIN");
				fOriginUNLOCOs.Add("HKHKG");
				fOriginUNLOCOs.Add("USLAX");
				fOriginUNLOCOs.Add("WFFUT");
			}
			if (CurrentOriginUNOCOIndex > fOriginUNLOCOs.Count)
			{
				CurrentOriginUNOCOIndex = 0;
			}
			return (string)fOriginUNLOCOs[CurrentOriginUNOCOIndex++];
		}

		int CurrentDestinationUNOCOIndex;
		ArrayList fDestinationUNLOCOs;
		protected string NextDestinationUNLOCO()
		{
			if (fDestinationUNLOCOs == null)
			{
				fDestinationUNLOCOs = new ArrayList();
				fDestinationUNLOCOs.Add("USLAX");
				fDestinationUNLOCOs.Add("NZAKL");
				fDestinationUNLOCOs.Add("AUSYD");
				fDestinationUNLOCOs.Add("SGSIN");
				fDestinationUNLOCOs.Add("HKHKG");
				fDestinationUNLOCOs.Add("UAAAR");
			}
			if (CurrentDestinationUNOCOIndex > fDestinationUNLOCOs.Count)
			{
				CurrentDestinationUNOCOIndex = 0;
			}
			return (string)fDestinationUNLOCOs[CurrentDestinationUNOCOIndex++];
		}

		protected virtual Type ParentConsolType
		{
			get { return typeof(CFSLoadListConsol); }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CFSLoadListConsol result = GetValidUniqueConsol();
			return result;
		}

		protected CFSLoadListConsol GetValidUniqueConsol()
		{
			CFSLoadListConsol result = (CFSLoadListConsol)Factory.NewWithValidTestData(ParentConsolType);
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Transport transport = result.Transports[0];
			transport.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_IsLinked = true;
			transport.JW_JX = GetNextSailing();
			return result;
		}

		protected ZGuid GetNextSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = NextVoyageNumber();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = NextOriginUNLOCO();
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = NextDestinationUNLOCO();
			voyage.GenerateSailings();
			return voyage.Sailings[0].PK;
		}
	}
}
