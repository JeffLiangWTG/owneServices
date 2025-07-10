using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	[TestedType(typeof(WorkflowExceptionReader))]
	public class WorkflowExceptionReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadIntoBuninessObject_ShouldUpdateException()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);
			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);

			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("exceptions count", 1, parent.WorkflowItems.Exceptions.Count);
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update exception", exception.P9_Description, updatedException.P9_Description);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...", logger.Logs.Trim());
			});
		}

		public void TestReadIntoBuninessObject_CompletionTimeShouldBeSet()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);
			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = exception.P9_Description,
				Actioned = true,
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);

			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("exceptions count", 1, parent.WorkflowItems.Exceptions.Count);
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				Assert("Should be actioned", updatedException.IsExceptionActioned);
				Assert("P9_CompletedTimeUtc should have value", updatedException.P9_CompletedTimeUtc.IsValid);
			});
		}

		public void TestReadIntoBuninessObject_ShouldCreateNewException_WhenNoExceptionID_OrNotMatchID()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);
			Factory.SaveForTesting();

			var exceptionDataObjectNoID = new WorkflowException()
			{
				ExceptionID = null,
				Description = "I am a new exception!"
			};

			var exceptionDataObjectNotMatchID = new WorkflowException()
			{
				ExceptionID = "BLA",
				Description = "I am a new exception! in this system!"
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObjectNoID, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var newException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 2, parent.WorkflowItems.Exceptions.Count);
				AssertEquals("Should not have changed current Exception", nameof(exception), exception.P9_Description);

				AssertNotNull("New Exception should not be null", newException);
				AssertEquals("Should create exception", exceptionDataObjectNoID.Description.Value, newException.P9_Description);

				AssertMultilineASCIIEquals("logs", @"
Information - No matching Exception: I am a new exception! found, creating new Exception: I am a new exception!.
Information - Populating Exception: I am a new exception!...", logger.Logs.Trim());
			});

			logger.ClearLogs();
			reader = new WorkflowExceptionReader(exceptionDataObjectNotMatchID, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			newException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 3, parent.WorkflowItems.Exceptions.Count);
				AssertEquals("Should not have changed current Exception", nameof(exception), exception.P9_Description);

				AssertNotNull("New Exception should not be null", newException);
				AssertEquals("Should create exception", exceptionDataObjectNotMatchID.Description.Value, newException.P9_Description);

				AssertMultilineASCIIEquals("logs", @"
Information - No matching Exception: I am a new exception! in this system! found, creating new Exception: I am a new exception! in this system!.
Information - Populating Exception: I am a new exception! in this system!...", logger.Logs.Trim());
			});
		}

		public void TestReadIntoBusinessObjectAsActionedShouldCreateEXAEventOnSave()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);

			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				Description = "Actioned Exception",
				Type = exceptionType.WET_Code,
				Actioned = true,
			};

			new WorkflowExceptionReader(exceptionDataObject, new TestErrorLogger(), Factory, null, parent.WorkflowItems.Exceptions).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 1, parent.WorkflowItems.Exceptions.Count);
				AssertNotNull("Should have an EXA Event", MasterFilesTestHelper.GetLatestLog(parent, Events.ExceptionActionedCode, "|TYP=TYP|DES=Actioned Exception"));
				AssertNotNull("Should have an EXR Event", MasterFilesTestHelper.GetLatestLog(parent, Events.ExceptionRaisedCode, "|TYP=TYP|DES=Actioned Exception"));
			});
		}

		public void TestChangeExceptionToActionedShouldCreateEXAEventOnSave()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = "Test Exception";
			exception.ExceptionTypeCode = "TYP";

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);

			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = exception.P9_Description,
				Actioned = true,
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);

			var updatedException = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 1, parent.WorkflowItems.Exceptions.Count);
				MasterFilesTestHelper.AssertEventRaised("Actioning exception with XML should raise EXA event", parent, Events.ExceptionActionedCode);
			});
		}

		public void VerifyAllFieldsAreMapped(UXmlDateTime date, UXmlDateTime actionedDate, UXmlDateTime endDate)
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);
			var now = ZDateTimeOffset.Now;

			var categories = new CodeDescriptionPairList();
			categories.AddPair("CAT", "Category");
			WorkflowDataRegistry.Instance.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(categories));

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);
			exceptionType.WET_Category = "CAT";

			var cause = exceptionType.Causes.AddNew();
			cause.WEC_Code = "CAU";
			cause.WEC_Description = "cause";
			var resolution = exceptionType.Resolutions.AddNew();
			resolution.WER_Code = "RES";
			resolution.WER_Description = "resolution";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			group.GG_Desc = "Group";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "STF";
			staff.GS_FullName = "Staff";

			Factory.SaveForTesting();

			var exceptionDataObjectForUpdateException = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
				Actioned = true,
				Date = date,

				Type = "TYP",
				Category = "CAT",

				Cause = "CAU",
				Resolution = "RES",

				Group = new Group() { Code = "GRP", Name = "Group" },
				Staff = new Staff() { Code = "STF", Name = "Staff" },

				ActionedDate = actionedDate,

				EndDate = endDate,
				DurationHours = 50,
				Location = new UNLOCO() { Code = "CNSHA", Name = "Shanghai" },
				Notes = "New note added"
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObjectForUpdateException, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 1, parent.WorkflowItems.Exceptions.Count);
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update P9_Description", exception.P9_Description, exceptionDataObjectForUpdateException.Description.Value);
				AssertEquals("Should update IsExceptionActioned", exception.IsExceptionActioned, exceptionDataObjectForUpdateException.Actioned.Value);
				AssertEquals("Should update P9_ActualDate", exception.P9_ActualDateOffset, exceptionDataObjectForUpdateException.Date.Value);
				AssertEquals("Should update ExceptionType", exception.ExceptionType.WET_Code, exceptionDataObjectForUpdateException.Type.Value);
				AssertEquals("Should have same ExceptionType category", exception.ExceptionType.WET_Category, exceptionDataObjectForUpdateException.Category.Value);
				AssertEquals("Should update Cause", exception.ProcessWorkflowException.Cause.WEC_Code, exceptionDataObjectForUpdateException.Cause.Value);
				AssertEquals("Should update Resolution", exception.ProcessWorkflowException.Resolution.WER_Code, exceptionDataObjectForUpdateException.Resolution.Value);
				AssertEquals("Should update P9_GG_AssignedGroup", exception.P9_GG_AssignedGroupCode, exceptionDataObjectForUpdateException.Group.Code);
				AssertEquals("Should have same Group name", exception.GroupName, exceptionDataObjectForUpdateException.Group.Name);
				AssertEquals("Should update P9_GS_NKAssignedStaffMember", exception.P9_GS_NKAssignedStaffMember, exceptionDataObjectForUpdateException.Staff.Code);
				AssertEquals("Should have same Staff name", exception.StaffName, exceptionDataObjectForUpdateException.Staff.Name);
				AssertEquals("Should update P9_CompletedTimeUtc", exception.P9_CompletedTimeUtc, updatedException.P9_CompletedTimeUtc);
				AssertEquals("Should update P9_ExceptionEndDate", exception.P9_ExceptionEndDate, exceptionDataObjectForUpdateException.EndDate);
				AssertEquals("Should update P9_ExceptionDurationHours", exception.P9_ExceptionDurationHours, exceptionDataObjectForUpdateException.DurationHours);
				AssertEquals("Should update P9_RL_NKExceptionLocation", exception.P9_RL_NKExceptionLocation, exceptionDataObjectForUpdateException.Location.Code);
				AssertEquals("Should update P9_Notes", exception.P9_NotesAsPlainText, exceptionDataObjectForUpdateException.Notes);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...", logger.Logs.Trim());
			});
		}

		public void TestAllFieldsAreMapped()
		{
			var now = ZDateTimeOffset.Now;
			VerifyAllFieldsAreMapped(now, now.AddDays(-1), now.AddDays(-2));
		}

		public void TestAllFieldsAreMappedWithEmptyDates()
		{
			VerifyAllFieldsAreMapped(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}

		public void TestShouldLogWarning_WhenExceptionDescriptionNotfound_AndNotUpdateTargetBO()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);
			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "",
				Actioned = true
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should keep exception descrition", exception.P9_Description, updatedException.P9_Description);
				AssertEquals("Should not update excetpion", false, updatedException.IsExceptionActioned);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Cannot populate Exception:  because:
