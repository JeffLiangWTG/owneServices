using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobService))]
	sealed class JobServiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestES_SubLocation_Caption()
		{
			AssertEquals("Sub Location", DataBoundResourceStrings.GetDataForProperty(Service.ES_SubLocationInfo).Caption);
		}

		#region Service Event

		public void TestCreateOrUpdateServiceEvent_OnSave()
		{
			var container = (BusinessObject)Factory.New<Forwarding.IForwardingContainer>();
			container.FillWithValidTestData();

			var now = ZDateTime.Now;

			var service = CreateServiceAndSave((IHaveServices)container, now, now.AddDays(10));
			var logsCollection = new List<Logs> { service.Logs, ((IStmALogParent)container).Logs };

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { "SVC", "SVR" });
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			foreach (var logs in logsCollection)
			{
				var filteredLogs = logs.Find(query);
				var svrLog = filteredLogs.First(c => c.SL_SE_NKEvent == "SVR");
				var svcLog = filteredLogs.First(c => c.SL_SE_NKEvent == "SVC");

				AssertNotNull("Should exist at least one log with SVR event", svrLog);
				AssertNotNull("Should exist at least one log with SVC event", svcLog);

				AssertEquals(now, svrLog.SL_EventTime);
				AssertEquals(now.AddDays(10), svcLog.SL_EventTime);

				AssertEquals("|TYP=FUM", svrLog.SL_Reference);
				AssertEquals("|TYP=FUM", svcLog.SL_Reference);
			}

			service.ES_Booked = now.AddDays(1);
			Factory.Save();

			foreach (var logs in logsCollection)
			{
				var filteredLogs = logs.Find(query);
				var svrLog = filteredLogs.First(c => c.SL_SE_NKEvent == "SVR");
				AssertEquals("Should update the event date on the SVR log", now.AddDays(1), svrLog.SL_EventTime);
			}

			service.ES_Completed = now.AddDays(15);
			Factory.Save();

			foreach (var logs in logsCollection)
			{
				var filteredLogs = logs.Find(query);
				var svcLog = filteredLogs.First(c => c.SL_SE_NKEvent == "SVC");
				AssertEquals("Should update the event date on the SVC log", now.AddDays(15), svcLog.SL_EventTime);
			}

			service.ES_ServiceCode = "CLN";
			Factory.Save();

			foreach (var logs in logsCollection)
			{
				var filteredLogs = logs.Find(query);
				AssertEquals(2, filteredLogs.Length);

				var svrLog = filteredLogs.First(c => c.SL_SE_NKEvent == "SVR");
				var svcLog = filteredLogs.First(c => c.SL_SE_NKEvent == "SVC");
				AssertEquals("Should create a new SVR log with the CLN Type Parameter", "|TYP=CLN", svrLog.SL_Reference);
				AssertEquals("Should create a new SVC log with the CLN Type Parameter", "|TYP=CLN", svcLog.SL_Reference);
			}
		}

		public void TestCancelOrDeleteServiceEvent_OnDelete()
		{
			var container = (BusinessObject)Factory.New<Forwarding.IForwardingContainer>();
			container.FillWithValidTestData();

			var now = ZDateTime.Now;

			var service = CreateServiceAndSave((IHaveServices)container, now, now.AddDays(10));

			var logs = ((IStmALogParent)container).Logs;
			Assert("Should exist one log with SVR event and not cancelled", logs.HasLogWith(c => !c.SL_IsCancelled && c.SL_SE_NKEvent == "SVR"));
			Assert("Should exist one log with SVC event and not cancelled", logs.HasLogWith(c => !c.SL_IsCancelled && c.SL_SE_NKEvent == "SVC"));

			service.Delete();
			Factory.Save();

			logs = ((IStmALogParent)container).Logs;
			Assert("Should cancel the SVR log", logs.HasLogWith(c => c.SL_IsCancelled && c.SL_SE_NKEvent == "SVR"));
			Assert("Should cancel the SVC log", logs.HasLogWith(c => c.SL_IsCancelled && c.SL_SE_NKEvent == "SVC"));
		}

		public void TestInvokeJobServiceDeletedOnParent_OnDelete()
		{
			var mockRCN = new Mock<IHaveServices>();
			var parent = mockRCN.Object;

			var now = ZDateTime.Now;

			JobService service = Factory.New<JobService>();
			service.Parent = parent;
			service.ES_Booked = now;
			service.ES_Completed = now.AddDays(10);

			Factory.Save();

			service.Delete();
			Factory.Save();

			mockRCN.Verify(x => x.JobServiceDeleted(It.IsAny<ZGuid>()), Times.Once);
			Assert(true);
		}

		JobService CreateServiceAndSave(IHaveServices parent, ZDateTime bookedDate, ZDateTime completedDate)
		{
			var service = parent.Services.AddNew();
			service.ES_Booked = bookedDate;
			service.ES_Completed = completedDate;
			service.ES_ServiceCode = "FUM";

			Factory.Save();

			return service;
		}

		public void TestCreateOrUpdateServiceEvent_NullReferenceException()
		{
			var container = (BusinessObject)Factory.New<Forwarding.IForwardingContainer>();
			container.FillWithValidTestData();

			var parent = (IHaveServices)container;
			var logParent = (IStmALogParent)container;
			var bookedDate = ZDateTime.Now;
			var completedDate = ZDateTime.Now.AddDays(10);

			var service = parent.Services.AddNew();
			service.ES_Booked = bookedDate;
			service.ES_Completed = completedDate;
			service.ES_ServiceCode = "FUM";

			logParent.Logs.eventsInTheProcessOfBeingAdded.Push("SVR");
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestCreateOrUpdateServiceEvent_LogDeletedAfterConcurrencyException_WI00556331()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			(declaration as BusinessObject).FillWithValidTestData();
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			declaration.JE_MessageType = "EXP";
			Factory.Save();

			var parent = (IHaveServices)declaration["DocsAndCartage"];
			var service = parent.Services.AddNew();
			service.ES_Booked = ZDateTime.Now;
			service.ES_Completed = ZDateTime.Now.AddDays(10);
			service.ES_ServiceCode = "CLN";

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var declarationIOF = factory2.Load<Enterprise.Integration.Customs.AU.IJobDeclaration>(declaration.PK);
			declarationIOF.JE_MessageType = "EXD";
			factory2.Save();

			declaration.JE_MessageType = "IMP";
			AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());

			(declaration as BusinessObject).CancelChanges();
			var allLogs = (declaration as IStmALogParent).Logs.GetAllLogs();
			var svcLog = allLogs.Cast<StmALog>().FirstOrDefault(c => c.SL_SE_NKEvent == "SVC");
			svcLog.Delete();

			AssertNoExceptionThrown("Does not throw System.Data.RowNotInTableException", () => Factory.Save());
		}

		#region IHaveEventsFromServices Parent

		public void TestCreateOrUpdateServiceEvent_OnSave_IHaveEventsFromServicesParent()
		{
			var parent = (BusinessObject)Factory.New<ITransitReceiveConsignment>();
			parent.FillWithValidTestData();

			var now = ZDateTime.Now;

			var service = ((IHaveEventsFromServices)parent).Services.AddNew();
			service.ES_Booked = now;
			service.ES_Completed = now.AddDays(10);
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.ServiceCompletedCode, AutoEvents.ServiceRequestedCode });
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var logs = ((IStmALogParent)parent).Logs.Find(query);
			var svrLog = logs.First(c => c.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode);
			var svcLog = logs.First(c => c.SL_SE_NKEvent == AutoEvents.ServiceCompletedCode);

			AssertNotNull("Should exist at least one log with SVR event", svrLog);
			AssertNotNull("Should exist at least one log with SVC event", svcLog);

			AssertEquals(now, svrLog.SL_EventTime);
			AssertEquals(now.AddDays(10), svcLog.SL_EventTime);

			AssertEquals($"|FAC={CargoWise.EventReference.Constants.Facilities.Code.Depot}|TYP={Constants.FreightServiceType.Codes.Fumigation}", svrLog.SL_Reference);
			AssertEquals($"|FAC={CargoWise.EventReference.Constants.Facilities.Code.Depot}|TYP={Constants.FreightServiceType.Codes.Fumigation}", svcLog.SL_Reference);

			service.ES_Booked = now.AddDays(1);
			Factory.Save();

			logs = ((IStmALogParent)parent).Logs.Find(query);
			svrLog = logs.First(c => c.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode);
			AssertEquals("Should update the event date on the SVR log", now.AddDays(1), svrLog.SL_EventTime);

			service.ES_Completed = now.AddDays(15);
			Factory.Save();

			logs = ((IStmALogParent)parent).Logs.Find(query);
			svcLog = logs.First(c => c.SL_SE_NKEvent == AutoEvents.ServiceCompletedCode);
			AssertEquals("Should update the event date on the SVC log", now.AddDays(15), svcLog.SL_EventTime);

			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			Factory.Save();

			logs = ((IStmALogParent)parent).Logs.Find(query);
			AssertEquals(2, logs.Length);

			svrLog = logs.First(c => c.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode);
			svcLog = logs.First(c => c.SL_SE_NKEvent == AutoEvents.ServiceCompletedCode);
			AssertEquals("Should create a new SVR log with the CLN Type Parameter", $"|FAC={CargoWise.EventReference.Constants.Facilities.Code.Depot}|TYP={Constants.FreightServiceType.Codes.Cleaning}", svrLog.SL_Reference);
			AssertEquals("Should create a new SVC log with the CLN Type Parameter", $"|FAC={CargoWise.EventReference.Constants.Facilities.Code.Depot}|TYP={Constants.FreightServiceType.Codes.Cleaning}", svcLog.SL_Reference);
		}

		public void TestCancelOrDeleteServiceEvent_OnDelete_IHaveEventsFromServicesParent()
		{
			var parent = (BusinessObject)Factory.New<ITransitReceiveConsignment>();
			parent.FillWithValidTestData();

			var now = ZDateTime.Now;

			var service = CreateServiceAndSave((IHaveEventsFromServices)parent, now, now.AddDays(10));

			var logs = ((IStmALogParent)parent).Logs;
			Assert("Should exist one log with SVR event and not cancelled", logs.HasLogWith(c => !c.SL_IsCancelled && c.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode));
			Assert("Should exist one log with SVC event and not cancelled", logs.HasLogWith(c => !c.SL_IsCancelled && c.SL_SE_NKEvent == AutoEvents.ServiceCompletedCode));

			service.Delete();
			Factory.Save();

			logs = ((IStmALogParent)parent).Logs;
			Assert("Should cancel the SVR log", logs.HasLogWith(c => c.SL_IsCancelled && c.SL_SE_NKEvent == AutoEvents.ServiceRequestedCode));
			Assert("Should cancel the SVC log", logs.HasLogWith(c => c.SL_IsCancelled && c.SL_SE_NKEvent == AutoEvents.ServiceCompletedCode));
		}

		#endregion

		#endregion

		#region TestRequestForServiceParent

		public void TestRequestForServiceParent()
		{
			JobService orphan = Factory.New<JobService>();
			AssertEquals("Expect parent of orphan JobService to be null", null, orphan.Parent);
			AssertEquals("Request for service parent should be the DummyWithServices", Dummy.PK, Service.RequestForServiceParent.PK);
		}

		#endregion

		#region TestSetToDefaultContractor

		public void TestSetToDefaultContractor()
		{
			Dummy.TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Dummy.ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			Service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Service.SetToDefaultContractor();
			AssertEquals("Expect contractor to be pulled from registry", FreightDataRegistry.Instance.FCLFumigationContractor.Value, Service.ES_OH_Contractor);
		}

		#endregion

		#region TestES_Calc_Description

		public void TestES_Calc_Description()
		{
			AssertEquals("Precondition - service code is not set", "", Service.ES_ServiceCode);
			AssertEquals("Service description should be blank", "", Service.ES_Calc_Description);

			Service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			AssertEquals("Service description should match service code", "Fumigation", Service.ES_Calc_Description);

			Service.ES_ServiceCode = "XXX";
			AssertEquals("", Service.ES_Calc_Description);
		}

		#endregion

		#region TestParentContextID

		public void TestParentContextID()
		{
			AssertEquals("Precondition - ES_CurrentContextID is not set.", ZGuid.Empty, Service.ES_CurrentContextID);
			AssertEquals("Precondition - ParentContextID is equal to parent.pk.", Service.Parent.PK, Service.ParentContextID);

			var contextID = new ZGuid();
			Service.ParentContextID = contextID;

			AssertEquals(ZGuid.Empty, Service.ES_CurrentContextID);
			AssertEquals("", Service.ES_CurrentContextTableCode);
		}

		public void TestParentContextID_DummyWithContext()
		{
			var dummyWithContext = Factory.New<DummyWithServicesWithContext>();
			var serviceWithContext = dummyWithContext.Services.AddNew();
			Factory.Save();

			AssertEquals("Precondition - ES_CurrentContextID is not set.", ZGuid.Empty, serviceWithContext.ES_CurrentContextID);
			AssertEquals("Precondition - ParentContextID is equal to parent.pk.", serviceWithContext.Parent.PK, serviceWithContext.ParentContextID);

			var contextID = ZGuid.NewZGuid();
			serviceWithContext.ParentContextID = contextID;
			AssertEquals(contextID, serviceWithContext.ES_CurrentContextID);
			AssertEquals("DUM", serviceWithContext.ES_CurrentContextTableCode);

			serviceWithContext.ParentContextID = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, serviceWithContext.ES_CurrentContextID);
			AssertEquals("", serviceWithContext.ES_CurrentContextTableCode);
		}

		#endregion

		#region TestGetTypeFromPrefix

		public void TestGetTypeFromPrefix()
		{
			var service = Factory.New<JobServiceWithExposedHashTable>();
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.Freight.IJobDocsAndCartage>(), service.GetTypeFromPrefix(JobDocsAndCartageSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonContainer>(), service.GetTypeFromPrefix(JobContainerSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(), service.GetTypeFromPrefix(CusContainerSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<ProcessManagement.Integration.IWorkItem>(), service.GetTypeFromPrefix(WorkItemSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Warehouse.Integration.IWhsDocket>(), service.GetTypeFromPrefix(WhsDocketSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Freight.LocalCartage.Integration.ICommonBookedCtgMove>(), service.GetTypeFromPrefix(JobBookedCtgMoveSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.TransportConsignment.IDtbBookingConsignment>(), service.GetTypeFromPrefix(DtbBookingSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Warehouse.Integration.IWhsAdHocServiceJob>(), service.GetTypeFromPrefix(WhsAdHocServiceJobSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Warehouse.Integration.IWhsVASOrder>(), service.GetTypeFromPrefix(WhsVASOrderSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.Customs.ICusInBondHeader>(), service.GetTypeFromPrefix(CusInBondHeaderSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.LandTransport.IDtbConsignment>(), service.GetTypeFromPrefix(DtbConsignmentSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<TransportConsignment.Integration.IDtbConsignmentRunSheet>(), service.GetTypeFromPrefix(DtbConsignmentRunSheetSchema.Constants.Prefix));
			AssertGetTypeFromPrefix(ObjectFactory.GetType<Enterprise.Integration.ICYDAdHocServiceOrder>(), service.GetTypeFromPrefix(CYDAdHocServiceOrderSchema.Constants.Prefix));
		}

		static void AssertGetTypeFromPrefix(Type expectedType, Type typeFromService)
		{
			AssertEquals("Hashtable should return correct type", expectedType, typeFromService);
			AssertNotNull("Type should implement IHaveService.", typeFromService.IsAssignableFrom(typeof(IHaveServices)));
		}

		public void TestReportErrorWhenPrefixToTypeHashDoesNotContainPrefix()
		{
			var service = Factory.New<JobServiceWithExposedHashTable>();
			AssertExceptionThrown<KeyNotFoundException>(() => service.GetTypeFromPrefix("Test"));
			AssertEquals("PrefixToTypeHash does not contain the prefix : Test", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Test ES_OA_Location sets ES_RX_NKServiceRateCurrency

		public void TestES_OA_LocationSetsES_RX_NKServiceRateCurrency()
		{
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Service.ES_OA_Location = address.PK;

			AssertEquals("Should default from country currency", Core.Constants.CurrencyCodes.Australia, Service.ES_RX_NKServiceRateCurrency);

			Service.ES_OA_Location = ZGuid.Empty;

			AssertEquals("Currency should be cleared", ZString.Empty, Service.ES_RX_NKServiceRateCurrency);

			var chinaAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			chinaAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			Service.ES_OA_Location = chinaAddress.PK;
			Factory.Save();

			AssertEquals(Core.Constants.CurrencyCodes.China, Service.ES_RX_NKServiceRateCurrency);

			var newFactory = new BusinessObjectFactory();
			var serviceInAnotherFactory = newFactory.Load<JobService>(Service.PK);

			AssertEquals(Core.Constants.CurrencyCodes.China, serviceInAnotherFactory.ES_RX_NKServiceRateCurrency);
		}

		#endregion

		#region ES_ServiceRate

		public void TestES_ServiceRate()
		{
			var dummyRatingParent = Factory.New<DummyWithServices>();
			var service = dummyRatingParent.Services.AddNew();

			AssertEquals(false, service.ES_ServiceRateInfo.ReadOnly);

			service.ES_ServiceRate = 5m;
			AssertEquals(5m, service.ES_ServiceRate);
		}

		#endregion

		#region HaveServiceId

		public void TestHaveServiceId()
		{
			var dummyRatingParent = Factory.New<DummyWithServices>();
			var service = dummyRatingParent.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service.ES_ServiceId = "WTLKKK00000043";
			Assert("ES_ServiceId is not empty", service.HaveServiceId);

			service.ES_ServiceId = ZString.Empty;
			service.ShouldPopulateServiceId = true;
			Assert("ES_ServiceId is empty and ShouldPopulateServiceId is true before save", service.HaveServiceId);

			service.ShouldPopulateServiceId = false;
			Assert("ES_ServiceId is empty and ShouldPopulateServiceId is false", !service.HaveServiceId);

			Factory.Save();

			service.ShouldPopulateServiceId = true;
			Assert("ES_ServiceId is empty and ShouldPopulateServiceId is true after save", !service.HaveServiceId);
		}

		#endregion

		#region TestClone

		public void TestClone()
		{
			var clonedService = (JobService)Service.Clone();
			AssertEquals("Properties of cloned service should be same as original", Service.ES_ServiceCode, clonedService.ES_ServiceCode);
			AssertEquals("Properties of cloned service should be same as original", Service.ES_Completed, clonedService.ES_Completed);
			AssertEquals("Properties of cloned service should be same as original", Service.ES_ParentID, clonedService.ES_ParentID);
			AssertEquals("Properties of cloned service should be same as original", Service.ES_ParentTableCode, clonedService.ES_ParentTableCode);
			AssertEquals("ES_ServiceId of cloned service should be empty", ZString.Empty, clonedService.ES_ServiceId);
			AssertEquals("ES_ExternalServiceId of cloned service should be empty", ZString.Empty, clonedService.ES_ExternalServiceId);
		}

		#endregion

		#region TestParent

		public void TestParent()
		{
			var dummyParent = Factory.New<DummySubClassWithServices>();
			var service1 = dummyParent.Services.AddNew();
			AssertEquals(dummyParent, service1.Parent);

			service1.Parent = null;
			AssertNotEquals(dummyParent, service1.Parent);
			AssertEquals(dummyParent.PK, service1.Parent.PK);
			AssertEquals(typeof(DummyWithServices), service1.Parent.GetType());
		}

		[ExpectNoExceptions]
		public void TestParent_DtbConsignment()
		{
			var consignment = Factory.New(ObjectFactory.GetType<Enterprise.Integration.LandTransport.IDtbConsignment>()) as IHaveServices;
			AssertNotNull("Consignment should implement IHaveServices", consignment);

			var service = consignment.Services.AddNew();
			service.Parent = null;
			AssertEquals(consignment, service.Parent);
		}

		#endregion

		#region TestDatesKind_Unspecified

		public void TestDates_DateTimeKind_Unspecified()
		{
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Service.ES_Booked.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Service.ES_Completed.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, Service.ES_Duration.Kind);
		}

		#endregion

		public void TestPopulateServiceIdIfNeeded()
		{
			var service = Dummy.Services.AddNew();
			service.ES_ServiceId = ZString.Empty;
			Factory.Save();
			AssertNotEquals("Should populate service id", ZString.Empty, service.ES_ServiceId);

			service.ES_ServiceId = ZString.Empty;
			Factory.Save();
			AssertEquals("Should not populate service id for an existing service", ZString.Empty, service.ES_ServiceId);

			service = Dummy.Services.AddNew();
			service.ES_ServiceId = "WTLKKK00000043";
			Factory.Save();
			AssertEquals("Should not re-populate service id if already set", "WTLKKK00000043", service.ES_ServiceId);
		}

		public void TestES_BookedSetsES_BookedDateTimeOffset()
		{
			var service = Factory.New<JobService>();
			Assert(service.ES_Booked.IsEmpty);
			Assert(service.ES_BookedDateTimeOffset.IsEmpty);

			var dateTime = new ZDateTime(2024, 8, 22, 12, 12, 12);
			service.ES_Booked = dateTime;
			AssertEquals(dateTime.ToDateTimeOffset(null), service.ES_BookedDateTimeOffset);
		}

		public void TestES_BookedSetsES_BookedDateTimeOffset_WithParentServiceBranch()
		{
			var service = Factory.New<JobService>();
			Assert(service.ES_Booked.IsEmpty);
			Assert(service.ES_BookedDateTimeOffset.IsEmpty);

			var branchMock = new Mock<IBranch>();
			branchMock.SetupGet(b => b.NKUNLOCO).Returns("AUSYD");
			var parentMock = new Mock<IHaveServices>();
			parentMock.Setup(p => p.ServiceBranch).Returns(branchMock.Object);
			service.Parent = parentMock.Object;

			var dateTime = new ZDateTime(2024, 8, 22, 12, 12, 12);
			service.ES_Booked = dateTime;
			AssertEquals(dateTime.ToDateTimeOffset(Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))), service.ES_BookedDateTimeOffset);
		}

		public void TestES_BookedReturnsES_BookedDateTimeOffsetAsDateTime()
		{
			var service = Factory.New<JobService>();
			Assert(service.ES_Booked.IsEmpty);
			Assert(service.ES_BookedDateTimeOffset.IsEmpty);

			var dateTimeOffset = new ZDateTimeOffset(2024, 8, 22, 12, 12, 12, Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")).LocationDateTimeOffset.Offset);
			service.ES_BookedDateTimeOffset = dateTimeOffset;
			AssertEquals(new ZDateTime(2024, 8, 22, 12, 12, 12), service.ES_Booked);
		}

		public void TestES_CompletedSetsES_CompletedDateTimeOffset()
		{
			var service = Factory.New<JobService>();
			Assert(service.ES_Completed.IsEmpty);
			Assert(service.ES_CompletedDateTimeOffset.IsEmpty);

			var dateTime = new ZDateTime(2024, 8, 22, 12, 12, 12);
			service.ES_Completed = dateTime;
			AssertEquals(dateTime.ToDateTimeOffset(null), service.ES_CompletedDateTimeOffset);
		}

		public void TestES_CompletedSetsES_CompletedTimeOffset_WithParentServiceBranch()
		{
			var service = Factory.New<JobService>();
			Assert(service.ES_Booked.IsEmpty);
			Assert(service.ES_BookedDateTimeOffset.IsEmpty);

			var branchMock = new Mock<IBranch>();
			branchMock.SetupGet(b => b.NKUNLOCO).Returns("AUSYD");
			var parentMock = new Mock<IHaveServices>();
			parentMock.Setup(p => p.ServiceBranch).Returns(branchMock.Object);
			service.Parent = parentMock.Object;

			var dateTime = new ZDateTime(2024, 8, 22, 12, 12, 12);
			service.ES_Completed = dateTime;
			AssertEquals(dateTime.ToDateTimeOffset(Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))), service.ES_CompletedDateTimeOffset);
		}

		public void TestES_CompletedReturnsES_CompletedDateTimeOffsetAsDateTime()
		{
			var service = Factory.New<JobService>();
			Assert(service.ES_Completed.IsEmpty);
			Assert(service.ES_CompletedDateTimeOffset.IsEmpty);

			var dateTimeOffset = new ZDateTimeOffset(2024, 8, 22, 12, 12, 12, Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")).LocationDateTimeOffset.Offset);
			service.ES_CompletedDateTimeOffset = dateTimeOffset;
			AssertEquals(new ZDateTime(2024, 8, 22, 12, 12, 12), service.ES_Completed);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<DummyWithServices>().Services.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Dummy = Factory.New<DummyWithServices>();
			Service = Dummy.Services.AddNew();

			Factory.Save();
		}

		DummyWithServices Dummy;
		JobService Service;

		#endregion
	}
}
