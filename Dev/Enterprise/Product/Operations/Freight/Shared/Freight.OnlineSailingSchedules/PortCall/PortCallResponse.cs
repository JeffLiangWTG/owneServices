using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallResponse : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string VesselName = "VesselName";
			public const string IMO = "IMO";
			public const string CallSign = "CallSign";
			public const string CarrierCode = "CarrierCode";
			public const string VoyageNumber = "VoyageNumber";
			public const string EstimatedTime = "EstimatedTime";
			public const string ReferenceNumber = "ReferenceNumber";
		}

		#endregion

		public void SetValues(PortCallItem portCall)
		{
			if (portCall.Vessel != null)
			{
				VesselName = portCall.Vessel.VesselName;
				IMO = portCall.Vessel.ImoNumber;
				CallSign = portCall.Vessel.CallSign;
			}

			if (portCall.Carrier != null)
			{
				CarrierCode = portCall.Carrier.Code;
			}

			VoyageNumber = RequestType == PortCallRequestType.Load
				? portCall.VoyageNumberOut
				: portCall.VoyageNumberIn;

			ReferenceNumber = RequestType == PortCallRequestType.Load
				? portCall.DepartureNumber
				: portCall.ArrivalNumber;

			EstimatedTime = RequestType == PortCallRequestType.Load
				? portCall.Etd ?? ZDateTime.Empty
				: portCall.Eta ?? ZDateTime.Empty;
		}

		#region Properties

		[ReadOnly(true)]
		public ZString VesselName
		{
			get => vesselName;
			private set => SetNonPersistentPropertyValue(VesselNameInfo, ref vesselName, value);
		}
		ZString vesselName;

		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(Schema.VesselName);

		[ReadOnly(true)]
		public ZString IMO
		{
			get => imo;
			private set => SetNonPersistentPropertyValue(IMOInfo, ref imo, value);
		}
		ZString imo;

		public ZPropertyInfo IMOInfo => GetZPropertyInfo(Schema.IMO);

		[ReadOnly(true)]
		public ZString CallSign
		{
			get => callSign;
			private set => SetNonPersistentPropertyValue(CallSignInfo, ref callSign, value);
		}
		ZString callSign;

		public ZPropertyInfo CallSignInfo => GetZPropertyInfo(Schema.CallSign);

		[ReadOnly(true)]
		public ZString CarrierCode
		{
			get => carrierCode;
			private set => SetNonPersistentPropertyValue(CarrierCodeInfo, ref carrierCode, value);
		}
		ZString carrierCode;

		public ZPropertyInfo CarrierCodeInfo => GetZPropertyInfo(Schema.CarrierCode);

		[ReadOnly(true)]
		public ZString VoyageNumber
		{
			get => voyageNumber;
			private set => SetNonPersistentPropertyValue(VoyageNumberInfo, ref voyageNumber, value);
		}
		ZString voyageNumber;

		public ZPropertyInfo VoyageNumberInfo => GetZPropertyInfo(Schema.VoyageNumber);

		[ReadOnly(true)]
		public ZDateTime EstimatedTime
		{
			get => estimatedTime;
			private set => SetNonPersistentPropertyValue(EstimatedTimeInfo, ref estimatedTime, value);
		}
		ZDateTime estimatedTime;

		public ZPropertyInfo EstimatedTimeInfo => GetZPropertyInfo(Schema.EstimatedTime);

		[ReadOnly(true)]
		public ZString ReferenceNumber
		{
			get => referenceNumber;
			private set => SetNonPersistentPropertyValue(VoyageNumberInfo, ref referenceNumber, value);
		}
		ZString referenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(Schema.ReferenceNumber);

		public PortCallRequestType RequestType { get; set; }

		#endregion

	}
}
