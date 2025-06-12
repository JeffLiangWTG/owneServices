using System;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WiseCloudSQLAccess
{
	[XmlRoot(Element.Row, IsNullable = false, Namespace = Helper.SolarwindsNamespace)]
	[Serializable]
	public class WiseCloudSQLAccessRecord
	{
		public const decimal Mega = 1000000m;
		string minTimeStampString;
		string totalBytesConsumedString;

		public struct Element
		{
			public const string Data = "data";
			public const string Row = "row";
			public const string SourceIp = "c0";
			public const string DestinationIp = "c1";
			public const string TotalBytesConsumedString = "c2";
			public const string MinTimeStampString = "c3";
			public const string MaxTimeStampString = "c4";
		}

		public WiseCloudSQLAccessRecord()
		{
		}

		public WiseCloudSQLAccessRecord(string customerCode, string customerIP, string sourceIp, string destinationIp, int totalMegaBytes, DateTime timeStamp)
		{
			CustomerCode = customerCode;
			CustomerIp = customerIP;
			SourceIp = sourceIp;
			DestinationIp = destinationIp;
			TotalMegaBytesConsumed = totalMegaBytes;
			MinTimeStamp = timeStamp;
		}

		public WiseCloudSQLAccessRecord(XElement item)
		{
			SourceIp = item.ElementValue(Element.SourceIp);
			DestinationIp = item.ElementValue(Element.DestinationIp);
			TotalBytesConsumedString = item.ElementValue(Element.TotalBytesConsumedString);
			MinTimeStampString = item.ElementValue(Element.MinTimeStampString);
			MaxTimeStampString = item.ElementValue(Element.MaxTimeStampString);
		}

		public WiseCloudSQLAccessRecord(string sourceIp, string destinationIp, string totalBytesConsumedString, DateTime minTimeStamp, string maxTimeStampString)
		{
			SourceIp = sourceIp;
			DestinationIp = destinationIp;
			TotalBytesConsumedString = totalBytesConsumedString;
			MinTimeStamp = minTimeStamp;
			MaxTimeStampString = maxTimeStampString;
		}


		[XmlIgnore]
		public string CustomerCode { get; set; }
		[XmlIgnore]
		public string CustomerIp { get; set; }
		[XmlElement(Element.SourceIp)]
		public string SourceIp { get; set; }
		[XmlElement(Element.DestinationIp)]
		public string DestinationIp { get; set; }
		[XmlElement(Element.TotalBytesConsumedString)]
		public string TotalBytesConsumedString
		{
			get => totalBytesConsumedString;
			set
			{
				totalBytesConsumedString = value;
				TotalMegaBytesConsumed = BytesStringToMegaBytesConsumed(totalBytesConsumedString);
			}
		}
		[XmlElement(Element.MinTimeStampString)]
		public string MinTimeStampString
		{
			get => minTimeStampString;
			set
			{
				minTimeStampString = value;
				MinTimeStamp = Helper.ISO8601StringToDateTimeUtc(minTimeStampString);
			}
		}
		[XmlElement(Element.MaxTimeStampString)]
		public string MaxTimeStampString { get; set; }

		[XmlIgnore]
		public int TotalMegaBytesConsumed { get; private set; }
		[XmlIgnore]
		public DateTime MinTimeStamp { get; private set; }

		public bool HasDataConsumption()
		{
			return TotalMegaBytesConsumed > 0;
		}

		public bool HasCustomer()
		{
			return !string.IsNullOrEmpty(CustomerCode);
		}

		public override int GetHashCode()
		{
			return SourceIp.GetHashCode() ^ DestinationIp.GetHashCode() ^ TotalMegaBytesConsumed.GetHashCode() ^ MinTimeStamp.GetHashCode();
		}

		static int BytesStringToMegaBytesConsumed(string value)
		{
			var megaBytes = 0;

			if (!string.IsNullOrWhiteSpace(value) && decimal.TryParse(value, out _))
			{
				var totalMegaBytesInDecimal =
					decimal.Parse(value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint) / Mega;
				megaBytes = decimal.ToInt32(Math.Round(totalMegaBytesInDecimal, 0, MidpointRounding.AwayFromZero));
			}

			return megaBytes;
		}
	}
}
