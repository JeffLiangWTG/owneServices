using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class EIDOBaseApplicatorTest : ReleaseImportOrderActionMethodApplicatorTest
	{
		[EIDOMessagingConfiguration]
		public void TestPerformance()
		{
			const int ShipmentCount = 10;

			ZGuid[] pks = Array.ConvertAll(CreateShipmentsForPerformanceTest(ShipmentCount), (s) => s.PK);
			Factory.Save();

			BusinessObjectFactory factoryForRun = new BusinessObjectFactory();
			BaseAgencyTest.SetAcosCode(GlbBranch.CurrentBranch.OrgProxy, "us");
			BusinessObject[] targets = factoryForRun.Load<BillOfLading>(new ZQuery(JobShipmentSchema.PK, pks));

			AssertEquals("precondition:", ShipmentCount, targets.Length);
			this.ApplyApplicator(targets, "");
			// DO NOT INCREASE THIS WITHOUT FIRST DISCUSSING WITH THE AGENCY TEAM!!!
			AssertMaxDbHits(200, factoryForRun);
			/*
OrgHeader: 23
OrgAddress: 20
OrgContact: 20
UNDGDataItem: 20
JobVoyage: 14
OrgCusCode: 12
CusInBondHeader: 10
JobDocAddress: 10
StmNote: 10
OrgAddressCapability: 8
JobVoyDestination: 6
JobVoyOrigin: 6
JobSailing: 5
UNDGSubstance: 4
JobContainerPackPivot: 2
OrgCompanyData: 2
RefVessel: 2
ViewUNDGAttribute: 2
CusEntryNum: 1
EDIMessage: 1
JobCO2e: 1
JobConsolTransport: 1
JobContainer: 1
JobHeader: 1
JobPackLines: 1
JobShipment: 1
JobTradeLaneVoyage: 1
JobVoyCountry: 1
RefContainerStock: 1
RefCurrency: 1
RefPackType: 1
RefUNLOCO: 1
StmALog: 1
UNDGSubstancePivot: 1
GlbBranch: 0
GlbCompany: 0
GlbDepartment: 0
GlbGroup: 0
GlbStaff: 0
GlbWorkTime: 0
RefCommodityCode: 0
RefContainer: 0
RefCountry: 0
RefCountryRequiredDocument: 0
RefCountryRules: 0
RefLocalLanguage: 0
RefServiceLevel: 0
RefTimeZone: 0
RefTimeZoneRule: 0
RefTimeZoneSet: 0*/

			//Hits: 200
		}

		[EIDOMessagingConfiguration(Enabled = false)]
		public void TestRunDisabled()
		{
			ZQuery eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);

			const string expectedLog =
				"ERROR: E-IDO messaging has not been enabled.\nYou can enable it in the 'Liner & Agency -> E-IDO Messaging' registry option.";

			BillOfLadingContainer target = NewContainerTarget("SGSIN", "AUBNE");

			this.ApplyApplicator(new BusinessObject[] { target.Booking }, expectedLog);
		}

		#region Implementation

		protected virtual BillOfLading[] CreateShipmentsForPerformanceTest(int count)
		{
			ZDateTime now = ZDateTime.Now;

			OrgHeader principal = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal");
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			BaseAgencyTest.SetAcosCode(principal, "principal");

			BillOfLading[] result = new BillOfLading[count];
			UNDGSubstance[] substances = Factory.Load<UNDGSubstance>(MaxRowsQuery(2));
			string[] containerNumbers = new string[] { "FAKE4100011", "FAKE4100027", "FAKE4100032", "FAKE4100046" };
			RefContainer[] containerTypes = Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP,20RE,40GP,45HC,40RE".Split(',')));

			JobSailing sailing = null;
			for (int i = 0; i < result.Length; i++)
			{
				OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
				shippingLine.OH_IsShippingLine = true;
				shippingLine.OH_IsShippingProvider = true;
				BaseAgencyTest.SetAcosCode(shippingLine, "ShpLine");

				if (sailing == null || i % 2 == 0)
				{
					OrgHeader cto = Factory.NewWithValidTestData<OrgHeader>();
					BaseAgencyTest.SetAcosCode(cto, "CTO");

					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.JV_VoyageFlight = "x" + i;
					voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
					voyage.JV_OH_Line = shippingLine.PK;

					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = "SGSIN";
					origin.JA_E_DEP = now.AddDays(i + 1);

					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUBNE";
					destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;
					destination.JB_E_ARV = now.AddDays(i + 5);

					voyage.GenerateSailings();
					sailing = voyage.Sailings[0];
				}

				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;

				OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_IsConsignee = true;

				BillOfLading shipment = Factory.New<BillOfLading>();
				shipment.JS_UniqueConsignRef = string.Format("S00000{0:000}", i);
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_JX = sailing.PK;
				shipment.JS_GoodsDescription = "Short description " + i;
				shipment.JS_HouseBill = "SINSYD" + i.ToString("0000");
				shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
				shipment.JS_OH_DeliveryAgent = principal.PK;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				shipment.OuterPackLines.RemoveAndDeleteAll();

				if (i % 3 == 0)
				{
					JobVoyage secondVoyage = Factory.New<JobVoyage>();
					secondVoyage.JV_VoyageFlight = "y" + i;
					secondVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
					secondVoyage.JV_OH_Line = shippingLine.PK;

					VoyageOrigin origin = secondVoyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = "AUBNE";
					origin.JA_E_DEP = now.AddDays(i + 10);

					VoyageDestination destination = secondVoyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = "AUSYD";
					destination.JB_E_ARV = now.AddDays(i + 15);

					Transport transport = shipment.Transports.AddNew();
					transport.JW_IsLinked = true;
					transport.JW_JX = secondVoyage.Sailings[0].PK;
				}

				BillOfLadingContainer container1 = shipment.RealContainers.AddNew();
				container1.JC_ContainerNum = Select(containerNumbers, i * 2 + 1);
				container1.JC_RC = Select(containerTypes, i * 2 + 1).PK;

				BillOfLadingContainer container2 = shipment.RealContainers.AddNew();
				container2.JC_ContainerNum = Select(containerNumbers, i * 2 + 2);
				container2.JC_RC = Select(containerTypes, i * 2 + 2).PK;

				BillOfLadingPackLine packline1 = shipment.OuterPackLines.AddNew();
				SetUNDG(packline1, substances, i + 0);
				packline1.JL_JC = container1.PK;
				packline1.JL_MarksAndNumbers = "Marks and Numbers.";

				BillOfLadingPackLine packline2 = shipment.OuterPackLines.AddNew();
				SetUNDG(packline2, substances, i + 1);
				packline2.JL_JC = container2.PK;
				packline2.JL_MarksAndNumbers = "Marks and Numbers.";

				result[i] = shipment;
			}

			return result;
		}

		protected BillOfLadingContainer NewContainerTarget(string load, string discharge)
		{
			BillOfLading shipment = NewShipmentTarget(load, discharge);
			BillOfLadingContainer container = NewContainerTarget(shipment, "FAKE4100011");

			shipment.RunPreSaveValidation();

			AssertNoErrors("precondition:", shipment);
			AssertNoMessageErrors("precondition:", shipment);

			return container;
		}

		protected BillOfLading NewShipmentTarget(string load, string discharge)
		{
			ZDateTime now = ZDateTime.Now;

			OrgHeader principal = FindOrCreatePrincipal(Factory, "Principal");
			BaseAgencyTest.SetAcosCode(principal, "principal");

			Factory.Save();

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			OrgHeader cto = Factory.NewWithValidTestData<OrgHeader>();
			BaseAgencyTest.SetAcosCode(cto, "CTO");

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsShippingProvider = true;
			BaseAgencyTest.SetAcosCode(shippingLine, "SL");

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageFlight = "x42";
			voyage.JV_OH_Line = shippingLine.PK;
			voyage.Countries.GetCountry("AU", true).J0_AllocationMethod = AllocationMethodList.Codes.Ignore;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = now.AddDays(1);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_E_ARV = now.AddDays(2);

			voyage.GenerateSailings();

			JobSailing sailing = voyage.Sailings[0];
			sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;

			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_UniqueConsignRef = "S00000100";
			shipment.JS_RL_NKOrigin = load;
			shipment.JS_RL_NKDestination = discharge;
			shipment.JS_JX = sailing.PK;
			shipment.JS_HouseBill = "SINBNE0234";
			shipment.JS_GoodsDescription = "Short description";
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = principal.PK;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			return shipment;
		}

		protected virtual BillOfLadingContainer NewContainerTarget(BillOfLading shipment, string containerNumber)
		{
			OrgHeader emptyReturn = Factory.NewWithValidTestData<OrgHeader>();
			BaseAgencyTest.SetAcosCode(emptyReturn, "CY");

			BillOfLadingContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_OA_ArrivalContainerYardAddress = emptyReturn.MainAddress.PK;
			container.JC_ContainerImportDORelease = "PIN";

			BillOfLadingPackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;
			packline.JL_MarksAndNumbers = "Marks and Numbers.";
			return container;
		}

		void SetUNDG(BillOfLadingPackLine packline, UNDGSubstance[] substances, int index)
		{
			index %= substances.Length + 1;
			if (index == 0)
			{
				packline.UNDGs.DeleteAll();
			}
			else
			{
				var undg = packline.UNDGs.AddNew();
				undg.DI_DG = substances[index - 1].PK;
				if (!undg.DI_NECWeight_ReadOnly)
				{
					undg.DI_DGWeight = 1;
					undg.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;
					undg.DI_NECWeight = 1;
					undg.DI_NECWeightUQ = Core.Constants.Weight.Kilograms;
				}
			}
		}

		U Select<U>(U[] array, int index)
		{
			return array[index % array.Length];
		}

		ZQuery MaxRowsQuery(int max)
		{
			ZQuery result = new ZQuery();
			result.MaximumRows = max;
			return result;
		}

		static OrgHeader FindOrCreatePrincipal(BusinessObjectFactory factory, string code)
		{
			OrgHeader result = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code);
			if (result == null)
			{
				result = factory.NewWithValidTestData<OrgHeader>();
				result.OH_Code = code;
				result.OH_IsShippingProvider = true;
				result.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			}

			return result;
		}

		#endregion
	}
}
