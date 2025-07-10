using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class RoutingResponseHeaderCollection : NonPersistentBusinessObjectCollection<RoutingResponseHeader>
	{
		public RoutingResponseHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public string Load(IEnumerable<RoutingRequest> requests, Action<string> updateStatus = null)
		{
			RemoveAll();

			var errorMessageBuilder = new StringBuilder();
			using (var client = GetS8Client(updateStatus))
			{
				foreach (var request in requests)
				{
					var errorMessage = Load(client, request);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						errorMessageBuilder.AppendLine(errorMessage);
					}
				}
			}

			return errorMessageBuilder.ToString().TrimEnd('\r', '\n');
		}

		string Load(IS8Client client, RoutingRequest request)
		{
			Argument.NotNull(request, nameof(request));

			var serviceCallResult = client.SolveRouting(request);

			if (serviceCallResult.Succeeded)
			{
				var response = serviceCallResult.Result;
				var rows = response.Split('\n');

				foreach (var row in rows)
				{
					var routing = row.Replace("&lt;", "<").Replace("&gt;", ">");
					if (!string.IsNullOrEmpty(routing) && routing.Contains("<") && routing.Contains(">"))
					{
						Add(new RoutingResponseHeader(routing, Factory, request));
					}
				}

				return string.Empty;
			}

			if (serviceCallResult.WasOperationCancelledByUser)
			{
				return string.Empty;
			}

			#region Error Report

			if (serviceCallResult.ShouldBeReported && ShouldReportError())
			{
				ErrorReporter.ReportOnce("S8_RoutingResponseHeaderCollection_Load_ErrorMessage", string.Format(CultureInfo.InvariantCulture,
	@"The request DepartureDate: {0},
OriginUNLOCOCode: {1},
DestinationUNLOCOCode: {2},
IncludeCodeShares: {3},
AirlineCode: {4},
CodeShareInterlineOption: {5},
MinimumConnectionTime: {6},
CargoPassengerFlightOption: {7},
EquipmentType: {8},
IncludeWeeklyTimetable: {9},
ConnectionsCount: {10},

The SolveRouting error message and call stack:
{11}
{12}",
					request.DepartureDate,
					request.OriginUNLOCOCode,
					request.DestinationUNLOCOCode,
					request.IncludeCodeShares,
					request.AirlineCode,
					request.CodeShareInterlineOption,
					request.MinimumConnectionTime,
					request.CargoPassengerFlightOption,
					request.EquipmentType,
					request.IncludeWeeklyTimetable,
					request.ConnectionsCount,
					serviceCallResult.ErrorMessage,
					serviceCallResult.StackTrace));
			}

			#endregion
			return serviceCallResult.ErrorMessage;
		}

		protected virtual bool ShouldReportError()
		{
			// we don't want to send the error report under DEBUG mode
			if (!Globals.IsTest)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RoutingResponseHeader("", Factory);
		}

		protected override bool AllowNewCore { get { return false; } }
		protected override bool AllowRemoveCore { get { return false; } }

		public virtual IS8Client GetS8Client(Action<string> updateStatus)
		{
			return new S8Client(updateStatus);
		}
	}
}