Exception without description", logger.Logs.Trim());
			});
		}

		public void TestShouldLogWarning_WhenExceptionTypeNotFound()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);
			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
				Type = "BLA!!"
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update P9_Description", exception.P9_Description, exceptionDataObject.Description.Value);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Warning - Attempted to insert 5 characters into Field [P9_SE_NKExceptionEvent] which has a maximum length of 3 characters. Field was truncated.
Warning - Could not find exception type:BLA!! of the exception:I have changed!", logger.Logs.Trim());
			});
		}

		public void TestShouldLogWarning_WhenExceptionCategoryNotFound()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);

			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
				Type = "TYP",
				Category = "BLA!!"
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update P9_Description", exception.P9_Description, exceptionDataObject.Description.Value);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Warning - Could not find category:BLA!! on exception type:TYP of the exception:I have changed!", logger.Logs.Trim());
			});
		}

		public void TestShouldLogWarning_WhenExceptionCauseNotFound()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);

			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
				Type = "TYP",
				Cause = "BLA!!"
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update P9_Description", exception.P9_Description, exceptionDataObject.Description.Value);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Warning - Could not find exception cause:BLA!! of the exception:I have changed!", logger.Logs.Trim());
			});
		}

		public void TestShouldLogWarning_WhenExceptionResolutionNotFound()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);

			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
				Type = "TYP",
				Resolution = "BLA!!"
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update P9_Description", exception.P9_Description, exceptionDataObject.Description.Value);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Warning - Could not find exception resolution:BLA!! of the exception:I have changed!", logger.Logs.Trim());
			});
		}

		public void TestShouldLogWarning_WhenExceptionGroupNotFound()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var exception = parent.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EXP";
			exception.P9_Description = nameof(exception);

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);

			Factory.SaveForTesting();

			var exceptionDataObject = new WorkflowException()
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!",
				Type = "TYP",
				Group = new Group() { Code = "BLA!" }
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionReader(exceptionDataObject, logger, Factory, exception, parent.WorkflowItems.Exceptions);
			var updatedException = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Updated Exception should not be null", updatedException);
				AssertEquals("Should be the same Exception", exception.PK, updatedException.PK);
				AssertEquals("Should update P9_Description", exception.P9_Description, exceptionDataObject.Description.Value);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Warning - Could not find exception group:BLA! of the exception:I have changed!", logger.Logs.Trim());
			});
		}
	}
}

