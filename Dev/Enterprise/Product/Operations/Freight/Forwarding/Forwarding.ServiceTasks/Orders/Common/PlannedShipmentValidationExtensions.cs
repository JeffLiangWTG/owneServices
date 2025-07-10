using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	static class PlannedShipmentValidationExtensions
	{
		public static bool ValidateShipmentConsolLink(this JobSupplierBooking booking)
		{
			return ValidateShipmentConsolLink(string.Format(SqlQueryTemplate, JobSupplierBookingLine.Schema.JSL_JSB_Booking), booking.PK);
		}

		public static bool ValidateShipmentConsolLink(this CommonContainerLoadList containerLoadList)
		{
			return ValidateShipmentConsolLink(string.Format(SqlQueryTemplate, ContainerLoadListLine.Schema.CLL_CLH_LoadListHeader), containerLoadList.PK);
		}

		static bool ValidateShipmentConsolLink(string sqlText, ZGuid headerPK)
		{
			return null == Db.Connection.ExecuteScalar(
				sqlText,
				command =>
				{
					command.AddParameter("@headerPK", System.Data.SqlDbType.UniqueIdentifier, headerPK.ToGuid());
				});
		}

		public static void RaisePlannedShipmentValidationException(this Logs logs)
		{
			logs.CreateOrRecreateEventLog(
				Events.ExceptionRaised,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				string.Empty,
				[
					new (Params.Reason, ValidationFailureMessage)
				]);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log reference values")]
		const string ValidationFailureMessage = "The shipments were detached from the container's consol before the pack lines could be created";

		const string SqlQueryTemplate = @"
SELECT
	TOP 1 1
FROM
	dbo.ContainerLoadListLine
	INNER JOIN dbo.JobContainer ON CLL_JC_Container = JC_PK
	INNER JOIN dbo.JobSupplierBookingLine ON CLL_JSL_BookingLine = JSL_PK
WHERE
	{0} = @headerPK
	AND EXISTS (
		SELECT 1
		FROM   dbo.JobPackLines
		WHERE  JL_JSL_BookingLine = JSL_PK
	)
	AND NOT EXISTS (
		SELECT
			1
		FROM
			dbo.JobConShipLink
			INNER JOIN JobPackLines ON JN_JS = JL_JS
		WHERE
			JL_JSL_BookingLine = JSL_PK
			AND JN_JK = JC_JK
	)";
	}
}
