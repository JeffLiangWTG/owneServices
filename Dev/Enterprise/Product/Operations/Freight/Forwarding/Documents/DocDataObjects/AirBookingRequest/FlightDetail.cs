using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class FlightDetail : DocDataObject
	{
		public FlightDetail(object id)
			: base(id)
		{
		}

		#region FlightNumber

		public ZString FlightNumber
		{
			get => flightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(FlightNumberInfo, ref flightNumber, value))
				{
					Validate(FlightNumberInfo);
				}
			}
		}

		ZString flightNumber;

		public ZPropertyInfo FlightNumberInfo => GetZPropertyInfo(nameof(FlightNumber));

		#endregion

		#region PortOfLoading

		public Unloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}

		Unloco portOfLoading;

		#endregion PortOfLoading

		#region PortOfDischarge

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		Unloco portOfDischarge;

		#endregion PortOfDischarge

		#region Status

		public FlightStatus Status
		{
			get => status;
			set => status = SetChild(status, value);
		}

		FlightStatus status;

		#endregion

		#region TransportType

		public CodeDescription TransportType
		{
			get => transportType;
			set => transportType = SetChild(transportType, value);
		}

		CodeDescription transportType;

		#endregion

		#region ETD

		public ZDateTime ETD
		{
			get => etd;
			set
			{
				if (SetNonPersistentPropertyValue(ETDInfo, ref etd, value))
				{
					Validate(ETDInfo);
				}
			}
		}

		ZDateTime etd;

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(nameof(ETD));

		#endregion

		#region ETA

		public ZDateTime ETA
		{
			get => eta;
			set
			{
				if (SetNonPersistentPropertyValue(ETAInfo, ref eta, value))
				{
					Validate(ETAInfo);
				}
			}
		}

		ZDateTime eta;

		public ZPropertyInfo ETAInfo => GetZPropertyInfo(nameof(ETA));

		#endregion

		#region AllotmentId

		public ZString AllotmentId
		{
			get => allotmentId;
			set
			{
				if (SetNonPersistentPropertyValue(AllotmentIdInfo, ref allotmentId, value))
				{
					Validate(AllotmentIdInfo);
				}
			}
		}

		ZString allotmentId;

		public ZPropertyInfo AllotmentIdInfo => GetZPropertyInfo(nameof(AllotmentId));

		#endregion AllotmentId
	}
}
