using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IPortMatchingSupport
	{
		ILocationReference PortOfDischarge { get; }

		void SetFirstArrivalPort(ZString unloco, ZDateTimeOffset dateTime);

		void SetLastForeignPort(ZString unloco, ZDateTimeOffset dateTime);

		void ReportException(Exception exception);
	}
}
