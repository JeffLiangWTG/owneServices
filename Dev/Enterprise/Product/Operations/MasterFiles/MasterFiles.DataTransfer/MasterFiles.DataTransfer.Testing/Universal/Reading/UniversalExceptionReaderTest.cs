using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UniversalExceptionReaderTest : TestCaseWithFactory
	{
		[TestDate(2013, 11, 10)]
		public void TestPopulateExceptions()
		{
			var reader = new UniversalExceptionReader();
			var universalObjectFactory = new UniversalObjectFactory();
			var shipmentBO = universalObjectFactory.BOFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var exceptionCollectionParent = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var workflowExceptionList = new List<WorkflowException>();
			TestDateAttribute.UseUNLOCO = true;

			var workflowException = new WorkflowException();
			var workflowException2 = new WorkflowException();

			workflowException.Description = "exception1";
			workflowException.Category = null;
			workflowException.Type = null;
			workflowException.Actioned = false;
			workflowException.Staff = null;
			workflowException.Group = null;
			workflowException.Date = ZDateTimeOffset.UtcNow;
			workflowException.Cause = null;
			workflowException.Resolution = null;
			workflowException.ActionedDate = ZDateTimeOffset.Empty;
			workflowException.EndDate = ZDateTimeOffset.UtcNow.AddDays(2);
			workflowException.DurationHours = 0;
			workflowException.Location = null;
			workflowExceptionList.Add(workflowException);

			var categories = new CodeDescriptionPairList();
			categories.AddPair("AAA", "Category");
			WorkflowDataRegistry.Instance.ExceptionCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(categories));

			var exceptionType = universalObjectFactory.BOFactory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "TYP";
			exceptionType.WET_Description = nameof(exceptionType);
			exceptionType.WET_Category = "AAA";
			var cause = exceptionType.Causes.AddNew();
			cause.WEC_Code = "CAU";
			cause.WEC_Description = "cause";
			var resolution = exceptionType.Resolutions.AddNew();
			resolution.WER_Code = "RES";
			resolution.WER_Description = "resolution";

			var group = universalObjectFactory.BOFactory.New<GlbGroup>();
			group.GG_Code = "GRP";
			group.GG_Desc = "Group";

			var dataGroup = Group.New(group);

			var staff = universalObjectFactory.BOFactory.New<GlbStaff>();
			staff.GS_Code = "STF";
			staff.GS_FullName = "Staff";

			var dataStaff = Staff.New(staff);

			workflowException2.Description = "exception2";
			workflowException2.Category = exceptionType.WET_Category;
			workflowException2.Type = exceptionType.WET_Code;
			workflowException2.Actioned = true;
			workflowException2.Staff = dataStaff;
			workflowException2.Group = dataGroup;
			workflowException2.Date = ZDateTimeOffset.UtcNow;
			workflowException2.Cause = cause.WEC_Code;
			workflowException2.Resolution = resolution.WER_Code;
			workflowException2.ActionedDate = ZDateTimeOffset.UtcNow.AddDays(1);
			workflowException2.EndDate = ZDateTimeOffset.UtcNow.AddDays(2);
			workflowException2.DurationHours = 50;
			workflowException2.Location = new UNLOCO { Code = "AUSYD" };
			workflowExceptionList.Add(workflowException2);

			exceptionCollectionParent.SetExceptionCollection(() => workflowExceptionList);

			reader.PopulateExceptions(exceptionCollectionParent, shipmentBO, new TestErrorLogger(), universalObjectFactory);
			var exceptions = ((IWorkflowProvider)shipmentBO).WorkflowItems.Exceptions;
			exceptions.Sort("P9_Description", ListSortDirection.Ascending);

			AssertEquals("include internal exception", 2, exceptions.Count);

			var exception1 = exceptions[0];
			AssertEquals("Description", workflowException.Description, exception1.P9_Description);
			AssertEquals("Actioned", workflowException.Actioned, exception1.IsExceptionActioned);
			AssertEquals("Date", workflowException.Date, exception1.P9_ActualDateOffset);
			AssertEquals("Type", workflowException.Type, exception1.ExceptionType?.WET_Code);
			AssertEquals("Category", workflowException.Category, exception1.ExceptionType?.WET_Category);
			AssertEquals("Cause", workflowException.Cause, exception1.ProcessWorkflowException?.Cause.WEC_Code);
			AssertEquals("Resolution", workflowException.Resolution, exception1.ProcessWorkflowException?.Resolution.WER_Code);
			AssertEquals("Group Code", workflowException.Group?.Code, exception1.AssignedGroup?.GG_Code);
			AssertEquals("Group Name", workflowException.Group?.Name, exception1.AssignedGroup?.GG_Desc);
			AssertEquals("Staff Code", workflowException.Staff?.Code, exception1.AssignedStaffMember?.GS_Code);
			AssertEquals("Staff Name", workflowException.Staff?.Name, exception1.AssignedStaffMember?.GS_FullName);
			AssertEquals("Actioned Date", workflowException.ActionedDate, exception1.P9_CompletedTime);
			AssertEquals("Exception End Date", workflowException.EndDate, exception1.P9_ExceptionEndDate);
			AssertEquals("Duration Hours", workflowException.DurationHours, exception1.P9_ExceptionDurationHours);
			AssertEquals("Location", Env.CurrentBranch.NKUNLOCO, exception1.P9_RL_NKExceptionLocation);

			var exception2 = exceptions[1];
			AssertEquals("Description", workflowException2.Description, exception2.P9_Description);
			AssertEquals("Actioned", workflowException2.Actioned, exception2.IsExceptionActioned);
			AssertEquals("Date", workflowException2.Date, exception2.P9_ActualDateOffset);
			AssertEquals("Type", workflowException2.Type, exception2.ExceptionType?.WET_Code);
			AssertEquals("Category", workflowException2.Category, exception2.ExceptionType?.WET_Category);
			AssertEquals("Cause", workflowException2.Cause, exception2.ProcessWorkflowException?.Cause.WEC_Code);
			AssertEquals("Resolution", workflowException2.Resolution, exception2.ProcessWorkflowException?.Resolution.WER_Code);
			AssertEquals("Group Code", workflowException2.Group?.Code, exception2.AssignedGroup?.GG_Code);
			AssertEquals("Group Name", workflowException2.Group?.Name, exception2.AssignedGroup?.GG_Desc);
			AssertEquals("Staff Code", workflowException2.Staff?.Code, exception2.AssignedStaffMember?.GS_Code);
			AssertEquals("Staff Name", workflowException2.Staff?.Name, exception2.AssignedStaffMember?.GS_FullName);
			AssertEquals("Actioned Date", workflowException2.ActionedDate, exception2.P9_CompletedTime);
			AssertEquals("Actioned Date UTC", workflowException2.ActionedDate.Value.ToUTCTime(ZDateTimeOffset.Now.Offset), exception2.P9_CompletedTimeUtc);
			AssertEquals("Exception End Date", workflowException2.EndDate, exception2.P9_ExceptionEndDate);
			AssertEquals("Exception Duration Hours", workflowException2.DurationHours, exception2.P9_ExceptionDurationHours);
			AssertEquals("Location", workflowException2.Location.Code, exception2.P9_RL_NKExceptionLocation);
		}

		public void TestPopululateExceptionDoesNotDuplicate()
		{
			var reader = new UniversalExceptionReader();
			var universalObjectFactory = new UniversalObjectFactory();
			var dtbBookingBO = universalObjectFactory.BOFactory.New(ObjectFactory.GetType<IDtbBooking>());
			var dtbBookingConsolBO = universalObjectFactory.BOFactory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			var forwardingShipmentBO = universalObjectFactory.BOFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWithException = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var workflowExceptionList = new List<WorkflowException>();
			TestDateAttribute.UseUNLOCO = true;

			((IDtbBooking)dtbBookingBO).KM_KB_Booking = dtbBookingConsolBO.PK;
			var dtbBookingConsol = (IDtbBookingConsolidation)dtbBookingConsolBO;
			dtbBookingConsol.KB_ParentTableCode = "JS";
			dtbBookingConsol.KB_ParentID = forwardingShipmentBO.PK;

			var shipmentException = ((IWorkflowProvider)forwardingShipmentBO).WorkflowItems.Exceptions.AddNew();
			shipmentException.P9_TaskID = "T00001";
			shipmentException.P9_Description = "test";
			var workflowException = new WorkflowException();
			workflowException.ExceptionID = "T00001";
			workflowException.Description = "test";
			workflowExceptionList.Add(workflowException);
			shipmentWithException.SetExceptionCollection(() => workflowExceptionList);

			reader.PopulateExceptions(shipmentWithException, dtbBookingBO, new TestErrorLogger(), universalObjectFactory);

			Assert("The DtbBooking should not have any exceptions", ((IWorkflowProvider)dtbBookingBO).WorkflowItems.Exceptions.IsNullOrEmpty());
		}

		[TestUtcOffset(5, 0, 0)]
		[TestDate(2013, 11, 10)]
		public void TestPopulateExceptionsDatesWithNoOffsets_UseCurrentOffset()
		{
			var reader = new UniversalExceptionReader();
			var universalObjectFactory = new UniversalObjectFactory();
			var shipmentBO = universalObjectFactory.BOFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var exceptionCollectionParent = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var workflowExceptionList = new List<WorkflowException>();

			var workflowException = new WorkflowException();
			workflowException.Description = "exception1";
			workflowException.Actioned = true;
			workflowException.Date = ZDateTime.Now;
			workflowException.ActionedDate = ZDateTime.Now.AddDays(-5);
			workflowException.EndDate = ZDateTime.Now.AddDays(2);
			workflowExceptionList.Add(workflowException);

			exceptionCollectionParent.SetExceptionCollection(() => workflowExceptionList);

			reader.PopulateExceptions(exceptionCollectionParent, shipmentBO, new TestErrorLogger(), universalObjectFactory);
			var exceptions = ((IWorkflowProvider)shipmentBO).WorkflowItems.Exceptions;

			AssertEquals(1, exceptions.Count);

			var exception1 = exceptions[0];
			AssertEquals("Date", ZDateTimeOffset.Now, exception1.P9_ActualDateOffset);
			AssertEquals("Actioned Date", ZDateTimeOffset.Now.AddDays(-5), exception1.P9_CompletedTime);
			AssertEquals("Exception End Date", ZDateTimeOffset.Now.AddDays(+2), exception1.P9_ExceptionEndDate);
		}

		[TestUtcOffset(5, 0, 0)]
		[TestDate(2013, 11, 10)]
		public void TestPopulateExceptionsDatesWithOffsets()
		{
			var reader = new UniversalExceptionReader();
			var universalObjectFactory = new UniversalObjectFactory();
			var shipmentBO = universalObjectFactory.BOFactory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var exceptionCollectionParent = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var workflowExceptionList = new List<WorkflowException>();

			var workflowException = new WorkflowException();
			workflowException.Description = "exception1";
			workflowException.Actioned = true;
			workflowException.Date = new ZDateTimeOffset(2015, 10, 10, 0, 0, 0, new TimeSpan(10, 0, 0));
			workflowException.ActionedDate = new ZDateTimeOffset(2015, 10, 10, 0, 0, 0, new TimeSpan(2, 0, 0));
			workflowException.EndDate = new ZDateTimeOffset(2015, 10, 10, 0, 0, 0, new TimeSpan(8, 0, 0));
			workflowExceptionList.Add(workflowException);

			exceptionCollectionParent.SetExceptionCollection(() => workflowExceptionList);

			reader.PopulateExceptions(exceptionCollectionParent, shipmentBO, new TestErrorLogger(), universalObjectFactory);
			var exceptions = ((IWorkflowProvider)shipmentBO).WorkflowItems.Exceptions;

			AssertEquals(1, exceptions.Count);

			var exception1 = exceptions[0];
			AssertEquals("Date", workflowException.Date, exception1.P9_ActualDateOffset);
			AssertEquals("Actioned Date", workflowException.ActionedDate, exception1.P9_CompletedTime);
			AssertEquals("Exception End Date", workflowException.EndDate, exception1.P9_ExceptionEndDate);
		}
	}
}
