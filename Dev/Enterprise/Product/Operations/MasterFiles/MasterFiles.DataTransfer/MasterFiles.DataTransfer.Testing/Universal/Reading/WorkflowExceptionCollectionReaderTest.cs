using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	[TestedType(typeof(WorkflowExceptionCollectionReader))]
	public class WorkflowExceptionCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var parent = Factory.New<DummyWithWorkflow>();

			var exception1 = parent.WorkflowItems.Exceptions.AddNew();
			exception1.P9_TaskID = "EX1";
			exception1.P9_Description = nameof(exception1);

			var exception2 = parent.WorkflowItems.Exceptions.AddNew();
			exception2.P9_TaskID = "EX2";
			exception2.P9_Description = nameof(exception2);

			var exceptionDataObjectForException1 = new WorkflowException
			{
				ExceptionID = exception1.P9_TaskID,
				Description = "I have changed!"
			};

			var exceptionDataObjectForNewException = new WorkflowException
			{
				ExceptionID = null,
				Description = "I am a new exception!"
			};

			var exceptionWhithoutDescription = new WorkflowException();

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionCollectionReader(new[] { exceptionDataObjectForException1, exceptionDataObjectForNewException, exceptionWhithoutDescription }, logger, Factory, parent);

			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 3, parent.WorkflowItems.Exceptions.Count);
				AssertCollectionContains("Exception 1 not removed", exception1, parent.WorkflowItems.Exceptions);
				AssertEquals("Exception 1 has changed", exceptionDataObjectForException1.Description, exception1.P9_Description);
				AssertCollectionContains("Exception 2 not matched, but not removed", exception2, parent.WorkflowItems.Exceptions);
				AssertNotNull("New Exception was added", parent.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == exceptionDataObjectForNewException.Description.Value));

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Information - No matching Exception: I am a new exception! found, creating new Exception: I am a new exception!.
Information - Populating Exception: I am a new exception!...
Information - Cannot populate Exception:  because:
Exception without description", logger.Logs.Trim());
			});
		}

		public void TestReadIntoCollection_ShouldCreateNewException_WhenExceptionMatchTaskID_FromOtherParent()
		{
			var parentToImport = Factory.New<DummyWithWorkflow>();
			var exception = parentToImport.WorkflowItems.Exceptions.AddNew();
			exception.P9_TaskID = "EX";
			exception.P9_Description = nameof(exception);

			var otherParent = Factory.New<DummyWithWorkflow>();
			var exceptionOnOtherParent = otherParent.WorkflowItems.Exceptions.AddNew();
			exceptionOnOtherParent.P9_TaskID = "EXP";
			exceptionOnOtherParent.P9_Description = nameof(exceptionOnOtherParent);

			var exceptionDataObjectForException = new WorkflowException
			{
				ExceptionID = exception.P9_TaskID,
				Description = "I have changed!"
			};

			var exceptionDataObjectOnOtherParent = new WorkflowException
			{
				ExceptionID = exceptionOnOtherParent.P9_TaskID,
				Description = exceptionOnOtherParent.P9_Description
			};

			var logger = new TestErrorLogger();
			var reader = new WorkflowExceptionCollectionReader(new[] { exceptionDataObjectForException, exceptionDataObjectOnOtherParent }, logger, Factory, parentToImport);

			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals("Exceptions count", 2, parentToImport.WorkflowItems.Exceptions.Count);
				AssertCollectionContains("Exception not removed", exception, parentToImport.WorkflowItems.Exceptions);
				AssertEquals("Exception 1 has changed", exceptionDataObjectForException.Description, exception.P9_Description);
				AssertEquals("Exception on other parent should not be deleted", 1, otherParent.WorkflowItems.Exceptions.Count);

				var newExceptionFromOtherParent = parentToImport.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == exceptionDataObjectOnOtherParent.Description.Value);
				AssertNotNull("New Exception from other parent was added", newExceptionFromOtherParent);
				AssertNotEquals("New Exception from other parent was created with other ID", exceptionOnOtherParent.P9_TaskID, newExceptionFromOtherParent.P9_TaskID);

				AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Exception.
Information - Populating Exception: I have changed!...
Information - No matching Exception: exceptionOnOtherParent found, creating new Exception: exceptionOnOtherParent.
Information - Populating Exception: exceptionOnOtherParent...", logger.Logs.Trim());
			});
		}
	}
}
