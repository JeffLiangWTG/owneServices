using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingRouting : SterlingRecord
	{
		public SterlingRouting()
		{
		}

		#region Source
		public Xsd.PlannedLeg Source
		{
			get { return fSource; }
			set { fSource = value; }
		}
		Xsd.PlannedLeg fSource;
		#endregion

		#region Record

		#region Header

		public override ZString RecordHeader
		{
			get
			{
				return "RTN";
			}
		}

		#endregion

		#region Generate

		public override void GenerateRecord()
		{
			AddField(TransportMode);
			AddField(PortOfLoading);
			AddField(PortOfLoadingEstimatedDateTime);
			AddField(PortOfLoadingActualDateTime);
			AddField(PortOfDischarge);
			AddField(PortOfDischargeEstimatedDateTime);
			AddField(PortOfDischargeActualDateTime);
			AddField(FlightNoJourneyNoTruckRegNo);
			AddField(VesselName);
			AddField(LloydsNo);
			AddField(VoyageNo);
			AddField(Level);
			TerminateRecord();
		}

		#endregion

		#endregion

		#region Properties

		#region TransportMode

		public ZString TransportMode
		{
			get
			{
				return Source.TransportMode.ToString();
			}
		}

		#endregion

		#region PortOfLoading

		public ZString PortOfLoading
		{
			get
			{
				return Source.PortOfLoading.Port.Value;
			}
		}

		#endregion

		#region PortOfLoadingEstimatedDateTime

		public ZString PortOfLoadingEstimatedDateTime
		{
			get
			{
				return ToTimeFormat(Source.PortOfLoading.EstimatedDateTime);
			}
		}

		#endregion

		#region PortOfLoadingActualDateTime

		public ZString PortOfLoadingActualDateTime
		{
			get
			{
				return ToTimeFormat(Source.PortOfLoading.ActualDateTime);
			}
		}

		#endregion

		#region PortOfDischarge

		public ZString PortOfDischarge
		{
			get
			{
				return Source.PortOfDischarge.Port.Value;
			}
		}

		#endregion

		#region PortOfDischargeEstimatedDateTime

		public ZString PortOfDischargeEstimatedDateTime
		{
			get
			{
				return ToTimeFormat(Source.PortOfDischarge.EstimatedDateTime);
			}
		}

		#endregion

		#region PortOfDischargeActualDateTime

		public ZString PortOfDischargeActualDateTime
		{
			get
			{
				return ToTimeFormat(Source.PortOfDischarge.ActualDateTime);
			}
		}

		#endregion

		#region FlightNoJourneyNoTruckRegNo

		public ZString FlightNoJourneyNoTruckRegNo
		{
			get
			{
				string result = "";
				Xsd.FlightWithFlightNumber flight = Source.Item as Xsd.FlightWithFlightNumber;
				if (flight != null)
				{
					result = flight.FlightNoJourneyNoTruckRegNo;
				}
				return result;
			}
		}

		#endregion

		#region VesselName

		public ZString VesselName
		{
			get
			{
				string result = "";
				Xsd.SailingWithVesselVoyage vesselInfo = Source.Item as Xsd.SailingWithVesselVoyage;
				if (vesselInfo != null)
				{
					result = vesselInfo.VesselName;
				}
				return result;
			}
		}

		#endregion

		#region LloydsNo

		public ZString LloydsNo
		{
			get
			{
				string result = "";
				Xsd.SailingWithVesselVoyage vesselInfo = Source.Item as Xsd.SailingWithVesselVoyage;
				if (vesselInfo != null)
				{
					result = vesselInfo.LloydsNo;
				}
				return result;
			}
		}

		#endregion

		#region VoyageNo

		public ZString VoyageNo
		{
			get
			{
				string result = "";
				Xsd.SailingWithVesselVoyage vesselInfo = Source.Item as Xsd.SailingWithVesselVoyage;
				if (vesselInfo != null)
				{
					result = vesselInfo.VoyageNo;
				}
				return result;
			}
		}

		#endregion

		#region Level

		public ZString Level
		{
			get
			{
				return fLevel;
			}
			set
			{
				fLevel = value;
			}
		}
		ZString fLevel;

		#endregion

		#endregion
	}
}
