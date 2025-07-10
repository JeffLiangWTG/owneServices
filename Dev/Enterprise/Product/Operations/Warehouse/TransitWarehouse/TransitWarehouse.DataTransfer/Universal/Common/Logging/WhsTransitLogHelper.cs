using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class WhsTransitLogHelper
	{
		public static ZString GetCommaSeparatedStringValuesFromQuery(UniversalObjectFactory factory, ZString tableName, ZString columnName, ZQuery query)
		{
			var rows = factory.RowFactory.Load(tableName, query);
			return string.Join(", ", rows.Select(bo => bo[columnName].ToString()).OrderBy(id => id));
		}

		public static void AddStmALogToBizoObject<T>(T bizoObject, Event eventType, string eventReference, ZDateTimeOffset? eventTime = null) where T : EnterpriseBusinessObject
		{
			bizoObject.Logs.AddNew (new EventValue(
				eventType,
				eventTime: eventTime ?? ZDateTimeOffset.Now,
				reference: eventReference,
				isEstimate: false));
		}

		public static void AddStmALog(UniversalObjectFactory factory, IXmlImportLogger logger, Guid parentPK, string parentTableName, string reference, string eventCode)
		{
			var row = factory.RowFactory.NewRowWithPK(StmALogSchema.Instance);
			row.SetValue(StmALogSchema.SL_EventTime, ZDateTime.Now, logger);
			row.SetValue(StmALogSchema.SL_EventTimeUtc, ZDateTime.UtcNow, logger);
			row.SetValue(StmALogSchema.SL_FireWorkflow, ZBool.True, logger);
			row.SetValue(StmALogSchema.SL_GB_NKBranch, Env.Instance.CurrentBranch.Code, logger);
			row.SetValue(StmALogSchema.SL_GE_NKDepartment, Env.Instance.CurrentDepartment.Code, logger);
			row.SetValue(StmALogSchema.SL_GS_NKUser, (NoResString)"~AD", logger); // User code for service task
			row.SetValue(StmALogSchema.SL_Parent, parentPK, logger);
			row.SetValue(StmALogSchema.SL_PostedTimeUtc, ZDateTime.UtcNow, logger);
			row.SetValue(StmALogSchema.SL_SE_NKEvent, eventCode, logger);
			row.SetValue(StmALogSchema.SL_Table, parentTableName, logger);
			row.SetValue(StmALogSchema.SL_Reference, reference, logger);
		}

		public static string GetEventReferenceString(params KeyValuePair<string, string>[] eventCodeAndValues)
		{
			if (eventCodeAndValues == null)
			{
				throw new ArgumentNullException(nameof(eventCodeAndValues));
			}

			var stringBuilder = new StringBuilder();
			foreach (var eventCodeAndValue in eventCodeAndValues)
			{
				if (!string.IsNullOrEmpty(eventCodeAndValue.Value))
				{
					stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "|{0}={1}", eventCodeAndValue.Key, eventCodeAndValue.Value));
				}
			}
			return stringBuilder.ToString();
		}

		public static void LogRecipientRole(UniversalShipment dataObject, IXmlImportLogger logger)
		{
			var recipientRole = dataObject.DataContext?.RecipientRoleCollection?.FirstOrDefault();

			if (recipientRole != null)
			{
				logger.Log(LogType.Information, Res.GetString("551ad075-2a55-4ed8-aeec-79b09fead742", "Recipient Role Service is '{0} - {1}'.", recipientRole.Code, recipientRole.Description));
			}
		}

		public static void LogStartOfReceiveInstruction(IXmlImportLogger logger)
			=> logger.Log(LogType.Information, Res.GetString("80fb4b83-8422-4872-94d0-a79dfbd7d962", "================================Processing Receive Instruction================================"));

		public static void LogStartOfDispatchInstruction(IXmlImportLogger logger)
			=> logger.Log(LogType.Information, Res.GetString("18f674c9-a5ee-4928-a9ca-b5d543e7651e", "================================Processing Dispatch Instruction================================"));

		public static ZString FormatCodeDescriptionForLogging(ZString code, ZString? description)
			=> !string.IsNullOrEmpty(description) ? Res.GetString("bcb87455-b7bf-4354-9090-f5267e04a63a", "'{0} - {1}'", code, description) : Res.GetString("55a85f00-0c90-4be0-b94b-447e5066efdc", "'{0}'", code);

		#region LogBookingConfirmed

		public static void LogBookingConfirmed(WhsItemReceiveTransportationUnit rtu, ZString gateBookingReferenceType, ZString gateBookingReferenceNumber)
		{
			LogBookingConfirmed(rtu, rtu.Warehouse, rtu.WRH_ReferenceNumber, gateBookingReferenceType, gateBookingReferenceNumber);
		}

		public static void LogBookingConfirmed(WhsItemDispatchTransportationUnit dtu, ZString gateBookingReferenceType, ZString gateBookingReferenceNumber)
		{
			LogBookingConfirmed(dtu, dtu.Warehouse, dtu.WDH_ReferenceNumber, gateBookingReferenceType, gateBookingReferenceNumber);
		}

		static void LogBookingConfirmed<T>(T entity, WhsWarehouse warehouse, ZString entityReference, ZString gateBookingReferenceType, ZString gateBookingReferenceNumber) where T : IStmALogProvider
		{
			var warehouseCode = warehouse.WW_WarehouseCode;
			var location = warehouse.WarehouseAddress != null ? (string)warehouse.WarehouseAddress.OA_City : string.Empty;
			entity.Logs.AddNew(AutoEvents.BookingConfirmed, entityReference, ZDateTimeOffset.Now.ToDateTime(),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, entity.GetType().Name),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Facility, Facilities.Code.Warehouse),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Location, location),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Warehouse, warehouseCode),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, gateBookingReferenceType),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, gateBookingReferenceNumber));
		}

		#endregion
	}
}
