using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class ModuleToModuleSenderTest : TestCaseWithFactory
	{
		public void TestCreateNewEntityFromParent_WithExceptionThrown()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var sender = new DummyModuleToModuleSender();
			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataExportCode,
				DataContext = DataContextFactory.New(),
			};

			sender.Events = new UniversalEvent[] { universalEvent };
			sender.ExceptionToThrowWhenGettingEvents = new ZSaveException(new ZDataException(new Exception("Some Save Error."), ((INeedRow)dummy).Row, TestConnection), Factory);

			var publishResult1 = sender.CreateEntityFromParent(dummy);
			AssertNull(publishResult1.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult1.ResultType);
			AssertEquals($@"Failure!
Save Exception occurred:

** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: DummyBizo
PK: {dummy.PK}
RowState: Added
Factory validation suspended: False
Factory name for debugging: 
Business object around row = Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow
Business object validation suspended: 0
Business object is marking as needing validation suspended: False
Business object light validation is enabled: True
Business object additional info: 

Inner Message = Some Save Error.

", publishResult1.ErrorMessage);

			sender.ExceptionToThrowWhenGettingEvents = new MessageProcessingBusinessFailureException("Some Message Processing Failure.");
			var publishResult2 = sender.CreateEntityFromParent(dummy);
			AssertNull(publishResult2.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult2.ResultType);
			AssertEquals("Failure!\r\nUniversal Processing Failure occurred:\r\nSome Message Processing Failure.", publishResult2.ErrorMessage);
		}

		public void TestCreateNewEntityFromParent_WithNoEvents()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var sender = new DummyModuleToModuleSender();

			var publishResult = sender.CreateEntityFromParent(dummy);
			AssertNull(publishResult);
		}

		public void TestCreateNewEntityFromParent_WithSuccessfulExportedJob()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var sender = new DummyModuleToModuleSender();

			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataExportCode,
				DataContext = DataContextFactory.New(),
			};

			sender.Events = new UniversalEvent[] { universalEvent };

			var publishResult = sender.CreateEntityFromParent(dummy);
			AssertNull(publishResult.FindJobIfExists());
			AssertEquals(UniversalResult.External, publishResult.ResultType);
			AssertEquals("", publishResult.ErrorMessage);
		}

		public void TestCreateNewEntityFromParent_WithFailedExportedJob()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var sender = new DummyModuleToModuleSender();

			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataExportFailureCode,
				DataContext = DataContextFactory.New(),
				ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Failed export!" } }
			};

			sender.Events = new UniversalEvent[] { universalEvent };

			var publishResult = sender.CreateEntityFromParent(dummy);
			AssertNull(publishResult.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult.ResultType);
			AssertEquals(@"Failure!
Failed export!", publishResult.ErrorMessage);
		}

		public void TestCreateNewEntityFromParent_WithSuccessfulImportedJob()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);

			var dummyToLoad = Factory.New<DummyWithWorkflow>();
			dummyToLoad.Z0_Description = "LOADME";

			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			var sender = new DummyModuleToModuleSender();

			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataImportCode,
				DataContext = DataContextFactory.New(),
			};

			universalEvent.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "LOADME");
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			sender.Events = new UniversalEvent[] { universalEvent };

			var publishResult = sender.CreateEntityFromParent(dummy);
			var job = publishResult.FindJobIfExists();
			AssertEquals(dummyToLoad.PK, job.PK);
			AssertEquals(UniversalResult.Internal, publishResult.ResultType);
			AssertEquals("", publishResult.ErrorMessage);
		}

		public void TestCreateNewEntityFromParent_WithFailedImportedJob()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var sender = new DummyModuleToModuleSender();

			var universalEvent = new UniversalEvent
			{
				EventType = Events.DataImportFailureCode,
				DataContext = DataContextFactory.New(),
				ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Failed import!" } }
			};

			universalEvent.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "");
			universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			sender.Events = new UniversalEvent[] { universalEvent };

			var publishResult = sender.CreateEntityFromParent(dummy);
			AssertNull(publishResult.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult.ResultType);
			AssertEquals(@"Failure!
Failed import!", publishResult.ErrorMessage);
		}

		#region Implementation

		public class DummyModuleToModuleSender : ModuleToModuleSender<DummyWithWorkflow>
		{
			protected override UniversalEvent[] GetUniversalEvents(DummyWithWorkflow parentEntity)
			{
				if (ExceptionToThrowWhenGettingEvents != null)
				{
					throw ExceptionToThrowWhenGettingEvents;
				}

				return Events ?? Array.Empty<UniversalEvent>();
			}

			public Exception ExceptionToThrowWhenGettingEvents { get; set; }

			public UniversalEvent[] Events { get; set; }

			public void SetUpEventIfNeeded(string eventType, string reference)
			{
				var universalEvent = new UniversalEvent
				{
					EventType = eventType,
					DataContext = DataContextFactory.New()
				};

				if (eventType == Enterprise.ZArchitecture.Business.Events.DataImportFailureCode)
				{
					universalEvent.ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Failed import!" } };
				}
				if (eventType == Enterprise.ZArchitecture.Business.Events.DataExportFailureCode)
				{
					universalEvent.ContextCollection = new List<Context>() { new Context { Type = nameof(UniversalEvent.ContextTypes.FailureReason), Value = "Failed export!" } };
				}

				if (eventType != Enterprise.ZArchitecture.Business.Events.DataExportCode)
				{
					universalEvent.DataContext.AddDataSource(DataContextType.DummyBusinessObject, reference);
					universalEvent.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				}
				Events = new UniversalEvent[] { universalEvent };
			}

			protected override ZString ErrorPrefix
			{
				get { return "Failure!"; }
			}

			public PublishToUniversalResult CreateEntityFromParent(DummyWithWorkflow parentEntity)
			{
				return CreateNewEntityFromParent(parentEntity);
			}

			protected override DataContextType EntityTypeToLoad
			{
				get { return DataContextType.DummyBusinessObject; }
			}
		}

		#endregion
	}
}
