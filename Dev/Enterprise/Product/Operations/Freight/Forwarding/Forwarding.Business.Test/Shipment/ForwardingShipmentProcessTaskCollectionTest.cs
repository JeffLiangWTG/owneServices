using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentProcessTaskCollection))]
	sealed class ForwardingShipmentProcessTaskCollectionTest : RoutingSupportProcessTaskCollectionTest<ForwardingShipmentProcessTaskCollection>
	{
		public void TestCollectionNotLoadedWhenShipmentForwardingFlagIsFalse()
		{
			var creationFactory = new BusinessObjectFactory();

			var shipment = creationFactory.NewWithValidTestData<ForwardingShipment>();
			var collection = new ForwardingShipmentProcessTaskCollection(shipment);

			var milestone = collection.Milestones.AddNew();
			var trigger = collection.Triggers.AddNew();

			AssertContainsExactElementsInAnyOrder("Prerequisite: collection has workflow tasks",
				new[] { milestone, trigger },
				collection.Cast<ProcessTask>());

			creationFactory.Save();

			var factory = new BusinessObjectFactory();
			shipment = factory.Load<ForwardingShipment>(shipment.PK);

			shipment.JS_IsForwardRegistered = false;

			collection = new ForwardingShipmentProcessTaskCollection(shipment);
			collection.Load();

			AssertEquals("Loading of processtasks is suppressed when shipment is not a forwarding one", 0, collection.Count);

			shipment.JS_IsForwardRegistered = true;

			collection = new ForwardingShipmentProcessTaskCollection(shipment);
			collection.Load();

			AssertEquals("Processtasks loaded for forwarding shipment", 2, collection.Count);
		}

		#region OriginCountry / DestinationCountry

		public void TestOriginCountry()
		{
			Shipment.JS_RL_NKOrigin = "MYPKG";
			AssertEquals("MY", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals("MY", Collection.DestinationCountry);
		}

		#endregion

		#region IsCondition1Met

		public void TestIsCondition1Met_ForOriginDifferentFromFirstLoad()
		{
			Consol1.Shipments.Add(Shipment);
			Consol2.Shipments.Add(Shipment);

			Consol1.JK_RL_NKLoadPort = "AUSYD";
			Consol1.JK_RL_NKDischargePort = "MYPKG";
			Consol1.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 1, 1);
			Consol1.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 2, 2);
			Consol2.JK_RL_NKLoadPort = "MYPKG";
			Consol2.JK_RL_NKDischargePort = "SGSIN";
			Consol2.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 3, 3);
			Consol2.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 4, 4);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));

			Shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
		}

		public void TestIsCondition1Met_ForDestinationDifferentFromFinalDischarge()
		{
			Consol1.Shipments.Add(Shipment);
			Consol2.Shipments.Add(Shipment);

			Consol1.JK_RL_NKLoadPort = "AUSYD";
			Consol1.JK_RL_NKDischargePort = "MYPKG";
			Consol1.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 1, 1);
			Consol1.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 2, 2);
			Consol2.JK_RL_NKLoadPort = "MYPKG";
			Consol2.JK_RL_NKDischargePort = "SGSIN";
			Consol2.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2000, 3, 3);
			Consol2.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2000, 4, 4);

			Shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));

			Shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
		}

		public void TestIsCondition1Met_ConsolDischargeLeg()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg));
			Consol1.Shipments.Add(Shipment);
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg));
		}

		public void TestIsCondition1Met_ConsolPreCarriageLeg()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolPreCarriageLeg));
			Consol1.Transports[0].JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			Consol1.Shipments.Add(Shipment);
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.ConsolPreCarriageLeg));
		}

		public void TestIsCondition1Met_ForHasBrokerageAttached()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached));
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = Shipment.PK;
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached));
		}

		public void TestIsCondition1Met_ForHasAirCargoAttached()
		{
			Assert(!Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
			var hAWB = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			hAWB.CS_JS = Shipment.PK;
			Assert(Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
		}

		public void TestIsCondition1Met_ForHasAirCargoAttachedNZ()
		{
			Assert(!Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
			var mAWB = Factory.New<Enterprise.Integration.Customs.NZ.ICusMAWB>();
			var hAWB = Factory.New<Enterprise.Integration.Customs.NZ.ICusHAWB>();
			hAWB.CS_CM = mAWB.PK;
			hAWB.CS_JS = Shipment.PK;
			Assert(!Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.AirCargoAttached));
		}

		public void TestIsCondition1Met_ForCurrentCompanyHandlesPickupCartage()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesPickupCartage));
			Shipment.DocsAndCartage.PickupCartageCoPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesPickupCartage));
		}

		public void TestIsCondition1Met_ForCurrentCompanyHandlesDeliveryCartage()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesDeliveryCartage));
			Shipment.DocsAndCartage.DeliveryCartageCoPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.CurrentCompanyHandlesDeliveryCartage));
		}

		#endregion

		#region IsCondition2Met

		public void TestIsCondition1or2Met_ForImport()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Import, ""));
			Shipment.JS_RL_NKOrigin = "MYPKG";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(true, Shipment.IsImport());
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Import, ""));
		}

		public void TestIsCondition1or2Met_ForExport()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Export, ""));
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Export, ""));
		}

		public void TestIsCondition1or2Met_ForDomestic()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Domestic));
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Domestic, ""));
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort.Substring(0, 2) + "XXX";
			AssertEquals(true, Shipment.IsDomestic());
			AssertEquals(true, Collection.IsCondition1Met(JobShipmentWorkflowCondition1CodeList.Codes.Domestic));
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.Domestic, ""));
		}

		public void TestIsCondition2Met_ForLCL()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.AIR;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.LCL, ""));
		}

		public void TestIsCondition2Met_ForFCL()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.FCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.FCL, ""));
		}

		public void TestIsCondition2Met_ForBuyersConsolMaster()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster, ""));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster, ""));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolMaster, ""));
		}

		public void TestIsCondition2Met_ForBuyersConsolSub()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub, ""));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub, ""));

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.BuyersConsolSub, ""));
		}

		public void TestIsCondition2Met_ForColoadMaster()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster, ""));

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster, ""));

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadMaster, ""));
		}

		public void TestIsCondition2Met_ForColoadSub()
		{
			Shipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
			Shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub, ""));

			Shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ColoadSub, ""));
		}

		public void TestIsCondition2Met_ForAssemblyMaster()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster, ""));

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster, ""));

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblyMaster, ""));
		}

		public void TestIsCondition2Met_ForAssemblySub()
		{
			Shipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
			Shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));

			Shipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));

			Shipment.CoLoadMasterShipment.JS_JS_ColoadMasterShipment = Factory.New<ForwardingShipment>().PK;
			Shipment.CoLoadMasterShipment.CoLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));

			Shipment.CoLoadShipments.AddNew();
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.AssemblySub, ""));
		}

		public void TestIsCondition2Met_ForReleaseType()
		{
			Shipment.JS_ReleaseType = "AAA";
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "AAA"));
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "BBB"));

			Shipment.JS_ReleaseType = "BBB";
			AssertEquals(false, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "AAA"));
			AssertEquals(true, Collection.IsCondition2Met(JobShipmentWorkflowCondition2CodeList.Codes.ReleaseType, "BBB"));
		}

		public void TestIsUserDefinedCondition2Met_WithAdditionaJob()
		{
			var shipment1 = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment1.JS_HouseBill = "GUD";
			var shipmentWorkflow1 = (IWorkflowProvider)shipment1;
			var declaration1 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration1.JE_HouseBill = "GUY";
			declaration1.JE_JS = shipment1.PK;

			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "SHP");
			var task1 = template.WorkflowItems.Tasks.AddNew();

			task1.P9_Description = "TASK";
			var taskTemplateConditions = task1.TemplateConditions;
			taskTemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

			CombineAssertions(() =>
			{
				taskTemplateConditions.TemplateCondition2Value = "\"<JS_HouseBill>\" == \"GUD\"";
				AssertEquals("JS_HouseBill = GUD", true, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<JS_HouseBill>\" == \"\"";
				AssertEquals("JS_HouseBill = ", false, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<JS_HouseBill>\" == \"XX\"";
				AssertEquals("JS_HouseBill = XX", false, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<JE_HouseBill>\" == \"GUY\"";
				AssertEquals("JE_HouseBill = GUY", true, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<JE_HouseBill>\" == \"\"";
				AssertEquals("JE_HouseBill = ", false, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<JE_HouseBill>\" == \"xxx\"";
				AssertEquals("JE_HouseBill = xxx", false, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<_DataSource.JobDeclaration.JE_HouseBill>\" == \"GUY\"";
				AssertEquals("_DataSource.JobDeclaration.JE_HouseBill = GUY", true, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<_DataSource.JobDeclaration.JE_HouseBill>\" == \"\"";
				AssertEquals("_DataSource.JobDeclaration.JE_HouseBill = ", false, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));

				taskTemplateConditions.TemplateCondition2Value = "\"<_DataSource.JobDeclaration.JE_HouseBill>\" == \"XX\"";
				AssertEquals("_DataSource.JobDeclaration.JE_HouseBill = xx", false, shipmentWorkflow1.WorkflowItems.IsCondition2Met(task1));
			});
		}

		#endregion

		#region HasNewConditionBeenMet

		public void TestHasNewConditionBeenMet()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Assert("No declaration, condition must be false", !shipment.WorkflowItems.HasNewConditionBeenMetSinceLastSave());
			BusinessObject declaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration["JE_JS"] = shipment.PK;
			Assert("New declaration, not saved yet, condition must be true", shipment.WorkflowItems.HasNewConditionBeenMetSinceLastSave());
			Factory.Save();
			Assert("Declaration saved, condition must be false", !shipment.WorkflowItems.HasNewConditionBeenMetSinceLastSave());
		}

		#endregion

		public void TestAdditionalFilter()
		{
			var processTaskCollection = new ForwardingShipmentProcessTaskCollectionForTest(Shipment);
			var expectedFilter = new ZQuery(ProcessTasksSchema.P9_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			AssertCollectionContains(expectedFilter, processTaskCollection.AdditionalFilter.GetCompositeParts());
		}

		#region Implementation

		protected override ForwardingShipmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new ForwardingShipmentProcessTaskCollection(Shipment);
		}

		ForwardingConsol Consol1
		{
			get { return consol1 ?? (consol1 = Factory.NewWithValidTestData<ForwardingConsol>()); }
		}
		ForwardingConsol consol1;

		ForwardingConsol Consol2
		{
			get { return consol2 ?? (consol2 = Factory.NewWithValidTestData<ForwardingConsol>()); }
		}
		ForwardingConsol consol2;

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.NewWithValidTestData<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		class ForwardingShipmentProcessTaskCollectionForTest : ForwardingShipmentProcessTaskCollection
		{
			public ForwardingShipmentProcessTaskCollectionForTest(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.AdditionalFilter; }
			}
		}

		#endregion
	}
}
