using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolProcessTaskCollection))]
	sealed class ForwardingConsolProcessTasksCollectionTest : RoutingSupportProcessTaskCollectionTest<ForwardingConsolProcessTaskCollection>
	{
		public void TestCollectionNotLoadedWhenConsolForwardingFlagIsFalse()
		{
			var creationFactory = new BusinessObjectFactory();

			var consol = creationFactory.NewWithValidTestData<ForwardingConsol>();
			var collection = new ForwardingConsolProcessTaskCollection(consol);

			var milestone = collection.Milestones.AddNew();
			var trigger = collection.Triggers.AddNew();

			AssertContainsExactElementsInAnyOrder("Prerequisite: collection has workflow tasks",
				new[] { milestone, trigger },
				collection.Cast<ProcessTask>());

			creationFactory.Save();

			var factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			consol.JK_IsForwarding = false;

			collection = new ForwardingConsolProcessTaskCollection(consol);
			collection.Load();

			AssertEquals("Loading of processtasks is suppressed when consol is not a forwarding one", 0, collection.Count);

			consol.JK_IsForwarding = true;

			collection = new ForwardingConsolProcessTaskCollection(consol);
			collection.Load();

			AssertEquals("Processtasks loaded for forwarding consol", 2, collection.Count);
		}

		#region OriginCountry / DestinationCountry

		public void TestOriginCountry()
		{
			Consol.JK_RL_NKLoadPort = "MYPKG";
			AssertEquals("MY", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			Consol.JK_RL_NKDischargePort = "MYPKG";
			AssertEquals("MY", Collection.DestinationCountry);
		}

		#endregion

		#region IsConditionMet

		public void TestIsCondition1or2Met_ForImport()
		{
			Consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Consol.JK_RL_NKDischargePort = "MYPKG";
			AssertEquals(false, Consol.IsImport());
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(false, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.Import, ""));

			Consol.JK_RL_NKLoadPort = "MYPKG";
			Consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(true, Consol.IsImport());
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(true, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.Import, ""));
		}

		public void TestIsCondition1or2Met_ForExport()
		{
			Consol.JK_RL_NKLoadPort = "MYPKG";
			Consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(false, Consol.IsExport());
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(false, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.Export, ""));

			Consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Consol.JK_RL_NKDischargePort = "MYPKG";
			AssertEquals(true, Consol.IsExport());
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(true, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.Export, ""));
		}

		public void TestIsCondition1Met_ForMainTransport()
		{
			AssertEquals("MainTransport condition is always met. It is used to determine which leg to attach to.", true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.MainTransport));
		}

		public void TestIsCondition1Met_ForHas2ndTransport()
		{
			Transport1.JW_RL_NKLoadPort = "SGSIN";
			Transport1.JW_RL_NKDiscPort = "MYPKG";
			Transport2.JW_RL_NKLoadPort = "MYPKG";
			Transport2.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg));

			Transport3.JW_RL_NKLoadPort = "AUMEL";
			Transport3.JW_RL_NKDiscPort = "AUSYD";
			Transport4.JW_RL_NKLoadPort = "AUSYD";
			Transport4.JW_RL_NKDiscPort = "USLAX";
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg));
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
		}

		public void TestIsCondition1Met_ForHas3ndTransport()
		{
			Transport1.JW_RL_NKLoadPort = "SGSIN";
			Transport1.JW_RL_NKDiscPort = "MYPKG";
			Transport2.JW_RL_NKLoadPort = "MYPKG";
			Transport2.JW_RL_NKDiscPort = "AUMEL";
			Transport3.JW_RL_NKLoadPort = "AUMEL";
			Transport3.JW_RL_NKDiscPort = "AUBNE";
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg));
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));

			Transport4.JW_RL_NKLoadPort = "AUBNE";
			Transport4.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg));
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
		}

		public void TestIsCondition1Met_ForHas4thTransport()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KZAAU";
			consol.JK_RL_NKDischargePort = "AUMEL";

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "KZAAU";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ETD = new ZDateTime(2011, 5, 10);
			transport1.JW_ETD = new ZDateTime(2011, 5, 11);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "MYPKG";
			transport2.JW_ETD = new ZDateTime(2011, 5, 12);
			transport2.JW_ETD = new ZDateTime(2011, 5, 13);

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "MYPKG";
			transport3.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_ETD = new ZDateTime(2011, 5, 14);
			transport2.JW_ETD = new ZDateTime(2011, 5, 15);

			ForwardingConsolProcessTaskCollection collection = new ForwardingConsolProcessTaskCollection(consol);

			AssertEquals(true, collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
			AssertEquals(false, collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg));

			consol.JK_RL_NKDischargePort = "AUSYD";

			Transport transport4 = consol.Transports.AddNew();
			transport4.JW_RL_NKLoadPort = "AUMEL";
			transport4.JW_RL_NKDiscPort = "AUSYD";
			transport4.JW_ETD = new ZDateTime(2011, 5, 16);
			transport4.JW_ETD = new ZDateTime(2011, 5, 17);

			AssertEquals(true, collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
			AssertEquals(true, collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg));
		}

		public void TestIsCondition2Met_ForLCL()
		{
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.ContainerModes.LCL, ""));
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.ContainerModes.LCL, ""));
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(false, Collection.IsCondition2Met(Core.Constants.ContainerModes.LCL, ""));
		}

		public void TestIsCondition2Met_ForFCL()
		{
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.ContainerModes.FCL, ""));
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(Core.Constants.ContainerModes.FCL, ""));
		}

		public void TestIsCondition2Met_ForReleaseType()
		{
			Consol.JK_ReleaseType = "AAA";
			AssertEquals(true, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.ReleaseType, "AAA"));
			AssertEquals(false, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.ReleaseType, "BBB"));

			Consol.JK_ReleaseType = "BBB";
			AssertEquals(false, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.ReleaseType, "AAA"));
			AssertEquals(true, Collection.IsCondition2Met(JobConsolWorkflowCondition2CodeList.Codes.ReleaseType, "BBB"));
		}

		#endregion

		#region Defaulting

		public void TestDefaultingTriggerCondition_ComplexTest()
		{
			var has2ndLeg = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			var has3rdLeg = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			var has4thLeg = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			var someCondition = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;
			var rfpCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			Consol.Transports.RemoveAndDeleteAll();

			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);

			Consol.Transports.New(from: "AUSYD", to: "USLAX");
			Consol.Transports.New(from: "USLAX", to: "AUMEL");
			Consol.Transports.New(from: "AUMEL", to: "USNYC");
			Consol.Transports.New(from: "USNYC", to: "UAIEV");
			Consol.Transports.New(from: "UAIEV", to: "NZAKL");

			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, ZString.Empty, rfpCondition, "LOC=<FirstLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<SecondLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<ThirdLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.DepartureCode, ZString.Empty, has4thLeg, rfpCondition, "LOC=<FourthLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.DepartureCode, someCondition, ZString.Empty, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.DepartureCode, someCondition, has2ndLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.DepartureCode, someCondition, has3rdLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.DepartureCode, someCondition, has4thLeg, someCondition, ZString.Empty);

			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, ZString.Empty, rfpCondition, "LOC=<FirstLeg.Origin>");
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<SecondLeg.Origin>");
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<ThirdLeg.Origin>");
			EnsureProcessTaskCreated(Events.CutOffDateCode, ZString.Empty, has4thLeg, rfpCondition, "LOC=<FourthLeg.Origin>");
			EnsureProcessTaskCreated(Events.CutOffDateCode, someCondition, ZString.Empty, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, someCondition, has2ndLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, someCondition, has3rdLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.CutOffDateCode, someCondition, has4thLeg, someCondition, ZString.Empty);

			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<FirstLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<SecondLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, has4thLeg, rfpCondition, "LOC=<ThirdLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ArrivalCode, ZString.Empty, ZString.Empty, rfpCondition, "LOC=<LastLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ArrivalCode, someCondition, has2ndLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, someCondition, has3rdLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, someCondition, has4thLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.ArrivalCode, someCondition, ZString.Empty, someCondition, ZString.Empty);

			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<FirstLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<SecondLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, has4thLeg, rfpCondition, "LOC=<ThirdLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.StorageCommencedCode, ZString.Empty, ZString.Empty, rfpCondition, "LOC=<LastLeg.Destination>,FAC=CTO");
			EnsureProcessTaskCreated(Events.StorageCommencedCode, someCondition, has2ndLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, someCondition, has3rdLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, someCondition, has4thLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.StorageCommencedCode, someCondition, ZString.Empty, someCondition, ZString.Empty);

			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, ZString.Empty, rfpCondition, "LOC=<FirstLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<SecondLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<ThirdLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, ZString.Empty, has4thLeg, rfpCondition, "LOC=<FourthLeg.Origin>,FAC=CTO");
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, someCondition, ZString.Empty, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, someCondition, has2ndLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, someCondition, has3rdLeg, someCondition, ZString.Empty);
			EnsureProcessTaskCreated(Events.ReceiptCommencedCode, someCondition, has4thLeg, someCondition, ZString.Empty);
		}

		void EnsureProcessTaskCreated(ZString evnt, ZString templateTriggerCondition, ZString templateCreateCondition, ZString processTaskTriggerCondition, ZString expectedConditionValue)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TemplateConditions.TemplateCondition1 = templateCreateCondition;
			templateMilestone.TriggerConditions.TriggerCondition = templateTriggerCondition;
			templateMilestone.TriggerConditions.TriggerEventCode = evnt;

			var view = new MilestoneCollectionView(Collection);

			Collection.RemoveAndDeleteAll();
			view.CreateItemsFromTemplate_ForTest(new[] { new TemplateItemApplication(template, new[] { templateMilestone }) });

			var processTask = Collection.FirstOrDefault() as ProcessTask;

			AssertNotNull("Created process task", processTask);
			AssertEquals("P9_TriggerCondition", processTaskTriggerCondition, processTask.P9_TriggerCondition);
			AssertEquals("P9_TriggerConditionValue", expectedConditionValue, processTask.P9_TriggerConditionValue);
		}

		#endregion

		#region Implementation

		protected override ForwardingConsolProcessTaskCollection GetCollectionToTestCore()
		{
			return new ForwardingConsolProcessTaskCollection(Consol);
		}

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
					consol.JK_RL_NKLoadPort = "MYPKG";
					consol.JK_RL_NKDischargePort = "AUSYD";
				}
				return consol;
			}
		}
		ForwardingConsol consol;

		Transport Transport1
		{
			get
			{
				if (transport1 == null)
				{
					transport1 = Consol.Transports[0];
					transport1.JW_ETD = new ZDateTime(2000, 1, 1);
					transport1.JW_ETA = new ZDateTime(2000, 2, 2);
				}
				return transport1;
			}
		}
		Transport transport1;

		Transport Transport2
		{
			get
			{
				if (transport2 == null)
				{
					transport2 = Consol.Transports.AddNew();
					transport2.JW_ETD = new ZDateTime(2000, 3, 3);
					transport2.JW_ETA = new ZDateTime(2000, 4, 4);
				}
				return transport2;
			}
		}
		Transport transport2;

		Transport Transport3
		{
			get
			{
				if (transport3 == null)
				{
					transport3 = Consol.Transports.AddNew();
					transport3.JW_ETD = new ZDateTime(2000, 5, 5);
					transport3.JW_ETA = new ZDateTime(2000, 6, 6);
				}
				return transport3;
			}
		}
		Transport transport3;

		Transport Transport4
		{
			get
			{
				if (transport4 == null)
				{
					transport4 = Consol.Transports.AddNew();
					transport4.JW_ETD = new ZDateTime(2000, 7, 7);
					transport4.JW_ETA = new ZDateTime(2000, 8, 8);
				}
				return transport4;
			}
		}
		Transport transport4;

		#endregion
	}
}
