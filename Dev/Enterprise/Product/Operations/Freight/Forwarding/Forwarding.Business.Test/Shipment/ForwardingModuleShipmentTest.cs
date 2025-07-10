using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupporter.DocumentSupporterHelper;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingModuleShipment))]
	sealed class ForwardingModuleShipmentTest : ShipmentBusinessObjectTest
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonShipment);
			}
		}

		#endregion

		public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			// TODO: This should be undo when when Enterprise.Integration.Customs.AU.ICustomsManifestLineSequence is changed to make JobConsol it's parent; there is a requirement that a CustomsManifestLineSequence record still exists after ForwardingShipment is deleted and this record is can be linked back JobConsol
			Assert(true);
		}

		public void TestConsolFields()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MASTERBILL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "VOYAGE";
			transport.JW_Vessel = "VESSEL NAME";
			Factory.Save();

			Shipment.Consols.Add(consol);
			AssertEquals("Consol ID", consol.JK_UniqueConsignRef, Shipment.JS_JK_ConsolID);
			AssertEquals("MasterBill", consol.JK_MasterBillNum, Shipment.JS_JK_MasterBillNum);
			AssertEquals("Voyage", consol.JK_JX_JV_VoyageFlight, Shipment.JS_JK_VoyageFlight);
			AssertEquals("Vessel", consol.JK_JX_JV_NKVessel, Shipment.JS_JK_Vessel);
		}

		public void TestJobHeaderFields()
		{
			Shipment.JS_UniqueConsignRef = "ShipmentID";
			Factory.Save();
			AssertEquals("No JobHeader Branch.", "", Shipment.JS_JH_Branch);
			AssertEquals("No JobHeader Dept.", "", Shipment.JS_JH_Dept);

			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = Shipment.PK;
			jobHeader.JH_JobNum = "S00001000";
			Factory.Save();

			AssertEquals("JobHeader Branch.", GlbBranch.CurrentBranch.GB_Code, Shipment.JS_JH_Branch);
			AssertEquals("JobHeader Dept.", GlbDepartment.CurrentDepartment.GE_Code, Shipment.JS_JH_Dept);
		}

		public void TestImportManifestStatusForSeaCargoWithClearStatus()
		{
			Shipment.JS_TransportMode = Constants.TransportModes.Sea;
			BusinessObject oceanBill = (BusinessObject)Factory.New<AU.ICusSCAOceanBill>();
			BusinessObject houseBill = (BusinessObject)Factory.New<AU.ICusSCAHouse>();
			houseBill[CusSCAHouseSchema.CA_CB.Name] = oceanBill.PK;
			houseBill[CusSCAHouseSchema.CA_JS.Name] = Shipment.PK;
			houseBill[CusSCAHouseSchema.CA_ShipmentStatus.Name] = "ACK";

			AssertEquals("Sea Cargo Status", "Acknowledged.", Shipment.JS_Calc_ImportManifestStatus);
		}

		public void TestImportManifestStatusForAirCargoWithClearStatus()
		{
			CusEntryNumber manifestStatus = Factory.New<CusEntryNumber>();
			manifestStatus.CE_EntryNum = "C000";
			manifestStatus.CE_EntryType = ForwardingModuleShipment.EntryTypeForImportManifestStatus;
			manifestStatus.CE_ParentID = Shipment.PK;
			manifestStatus.CE_ParentTable = CommonShipment.Schema.TableName;
			manifestStatus.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			manifestStatus.CE_EntryStatus = "CLD";

			AssertEquals("Entry status", "C000:Cleared", Shipment.JS_Calc_ImportManifestStatus);
		}

		public void TestImportManifestStatusForAirCargoWithWaitingStatus()
		{
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			BusinessObject mAWB = (BusinessObject)Factory.New<AU.ICusMAWB>();
			BusinessObject cusHAWB = (BusinessObject)Factory.New<AU.ICusHAWB>();
			cusHAWB[ZArchitecture.Schema.CusHAWBSchema.CS_CM] = mAWB.PK;
			cusHAWB[ZArchitecture.Schema.CusHAWBSchema.CS_IsResponsePending.Name] = true;
			cusHAWB[ZArchitecture.Schema.CusHAWBSchema.CS_JS.Name] = Shipment.PK;

			AddCusEntryNum(Shipment.PK, "C000", ForwardingModuleShipment.EntryTypeForImportManifestStatus, "CLD");
			AssertEquals("Entry status", "WAIT:Impeded", Shipment.JS_Calc_ImportManifestStatus);
		}

		public void TestShowCusEntryInformation()
		{
			ForwardingModuleShipment shipment = Factory.New<ForwardingModuleShipment>();
			AssertEquals("Cus Entry Number does not exist - EntryNum Value Blank", "", shipment.EntryNum);
			AssertEquals("Cus Entry Number does not exist - EntryNumberType Value Blank", "", shipment.EntryNumberType);
			AssertEquals("Cus Entry Number does not exist - EntryNumberStatus Value Blank", "", shipment.EntryNumberStatus);
			ForwardingModuleShipment shipmentWithEntryNum = Factory.New<ForwardingModuleShipment>();
			AddCusEntryNum(shipmentWithEntryNum.PK, TestEntryNum, TestEntryNumberType, TestEntryNumberStatus);
			Factory.Save();
			AssertEquals("Cus Entry Number does not exist - EntryNum Value Blank", TestEntryNum, shipmentWithEntryNum.EntryNum);
			AssertEquals("Cus Entry Number does not exist - EntryNumberType Value Blank", TestEntryNumberType, shipmentWithEntryNum.EntryNumberType);
			AssertEquals("Cus Entry Number does not exist - EntryNumberStatus Value Blank", TestEntryNumberStatus, shipmentWithEntryNum.EntryNumberStatus);
		}

		public void TestCustomsBroker()
		{
			ForwardingModuleShipment shipment = Factory.NewWithValidTestData<ForwardingModuleShipment>();
			GlbStaff staff = Factory.New<GlbStaff>();
			BusinessObject jobDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			staff.GS_Code = "TST";
			jobDeclaration[JobDeclarationSchema.JE_GS_NKCusAgent] = "TST";
			jobDeclaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			Factory.Save();

			AssertEquals("TST", shipment.CustomsBroker);
		}

		public void TestCustomsBrokerDependOnCountry()
		{
			ForwardingModuleShipment shipment = Factory.NewWithValidTestData<ForwardingModuleShipment>();

			GlbCompany otherCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany1.GC_Code = "!AU";
			otherCompany1.SetCountry(Core.Constants.CountryCodes.Australia);
			GlbBranch otherCompanyBranch1 = Factory.New<GlbBranch>();
			otherCompanyBranch1.GB_GC = otherCompany1.PK;
			otherCompanyBranch1.GB_Code = "!AU";
			otherCompanyBranch1.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var pk1 = otherCompanyBranch1.PK;
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			BusinessObject jobDeclaration1 = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			staff1.GS_Code = "TS1";
			staff1.GS_GB_HomeBranch = pk1;
			staff1.GS_LoginName = "!AU";
			jobDeclaration1[JobDeclarationSchema.JE_GB] = pk1;
			jobDeclaration1[JobDeclarationSchema.JE_GS_NKCusAgent] = "TS1";
			jobDeclaration1[JobDeclarationSchema.JE_JS] = shipment.PK;

			GlbCompany otherCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany2.GC_Code = "!US";
			otherCompany2.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			GlbBranch otherCompanyBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			otherCompanyBranch2.GB_GC = otherCompany2.PK;
			otherCompanyBranch2.GB_Code = "!US";
			otherCompanyBranch2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var pk2 = otherCompanyBranch2.PK;
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			BusinessObject jobDeclaration2 = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			staff2.GS_Code = "TS2";
			staff2.GS_GB_HomeBranch = pk2;
			staff2.GS_LoginName = "!US";
			jobDeclaration2[JobDeclarationSchema.JE_GB] = pk2;
			jobDeclaration2[JobDeclarationSchema.JE_GS_NKCusAgent] = "TS2";
			jobDeclaration2[JobDeclarationSchema.JE_JS] = shipment.PK;

			GlbCompany otherCompany3 = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany3.GC_Code = "!AF";
			otherCompany3.SetCountry(Core.Constants.CountryCodes.Afghanistan);
			GlbBranch otherCompanyBranch3 = Factory.NewWithValidTestData<GlbBranch>();
			otherCompanyBranch3.GB_GC = otherCompany3.PK;
			otherCompanyBranch3.GB_Code = "!AF";
			otherCompanyBranch3.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Afghanistan;
			var pk3 = otherCompanyBranch3.PK;
			GlbStaff staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "TS3";
			staff3.GS_GB_HomeBranch = pk3;
			staff3.GS_LoginName = "!AF";

			Factory.Save();
			Factory.ClearQueryCache();
			var shipmengtpkoriginPK = shipment.PK;

			using (new Environment.TemporaryUserContext { BranchPK = otherCompanyBranch1.PK.ToGuid() }.Set())
			{
				var shipmentau = Factory.CreateNewFactory().Load<ForwardingModuleShipment>(shipmengtpkoriginPK);
				AssertEquals("TS1", (string)shipmentau.CustomsBroker);
			}

			using (new Environment.TemporaryUserContext { BranchPK = otherCompanyBranch2.PK.ToGuid() }.Set())
			{
				var shipmentus = Factory.CreateNewFactory().Load<ForwardingModuleShipment>(shipmengtpkoriginPK);
				AssertEquals("TS2", (string)shipmentus.CustomsBroker);
			}

			using (new Environment.TemporaryUserContext { BranchPK = otherCompanyBranch3.PK.ToGuid() }.Set())
			{
				var shipmentother = Factory.CreateNewFactory().Load<ForwardingModuleShipment>(shipmengtpkoriginPK);
				AssertEquals("TS1,TS2", (string)shipmentother.CustomsBroker);
			}
		}

		public void TestPackLineTotals()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			ForwardingContainer container = consol.Containers.AddNew();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolume = 2;
			packLine.JL_ActualWeight = 20;
			packLine.JL_PackageCount = 4;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingModuleShipment moduleShipment = factory2.Load<ForwardingModuleShipment>(shipment.PK);
			AssertEquals(2, (int)moduleShipment.Containers.First().GetTotalVolumeByShipment(moduleShipment));
			AssertEquals(20, (int)moduleShipment.Containers.First().GetTotalWeightByShipment(moduleShipment));
			AssertEquals(4, (int)moduleShipment.Containers.First().GetTotalPackagesByShipment(moduleShipment));
		}

		public void TestGenericOrderNumber()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER";

			var shipment = Factory.New<ForwardingShipment>();
			var moduleShipment = Factory.Load<ForwardingModuleShipment>(shipment.PK);
			AssertEquals("No orders should be attached yet", 0, shipment.GenericOrders.Count);

			shipment.AttachedOrders.AddNew().JD_OrderNumber = "ORD1";
			shipment.AttachedOrders[0].BuyerPK = buyer.PK;
			AssertEquals("Shipment order # should contain order number", "ORD1", moduleShipment.JS_GenericOrderNumbers);

			shipment.AttachedOrders.AddNew().JD_OrderNumber = "ORD2";
			shipment.AttachedOrders[1].BuyerPK = buyer.PK;
			AssertEquals("Shipment order # should contain both order numbers", "ORD1, ORD2", moduleShipment.JS_GenericOrderNumbers);

			IAttachedOrder whsOrder1 = (IAttachedOrder)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsOrder>());
			shipment.AttachedWarehouseOrders.Add(whsOrder1);
			Factory.Save();
			AssertEquals("Shipment order # should contain order and warehouse order numbers", "ORD1, ORD2, " + whsOrder1.JobNo, moduleShipment.JS_GenericOrderNumbers);

			shipment.AttachedOrders.AddNew().JD_OrderNumber = "ORD3";
			shipment.AttachedOrders[2].BuyerPK = buyer.PK;
			AssertEquals("All orders are still attached to shipment", shipment.GenericOrders.Count, 4);
			AssertEquals("Shipment order # should be capped at three with orders and warehouse orders come after orders", "ORD1, ORD2, ORD3...", moduleShipment.JS_GenericOrderNumbers);

			IAttachedOrder whsOrder2 = (IAttachedOrder)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsOrder>());
			shipment.AttachedWarehouseOrders.Add(whsOrder2);
			shipment.AttachedOrders[1].Delete();
			Factory.Save();

			AssertEquals(shipment.GenericOrders.Count, 4);
			AssertEquals("An order should be removed and a warehouse ordered added", "ORD1, ORD3, " + whsOrder1.JobNo + "...", moduleShipment.JS_GenericOrderNumbers);

			shipment.AttachedOrders.DeleteAll();
			AssertEquals("All purchase orders have been removed", shipment.GenericOrders.Count, 2);
			AssertEquals(whsOrder1.JobNo + ", " + whsOrder2.JobNo, moduleShipment.JS_GenericOrderNumbers);

			shipment.AttachedWarehouseOrders.Remove(whsOrder1);
			Factory.Save();
			AssertEquals(shipment.GenericOrders.Count, 1);
			AssertEquals(whsOrder2.JobNo, moduleShipment.JS_GenericOrderNumbers);

			shipment.AttachedWarehouseOrders.Remove(whsOrder2);
			Factory.Save();
			AssertEquals("All orders should be removed", shipment.GenericOrders.Count, 0);
			AssertEquals(ZString.Empty, moduleShipment.JS_GenericOrderNumbers);
		}

		class TestProcessTaskCollectionFactory : IProcessTaskCollectionFactory
		{
			readonly IProcessTaskCollectionFactory factory;
			public int CreationCount { get; private set; }

			public TestProcessTaskCollectionFactory()
			{
				factory = ProcessTaskCollectionFactoryFactory.NewInstance;
			}

			public TProcessTaskCollection GetOrCreate<TProcessTaskCollection>(ZGuid parentPK, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc) where TProcessTaskCollection : ProcessTaskCollection
			{
				CreationCount++;
				return factory.GetOrCreate(parentPK, creatorFunc);
			}

			public TProcessTaskCollection GetOrCreate<TProcessTaskCollection, TAdditionalUniquenessConstraint>(ZGuid parentPK, TAdditionalUniquenessConstraint key, ProcessTaskCollectionCreator<TProcessTaskCollection> creatorFunc)
				where TProcessTaskCollection : ProcessTaskCollection
				where TAdditionalUniquenessConstraint : IEquatable<TAdditionalUniquenessConstraint>
			{
				CreationCount++;
				return factory.GetOrCreate(parentPK, key, creatorFunc);
			}
		}

		public void TestProcessTaskCollectionSharedInWorkflow()
		{
			var testFactory = new TestProcessTaskCollectionFactory();
			var shipment = Factory.NewWithValidTestData<ForwardingModuleShipment>();
			Factory.ServiceContainer.AddService<IProcessTaskCollectionFactory>(testFactory);
			var trigger = shipment.WorkflowItems.Triggers.AddNew();
			Factory.Save();
			AssertEquals("Template application, and the one shipment type.", 2, testFactory.CreationCount);
		}

		public void TestProcessTaskCollectionNotSharedWithDifferentJobs()
		{
			var modShipment = Factory.NewWithValidTestData<ForwardingModuleShipment>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertNotEquals(modShipment.WorkflowItems, shipment.WorkflowItems);
		}

		public void TestProcessTaskCollectionSharedWithSameJob()
		{
			var modShipment = Factory.NewWithValidTestData<ForwardingModuleShipment>();
			var shipment = Factory.Load<ForwardingShipment>(modShipment.PK);
			AssertEquals(modShipment.WorkflowItems, shipment.WorkflowItems);
		}

		public void TestProcessTaskCollectionSharedWithSameJob_OneLoad()
		{
			var modShipment = Factory.NewWithValidTestData<ForwardingModuleShipment>();
			var rebuildCount = 0;
			modShipment.WorkflowItems.Tasks.AddNew();
			modShipment.WorkflowItems.TriggersIncludingRelated.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var s1 = newFactory.Load<ForwardingModuleShipment>(modShipment.PK);
			var s2 = newFactory.Load<ForwardingShipment>(modShipment.PK);
			s1.WorkflowItems.CountChanged += (s, e) => rebuildCount++;
			AssertEquals(s1.WorkflowItems, s2.WorkflowItems);
			AssertEquals(0, rebuildCount);
		}

		public void TestITopLevelBusinessEntityForDocSup()
		{
			var moduleShipment = Factory.New<ForwardingModuleShipment>() as ITopLevelBusinessEntityForDocSup;
			AssertNotNull(moduleShipment);

			var topLevelBusinessEntity = moduleShipment.TopLevelBusinessEntity;
			Assert("Top level business object type should be ForwardingShipment.", topLevelBusinessEntity.FullName.EndsWith(nameof(ForwardingShipment)));
		}

		public new void TestGetDeclarationFor()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "!1#";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "#1@";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "!2#";
			company2.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "#2@";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration1 = Factory.New<IBaseJobDeclaration>();
			declaration1.JE_GB = branch1.PK;
			declaration1.JE_JS = Shipment.PK;
			declaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			var declaration2 = Factory.New<IBaseJobDeclaration>();
			declaration2.JE_GB = branch2.PK;
			declaration2.JE_JS = Shipment.PK;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<CommonShipment>(Shipment.PK);
			AssertNull("No dec with same company for this shipment", shipmentInNewFactory.GetDeclarationFor(GlbCompany.CurrentCompany.PK));
			var declaration = shipmentInNewFactory.GetDeclarationFor(company2.PK);
			AssertEquals("Should have matched to declaration2 as it was created earlier", declaration2.PK, declaration.PK);

			var declaration3 = Factory.New<IBaseJobDeclaration>();
			declaration3.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration3.JE_JS = Shipment.PK;
			declaration3.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);
			Factory.Save();
			declaration = shipmentInNewFactory.GetDeclarationFor(GlbCompany.CurrentCompany.PK);
			AssertEquals("Should have matched to declaration3 as it was created earlier", declaration3.PK, declaration.PK);

			AssertEquals("Should not have created the shipment a second time", Shipment, Shipment.Factory.Load<CommonShipment>(Shipment.PK));
			AssertEquals("Should not have created the shipment a second time", 1, Shipment.Factory.GetBizOsForDataRow(((INeedRow)Shipment).Row).Length);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ForwardingModuleShipment>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<ForwardingModuleShipment>();
		}

		void AddCusEntryNum(ZGuid parentID, ZString entryNum, ZString entryNumberType, ZString entryNumberStatus)
		{
			CusEntryNumber cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_EntryIsSystemGenerated = false;
			cusEntryNum.CE_EntryNum = entryNum;
			cusEntryNum.CE_EntryType = entryNumberType;
			cusEntryNum.CE_ParentID = parentID;
			cusEntryNum.CE_ParentTable = CommonShipment.Schema.TableName;
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusEntryNum.CE_EntryStatus = entryNumberStatus;
		}

		public new ForwardingModuleShipment Shipment;

		const string TestEntryNum = "1M2928109029";
		const string TestEntryNumberType = "ECN";
		const string TestEntryNumberStatus = "CLR";

		#endregion
	}
}
