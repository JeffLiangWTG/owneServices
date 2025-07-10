using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsLogEventParent : IStmALogParent
	{
		ZString EventFreeTextReference { get; }
		string EventReferenceParameterType { get; }
		WhsWarehouse Warehouse { get; }
	}

	public static class IWhsLogEventParentExtentions
	{
		public static void AddEvents(this IWhsLogEventParent bizO, Event eventToLog)
		{
			var reference = bizO.EventFreeTextReference;
			var eventReferenceParameterType = bizO.EventReferenceParameterType;

			bizO.AddEvents(eventToLog, reference, eventReferenceParameterType);
		}

		public static void AddEvents(this IWhsLogEventParent bizO, Event eventToLog, ZString reference, string eventReferenceParameterType)
		{
			var warehouse = bizO.Warehouse;
			var location = warehouse != null && warehouse.WarehouseAddress != null ? warehouse.WarehouseAddress.OA_City : ZString.Empty;

			bizO.Logs.AddNew(eventToLog, reference,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventReferenceParameterType),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, location));
		}
	}
}
