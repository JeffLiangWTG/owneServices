using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class ContainerWorkflowExceptionGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2011, 06, 01, 10, 20, 30)]
		public void TestHighWaterMark()
		{
			HighWaterMarkItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2011, 01, 01));
			AssertEquals("Precondition", new DateTime(2011, 01, 01), HighWaterMarkItem.Value);
			BusinessObject consol1 = CreateConsol("AConsol", null);
			Factory.Save();
			var generator = new ContainerWorkflowExceptionGeneratorForTesting();
			generator.Process(new NotificationCollection());
			CombineAssertions("HighWaterMarkItem set to current UTC", () =>
			{
				ZDateTime highWaterMark = HighWaterMarkItem.Value;
				AssertEquals(2011, highWaterMark.Year);
				AssertEquals(06, highWaterMark.Month);
				AssertEquals(01, highWaterMark.Day);
				AssertEquals(10, highWaterMark.Hour);
				AssertEquals(20, highWaterMark.Minute);
			}

			);
			HighWaterMarkItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2011, 01, 01));
			AssertEquals("Precondition", new DateTime(2011, 01, 01), HighWaterMarkItem.Value);
			BusinessObject consol2 = CreateConsol("AnotherOne", null);
			Factory.Save();
			NumberOfTimesToThrowConcurrencyException = 3;
			AssertExceptionThrown(typeof(ZCannotSaveException), () => generator.Process(new NotificationCollection()));
			AssertEquals("HighWaterMarkItem not set to new value as bad things happened on factory saving", new DateTime(2011, 01, 01), HighWaterMarkItem.Value);
		}

		public void TestSaveConcurrencyExceptionRetries()
		{
			BusinessObject consol1 = CreateConsol("A1", null);
			Factory.Save();
			NotificationCollection notifications = new NotificationCollection();
			var generator = new ContainerWorkflowExceptionGeneratorForTesting();
			NumberOfTimesToThrowConcurrencyException = 1;
			generator.Process(notifications);
			AssertContainerException(consol1, true);
			BusinessObject consol2 = CreateConsol("A2", null);
			Factory.Save();
			NumberOfTimesToThrowConcurrencyException = 2;
			generator.Process(notifications);
			AssertContainerException(consol2, true);
			BusinessObject consol3 = CreateConsol("A3", null);
			Factory.Save();
			NumberOfTimesToThrowConcurrencyException = 3;
			AssertExceptionThrown(typeof(ZCannotSaveException), () => generator.Process(notifications));
			AssertContainerException(consol3, false);
			notifications.Clear();
			NumberOfTimesToThrowConcurrencyException = 10;
			AssertExceptionThrown(typeof(ZCannotSaveException), () => generator.Process(notifications));
			AssertEquals("Retried 3 times to save factory", 7, NumberOfTimesToThrowConcurrencyException);
			AssertContainerException(consol3, false);
		}

		public void TestNonConcurrencyExceptionsAreThrown()
		{
			BusinessObject consol1 = CreateConsol("A1", null);
			Factory.Save();
			AssertExceptionThrown("Non-concurrency exceptions are not eaten", typeof(Exception), () =>
			{
				var generator = new ContainerWorkflowExceptionGeneratorForTesting();
				generator.ShouldThrowNonConcurrencyException = true;
				generator.Process(new NotificationCollection());
			}

			);
		}

		[SnailTest]
		public void TestBusinessObjectsAreProcessedInBatches()
		{
			for (int i = 0; i < 301; i++)
			{
				CreateConsol("A" + i, null);
			}

			Factory.Save();
			var generator = new ContainerWorkflowExceptionGeneratorForTesting();
			AssertNoExceptionThrown(() => generator.Process(new NotificationCollection()));
			AssertEquals("Objects were loaded in batches of top 100", 4, generator.LoadedObjects.Count);
			AssertEquals("First batch", 100, generator.LoadedObjects.Values.ElementAt(0));
			AssertEquals("Second batch", 100, generator.LoadedObjects.Values.ElementAt(1));
			AssertEquals("Third batch", 100, generator.LoadedObjects.Values.ElementAt(2));
			AssertEquals("Fourth batch", 1, generator.LoadedObjects.Values.ElementAt(3));
			AssertEquals("Each batch was loaded in different factory", 4, generator.LoadedObjects.Keys.Distinct().Count());
		}

		public void TestProcess()
		{
			BusinessObject consol1 = CreateConsol("Amazing", null);
			BusinessObject consol2 = CreateConsol("Another", null);
			BusinessObject consol3 = CreateConsol("Bench", null);
			BusinessObject consolWithException = CreateConsol("AlreadyHaveOne", ProcessWorkflowExceptionType.ExceptionContainerDetention);
			BusinessObject consolShouldBeNotFiltered = CreateConsol("JustConsol", null);
			BusinessObject declaration1 = CreateDeclaration("Arrgh", null);
			BusinessObject declaration2 = CreateDeclaration("BooBoo", null);
			BusinessObject declaration3 = CreateDeclaration("Bee", null);
			BusinessObject declarationWithException = CreateDeclaration("AAA", ProcessWorkflowExceptionType.ExceptionContainerDetention);
			BusinessObject declarationShouldBeNotFiltered = CreateDeclaration("Random", null);
			Factory.Save();
			AssertContainerException(consol1, false);
			AssertContainerException(consol2, false);
			AssertContainerException(consol3, false);
			AssertContainerException(consolWithException, true);
			AssertContainerException(consolShouldBeNotFiltered, false);
			AssertContainerException(declaration1, false);
			AssertContainerException(declaration2, false);
			AssertContainerException(declaration3, false);
			AssertContainerException(declarationWithException, true);
			AssertContainerException(declarationShouldBeNotFiltered, false);
			var generator = new ContainerWorkflowExceptionGeneratorForTesting();
			generator.Process(new NotificationCollection());
			AssertContainerException(consol1, true);
			AssertContainerException(consol2, true);
			AssertContainerException(consol3, true);
			AssertContainerException(consolWithException, true);
			AssertContainerException(consolShouldBeNotFiltered, false);
			AssertContainerException(declaration1, true);
			AssertContainerException(declaration2, true);
			AssertContainerException(declaration3, true);
			AssertContainerException(declarationWithException, true);
			AssertContainerException(declarationShouldBeNotFiltered, false);
		}

		#region Implementation

		void AssertContainerException(BusinessObject parent, bool hasContainerException)
		{
			ProcessTaskCollection workflowItems = ((IWorkflowProvider)parent).WorkflowItems;
			workflowItems.Load();
			AssertEquals("Exceptions count", hasContainerException ? 1 : 0, workflowItems.Exceptions.Count);
			if (hasContainerException)
			{
				AssertEquals("Expected exception type", ProcessWorkflowExceptionType.ExceptionContainerDetention, workflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
			}
		}

		BusinessObject CreateConsol(string uniqueRef, string alreadyExistingEvent)
		{
			BusinessObject consol = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_UniqueConsignRef] = uniqueRef;
			if (alreadyExistingEvent != null)
			{
				var containerException = ((IWorkflowProvider)consol).WorkflowItems.Exceptions.AddNew();
				containerException.P9_SE_NKExceptionEvent = alreadyExistingEvent;
				containerException.P9_Description = containerException.GetExceptionEventDescription(containerException.P9_SE_NKExceptionEvent);
			}

			return consol;
		}

		BusinessObject CreateDeclaration(string agentsRef, string alreadyExistingEvent)
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_AgentsReference] = agentsRef;
			if (alreadyExistingEvent != null)
			{
				var containerException = ((IWorkflowProvider)declaration).WorkflowItems.Exceptions.AddNew();
				containerException.P9_SE_NKExceptionEvent = alreadyExistingEvent;
				containerException.P9_Description = containerException.GetExceptionEventDescription(containerException.P9_SE_NKExceptionEvent);
			}

			return declaration;
		}

		public static DateTimeRegistryItem HighWaterMarkItem
		{
			get
			{
				return highWaterMarkItem ?? (highWaterMarkItem = new DateTimeRegistryItem("HighWaterMarkItem", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsHidden, DateTime.MinValue));
			}
		}

		static DateTimeRegistryItem highWaterMarkItem;

		class ContainerWorkflowExceptionGeneratorForTesting : ContainerWorkflowExceptionGenerator
		{
			#region Implementation

			protected override string ContainerExceptionEvent
			{
				get
				{
					return ProcessWorkflowExceptionType.ExceptionContainerDetention;
				}
			}

			protected override DateTimeRegistryItem HighWaterMarkRegistryItem
			{
				get
				{
					return HighWaterMarkItem;
				}
			}

			protected override IEnumerable<ZDBOnlyQuery> ConsolQueries
			{
				get
				{
					ZDBOnlyQuery consolQueryA = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
					consolQueryA.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, SQLComparisonOperator.StartsWith, "A");
					ZDBOnlyQuery consolQueryB = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
					consolQueryB.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, SQLComparisonOperator.StartsWith, "B");
					yield return consolQueryA;
					yield return consolQueryB;
				}
			}

			protected override IEnumerable<ZDBOnlyQuery> DeclarationQueries
			{
				get
				{
					ZDBOnlyQuery declarationQueryA = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
					declarationQueryA.AddToFilter(JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "A");
					ZDBOnlyQuery declarationQueryB = new ZDBOnlyQuery(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
					declarationQueryB.AddToFilter(JobDeclarationSchema.JE_AgentsReference, SQLComparisonOperator.StartsWith, "B");
					yield return declarationQueryA;
					yield return declarationQueryB;
				}
			}

			#endregion Implementation

			#region Overrides for testing

			protected override BusinessObject[] CreateFactoryAndLoadBusinessObjects(Type bizoType, ZDBOnlyQuery query)
			{
				BusinessObject[] loadedObjects = base.CreateFactoryAndLoadBusinessObjects(bizoType, query);
				if (loadedObjects.Length > 0)
				{
					LoadedObjects.Add(loadedObjects[0].Factory._Instance, loadedObjects.Length);
				}

				return loadedObjects;
			}

			public Dictionary<long, int> LoadedObjects = new Dictionary<long, int>();

			protected override bool TrySaveFactory(BusinessObjectFactory factory, bool shouldEatConcurrencyException)
			{
				if (NumberOfTimesToThrowConcurrencyException > 0)
				{
					DummyBusinessObject dummy = factory.New<DummyBusinessObject>();
					factory.Saving += (factoryToBeSaved) =>
					{
						if (NumberOfTimesToThrowConcurrencyException > 0)
						{
							NumberOfTimesToThrowConcurrencyException--;
							throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)dummy).Row, Db.Connection), factoryToBeSaved);
						}
					};
				}
				else if (ShouldThrowNonConcurrencyException)
				{
					factory.Saving += (factoryToBeSaved) =>
					{
						throw new Exception("I like to throw it, throw it!");
					};
				}

				return base.TrySaveFactory(factory, shouldEatConcurrencyException);
			}

			public bool ShouldThrowNonConcurrencyException;

			#endregion Overrides for testing
		}

		protected override void SetUp()
		{
			base.SetUp();
			NumberOfTimesToThrowConcurrencyException = 0;
		}

		static int NumberOfTimesToThrowConcurrencyException;

		#endregion Implementation
	}
}
