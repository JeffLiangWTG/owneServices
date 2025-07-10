using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentFetchStrategyTest : ShipmentFetchStrategyTest
	{
		public void TestBashFetchForView_JS_JK_ConsolID()
		{
			// JobConsol: 3
			// JobConShipLink: 1
			BashFetchForView(ForwardingShipment.Schema.JS_JK_ConsolID, 4);
		}

		public void TestBashFetchForView_JS_Calc_40RECount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_40RECount", 8);
		}

		public void TestBashFetchForView_JS_Calc_40GPCount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_40GPCount", 8);
		}

		public void TestBashFetchForView_JS_Calc_20RECount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_20RECount", 8);
		}

		public void TestBashFetchForView_JS_Calc_20GPCount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_20GPCount", 8);
		}

		public void TestBashFetchForView_JS_Calc_ContainerCount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_ContainerCount", 8);
		}

		public void TestBashFetchForView_CustomsEntryNumber()
		{
			// CusEntryHeader: 12
			// CusEntryNum: 2
			// CusHAWB: 1
			// CusSCAHouse: 1
			// CusInBondHeader: 1
			// JobDeclaration: 1
			BashFetchForView(ForwardingShipment.Schema.CustomsEntryNumber, 18);
		}

		public void TestBashFetchForView_CustomsEntryNumberType()
		{
			// CusEntryHeader: 12
			// CusEntryNum: 2
			// CusHAWB: 1
			// CusSCAHouse: 1
			// CusInBondHeader: 1
			// JobDeclaration: 1
			BashFetchForView(ForwardingShipment.Schema.CustomsEntryNumberType, 18);
		}

		public void TestBashFetchForView_Job_JS_Status()
		{
			// JobHeader: 1
			BashFetchForView("Job+" + JobHeader.Schema.JH_Status, 1);
		}

		public void TestBashFetchForView_JS_Calc_TEUCount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_TEUCount", 8);
		}

		public void TestBashFetchForView_JS_Calc_OtherContainerCount()
		{
			// JobContainerPackPivot: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			BashFetchForView("JS_Calc_OtherContainerCount", 8);
		}

		public void TestBashFetchForView_JS_JK_VoyageFlight()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			BashFetchForView(ForwardingShipment.Schema.JS_JK_VoyageFlight, 11);
		}

		public void TestBashFetchForView_JS_JK_Vessel()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			BashFetchForView(ForwardingShipment.Schema.JS_JK_Vessel, 11);
		}

		public void TestBashFetchForView_JS_GenericOrderNumbers()
		{
			// JobOrderHeader: 1
			// WhsDocketJobPivot: 1
			BashFetchForView(ForwardingModuleShipment.Schema.JS_GenericOrderNumbers, 2);
		}

		public void TestBashFetchForView_JS_JK_ReceivingAgent()
		{
			// JobConsol: 3
			// JobConShipLink: 1
			BashFetchForView(ForwardingShipment.Schema.JS_JK_ReceivingAgent, 4);
		}

		public void TestBashFetchForView_JS_JK_SendingAgent()
		{
			// JobConsol: 3
			// JobConShipLink: 1
			BashFetchForView(ForwardingShipment.Schema.JS_JK_SendingAgent, 4);
		}

		public void TestBashFetchForView_JS_JK_MasterBillNum()
		{
			// JobConsol: 3
			// JobConShipLink: 1
			BashFetchForView(ForwardingShipment.Schema.JS_JK_MasterBillNum, 4);
		}

		public void TestBashFetchForView_JS_Calc_ImportManifestStatus()
		{
			// CusEntryNum: 1
			BashFetchForView(ForwardingModuleShipment.Schema.JS_Calc_ImportManifestStatus, 1);
		}

		public void TestBashFetchForView_JS_InspectionType()
		{
			// CusEntryNum: 1
			BashFetchForView(ForwardingModuleShipment.Schema.JS_InspectionTypeCode, 1);
		}

		public void TestBashFetchForView_JS_JH_Branch()
		{
			// GlbBranch: 1
			// JobHeader: 1
			BashFetchForView(ForwardingModuleShipment.Schema.JS_JH_Branch, 2);
		}

		public void TestBashFetchForView_JS_JH_Dept()
		{
			// GlbDepartment: 1
			// JobHeader: 1
			BashFetchForView(ForwardingModuleShipment.Schema.JS_JH_Dept, 2);
		}

		public void TestBashFetchForView_JS_OrderReferences()
		{
			// JobDocsAndCartage: 1
			// JobOrderItem: 1
			BashFetchForView(nameof(ForwardingShipment.JS_OrderReferences), 2);
		}

		public void TestBashFetchForView_JS_Calc_DGClass()
		{
			// JobPackLines: 1
			// UNDGDataItems: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DGClass), 2);
		}

		public void TestBashFetchForView_JS_Calc_DGSubstance()
		{
			// JobPackLines: 1
			// UNDGDataItems: 1
			// UNDGSubstancePivot: 1
			// UNDGSubstance: 3

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DGSubstance), 6);
		}

		public void TestBashFetchForView_JS_Calc_DGPSA()
		{
			// UNDGSubstance: 3
			// UNDGReference: 2
			// JobPackLines: 1
			// UNDGDataItem: 1
			// UNDGSubstancePivot: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DGPSA), 8);
		}

		public void TestBashFetchForView_JS_Calc_DIHazardousWasteCode()
		{
			// JobPackLines: 1
			// UNDGDataItem: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DIHazardousWasteCode), 2);
		}

		public void TestBashFetchForView_JS_Calc_DISpecialPermitIssueDate()
		{
			// JobPackLines: 1
			// UNDGDataItem: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DISpecialPermitIssueDate), 2);
		}

		public void TestBashFetchForView_JS_Calc_DISpecialPermitNumner()
		{
			// JobPackLines: 1
			// UNDGDataItem: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DISpecialPermitNumber), 2);
		}

		public void TestBashFetchForView_JS_Calc_DIIsSalvagePackaging()
		{
			// JobPackLines: 1
			// UNDGDataItem: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DIIsSalvagePackaging), 2);
		}

		public void TestBashFetchForView_JS_Calc_DIIsResidueLastContained()
		{
			// JobPackLines: 1
			// UNDGDataItem: 1

			BashFetchForView(nameof(ForwardingShipment.JS_Calc_DIIsResidueLastContained), 2);
		}

		#region Implementation

		static void BashFetchForView(string bindToString, int maxDbHits)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ForwardingModuleShipment[] shipments = factory.Load<ForwardingModuleShipment>(new ZQuery(JobShipmentSchema.PK, CreatePopulatedShipments()));

			factory.ResetDatabaseLoadCount();

			foreach (ForwardingModuleShipment shipment in shipments)
			{
				shipment.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn("", bindToString) });
			}

			foreach (ForwardingModuleShipment shipment in shipments)
			{
				HitProperty(shipment, bindToString);
			}

			AssertMaxDbHits(maxDbHits, factory);
		}

		static void HitProperty(object root, string pathStr)
		{
			string[] path = pathStr.Split(new char[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);

			object o = root;

			for (int i = 0; i < path.Length; i++)
			{
				if (o == null)
				{
					string message = string.Join("+", path, 0, i) + " returned null, you should populate your data better";
					throw new ApplicationException(message);
				}

				o = o.GetType().InvokeMember(path[i], BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, o, Array.Empty<object>());
			}
		}

		static List<ZGuid> CreatePopulatedShipments()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			Type declarationType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var inBondType = ObjectFactory.GetType<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();

			var dgSubs = factory.New<UNDGSubstance>();
			dgSubs.DG_Code = "0000";
			dgSubs.UNDGCountryReferences.DeleteAll();
			var dgSubs2 = factory.New<UNDGSubstance>();
			dgSubs2.DG_Code = "0001";
			dgSubs2.UNDGCountryReferences.DeleteAll();

			var countryRef1 = factory.New<UNDGCountryReference>();
			countryRef1.DCR_Code = "other";
			countryRef1.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			countryRef1.DCR_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			dgSubs.UNDGCountryReferences.Add(countryRef1);
			var countryRef2 = factory.New<UNDGCountryReference>();
			countryRef2.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			countryRef2.DCR_Code = "other1";
			countryRef2.DCR_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			dgSubs.UNDGCountryReferences.Add(countryRef2);
			var countryRef3 = factory.New<UNDGCountryReference>();
			countryRef3.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			countryRef3.DCR_Code = "other2";
			countryRef3.DCR_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			dgSubs.UNDGCountryReferences.Add(countryRef3);
			var countryRef4 = factory.New<UNDGCountryReference>();
			countryRef4.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			countryRef4.DCR_Code = "other3";
			countryRef4.DCR_RN_NKCountry = Core.Constants.CountryCodes.China;
			dgSubs.UNDGCountryReferences.Add(countryRef4);
			var countryRef5 = factory.New<UNDGCountryReference>();
			countryRef5.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			countryRef5.DCR_Code = "other4";
			countryRef5.DCR_RN_NKCountry = Core.Constants.CountryCodes.Japan;
			dgSubs.UNDGCountryReferences.Add(countryRef5);

			for (int i = 0; i < 12; i++)
			{
				var vessel = factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();

				ForwardingConsol consol = factory.New<ForwardingConsol>();

				ForwardingContainer container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TEST4100013";
				container.JC_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				Transport transport = consol.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings[0].PK;

				ForwardingShipment shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "shipment" + i;

				JobHeader job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				PackLine outerPackline = shipment.OuterPackLines.AddNew();
				outerPackline.SetContainer(consol, container);
				var undg = outerPackline.UNDGs.AddNew();
				undg.LinkDefault(dgSubs);
				undg.DI_DG = dgSubs.PK;
				var undg2 = outerPackline.UNDGs.AddNew();
				undg2.LinkDefault(dgSubs2);
				undg2.DI_DG = dgSubs2.PK;

				PackLine innerPackline = shipment.InnerPackLines.AddNew();

				ProcessTask milestone = shipment.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now);

				BusinessObject declaration = factory.NewWithValidTestData(declarationType);
				declaration[JobDeclarationSchema.JE_DeclarationReference] = "decl" + i;
				declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

				var inbondHeader = factory.NewWithValidTestData(inBondType);
				inbondHeader[CusInBondHeaderSchema.BH_ParentID] = shipment.PK;
				inbondHeader[CusInBondHeaderSchema.BH_ParentTableCode] = shipment.TablePrefix;
				inbondHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.InBond;
				inbondHeader[CusInBondHeaderSchema.BH_GB] = GlbBranch.CurrentBranch.PK;

				result.Add(shipment.PK);
			}

			factory.Save();

			return result;
		}

		#endregion
	}
}
