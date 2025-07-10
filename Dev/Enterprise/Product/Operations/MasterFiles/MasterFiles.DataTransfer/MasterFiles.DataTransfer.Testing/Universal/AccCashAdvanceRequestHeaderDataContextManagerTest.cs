using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.DataTransfer.Universal.AccCashAdvanceRequestMessageConstants;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AccCashAdvanceRequestHeaderDataContextManager))]
	public class AccARCashAdvanceRequestHeaderDataContextManagerTest : AccCashAdvanceRequestHeaderDataContextManagerTest
	{
		protected override string LedgerType { get => LedgerTypes.AccountsReceivable; }
	}

	[TestedType(typeof(AccCashAdvanceRequestHeaderDataContextManager))]
	public class AccAPCashAdvanceRequestHeaderDataContextManagerTest : AccCashAdvanceRequestHeaderDataContextManagerTest
	{
		protected override string LedgerType { get => LedgerTypes.AccountsPayable; }
	}

	public abstract class AccCashAdvanceRequestHeaderDataContextManagerTest : DataContextManagerTestCase<AccCashAdvanceRequestHeaderDataContextManager, AccCashAdvanceRequestHeader>
	{
		protected abstract string LedgerType { get; }

		public void TestMarkAsPaid_SuccessfullFor_Universal_2011_11()
		{
			EnableCashAdvanceFunctionality();
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(false, serviceTaskLog.HasErrors());
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cashAdvanceRequestHeader.CAH_Status);
		}

		public void TestMarkAsPaid_SuccessfullFor_Universal_2012_11()
		{
			EnableCashAdvanceFunctionality();
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.");

			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(false, serviceTaskLog.HasErrors());
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Paid, cashAdvanceRequestHeader.CAH_Status);
		}

		public void TestCashAdvanceRequestRegistryNotEnabled()
		{
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			string expectedError = $@" ";

			AssertEquals("Linked Event to AccCashAdvanceRequestHeader.", importResults.Single().Logs.ElementAt(0).Message);
			AssertEquals($"{LedgerType} manual setting of Advance Payment request status to paid is not Permitted in the Registry", importResults.Single().Logs.ElementAt(1).Message);

			Assert(serviceTaskLog.HasErrors());

			AssertEquals("Linked Event to AccCashAdvanceRequestHeader.", serviceTaskLog.Logs.ElementAt(0).Message);
			AssertEquals($"{LedgerType} manual setting of Advance Payment request status to paid is not Permitted in the Registry", serviceTaskLog.Logs.ElementAt(1).Message);
		}

		public void TestMissingCashAdvanceRequestReference()
		{
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, string.Empty);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			_ = manager.Process(message).ImportResults;

			AssertEquals(
@"ERROR - Could not find a valid Advance Payment Reference Number in the <DataTarget> Key parameter. Value found: ''
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestMissingCompanyCode()
		{
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid, false);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			_ = manager.Process(message).ImportResults;

			Assert(serviceTaskLog.HasErrors());
			AssertEquals(
$@"ERROR - Missing or Empty field {XUEFieldNames.Company} in Universal Event.
Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
		}

		public void TestIncorrectEventType()
		{
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);
			eventDataObject.EventType = Events.InterchangeAcknowledgedCode;

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = $"Unexpected Event Type encountered: {eventDataObject.EventType}.";

			Assert(serviceTaskLog.HasErrors());
			AssertEquals($@"Linked Event to AccCashAdvanceRequestHeader.
ERROR - {expectedError}", serviceTaskLog.ToString());

			AssertEquals($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.
Error - {expectedError}", importResults.Single().ToString());
		}

		public void TestMissingContexts()
		{
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = @"The following Context fields are either empty or missing:
LedgerType
Status";

			AssertEquals($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.
Error - {expectedError}", importResults.Single().ToString());
			Assert(serviceTaskLog.HasErrors());

			AssertEquals($@"Linked Event to AccCashAdvanceRequestHeader.
ERROR - {expectedError}", serviceTaskLog.ToString());
		}

		public void TestCashAdvanceRequestStatusNotSetToRequestedOrPartiallyPaid()
		{
			EnableCashAdvanceFunctionality();
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			List<ZString> statusList = new List<ZString>() { CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceStatusCodes.RequestHeader.Paid, CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, CashAdvanceStatusCodes.RequestHeader.Cancelled };

			foreach (string status in statusList)
			{
				cashAdvanceRequestHeader.CAH_Status = status;
				Factory.SaveForTesting();

				var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

				AssertEquals(status, cashAdvanceRequestHeader.CAH_Status);

				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;

				var expectedError = @"Payment Status can be updated only when Advance Payment is in Requested or Partially Paid status.";
				AssertEquals($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.
Error - {expectedError}", importResults.Single().ToString());

				Assert(serviceTaskLog.HasErrors());
				AssertEquals($@"Linked Event to AccCashAdvanceRequestHeader.
ERROR - {expectedError}", serviceTaskLog.ToString());
			}
		}

		public void TestIncorrectStatusInXml()
		{
			EnableCashAdvanceFunctionality();
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();
			Factory.SaveForTesting();

			List<ZString> statusList = new List<ZString>() { CashAdvanceStatusCodes.RequestHeader.Invoiced, CashAdvanceStatusCodes.RequestHeader.Requested, CashAdvanceStatusCodes.RequestHeader.Pending, CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, CashAdvanceStatusCodes.RequestHeader.Cancelled };

			foreach (string status in statusList)
			{
				var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, status);
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

				AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;

				var expectedError = FormattableString.Invariant($"Unexpected Advance Payment Request Status {status} encountered.");
				AssertEquals($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.
Error - {expectedError}", importResults.Single().ToString());

				Assert(serviceTaskLog.HasErrors());
				AssertEquals($@"Linked Event to AccCashAdvanceRequestHeader.
ERROR - {expectedError}", serviceTaskLog.ToString());
			}
		}

		public void TestIncorrectData()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);

			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();

			//Incorrect ledger
			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerTypes.General, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = FormattableString.Invariant($@"{LedgerTypes.General} Advance Payment {cashAdvanceRequestHeader.CAH_RequestReferenceNumber} cannot be found in company [EDI]");

			AssertEquals($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.
Error - {expectedError}", importResults.Single().ToString());

			Assert(serviceTaskLog.HasErrors());
			AssertEquals($@"Linked Event to AccCashAdvanceRequestHeader.
ERROR - {expectedError}", serviceTaskLog.ToString());

			serviceTaskLog.ClearLogs();

			//Incorrect Advance Payment number
			var newCashAdvanceReferenceNumber = "XXXXXXXXXX";
			eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, newCashAdvanceReferenceNumber);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

			message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			importResults = manager.Process(message).ImportResults;

			AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			serviceTaskLog.ClearLogs();

			//Incorrect Company
			eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~CO";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			importResults = manager.Process(message).ImportResults;

			AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", serviceTaskLog.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			serviceTaskLog.ClearLogs();
		}

		public void TestDuplicateContextType()
		{
			EnableCashAdvanceFunctionality();
			var cashAdvanceRequestHeader = GetNewBusinessObjectForTesting();

			Factory.SaveForTesting();

			var eventDataObject = GetNewIncomingMessage(new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext(), true, LedgerType, CashAdvanceStatusCodes.RequestHeader.Paid);
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccCashAdvanceRequest, cashAdvanceRequestHeader.CAH_RequestReferenceNumber);

			//Add duplicate context type
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = LedgerType });

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, cashAdvanceRequestHeader.CAH_Status);

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedError = FormattableString.Invariant($"Duplicate context <Type> {XUEFieldNames.LedgerType} found.");

			AssertEquals($@"Linked Event to {cashAdvanceRequestHeader.HumanReadableName}.
Error - {expectedError}", importResults.Single().ToString());

			Assert(serviceTaskLog.HasErrors());
			AssertEquals($@"Linked Event to AccCashAdvanceRequestHeader.
ERROR - {expectedError}", serviceTaskLog.ToString());

			serviceTaskLog.ClearLogs();
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		protected override AccCashAdvanceRequestHeader GetNewBusinessObjectForTesting()
		{
			var cashAdvanceRequestHeader = Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>();
			cashAdvanceRequestHeader.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			cashAdvanceRequestHeader.CAH_OSAmount = cashAdvanceRequestHeader.CAH_LocalAmount = 100m;
			cashAdvanceRequestHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			cashAdvanceRequestHeader.CAH_Ledger = LedgerType;
			var line = cashAdvanceRequestHeader.Lines.AddNew();
			line.CAL_CAH_RequestHeader = cashAdvanceRequestHeader.PK;
			line.CAL_GC_Company = cashAdvanceRequestHeader.CAH_GC_Company;

			return cashAdvanceRequestHeader;
		}

		void EnableCashAdvanceFunctionality()
		{
			var cashAdvanceFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();

			switch (LedgerType)
			{
				case LedgerTypes.AccountsPayable:
					cashAdvanceFunctionalityChecker.Setup(c => c.IsPayablesCashAdvanceFunctionalityEnabled).Returns(true);
					cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
					break;

				case LedgerTypes.AccountsReceivable:
					cashAdvanceFunctionalityChecker.Setup(c => c.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
					cashAdvanceFunctionalityChecker.Setup(c => c.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed).Returns(true);
					break;
			}

			ObjectFactory.Substitute(cashAdvanceFunctionalityChecker.Object);
		}

		UniversalEventDataObject GetNewIncomingMessage(IDataContextDataObject dataContext, bool populateRequiedFields = false, string ledger = "", string status = "", bool populateCompanyField = true)
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = dataContext;
			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.EventType = Events.StatusUpdatedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now.AddHours(-1);
			if (populateCompanyField)
			{
				eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			}

			if (populateRequiedFields)
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.LedgerType }, Value = ledger });
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = XUEFieldNames.Status }, Value = status });
				eventDataObject.IsEstimate = true;
			}
			return eventDataObject;
		}
	}
}
