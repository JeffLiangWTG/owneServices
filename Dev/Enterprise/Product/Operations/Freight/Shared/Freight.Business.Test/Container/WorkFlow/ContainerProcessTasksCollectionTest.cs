using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerProcessTaskCollection))]
	sealed class ContainerProcessTasksCollectionTest : ProcessTaskCollectionTest<ContainerProcessTaskCollection>
	{
		#region OriginCountry / DestinationCountry

		public void TestOriginAndDestinationCountry()
		{
			Consol1.Containers.Add(Container);
			Consol1.JK_RL_NKDischargePort = "AUSYD";
			Consol1.JK_RL_NKLoadPort = "NZAKL";
			AssertEquals(Consol1.LoadPort.RL_RN_NKCountryCode, Collection.OriginCountry);
			AssertEquals(Consol1.DischargePort.RL_RN_NKCountryCode, Collection.DestinationCountry);

			Consol1.Containers.Remove(Container);

			BusinessObject cusContainer = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseCusContainer)));
			cusContainer[CusContainerSchema.CO_JC] = Container.PK;
			JobDeclaration[JobDeclarationSchema.JE_RL_NKOrigin] = "AUSYD";
			JobDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = "NZAKL";
			cusContainer[CusContainerSchema.CO_JE] = JobDeclaration[JobDeclarationSchema.PK];

			AssertEquals("Origin Country", "AU", Collection.OriginCountry);
			AssertEquals("Destination Country", "NZ", Collection.DestinationCountry);
		}

		#endregion

		#region IsConditionMet

		public void TestIsCondition1Met_NoContainerParent()
		{
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg));
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg));
		}

		public void TestIsCondition1Met_ForHas2ndTransport()
		{
			Consol1.JK_RL_NKLoadPort = "MYPKG";
			Consol1.JK_RL_NKDischargePort = "AUSYD";
			Consol1.Containers.Add(Container);

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
			Consol1.JK_RL_NKLoadPort = "MYPKG";
			Consol1.JK_RL_NKDischargePort = "AUSYD";
			Consol1.Containers.Add(Container);

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
			Consol1.JK_RL_NKLoadPort = "KZAAU";
			Consol1.JK_RL_NKDischargePort = "AUMEL";
			Consol1.Containers.Add(Container);

			Transport1.JW_RL_NKLoadPort = "KZAAU";
			Transport1.JW_RL_NKDiscPort = "SGSIN";
			Transport2.JW_RL_NKLoadPort = "SGSIN";
			Transport2.JW_RL_NKDiscPort = "MYPKG";
			Transport3.JW_RL_NKLoadPort = "MYPKG";
			Transport3.JW_RL_NKDiscPort = "AUMEL";

			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
			AssertEquals(false, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg));

			Consol1.JK_RL_NKDischargePort = "AUSYD";

			Transport4.JW_RL_NKLoadPort = "AUMEL";
			Transport4.JW_RL_NKDiscPort = "AUSYD";

			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg));
			AssertEquals(true, Collection.IsCondition1Met(JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg));
		}

		#endregion

		#region Defaulting

		public void TestContainer_DefaultingTriggerCondition()
		{
			var has2ndLeg = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			var has3rdLeg = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			var has4thLeg = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			var someCondition = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;
			var rfpCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var departureRelatedEvents = new[]
			{
				Events.DepartureCode,
				Events.FreightLoadedCode,
				Events.GateInCode
			};

			var arrivalRelatedEvents = new[]
			{
				Events.ArrivalCode,
				Events.FreightUnloadedCode,
				Events.GateOutCode
			};

			Consol1.JK_RL_NKLoadPort = "AUSYD";
			Consol1.JK_RL_NKDischargePort = "NZAKL";
			Consol1.Transports.RemoveAndDeleteAll();
			Consol1.Containers.Add(Container);

			foreach (var departureRelatedEvent in departureRelatedEvents)
			{
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);
			}

			foreach (var arrivalRelatedEvent in arrivalRelatedEvents)
			{
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, has2ndLeg, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, has3rdLeg, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, has4thLeg, ZString.Empty, ZString.Empty);
			}

			Consol1.Transports.New(from: "AUSYD", to: "USLAX");
			Consol1.Transports.New(from: "USLAX", to: "AUMEL");
			Consol1.Transports.New(from: "AUMEL", to: "USNYC");
			Consol1.Transports.New(from: "USNYC", to: "UAIEV");
			Consol1.Transports.New(from: "UAIEV", to: "NZAKL");

			foreach (var departureRelatedEvent in departureRelatedEvents)
			{
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<SecondLeg.Origin>,FAC=CTO");
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<ThirdLeg.Origin>,FAC=CTO");
				EnsureProcessTaskCreated(departureRelatedEvent, ZString.Empty, has4thLeg, rfpCondition, "LOC=<FourthLeg.Origin>,FAC=CTO");
				EnsureProcessTaskCreated(departureRelatedEvent, someCondition, ZString.Empty, someCondition, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, someCondition, has2ndLeg, someCondition, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, someCondition, has3rdLeg, someCondition, ZString.Empty);
				EnsureProcessTaskCreated(departureRelatedEvent, someCondition, has4thLeg, someCondition, ZString.Empty);
			}

			foreach (var arrivalRelatedEvent in arrivalRelatedEvents)
			{
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, has2ndLeg, rfpCondition, "LOC=<FirstLeg.Destination>,FAC=CTO");
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, has3rdLeg, rfpCondition, "LOC=<SecondLeg.Destination>,FAC=CTO");
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, has4thLeg, rfpCondition, "LOC=<ThirdLeg.Destination>,FAC=CTO");
				EnsureProcessTaskCreated(arrivalRelatedEvent, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, someCondition, has2ndLeg, someCondition, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, someCondition, has3rdLeg, someCondition, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, someCondition, has4thLeg, someCondition, ZString.Empty);
				EnsureProcessTaskCreated(arrivalRelatedEvent, someCondition, ZString.Empty, someCondition, ZString.Empty);
			}
		}

		void EnsureProcessTaskCreated(ZString evnt, ZString templateTriggerCondition, ZString templateCreateCondition, ZString processTaskTriggerCondition, ZString expectedConditionValue)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;

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

		protected override ContainerProcessTaskCollection GetCollectionToTestCore()
		{
			return new ContainerProcessTaskCollection(Container);
		}

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Factory.NewWithValidTestData<CommonContainer>();
				}
				return container;
			}
		}
		CommonContainer container;

		CommonConsol Consol1
		{
			get { return consol1 ?? (consol1 = Factory.NewWithValidTestData<CommonConsol>()); }
		}
		CommonConsol consol1;

		BusinessObject JobDeclaration
		{
			get { return jobdeclaration ?? (jobdeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)))); }
		}
		BusinessObject jobdeclaration;

		Transport Transport1
		{
			get
			{
				if (transport1 == null)
				{
					transport1 = Consol1.Transports[0];
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
					transport2 = Consol1.Transports.AddNew();
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
					transport3 = Consol1.Transports.AddNew();
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
					transport4 = Consol1.Transports.AddNew();
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
