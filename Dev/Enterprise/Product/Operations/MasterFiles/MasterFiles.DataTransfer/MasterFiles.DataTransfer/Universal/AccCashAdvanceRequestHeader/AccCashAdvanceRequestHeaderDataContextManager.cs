using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.DataTransfer.Universal.AccCashAdvanceRequestMessageConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AccCashAdvanceRequestHeaderDataContextManager : EventDataContextManager<AccCashAdvanceRequestHeader>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType => DataContextType.AccCashAdvanceRequest;

		public override ZString DataContextKey => ParentBO.CAH_RequestReferenceNumber;

		public override string DefaultOutputDirectory => null;

		public bool ManagesTransactions => false;

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject is AccCashAdvanceRequestHeader parent)
			{
				var eventObject = (UniversalEvent)eventDataObject;
				if (eventObject.ContextCollection == null)
				{
					logger.LogBoth(LogType.Error, (NoResString)"Event does not contain a Context Collection.");
					return;
				}

				if (!eventObject.EventType.HasValue || eventObject.EventType.Value != Events.StatusUpdatedCode)
				{
					logger.LogBoth(LogType.Error, $"Unexpected Event Type encountered: {eventObject.EventType}.");
					return;
				}

				if (!CheckForMissingDuplicateOrEmptyFields(logger, eventObject, out var fieldValues))
				{
					var ledger = fieldValues[XUEFieldNames.LedgerType];
					var cashAdvanceRequestStatus = fieldValues[XUEFieldNames.Status];

					if (ValidateLedgerType(logger, ledger, parent.CAH_RequestReferenceNumber, message) &&
						ValidateLedgerAllowManualSettingOfCashAdvanceRequest(logger, ledger) &&
						ValidateCashAdvanceRequestStatus(logger, cashAdvanceRequestStatus) &&
						ValidateIfCanBeMarkedAsPaid(logger, parent))
					{
						var result = parent.MarkAsPaid();

						if (!result.IsSuccessful)
						{
							logger.LogBoth(LogType.Error, result.ErrorMessage);
						}
					}
				}
			}
			else
			{
				var errorMessage = $"Unable to process - unsupported type detected, Parent BO type is {businessObject.GetType()}";

				ErrorReporter.ReportOnce(errorMessage);
				logger.LogBoth(LogType.Error, errorMessage);
				return;
			}
		}

		bool ValidateIfCanBeMarkedAsPaid(IXmlSessionTracker logger, AccCashAdvanceRequestHeader accCashAdvanceRequestHeader)
		{
			if (!accCashAdvanceRequestHeader?.CanBeMarkedAsPaid ?? false)
			{
				logger.LogBoth(LogType.Error, (NoResString)"Payment Status can be updated only when Advance Payment is in Requested or Partially Paid status.");
				return false;
			}
			else
			{
				return true;
			}
		}

		bool ValidateCashAdvanceRequestStatus(IXmlSessionTracker logger, string cashAdvanceRequestStatus)
		{
			if (cashAdvanceRequestStatus == CashAdvanceStatusCodes.RequestHeader.Paid)
			{
				return true;
			}
			else
			{
				logger.LogBoth(LogType.Error, $"Unexpected Advance Payment Request Status {cashAdvanceRequestStatus} encountered.");
				return false;
			}
		}

		bool ValidateLedgerType(IXmlSessionTracker logger, string ledgerType, string referenceNumber, IEDIMessage message)
		{
			if (ledgerType == LedgerTypes.AccountsReceivable || ledgerType == LedgerTypes.AccountsPayable)
			{
				return true;
			}
			else
			{
				logger.LogBoth(LogType.Error, $"{ledgerType} Advance Payment {referenceNumber} cannot be found in company [{message?.Branch?.Company?.Code}]");
				return false;
			}
		}

		bool ValidateLedgerAllowManualSettingOfCashAdvanceRequest(IXmlSessionTracker logger, string ledgerType)
		{
			var accCashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();

			if ((ledgerType == LedgerTypes.AccountsPayable && !accCashAdvanceFunctionalityChecker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed) ||
				(ledgerType == LedgerTypes.AccountsReceivable && !accCashAdvanceFunctionalityChecker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed))
			{
				logger.LogBoth(LogType.Error, $"{ledgerType} manual setting of Advance Payment request status to paid is not Permitted in the Registry");
				return false;
			}
			else
			{
				return true;
			}
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(matchingValues, nameof(matchingValues));
			Argument.NotNull(factory, nameof(factory));
			var cashAdvanceReferenceNumber = matchingValues.Key;
			if (cashAdvanceReferenceNumber.IsEmpty || !(matchingValues.DataObject is UniversalEvent universalEvent))
			{
				return ZQuery.NoResultQuery;
			}

			var companyCode = universalEvent.DataContext.CompanyCodeToImportInto;
			GlbCompany company = null;

			if (companyCode.IsEmpty)
			{
				return ZQuery.NoResultQuery;
			}
			company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);

			var query = new ZQuery(AccCashAdvanceRequestHeaderSchema.CAH_RequestReferenceNumber, cashAdvanceReferenceNumber);
			query.AddToFilter(new ZQuery(AccCashAdvanceRequestHeaderSchema.CAH_GC_Company, company?.PK ?? ZGuid.Empty));

			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new AccCashAdvanceRequestHeaderEventParentFinder(factory, this, logger);

		bool CheckForMissingDuplicateOrEmptyFields(IXmlSessionTracker logger, UniversalEvent eventObject, out Dictionary<ZString, ZString> fieldValues)
		{
			var isMissingOrDuplicateFields = false;
			var eventType = eventObject.EventType.GetValueOrDefault();
			fieldValues = new Dictionary<ZString, ZString>();

			foreach (var context in eventObject.ContextCollection.Where(x => x.Type.Type.HasValue && x.Value.HasValue && !x.Value.Value.IsEmpty))
			{
				if (fieldValues.ContainsKey(context.Type.Type.Value))
				{
					logger.LogBoth(LogType.Error, FormattableString.Invariant($"Duplicate context <Type> {context.Type.Type.Value} found."));
					isMissingOrDuplicateFields = true;
				}
				else
				{
					fieldValues.Add(context.Type.Type.Value, context.Value.Value);
				}
			}

			var missingOrEmptyFields = XUEFieldNames.GetRequiredFields().Except(fieldValues.Keys);
			if (missingOrEmptyFields.Any())
			{
				logger.LogBoth(LogType.Error, $@"The following Context fields are either empty or missing:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, missingOrEmptyFields)}");
				isMissingOrDuplicateFields = true;
			}
			return isMissingOrDuplicateFields;
		}
	}
}
