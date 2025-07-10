using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SynchroniserTestCase : TestCaseWithFactory
	{
		public const string TestOceanBill = "OBL2341284";
		public const string TestRV_NKVessel = "8610033";
		public const string TestVoyage = "4";
		public const string TestRL_NKPortOfLoading = "USLAX";
		public const string TestRL_NKPortOfDischarge = "AUSYD";
		public ZGuid TestOH_ShippingLine
		{
			get
			{
				if (!fTestOH_ShippingLine.IsValid)
				{
					fTestOH_ShippingLine = TestShippingLine.PK;
				}
				return fTestOH_ShippingLine;
			}
		}

		public OrgHeader TestShippingLine
		{
			get
			{
				if (testShippingLine == null)
				{
					testShippingLine = Factory.New<OrgHeader>();
					testShippingLine.OH_FullName = "Synchroniser Shipping Line";
					testShippingLine.OH_Code = "TSTSHPLNE";
					testShippingLine.OH_IsShippingLine = true;
				}
				return testShippingLine;
			}
		}
		OrgHeader testShippingLine;

		public const string TestPrincipalID = "";
		public const string TestLloydsIMO = "";

		public const string TestHouseBillNumber = "J2004290011";
		public const string TestMasterHouse = "J20042900";
		public const string TestRL_NK_PortOfOrigin = "USLAX";
		public const string TestRL_NK_PortOfDestination = "AUMEL";
		public const string TestRN_NKGoodsOrigin = "US";
		public const string TestPrepaidCollectOther = "PPD";

		public ZGuid TestOH_Consignor
		{
			get
			{
				if (!fTestOH_Consignor.IsValid)
				{
					OrgHeader consignor = Factory.New<OrgHeader>();
					consignor.OH_FullName = "Synchroniser Consignor";
					consignor.OH_Code = "TSTCNSGNR";
					consignor.MainAddress.OA_Address1 = "addr1";
					consignor.MainAddress.OA_Address2 = "addr2";
					consignor.MainAddress.OA_City = "foocity";
					consignor.MainAddress.OA_Phone = "123";
					consignor.MainAddress.OA_PostCode = "3234";
					consignor.MainAddress.OA_State = "BOO";
					consignor.OH_IsConsignor = true;
					fTestOH_Consignor = consignor.PK;
				}
				return fTestOH_Consignor;
			}
		}

		public ZGuid TestOH_Consignee
		{
			get
			{
				if (!fTestOH_Consignee.IsValid)
				{
					OrgHeader consignee = Factory.New<OrgHeader>();
					consignee.OH_FullName = "Synchroniser Consignee";
					consignee.OH_Code = "TSTCNSNEE";
					consignee.MainAddress.OA_Address1 = "1addr";
					consignee.MainAddress.OA_Address2 = "2addr";
					consignee.MainAddress.OA_City = "barcity";
					consignee.MainAddress.OA_Phone = "333";
					consignee.MainAddress.OA_Fax = "555";
					consignee.MainAddress.OA_PostCode = "6768";
					consignee.MainAddress.OA_State = "HEH";
					consignee.OH_IsConsignee = true;
					fTestOH_Consignee = consignee.PK;
				}
				return fTestOH_Consignee;
			}
		}

		public ZGuid TestOH_NotifyParty
		{
			get
			{
				if (!fTestOH_NotifyParty.IsValid)
				{
					OrgHeader notifyParty = Factory.New<OrgHeader>();
					notifyParty.OH_FullName = "Synchroniser NotifyParty";
					notifyParty.OH_Code = "TSTNTFYPT";
					fTestOH_NotifyParty = notifyParty.PK;
				}
				return fTestOH_NotifyParty;
			}
		}

		public const string TestMoveUnderbondFrom = "D321A";
		public const string TestMoveUnderbondTo = "A077J";
		public const string TestContainerNumber = "CTRL0000022";
		public const string TestContainerNumber2 = "GATU0613887";
		public const string TestSealNumber = "487564";
		public const string TestSealNumber2 = "7837302";
		public const string TestContainerTypeNK = "40GP";
		public const string TestContainerType2NK = "20GP";

		public const string TestGoodsDescription = "SHORT DESCRIPTION";
		public const string TestMarksAndNumbers = "SCRATCH HERE - Scratch there";
		public const string TestMarksAndNumbers2 = "More marks and numbers";
		public const string Test3CharPackingType = "BOX";
		public const string Test2CharPackingType = "BX";
		public const int TestPackageCount = 20;
		public const decimal TestWeight = 1201.4m;
		public const string TestWeightUQNotKG = Enterprise.Core.Constants.Weight.Pounds;
		public const decimal TestVolume = 32.0m;
		public const string TestVolumeUQNotM3 = Enterprise.Core.Constants.Volume.CubicFeet;

		public const string TestOrderNumber1 = "R332609";
		public const string TestOrderNumber2 = "T832610";
		public const string TestReferenceOrderNumberNote = "Gis-33092";

		public const decimal TestRC_Length = 1m;
		public const decimal TestRC_Height = 2m;
		public const decimal TestRC_Width = 3m;
		public const decimal TestRC_TareWeight = 7m;
		public const decimal TestJC_TotalLength = 4m;
		public const decimal TestJC_TotalHeight = 5m;
		public const decimal TestJC_TotalWidth = 6m;
		public const decimal TestJC_TareWeight = 8m;

		ZGuid fTestOH_ShippingLine;
		ZGuid fTestOH_Consignee;
		ZGuid fTestOH_Consignor;
		ZGuid fTestOH_NotifyParty;

		protected ForwardingConsol CreateSeaConsol()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			return result;
		}

		protected ForwardingConsol CreateFCLConsol()
		{
			ForwardingConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			return result;
		}

		protected ForwardingConsol CreateBCNConsol()
		{
			ForwardingConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			return result;
		}

		protected ForwardingConsol CreateGroupageConsol()
		{
			ForwardingConsol result = CreateSeaConsol();
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Groupage;
			return result;
		}

		protected ForwardingConsol CreateAirConsol()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			result.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Loose;
			return result;
		}

		protected OrgHeader GetLocalConsignee()
		{
			OrgHeader result = OrgHeader.New(Factory);
			result.OH_FullName = "Local Consignee";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.MainAddress.OA_Address1 = "Local consignee address";
			result.OH_IsConsignee = true;
			return result;
		}

		protected OrgHeader GetOverseasConsignor()
		{
			OrgHeader result = OrgHeader.New(Factory);
			result.OH_FullName = "Overseas Consignor";
			result.OH_RL_NKClosestPort = "USLAX";
			result.MainAddress.OA_Address1 = "overseas consignor address";
			result.OH_IsConsignor = true;
			return result;
		}

		protected OrgHeader GetNotifyParty()
		{
			OrgHeader result = OrgHeader.New(Factory);
			result.OH_FullName = "Notify Party";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.MainAddress.OA_Address1 = "Notify Party Address";
			return result;
		}

		protected JobDocAddress GetDocConsignee(CommonShipment parent)
		{
			JobDocAddress result = JobDocAddress.New(parent);
			result.E2_AddressOverride = true;
			result.E2_CompanyName = "Doc Consignee";
			result.E2_Address1 = "Local consignee address";
			result.E2_City = "CNEECity";
			result.E2_State = "CNEEState";
			return result;
		}

		protected JobDocAddress GetDocConsignor(CommonShipment parent)
		{
			JobDocAddress result = JobDocAddress.New(parent);
			result.E2_AddressOverride = true;
			result.E2_CompanyName = "Doc Consignor";
			result.E2_Address1 = "overseas consignor address";
			result.E2_City = "CNORCity";
			result.E2_State = "CNOEState";
			return result;
		}

		protected JobDocAddress GetDocNotifyParty(CommonShipment parent)
		{
			JobDocAddress result = JobDocAddress.New(parent);
			result.E2_AddressOverride = true;
			result.E2_CompanyName = "Doc Notify Party";
			result.E2_Address1 = "Notify Party Address";
			result.E2_City = "NPCity";
			result.E2_State = "NPState";
			return result;
		}

		protected JobSailing ImportSailing
		{
			get
			{
				if (fImportSailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.JV_VoyageFlight = TestVoyage;
					voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty)).RV_FK;
					voyage.Origins.AddNew();
					voyage.Origins[0].JA_RL_NKPortOfLoading = "NZAKL";
					voyage.Origins[0].JA_E_DEP = ZDateTime.Now.AddDays(-8);
					voyage.Destinations.AddNew();
					voyage.Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";
					voyage.Destinations[0].JB_E_ARV = ZDateTime.Now.AddDays(1);
					voyage.GenerateSailings();
					fImportSailing = voyage.Sailings[0];
				}
				return fImportSailing;
			}
		}
		JobSailing fImportSailing;
	}
}
