using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalExceptionWriterTest : TestCaseWithFactory
	{
		public void TestPopulateExceptions()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertPopulateExceptions(shipmentBO, shipment);
		}

		void AssertPopulateExceptions(BusinessObject source, IExceptionCollectionParent exceptionCollectionParent)
		{
			var writer = new UniversalExceptionWriter();
			var exception1 = ((IWorkflowProvider)source).WorkflowItems.Exceptions.AddNew();

			exception1.P9_Description = "exception1";
			exception1.IsExceptionActioned = false;
			exception1.P9_ActualDateOffset = ZDateTimeOffset.Now;
			exception1.ExceptionTypeCode = ZString.Empty;
			exception1.P9_ExceptionEndDate = ZDateTimeOffset.Empty;
			exception1.P9_ExceptionDurationHours = 0;
			exception1.P9_RL_NKExceptionLocation = ZString.Empty;
			exception1.P9_NotesAsString = "This is a test note";

			var exception2 = ((IWorkflowProvider)source).WorkflowItems.Exceptions.AddNew();

			exception2.P9_Description = "exception2";
			exception2.IsExceptionActioned = true;
			exception2.P9_ActualDateOffset = ZDateTimeOffset.Now;
			exception2.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(2);
			exception2.P9_ExceptionDurationHours = 50;
			exception2.P9_RL_NKExceptionLocation = "CNSHA";

			var categories = new CodeDescriptionPairList();
			categories.AddPair("AAA", "Category");
			WorkflowDataRegistry.Instance.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(categories));

			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);
			exceptionType.WET_Category = "AAA";
			var cause = exceptionType.Causes.AddNew();
			cause.WEC_Code = "CAU";
			cause.WEC_Description = "cause";
			var resolution = exceptionType.Resolutions.AddNew();
			resolution.WER_Code = "RES";
			resolution.WER_Description = "resolution";

			exception2.ExceptionTypeCode = exceptionType.WET_Code;
			exception2.ExceptionCausePK = cause.PK;
			exception2.ExceptionResolutionPK = resolution.PK;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			group.GG_Desc = "Group";
			exception2.P9_GG_AssignedGroup = group.PK;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "STF";
			staff.GS_FullName = "Staff";
			exception2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			writer.PopulateExceptions(source, exceptionCollectionParent as IDataObject);
			AssertEquals("include internal exception", 2, exceptionCollectionParent.ExceptionCollection.Count);

			AssertExceptionIsCorrect(exception1, exceptionCollectionParent.ExceptionCollection[0]);
			AssertExceptionIsCorrect(exception2, exceptionCollectionParent.ExceptionCollection[1]);
		}

		void AssertExceptionIsCorrect(ProcessTask processTask, WorkflowException exception)
		{
			AssertEquals("ExceptionID", processTask.P9_TaskID, exception.ExceptionID);
			AssertEquals("Description", processTask.P9_Description, exception.Description);
			AssertEquals("Type", processTask.P9_ActualDateOffset.IsValid ? processTask.P9_ActualDateOffset : null, exception.Date);
			AssertEquals("Category", processTask.ExceptionType?.WET_Category, exception.Category);
			AssertEquals("Cause", processTask.ProcessWorkflowException?.Cause.WEC_Code, exception.Cause);
			AssertEquals("Resolution", processTask.ProcessWorkflowException?.Resolution.WER_Code, exception.Resolution);
			AssertEquals("Group Code", processTask.AssignedGroup?.GG_Code, exception.Group?.Code);
			AssertEquals("Group Name", processTask.AssignedGroup?.GG_Desc, exception.Group?.Name);
			AssertEquals("Staff Code", processTask.AssignedStaffMember?.GS_Code, exception.Staff?.Code);
			AssertEquals("Staff Name", processTask.AssignedStaffMember?.GS_FullName, exception.Staff?.Name);
			AssertEquals("Actioned", processTask.IsExceptionActioned, exception.Actioned);
			if (processTask.IsExceptionActioned)
			{
				if (!processTask.P9_CompletedTimeUtc.IsValid)
				{
					AssertEquals("P9_CompletedTimeUtc should be populated in the DB if processTask.IsExceptionActioned, this case only happened because of a bug", ZDateTimeOffset.Empty, exception.ActionedDate.Value);
				}
				else
				{
					AssertEquals("Actioned Date", processTask.P9_CompletedTimeUtc, exception.ActionedDate.Value.ToUTCTime(ZDateTimeOffset.Now.Offset));
				}
			}
			else
			{
				AssertNull("Actioned Date", exception.ActionedDate);
			}
			AssertEquals("Exception End Date", processTask.P9_ExceptionEndDate.IsValid ? processTask.P9_ExceptionEndDate : null, exception.EndDate);
			AssertEquals("Exception Duration Hours", processTask.P9_ExceptionDurationHours, exception.DurationHours);
			AssertEquals("Location", processTask.P9_RL_NKExceptionLocation, exception.Location.Code);
			AssertEquals("Notes", processTask.P9_NotesAsPlainText, exception.Notes);
		}

		public void TestPopulateActionedExceptionWithNoActualDate()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var exceptionCollectionParent = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var exception = ((IWorkflowProvider)shipmentBO).WorkflowItems.Exceptions.AddNew();

			exception.P9_Description = "exception1";
			exception.IsExceptionActioned = true;
			exception.ExceptionTypeCode = ZString.Empty;
			exception.P9_ExceptionEndDate = ZDateTimeOffset.Empty;
			exception.P9_ExceptionDurationHours = 0;
			exception.P9_RL_NKExceptionLocation = ZString.Empty;

			var writer = new UniversalExceptionWriter();
			writer.PopulateExceptions(shipmentBO, exceptionCollectionParent);
			AssertExceptionIsCorrect(exception, exceptionCollectionParent.ExceptionCollection[0]);
		}

		public void TestPopulateExceptionWithInvalidCompletionTime()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var exceptionCollectionParent = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var exception = ((IWorkflowProvider)shipmentBO).WorkflowItems.Exceptions.AddNew();

			exception.P9_Description = "exception1";
			exception.IsExceptionActioned = true;
			exception.ExceptionTypeCode = ZString.Empty;
			exception.P9_ExceptionEndDate = ZDateTimeOffset.Empty;
			exception.P9_ExceptionDurationHours = 0;
			exception.P9_RL_NKExceptionLocation = ZString.Empty;
			exception.P9_ActualDateOffset = ZDateTimeOffset.Now;
			exception.P9_CompletedTimeUtc = ZDateTime.Invalid;

			var writer = new UniversalExceptionWriter();
			writer.PopulateExceptions(shipmentBO, exceptionCollectionParent);
			AssertExceptionIsCorrect(exception, exceptionCollectionParent.ExceptionCollection[0]);
		}
	}
}
